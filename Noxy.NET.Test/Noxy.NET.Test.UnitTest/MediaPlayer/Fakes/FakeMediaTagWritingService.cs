using Noxy.NET.MediaPlayer.Application.Interfaces.Services;
using Noxy.NET.MediaPlayer.Domain.Models;

namespace Noxy.NET.Test.MediaPlayer.Fakes;

public sealed class FakeMediaTagWritingService : IMediaTagWritingService
{
    public List<(string FilePath, MediaTagValues Values)> WriteList { get; } = [];

    // The picture each write was given, in the order of WriteList (null: none).
    public List<byte[]?> PictureList { get; } = [];
    public Exception? Failure { get; set; }
    public Action<string>? BeforeWrite { get; set; }

    public void Write(string filePath, MediaTagValues values, byte[]? picture = null)
    {
        BeforeWrite?.Invoke(filePath);

        if (Failure is not null) throw Failure;

        WriteList.Add((filePath, values));
        PictureList.Add(picture);
    }
}
