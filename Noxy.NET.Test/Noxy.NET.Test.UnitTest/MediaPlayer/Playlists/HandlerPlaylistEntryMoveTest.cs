using Noxy.NET.MediaPlayer.Application.Handlers.Playlist;
using Noxy.NET.MediaPlayer.Application.Notifications.Playlist;
using Noxy.NET.MediaPlayer.Domain.Entities;
using Noxy.NET.MediaPlayer.Domain.Exceptions;
using Noxy.NET.MediaPlayer.Domain.Responses.Playlist;
using Noxy.NET.Test.MediaPlayer.Fakes;

namespace Noxy.NET.Test.MediaPlayer.Playlists;

public class HandlerPlaylistEntryMoveTest
{
    private static readonly Guid UserID = Guid.CreateVersion7();

    private readonly FakeStore _store = new();
    private readonly FakePublisher _publisher = new();
    private readonly FakeTimeProvider _time = new();
    private readonly HandlerPlaylistEntryMove _handler;

    public HandlerPlaylistEntryMoveTest()
    {
        _handler = new(new FakeUnitOfWorkFactory(_store), _publisher, _time);
    }

    private static CancellationToken Token => TestContext.Current.CancellationToken;

    private Guid MediaFile()
    {
        MediaFile file = new()
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

    private Guid Create(IEnumerable<Guid> mediaFileList)
    {
        DateTime now = _time.Now.UtcDateTime;
        Playlist playlist = new()
        {
            UserID = UserID,
            Name = "Mix",
            TimeCreated = now,
            TimeModified = now
        };

        foreach (Guid mediaFileID in mediaFileList)
        {
            playlist.EntryList.Add(new() { PlaylistID = playlist.ID, MediaFileID = mediaFileID, Position = playlist.EntryList.Count });
        }

        _store.PlaylistList.Add(playlist);
        return playlist.ID;
    }

    private Task<ResponsePlaylistDetail> Move(Guid playlistID, Guid entryID, int position)
    {
        return _handler.Handle(new(UserID, playlistID, entryID, position), Token).AsTask();
    }

    private Playlist Stored(Guid playlistID)
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
    public async Task Move_WhenTheCommitFails_PublishesNothing()
    {
        Guid id = Create(MediaFileList(2));
        Guid entryID = EntryIDList(id)[0];
        _store.FailCommit = true;

        await Assert.ThrowsAsync<InvalidOperationException>(() => Move(id, entryID, 1));

        Assert.Empty(_publisher.PublishedList);
    }

    [Fact]
    public async Task Move_PublishesOnlyAfterTheCommit()
    {
        Guid id = Create(MediaFileList(3));
        Guid entryID = EntryIDList(id)[2];
        List<int> commitsAtPublishList = [];
        _publisher.OnPublish = _ => commitsAtPublishList.Add(_store.CommitCount);
        int commits = _store.CommitCount;

        await Move(id, entryID, 0);

        Assert.Equal([commits + 1], commitsAtPublishList);
    }

    [Fact]
    public async Task Move_PassesTheCancellationTokenToThePublish()
    {
        Guid id = Create(MediaFileList(2));
        Guid entryID = EntryIDList(id)[0];
        using CancellationTokenSource source = new();

        await _handler.Handle(new(UserID, id, entryID, 1), source.Token);

        Assert.Equal(source.Token, Assert.Single(_publisher.TokenList));
    }

    [Fact]
    public async Task Move_Down_ShiftsTheOthersUp()
    {
        List<Guid> mediaFileList = MediaFileList(4);
        Guid id = Create(mediaFileList);

        ResponsePlaylistDetail after = await Move(id, EntryIDList(id)[0], 2);

        Assert.Equal([mediaFileList[1], mediaFileList[2], mediaFileList[0], mediaFileList[3]], after.EntryList.Select(x => x.MediaFileID));
        Assert.Equal([mediaFileList[1], mediaFileList[2], mediaFileList[0], mediaFileList[3]], StoredMediaFileOrder(id));
        AssertDense(id);
    }

    [Fact]
    public async Task Move_Up_ShiftsTheOthersDown()
    {
        List<Guid> mediaFileList = MediaFileList(4);
        Guid id = Create(mediaFileList);

        await Move(id, EntryIDList(id)[3], 1);

        Assert.Equal([mediaFileList[0], mediaFileList[3], mediaFileList[1], mediaFileList[2]], StoredMediaFileOrder(id));
        AssertDense(id);
    }

    [Fact]
    public async Task Move_ToTheSamePosition_IsANoOp()
    {
        Guid id = Create(MediaFileList(3));
        Guid entryID = EntryIDList(id)[1];
        DateTime modified = Stored(id).TimeModified;
        int commits = _store.CommitCount;
        Advance();

        await Move(id, entryID, 1);

        Assert.Equal(commits, _store.CommitCount);
        Assert.Empty(_publisher.GetList<NotificationPlaylistUpdated>());
        Assert.Equal(modified, Stored(id).TimeModified);
    }

    [Fact]
    public async Task Move_OutOfRange_Throws()
    {
        Guid id = Create(MediaFileList(3));
        Guid entryID = EntryIDList(id)[0];

        await Assert.ThrowsAsync<InvalidPlaylistPositionException>(() => Move(id, entryID, 3));
        await Assert.ThrowsAsync<InvalidPlaylistPositionException>(() => Move(id, entryID, -1));
    }

    [Fact]
    public async Task Move_UnknownEntry_ThrowsNotFound()
    {
        Guid id = Create(MediaFileList(2));

        await Assert.ThrowsAsync<PlaylistEntryNotFoundException>(() => Move(id, Guid.CreateVersion7(), 0));
    }

    [Fact]
    public async Task Move_ToTheLastPosition_Works()
    {
        List<Guid> mediaFileList = MediaFileList(3);
        Guid id = Create(mediaFileList);

        await Move(id, EntryIDList(id)[0], 2);

        Assert.Equal([mediaFileList[1], mediaFileList[2], mediaFileList[0]], StoredMediaFileOrder(id));
    }

    [Fact]
    public async Task Move_AnotherUsersPlaylist_ThrowsNotFound()
    {
        Guid id = Create(MediaFileList(2));
        Guid entryID = EntryIDList(id)[0];

        await Assert.ThrowsAsync<PlaylistNotFoundException>(() => _handler.Handle(new(Guid.CreateVersion7(), id, entryID, 1), Token).AsTask());
    }

    [Fact]
    public async Task Move_BumpsAndPushes()
    {
        Guid id = Create(MediaFileList(3));
        Guid entryID = EntryIDList(id)[0];
        Advance();

        await Move(id, entryID, 2);

        Assert.Equal(_time.Now.UtcDateTime, Stored(id).TimeModified);
        NotificationPlaylistUpdated pushed = Assert.Single(_publisher.GetList<NotificationPlaylistUpdated>());
        Assert.Equal(3, pushed.Playlist.EntryCount);
    }

    [Fact]
    public async Task Move_PositionsAreAlwaysDense_AfterSeveralMoves()
    {
        Guid repeated = MediaFile();
        Guid id = Create([repeated, .. MediaFileList(3), repeated]);

        await Move(id, EntryIDList(id)[4], 0);
        AssertDense(id);

        await Move(id, EntryIDList(id)[0], 4);
        AssertDense(id);

        await Move(id, EntryIDList(id)[2], 1);
        AssertDense(id);
    }

    [Fact]
    public async Task Move_ReturnsEntriesWithTheirMediaFileMetadata()
    {
        Guid mediaFileID = MediaFile();
        _store.MediaFileList.Single(x => x.ID == mediaFileID).Metadata = new() { Title = "Hello", MediaFileID = mediaFileID };
        Guid id = Create([mediaFileID, mediaFileID]);

        ResponsePlaylistDetail moved = await Move(id, EntryIDList(id)[0], 1);

        Assert.All(moved.EntryList, entry => Assert.Equal("Hello", entry.MediaFile.Title));
    }
}
