using System.Net;
using Noxy.NET.MediaPlayer.Application.Requests.Playlist;
using Noxy.NET.MediaPlayer.Domain.Responses.Media;
using Noxy.NET.MediaPlayer.Domain.Responses.Playlist;
using Noxy.NET.MediaPlayer.Presentation.Enums;
using Noxy.NET.MediaPlayer.Presentation.Exceptions;
using Noxy.NET.MediaPlayer.Presentation.Services;
using Noxy.NET.Test.MediaPlayer.Fakes;

namespace Noxy.NET.Test.MediaPlayer.Presentation;

public class PlaylistServiceTest
{
    private readonly FakeApiClient _api = new();
    private readonly FakeSyncService _sync = new();
    private readonly FakeDownloadService _download = new();
    private readonly FakeMetadataStore _store = new();
    private readonly FakePlaybackControlService _control = new();
    private readonly PlaylistService _service;

    public PlaylistServiceTest()
    {
        _service = new(_api, _sync, _download, _store, _control);
    }

    private static ResponseMediaFile Media()
    {
        return new() { ID = Guid.CreateVersion7(), Title = "Media", ContentType = "audio/mpeg", SizeBytes = 1, TimeCreated = DateTime.UtcNow, TimeModified = DateTime.UtcNow };
    }

    private async Task<(Guid ID, List<ResponseMediaFile> MediaList)> HoldDetail(int count)
    {
        Guid id = Guid.CreateVersion7();
        List<ResponseMediaFile> mediaList = [.. Enumerable.Range(0, count).Select(_ => Media())];
        ResponsePlaylistDetail detail = FakeApiClient.DetailFor(id);
        await _store.SavePlaylistDetail(new()
        {
            ID = id,
            Name = "Playlist",
            AllowDuplicates = true,
            EntryCount = count,
            TimeCreated = detail.TimeCreated,
            TimeModified = detail.TimeModified,
            TimeLastPlayed = null,
            EntryList = [.. mediaList.Select((x, index) => new ResponsePlaylistEntry { ID = Guid.CreateVersion7(), MediaFileID = x.ID, Position = index, MediaFile = x })]
        });

        return (id, mediaList);
    }

