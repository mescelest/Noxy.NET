using System.Net;
using Noxy.NET.MediaPlayer.Presentation.Enums;
using Noxy.NET.MediaPlayer.Presentation.Exceptions;
using Noxy.NET.MediaPlayer.Presentation.Services;
using Noxy.NET.Test.MediaPlayer.Fakes;

namespace Noxy.NET.Test.MediaPlayer.Presentation;

public class SessionServiceTest
{
    private readonly FakeTimeProvider _time = new();
    private readonly FakeAuthApiClient _clientAuthApi;
    private readonly FakeGoogleSignInService _serviceGoogleSignIn = new();
    private readonly FakeSessionStorage _storageSession = new();
    private readonly SessionService _service;
    private int _changed;

    public SessionServiceTest()
    {
        _clientAuthApi = new(_time);
        _service = new(_clientAuthApi, _serviceGoogleSignIn, _storageSession, _time);
        _service.Changed += () => _changed++;
    }

    private void Advance(TimeSpan span)
    {
        _time.Now = _time.Now.Add(span);
    }

    [Fact]
    public async Task SignIn_StoresTheRefreshTokenAndKeepsTheAccessTokenInMemoryOnly()
    {
        await _service.SignIn("a@example.com", "pw");

        Assert.True(_service.IsSignedIn);
        Assert.Equal("refresh-1", _storageSession.RefreshToken);
        Assert.NotNull(await _service.GetAccessToken());
        Assert.NotEqual(_storageSession.RefreshToken, await _service.GetAccessToken());
    }

    [Fact]
    public async Task SignUp_SignsInWithTheNewAccount()
    {
        await _service.SignUp("a@example.com", "a-long-enough-password");

        Assert.Equal(["SignUp:a@example.com"], _clientAuthApi.CallList);
        Assert.True(_service.IsSignedIn);
    }

    [Fact]
    public async Task SignIn_WhenTheServerRejectsIt_ThrowsApiException_AndStaysSignedOut()
    {
        _clientAuthApi.Failure = FakeAuthApiClient.Rejected();

        await Assert.ThrowsAsync<ApiException>(() => _service.SignIn("a@example.com", "wrong"));

        Assert.False(_service.IsSignedIn);
        Assert.Null(_storageSession.RefreshToken);
        Assert.Equal(0, _changed);
    }

    [Fact]
    public async Task SignInWithGoogle_SendsTheIdTokenAndSignsIn_AndStoresOnlyOurRefreshToken()
    {
        _serviceGoogleSignIn.IdToken = "google-token";

        bool signedIn = await _service.SignInWithGoogle(TestContext.Current.CancellationToken);

        Assert.True(signedIn);
        Assert.Equal(["SignInGoogle:google-token"], _clientAuthApi.CallList);
        Assert.Equal("refresh-1", _storageSession.RefreshToken);
        Assert.True(_service.IsSignedIn);
    }

    [Fact]
    public async Task SignInWithGoogle_WhenTheUserCancels_ReturnsFalse_AndStaysSignedOut_WithNoServerCall()
    {
        _serviceGoogleSignIn.IdToken = null;

        bool signedIn = await _service.SignInWithGoogle(TestContext.Current.CancellationToken);

        Assert.False(signedIn);
        Assert.False(_service.IsSignedIn);
        Assert.Empty(_clientAuthApi.CallList);
        Assert.Null(_storageSession.RefreshToken);
        Assert.Equal(0, _changed);
    }

    [Fact]
    public async Task SignInWithGoogle_WhenTheServerRejectsTheToken_ThrowsApiException_AndStaysSignedOut()
    {
        _clientAuthApi.Failure = FakeAuthApiClient.Rejected();

        await Assert.ThrowsAsync<ApiException>(() => _service.SignInWithGoogle(TestContext.Current.CancellationToken));

        Assert.False(_service.IsSignedIn);
        Assert.Null(_storageSession.RefreshToken);
    }

    [Fact]
    public async Task TryRestore_WithAStoredRefreshToken_SignsIn_AndRotatesTheStoredToken()
    {
        _storageSession.RefreshToken = "stored-refresh";

        RestoreResultEnum restored = await _service.TryRestore();

        Assert.Equal(RestoreResultEnum.SignedIn, restored);
        Assert.Equal(["Refresh:stored-refresh"], _clientAuthApi.CallList);
        Assert.Equal("refresh-1", _storageSession.RefreshToken);
        Assert.True(_service.IsSignedIn);
        Assert.True(_service.HasSession);
    }

    [Fact]
    public async Task TryRestore_WithNoStoredToken_ReturnsNoSession_WithNoServerCall_AndHasSessionIsFalse()
    {
        Assert.Equal(RestoreResultEnum.NoSession, await _service.TryRestore());

        Assert.Empty(_clientAuthApi.CallList);
        Assert.False(_service.IsSignedIn);
        Assert.False(_service.HasSession);
    }

