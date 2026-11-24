namespace Noxy.NET.MediaPlayer.Application.Interfaces.Services;

public interface IThumbnailService
{
    byte[]? CreateThumbnail(byte[] image);
}
