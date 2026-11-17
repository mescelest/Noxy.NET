using Noxy.NET.MediaPlayer.Presentation.Interfaces.Services;

namespace Noxy.NET.Test.MediaPlayer.Fakes;

public sealed class FakeRemoteCommandHandlerService : IRemoteCommandHandlerService
{
    public int StartCount { get; private set; }

    public void Start()
    {
        StartCount++;
    }
}
