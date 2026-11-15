using Noxy.NET.MediaPlayer.Domain.Responses.Playback;

namespace Noxy.NET.MediaPlayer.Application.Models.Playback;

// No ETag and no session: the user has none. An ETag and no session: the caller already has this exact session, so there is nothing to send.
public record PlaybackSessionView(string? ETag, ResponsePlaybackSession? Session);
