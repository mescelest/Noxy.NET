using Noxy.NET.MediaPlayer.Domain.Responses.Media;

namespace Noxy.NET.MediaPlayer.App.Models;

// Key is what a row is told apart by: the media file's ID in the Library, the entry's ID in a playlist, where one media file can appear twice.
public sealed record MediaFileTableRow(Guid Key, ResponseMediaFile MediaFile);
