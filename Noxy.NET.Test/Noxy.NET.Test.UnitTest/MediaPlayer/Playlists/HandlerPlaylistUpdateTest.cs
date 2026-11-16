using Noxy.NET.MediaPlayer.Application.Handlers.Playlist;
using Noxy.NET.MediaPlayer.Application.Notifications.Playlist;
using Noxy.NET.MediaPlayer.Domain.Entities;
using Noxy.NET.MediaPlayer.Domain.Enums;
using Noxy.NET.MediaPlayer.Domain.Exceptions;
using Noxy.NET.MediaPlayer.Domain.Responses.Playlist;
using Noxy.NET.Test.MediaPlayer.Fakes;

namespace Noxy.NET.Test.MediaPlayer.Playlists;

public class HandlerPlaylistUpdateTest
{
    private static readonly Guid UserID = Guid.CreateVersion7();

    private readonly FakeStore _store = new();
    private readonly FakePublisher _publisher = new();
    private readonly FakeTimeProvider _time = new();
    private readonly HandlerPlaylistUpdate _handler;

    public HandlerPlaylistUpdateTest()
    {
        _handler = new(new FakeUnitOfWorkFactory(_store), _publisher, _time);
    }

    private static CancellationToken Token => TestContext.Current.CancellationToken;

    private void Advance()
    {
        _time.Now = _time.Now.AddSeconds(1);
    }

    private Guid Create(string name = "Mix", bool allowDuplicates = true, IEnumerable<Guid>? mediaFileIDList = null)
    {
        DateTime now = _time.Now.UtcDateTime;
        EntityPlaylist playlist = new()
        {
            UserID = UserID,
            Name = name,
            AllowDuplicates = allowDuplicates,
            TimeCreated = now,
            TimeModified = now
        };

        foreach (Guid mediaFileID in mediaFileIDList ?? [])
        {
            playlist.EntryList.Add(new() { PlaylistID = playlist.ID, MediaFileID = mediaFileID, Position = playlist.EntryList.Count });
        }

        _store.PlaylistList.Add(playlist);
        return playlist.ID;
    }

    private List<Guid> MediaFileIDList(int count)
    {
        return [.. Enumerable.Range(0, count).Select(_ => MediaFile())];
    }

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

    private EntityPlaylist Stored(Guid playlistID)
    {
        return _store.PlaylistList.Single(x => x.ID == playlistID);
    }

    [Fact]
    public async Task Update_WhenTheCommitFails_PublishesNothing()
    {
        Guid id = Create();
        _store.FailCommit = true;

        await Assert.ThrowsAsync<InvalidOperationException>(() => _handler.Handle(new(UserID, id, new("Renamed")), Token).AsTask());

        Assert.Empty(_publisher.PublishedList);
    }

    [Fact]
    public async Task Update_PublishesOnlyAfterTheCommit()
    {
        Guid id = Create();
        List<int> commitsAtPublishList = [];
        _publisher.OnPublish = _ => commitsAtPublishList.Add(_store.CommitCount);
        int commits = _store.CommitCount;

        await _handler.Handle(new(UserID, id, new("Renamed")), Token);

        Assert.Equal([commits + 1], commitsAtPublishList);
    }

    [Fact]
    public async Task Update_PassesTheCancellationTokenToThePublish()
    {
        Guid id = Create();
        using CancellationTokenSource source = new();

        await _handler.Handle(new(UserID, id, new("Renamed")), source.Token);

        Assert.Equal(source.Token, Assert.Single(_publisher.TokenList));
    }

    [Fact]
    public async Task Update_ChangesOnlySentFields()
    {
        Guid id = Create("Before");

        ResponsePlaylist renamed = await _handler.Handle(new(UserID, id, new("  After ")), Token);

        Assert.Equal("After", renamed.Name);
        Assert.True(renamed.AllowDuplicates);

        ResponsePlaylist flagged = await _handler.Handle(new(UserID, id, AllowDuplicates: new(false)), Token);

        Assert.Equal("After", flagged.Name);
        Assert.False(flagged.AllowDuplicates);
    }

