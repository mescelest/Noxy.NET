using Noxy.NET.MediaPlayer.Application.Interfaces.Services;
using Noxy.NET.MediaPlayer.Domain.Entities;

namespace Noxy.NET.Test.MediaPlayer.Fakes;

public sealed class FakeMediaTagWriter : IMediaTagWriter
{
    public List<(string FilePath, MediaFileMetadata Metadata)> WriteList { get; } = [];
    public Exception? Failure { get; set; }
    public Action<string>? BeforeWrite { get; set; }

    public void Write(string filePath, MediaFileMetadata metadata)
    {
        BeforeWrite?.Invoke(filePath);

        if (Failure is not null) throw Failure;

        WriteList.Add((filePath, metadata));
    }
}
