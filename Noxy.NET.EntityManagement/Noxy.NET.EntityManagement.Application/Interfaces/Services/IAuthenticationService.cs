using Noxy.NET.EntityManagement.Domain.Models.Forms.Authentication;
using Noxy.NET.EntityManagement.Domain.Models.Responses.Authentication;

namespace Noxy.NET.EntityManagement.Application.Interfaces.Services;

public interface IAuthenticationService
{
    Task<AuthenticationSignInResponse> SignInUser(AuthenticationSignInFormModel model);
    Task<AuthenticationSignUpResponse> SignUpUser(AuthenticationSignUpFormModel model);
    Task<AuthenticationRenewResponse> RenewUser(string email);
}
