using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Noxy.NET.CaseManagement.Application.Interfaces.Services;
using Noxy.NET.CaseManagement.Domain.Forms.Authentication;

namespace Noxy.NET.CaseManagement.API.Controllers;

[Route("[controller]")]
[ApiController]
public class UserController(IAuthenticationService serviceAuthentication) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<string>> CreateUser(AuthenticationSignUpAPIFormModel model)
    {
        return await serviceAuthentication.SignUpUser(model);
    }

    [HttpPost("LogIn")]
    public async Task<ActionResult<string>> LogIn(AuthenticationSignInAPIFormModel model)
    {
        return await serviceAuthentication.SignInUser(model);
    }

    [Authorize]
    [HttpPost("Renew")]
    public async Task<ActionResult<string>> Renew()
    {
        Claim? claim = HttpContext.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Email);
        if (claim == null) return Unauthorized();
        return await serviceAuthentication.RenewUser(claim.Value);
    }
}