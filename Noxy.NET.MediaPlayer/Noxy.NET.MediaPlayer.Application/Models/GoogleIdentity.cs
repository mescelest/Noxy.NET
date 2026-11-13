namespace Noxy.NET.MediaPlayer.Application.Models;

// Subject is Google's stable user ID (the "sub" claim). Email can change, Subject cannot.
public record GoogleIdentity(string Subject, string Email, bool IsEmailVerified);
