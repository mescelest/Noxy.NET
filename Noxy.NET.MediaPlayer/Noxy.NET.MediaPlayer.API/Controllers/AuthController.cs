using Microsoft.AspNetCore.Mvc;
using Noxy.NET.MediaPlayer.Domain.Requests.Authentication;
using Noxy.NET.MediaPlayer.Domain.Responses.Authentication;

namespace Noxy.NET.MediaPlayer.API.Controllers;

[Route("[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    [HttpPost("SignUp")]
    public ActionResult<ResponseAuthentication> SignUp(RequestSignUp request)
    {
        return StatusCode(StatusCodes.Status501NotImplemented);
    }

    [HttpPost("SignIn")]
    public ActionResult<ResponseAuthentication> SignIn(RequestSignIn request)
    {
        return StatusCode(StatusCodes.Status501NotImplemented);
    }

    [HttpPost("Refresh")]
    public ActionResult<ResponseAuthentication> Refresh(RequestRefresh request)
    {
        return StatusCode(StatusCodes.Status501NotImplemented);
    }
}
