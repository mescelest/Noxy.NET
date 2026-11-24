using System.Net;
using Noxy.NET.MediaPlayer.Domain.Responses.Media;
using Noxy.NET.MediaPlayer.Domain.Responses.Playlist;
using Noxy.NET.MediaPlayer.Presentation.Exceptions;
using Noxy.NET.MediaPlayer.Presentation.Models;
using Noxy.NET.MediaPlayer.Presentation.Services;
using Noxy.NET.Test.MediaPlayer.Fakes;

namespace Noxy.NET.Test.MediaPlayer.Presentation;

public class SyncServiceTest
{
    private static readonly DateTime Start = new(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);

    private readonly FakeApiClient _clientApi = new();
    private readonly FakeMetadataStore _storeMetadata = new();
    private readonly SyncService _serviceSync;

    public SyncServiceTest()
    {
        _serviceSync = new(_clientApi, _storeMetadata);
    }

    private static ResponseMediaFile MediaFile(string title, Guid? id = null, string? hash = null)
    {
        return new()
        {
            ID = id ?? Guid.CreateVersion7(),
            Title = title,
            ContentType = "audio/mpeg",
            SizeBytes = 1234,
            ContentHash = hash ?? new string('a', 64),
            TimeCreated = Start,
            TimeModified = Start
        };
    }

    private static ResponsePlaylist Playlist(string name, Guid? id = null, DateTime? modified = null)
    {
        return new()
        {
            ID = id ?? Guid.CreateVersion7(),
            Name = name,
            AllowDuplicates = true,
            EntryCount = 1,
            TimeCreated = Start,
            TimeModified = modified ?? Start,
            TimeLastPlayed = null
        };
    }

    private static ResponsePlaylistDetail Detail(Guid id, DateTime? modified = null)
    {
        ResponseMediaFile media = MediaFile("Entry");
        return new()
        {
            ID = id,
            Name = "Mix",
            AllowDuplicates = true,
            EntryCount = 1,
            TimeCreated = Start,
            TimeModified = modified ?? Start,
            TimeLastPlayed = null,
            EntryList = [new() { ID = Guid.CreateVersion7(), MediaFileID = media.ID, Position = 0, MediaFile = media }]
        };
    }

    private static ApiResponse<T> Ok<T>(T value, string? eTag = null)
    {
        return new(HttpStatusCode.OK, value, eTag);
    }

    private static ApiResponse<T> NotModified<T>()
    {
        return new(HttpStatusCode.NotModified, default, null);
    }

    [Fact]
    public async Task SyncLibrary_FirstRun_SendsNoETag_StoresTheListAndTheETag()
    {
        List<ResponseMediaFile> list = [MediaFile("One"), MediaFile("Two")];
        _clientApi.LibraryHandler = _ => Ok(list, "\"v1\"");

        await _serviceSync.SyncLibrary();

        Assert.Equal([null], _clientApi.LibraryETagList);
        Assert.Equal(["One", "Two"], (await _storeMetadata.GetMediaFileList()).Select(x => x.Title).ToList());
        Assert.Equal("\"v1\"", await _storeMetadata.GetLibraryETag());
        Assert.False(_serviceSync.IsOffline);
    }

    [Fact]
    public async Task SyncLibrary_On304_ChangesNothing()
    {
        await _storeMetadata.ReplaceMediaFileList([MediaFile("One")]);
        await _storeMetadata.SetLibraryETag("\"v1\"");
        _clientApi.LibraryHandler = _ => NotModified<List<ResponseMediaFile>>();
        int changed = 0;
        _serviceSync.Changed += () => changed++;

        await _serviceSync.SyncLibrary();

        Assert.Equal(["\"v1\""], _clientApi.LibraryETagList);
        Assert.Equal(["One"], (await _storeMetadata.GetMediaFileList()).Select(x => x.Title).ToList());
        Assert.Equal("\"v1\"", await _storeMetadata.GetLibraryETag());
        Assert.Equal(0, changed);
    }

    [Fact]
    public async Task SyncLibrary_On200_ReplacesTheLocalListAndStoresTheNewETag()
    {
        ResponseMediaFile kept = MediaFile("Kept");
        await _storeMetadata.ReplaceMediaFileList([kept, MediaFile("Gone")]);
        await _storeMetadata.SetLibraryETag("\"v1\"");
        _clientApi.LibraryHandler = _ => Ok(new List<ResponseMediaFile> { kept, MediaFile("New") }, "\"v2\"");

        await _serviceSync.SyncLibrary();

        Assert.Equal(["Kept", "New"], (await _storeMetadata.GetMediaFileList()).Select(x => x.Title).ToList());
        Assert.Equal("\"v2\"", await _storeMetadata.GetLibraryETag());
    }

