using Microsoft.Extensions.Options;
using Noxy.NET.MediaPlayer.Application.Configurations;
using Noxy.NET.MediaPlayer.Application.Handlers.Playlist;
using Noxy.NET.MediaPlayer.Application.Notifications.Playlist;
using Noxy.NET.MediaPlayer.Domain.Responses.Playlist;
using Noxy.NET.Test.MediaPlayer.Fakes;

namespace Noxy.NET.Test.MediaPlayer.Playlists;

public class HandlerPlaylistCreatePositionTest
{
    private static readonly Guid UserID = Guid.CreateVersion7();

    private readonly FakeStore _store = new();
    private readonly FakePublisher _publisher = new();
    private readonly HandlerPlaylistCreate _handler;

    public HandlerPlaylistCreatePositionTest()
    {
        _handler = new(new FakeUnitOfWorkFactory(_store), Options.Create(new ConfigPlaylist()), _publisher, new FakeTimeProvider());
    }

    private static CancellationToken Token => TestContext.Current.CancellationToken;

    private void Seed(string name, int position, Guid? userID = null)
    {
        _store.PlaylistList.Add(new() { UserID = userID ?? UserID, Name = name, Position = position, TimeCreated = DateTime.UtcNow, TimeModified = DateTime.UtcNow });
    }

    [Fact]
    public async Task Create_PutsTheNewPlaylistOnTop_AndMovesTheOthersDownByOne()
    {
        Seed("A", 0);
        Seed("B", 1);

        ResponsePlaylist created = await _handler.Handle(new(UserID, "New", true), Token);

        Assert.Equal(0, created.Position);
        Assert.Equal(["New", "A", "B"], _store.PlaylistList.OrderBy(x => x.Position).Select(x => x.Name));
        Assert.Equal([0, 1, 2], _store.PlaylistList.OrderBy(x => x.Position).Select(x => x.Position));
    }

    [Fact]
    public async Task Create_DoesNotChangeAnotherUsersPositions()
    {
        Guid other = Guid.CreateVersion7();
        Seed("Theirs A", 0, other);
        Seed("Theirs B", 1, other);

        await _handler.Handle(new(UserID, "Mine", true), Token);

        Assert.Equal([("Theirs A", 0), ("Theirs B", 1)], _store.PlaylistList.Where(x => x.UserID == other).OrderBy(x => x.Position).Select(x => (x.Name, x.Position)));
    }

    [Fact]
    public async Task Create_WithNoPlaylists_GivesPositionZero()
    {
        ResponsePlaylist created = await _handler.Handle(new(UserID, "First", true), Token);

        Assert.Equal(0, created.Position);
    }

    [Fact]
    public async Task Create_PublishesPlaylistOrderChanged_WhenOthersMovedDown()
    {
        Seed("A", 0);

        await _handler.Handle(new(UserID, "New", true), Token);

        Assert.Equal(UserID, Assert.Single(_publisher.GetList<NotificationPlaylistOrderChanged>()).UserID);
    }

    [Fact]
    public async Task Create_ForTheFirstPlaylistOfAUser_PublishesNoOrderChange()
    {
        Seed("Theirs", 0, Guid.CreateVersion7());

        await _handler.Handle(new(UserID, "First", true), Token);

        Assert.Empty(_publisher.GetList<NotificationPlaylistOrderChanged>());
    }
}
