using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Noxy.NET.EntityManagement.API.Queries;
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
        return await mediator.Send(new QueryAuthenticationSignUp(request));
    }

    [HttpPost("SignIn")]
    public async Task<ActionResult<ResponseAuthenticationSignIn>> SignIn(RequestAuthenticationSignIn request)
    {
        return await mediator.Send(new QueryAuthenticationSignIn(request));
    }

    [Authorize]
    [HttpPost("Renew")]
    public async Task<ActionResult<ResponseAuthenticationRenew>> Renew()
    {
        Claim? claim = HttpContext.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Email);
        if (claim == null) return Unauthorized();
        return await mediator.Send(new QueryAuthenticationRenew(claim.Value));
    }
}