    [Fact]
    public async Task SyncLibrary_WhenTheLocalListIsEmptyButAnETagIsStored_SendsNoETag_AndStoresTheFullList()
    {
        await _storeMetadata.SetLibraryETag("\"v1\"");
        _clientApi.LibraryHandler = eTag => eTag is null ? Ok(new List<ResponseMediaFile> { MediaFile("One") }, "\"v1\"") : NotModified<List<ResponseMediaFile>>();

        await _serviceSync.SyncLibrary();

        Assert.Equal([null], _clientApi.LibraryETagList);
        Assert.Equal(["One"], (await _storeMetadata.GetMediaFileList()).Select(x => x.Title).ToList());
    }

    [Fact]
    public async Task SyncLibrary_Force_SendsNoETag_EvenWhenOneIsStored()
    {
        await _storeMetadata.ReplaceMediaFileList([MediaFile("One")]);
        await _storeMetadata.SetLibraryETag("\"v1\"");
        _clientApi.LibraryHandler = _ => Ok(new List<ResponseMediaFile> { MediaFile("Two") }, "\"v2\"");

        await _serviceSync.SyncLibrary(force: true);

        Assert.Equal([null], _clientApi.LibraryETagList);
        Assert.Equal(["Two"], (await _storeMetadata.GetMediaFileList()).Select(x => x.Title).ToList());
    }

    [Fact]
    public async Task SyncLibrary_WhenTheNetworkIsDown_LeavesListAndETagAlone_AndSetsOffline()
    {
        await _storeMetadata.ReplaceMediaFileList([MediaFile("One")]);
        await _storeMetadata.SetLibraryETag("\"v1\"");
        _clientApi.LibraryHandler = _ => throw new HttpRequestException("down");

        await _serviceSync.SyncLibrary();

        Assert.True(_serviceSync.IsOffline);
        Assert.Equal(["One"], (await _storeMetadata.GetMediaFileList()).Select(x => x.Title).ToList());
        Assert.Equal("\"v1\"", await _storeMetadata.GetLibraryETag());
    }

    [Fact]
    public async Task SyncLibrary_WhenTheRequestTimesOut_SetsOffline()
    {
        _clientApi.LibraryHandler = _ => throw new TaskCanceledException("timeout");

        await _serviceSync.SyncLibrary();

        Assert.True(_serviceSync.IsOffline);
    }

    [Fact]
    public async Task SyncLibrary_WhenNoServerAddressIsSaved_DoesNothing_AndSetsOffline()
    {
        await _storeMetadata.ReplaceMediaFileList([MediaFile("One")]);
        _clientApi.LibraryHandler = _ => throw new ServerAddressMissingException();

        await _serviceSync.SyncLibrary();

        Assert.True(_serviceSync.IsOffline);
        Assert.Single(await _storeMetadata.GetMediaFileList());
    }

    [Fact]
    public async Task SyncLibrary_WhenTheServerRejects_PropagatesTheApiException_AndStaysOnline()
    {
        _clientApi.LibraryHandler = _ => throw new ApiException(HttpStatusCode.InternalServerError, "Broken.");

        ApiException exception = await Assert.ThrowsAsync<ApiException>(() => _serviceSync.SyncLibrary());

        Assert.Equal(HttpStatusCode.InternalServerError, exception.Status);
        Assert.False(_serviceSync.IsOffline);
    }

    [Fact]
    public async Task SyncLibrary_AfterAFailure_ThenSuccess_ClearsOffline()
    {
        _clientApi.LibraryHandler = _ => throw new HttpRequestException("down");
        await _serviceSync.SyncLibrary();
        Assert.True(_serviceSync.IsOffline);

        _clientApi.LibraryHandler = _ => Ok(new List<ResponseMediaFile>(), "\"v1\"");
        await _serviceSync.SyncLibrary();

        Assert.False(_serviceSync.IsOffline);
    }

    [Fact]
    public async Task SyncLibrary_On304_AfterAFailure_ClearsOffline()
    {
        await _storeMetadata.ReplaceMediaFileList([MediaFile("One")]);
        await _storeMetadata.SetLibraryETag("\"v1\"");
        _clientApi.LibraryHandler = _ => throw new HttpRequestException("down");
        await _serviceSync.SyncLibrary();

        _clientApi.LibraryHandler = _ => NotModified<List<ResponseMediaFile>>();
        await _serviceSync.SyncLibrary();

        Assert.False(_serviceSync.IsOffline);
    }

