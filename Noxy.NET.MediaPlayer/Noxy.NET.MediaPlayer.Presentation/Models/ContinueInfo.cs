namespace Noxy.NET.MediaPlayer.Presentation.Models;

// What the Continue card shows: the media file the saved queue stopped at and where in it.
public record ContinueInfo(string? Title, string? Artist, double PositionSeconds);
