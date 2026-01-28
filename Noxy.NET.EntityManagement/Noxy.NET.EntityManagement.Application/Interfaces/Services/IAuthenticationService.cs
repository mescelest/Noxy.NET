using Noxy.NET.EntityManagement.Domain.Models.Forms.Authentication;
using Noxy.NET.EntityManagement.Domain.Models.Responses.Authentication;

namespace Noxy.NET.EntityManagement.Application.Interfaces.Services;

public interface IAuthenticationService
{
    Task<SignInResponse> SignInUser(AuthenticationSignInAPIFormModel model);
    Task<SignUpResponse> SignUpUser(AuthenticationSignUpAPIFormModel model);
    Task<string> RenewUser(string email);
}