    [Fact]
    public async Task SyncPlaylistList_FirstRun_SendsNoETag_StoresTheListAndTheETag()
    {
        _clientApi.PlaylistListHandler = _ => Ok(new List<ResponsePlaylist> { Playlist("A"), Playlist("B") }, "\"p1\"");

        await _serviceSync.SyncPlaylistList();

        Assert.Equal([null], _clientApi.PlaylistListETagList);
        Assert.Equal(["A", "B"], (await _storeMetadata.GetPlaylistList()).Select(x => x.Name).ToList());
        Assert.Equal("\"p1\"", await _storeMetadata.GetPlaylistListETag());
    }

    [Fact]
    public async Task SyncPlaylistList_On304_ChangesNothing()
    {
        await _storeMetadata.ReplacePlaylistList([Playlist("A")]);
        await _storeMetadata.SetPlaylistListETag("\"p1\"");
        _clientApi.PlaylistListHandler = _ => NotModified<List<ResponsePlaylist>>();
        int changed = 0;
        _serviceSync.Changed += () => changed++;

        await _serviceSync.SyncPlaylistList();

        Assert.Equal(["\"p1\""], _clientApi.PlaylistListETagList);
        Assert.Equal(["A"], (await _storeMetadata.GetPlaylistList()).Select(x => x.Name).ToList());
        Assert.Equal(0, changed);
    }

    [Fact]
    public async Task SyncPlaylistList_On200_ReplacesTheLocalListAndStoresTheNewETag()
    {
        await _storeMetadata.ReplacePlaylistList([Playlist("Gone")]);
        await _storeMetadata.SetPlaylistListETag("\"p1\"");
        _clientApi.PlaylistListHandler = _ => Ok(new List<ResponsePlaylist> { Playlist("New") }, "\"p2\"");

        await _serviceSync.SyncPlaylistList();

        Assert.Equal(["New"], (await _storeMetadata.GetPlaylistList()).Select(x => x.Name).ToList());
        Assert.Equal("\"p2\"", await _storeMetadata.GetPlaylistListETag());
    }

    [Fact]
    public async Task SyncPlaylistList_WhenTheLocalListIsEmptyButAnETagIsStored_SendsNoETag()
    {
        await _storeMetadata.SetPlaylistListETag("\"p1\"");
        _clientApi.PlaylistListHandler = eTag => eTag is null ? Ok(new List<ResponsePlaylist> { Playlist("A") }, "\"p1\"") : NotModified<List<ResponsePlaylist>>();

        await _serviceSync.SyncPlaylistList();

        Assert.Equal([null], _clientApi.PlaylistListETagList);
        Assert.Single(await _storeMetadata.GetPlaylistList());
    }

    [Fact]
    public async Task SyncPlaylistList_Force_SendsNoETag_EvenWhenOneIsStored()
    {
        await _storeMetadata.ReplacePlaylistList([Playlist("A")]);
        await _storeMetadata.SetPlaylistListETag("\"p1\"");
        _clientApi.PlaylistListHandler = _ => Ok(new List<ResponsePlaylist> { Playlist("B") }, "\"p2\"");

        await _serviceSync.SyncPlaylistList(force: true);

        Assert.Equal([null], _clientApi.PlaylistListETagList);
    }

    [Fact]
    public async Task SyncPlaylistList_WhenTheNetworkIsDown_LeavesListAndETagAlone_AndSetsOffline()
    {
        await _storeMetadata.ReplacePlaylistList([Playlist("A")]);
        await _storeMetadata.SetPlaylistListETag("\"p1\"");
        _clientApi.PlaylistListHandler = _ => throw new HttpRequestException("down");

        await _serviceSync.SyncPlaylistList();

        Assert.True(_serviceSync.IsOffline);
        Assert.Single(await _storeMetadata.GetPlaylistList());
        Assert.Equal("\"p1\"", await _storeMetadata.GetPlaylistListETag());
    }

    [Fact]
    public async Task SyncPlaylistList_WhenNoServerAddressIsSaved_DoesNothing_AndSetsOffline()
    {
        _clientApi.PlaylistListHandler = _ => throw new ServerAddressMissingException();

        await _serviceSync.SyncPlaylistList();

        Assert.True(_serviceSync.IsOffline);
    }

    [Fact]
    public async Task SyncPlaylistList_AfterAFailure_ThenSuccess_ClearsOffline()
    {
        _clientApi.PlaylistListHandler = _ => throw new HttpRequestException("down");
        await _serviceSync.SyncPlaylistList();

        _clientApi.PlaylistListHandler = _ => Ok(new List<ResponsePlaylist>(), "\"p1\"");
        await _serviceSync.SyncPlaylistList();

        Assert.False(_serviceSync.IsOffline);
    }

