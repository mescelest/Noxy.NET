using Noxy.NET.MediaPlayer.Domain.Responses.Authentication;

namespace Noxy.NET.MediaPlayer.Presentation.Interfaces.Services;

// Sends no bearer token. A response that is not a success throws ApiException with the server's status and title.
public interface IAuthApiClient
{
    Task<ResponseAuthentication> SignUp(string email, string password);

    Task<ResponseAuthentication> SignIn(string email, string password);

    Task<ResponseAuthentication> SignInGoogle(string idToken);

    Task<ResponseAuthentication> Refresh(string refreshToken);
}
