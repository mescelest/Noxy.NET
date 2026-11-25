namespace Noxy.NET.MediaPlayer.Presentation.Models;

// A picture the user chose: its bytes and the kind of file it came from.
public record PickedPicture(byte[] Bytes, string ContentType);