    [Theory]
    [InlineData(HttpStatusCode.Unauthorized)]
    [InlineData(HttpStatusCode.BadRequest)]
    public async Task TryRestore_WhenTheRefreshIsRejected_ReturnsRejected_ClearsTheStoredToken_AndHasSessionIsFalse(HttpStatusCode status)
    {
        _storageSession.RefreshToken = "stored-refresh";
        _clientAuthApi.Failure = FakeAuthApiClient.Rejected(status);

        Assert.Equal(RestoreResultEnum.Rejected, await _service.TryRestore());

        Assert.Null(_storageSession.RefreshToken);
        Assert.False(_service.IsSignedIn);
        Assert.False(_service.HasSession);
    }

    [Fact]
    public async Task TryRestore_WhenTheNetworkIsDown_ReturnsOffline_KeepsTheStoredToken_AndHasSessionIsTrue()
    {
        _storageSession.RefreshToken = "stored-refresh";
        _clientAuthApi.Failure = new HttpRequestException("The network is down.");
        int changed = 0;
        _service.Changed += () => changed++;

        Assert.Equal(RestoreResultEnum.Offline, await _service.TryRestore());

        Assert.Equal("stored-refresh", _storageSession.RefreshToken);
        Assert.False(_service.IsSignedIn);
        Assert.True(_service.HasSession);
        Assert.Equal(1, changed);
    }

    [Theory]
    [InlineData(HttpStatusCode.BadGateway)]
    [InlineData(HttpStatusCode.ServiceUnavailable)]
    [InlineData(HttpStatusCode.InternalServerError)]
    [InlineData(HttpStatusCode.RequestTimeout)]
    [InlineData(HttpStatusCode.TooManyRequests)]
    public async Task TryRestore_WhenTheServerAnswersWithAServerError_ReturnsOffline_AndKeepsTheStoredToken(HttpStatusCode status)
    {
        _storageSession.RefreshToken = "stored-refresh";
        _clientAuthApi.Failure = new ApiException(status, "The server is having trouble.");

        Assert.Equal(RestoreResultEnum.Offline, await _service.TryRestore());

        Assert.Equal("stored-refresh", _storageSession.RefreshToken);
        Assert.True(_service.HasSession);
    }

    [Fact]
    public async Task TryRestore_WhenTheRequestTimesOut_ReturnsOffline_AndKeepsTheStoredToken()
    {
        _storageSession.RefreshToken = "stored-refresh";
        _clientAuthApi.Failure = new TaskCanceledException("The request timed out.");

        Assert.Equal(RestoreResultEnum.Offline, await _service.TryRestore());

        Assert.Equal("stored-refresh", _storageSession.RefreshToken);
        Assert.True(_service.HasSession);
    }


    [Fact]
    public async Task TryRestore_WhenNoServerAddressIsSaved_ReturnsOffline_AndKeepsTheStoredToken()
    {
        _storageSession.RefreshToken = "stored-refresh";
        _clientAuthApi.Failure = new ServerAddressMissingException();

        Assert.Equal(RestoreResultEnum.Offline, await _service.TryRestore());

        Assert.Equal("stored-refresh", _storageSession.RefreshToken);
        Assert.True(_service.HasSession);
    }

    [Fact]
    public async Task AnOfflineSession_BecomesSignedIn_WhenALaterRestoreReachesTheServer_AndEndsWithSignOut()
    {
        _storageSession.RefreshToken = "stored-refresh";
        _clientAuthApi.Failure = new HttpRequestException("The network is down.");
        await _service.TryRestore();

        _clientAuthApi.Failure = null;
        Assert.Equal(RestoreResultEnum.SignedIn, await _service.TryRestore());
        Assert.True(_service.IsSignedIn);

        await _service.SignOut();
        Assert.False(_service.HasSession);

        _storageSession.RefreshToken = "stored-refresh";
        _clientAuthApi.Failure = new HttpRequestException("The network is down.");
        await _service.TryRestore();
        Assert.True(_service.HasSession);

        await _service.SignOut();
        Assert.False(_service.HasSession);
    }

    [Fact]
    public async Task GetAccessToken_WhenSignedOut_ReturnsNull_WithNoServerCall()
    {
        Assert.Null(await _service.GetAccessToken());

        Assert.Empty(_clientAuthApi.CallList);
    }

    [Fact]
    public async Task GetAccessToken_WhileItIsFresh_ReturnsItWithoutRefreshing()
    {
        await _service.SignIn("a@example.com", "pw");
        string first = await _service.GetAccessToken() ?? throw new InvalidOperationException("Not signed in.");
        Advance(TimeSpan.FromMinutes(10));

        Assert.Equal(first, await _service.GetAccessToken());
        Assert.Equal(0, _clientAuthApi.RefreshCount);
    }

