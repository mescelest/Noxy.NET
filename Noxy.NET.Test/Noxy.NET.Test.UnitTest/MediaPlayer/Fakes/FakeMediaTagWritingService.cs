using Noxy.NET.MediaPlayer.Application.Interfaces.Services;
using Noxy.NET.MediaPlayer.Domain.Models;

namespace Noxy.NET.Test.MediaPlayer.Fakes;

public sealed class FakeMediaTagWritingService : IMediaTagWritingService
{
    public List<(string FilePath, MediaTagValues Values)> WriteList { get; } = [];
    public Exception? Failure { get; set; }
    public Action<string>? BeforeWrite { get; set; }

    public void Write(string filePath, MediaTagValues values)
    {
        BeforeWrite?.Invoke(filePath);

        if (Failure is not null) throw Failure;

        WriteList.Add((filePath, values));
    }
}
