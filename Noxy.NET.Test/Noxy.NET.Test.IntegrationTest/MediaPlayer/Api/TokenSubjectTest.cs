using System.Net;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Noxy.NET.Test.MediaPlayer.Fixtures;

namespace Noxy.NET.Test.MediaPlayer.Api;

public class TokenSubjectTest : IDisposable
{
    private const string Issuer = "Noxy.NET.MediaPlayer";
    private const string Audience = "Noxy.NET.MediaPlayer";

    private readonly ApiFactory _factory = new();

    public void Dispose()
    {
        _factory.Dispose();
    }

    // Signed with the real key, issuer and audience, so the only thing wrong with it is its claims.
    private static string CreateToken(params Claim[] claimList)
    {
        SecurityTokenDescriptor descriptor = new()
        {
            Subject = new(claimList),
            Issuer = Issuer,
            Audience = Audience,
            Expires = DateTime.UtcNow.AddMinutes(5),
            SigningCredentials = new(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(new string('s', 48))), SecurityAlgorithms.HmacSha256)
        };

        return new JsonWebTokenHandler().CreateToken(descriptor);
    }

    private async Task<HttpStatusCode> GetLibrary(string token)
    {
        using HttpClient client = _factory.CreateClient();
        using HttpRequestMessage request = new(HttpMethod.Get, "/Media/Library");
        request.Headers.Authorization = new("Bearer", token);

        using HttpResponseMessage response = await client.SendAsync(request, TestContext.Current.CancellationToken);
        return response.StatusCode;
    }

    [Fact]
    public async Task ATokenWithAUserID_IsAccepted()
    {
        Assert.Equal(HttpStatusCode.OK, await GetLibrary(CreateToken(new Claim(JwtRegisteredClaimNames.Sub, Guid.CreateVersion7().ToString()))));
    }

    [Fact]
    public async Task ATokenWhoseSubIsNotAUserID_Returns401()
    {
        Assert.Equal(HttpStatusCode.Unauthorized, await GetLibrary(CreateToken(new Claim(JwtRegisteredClaimNames.Sub, "not-a-guid"))));
    }

    [Fact]
    public async Task ATokenWithoutASub_Returns401()
    {
        Assert.Equal(HttpStatusCode.Unauthorized, await GetLibrary(CreateToken(new Claim(JwtRegisteredClaimNames.Email, "user@example.com"))));
    }

    private async Task<bool> PassesTheFallbackPolicy(params Claim[] claimList)
    {
        IAuthorizationPolicyProvider provider = _factory.Services.GetRequiredService<IAuthorizationPolicyProvider>();
        IAuthorizationService service = _factory.Services.GetRequiredService<IAuthorizationService>();
        AuthorizationPolicy policy = await provider.GetFallbackPolicyAsync() ?? throw new InvalidOperationException("There is no fallback policy.");
        ClaimsPrincipal user = new(new ClaimsIdentity(claimList, "Test"));

        return (await service.AuthorizeAsync(user, policy)).Succeeded;
    }

    [Fact]
    public async Task TheFallbackPolicy_AcceptsASignedInUserWithAUserID()
    {
        Assert.True(await PassesTheFallbackPolicy(new Claim(JwtRegisteredClaimNames.Sub, Guid.CreateVersion7().ToString())));
    }

    [Theory]
    [InlineData("not-a-guid")]
    [InlineData("")]
    public async Task TheFallbackPolicy_RejectsASignedInUserWhoseSubIsNotAUserID(string sub)
    {
        Assert.False(await PassesTheFallbackPolicy(new Claim(JwtRegisteredClaimNames.Sub, sub)));
    }

    [Fact]
    public async Task TheFallbackPolicy_RejectsASignedInUserWithoutASub()
    {
        Assert.False(await PassesTheFallbackPolicy(new Claim(JwtRegisteredClaimNames.Email, "user@example.com")));
    }
}
