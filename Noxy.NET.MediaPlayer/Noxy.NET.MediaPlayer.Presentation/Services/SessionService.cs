using System.Net;
using Noxy.NET.MediaPlayer.Domain.Responses.Authentication;
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

    public bool IsSignedIn => _accessToken is not null;

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

    public async Task<bool> TryRestore()
    {
        if (await storageSession.GetRefreshToken() is null) return false;

        try
        {
            return await Refresh();
        }
        catch (Exception exception) when (exception is HttpRequestException or ServerAddressMissingException)
        {
            // The server could not be reached: the stored token is still good, so it stays, and the user stays signed out until a later start.
            return false;
        }
    }

    public async Task SignOut()
    {
        bool wasSignedIn = IsSignedIn;
        _accessToken = null;
        _accessTokenExpires = null;
        await storageSession.ClearRefreshToken();

        if (wasSignedIn) Changed?.Invoke();
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
        await storageSession.SetRefreshToken(tokens.RefreshToken);

        if (raiseChanged) Changed?.Invoke();
    }
}
