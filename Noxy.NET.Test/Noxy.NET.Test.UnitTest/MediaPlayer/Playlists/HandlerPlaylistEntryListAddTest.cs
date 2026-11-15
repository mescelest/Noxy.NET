using Microsoft.Extensions.Options;
using Noxy.NET.MediaPlayer.Application.Configurations;
using Noxy.NET.MediaPlayer.Application.Handlers.Playlist;
using Noxy.NET.MediaPlayer.Application.Notifications.Playlist;
using Noxy.NET.MediaPlayer.Domain.Entities;
using Noxy.NET.MediaPlayer.Domain.Exceptions;
using Noxy.NET.MediaPlayer.Domain.Responses.Playlist;
using Noxy.NET.Test.MediaPlayer.Fakes;

namespace Noxy.NET.Test.MediaPlayer.Playlists;

public class HandlerPlaylistEntryListAddTest
{
    private static readonly Guid UserID = Guid.CreateVersion7();

    private readonly FakeStore _store = new();
    private readonly FakePublisher _publisher = new();
    private readonly FakeTimeProvider _time = new();
    private readonly ConfigPlaylist _config = new();
    private readonly HandlerPlaylistEntryListAdd _handler;

    public HandlerPlaylistEntryListAddTest()
    {
        _handler = new(new FakeUnitOfWorkFactory(_store), Options.Create(_config), _publisher, _time);
    }

    private static CancellationToken Token => TestContext.Current.CancellationToken;

