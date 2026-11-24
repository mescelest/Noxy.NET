using Noxy.NET.MediaPlayer.Domain.Responses.Media;
using Noxy.NET.MediaPlayer.Domain.Responses.Playlist;
using Noxy.NET.MediaPlayer.Presentation;
using Noxy.NET.MediaPlayer.Presentation.Models;
using Noxy.NET.MediaPlayer.Presentation.Services;
using Noxy.NET.Test.MediaPlayer.Fakes;

namespace Noxy.NET.Test.MediaPlayer.Presentation;

public class HomeServiceTest
{
    private static readonly DateTime Now = new(2026, 11, 17, 12, 0, 0, DateTimeKind.Utc);

    private readonly FakeMetadataStore _store = new();
    private readonly FakePlaybackControlService _control = new();
    private readonly FakeLocalQueueStore _queueStore = new();
    private readonly HomeService _service;

    public HomeServiceTest()
    {
        _service = new(_store, _control, _queueStore);
    }

    private async Task<ResponsePlaylist> AddPlaylist(string name, DateTime? played, DateTime? modified = null)
    {
        ResponsePlaylist playlist = new() { ID = Guid.CreateVersion7(), Name = name, AllowDuplicates = true, EntryCount = 0, TimeCreated = modified ?? Now, TimeModified = modified ?? Now, TimeLastPlayed = played };
        await _store.SavePlaylist(playlist);

        return playlist;
    }

    [Fact]
    public async Task Get_ReturnsAtMostSixPlayedPlaylists_NewestFirst()
    {
        for (int index = 0; index < 8; index++) await AddPlaylist($"p{index}", Now.AddMinutes(index));
        await AddPlaylist("never", null);

        HomeModel model = await _service.Get();

        Assert.Equal(["p7", "p6", "p5", "p4", "p3", "p2"], model.RecentList.Select(x => x.Name));
        Assert.True(model.HasPlaylists);
    }

    [Fact]
    public async Task Get_WhenNothingWasPlayed_ListsTheNewestPlaylistsFirst()
    {
        await AddPlaylist("old", null, Now.AddDays(-2));
        await AddPlaylist("new", null, Now.AddDays(-1));

        HomeModel model = await _service.Get();

        Assert.Equal(["new", "old"], model.RecentList.Select(x => x.Name));
    }

    [Fact]
    public async Task Get_WithNoPlaylists_HasPlaylistsIsFalse()
    {
        HomeModel model = await _service.Get();

        Assert.Empty(model.RecentList);
        Assert.False(model.HasPlaylists);
    }

    [Fact]
    public async Task Get_IncludesTheContinueInfo_AndTheTitleOfTheCurrentMediaFile()
    {
        ResponseMediaFile media = new() { ID = Guid.CreateVersion7(), Title = "Song", Artist = "Band", ContentType = "audio/mpeg", SizeBytes = 1, ContentHash = "h", TimeCreated = Now, TimeModified = Now };
        await _store.SaveMediaFile(media);
        await _queueStore.Save(new(new() { QueueRevision = 1, Source = null, HistoryList = [], Current = new(Guid.CreateVersion7(), media.ID), UserQueueList = [], SourceList = [] }, 42, Now));
        _control.CanContinue = true;

        HomeModel model = await _service.Get();

        Assert.Equal(new("Song", "Band", 42), model.Continue);
    }

    [Fact]
    public async Task Get_WhenCannotContinue_HasNoContinue()
    {
        await _queueStore.Save(new(new() { QueueRevision = 1, Source = null, HistoryList = [], Current = new(Guid.CreateVersion7(), Guid.CreateVersion7()), UserQueueList = [], SourceList = [] }, 5, Now));
        _control.CanContinue = false;

        Assert.Null((await _service.Get()).Continue);
    }

    [Fact]
    public async Task Get_WhenThereIsNoLocalCopy_HasNoContinue()
    {
        _control.CanContinue = true;

        Assert.Null((await _service.Get()).Continue);
    }

    [Fact]
    public async Task Get_NeverIncludesAPlaylistThatIsNotInTheLocalList()
    {
        ResponsePlaylist gone = await AddPlaylist("gone", Now);
        await AddPlaylist("kept", Now.AddMinutes(-1));
        await _store.DeletePlaylist(gone.ID);

        HomeModel model = await _service.Get();

        Assert.Equal(["kept"], model.RecentList.Select(x => x.Name));
    }

    [Fact]
    public void ItNeedsNoApiClient_ItReadsOnlyTheLocalStore()
    {
        // ReSharper disable once ParameterOnlyUsedForPreconditionCheck.Local
        Assert.DoesNotContain(typeof(HomeService).GetConstructors().Single().GetParameters(), x => x.ParameterType.Name.Contains("Api"));
        Assert.Equal(6, PresentationConstants.HomeRecentCount);
    }
}
