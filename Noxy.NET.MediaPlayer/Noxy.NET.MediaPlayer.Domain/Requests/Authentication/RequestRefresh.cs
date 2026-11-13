using System.ComponentModel.DataAnnotations;

namespace Noxy.NET.MediaPlayer.Domain.Requests.Authentication;

public class RequestRefresh
{
    [Required]
    public string RefreshToken { get; set; } = string.Empty;
}
