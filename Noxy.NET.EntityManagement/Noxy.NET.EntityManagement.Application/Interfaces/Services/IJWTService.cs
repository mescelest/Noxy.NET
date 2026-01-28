using System.IdentityModel.Tokens.Jwt;
using Noxy.NET.EntityManagement.Domain.Entities.Authentication;

namespace Noxy.NET.EntityManagement.Application.Interfaces.Services;

public interface IJWTService
{
    string Create(EntityUser entityUser);
    JwtSecurityToken ReadJWT(string jwt);
}
