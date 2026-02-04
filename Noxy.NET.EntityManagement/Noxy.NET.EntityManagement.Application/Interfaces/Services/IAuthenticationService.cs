using Noxy.NET.EntityManagement.Domain.Models.Forms.Authentication;
using Noxy.NET.EntityManagement.Domain.Models.Responses.Authentication;

namespace Noxy.NET.EntityManagement.Application.Interfaces.Services;

public interface IAuthenticationService
{
    Task<AuthenticationSignInResponse> SignInUser(FormModelAuthenticationSignIn model);
    Task<AuthenticationSignUpResponse> SignUpUser(FormModelAuthenticationSignUp model);
    Task<AuthenticationRenewResponse> RenewUser(string email);
}
