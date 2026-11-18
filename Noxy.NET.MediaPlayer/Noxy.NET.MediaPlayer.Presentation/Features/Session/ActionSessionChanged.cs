namespace Noxy.NET.MediaPlayer.Presentation.Features.Session;

public record ActionSessionChanged(bool IsSignedIn, bool HasSession, bool IsOffline, bool IsRetrying, bool IsRestored);
