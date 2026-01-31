using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Noxy.NET.EntityManagement.Application.Interfaces.Services;
using Noxy.NET.EntityManagement.Domain.Models.Forms.Authentication;
using Noxy.NET.EntityManagement.Domain.Models.Responses.Authentication;

namespace Noxy.NET.EntityManagement.API.Controllers;

[Route("[controller]")]
[ApiController]
public class AuthenticationController(IAuthenticationService serviceAuthentication) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<SignUpResponse>> SignUp(AuthenticationSignUpFormModel model)
    {
        return await serviceAuthentication.SignUpUser(model);
    }

    [HttpPost("SignIn")]
    public async Task<ActionResult<SignInResponse>> SignIn(AuthenticationSignInFormModel model)
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
