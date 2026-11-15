namespace Noxy.NET.MediaPlayer.Application.Models.Authentication;

public record RefreshToken(string Token, string Hash, DateTime TimeExpires);
