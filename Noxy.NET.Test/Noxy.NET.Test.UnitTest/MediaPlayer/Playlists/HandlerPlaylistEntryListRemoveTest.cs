using Noxy.NET.MediaPlayer.Application.Handlers.Playlist;
using Noxy.NET.MediaPlayer.Application.Notifications.Playlist;
using Noxy.NET.MediaPlayer.Domain.Entities;
using Noxy.NET.MediaPlayer.Domain.Exceptions;
using Noxy.NET.MediaPlayer.Domain.Responses.Playlist;
using Noxy.NET.Test.MediaPlayer.Fakes;

namespace Noxy.NET.Test.MediaPlayer.Playlists;

public class HandlerPlaylistEntryListRemoveTest
{
    private static readonly Guid UserID = Guid.CreateVersion7();

    private readonly FakeStore _store = new();
    private readonly FakePublisher _publisher = new();
    private readonly FakeTimeProvider _time = new();
    private readonly HandlerPlaylistEntryListRemove _handler;

    public HandlerPlaylistEntryListRemoveTest()
    {
        _handler = new(new FakeUnitOfWorkFactory(_store), _publisher, _time);
    }

    private static CancellationToken Token => TestContext.Current.CancellationToken;

    private Guid MediaFile()
    {
        EntityMediaFile file = new()
        {
            FilePath = $"{Guid.CreateVersion7()}.mp3",
            ContentType = "audio/mpeg",
            SizeBytes = 3,
            ContentHash = new string('a', 64),
            UserID = UserID
        };

        _store.MediaFileList.Add(file);
        return file.ID;
    }

    private List<Guid> MediaFileList(int count)
    {
        return [.. Enumerable.Range(0, count).Select(_ => MediaFile())];
    }

    private void Advance()
    {
        _time.Now = _time.Now.AddSeconds(1);
    }

    private Guid Create(string name = "Mix", IEnumerable<Guid>? mediaFileList = null)
    {
        DateTime now = _time.Now.UtcDateTime;
        EntityPlaylist playlist = new()
        {
            UserID = UserID,
            Name = name,
            TimeCreated = now,
            TimeModified = now
        };

        foreach (Guid mediaFileID in mediaFileList ?? [])
        {
            playlist.EntryList.Add(new() { PlaylistID = playlist.ID, MediaFileID = mediaFileID, Position = playlist.EntryList.Count });
        }

        _store.PlaylistList.Add(playlist);
        return playlist.ID;
    }

    private Task<ResponsePlaylistDetail> Remove(Guid playlistID, params Guid[] entryIDList)
    {
        return _handler.Handle(new(UserID, playlistID, [.. entryIDList]), Token).AsTask();
    }

    private EntityPlaylist Stored(Guid playlistID)
    {
        return _store.PlaylistList.Single(x => x.ID == playlistID);
    }

    private List<Guid> EntryIDList(Guid playlistID)
    {
        return [.. Stored(playlistID).EntryList.OrderBy(x => x.Position).Select(x => x.ID)];
    }

    private List<Guid> StoredMediaFileOrder(Guid playlistID)
    {
        return [.. Stored(playlistID).EntryList.OrderBy(x => x.Position).Select(x => x.MediaFileID)];
    }

    private void AssertDense(Guid playlistID)
    {
        List<int> positions = [.. Stored(playlistID).EntryList.Select(x => x.Position).Order()];

        Assert.Equal(Enumerable.Range(0, positions.Count), positions);
    }

    [Fact]
    public async Task Remove_WhenTheCommitFails_PublishesNothing()
    {
        Guid id = Create(mediaFileList: MediaFileList(2));
        Guid entryID = EntryIDList(id)[0];
        _store.FailCommit = true;

        await Assert.ThrowsAsync<InvalidOperationException>(() => Remove(id, entryID));

        Assert.Empty(_publisher.PublishedList);
    }

    [Fact]
    public async Task Remove_PublishesOnlyAfterTheCommit()
    {
        Guid id = Create(mediaFileList: MediaFileList(2));
        Guid entryID = EntryIDList(id)[0];
        List<int> commitsAtPublishList = [];
        _publisher.OnPublish = _ => commitsAtPublishList.Add(_store.CommitCount);
        int commits = _store.CommitCount;

        await Remove(id, entryID);

        Assert.Equal([commits + 1], commitsAtPublishList);
    }

    [Fact]
    public async Task Remove_PassesTheCancellationTokenToThePublish()
    {
        Guid id = Create(mediaFileList: MediaFileList(2));
        Guid entryID = EntryIDList(id)[0];
        using CancellationTokenSource source = new();

        await _handler.Handle(new(UserID, id, [entryID]), source.Token);

        Assert.Equal(source.Token, Assert.Single(_publisher.TokenList));
    }

    [Fact]
    public async Task Remove_RemovesAndRenumbers()
    {
        List<Guid> mediaFileList = MediaFileList(3);
        Guid id = Create(mediaFileList: mediaFileList);

        ResponsePlaylistDetail after = await Remove(id, EntryIDList(id)[0]);

        Assert.Equal([mediaFileList[1], mediaFileList[2]], after.EntryList.Select(x => x.MediaFileID));
        Assert.Equal([0, 1], after.EntryList.Select(x => x.Position));
        Assert.Equal([mediaFileList[1], mediaFileList[2]], StoredMediaFileOrder(id));
        AssertDense(id);
    }

    [Fact]
    public async Task Remove_SeveralAtOnce_LeavesDensePositions()
    {
        List<Guid> mediaFileList = MediaFileList(5);
        Guid id = Create(mediaFileList: mediaFileList);
        List<Guid> entryIDList = EntryIDList(id);

        await Remove(id, entryIDList[1], entryIDList[3], entryIDList[1]);

        Assert.Equal([mediaFileList[0], mediaFileList[2], mediaFileList[4]], StoredMediaFileOrder(id));
        AssertDense(id);
    }

    [Fact]
    public async Task Remove_UnknownEntry_ThrowsAndRemovesNothing()
    {
        Guid id = Create(mediaFileList: MediaFileList(2));

        await Assert.ThrowsAsync<PlaylistEntryNotFoundException>(() => Remove(id, EntryIDList(id)[0], Guid.CreateVersion7()));

        Assert.Equal(2, Stored(id).EntryList.Count);
    }

    [Fact]
    public async Task Remove_AnEntryOfAnotherPlaylist_ThrowsNotFound()
    {
        Guid id = Create(mediaFileList: MediaFileList(1));
        Guid other = Create("Other", MediaFileList(1));

        await Assert.ThrowsAsync<PlaylistEntryNotFoundException>(() => Remove(id, EntryIDList(other)[0]));
        Assert.Single(Stored(other).EntryList);
    }

    [Fact]
    public async Task Remove_AnotherUsersPlaylist_ThrowsNotFound()
    {
        Guid id = Create(mediaFileList: MediaFileList(1));

        await Assert.ThrowsAsync<PlaylistNotFoundException>(() => _handler.Handle(new(Guid.CreateVersion7(), id, [EntryIDList(id)[0]]), Token).AsTask());
    }

    [Fact]
    public async Task Remove_BumpsAndPushes()
    {
        Guid id = Create(mediaFileList: MediaFileList(2));
        Guid entryID = EntryIDList(id)[0];
        Advance();

        await Remove(id, entryID);

        Assert.Equal(_time.Now.UtcDateTime, Stored(id).TimeModified);
        NotificationPlaylistUpdated pushed = Assert.Single(_publisher.GetList<NotificationPlaylistUpdated>());
        Assert.Equal(1, pushed.Playlist.EntryCount);
    }
}
