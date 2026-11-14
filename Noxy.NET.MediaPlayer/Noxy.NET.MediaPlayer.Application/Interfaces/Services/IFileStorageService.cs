namespace Noxy.NET.MediaPlayer.Application.Interfaces.Services;

public interface IFileStorageService
{
    Task<string> SaveAsync(Stream content, string extension, CancellationToken cancellationToken = default);
    Stream OpenRead(string fileName);
    void Delete(string fileName);
}
