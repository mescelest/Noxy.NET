using Microsoft.Extensions.Options;
using Noxy.NET.MediaPlayer.Application.Configurations;
using Noxy.NET.MediaPlayer.Application.Services;
using Noxy.NET.MediaPlayer.Domain.Exceptions;

namespace Noxy.NET.Test.MediaPlayer.Authentication;

public class GoogleTokenServiceTest
{
    private static GoogleTokenService Service(params string[] clientIdList)
    {
        return new(Options.Create(new ConfigGoogle { ClientIdList = [.. clientIdList] }));
    }

    [Theory]
    [InlineData]
    [InlineData("")]
    [InlineData("", "  ")]
    public async Task Validate_WithNoUsableClientID_ThrowsNotConfigured_BeforeLookingAtTheToken(params string[] clientIdList)
    {
        await Assert.ThrowsAsync<GoogleLoginNotConfiguredException>(() => Service(clientIdList).Validate("any.token.value"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Validate_WithABlankToken_ThrowsAuthenticationFailed(string idToken)
    {
        await Assert.ThrowsAsync<AuthenticationFailedException>(() => Service("android.apps.googleusercontent.com").Validate(idToken));
    }

    [Fact]
    public async Task Validate_WithATokenThatIsNotAJwt_ThrowsAuthenticationFailed_WhateverClientsAreListed()
    {
        await Assert.ThrowsAsync<AuthenticationFailedException>(() => Service("android.apps.googleusercontent.com", "desktop.apps.googleusercontent.com").Validate("this is not a jwt"));
    }
}
