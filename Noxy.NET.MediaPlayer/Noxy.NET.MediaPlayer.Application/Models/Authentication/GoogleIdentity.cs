namespace Noxy.NET.MediaPlayer.Application.Models.Authentication;

public record GoogleIdentity(string Subject, string Email, bool IsEmailVerified);
