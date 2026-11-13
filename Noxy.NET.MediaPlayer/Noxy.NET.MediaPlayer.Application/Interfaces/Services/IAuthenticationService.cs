using Noxy.NET.MediaPlayer.Domain.Requests.Authentication;

namespace Noxy.NET.MediaPlayer.Application.Interfaces.Services;

public interface IAuthenticationService
{
    Task SignUp(RequestSignUp request, CancellationToken cancellationToken = default);
}
