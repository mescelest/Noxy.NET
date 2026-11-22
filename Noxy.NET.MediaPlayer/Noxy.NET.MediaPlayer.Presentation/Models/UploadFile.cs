namespace Noxy.NET.MediaPlayer.Presentation.Models;

// A file the user picked to upload. The stream is opened when it is sent, and again if the send has to be tried a second time.
public record UploadFile(string Name, string? ContentType, Func<Stream> OpenContent);