    [Fact]
    public async Task SyncPlaylistList_ReFetchesAHeldDetailWhoseSummaryIsNewer_AndLeavesAnUpToDateOneAlone()
    {
        Guid stale = Guid.CreateVersion7();
        Guid current = Guid.CreateVersion7();
        await _storeMetadata.SavePlaylistDetail(Detail(stale, Start));
        await _storeMetadata.SavePlaylistDetail(Detail(current, Start));
        await _storeMetadata.ReplacePlaylistList([Playlist("Stale", stale), Playlist("Current", current)]);
        await _storeMetadata.SetPlaylistListETag("\"p1\"");
        ResponsePlaylistDetail fresh = Detail(stale, Start.AddHours(1));
        _clientApi.PlaylistDetailHandler = (_, _) => Ok(fresh);
        _clientApi.PlaylistListHandler = _ => Ok(new List<ResponsePlaylist> { Playlist("Stale", stale, Start.AddHours(1)), Playlist("Current", current, Start) }, "\"p2\"");

        await _serviceSync.SyncPlaylistList();

        Assert.Equal([stale], _clientApi.PlaylistDetailRequestList.Select(x => x.ID).ToList());
        ResponsePlaylistDetail? stored = await _storeMetadata.GetPlaylistDetail(stale);
        Assert.Equal(Start.AddHours(1), stored?.TimeModified);
    }

    [Fact]
    public async Task SyncPlaylistList_DoesNotFetchADetailThatIsNotHeld()
    {
        Guid id = Guid.CreateVersion7();
        _clientApi.PlaylistListHandler = _ => Ok(new List<ResponsePlaylist> { Playlist("A", id) }, "\"p1\"");

        await _serviceSync.SyncPlaylistList();

        Assert.Empty(_clientApi.PlaylistDetailRequestList);
    }

    [Fact]
    public async Task SyncPlaylistList_DropsTheDetailOfAPlaylistThatNoLongerExists()
    {
        Guid gone = Guid.CreateVersion7();
        await _storeMetadata.SavePlaylistDetail(Detail(gone));
        await _storeMetadata.ReplacePlaylistList([Playlist("Gone", gone)]);
        await _storeMetadata.SetPlaylistListETag("\"p1\"");
        _clientApi.PlaylistListHandler = _ => Ok(new List<ResponsePlaylist>(), "\"p2\"");

        await _serviceSync.SyncPlaylistList();

        Assert.Null(await _storeMetadata.GetPlaylistDetail(gone));
        Assert.Empty(await _storeMetadata.GetHeldPlaylistIDList());
    }

    [Fact]
    public async Task SyncPlaylistList_WhenTheDetailRefetchFindsTheServerGone_KeepsTheListAndItsETag_AndSetsOffline()
    {
        Guid id = Guid.CreateVersion7();
        await _storeMetadata.SavePlaylistDetail(Detail(id, Start));
        await _storeMetadata.ReplacePlaylistList([Playlist("A", id)]);
        _clientApi.PlaylistDetailHandler = (_, _) => throw new HttpRequestException("down");
        _clientApi.PlaylistListHandler = _ => Ok(new List<ResponsePlaylist> { Playlist("A", id, Start.AddHours(1)) }, "\"p2\"");

        await _serviceSync.SyncPlaylistList();

        Assert.True(_serviceSync.IsOffline);
        Assert.Equal("\"p2\"", await _storeMetadata.GetPlaylistListETag());
        Assert.Equal(Start, (await _storeMetadata.GetPlaylistDetail(id))?.TimeModified);
    }

    [Fact]
    public async Task SyncPlaylistDetail_AlwaysDownloadsInFull_AndStoresIt()
    {
        Guid id = Guid.CreateVersion7();
        await _storeMetadata.SavePlaylistDetail(Detail(id, Start));
        _clientApi.PlaylistDetailHandler = (_, _) => Ok(Detail(id, Start.AddHours(2)), "\"d1\"");

        await _serviceSync.SyncPlaylistDetail(id);

        Assert.Equal([(id, null)], _clientApi.PlaylistDetailRequestList);
        Assert.Equal(Start.AddHours(2), (await _storeMetadata.GetPlaylistDetail(id))?.TimeModified);
    }

