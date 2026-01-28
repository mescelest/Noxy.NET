using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Noxy.NET.CaseManagement.Application.Interfaces.Services;
using Noxy.NET.CaseManagement.Domain.Entities.Authentication;

namespace Noxy.NET.CaseManagement.Application.Services;

public class JWTService(IConfiguration serviceConfiguration) : IJWTService
{
    private JwtSecurityTokenHandler TokenHandler { get; } = new();

    private byte[] Secret => Encoding.ASCII.GetBytes(serviceConfiguration["Authentication:Secret"] ?? throw new InvalidOperationException());
    private string Algorithm => serviceConfiguration["Authentication:Algorithm"] ?? throw new InvalidOperationException();
    private string Audience => serviceConfiguration["Authentication:Audience"] ?? throw new InvalidOperationException();
    private string Issuer => serviceConfiguration["Authentication:Issuer"] ?? throw new InvalidOperationException();

    public string Create(EntityUser entityUser)
    {
        SecurityTokenDescriptor tokenDescriptor = new()
        {
            Subject = new([
                new(JwtRegisteredClaimNames.Email, entityUser.Email),
                new(JwtRegisteredClaimNames.Aud, Audience),
                new(JwtRegisteredClaimNames.Iss, Issuer)
            ]),
            Expires = DateTime.UtcNow.AddDays(7),
            SigningCredentials = GetSigningCredentials()
        };

        SecurityToken? token = TokenHandler.CreateToken(tokenDescriptor);
        return TokenHandler.WriteToken(token);
    }

    public JwtSecurityToken ReadJWT(string jwt)
    {
        return TokenHandler.ReadJwtToken(jwt);
    }
    
    public SigningCredentials GetSigningCredentials()
    {
        return new(new SymmetricSecurityKey(Secret), Algorithm);
    }
}