namespace Noxy.NET.MediaPlayer.Presentation.Models;

// What the system media overlay shows: the same as the player bar.
public sealed record MediaKeyState(bool IsPlaying, string Title, string Artist);
