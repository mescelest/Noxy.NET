using Noxy.NET.MediaPlayer.Application.Interfaces.Services;
using Noxy.NET.MediaPlayer.Application.Models.Authentication;
using Noxy.NET.MediaPlayer.Domain.Exceptions;

namespace Noxy.NET.Test.MediaPlayer.Fakes;

public sealed class FakeGoogleTokenService : IGoogleTokenService
{
    private readonly Dictionary<string, GoogleIdentity> _identities = [];

    public void Register(string idToken, GoogleIdentity identity)
    {
        _identities[idToken] = identity;
    }

    public Task<GoogleIdentity> Validate(string idToken)
    {
        return _identities.TryGetValue(idToken, out GoogleIdentity? identity) ? Task.FromResult(identity) : throw new AuthenticationFailedException();
    }
}
