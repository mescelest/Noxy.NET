namespace Noxy.NET.MediaPlayer.Application.Models.Metadata;

// The bytes are null when the caller already holds the picture the ETag names.
public record AlbumArtPicture(string ETag, byte[]? Bytes);
