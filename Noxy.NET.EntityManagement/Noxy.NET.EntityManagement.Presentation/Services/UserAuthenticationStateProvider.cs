using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using Noxy.NET.EntityManagement.Application.Interfaces.Services;

namespace Noxy.NET.EntityManagement.Presentation.Services;

public class UserAuthenticationStateProvider : AuthenticationStateProvider
{
    private const string UserKey = "user";
    private const string AuthenticationType = "JWTAuthentication";
    private readonly IJWTService _jwt;

    private readonly ILocalStorageService _storage;

    private ClaimsPrincipal _currentUser = new(new ClaimsIdentity());

    public UserAuthenticationStateProvider(ILocalStorageService storage, IJWTService jwt)
    {
        _storage = storage;
        _jwt = jwt;
        _ = InitializeAsync();
    }

    public JwtSecurityToken? Identity { get; private set; }

    public bool IsInitialized { get; private set; }

    private async Task InitializeAsync()
    {
        try
        {
            string? token = await _storage.GetItemAsync<string>(UserKey);
            if (!string.IsNullOrWhiteSpace(token))
            {
                Identity = _jwt.ReadJWT(token.Trim('"'));
                _currentUser = CreatePrincipal(Identity);
            }
        }
        catch
        {
            // Ignore errors and treat as unauthenticated
        }

        IsInitialized = true;
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    public override Task<AuthenticationState> GetAuthenticationStateAsync() => Task.FromResult(new AuthenticationState(_currentUser));

    public async Task UpdateIdentity(string jwt)
    {
        Identity = _jwt.ReadJWT(jwt);
        _currentUser = CreatePrincipal(Identity);

        await _storage.SetItemAsync(UserKey, jwt);

        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    public async Task ResetIdentity()
    {
        Identity = null;
        _currentUser = new(new ClaimsIdentity());

        await _storage.RemoveItemAsync(UserKey);

        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    private static ClaimsPrincipal CreatePrincipal(JwtSecurityToken jwt) => new(CreateIdentity(jwt));
    private static ClaimsIdentity CreateIdentity(JwtSecurityToken jwt) => new(jwt.Claims, AuthenticationType);
}
