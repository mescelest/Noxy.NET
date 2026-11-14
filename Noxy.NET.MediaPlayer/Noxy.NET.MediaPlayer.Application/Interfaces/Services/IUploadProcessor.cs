namespace Noxy.NET.MediaPlayer.Application.Interfaces.Services;

public interface IUploadProcessor
{
    // Finishes the oldest pending upload job and reports the result to its owner. False when no job was pending.
    // True otherwise, including when another claimer took the job first.
    Task<bool> ProcessNext(CancellationToken cancellationToken = default);

    // Run once at start-up, before any upload is accepted: puts interrupted jobs back in the queue
    // and deletes staged files that no job points to.
    Task Recover(CancellationToken cancellationToken = default);
}
