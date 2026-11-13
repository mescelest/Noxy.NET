namespace Noxy.NET.MediaPlayer.Application.Models;

// Token goes to the client; Hash is what gets stored. The plain token is never persisted.
public record RefreshTokenSecret(string Token, string Hash, DateTime TimeExpires);
