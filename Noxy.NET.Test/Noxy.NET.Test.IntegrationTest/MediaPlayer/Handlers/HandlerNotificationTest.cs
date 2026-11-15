using Microsoft.Extensions.Logging;
using Noxy.NET.MediaPlayer.API.Handlers;
using Noxy.NET.MediaPlayer.Domain.Enums;
using Noxy.NET.MediaPlayer.Domain.Responses.Media;
using Noxy.NET.MediaPlayer.Domain.Responses.Playlist;
using Noxy.NET.Test.MediaPlayer.Fakes;

namespace Noxy.NET.Test.MediaPlayer.Handlers;

public class HandlerNotificationTest
{
    private static readonly Guid UserID = Guid.CreateVersion7();

    private readonly FakeHubContext _hub = new();

    private static CancellationToken Token => TestContext.Current.CancellationToken;

    private static ResponseMediaFile MediaFile()
    {
        return new()
        {
            ID = Guid.CreateVersion7(),
            Title = "Media",
            ContentType = "audio/mpeg",
            SizeBytes = 1,
            ContentHash = new string('a', 64),
            TimeCreated = DateTime.UtcNow,
            TimeModified = DateTime.UtcNow
        };
    }

    private static ResponsePlaylist Playlist()
    {
        return new()
        {
            ID = Guid.CreateVersion7(),
            Name = "Mix",
            AllowDuplicates = true,
            EntryCount = 0,
            TimeCreated = DateTime.UtcNow,
            TimeModified = DateTime.UtcNow
        };
    }

    private static ResponseUploadResult UploadResult(bool succeeded)
    {
        return new()
        {
            JobID = Guid.CreateVersion7(),
            Title = "Media",
            Succeeded = succeeded
        };
    }

    private void AssertPushed(string eventName, object payload)
    {
        (string userID, string method, object?[] argumentList, CancellationToken token) = Assert.Single(_hub.SendList);

        Assert.Equal(UserID.ToString(), userID);
        Assert.Equal(eventName, method);
        Assert.Same(payload, Assert.Single(argumentList));
        Assert.Equal(Token, token);
    }

    [Fact]
    public async Task MediaUpdated_PushesTheEventWithThePayloadToThatUserOnly()
    {
        ResponseMediaFile mediaFile = MediaFile();

        await new HandlerNotificationMediaUpdated(_hub, new FakeLogger<HandlerNotificationMediaUpdated>()).Handle(new(UserID, mediaFile), Token);

        AssertPushed(nameof(MediaHubEvent.MediaUpdated), mediaFile);
    }

    [Fact]
    public async Task MediaDeleted_PushesTheEventWithTheIdToThatUserOnly()
    {
        Guid id = Guid.CreateVersion7();

        await new HandlerNotificationMediaDeleted(_hub, new FakeLogger<HandlerNotificationMediaDeleted>()).Handle(new(UserID, id), Token);

        (string userID, string method, object?[] argumentList, CancellationToken token) = Assert.Single(_hub.SendList);
        Assert.Equal(UserID.ToString(), userID);
        Assert.Equal(nameof(MediaHubEvent.MediaDeleted), method);
        Assert.Equal(id, Assert.Single(argumentList));
        Assert.Equal(Token, token);
    }

    [Fact]
    public async Task PlaylistUpdated_PushesTheEventWithThePayloadToThatUserOnly()
    {
        ResponsePlaylist playlist = Playlist();

        await new HandlerNotificationPlaylistUpdated(_hub, new FakeLogger<HandlerNotificationPlaylistUpdated>()).Handle(new(UserID, playlist), Token);

        AssertPushed(nameof(MediaHubEvent.PlaylistUpdated), playlist);
    }

    [Fact]
    public async Task PlaylistDeleted_PushesTheEventWithTheIdToThatUserOnly()
    {
        Guid id = Guid.CreateVersion7();

        await new HandlerNotificationPlaylistDeleted(_hub, new FakeLogger<HandlerNotificationPlaylistDeleted>()).Handle(new(UserID, id), Token);

        (string userID, string method, object?[] argumentList, CancellationToken token) = Assert.Single(_hub.SendList);
        Assert.Equal(UserID.ToString(), userID);
        Assert.Equal(nameof(MediaHubEvent.PlaylistDeleted), method);
        Assert.Equal(id, Assert.Single(argumentList));
        Assert.Equal(Token, token);
    }

    [Fact]
    public async Task UploadSucceeded_PushesTheEventWithThePayloadToThatUserOnly()
    {
        ResponseUploadResult result = UploadResult(true);

        await new HandlerNotificationUploadSucceeded(_hub, new FakeLogger<HandlerNotificationUploadSucceeded>()).Handle(new(UserID, result), Token);

        AssertPushed(nameof(MediaHubEvent.UploadSucceeded), result);
    }

    [Fact]
    public async Task UploadFailed_PushesTheEventWithThePayloadToThatUserOnly()
    {
        ResponseUploadResult result = UploadResult(false);

        await new HandlerNotificationUploadFailed(_hub, new FakeLogger<HandlerNotificationUploadFailed>()).Handle(new(UserID, result), Token);

        AssertPushed(nameof(MediaHubEvent.UploadFailed), result);
    }

    [Fact]
    public async Task EveryHandler_WhenThePushThrows_DoesNotThrowAndLogsAWarning()
    {
        _hub.Failure = new InvalidOperationException("The connection is gone.");
        FakeLogger<HandlerNotificationMediaUpdated> mediaUpdated = new();
        FakeLogger<HandlerNotificationMediaDeleted> mediaDeleted = new();
        FakeLogger<HandlerNotificationPlaylistUpdated> playlistUpdated = new();
        FakeLogger<HandlerNotificationPlaylistDeleted> playlistDeleted = new();
        FakeLogger<HandlerNotificationUploadSucceeded> uploadSucceeded = new();
        FakeLogger<HandlerNotificationUploadFailed> uploadFailed = new();

        await new HandlerNotificationMediaUpdated(_hub, mediaUpdated).Handle(new(UserID, MediaFile()), Token);
        await new HandlerNotificationMediaDeleted(_hub, mediaDeleted).Handle(new(UserID, Guid.CreateVersion7()), Token);
        await new HandlerNotificationPlaylistUpdated(_hub, playlistUpdated).Handle(new(UserID, Playlist()), Token);
        await new HandlerNotificationPlaylistDeleted(_hub, playlistDeleted).Handle(new(UserID, Guid.CreateVersion7()), Token);
        await new HandlerNotificationUploadSucceeded(_hub, uploadSucceeded).Handle(new(UserID, UploadResult(true)), Token);
        await new HandlerNotificationUploadFailed(_hub, uploadFailed).Handle(new(UserID, UploadResult(false)), Token);

        Assert.Empty(_hub.SendList);
        Assert.Contains(mediaUpdated.EntryList, x => x.Level == LogLevel.Warning);
        Assert.Contains(mediaDeleted.EntryList, x => x.Level == LogLevel.Warning);
        Assert.Contains(playlistUpdated.EntryList, x => x.Level == LogLevel.Warning);
        Assert.Contains(playlistDeleted.EntryList, x => x.Level == LogLevel.Warning);
        Assert.Contains(uploadSucceeded.EntryList, x => x.Level == LogLevel.Warning);
        Assert.Contains(uploadFailed.EntryList, x => x.Level == LogLevel.Warning);
    }
}