    [Fact]
    public async Task Update_NoChange_DoesNotCommitBumpOrPush()
    {
        Guid id = Create();
        DateTime modified = Stored(id).TimeModified;
        int commits = _store.CommitCount;
        Advance();

        await _handler.Handle(new(UserID, id, new(" Mix "), new(true)), Token);
        await _handler.Handle(new(UserID, id), Token);

        Assert.Equal(commits, _store.CommitCount);
        Assert.Empty(_publisher.GetList<NotificationPlaylistUpdated>());
        Assert.Equal(modified, Stored(id).TimeModified);
    }

    [Fact]
    public async Task Update_RenameBumpsTimeModifiedAndPushes()
    {
        Guid id = Create();
        Advance();

        await _handler.Handle(new(UserID, id, new("Renamed")), Token);

        Assert.Equal(_time.Now.UtcDateTime, Stored(id).TimeModified);
        NotificationPlaylistUpdated pushed = Assert.Single(_publisher.GetList<NotificationPlaylistUpdated>());
        Assert.Equal("Renamed", pushed.Playlist.Name);
    }

    [Fact]
    public async Task Update_APushCarriesTheEntryCountButNoEntries()
    {
        Guid id = Create(mediaFileIDList: MediaFileIDList(2));

        await _handler.Handle(new(UserID, id, new("Renamed")), Token);

        ResponsePlaylist pushed = Assert.Single(_publisher.GetList<NotificationPlaylistUpdated>()).Playlist;
        Assert.Equal(2, pushed.EntryCount);
        Assert.IsNotType<ResponsePlaylistDetail>(pushed);
    }

    [Fact]
    public async Task Update_ABlankName_Throws()
    {
        Guid id = Create();

        await Assert.ThrowsAsync<ArgumentException>(() => _handler.Handle(new(UserID, id, new("   ")), Token).AsTask());
    }

    [Fact]
    public async Task Update_DuplicatesOffWithDuplicates_Throws_AndChangesNothing()
    {
        Guid mediaFile = MediaFile();
        Guid id = Create(mediaFileIDList: [mediaFile, mediaFile]);

        await Assert.ThrowsAsync<PlaylistHasDuplicatesException>(() => _handler.Handle(new(UserID, id, new("Renamed"), new(false)), Token).AsTask());

        Assert.True(Stored(id).AllowDuplicates);
        Assert.Equal("Mix", Stored(id).Name);
    }

    [Fact]
    public async Task Update_DuplicatesOffWithoutDuplicates_Works()
    {
        Guid id = Create(mediaFileIDList: MediaFileIDList(3));

        ResponsePlaylist response = await _handler.Handle(new(UserID, id, AllowDuplicates: new(false)), Token);

        Assert.False(response.AllowDuplicates);
        Assert.False(Stored(id).AllowDuplicates);
    }

    [Fact]
    public async Task Update_DuplicatesOnAlwaysWorks()
    {
        Guid id = Create(allowDuplicates: false);

        ResponsePlaylist response = await _handler.Handle(new(UserID, id, AllowDuplicates: new(true)), Token);

        Assert.True(response.AllowDuplicates);
    }

    [Fact]
    public async Task Update_AnotherUsersPlaylist_ThrowsNotFound()
    {
        Guid id = Create();

        await Assert.ThrowsAsync<PlaylistNotFoundException>(() => _handler.Handle(new(Guid.CreateVersion7(), id, new("Stolen")), Token).AsTask());
    }

    #region -- Modes --

    [Fact]
    public async Task Update_SetsTheShuffleMode_AndBumpsTheModifiedTime_AndPublishes()
    {
        Guid id = Create();
        Advance();

        ResponsePlaylist response = await _handler.Handle(new(UserID, id, Shuffle: new(ShuffleModeEnum.Random)), Token);

        Assert.Equal(ShuffleModeEnum.Random, response.Shuffle);
        Assert.Equal(ShuffleModeEnum.Random, Stored(id).Shuffle);
        Assert.Equal(_time.Now.UtcDateTime, Stored(id).TimeModified);
        Assert.Equal(ShuffleModeEnum.Random, Assert.Single(_publisher.GetList<NotificationPlaylistUpdated>()).Playlist.Shuffle);
    }

