using System.IdentityModel.Tokens.Jwt;
using Noxy.NET.CaseManagement.Domain.Entities.Authentication;

namespace Noxy.NET.CaseManagement.Application.Interfaces.Services;

public interface IJWTService
{
    string Create(EntityUser entityUser);
    JwtSecurityToken ReadJWT(string jwt);
}