    [Fact]
    public async Task GetAccessToken_WhenItExpiresWithinTheMargin_RefreshesFirst()
    {
        await _service.SignIn("a@example.com", "pw");
        string first = await _service.GetAccessToken() ?? throw new InvalidOperationException("Not signed in.");
        Advance(TimeSpan.FromMinutes(15).Subtract(TimeSpan.FromSeconds(30)));

        string? second = await _service.GetAccessToken();

        Assert.Equal(1, _clientAuthApi.RefreshCount);
        Assert.NotNull(second);
        Assert.NotEqual(first, second);
        Assert.Equal("refresh-2", _storageSession.RefreshToken);
    }

    [Fact]
    public async Task GetAccessToken_WhenTheRefreshIsRejected_SignsOut_AndReturnsNull()
    {
        await _service.SignIn("a@example.com", "pw");
        Advance(TimeSpan.FromMinutes(20));
        _clientAuthApi.Failure = FakeAuthApiClient.Rejected();

        Assert.Null(await _service.GetAccessToken());

        Assert.False(_service.IsSignedIn);
        Assert.Null(_storageSession.RefreshToken);
    }

    [Fact]
    public async Task ConcurrentRefresh_CallsTheServerOnce()
    {
        await _service.SignIn("a@example.com", "pw");
        TaskCompletionSource gate = new(TaskCreationOptions.RunContinuationsAsynchronously);
        _clientAuthApi.RefreshGate = gate.Task;

        List<Task<bool>> callList = [.. Enumerable.Range(0, 10).Select(_ => _service.Refresh())];
        await Task.Delay(50, TestContext.Current.CancellationToken);
        gate.SetResult();
        bool[] resultList = await Task.WhenAll(callList);

        Assert.All(resultList, Assert.True);
        Assert.Equal(1, _clientAuthApi.RefreshCount);
        Assert.Equal("refresh-2", _storageSession.RefreshToken);
    }

    [Fact]
    public async Task ConcurrentGetAccessToken_WhenExpired_RefreshesOnce()
    {
        await _service.SignIn("a@example.com", "pw");
        Advance(TimeSpan.FromMinutes(14));
        TaskCompletionSource gate = new(TaskCreationOptions.RunContinuationsAsynchronously);
        _clientAuthApi.RefreshGate = gate.Task;

        List<Task<string?>> callList = [.. Enumerable.Range(0, 10).Select(_ => _service.GetAccessToken())];
        await Task.Delay(50, TestContext.Current.CancellationToken);
        gate.SetResult();
        string?[] tokenList = await Task.WhenAll(callList);

        Assert.Equal(1, _clientAuthApi.RefreshCount);
        Assert.Single(tokenList.Distinct());
    }

    [Fact]
    public async Task Refresh_WhenRejected_SignsOut_AndRaisesChanged_AndReturnsFalse()
    {
        await _service.SignIn("a@example.com", "pw");
        int before = _changed;
        _clientAuthApi.Failure = FakeAuthApiClient.Rejected();

        Assert.False(await _service.Refresh());

        Assert.False(_service.IsSignedIn);
        Assert.Null(_storageSession.RefreshToken);
        Assert.Equal(before + 1, _changed);
    }

    [Fact]
    public async Task Refresh_WhenTheNetworkIsDown_PropagatesAndStaysSignedIn()
    {
        await _service.SignIn("a@example.com", "pw");
        _clientAuthApi.Failure = new HttpRequestException("The network is down.");

        await Assert.ThrowsAsync<HttpRequestException>(() => _service.Refresh());

        Assert.True(_service.IsSignedIn);
        Assert.Equal("refresh-1", _storageSession.RefreshToken);
    }

    [Fact]
    public async Task SignOut_ClearsEverything()
    {
        await _service.SignIn("a@example.com", "pw");

        await _service.SignOut();

        Assert.False(_service.IsSignedIn);
        Assert.Null(_storageSession.RefreshToken);
        Assert.Null(await _service.GetAccessToken());
    }

    [Fact]
    public async Task Changed_IsRaisedOnSignInAndSignOut_ButNotOnASilentRefresh()
    {
        await _service.SignIn("a@example.com", "pw");
        Assert.Equal(1, _changed);

        Advance(TimeSpan.FromMinutes(14));
        await _service.GetAccessToken();
        Assert.Equal(1, _changed);

        await _service.SignOut();
        Assert.Equal(2, _changed);
    }

    [Fact]
    public async Task NoTokenAndNoPassword_AppearInAnExceptionMessage()
    {
        _clientAuthApi.Failure = FakeAuthApiClient.Rejected();

        ApiException exception = await Assert.ThrowsAsync<ApiException>(() => _service.SignIn("a@example.com", "hunter2-password"));

        Assert.DoesNotContain("hunter2-password", exception.Message);
    }
}
