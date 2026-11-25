using Noxy.NET.MediaPlayer.Application.Interfaces.Services;

namespace Noxy.NET.Test.MediaPlayer.Fakes;

public sealed class FakeAlbumArtService : IAlbumArtService
{
    public List<(Guid UserID, Guid AlbumID, byte[] Picture)> ApplyList { get; } = [];
    public bool Result { get; set; } = true;
    public Exception? Failure { get; set; }

    public List<(Guid UserID, Guid AlbumID, byte[] Picture)> ReplaceList { get; } = [];

    public Task Replace(Guid userID, Guid albumID, byte[] picture, CancellationToken cancellationToken = default)
    {
        ReplaceList.Add((userID, albumID, picture));
        if (Failure is not null) throw Failure;

        return Task.CompletedTask;
    }

    public Task<bool> Apply(Guid userID, Guid albumID, byte[] picture, CancellationToken cancellationToken = default)
    {
        ApplyList.Add((userID, albumID, picture));
        if (Failure is not null) throw Failure;

        return Task.FromResult(Result);
    }
}