    [Fact]
    public async Task SyncPlaylistDetail_WhenTheServerSays404_DeletesTheLocalPlaylist()
    {
        Guid id = Guid.CreateVersion7();
        await _storeMetadata.ReplacePlaylistList([Playlist("A", id)]);
        await _storeMetadata.SavePlaylistDetail(Detail(id));
        _clientApi.PlaylistDetailHandler = (_, _) => throw new ApiException(HttpStatusCode.NotFound, "Not found.");

        await _serviceSync.SyncPlaylistDetail(id);

        Assert.Empty(await _storeMetadata.GetPlaylistList());
        Assert.Null(await _storeMetadata.GetPlaylistDetail(id));
    }

    [Fact]
    public async Task SyncPlaylistDetail_WhenTheNetworkIsDown_KeepsTheStoredDetail_AndSetsOffline()
    {
        Guid id = Guid.CreateVersion7();
        await _storeMetadata.SavePlaylistDetail(Detail(id));
        _clientApi.PlaylistDetailHandler = (_, _) => throw new HttpRequestException("down");

        await _serviceSync.SyncPlaylistDetail(id);

        Assert.True(_serviceSync.IsOffline);
        Assert.NotNull(await _storeMetadata.GetPlaylistDetail(id));
    }

    [Fact]
    public async Task SyncPlaylistDetail_WhenTheServerRejectsWithAnotherStatus_Propagates()
    {
        _clientApi.PlaylistDetailHandler = (_, _) => throw new ApiException(HttpStatusCode.Forbidden, "No.");

        await Assert.ThrowsAsync<ApiException>(() => _serviceSync.SyncPlaylistDetail(Guid.CreateVersion7()));
    }

    [Fact]
    public async Task Changed_IsRaisedAfterEachStoreWrite_AndWhenOfflineChanges()
    {
        int changed = 0;
        _serviceSync.Changed += () => changed++;
        _clientApi.LibraryHandler = _ => Ok(new List<ResponseMediaFile> { MediaFile("One") }, "\"v1\"");
        _clientApi.PlaylistListHandler = _ => Ok(new List<ResponsePlaylist> { Playlist("A") }, "\"p1\"");
        Guid id = Guid.CreateVersion7();
        _clientApi.PlaylistDetailHandler = (_, _) => Ok(Detail(id));

        await _serviceSync.SyncLibrary();
        Assert.Equal(1, changed);

        await _serviceSync.SyncPlaylistList();
        Assert.Equal(2, changed);

        await _serviceSync.SyncPlaylistDetail(id);
        Assert.Equal(3, changed);

        _clientApi.LibraryHandler = _ => throw new HttpRequestException("down");
        await _serviceSync.SyncLibrary(force: true);
        Assert.Equal(4, changed);

        await _serviceSync.SyncLibrary(force: true);
        Assert.Equal(4, changed);
    }

    private static ResponsePlaylist PlayedPlaylist(Guid id, DateTime modified, DateTime? lastPlayed)
    {
        return new()
        {
            ID = id,
            Name = "Mix",
            AllowDuplicates = true,
            EntryCount = 1,
            TimeCreated = Start,
            TimeModified = modified,
            TimeLastPlayed = lastPlayed
        };
    }

    [Fact]
    public async Task ApplyMediaUpdated_SavesIt_AndKeepsTheStoredETag()
    {
        await _storeMetadata.SetLibraryETag("\"v1\"");
        ResponseMediaFile media = MediaFile("Pushed");

        await _serviceSync.ApplyMediaUpdated(media);

        Assert.Equal(["Pushed"], (await _storeMetadata.GetMediaFileList()).Select(x => x.Title).ToList());
        Assert.Equal("\"v1\"", await _storeMetadata.GetLibraryETag());
    }

    [Fact]
    public async Task ApplyMediaUpdated_ReplacesTheLocalCopyInPlace()
    {
        ResponseMediaFile media = MediaFile("Old");
        await _storeMetadata.ReplaceMediaFileList([media, MediaFile("Other")]);

        await _serviceSync.ApplyMediaUpdated(MediaFile("New", media.ID));

        Assert.Equal(["New", "Other"], (await _storeMetadata.GetMediaFileList()).Select(x => x.Title).ToList());
    }

    [Fact]
    public async Task ApplyMediaDeleted_RemovesIt_AndAnUnknownIDIsHarmless()
    {
        ResponseMediaFile media = MediaFile("One");
        await _storeMetadata.ReplaceMediaFileList([media]);

        await _serviceSync.ApplyMediaDeleted(Guid.CreateVersion7());
        Assert.Single(await _storeMetadata.GetMediaFileList());

        await _serviceSync.ApplyMediaDeleted(media.ID);
        Assert.Empty(await _storeMetadata.GetMediaFileList());
    }

