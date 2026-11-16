using Microsoft.Extensions.Logging;
using Noxy.NET.MediaPlayer.Domain.Enums;
using Noxy.NET.MediaPlayer.Presentation.Services;
using Noxy.NET.Test.MediaPlayer.Fakes;

namespace Noxy.NET.Test.MediaPlayer.Presentation;

public class MediaHubServiceTest
{
    [Fact]
    public void TheHandledEvents_AreExactlyThoseThisPlanSyncs()
    {
        List<string> expected =
        [
            nameof(MediaHubEventEnum.UploadSucceeded),
            nameof(MediaHubEventEnum.MediaUpdated),
            nameof(MediaHubEventEnum.MediaDeleted),
            nameof(MediaHubEventEnum.PlaylistUpdated),
            nameof(MediaHubEventEnum.PlaylistDeleted)
        ];

        Assert.Equal(expected, MediaHubService.HandledEventNameList);
    }

    [Fact]
    public async Task Start_WithNoSavedAddress_DoesNothing_AndDoesNotThrow()
    {
        FakeSyncService serviceSync = new();
        MediaHubService serviceHub = new(serviceSync, new FakeSessionService(), new FakeServerAddressStorage(null), new FakeLogger<MediaHubService>());

        await serviceHub.Start();
        await serviceHub.Stop();

        Assert.False(serviceHub.IsConnected);
        Assert.Empty(serviceSync.CallList);
    }

    [Fact]
    public async Task Start_WhenSignedOut_DoesNothing()
    {
        FakeSessionService serviceSession = new() { AccessToken = null };
        MediaHubService serviceHub = new(new FakeSyncService(), serviceSession, new FakeServerAddressStorage(), new FakeLogger<MediaHubService>());

        await serviceHub.Start();

        Assert.False(serviceHub.IsConnected);
    }

    [Fact]
    public async Task Start_WhenTheServerCannotBeReached_DoesNotThrow_LogsAWarning_AndCanBeStartedAgain()
    {
        FakeLogger<MediaHubService> logger = new();
        MediaHubService serviceHub = new(new FakeSyncService(), new FakeSessionService(), new FakeServerAddressStorage("http://127.0.0.1:1"), logger);

        await serviceHub.Start();
        await serviceHub.Start();

        Assert.False(serviceHub.IsConnected);
        Assert.Equal(2, logger.EntryList.Count(x => x.Level == LogLevel.Warning));
        Assert.DoesNotContain(logger.EntryList, x => x.Message.Contains("token", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task Stop_BeforeStart_DoesNotThrow()
    {
        MediaHubService serviceHub = new(new FakeSyncService(), new FakeSessionService(), new FakeServerAddressStorage(), new FakeLogger<MediaHubService>());

        await serviceHub.Stop();
    }
}