    [Fact]
    public async Task Create_PostsTheName_SavesTheSummary_AndReturnsIt()
    {
        ResponsePlaylist playlist = await _service.Create("  Road trip ");

        Assert.Equal("Road trip", Assert.Single(_api.CreatePlaylistRequestList).Name);
        Assert.Same(playlist, Assert.Single(_sync.PlaylistUpdatedList));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Create_SendsTheDuplicatesRuleItWasGiven(bool allowDuplicates)
    {
        await _service.Create("Strict", allowDuplicates);

        Assert.Equal(allowDuplicates, Assert.Single(_api.CreatePlaylistRequestList).AllowDuplicates);
    }

    [Fact]
    public async Task Create_WithoutARule_AllowsTheSameSongTwice()
    {
        await _service.Create("Open");

        Assert.True(Assert.Single(_api.CreatePlaylistRequestList).AllowDuplicates);
    }

    [Fact]
    public async Task Open_SyncsTheDetail_AndStartsNoDownload()
    {
        (Guid id, _) = await HoldDetail(3);

        await _service.Open(id);

        Assert.Equal([id], _sync.PlaylistDetailSyncList);
        Assert.Empty(_download.QueueMissingList);
    }

    [Fact]
    public async Task Open_WhenOffline_StillReturns_AndStartsNoDownload()
    {
        _sync.IsOffline = true;
        (Guid id, _) = await HoldDetail(2);

        await _service.Open(id);

        Assert.Empty(_download.QueueMissingList);
    }

    [Fact]
    public async Task ReadingTheStoredDetail_StartsNoDownload()
    {
        (Guid id, _) = await HoldDetail(2);

        await _store.GetPlaylistDetail(id);
        await _service.Refresh(id);

        Assert.Empty(_download.QueueMissingList);
        Assert.Equal([id], _sync.PlaylistDetailSyncList);
    }

    [Fact]
    public async Task DownloadAll_QueuesEveryEntryNotOnTheDevice()
    {
        (Guid id, List<ResponseMediaFile> mediaList) = await HoldDetail(3);
        await _store.SaveFileState(new(mediaList[2].ID, FileSyncStateEnum.OnDevice, null));

        await _service.DownloadAll(id);

        Assert.Equal([mediaList[0].ID, mediaList[1].ID], _download.QueueMissingList);
        Assert.Empty(_sync.PlaylistDetailSyncList);
    }

    [Fact]
    public async Task Play_CallsStartPlaylist_WithThePlaylistID()
    {
        Guid id = Guid.CreateVersion7();

        await _service.Play(id);

        Assert.Equal([$"StartPlaylist {id} "], _control.CallList);
    }

    [Fact]
    public async Task Play_FromAnEntry_PassesTheEntryID()
    {
        Guid id = Guid.CreateVersion7();
        Guid entryID = Guid.CreateVersion7();

        await _service.Play(id, entryID);

        Assert.Equal([$"StartPlaylist {id} {entryID}"], _control.CallList);
    }

    [Fact]
    public async Task Rename_And_SetAllowDuplicates_SendAnUpdateWithOnlyThatField()
    {
        Guid id = Guid.CreateVersion7();

        await _service.Rename(id, " New name ");
        await _service.SetAllowDuplicates(id, false);

        Assert.Equal("New name", _api.UpdatePlaylistRequestList[0].Request.Name.Value);
        Assert.False(_api.UpdatePlaylistRequestList[0].Request.AllowDuplicates.HasValue);
        Assert.False(_api.UpdatePlaylistRequestList[1].Request.AllowDuplicates.Value);
        Assert.False(_api.UpdatePlaylistRequestList[1].Request.Name.HasValue);
        Assert.Equal(2, _sync.PlaylistUpdatedList.Count);
    }

    [Fact]
    public async Task RemoveEntryList_SendsTheEntryIDs_AndSavesTheNewDetail()
    {
        Guid id = Guid.CreateVersion7();
        List<Guid> entryIDList = [Guid.CreateVersion7(), Guid.CreateVersion7()];

        await _service.RemoveEntryList(id, entryIDList);

        Assert.Equal(entryIDList, Assert.Single(_api.RemoveEntryCallList).Request.EntryIDList);
        Assert.NotNull(await _store.GetPlaylistDetail(id));
        Assert.Single(_sync.PlaylistUpdatedList);
    }

    [Fact]
    public async Task RemoveEntryList_OfNone_SendsNothing()
    {
        await _service.RemoveEntryList(Guid.CreateVersion7(), []);

        Assert.Empty(_api.RemoveEntryCallList);
    }

    [Fact]
    public async Task MoveEntry_SendsThePosition_AndSavesTheNewDetail()
    {
        Guid id = Guid.CreateVersion7();
        Guid entryID = Guid.CreateVersion7();

        await _service.MoveEntry(id, entryID, 4);

        Assert.Equal(4, Assert.Single(_api.MoveEntryCallList).Request.Position);
        Assert.NotNull(await _store.GetPlaylistDetail(id));
    }

    [Fact]
    public async Task Move_SendsThePosition_AndAppliesTheAnswerAsTheOrder()
    {
        Guid id = Guid.CreateVersion7();
        List<ResponsePlaylist> answer = [FakeApiClient.PlaylistFor(id, "Moved")];
        _api.MovePlaylistHandler = (_, _) => answer;

        await _service.Move(id, 2);

        (Guid movedID, var request) = Assert.Single(_api.MovePlaylistCallList);
        Assert.Equal(id, movedID);
        Assert.Equal(2, request.Position);
        Assert.Same(answer, Assert.Single(_sync.PlaylistOrderAppliedList));
    }

    [Fact]
    public async Task Move_WhenTheServerRefuses_Throws_AndAppliesNothing()
    {
        _api.WriteFailure = new ApiException(HttpStatusCode.BadRequest, "Bad position.");

        await Assert.ThrowsAsync<ApiException>(() => _service.Move(Guid.CreateVersion7(), 5));

        Assert.Empty(_sync.PlaylistOrderAppliedList);
    }

    [Fact]
    public async Task Delete_DeletesOnTheServer_AndRemovesTheLocalPlaylist()
    {
        Guid id = Guid.CreateVersion7();

        await _service.Delete(id);

        Assert.Equal([id], _api.DeletePlaylistList);
        Assert.Equal([id], _sync.PlaylistDeletedList);
    }

    [Fact]
    public async Task EveryWrite_WhenTheServerCannotBeReached_Throws_AndChangesNothingLocally()
    {
        _api.WriteFailure = new HttpRequestException("down");
        Guid id = Guid.CreateVersion7();

        await Assert.ThrowsAsync<HttpRequestException>(() => _service.Create("x"));
        await Assert.ThrowsAsync<HttpRequestException>(() => _service.Rename(id, "x"));
        await Assert.ThrowsAsync<HttpRequestException>(() => _service.SetAllowDuplicates(id, true));
        await Assert.ThrowsAsync<HttpRequestException>(() => _service.RemoveEntryList(id, [Guid.CreateVersion7()]));
        await Assert.ThrowsAsync<HttpRequestException>(() => _service.MoveEntry(id, Guid.CreateVersion7(), 0));
        await Assert.ThrowsAsync<HttpRequestException>(() => _service.Delete(id));

        Assert.Empty(_sync.PlaylistUpdatedList);
        Assert.Empty(_sync.PlaylistDeletedList);
        Assert.Null(await _store.GetPlaylistDetail(id));
    }

    [Fact]
    public async Task Edit_SendsTheTrimmedNameAndTheDuplicatesRuleInOneUpdate()
    {
        Guid id = Guid.CreateVersion7();

        await _service.Edit(id, " New name ", false);

        (Guid RequestID, RequestUpdatePlaylist Request) sent = Assert.Single(_api.UpdatePlaylistRequestList);
        Assert.Equal(id, sent.RequestID);
        Assert.Equal("New name", sent.Request.Name.Value);
        Assert.False(sent.Request.AllowDuplicates.Value);
        Assert.Single(_sync.PlaylistUpdatedList);
    }

    [Fact]
    public async Task Edit_WhenTheServerRefuses_ThrowsItsApiException_AndChangesNothingLocally()
    {
        _api.WriteFailure = new ApiException(HttpStatusCode.BadRequest, "The playlist has the same song twice.");
        Guid id = Guid.CreateVersion7();

        ApiException exception = await Assert.ThrowsAsync<ApiException>(() => _service.Edit(id, "x", false));

        Assert.Equal("The playlist has the same song twice.", exception.Title);
        Assert.Empty(_sync.PlaylistUpdatedList);
        Assert.Null(await _store.GetPlaylistDetail(id));
    }

    [Fact]
    public async Task AWriteTheServerRefuses_ThrowsItsApiException()
    {
        _api.WriteFailure = new ApiException(HttpStatusCode.BadRequest, "The playlist is full.");

        ApiException exception = await Assert.ThrowsAsync<ApiException>(() => _service.Create("x"));

        Assert.Equal("The playlist is full.", exception.Title);
    }

    [Fact]
    public async Task Edit_SendsAnArtHue_WhenOneWasChosen()
    {
        await _service.Edit(Guid.CreateVersion7(), "Mix", true, new(200));

        RequestUpdatePlaylist sent = Assert.Single(_api.UpdatePlaylistRequestList).Request;
        Assert.True(sent.ArtHue.HasValue);
        Assert.Equal(200, sent.ArtHue.Value);
    }

    [Fact]
    public async Task Edit_WithTheDefaultChoice_SendsANullToClearIt()
    {
        await _service.Edit(Guid.CreateVersion7(), "Mix", true, new(null));

        RequestUpdatePlaylist sent = Assert.Single(_api.UpdatePlaylistRequestList).Request;
        Assert.True(sent.ArtHue.HasValue);
        Assert.Null(sent.ArtHue.Value);
    }

    [Fact]
    public async Task Edit_LeavesTheHueOutWhenItDidNotChange()
    {
        await _service.Edit(Guid.CreateVersion7(), "Mix", true);

        RequestUpdatePlaylist sent = Assert.Single(_api.UpdatePlaylistRequestList).Request;
        Assert.False(sent.ArtHue.HasValue);
    }

    [Fact]
    public async Task Create_SendsTheArtHueItWasGiven_AndNullWhenNone()
    {
        await _service.Create("Colourful", true, 200);
        await _service.Create("Plain");

        Assert.Equal([200, null], _api.CreatePlaylistRequestList.Select(x => x.ArtHue));
    }
}
