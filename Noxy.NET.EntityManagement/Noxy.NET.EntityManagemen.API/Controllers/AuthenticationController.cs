using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Noxy.NET.EntityManagement.API.Commands;
using Noxy.NET.EntityManagement.Domain.Requests;
using Noxy.NET.EntityManagement.Domain.Responses;

namespace Noxy.NET.EntityManagement.API.Controllers;

[Route("[controller]")]
[ApiController]
public class AuthenticationController(IMediator mediator) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<ResponseAuthenticationSignUp>> SignUp(RequestAuthenticationSignUp request)
    {
        return await mediator.Send(new CommandAuthenticationSignUp(request));
    }

    [HttpPost("SignIn")]
    public async Task<ActionResult<ResponseAuthenticationSignIn>> SignIn(RequestAuthenticationSignIn request)
    {
        return await mediator.Send(new CommandAuthenticationSignIn(request));
    }

    [Authorize]
    [HttpPost("Renew")]
    public async Task<ActionResult<ResponseAuthenticationRenew>> Renew()
    {
        Claim? claim = HttpContext.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Email);
        if (claim == null) return Unauthorized();
        return await mediator.Send(new CommandAuthenticationRenew(claim.Value));
    }
}