    [Fact]
    public async Task ApplyUploadSucceeded_WithAMedia_SavesIt()
    {
        await _serviceSync.ApplyUploadSucceeded(new() { JobID = Guid.CreateVersion7(), Title = "Up", Succeeded = true, Media = MediaFile("Up") });

        Assert.Equal(["Up"], (await _storeMetadata.GetMediaFileList()).Select(x => x.Title).ToList());
    }

    [Fact]
    public async Task ApplyUploadSucceeded_WithNoMedia_ChangesNothing()
    {
        int changed = 0;
        _serviceSync.Changed += () => changed++;

        await _serviceSync.ApplyUploadSucceeded(new() { JobID = Guid.CreateVersion7(), Title = "Up", Succeeded = true });

        Assert.Empty(await _storeMetadata.GetMediaFileList());
        Assert.Equal(0, changed);
    }

    [Fact]
    public async Task ApplyPlaylistUpdated_SavesTheSummary()
    {
        ResponsePlaylist playlist = Playlist("Pushed");

        await _serviceSync.ApplyPlaylistUpdated(playlist);

        Assert.Equal(["Pushed"], (await _storeMetadata.GetPlaylistList()).Select(x => x.Name).ToList());
    }

    [Fact]
    public async Task ApplyPlaylistUpdated_WithANewerTimeThanAHeldDetail_RefetchesThatDetail()
    {
        Guid id = Guid.CreateVersion7();
        await _storeMetadata.SavePlaylistDetail(Detail(id, Start));
        await _storeMetadata.SavePlaylist(Playlist("Mix", id, Start));
        _clientApi.PlaylistDetailHandler = (_, _) => Ok(Detail(id, Start.AddHours(1)));

        await _serviceSync.ApplyPlaylistUpdated(Playlist("Mix", id, Start.AddHours(1)));

        Assert.Equal([id], _clientApi.PlaylistDetailRequestList.Select(x => x.ID).ToList());
        Assert.Equal(Start.AddHours(1), (await _storeMetadata.GetPlaylistDetail(id))?.TimeModified);
    }

    [Fact]
    public async Task ApplyPlaylistUpdated_WithAnOlderTimeThanTheLocalSummary_IsIgnored()
    {
        Guid id = Guid.CreateVersion7();
        await _storeMetadata.SavePlaylist(Playlist("Newer", id, Start.AddHours(2)));
        int changed = 0;
        _serviceSync.Changed += () => changed++;

        await _serviceSync.ApplyPlaylistUpdated(Playlist("Older", id, Start.AddHours(1)));

        Assert.Equal(["Newer"], (await _storeMetadata.GetPlaylistList()).Select(x => x.Name).ToList());
        Assert.Equal(0, changed);
    }

    [Fact]
    public async Task ApplyPlaylistUpdated_WithTheSameModifiedTimeButANewerLastPlayedTime_IsApplied_AndDoesNotRefetchTheDetail()
    {
        Guid id = Guid.CreateVersion7();
        await _storeMetadata.SavePlaylistDetail(Detail(id, Start));
        await _storeMetadata.SavePlaylist(PlayedPlaylist(id, Start, null));

        await _serviceSync.ApplyPlaylistUpdated(PlayedPlaylist(id, Start, Start.AddMinutes(5)));

        Assert.Equal(Start.AddMinutes(5), (await _storeMetadata.GetPlaylistList()).Single().TimeLastPlayed);
        Assert.Empty(_clientApi.PlaylistDetailRequestList);
    }

    [Fact]
    public async Task ApplyPlaylistUpdated_WithTheSameTimesAsTheLocalSummary_IsIgnored()
    {
        Guid id = Guid.CreateVersion7();
        await _storeMetadata.SavePlaylist(PlayedPlaylist(id, Start, Start.AddMinutes(5)));
        int changed = 0;
        _serviceSync.Changed += () => changed++;

        await _serviceSync.ApplyPlaylistUpdated(PlayedPlaylist(id, Start, Start.AddMinutes(5)));

        Assert.Equal(0, changed);
    }

    [Fact]
    public async Task ApplyPlaylistUpdated_ForAPlaylistWhoseDetailIsNotHeld_FetchesNothing()
    {
        await _serviceSync.ApplyPlaylistUpdated(Playlist("Mix"));

        Assert.Empty(_clientApi.PlaylistDetailRequestList);
    }

