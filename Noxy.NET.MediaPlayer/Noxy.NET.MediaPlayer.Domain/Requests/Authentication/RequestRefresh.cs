using System.ComponentModel.DataAnnotations;

namespace Noxy.NET.MediaPlayer.Domain.Requests.Authentication;

public class RequestRefresh
{
    [Required]
    [MaxLength(128)]
    public string RefreshToken { get; set; } = string.Empty;
}
