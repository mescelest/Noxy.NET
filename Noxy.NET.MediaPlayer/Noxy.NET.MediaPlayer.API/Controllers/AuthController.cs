using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Noxy.NET.MediaPlayer.Application.Interfaces.Services;
using Noxy.NET.MediaPlayer.Domain.Requests.Authentication;
using Noxy.NET.MediaPlayer.Domain.Responses.Authentication;

namespace Noxy.NET.MediaPlayer.API.Controllers;

[AllowAnonymous]
[EnableRateLimiting(Constants.RateLimitPolicyAuth)]
[Route("[controller]")]
[ApiController]
public class AuthController(IAuthenticationService authenticationService) : ControllerBase
{
    [HttpPost("SignUp")]
    public async Task<ActionResult<ResponseAuthentication>> SignUp(RequestSignUp request, CancellationToken cancellationToken)
    {
        return await authenticationService.SignUp(request, cancellationToken);
    }

    [HttpPost("SignIn")]
    public async Task<ActionResult<ResponseAuthentication>> SignIn(RequestSignIn request, CancellationToken cancellationToken)
    {
        return await authenticationService.SignIn(request, cancellationToken);
    }

    [HttpPost("Refresh")]
    public async Task<ActionResult<ResponseAuthentication>> Refresh(RequestRefresh request, CancellationToken cancellationToken)
    {
        return await authenticationService.Refresh(request, cancellationToken);
    }
}
