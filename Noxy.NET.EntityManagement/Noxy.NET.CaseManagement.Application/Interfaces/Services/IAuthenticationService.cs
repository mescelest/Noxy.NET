using Noxy.NET.CaseManagement.Domain.Forms.Authentication;

namespace Noxy.NET.CaseManagement.Application.Interfaces.Services;

public interface IAuthenticationService
{
    Task<string> SignInUser(AuthenticationSignInAPIFormModel model);
    Task<string> SignUpUser(AuthenticationSignUpAPIFormModel model);
    Task<string> RenewUser(string email);
}