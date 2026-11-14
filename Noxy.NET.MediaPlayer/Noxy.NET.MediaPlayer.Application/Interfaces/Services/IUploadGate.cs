namespace Noxy.NET.MediaPlayer.Application.Interfaces.Services;

public interface IUploadGate
{
    // Lets one upload per user run at a time. Null when that user already has one running; dispose the lease when it ends.
    IDisposable? TryEnter(Guid userID);
}