    private Guid MediaFile(Guid? userID = null)
    {
        MediaFile file = new()
        {
            FilePath = $"{Guid.CreateVersion7()}.mp3",
            ContentType = "audio/mpeg",
            SizeBytes = 3,
            ContentHash = new string('a', 64),
            UserID = userID ?? UserID
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

    private Guid Create(bool allowDuplicates = true)
    {
        DateTime now = _time.Now.UtcDateTime;
        Playlist playlist = new()
        {
            UserID = UserID,
            Name = "Mix",
            AllowDuplicates = allowDuplicates,
            TimeCreated = now,
            TimeModified = now
        };

        _store.PlaylistList.Add(playlist);
        return playlist.ID;
    }

    private async Task<ResponsePlaylistDetail> Add(Guid playlistID, IEnumerable<Guid> mediaFileList, int? position = null, Guid? userID = null)
    {
        return await _handler.Handle(new(userID ?? UserID, playlistID, [.. mediaFileList], position), Token);
    }

    private Playlist Stored(Guid playlistID)
    {
        return _store.PlaylistList.Single(x => x.ID == playlistID);
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
    public async Task Add_WhenTheCommitFails_PublishesNothing()
    {
        Guid id = Create();
        List<Guid> mediaFileList = MediaFileList(1);
        _store.FailCommit = true;

        await Assert.ThrowsAsync<InvalidOperationException>(() => Add(id, mediaFileList));

        Assert.Empty(_publisher.PublishedList);
    }

    [Fact]
    public async Task Add_PublishesOnlyAfterTheCommit()
    {
        Guid id = Create();
        List<Guid> mediaFileList = MediaFileList(3);
        List<int> commitsAtPublishList = [];
        _publisher.OnPublish = _ => commitsAtPublishList.Add(_store.CommitCount);
        int commits = _store.CommitCount;

        await Add(id, mediaFileList);

        Assert.Equal([commits + 1], commitsAtPublishList);
    }

    [Fact]
    public async Task Add_PassesTheCancellationTokenToThePublish()
    {
        Guid id = Create();
        List<Guid> mediaFileList = MediaFileList(1);
        using CancellationTokenSource source = new();

        await _handler.Handle(new(UserID, id, mediaFileList, null), source.Token);

        Assert.Equal(source.Token, Assert.Single(_publisher.TokenList));
    }

    [Fact]
    public async Task Add_AppendsInOrder_WithDensePositions()
    {
        Guid id = Create();
        List<Guid> mediaFileList = MediaFileList(3);

        await Add(id, mediaFileList.Take(2));
        ResponsePlaylistDetail detail = await Add(id, mediaFileList.Skip(2));

        Assert.Equal(mediaFileList, detail.EntryList.Select(x => x.MediaFileID));
        Assert.Equal([0, 1, 2], detail.EntryList.Select(x => x.Position));
        Assert.Equal(3, detail.EntryCount);
        Assert.Equal(mediaFileList, StoredMediaFileOrder(id));
        AssertDense(id);
    }

    [Fact]
    public async Task Add_AtAPosition_ShiftsTheOthers()
    {
        Guid id = Create();
        List<Guid> mediaFileList = MediaFileList(3);
        Guid inserted = MediaFile();
        await Add(id, mediaFileList);

        ResponsePlaylistDetail detail = await Add(id, [inserted], 1);

        Assert.Equal([mediaFileList[0], inserted, mediaFileList[1], mediaFileList[2]], detail.EntryList.Select(x => x.MediaFileID));
        Assert.Equal([mediaFileList[0], inserted, mediaFileList[1], mediaFileList[2]], StoredMediaFileOrder(id));
        AssertDense(id);
    }

    [Fact]
    public async Task Add_SeveralAtAPosition_KeepTheirOrder()
    {
        Guid id = Create();
        List<Guid> mediaFileList = MediaFileList(2);
        List<Guid> inserted = MediaFileList(2);
        await Add(id, mediaFileList);

        await Add(id, inserted, 0);

        Assert.Equal([inserted[0], inserted[1], mediaFileList[0], mediaFileList[1]], StoredMediaFileOrder(id));
        AssertDense(id);
    }

    [Fact]
    public async Task Add_PositionEqualToCount_AppendsAndGreaterThrows()
    {
        Guid id = Create();
        List<Guid> mediaFileList = MediaFileList(2);
        await Add(id, mediaFileList);

        Guid appended = MediaFile();
        await Add(id, [appended], 2);

        Assert.Equal(appended, StoredMediaFileOrder(id)[^1]);
        await Assert.ThrowsAsync<InvalidPlaylistPositionException>(() => Add(id, [MediaFile()], 4));
        Assert.Equal(3, Stored(id).EntryList.Count);
    }

    [Fact]
    public async Task Add_SameMediaFileTwice_AllowedWhenDuplicatesAreOn()
    {
        Guid id = Create();
        Guid mediaFile = MediaFile();

        await Add(id, [mediaFile, mediaFile]);
        await Add(id, [mediaFile]);

        Assert.Equal([mediaFile, mediaFile, mediaFile], StoredMediaFileOrder(id));
        AssertDense(id);
    }

    [Fact]
    public async Task Add_DuplicatesOff_SkipsMediaFilesAlreadyInAndRepeatsInTheRequest()
    {
        Guid id = Create(allowDuplicates: false);
        List<Guid> mediaFileList = MediaFileList(3);
        await Add(id, [mediaFileList[0]]);

        ResponsePlaylistDetail detail = await Add(id, [mediaFileList[0], mediaFileList[1], mediaFileList[1], mediaFileList[2]]);

        Assert.Equal(mediaFileList, detail.EntryList.Select(x => x.MediaFileID));
        Assert.Equal(mediaFileList, StoredMediaFileOrder(id));
        AssertDense(id);
    }

    [Fact]
    public async Task Add_AllSkipped_IsANoOp()
    {
        Guid id = Create(allowDuplicates: false);
        Guid mediaFile = MediaFile();
        await Add(id, [mediaFile]);
        DateTime modified = Stored(id).TimeModified;
        int commits = _store.CommitCount;
        int pushes = _publisher.GetList<NotificationPlaylistUpdated>().Count;
        Advance();

        ResponsePlaylistDetail detail = await Add(id, [mediaFile]);

        Assert.Single(detail.EntryList);
        Assert.Equal(commits, _store.CommitCount);
        Assert.Equal(pushes, _publisher.GetList<NotificationPlaylistUpdated>().Count);
        Assert.Equal(modified, Stored(id).TimeModified);
    }

    [Fact]
    public async Task Add_UnknownOrOtherUsersMediaFile_ThrowsNotFound_AndAddsNothing()
    {
        Guid id = Create();
        Guid mine = MediaFile();
        Guid theirs = MediaFile(Guid.CreateVersion7());

        await Assert.ThrowsAsync<MediaNotFoundException>(() => Add(id, [mine, theirs]));
        await Assert.ThrowsAsync<MediaNotFoundException>(() => Add(id, [mine, Guid.CreateVersion7()]));

        Assert.Empty(Stored(id).EntryList);
    }

    [Fact]
    public async Task Add_PastTheEntryCap_Throws_AndAddsNothing()
    {
        _config.MaxEntriesPerPlaylist = 2;
        Guid id = Create();
        Guid mediaFile = MediaFile();

        await Assert.ThrowsAsync<PlaylistEntryLimitReachedException>(() => Add(id, [mediaFile, mediaFile, mediaFile]));
        Assert.Empty(Stored(id).EntryList);

        await Add(id, [mediaFile, mediaFile]);
        await Assert.ThrowsAsync<PlaylistEntryLimitReachedException>(() => Add(id, [mediaFile]));
        Assert.Equal(2, Stored(id).EntryList.Count);
    }

    [Fact]
    public async Task Add_SkippedDuplicates_DoNotCountTowardTheEntryCap()
    {
        _config.MaxEntriesPerPlaylist = 2;
        Guid id = Create(allowDuplicates: false);
        List<Guid> mediaFileList = MediaFileList(3);
        await Add(id, mediaFileList.Take(2));

        await Add(id, mediaFileList.Take(2));

        await Assert.ThrowsAsync<PlaylistEntryLimitReachedException>(() => Add(id, [mediaFileList[2]]));
        Assert.Equal(2, Stored(id).EntryList.Count);
    }

    [Fact]
    public async Task Add_BumpsAndPushesOnce_WithTheNewEntryCount()
    {
        Guid id = Create();
        List<Guid> mediaFileList = MediaFileList(3);
        int pushes = _publisher.GetList<NotificationPlaylistUpdated>().Count;
        Advance();

        await Add(id, mediaFileList);

        Assert.Equal(_time.Now.UtcDateTime, Stored(id).TimeModified);
        Assert.Equal(pushes + 1, _publisher.GetList<NotificationPlaylistUpdated>().Count);
        Assert.Equal(3, _publisher.GetList<NotificationPlaylistUpdated>()[^1].Playlist.EntryCount);
    }

    [Fact]
    public async Task Add_ToAnotherUsersPlaylist_ThrowsNotFound()
    {
        Guid id = Create();
        Guid stranger = Guid.CreateVersion7();
        Guid mediaFile = MediaFile(stranger);

        await Assert.ThrowsAsync<PlaylistNotFoundException>(() => Add(id, [mediaFile], userID: stranger));
    }

    [Fact]
    public async Task Add_ReturnsEntriesWithTheirMediaFileMetadata()
    {
        Guid mediaFileID = MediaFile();
        _store.MediaFileList.Single(x => x.ID == mediaFileID).Metadata = new() { Title = "Hello", MediaFileID = mediaFileID };
        Guid id = Create();

        ResponsePlaylistDetail added = await Add(id, [mediaFileID]);
        ResponsePlaylistDetail twice = await Add(id, [mediaFileID]);

        Assert.Equal("Hello", added.EntryList.Single().MediaFile.Title);
        Assert.All(twice.EntryList, entry => Assert.Equal("Hello", entry.MediaFile.Title));
    }
}
