using Google.Apis.Auth;
using Microsoft.Extensions.Options;
using Noxy.NET.MediaPlayer.Application.Configurations;
using Noxy.NET.MediaPlayer.Application.Interfaces.Services;
using Noxy.NET.MediaPlayer.Application.Models.Authentication;
using Noxy.NET.MediaPlayer.Domain.Exceptions;

namespace Noxy.NET.MediaPlayer.Application.Services;

public class GoogleTokenService(IOptions<ConfigGoogle> configGoogle) : IGoogleTokenService
{
    public async Task<GoogleIdentity> Validate(string idToken)
    {
        if (string.IsNullOrWhiteSpace(configGoogle.Value.ClientId))
        {
            throw new GoogleLoginNotConfiguredException();
        }

        if (string.IsNullOrWhiteSpace(idToken))
        {
            throw new AuthenticationFailedException();
        }

        try
        {
            // Checks the signature against Google's published keys, the issuer, the expiry, and that the token was issued for our client ID.
            GoogleJsonWebSignature.Payload payload = await GoogleJsonWebSignature.ValidateAsync(idToken, new() { Audience = [configGoogle.Value.ClientId] });

            if (string.IsNullOrEmpty(payload.Subject) || string.IsNullOrEmpty(payload.Email))
            {
                throw new AuthenticationFailedException();
            }

            return new(payload.Subject, payload.Email, payload.EmailVerified);
        }
        catch (InvalidJwtException)
        {
            throw new AuthenticationFailedException();
        }
    }
}
