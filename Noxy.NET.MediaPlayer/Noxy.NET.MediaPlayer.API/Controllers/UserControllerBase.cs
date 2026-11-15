using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace Noxy.NET.MediaPlayer.API.Controllers;

public abstract class UserControllerBase : ControllerBase
{
    // The bearer validation and the fallback authorization policy both reject a user whose sub is not a user ID, so by the time an action runs this cannot be missing.
    protected Guid UserID => Guid.TryParse(User.FindFirstValue(JwtRegisteredClaimNames.Sub), out Guid userID)
        ? userID
        : throw new InvalidOperationException("The signed-in user has no valid sub claim; the bearer validation should have rejected the token.");
}
