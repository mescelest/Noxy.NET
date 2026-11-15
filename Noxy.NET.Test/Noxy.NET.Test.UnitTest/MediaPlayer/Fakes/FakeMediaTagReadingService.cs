using Noxy.NET.MediaPlayer.Application.Interfaces.Services;
using Noxy.NET.MediaPlayer.Application.Models.Metadata;

namespace Noxy.NET.Test.MediaPlayer.Fakes;

public sealed class FakeMediaTagReadingService : IMediaTagReadingService
{
    public List<string> ReadList { get; } = [];

    // What every read returns unless a path has its own entry.
    public MediaTagSet Result { get; set; } = new(null, [], null, [], [], null, null, null, null, null, null);

    public Dictionary<string, MediaTagSet> ResultByPath { get; } = [];

    public Exception? Failure { get; set; }

    public MediaTagSet Read(string path)
    {
        ReadList.Add(path);

        if (Failure is not null) throw Failure;

        return ResultByPath.GetValueOrDefault(path, Result);
    }
}
