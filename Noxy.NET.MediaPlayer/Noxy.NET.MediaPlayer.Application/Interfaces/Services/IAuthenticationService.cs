using Noxy.NET.MediaPlayer.Domain.Requests.Authentication;
using Noxy.NET.MediaPlayer.Domain.Responses.Authentication;

namespace Noxy.NET.MediaPlayer.Application.Interfaces.Services;

public interface IAuthenticationService
{
    Task<ResponseAuthentication> SignUp(RequestSignUp request, CancellationToken cancellationToken = default);
    Task<ResponseAuthentication> SignIn(RequestSignIn request, CancellationToken cancellationToken = default);
    Task<ResponseAuthentication> Refresh(RequestRefresh request, CancellationToken cancellationToken = default);
}