    [Fact]
    public async Task Update_SetsTheRepeatMode()
    {
        Guid id = Create();

        ResponsePlaylist response = await _handler.Handle(new(UserID, id, Repeat: new(RepeatModeEnum.Forever)), Token);

        Assert.Equal(RepeatModeEnum.Forever, response.Repeat);
        Assert.Equal(RepeatModeEnum.Forever, Stored(id).Repeat);
        Assert.Null(Stored(id).Shuffle);
        Assert.Single(_publisher.GetList<NotificationPlaylistUpdated>());
    }

    [Fact]
    public async Task Update_WithTheModeItAlreadyHas_ChangesNothing_AndPublishesNothing()
    {
        Guid id = Create();
        Stored(id).Shuffle = ShuffleModeEnum.Random;
        Stored(id).Repeat = RepeatModeEnum.Once;
        DateTime modified = Stored(id).TimeModified;
        int commits = _store.CommitCount;
        Advance();

        await _handler.Handle(new(UserID, id, Shuffle: new(ShuffleModeEnum.Random), Repeat: new(RepeatModeEnum.Once)), Token);

        Assert.Equal(commits, _store.CommitCount);
        Assert.Empty(_publisher.GetList<NotificationPlaylistUpdated>());
        Assert.Equal(modified, Stored(id).TimeModified);
    }

    [Fact]
    public async Task Update_WithExplicitNull_ClearsTheMode()
    {
        Guid id = Create();
        Stored(id).Shuffle = ShuffleModeEnum.SpreadArtists;
        Stored(id).Repeat = RepeatModeEnum.Forever;

        ResponsePlaylist response = await _handler.Handle(new(UserID, id, Shuffle: new(null)), Token);

        Assert.Null(response.Shuffle);
        Assert.Null(Stored(id).Shuffle);
        Assert.Equal(RepeatModeEnum.Forever, Stored(id).Repeat);
        Assert.Single(_publisher.GetList<NotificationPlaylistUpdated>());
    }

    [Fact]
    public async Task Update_WithExplicitNullOnAModeThatIsAlreadyNull_ChangesNothing()
    {
        Guid id = Create();
        int commits = _store.CommitCount;

        await _handler.Handle(new(UserID, id, Shuffle: new(null), Repeat: new(null)), Token);

        Assert.Equal(commits, _store.CommitCount);
        Assert.Empty(_publisher.GetList<NotificationPlaylistUpdated>());
    }

    [Fact]
    public async Task Update_WithTheFieldLeftOut_DoesNotTouchTheMode()
    {
        Guid id = Create();
        Stored(id).Shuffle = ShuffleModeEnum.Random;
        Stored(id).Repeat = RepeatModeEnum.Once;

        await _handler.Handle(new(UserID, id, new("Renamed")), Token);

        Assert.Equal(ShuffleModeEnum.Random, Stored(id).Shuffle);
        Assert.Equal(RepeatModeEnum.Once, Stored(id).Repeat);
    }

    [Fact]
    public async Task Update_OfTheModeDoesNotChangeTheEntriesOrTheDuplicateRule()
    {
        List<Guid> mediaFileIDList = MediaFileIDList(2);
        Guid id = Create(allowDuplicates: false, mediaFileIDList: mediaFileIDList);

        await _handler.Handle(new(UserID, id, Shuffle: new(ShuffleModeEnum.Random), Repeat: new(RepeatModeEnum.Forever)), Token);

        Assert.Equal(mediaFileIDList, Stored(id).EntryList.OrderBy(x => x.Position).Select(x => x.MediaFileID));
        Assert.False(Stored(id).AllowDuplicates);
        Assert.Equal(2, Assert.Single(_publisher.GetList<NotificationPlaylistUpdated>()).Playlist.EntryCount);
    }

    #endregion -- Modes --
}
