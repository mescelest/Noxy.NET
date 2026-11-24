using Noxy.NET.MediaPlayer.Presentation.Interfaces.Services;

namespace Noxy.NET.Test.MediaPlayer.Fakes;

public sealed class FakeFolderOpener : IFolderOpener
{
    public bool IsSupported { get; set; } = true;

    // The paths that were shown, in order.
    public List<string> RevealedList { get; } = [];

    // When set, Reveal throws it after recording the path.
    public Exception? Failure { get; set; }

    public Task Reveal(string path)
    {
        RevealedList.Add(path);
        if (Failure is not null) throw Failure;

        return Task.CompletedTask;
    }
}
