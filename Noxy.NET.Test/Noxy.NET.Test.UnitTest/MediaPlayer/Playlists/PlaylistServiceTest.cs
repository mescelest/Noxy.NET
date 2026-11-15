using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Noxy.NET.MediaPlayer.Application.Configurations;
using Noxy.NET.MediaPlayer.Application.Models;
using Noxy.NET.MediaPlayer.Application.Services;
using Noxy.NET.MediaPlayer.Domain.Entities;
using Noxy.NET.MediaPlayer.Domain.Exceptions;
using Noxy.NET.MediaPlayer.Domain.Responses.Playlist;
using Noxy.NET.Test.MediaPlayer.Fakes;

namespace Noxy.NET.Test.MediaPlayer.Playlists;

public class PlaylistServiceTest
{
    private static readonly Guid UserID = Guid.CreateVersion7();

    private readonly FakeStore _store = new();
    private readonly FakePlaylistNotifier _notifier = new();
    private readonly FakeTimeProvider _time = new();
    private readonly ConfigPlaylist _config = new();
    private readonly PlaylistService _service;

    public PlaylistServiceTest()
    {
        _service = new(new FakeUnitOfWorkFactory(_store), Options.Create(_config), _notifier, new ETagService(), _time, NullLogger<PlaylistService>.Instance);
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

    private async Task<Guid> Create(string name = "Mix", bool allowDuplicates = true, Guid? userID = null)
    {
        ResponsePlaylist created = await _service.Create(userID ?? UserID, new() { Name = name, AllowDuplicates = allowDuplicates }, Token);
        return created.ID;
    }

    private Task<ResponsePlaylistDetail> Add(Guid playlistID, IEnumerable<Guid> mediaFileList, int? position = null, Guid? userID = null)
    {
        return _service.AddEntryList(userID ?? UserID, playlistID, new() { MediaFileIDList = [.. mediaFileList], Position = position }, Token);
    }

    private Task<ResponsePlaylistDetail> Remove(Guid playlistID, params Guid[] entryIDList)
    {
        return _service.RemoveEntryList(UserID, playlistID, new() { EntryIDList = [.. entryIDList] }, Token);
    }

    private Task<ResponsePlaylistDetail> Move(Guid playlistID, Guid entryID, int position)
    {
        return _service.MoveEntry(UserID, playlistID, entryID, new() { Position = position }, Token);
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

    private Task<PlaylistLibrary> Library(string? ifNoneMatch = null, Guid? userID = null)
    {
        return _service.GetPlaylistLibrary(userID ?? UserID, ifNoneMatch, Token);
    }

    #region -- Create --

    [Fact]
    public async Task Create_TrimsTheNameAndDefaultsDuplicatesToAllowed()
    {
        ResponsePlaylist created = await _service.Create(UserID, new() { Name = "  Road trip  " }, Token);

        Assert.Equal("Road trip", created.Name);
        Assert.True(created.AllowDuplicates);
        Assert.Equal(0, created.EntryCount);
        Assert.Equal(DateTimeKind.Utc, created.TimeModified.Kind);
        Assert.Equal("Road trip", Stored(created.ID).Name);
    }

    [Fact]
    public async Task Create_AtTheLimit_Throws_AndThe100thWorks()
    {
        for (int index = 0; index < 100; index++)
        {
            await Create($"Playlist {index}");
        }

        await Assert.ThrowsAsync<PlaylistLimitReachedException>(() => Create("One too many"));
        Assert.Equal(100, _store.PlaylistList.Count);
    }

    [Fact]
    public async Task Create_TheLimitIsPerUser()
    {
        _config.MaxPlaylistsPerUser = 1;
        await Create();

        await Create(userID: Guid.CreateVersion7());
        await Assert.ThrowsAsync<PlaylistLimitReachedException>(() => Create());
    }

    [Fact]
    public async Task Create_SameNameTwice_IsAllowed()
    {
        await Create("Same");
        await Create("Same");

        Assert.Equal(2, _store.PlaylistList.Count);
    }

    [Fact]
    public async Task Create_PushesTheNewPlaylist()
    {
        Guid id = await Create();

        (Guid userID, ResponsePlaylist pushed) = Assert.Single(_notifier.UpdatedList);
        Assert.Equal(UserID, userID);
        Assert.Equal(id, pushed.ID);
    }

    [Fact]
    public async Task Create_ABlankName_Throws()
    {
        await Assert.ThrowsAsync<ArgumentException>(() => Create("   "));
    }

    #endregion -- Create --

    #region -- Update --

    [Fact]
    public async Task Update_ChangesOnlySentFields()
    {
        Guid id = await Create("Before");

        ResponsePlaylist renamed = await _service.Update(UserID, id, new() { Name = new("  After ") }, Token);

        Assert.Equal("After", renamed.Name);
        Assert.True(renamed.AllowDuplicates);

        ResponsePlaylist flagged = await _service.Update(UserID, id, new() { AllowDuplicates = new(false) }, Token);

        Assert.Equal("After", flagged.Name);
        Assert.False(flagged.AllowDuplicates);
    }

    [Fact]
    public async Task Update_NoChange_DoesNotCommitBumpOrPush()
    {
        Guid id = await Create();
        DateTime modified = Stored(id).TimeModified;
        int commits = _store.CommitCount;
        int pushes = _notifier.UpdatedList.Count;
        Advance();

        await _service.Update(UserID, id, new() { Name = new(" Mix "), AllowDuplicates = new(true) }, Token);
        await _service.Update(UserID, id, new(), Token);

        Assert.Equal(commits, _store.CommitCount);
        Assert.Equal(pushes, _notifier.UpdatedList.Count);
        Assert.Equal(modified, Stored(id).TimeModified);
    }

    [Fact]
    public async Task Update_RenameBumpsTimeModifiedAndPushes()
    {
        Guid id = await Create();
        Advance();

        await _service.Update(UserID, id, new() { Name = new("Renamed") }, Token);

        Assert.Equal(_time.Now.UtcDateTime, Stored(id).TimeModified);
        Assert.Equal("Renamed", _notifier.UpdatedList[^1].Playlist.Name);
        Assert.Equal(2, _notifier.UpdatedList.Count);
    }

    [Fact]
    public async Task Update_APushCarriesTheEntryCountButNoEntries()
    {
        Guid id = await Create();
        await Add(id, MediaFileList(2));

        await _service.Update(UserID, id, new() { Name = new("Renamed") }, Token);

        ResponsePlaylist pushed = _notifier.UpdatedList[^1].Playlist;
        Assert.Equal(2, pushed.EntryCount);
        Assert.IsNotType<ResponsePlaylistDetail>(pushed);
    }

    [Fact]
    public async Task Update_DuplicatesOffWithDuplicates_Throws_AndChangesNothing()
    {
        Guid id = await Create();
        Guid mediaFile = MediaFile();
        await Add(id, [mediaFile, mediaFile]);

        await Assert.ThrowsAsync<PlaylistHasDuplicatesException>(() => _service.Update(UserID, id, new() { AllowDuplicates = new(false), Name = new("Renamed") }, Token));

        Assert.True(Stored(id).AllowDuplicates);
        Assert.Equal("Mix", Stored(id).Name);
    }

    [Fact]
    public async Task Update_DuplicatesOffWithoutDuplicates_Works()
    {
        Guid id = await Create();
        await Add(id, MediaFileList(3));

        ResponsePlaylist response = await _service.Update(UserID, id, new() { AllowDuplicates = new(false) }, Token);

        Assert.False(response.AllowDuplicates);
        Assert.False(Stored(id).AllowDuplicates);
    }

    [Fact]
    public async Task Update_DuplicatesOnAlwaysWorks()
    {
        Guid id = await Create(allowDuplicates: false);

        ResponsePlaylist response = await _service.Update(UserID, id, new() { AllowDuplicates = new(true) }, Token);

        Assert.True(response.AllowDuplicates);
    }

    [Fact]
    public async Task Update_AnotherUsersPlaylist_ThrowsNotFound()
    {
        Guid id = await Create();

        await Assert.ThrowsAsync<PlaylistNotFoundException>(() => _service.Update(Guid.CreateVersion7(), id, new() { Name = new("Stolen") }, Token));
    }

    #endregion -- Update --

    #region -- Delete --

    [Fact]
    public async Task Delete_RemovesThePlaylistAndPushesDeleted()
    {
        Guid id = await Create();

        await _service.Delete(UserID, id, Token);

        Assert.Empty(_store.PlaylistList);
        Assert.Equal((UserID, id), Assert.Single(_notifier.DeletedList));
    }

    [Fact]
    public async Task Delete_KeepsTheMediaFiles()
    {
        Guid id = await Create();
        await Add(id, MediaFileList(2));

        await _service.Delete(UserID, id, Token);

        Assert.Equal(2, _store.MediaFileList.Count);
    }

    [Fact]
    public async Task Delete_AnotherUsersPlaylist_ThrowsNotFound()
    {
        Guid id = await Create();

        await Assert.ThrowsAsync<PlaylistNotFoundException>(() => _service.Delete(Guid.CreateVersion7(), id, Token));
        Assert.Single(_store.PlaylistList);
    }

    #endregion -- Delete --

    #region -- Add --

    [Fact]
    public async Task Add_AppendsInOrder_WithDensePositions()
    {
        Guid id = await Create();
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
        Guid id = await Create();
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
        Guid id = await Create();
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
        Guid id = await Create();
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
        Guid id = await Create();
        Guid mediaFile = MediaFile();

        await Add(id, [mediaFile, mediaFile]);
        await Add(id, [mediaFile]);

        Assert.Equal([mediaFile, mediaFile, mediaFile], StoredMediaFileOrder(id));
        AssertDense(id);
    }

    [Fact]
    public async Task Add_DuplicatesOff_SkipsMediaFilesAlreadyInAndRepeatsInTheRequest()
    {
        Guid id = await Create(allowDuplicates: false);
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
        Guid id = await Create(allowDuplicates: false);
        Guid mediaFile = MediaFile();
        await Add(id, [mediaFile]);
        DateTime modified = Stored(id).TimeModified;
        int commits = _store.CommitCount;
        int pushes = _notifier.UpdatedList.Count;
        Advance();

        ResponsePlaylistDetail detail = await Add(id, [mediaFile]);

        Assert.Single(detail.EntryList);
        Assert.Equal(commits, _store.CommitCount);
        Assert.Equal(pushes, _notifier.UpdatedList.Count);
        Assert.Equal(modified, Stored(id).TimeModified);
    }

    [Fact]
    public async Task Add_UnknownOrOtherUsersMediaFile_ThrowsNotFound_AndAddsNothing()
    {
        Guid id = await Create();
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
        Guid id = await Create();
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
        Guid id = await Create(allowDuplicates: false);
        List<Guid> mediaFileList = MediaFileList(3);
        await Add(id, mediaFileList.Take(2));

        await Add(id, mediaFileList.Take(2));

        await Assert.ThrowsAsync<PlaylistEntryLimitReachedException>(() => Add(id, [mediaFileList[2]]));
        Assert.Equal(2, Stored(id).EntryList.Count);
    }

    [Fact]
    public async Task Add_BumpsAndPushesOnce_WithTheNewEntryCount()
    {
        Guid id = await Create();
        int pushes = _notifier.UpdatedList.Count;
        Advance();

        await Add(id, MediaFileList(3));

        Assert.Equal(_time.Now.UtcDateTime, Stored(id).TimeModified);
        Assert.Equal(pushes + 1, _notifier.UpdatedList.Count);
        Assert.Equal(3, _notifier.UpdatedList[^1].Playlist.EntryCount);
    }

    [Fact]
    public async Task Add_ToAnotherUsersPlaylist_ThrowsNotFound()
    {
        Guid id = await Create();
        Guid stranger = Guid.CreateVersion7();
        Guid mediaFile = MediaFile(stranger);

        await Assert.ThrowsAsync<PlaylistNotFoundException>(() => Add(id, [mediaFile], userID: stranger));
    }

    #endregion -- Add --

    #region -- Remove --

    [Fact]
    public async Task Remove_RemovesAndRenumbers()
    {
        Guid id = await Create();
        List<Guid> mediaFileList = MediaFileList(3);
        ResponsePlaylistDetail detail = await Add(id, mediaFileList);

        ResponsePlaylistDetail after = await Remove(id, detail.EntryList[0].ID);

        Assert.Equal([mediaFileList[1], mediaFileList[2]], after.EntryList.Select(x => x.MediaFileID));
        Assert.Equal([0, 1], after.EntryList.Select(x => x.Position));
        Assert.Equal([mediaFileList[1], mediaFileList[2]], StoredMediaFileOrder(id));
        AssertDense(id);
    }

    [Fact]
    public async Task Remove_SeveralAtOnce_LeavesDensePositions()
    {
        Guid id = await Create();
        List<Guid> mediaFileList = MediaFileList(5);
        ResponsePlaylistDetail detail = await Add(id, mediaFileList);

        await Remove(id, detail.EntryList[1].ID, detail.EntryList[3].ID, detail.EntryList[1].ID);

        Assert.Equal([mediaFileList[0], mediaFileList[2], mediaFileList[4]], StoredMediaFileOrder(id));
        AssertDense(id);
    }

    [Fact]
    public async Task Remove_UnknownEntry_ThrowsAndRemovesNothing()
    {
        Guid id = await Create();
        ResponsePlaylistDetail detail = await Add(id, MediaFileList(2));

        await Assert.ThrowsAsync<PlaylistEntryNotFoundException>(() => Remove(id, detail.EntryList[0].ID, Guid.CreateVersion7()));

        Assert.Equal(2, Stored(id).EntryList.Count);
    }

    [Fact]
    public async Task Remove_AnEntryOfAnotherPlaylist_ThrowsNotFound()
    {
        Guid id = await Create();
        Guid other = await Create("Other");
        ResponsePlaylistDetail otherDetail = await Add(other, MediaFileList(1));
        await Add(id, MediaFileList(1));

        await Assert.ThrowsAsync<PlaylistEntryNotFoundException>(() => Remove(id, otherDetail.EntryList[0].ID));
        Assert.Single(Stored(other).EntryList);
    }

    [Fact]
    public async Task Remove_BumpsAndPushes()
    {
        Guid id = await Create();
        ResponsePlaylistDetail detail = await Add(id, MediaFileList(2));
        int pushes = _notifier.UpdatedList.Count;
        Advance();

        await Remove(id, detail.EntryList[0].ID);

        Assert.Equal(_time.Now.UtcDateTime, Stored(id).TimeModified);
        Assert.Equal(pushes + 1, _notifier.UpdatedList.Count);
        Assert.Equal(1, _notifier.UpdatedList[^1].Playlist.EntryCount);
    }

    #endregion -- Remove --

    #region -- Move --

    [Fact]
    public async Task Move_Down_ShiftsTheOthersUp()
    {
        Guid id = await Create();
        List<Guid> mediaFileList = MediaFileList(4);
        ResponsePlaylistDetail detail = await Add(id, mediaFileList);

        ResponsePlaylistDetail after = await Move(id, detail.EntryList[0].ID, 2);

        Assert.Equal([mediaFileList[1], mediaFileList[2], mediaFileList[0], mediaFileList[3]], after.EntryList.Select(x => x.MediaFileID));
        Assert.Equal([mediaFileList[1], mediaFileList[2], mediaFileList[0], mediaFileList[3]], StoredMediaFileOrder(id));
        AssertDense(id);
    }

    [Fact]
    public async Task Move_Up_ShiftsTheOthersDown()
    {
        Guid id = await Create();
        List<Guid> mediaFileList = MediaFileList(4);
        ResponsePlaylistDetail detail = await Add(id, mediaFileList);

        await Move(id, detail.EntryList[3].ID, 1);

        Assert.Equal([mediaFileList[0], mediaFileList[3], mediaFileList[1], mediaFileList[2]], StoredMediaFileOrder(id));
        AssertDense(id);
    }

    [Fact]
    public async Task Move_ToTheSamePosition_IsANoOp()
    {
        Guid id = await Create();
        ResponsePlaylistDetail detail = await Add(id, MediaFileList(3));
        DateTime modified = Stored(id).TimeModified;
        int commits = _store.CommitCount;
        int pushes = _notifier.UpdatedList.Count;
        Advance();

        await Move(id, detail.EntryList[1].ID, 1);

        Assert.Equal(commits, _store.CommitCount);
        Assert.Equal(pushes, _notifier.UpdatedList.Count);
        Assert.Equal(modified, Stored(id).TimeModified);
    }

    [Fact]
    public async Task Move_OutOfRange_Throws()
    {
        Guid id = await Create();
        ResponsePlaylistDetail detail = await Add(id, MediaFileList(3));

        await Assert.ThrowsAsync<InvalidPlaylistPositionException>(() => Move(id, detail.EntryList[0].ID, 3));
        await Assert.ThrowsAsync<InvalidPlaylistPositionException>(() => Move(id, detail.EntryList[0].ID, -1));
    }

    [Fact]
    public async Task Move_UnknownEntry_ThrowsNotFound()
    {
        Guid id = await Create();
        await Add(id, MediaFileList(2));

        await Assert.ThrowsAsync<PlaylistEntryNotFoundException>(() => Move(id, Guid.CreateVersion7(), 0));
    }

    [Fact]
    public async Task Move_ToTheLastPosition_Works()
    {
        Guid id = await Create();
        List<Guid> mediaFileList = MediaFileList(3);
        ResponsePlaylistDetail detail = await Add(id, mediaFileList);

        await Move(id, detail.EntryList[0].ID, 2);

        Assert.Equal([mediaFileList[1], mediaFileList[2], mediaFileList[0]], StoredMediaFileOrder(id));
    }

    [Fact]
    public async Task Positions_AreAlwaysDense_AfterAMixOfOperations()
    {
        Guid id = await Create();
        Guid repeated = MediaFile();
        ResponsePlaylistDetail detail = await Add(id, [repeated, .. MediaFileList(3), repeated]);
        AssertDense(id);

        await Move(id, detail.EntryList[4].ID, 0);
        AssertDense(id);

        detail = await Add(id, MediaFileList(2), 2);
        AssertDense(id);

        detail = await Remove(id, detail.EntryList[0].ID, detail.EntryList[3].ID, detail.EntryList[6].ID);
        AssertDense(id);

        await Move(id, detail.EntryList[0].ID, detail.EntryList.Count - 1);
        AssertDense(id);
    }

    #endregion -- Move --

    #region -- Reads --

    [Fact]
    public async Task GetPlaylistLibrary_ReturnsTheUsersPlaylistsWithEntryCountsAndNoEntries()
    {
        Guid first = await Create("First");
        Advance();
        Guid second = await Create("Second");
        await Add(first, MediaFileList(3));
        await Create("Not mine", userID: Guid.CreateVersion7());

        PlaylistLibrary library = await Library();

        Assert.Matches("^\"[0-9a-f]{64}\"$", library.ETag);
        Assert.Equal(2, library.PlaylistList!.Count);
        Assert.Equal(3, library.PlaylistList.Single(x => x.ID == first).EntryCount);
        Assert.Equal(0, library.PlaylistList.Single(x => x.ID == second).EntryCount);
        Assert.All(library.PlaylistList, x => Assert.IsNotType<ResponsePlaylistDetail>(x));
    }

    [Fact]
    public async Task GetPlaylistLibrary_IsNewestFirst()
    {
        await Create("Older");
        Advance();
        await Create("Newer");

        PlaylistLibrary library = await Library();

        Assert.Equal(["Newer", "Older"], library.PlaylistList!.Select(x => x.Name));
    }

    [Fact]
    public async Task GetPlaylistLibrary_ETagChangesAfterEveryKindOfChange()
    {
        Guid id = await Create();
        List<Guid> mediaFileList = MediaFileList(3);
        List<string> eTagList = [(await Library()).ETag];

        async Task Step(Func<Task> change)
        {
            Advance();
            await change();
            eTagList.Add((await Library()).ETag);
        }

        ResponsePlaylistDetail detail = null!;
        await Step(() => _service.Update(UserID, id, new() { Name = new("Renamed") }, Token));
        await Step(() => _service.Update(UserID, id, new() { AllowDuplicates = new(false) }, Token));
        await Step(async () => detail = await Add(id, mediaFileList));
        await Step(() => Move(id, detail.EntryList[0].ID, 2));
        await Step(() => Remove(id, detail.EntryList[1].ID));
        Guid other = Guid.Empty;
        await Step(async () => other = await Create("Second"));
        await Step(() => _service.Delete(UserID, other, Token));

        for (int index = 1; index < eTagList.Count; index++)
        {
            Assert.NotEqual(eTagList[index - 1], eTagList[index]);
        }
    }

    [Fact]
    public async Task GetPlaylistLibrary_NoOpsKeepTheETag()
    {
        Guid id = await Create(allowDuplicates: false);
        ResponsePlaylistDetail detail = await Add(id, MediaFileList(2));
        string before = (await Library()).ETag;
        Advance();

        await Move(id, detail.EntryList[0].ID, 0);
        await _service.Update(UserID, id, new() { Name = new("Mix") }, Token);
        await Add(id, [detail.EntryList[0].MediaFileID]);

        Assert.Equal(before, (await Library()).ETag);
    }

    [Fact]
    public async Task GetPlaylistLibrary_MatchingIfNoneMatch_ReturnsNoList()
    {
        await Create();
        string eTag = (await Library()).ETag;

        PlaylistLibrary matched = await Library(eTag);
        PlaylistLibrary wildcard = await Library("*");

        Assert.Equal(eTag, matched.ETag);
        Assert.Null(matched.PlaylistList);
        Assert.Null(wildcard.PlaylistList);
    }

    [Theory]
    [InlineData("\"other\"")]
    [InlineData("garbage")]
    [InlineData("")]
    [InlineData(null)]
    public async Task GetPlaylistLibrary_MismatchedIfNoneMatch_ReturnsTheList(string? ifNoneMatch)
    {
        await Create();

        PlaylistLibrary library = await Library(ifNoneMatch);

        Assert.Single(library.PlaylistList!);
    }

    [Fact]
    public async Task GetPlaylistLibrary_IsPerUser()
    {
        await Create();
        string before = (await Library()).ETag;

        await Create(userID: Guid.CreateVersion7());

        Assert.Equal(before, (await Library()).ETag);
    }

    [Fact]
    public async Task GetPlaylistLibrary_EmptyList_HasAStableETag()
    {
        PlaylistLibrary first = await Library();
        PlaylistLibrary second = await Library();

        Assert.Equal(first.ETag, second.ETag);
        Assert.Empty(first.PlaylistList!);
    }

    [Fact]
    public async Task GetPlaylist_ReturnsEntriesByPosition()
    {
        Guid id = await Create();
        List<Guid> mediaFileList = MediaFileList(3);
        await Add(id, mediaFileList);

        PlaylistView view = await _service.GetPlaylist(UserID, id, null, Token);

        Assert.Equal(mediaFileList, view.Detail!.EntryList.Select(x => x.MediaFileID));
        Assert.Equal([0, 1, 2], view.Detail.EntryList.Select(x => x.Position));
        Assert.Equal(3, view.Detail.EntryCount);
    }

    [Fact]
    public async Task GetPlaylist_MatchingIfNoneMatch_ReturnsNoDetail()
    {
        Guid id = await Create();
        string eTag = (await _service.GetPlaylist(UserID, id, null, Token)).ETag;

        PlaylistView view = await _service.GetPlaylist(UserID, id, eTag, Token);

        Assert.Equal(eTag, view.ETag);
        Assert.Null(view.Detail);
    }

    [Fact]
    public async Task GetPlaylist_ETagChangesWhenItsEntriesChange_AndNotWhenAnotherPlaylistChanges()
    {
        Guid id = await Create();
        Guid other = await Create("Other");
        string before = (await _service.GetPlaylist(UserID, id, null, Token)).ETag;
        Advance();

        await Add(other, MediaFileList(1));
        Assert.Equal(before, (await _service.GetPlaylist(UserID, id, null, Token)).ETag);

        await Add(id, MediaFileList(1));
        Assert.NotEqual(before, (await _service.GetPlaylist(UserID, id, null, Token)).ETag);
    }

    [Fact]
    public async Task GetPlaylist_AnotherUsersPlaylist_ThrowsNotFound()
    {
        Guid id = await Create();

        await Assert.ThrowsAsync<PlaylistNotFoundException>(() => _service.GetPlaylist(Guid.CreateVersion7(), id, null, Token));
    }

    [Fact]
    public async Task GetPlaylist_ReturnsEntriesWithTheirMediaFileMetadata()
    {
        Guid mediaFileID = MediaFile();
        MediaFile mediaFile = _store.MediaFileList.Single(x => x.ID == mediaFileID);
        mediaFile.Metadata = new() { Title = "Hello", Artist = "Someone", MediaFileID = mediaFileID };
        Guid id = await Create();
        await Add(id, [mediaFileID]);

        ResponsePlaylistEntry entry = Assert.Single((await _service.GetPlaylist(UserID, id, null, Token)).Detail!.EntryList);

        Assert.Equal(mediaFileID, entry.MediaFileID);
        Assert.Equal(mediaFileID, entry.MediaFile.ID);
        Assert.Equal("Hello", entry.MediaFile.Title);
        Assert.Equal("Someone", entry.MediaFile.Artist);
        Assert.Equal(mediaFile.ContentHash, entry.MediaFile.ContentHash);
    }

    [Fact]
    public async Task Mutations_ReturnEntriesWithTheirMediaFileMetadata()
    {
        Guid mediaFileID = MediaFile();
        _store.MediaFileList.Single(x => x.ID == mediaFileID).Metadata = new() { Title = "Hello", MediaFileID = mediaFileID };
        Guid id = await Create();

        ResponsePlaylistDetail added = await Add(id, [mediaFileID]);
        ResponsePlaylistDetail moved = await Move(id, added.EntryList[0].ID, 0);
        ResponsePlaylistDetail twice = await Add(id, [mediaFileID]);

        Assert.Equal("Hello", added.EntryList.Single().MediaFile.Title);
        Assert.Equal("Hello", moved.EntryList.Single().MediaFile.Title);
        Assert.All(twice.EntryList, entry => Assert.Equal("Hello", entry.MediaFile.Title));
    }

    [Fact]
    public async Task GetPlaylist_AfterAMediaFilesMetadataChanged_NoLongerMatchesTheOldETag()
    {
        Guid mediaFileID = MediaFile();
        Guid id = await Create();
        await Add(id, [mediaFileID]);
        string eTag = (await _service.GetPlaylist(UserID, id, null, Token)).ETag;
        Advance();
        _store.MediaFileList.Single(x => x.ID == mediaFileID).TimeModified = _time.Now.UtcDateTime;

        PlaylistView view = await _service.GetPlaylist(UserID, id, eTag, Token);

        Assert.NotNull(view.Detail);
        Assert.NotEqual(eTag, view.ETag);
    }

    [Fact]
    public async Task GetPlaylist_WhenNothingChanged_ReturnsNullDetail()
    {
        Guid id = await Create();
        await Add(id, MediaFileList(2));
        string eTag = (await _service.GetPlaylist(UserID, id, null, Token)).ETag;

        PlaylistView view = await _service.GetPlaylist(UserID, id, eTag, Token);

        Assert.Null(view.Detail);
        Assert.Equal(eTag, view.ETag);
    }

    [Fact]
    public async Task GetPlaylist_SameMediaFileTwice_ReturnsBothEntriesAndAnETag()
    {
        Guid mediaFileID = MediaFile();
        Guid id = await Create();
        await Add(id, [mediaFileID, mediaFileID]);

        PlaylistView view = await _service.GetPlaylist(UserID, id, null, Token);

        Assert.Equal(2, view.Detail!.EntryList.Count);
        Assert.NotEmpty(view.ETag);
        Assert.Null((await _service.GetPlaylist(UserID, id, view.ETag, Token)).Detail);
    }

    [Fact]
    public async Task GetPlaylist_EmptyPlaylist_ReturnsADetailWithAnETag()
    {
        Guid id = await Create();

        PlaylistView view = await _service.GetPlaylist(UserID, id, null, Token);

        Assert.Empty(view.Detail!.EntryList);
        Assert.NotEmpty(view.ETag);
        Assert.Null((await _service.GetPlaylist(UserID, id, view.ETag, Token)).Detail);
    }

    [Fact]
    public async Task GetPlaylistLibrary_ETagIsUnchangedByAMediaFileEdit()
    {
        Guid mediaFileID = MediaFile();
        Guid id = await Create();
        await Add(id, [mediaFileID]);
        string before = (await Library()).ETag;
        Advance();
        _store.MediaFileList.Single(x => x.ID == mediaFileID).TimeModified = _time.Now.UtcDateTime;

        Assert.Equal(before, (await Library()).ETag);
    }

    #endregion -- Reads --

    [Fact]
    public async Task AnyOperation_NotifierThrows_StillSucceeds()
    {
        _notifier.Failure = new InvalidOperationException("The connection is gone.");

        Guid id = await Create();
        await _service.Update(UserID, id, new() { Name = new("Renamed") }, Token);
        ResponsePlaylistDetail detail = await Add(id, MediaFileList(2));
        await Move(id, detail.EntryList[0].ID, 1);
        await Remove(id, detail.EntryList[0].ID);
        await _service.Delete(UserID, id, Token);

        Assert.Empty(_store.PlaylistList);
    }
}
