using Noxy.NET.MediaPlayer.Application.Handlers.Playlist;
using Noxy.NET.MediaPlayer.Application.Notifications.Playlist;
using Noxy.NET.MediaPlayer.Domain.Entities;
using Noxy.NET.MediaPlayer.Domain.Exceptions;
using Noxy.NET.MediaPlayer.Domain.Responses.Playlist;
using Noxy.NET.Test.MediaPlayer.Fakes;

namespace Noxy.NET.Test.MediaPlayer.Playlists;

public class HandlerPlaylistMoveTest
{
    private static readonly Guid UserID = Guid.CreateVersion7();
    private static readonly DateTime Modified = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    private readonly FakeStore _store = new();
    private readonly FakePublisher _publisher = new();
    private readonly HandlerPlaylistMove _handler;

    public HandlerPlaylistMoveTest()
    {
        _handler = new(new FakeUnitOfWorkFactory(_store), _publisher);
    }

    private static CancellationToken Token => TestContext.Current.CancellationToken;

    private EntityPlaylist Seed(string name, int position, Guid? userID = null)
    {
        EntityPlaylist playlist = new() { UserID = userID ?? UserID, Name = name, Position = position, TimeCreated = Modified, TimeModified = Modified };
        _store.PlaylistList.Add(playlist);

        return playlist;
    }

    private List<string> Order(Guid? userID = null)
    {
        return [.. _store.PlaylistList.Where(x => x.UserID == (userID ?? UserID)).OrderBy(x => x.Position).Select(x => x.Name)];
    }

    [Fact]
    public async Task Move_Down_PutsThePlaylistAtThePosition_AndRenumbersTheOthers()
    {
        EntityPlaylist a = Seed("A", 0);
        Seed("B", 1);
        Seed("C", 2);

        List<ResponsePlaylist> result = await _handler.Handle(new(UserID, a.ID, 2), Token);

        Assert.Equal(["B", "C", "A"], Order());
        Assert.Equal([0, 1, 2], _store.PlaylistList.OrderBy(x => x.Position).Select(x => x.Position));
        Assert.Equal(["B", "C", "A"], result.Select(x => x.Name));
        Assert.Equal([0, 1, 2], result.Select(x => x.Position));
    }

    [Fact]
    public async Task Move_Up_PutsThePlaylistAtThePosition_AndRenumbersTheOthers()
    {
        Seed("A", 0);
        Seed("B", 1);
        EntityPlaylist c = Seed("C", 2);

        await _handler.Handle(new(UserID, c.ID, 0), Token);

        Assert.Equal(["C", "A", "B"], Order());
    }

    [Fact]
    public async Task Move_ToItsOwnPosition_ChangesNothing_AndSavesNothing()
    {
        Seed("A", 0);
        EntityPlaylist b = Seed("B", 1);
        int commits = _store.CommitCount;

        List<ResponsePlaylist> result = await _handler.Handle(new(UserID, b.ID, 1), Token);

        Assert.Equal(commits, _store.CommitCount);
        Assert.Equal(["A", "B"], result.Select(x => x.Name));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(2)]
    public async Task Move_OutOfRange_Throws_AndChangesNothing(int position)
    {
        Seed("A", 0);
        EntityPlaylist b = Seed("B", 1);

        await Assert.ThrowsAsync<InvalidPlaylistPositionException>(() => _handler.Handle(new(UserID, b.ID, position), Token).AsTask());

        Assert.Equal(["A", "B"], Order());
    }

    [Fact]
    public async Task Move_OfAnotherUsersPlaylist_Throws_AndTheirOrderIsUntouched()
    {
        Guid other = Guid.CreateVersion7();
        EntityPlaylist theirs = Seed("Theirs A", 0, other);
        Seed("Theirs B", 1, other);

        await Assert.ThrowsAsync<PlaylistNotFoundException>(() => _handler.Handle(new(UserID, theirs.ID, 1), Token).AsTask());

        Assert.Equal(["Theirs A", "Theirs B"], Order(other));
    }

    [Fact]
    public async Task Move_OfAnUnknownPlaylist_Throws()
    {
        Seed("A", 0);

        await Assert.ThrowsAsync<PlaylistNotFoundException>(() => _handler.Handle(new(UserID, Guid.CreateVersion7(), 0), Token).AsTask());
    }

    [Fact]
    public async Task Move_DoesNotChangeAnyModifiedTime_AndLeavesAnotherUsersPlaylistsAlone()
    {
        EntityPlaylist a = Seed("A", 0);
        Seed("B", 1);
        EntityPlaylist theirs = Seed("Theirs", 0, Guid.CreateVersion7());

        await _handler.Handle(new(UserID, a.ID, 1), Token);

        Assert.All(_store.PlaylistList, x => Assert.Equal(Modified, x.TimeModified));
        Assert.Equal(0, theirs.Position);
    }

    [Fact]
    public async Task Move_PublishesPlaylistOrderChanged_OnlyWhenSomethingMoved()
    {
        EntityPlaylist a = Seed("A", 0);
        Seed("B", 1);

        await _handler.Handle(new(UserID, a.ID, 0), Token);
        Assert.Empty(_publisher.GetList<NotificationPlaylistOrderChanged>());

        await _handler.Handle(new(UserID, a.ID, 1), Token);
        Assert.Equal(UserID, Assert.Single(_publisher.GetList<NotificationPlaylistOrderChanged>()).UserID);
    }

    [Fact]
    public async Task Move_WhenTheCommitFails_PublishesNothing()
    {
        EntityPlaylist a = Seed("A", 0);
        Seed("B", 1);
        _store.FailCommit = true;

        await Assert.ThrowsAsync<InvalidOperationException>(() => _handler.Handle(new(UserID, a.ID, 1), Token).AsTask());

        Assert.Empty(_publisher.PublishedList);
    }
}