    [Fact]
    public async Task ApplyPlaylistDeleted_RemovesPlaylistAndDetail_AndAnUnknownIDIsHarmless()
    {
        Guid id = Guid.CreateVersion7();
        await _storeMetadata.SavePlaylist(Playlist("Mix", id));
        await _storeMetadata.SavePlaylistDetail(Detail(id));

        await _serviceSync.ApplyPlaylistDeleted(Guid.CreateVersion7());
        Assert.Single(await _storeMetadata.GetPlaylistList());

        await _serviceSync.ApplyPlaylistDeleted(id);

        Assert.Empty(await _storeMetadata.GetPlaylistList());
        Assert.Null(await _storeMetadata.GetPlaylistDetail(id));
    }

    [Fact]
    public async Task ApplyPlaylistOrder_ReplacesTheStoredList_AndRaisesChanged()
    {
        await _storeMetadata.SavePlaylist(Playlist("Old"));
        int changed = 0;
        _serviceSync.Changed += () => changed++;
        List<ResponsePlaylist> newList = [Playlist("B"), Playlist("A")];

        await _serviceSync.ApplyPlaylistOrder(newList);

        Assert.Equal(["B", "A"], (await _storeMetadata.GetPlaylistList()).Select(x => x.Name));
        Assert.Equal(1, changed);
    }

    [Fact]
    public async Task APush_NeverWritesAnETag()
    {
        await _storeMetadata.SetLibraryETag("\"v1\"");
        await _storeMetadata.SetPlaylistListETag("\"p1\"");
        Guid id = Guid.CreateVersion7();
        ResponseMediaFile media = MediaFile("One");

        await _serviceSync.ApplyMediaUpdated(media);
        await _serviceSync.ApplyMediaDeleted(media.ID);
        await _serviceSync.ApplyUploadSucceeded(new() { JobID = id, Title = "Up", Succeeded = true, Media = MediaFile("Up") });
        await _serviceSync.ApplyPlaylistUpdated(Playlist("Mix", id));
        await _serviceSync.ApplyPlaylistDeleted(id);

        Assert.Equal("\"v1\"", await _storeMetadata.GetLibraryETag());
        Assert.Equal("\"p1\"", await _storeMetadata.GetPlaylistListETag());
    }

    [Fact]
    public async Task EveryApplyThatChangesTheStore_RaisesChanged()
    {
        int changed = 0;
        _serviceSync.Changed += () => changed++;
        Guid id = Guid.CreateVersion7();
        ResponseMediaFile media = MediaFile("One");

        await _serviceSync.ApplyMediaUpdated(media);
        await _serviceSync.ApplyMediaDeleted(media.ID);
        await _serviceSync.ApplyPlaylistUpdated(Playlist("Mix", id));
        await _serviceSync.ApplyPlaylistDeleted(id);

        Assert.Equal(4, changed);
    }

    #region -- Library diffs --

    private const string HashOne = "1111111111111111111111111111111111111111111111111111111111111111";
    private const string HashTwo = "2222222222222222222222222222222222222222222222222222222222222222";

    private List<LibraryDiff> CollectDiffList()
    {
        List<LibraryDiff> diffList = [];
        _serviceSync.LibraryDiffApplied += diffList.Add;

        return diffList;
    }

    [Fact]
    public async Task SyncLibrary_FirstRun_ListsEveryMediaFileAsAdded()
    {
        List<LibraryDiff> diffList = CollectDiffList();
        ResponseMediaFile one = MediaFile("One");
        ResponseMediaFile two = MediaFile("Two");
        _clientApi.LibraryHandler = _ => Ok(new List<ResponseMediaFile> { one, two }, "\"v1\"");

        await _serviceSync.SyncLibrary();

        LibraryDiff diff = Assert.Single(diffList);
        Assert.Equal([one.ID, two.ID], diff.AddedList.Select(x => x.ID));
        Assert.Empty(diff.ChangedList);
        Assert.Empty(diff.RemovedIDList);
    }

    [Fact]
    public async Task SyncLibrary_On200_RaisesOneDiff_ListingAddedChangedAndRemoved()
    {
        ResponseMediaFile kept = MediaFile("Kept", null, HashOne);
        ResponseMediaFile changedBefore = MediaFile("Changed", null, HashOne);
        ResponseMediaFile gone = MediaFile("Gone", null, HashOne);
        await _storeMetadata.ReplaceMediaFileList([kept, changedBefore, gone]);
        await _storeMetadata.SetLibraryETag("\"v1\"");
        ResponseMediaFile changedAfter = MediaFile("Changed", changedBefore.ID, HashTwo);
        ResponseMediaFile added = MediaFile("Added", null, HashOne);
        List<LibraryDiff> diffList = CollectDiffList();
        _clientApi.LibraryHandler = _ => Ok(new List<ResponseMediaFile> { kept, changedAfter, added }, "\"v2\"");

        await _serviceSync.SyncLibrary();

        LibraryDiff diff = Assert.Single(diffList);
        Assert.Equal([added.ID], diff.AddedList.Select(x => x.ID));
        Assert.Equal([changedAfter.ID], diff.ChangedList.Select(x => x.ID));
        Assert.Equal(HashTwo, diff.ChangedList.Single().ContentHash);
        Assert.Equal([gone.ID], diff.RemovedIDList);
    }

