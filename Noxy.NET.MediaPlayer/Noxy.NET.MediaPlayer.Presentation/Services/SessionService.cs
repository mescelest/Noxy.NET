using System.Net;
using Noxy.NET.MediaPlayer.Domain.Responses.Authentication;
using Noxy.NET.MediaPlayer.Presentation.Enums;
using Noxy.NET.MediaPlayer.Presentation.Exceptions;
using Noxy.NET.MediaPlayer.Presentation.Extensions;
using Noxy.NET.MediaPlayer.Presentation.Interfaces.Services;
using Noxy.NET.MediaPlayer.Presentation.Interfaces.Stores;

namespace Noxy.NET.MediaPlayer.Presentation.Services;

public class SessionService(IAuthApiClient clientAuthApi, IGoogleSignInService serviceGoogleSignIn, ISessionStorage storageSession, TimeProvider time) : ISessionService
{
    private readonly SemaphoreSlim _refreshLock = new(1, 1);

    // The access token only ever lives here, never on disk. Null expiry means the token carried none: it is used until the server says otherwise.
    private string? _accessToken;
    private DateTime? _accessTokenExpires;

    // A refresh token is stored and the last restore could not reach the server.
    private bool _hasOfflineSession;

    public bool IsSignedIn => _accessToken is not null;

    public bool HasSession => IsSignedIn || _hasOfflineSession;

    public event Action? Changed;

    public async Task SignUp(string email, string password)
    {
        await Apply(await clientAuthApi.SignUp(email, password), true);
    }

    public async Task SignIn(string email, string password)
    {
        await Apply(await clientAuthApi.SignIn(email, password), true);
    }

    public async Task<bool> SignInWithGoogle(CancellationToken cancellationToken = default)
    {
        string? idToken = await serviceGoogleSignIn.GetIdToken(cancellationToken);
        if (idToken is null) return false;

        await Apply(await clientAuthApi.SignInGoogle(idToken), true);
        return true;
    }

    public async Task<RestoreResultEnum> TryRestore()
    {
        if (await storageSession.GetRefreshToken() is null) return RestoreResultEnum.NoSession;

        try
        {
            if (await Refresh())
            {
                SetOfflineSession(false);
                return RestoreResultEnum.SignedIn;
            }

            return RestoreResultEnum.Rejected;
        }
        catch (Exception exception) when (exception is HttpRequestException or ServerAddressMissingException or TaskCanceledException || IsServerTrouble(exception))
        {
            // The server could not be reached: the stored token is still good, so it stays. The user is not signed in yet, but has a session: the app shows what is on the device and tries again.
            SetOfflineSession(true);
            return RestoreResultEnum.Offline;
        }
    }

    // A server that answers with an error of its own (5xx, too many requests, a timeout) has not rejected the token: the session is kept and the app tries again.
    private static bool IsServerTrouble(Exception exception)
    {
        return exception is ApiException { Status: >= HttpStatusCode.InternalServerError or HttpStatusCode.RequestTimeout or HttpStatusCode.TooManyRequests };
    }

    private void SetOfflineSession(bool value)
    {
        if (_hasOfflineSession == value) return;

        _hasOfflineSession = value;
        Changed?.Invoke();
    }

    public async Task SignOut()
    {
        bool wasSignedIn = IsSignedIn;
        _accessToken = null;
        _accessTokenExpires = null;
        bool hadOfflineSession = _hasOfflineSession;
        _hasOfflineSession = false;
        await storageSession.ClearRefreshToken();

        if (wasSignedIn || hadOfflineSession) Changed?.Invoke();
    }

    public async Task<string?> GetAccessToken()
    {
        if (_accessToken is null) return null;

        if (_accessTokenExpires is { } expires && expires - time.GetUtcNow().UtcDateTime <= TimeSpan.FromSeconds(Constants.RefreshMarginSeconds))
        {
            await Refresh();
        }

        return _accessToken;
    }

    public async Task<bool> Refresh()
    {
        // Refresh tokens are single use, so callers that arrive together share one refresh: whoever gets the lock after another refresh succeeded just takes its result.
        string? before = _accessToken;

        await _refreshLock.WaitAsync();
        try
        {
            if (_accessToken != before) return IsSignedIn;

            string? refreshToken = await storageSession.GetRefreshToken();
            if (refreshToken is null)
            {
                await SignOut();
                return false;
            }

            try
            {
                await Apply(await clientAuthApi.Refresh(refreshToken), before is null);
                return true;
            }
            catch (ApiException exception) when (exception.Status is HttpStatusCode.Unauthorized or HttpStatusCode.BadRequest)
            {
                await SignOut();
                return false;
            }
        }
        finally
        {
            _refreshLock.Release();
        }
    }

    private async Task Apply(ResponseAuthentication tokens, bool raiseChanged)
    {
        _accessToken = tokens.AccessToken;
        _accessTokenExpires = tokens.AccessToken.GetAccessTokenExpires();
        _hasOfflineSession = false;
        await storageSession.SetRefreshToken(tokens.RefreshToken);

        if (raiseChanged) Changed?.Invoke();
    }
}
