using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Noxy.NET.MediaPlayer.Application.Requests.Authentication;
using Noxy.NET.MediaPlayer.Domain.Responses.Authentication;

namespace Noxy.NET.MediaPlayer.API.Controllers;

[AllowAnonymous]
[EnableRateLimiting(Constants.RateLimitPolicyAuth)]
[Route("[controller]")]
[ApiController]
public class AuthController(IMediator mediator) : ControllerBase
{
    [HttpPost("SignUp")]
    public async Task<ActionResult<ResponseAuthentication>> SignUp(RequestSignUp request, CancellationToken cancellationToken)
    {
        return await mediator.Send(request.ToCommand(), cancellationToken);
    }

    [HttpPost("SignIn")]
    public async Task<ActionResult<ResponseAuthentication>> SignIn(RequestSignIn request, CancellationToken cancellationToken)
    {
        return await mediator.Send(request.ToCommand(), cancellationToken);
    }

    [HttpPost("Google")]
    public async Task<ActionResult<ResponseAuthentication>> SignInWithGoogle(RequestSignInGoogle request, CancellationToken cancellationToken)
    {
        return await mediator.Send(request.ToCommand(), cancellationToken);
    }

    [HttpPost("Refresh")]
    public async Task<ActionResult<ResponseAuthentication>> Refresh(RequestRefresh request, CancellationToken cancellationToken)
    {
        return await mediator.Send(request.ToCommand(), cancellationToken);
    }
}
