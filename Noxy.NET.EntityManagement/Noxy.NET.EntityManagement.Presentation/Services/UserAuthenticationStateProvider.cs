using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using Noxy.NET.EntityManagement.Application.Interfaces.Services;

namespace Noxy.NET.EntityManagement.Presentation.Services;

public class UserAuthenticationStateProvider(ILocalStorageService serviceStorage, IJWTService serviceJWT) : AuthenticationStateProvider
{
    private const string UserKey = "user";
    private const string AuthenticationType = "JWTAuthentication";

    public JwtSecurityToken? Identity { get; private set; }
    public JwtSecurityToken IdentityCurrent => Identity ?? throw new ArgumentNullException(nameof(Identity));

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        try
        {
            string result = await serviceStorage.GetItemAsync<string>(UserKey) ?? string.Empty;
            if (string.IsNullOrEmpty(result)) return new(await RegisterUserData(null));

            Identity ??= serviceJWT.ReadJWT(result.Trim('"'));
            return new(CreateClaimsPrincipal(Identity));
        }
        catch (JsonException)
        {
            return new(await RegisterUserData(null));
        }
        catch (InvalidOperationException ex)
        {
            if (ex.Source != "Microsoft.AspNetCore.Components.Server")
            {
                return new(await RegisterUserData(null));
            }
        }
        catch (Exception)
        {
            // ignored
        }

        return new(new());
    }

    public async Task UpdateIdentity(string? jwt)
    {
        NotifyAuthenticationStateChanged(await RegisterUserData(jwt));
    }

    public async Task ResetIdentity()
    {
        NotifyAuthenticationStateChanged(await RegisterUserData(null));
    }

    private async Task<ClaimsPrincipal> RegisterUserData(string? jwt)
    {
        if (jwt != null)
        {
            Identity = serviceJWT.ReadJWT(jwt);
            await serviceStorage.SetItemAsync(UserKey, jwt);
            return CreateClaimsPrincipal(Identity);
        }

        await serviceStorage.RemoveItemAsync(UserKey);
        return new();
    }

    private void NotifyAuthenticationStateChanged(ClaimsPrincipal principal)
    {
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(principal)));
    }

    private static ClaimsPrincipal CreateClaimsPrincipal(JwtSecurityToken jwt)
    {
        return new(CreateClaimsIdentity(jwt));
    }

    private static ClaimsIdentity CreateClaimsIdentity(JwtSecurityToken jwt)
    {
        return new(CreateClaims(jwt), AuthenticationType);
    }

    private static Claim[] CreateClaims(JwtSecurityToken jwt)
    {
        return jwt.Claims.ToArray();
    }
}
