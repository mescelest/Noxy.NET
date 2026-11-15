using Mediator;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Noxy.NET.MediaPlayer.Application.Notifications.Media;
using Noxy.NET.MediaPlayer.Application.Notifications.Playlist;
using Noxy.NET.MediaPlayer.Domain.Enums;
using Noxy.NET.MediaPlayer.Domain.Responses.Media;
using Noxy.NET.MediaPlayer.Domain.Responses.Playlist;
using Noxy.NET.Test.MediaPlayer.Fakes;
using Noxy.NET.Test.MediaPlayer.Fixtures;

namespace Noxy.NET.Test.MediaPlayer.Handlers;

public class HubNotificationFlowTest
{
    [Fact]
    public async Task Publisher_IsRegisteredInTheHost()
    {
        await using ApiFactory factory = new();

        IPublisher? publisher = factory.Services.GetService<IPublisher>();

        Assert.NotNull(publisher);
    }

    [Fact]
    public async Task Publishing_AnUploadSucceededNotification_ReachesAConnectedClientOfThatUserOnly()
    {
        await using ApiFactory factory = new();
        MediaApiClient api = new(factory);
        ApiAccount user = await api.SignUp("a@example.com");
        ApiAccount other = await api.SignUp("b@example.com");
        await using LibraryHubListener listener = await LibraryHubListener.Connect(factory, user.Token);
        await using LibraryHubListener stranger = await LibraryHubListener.Connect(factory, other.Token);
        ResponseUploadResult result = new() { JobID = Guid.CreateVersion7(), Title = "Media", Succeeded = true };

        await factory.Services.GetRequiredService<IPublisher>().Publish(new NotificationUploadSucceeded(user.UserID, result), TestContext.Current.CancellationToken);

        (string eventName, ResponseUploadResult received) = await listener.Next();
        Assert.Equal(nameof(MediaHubEvent.UploadSucceeded), eventName);
        Assert.Equal(result.JobID, received.JobID);
        Assert.True(await stranger.ReceivesNothingWithin(TimeSpan.FromSeconds(1)));
    }

    [Fact]
    public async Task Publishing_EveryNotificationType_ReachesAConnectedClientOfThatUser()
    {
        await using ApiFactory factory = new();
        MediaApiClient api = new(factory);
        ApiAccount user = await api.SignUp("a@example.com");
        await using LibraryHubListener listener = await LibraryHubListener.Connect(factory, user.Token);
        IPublisher publisher = factory.Services.GetRequiredService<IPublisher>();
        CancellationToken token = TestContext.Current.CancellationToken;
        ResponseMediaFile mediaFile = new()
        {
            ID = Guid.CreateVersion7(),
            Title = "Media",
            ContentType = "audio/mpeg",
            SizeBytes = 1,
            ContentHash = new string('a', 64),
            TimeCreated = DateTime.UtcNow,
            TimeModified = DateTime.UtcNow
        };
        ResponsePlaylist playlist = new()
        {
            ID = Guid.CreateVersion7(),
            Name = "Mix",
            AllowDuplicates = true,
            EntryCount = 0,
            TimeCreated = DateTime.UtcNow,
            TimeModified = DateTime.UtcNow
        };
        Guid deletedID = Guid.CreateVersion7();

        await publisher.Publish(new NotificationMediaUpdated(user.UserID, mediaFile), token);
        await publisher.Publish(new NotificationMediaDeleted(user.UserID, deletedID), token);
        await publisher.Publish(new NotificationPlaylistUpdated(user.UserID, playlist), token);
        await publisher.Publish(new NotificationPlaylistDeleted(user.UserID, deletedID), token);
        await publisher.Publish(new NotificationUploadSucceeded(user.UserID, new() { JobID = Guid.CreateVersion7(), Title = "Media", Succeeded = true }), token);
        await publisher.Publish(new NotificationUploadFailed(user.UserID, new() { JobID = Guid.CreateVersion7(), Title = "Media", Succeeded = false }), token);

        Assert.Equal(mediaFile.ID, (await listener.NextMediaUpdated()).ID);
        Assert.Equal(deletedID, await listener.NextMediaDeleted());
        Assert.Equal(playlist.ID, (await listener.NextPlaylistUpdated()).ID);
        Assert.Equal(deletedID, await listener.NextPlaylistDeleted());
        Assert.Equal(nameof(MediaHubEvent.UploadSucceeded), (await listener.Next()).Event);
        Assert.Equal(nameof(MediaHubEvent.UploadFailed), (await listener.Next()).Event);
        Assert.Equal(6, await listener.CountReceivedAfter(TimeSpan.FromMilliseconds(500)));
    }

    [Fact]
    public async Task Host_InProduction_FailsToStartWhenANotificationHandlerCannotBeResolved()
    {
        await using ApiFactory factory = new();
        await using WebApplicationFactory<Program> broken = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment(Environments.Production);
            builder.ConfigureServices(services => services.AddSingleton<INotificationHandler<NotificationMediaUpdated>, FakeUnresolvableNotificationHandler>());
        });

        Assert.ThrowsAny<Exception>(() => broken.Server);
    }
}
