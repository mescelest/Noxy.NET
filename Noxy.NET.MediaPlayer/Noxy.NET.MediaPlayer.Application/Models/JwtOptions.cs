using System.ComponentModel.DataAnnotations;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace Noxy.NET.MediaPlayer.Application.Models;

public class JwtOptions
{
    public const string SectionName = "Authentication";

    [Required]
    [MinLength(32)]
    public string Secret { get; set; } = string.Empty;

    [Required]
    public string Issuer { get; set; } = string.Empty;

    [Required]
    public string Audience { get; set; } = string.Empty;

    [Range(1, 1440)]
    public int AccessTokenMinutes { get; set; } = 15;

    [Range(1, 365)]
    public int RefreshTokenDays { get; set; } = 30;

    public SymmetricSecurityKey CreateSigningKey()
    {
        return new(Encoding.UTF8.GetBytes(Secret));
    }
}
