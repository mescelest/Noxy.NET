using System.Net;
using Microsoft.Extensions.Logging;
using Noxy.NET.MediaPlayer.Domain.Enums;
using Noxy.NET.MediaPlayer.Presentation.Exceptions;
using Noxy.NET.MediaPlayer.Presentation.Services;
using Noxy.NET.Test.MediaPlayer.Fakes;

namespace Noxy.NET.Test.MediaPlayer.Presentation;

public class MediaHubServiceTest
{
    [Fact]
    public void TheHandledEvents_AreExactlyThoseThisPlanAdds_AfterTheSyncedOnes()
    {
        List<string> expected =
        [
            nameof(MediaHubEventEnum.UploadSucceeded),
            nameof(MediaHubEventEnum.MediaUpdated),
            nameof(MediaHubEventEnum.MediaDeleted),
            nameof(MediaHubEventEnum.PlaylistUpdated),
            nameof(MediaHubEventEnum.PlaylistDeleted),
            nameof(MediaHubEventEnum.SettingUpdated),
            nameof(MediaHubEventEnum.ArtistUpdated),
            nameof(MediaHubEventEnum.ArtistDeleted),
            nameof(MediaHubEventEnum.AlbumUpdated),
            nameof(MediaHubEventEnum.AlbumDeleted),
            nameof(MediaHubEventEnum.GenreUpdated),
            nameof(MediaHubEventEnum.GenreDeleted),
            nameof(MediaHubEventEnum.DeviceListChanged),
            nameof(MediaHubEventEnum.PlayerStateChanged),
            nameof(MediaHubEventEnum.QueueChanged),
            nameof(MediaHubEventEnum.QueueSnapshot),
            nameof(MediaHubEventEnum.SessionEnded),
            nameof(MediaHubEventEnum.TransferFailed),
            nameof(MediaHubEventEnum.PlayerCommandReceived),
            nameof(MediaHubEventEnum.PlayerTransferRequested),
            nameof(MediaHubEventEnum.PlayerStartRequested)
        ];

        Assert.Equal(expected, MediaHubService.HandledEventNameList);
    }

    [Fact]
    public async Task Start_WithNoSavedAddress_DoesNothing_AndDoesNotThrow()
    {
        FakeSyncService serviceSync = new();
        MediaHubService serviceHub = new(serviceSync, new FakeSettingService(), new FakeMetadataSyncService(), new FakeSessionService(), new FakeServerAddressStorage(null), new FakeLogger<MediaHubService>());

        await serviceHub.Start();
        await serviceHub.Stop();

        Assert.False(serviceHub.IsConnected);
        Assert.Empty(serviceSync.CallList);
    }

    [Fact]
    public async Task Start_WhenSignedOut_DoesNothing()
    {
        FakeSessionService serviceSession = new() { AccessToken = null };
        MediaHubService serviceHub = new(new FakeSyncService(), new FakeSettingService(), new FakeMetadataSyncService(), serviceSession, new FakeServerAddressStorage(), new FakeLogger<MediaHubService>());

        await serviceHub.Start();

        Assert.False(serviceHub.IsConnected);
    }

    [Fact]
    public async Task Start_WhenTheServerCannotBeReached_DoesNotThrow_LogsAWarning_AndCanBeStartedAgain()
    {
        FakeLogger<MediaHubService> logger = new();
        MediaHubService serviceHub = new(new FakeSyncService(), new FakeSettingService(), new FakeMetadataSyncService(), new FakeSessionService(), new FakeServerAddressStorage("http://127.0.0.1:1"), logger);

        await serviceHub.Start();
        await serviceHub.Start();

        Assert.False(serviceHub.IsConnected);
        Assert.Equal(2, logger.EntryList.Count(x => x.Level == LogLevel.Warning));
        Assert.DoesNotContain(logger.EntryList, x => x.Message.Contains("token", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task Invoke_AndSend_WhenDisconnected_ReturnNullAndDoNotThrow()
    {
        MediaHubService serviceHub = new(new FakeSyncService(), new FakeSettingService(), new FakeMetadataSyncService(), new FakeSessionService(), new FakeServerAddressStorage(), new FakeLogger<MediaHubService>());

        Assert.Null(await serviceHub.Invoke<bool?>(MediaHubMethodEnum.ClaimActive, new object()));
        Assert.Null(await serviceHub.Invoke<List<string>>(MediaHubMethodEnum.RequestSnapshot, null));
        await serviceHub.Send(MediaHubMethodEnum.EditQueue, new object());
    }

    [Fact]
    public async Task Connected_IsNotRaised_WhenStartDoesNotConnect()
    {
        MediaHubService serviceHub = new(new FakeSyncService(), new FakeSettingService(), new FakeMetadataSyncService(), new FakeSessionService(), new FakeServerAddressStorage(null), new FakeLogger<MediaHubService>());
        int connected = 0;
        serviceHub.Connected += () => connected++;

        await serviceHub.Start();

        Assert.Equal(0, connected);
    }

    [Fact]
    public async Task Stop_BeforeStart_DoesNotThrow()
    {
        MediaHubService serviceHub = new(new FakeSyncService(), new FakeSettingService(), new FakeMetadataSyncService(), new FakeSessionService(), new FakeServerAddressStorage(), new FakeLogger<MediaHubService>());

        await serviceHub.Stop();
    }

    [Fact]
    public async Task TheReconnectSyncs_EachRunInTheirOwnTry_SoOneFailureDoesNotSkipTheRest()
    {
        FakeSyncService serviceSync = new();
        FakeSettingService serviceSetting = new() { SyncSettingListException = new ApiException(HttpStatusCode.InternalServerError, "Broken.") };
        FakeMetadataSyncService serviceMetadataSync = new() { SyncArtistListException = new ApiException(HttpStatusCode.InternalServerError, "Broken.") };
        FakeLogger<MediaHubService> logger = new();
        MediaHubService serviceHub = new(serviceSync, serviceSetting, serviceMetadataSync, new FakeSessionService(), new FakeServerAddressStorage(), logger);

        await serviceHub.SyncAfterReconnect();

        Assert.Equal(["SyncLibrary", "SyncPlaylistList"], serviceSync.CallList);
        Assert.Equal([false], serviceSetting.SyncRequestList);
        Assert.Equal(["SyncArtistList", "SyncAlbumList", "SyncGenreList"], serviceMetadataSync.CallList);
        Assert.Equal(2, logger.EntryList.Count(x => x.Level == LogLevel.Error));
    }
}