    [Fact]
    public async Task SyncLibrary_AMetadataOnlyChange_RaisesNoDiff()
    {
        ResponseMediaFile before = MediaFile("Before", null, HashOne);
        await _storeMetadata.ReplaceMediaFileList([before]);
        await _storeMetadata.SetLibraryETag("\"v1\"");
        List<LibraryDiff> diffList = CollectDiffList();
        _clientApi.LibraryHandler = _ => Ok(new List<ResponseMediaFile> { MediaFile("After", before.ID, HashOne) }, "\"v2\"");

        await _serviceSync.SyncLibrary();

        Assert.Empty(diffList);
        Assert.Equal("After", (await _storeMetadata.GetMediaFileList()).Single().Title);
    }

    [Fact]
    public async Task SyncLibrary_On304_RaisesNoDiff()
    {
        await _storeMetadata.ReplaceMediaFileList([MediaFile("One")]);
        await _storeMetadata.SetLibraryETag("\"v1\"");
        List<LibraryDiff> diffList = CollectDiffList();
        _clientApi.LibraryHandler = _ => NotModified<List<ResponseMediaFile>>();

        await _serviceSync.SyncLibrary();

        Assert.Empty(diffList);
    }

    [Fact]
    public async Task ApplyMediaUpdated_ForANewMediaFile_RaisesAnAddedDiff()
    {
        List<LibraryDiff> diffList = CollectDiffList();
        ResponseMediaFile media = MediaFile("New");

        await _serviceSync.ApplyMediaUpdated(media);

        LibraryDiff diff = Assert.Single(diffList);
        Assert.Equal([media.ID], diff.AddedList.Select(x => x.ID));
    }

    [Fact]
    public async Task ApplyMediaUpdated_WithADifferentHash_RaisesAChangedDiff()
    {
        ResponseMediaFile before = MediaFile("Media", null, HashOne);
        await _storeMetadata.SaveMediaFile(before);
        List<LibraryDiff> diffList = CollectDiffList();

        await _serviceSync.ApplyMediaUpdated(MediaFile("Media", before.ID, HashTwo));

        LibraryDiff diff = Assert.Single(diffList);
        Assert.Equal([before.ID], diff.ChangedList.Select(x => x.ID));
        Assert.Empty(diff.AddedList);
    }

    [Fact]
    public async Task ApplyMediaUpdated_WithTheSameHash_RaisesNoDiff()
    {
        ResponseMediaFile before = MediaFile("Before", null, HashOne);
        await _storeMetadata.SaveMediaFile(before);
        List<LibraryDiff> diffList = CollectDiffList();

        await _serviceSync.ApplyMediaUpdated(MediaFile("After", before.ID, HashOne));

        Assert.Empty(diffList);
    }

    [Fact]
    public async Task ApplyMediaDeleted_OfAMediaFileInTheLibrary_RaisesARemovedDiff()
    {
        ResponseMediaFile media = MediaFile("One");
        await _storeMetadata.SaveMediaFile(media);
        List<LibraryDiff> diffList = CollectDiffList();

        await _serviceSync.ApplyMediaDeleted(media.ID);

        LibraryDiff diff = Assert.Single(diffList);
        Assert.Equal([media.ID], diff.RemovedIDList);
    }

    [Fact]
    public async Task ApplyMediaDeleted_OfAnUnknownID_RaisesNoDiff()
    {
        List<LibraryDiff> diffList = CollectDiffList();

        await _serviceSync.ApplyMediaDeleted(Guid.CreateVersion7());

        Assert.Empty(diffList);
    }

    [Fact]
    public async Task ApplyUploadSucceeded_WithAMedia_RaisesAnAddedDiff()
    {
        List<LibraryDiff> diffList = CollectDiffList();
        ResponseMediaFile media = MediaFile("Up");

        await _serviceSync.ApplyUploadSucceeded(new() { JobID = Guid.CreateVersion7(), Title = "Up", Succeeded = true, Media = media });

        Assert.Equal([media.ID], Assert.Single(diffList).AddedList.Select(x => x.ID));
    }

    #endregion -- Library diffs --
}
