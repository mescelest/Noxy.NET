using Noxy.NET.MediaPlayer.Presentation.Models;

namespace Noxy.NET.MediaPlayer.Presentation.Interfaces.Stores;

public interface ILocalQueueStore
{
    // Null when nothing was saved.
    Task<LocalQueueSnapshot?> Get();

    // Replaces the one saved snapshot, in one write.
    Task Save(LocalQueueSnapshot snapshot);

    // Harmless when nothing is saved.
    Task Clear();
}
