namespace Noxy.NET.MediaPlayer.Application.Models.Media;

public record MediaDownload(Stream Content, string ContentType, string FileName, string ContentHash);
