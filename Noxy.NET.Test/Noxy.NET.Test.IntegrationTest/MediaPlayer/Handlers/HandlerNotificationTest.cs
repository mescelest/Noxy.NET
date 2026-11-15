using Microsoft.Extensions.Logging;
using Noxy.NET.MediaPlayer.API.Handlers;
using Noxy.NET.MediaPlayer.Domain.Enums;
using Noxy.NET.MediaPlayer.Domain.Responses.Media;
using Noxy.NET.MediaPlayer.Domain.Responses.Metadata;
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
    public async Task ArtistUpdated_PushesTheEventWithThePayloadToThatUserOnly()
    {
        ResponseArtist artist = new() { ID = Guid.CreateVersion7(), Name = "Artist", MediaFileCount = 1, TimeModified = DateTime.UtcNow };

        await new HandlerNotificationArtistUpdated(_hub, new FakeLogger<HandlerNotificationArtistUpdated>()).Handle(new(UserID, artist), Token);

        AssertPushed(nameof(MediaHubEvent.ArtistUpdated), artist);
    }

    [Fact]
    public async Task GenreUpdated_PushesTheEventWithThePayloadToThatUserOnly()
    {
        ResponseGenre genre = new() { ID = Guid.CreateVersion7(), Name = "Genre", MediaFileCount = 1, TimeModified = DateTime.UtcNow };

        await new HandlerNotificationGenreUpdated(_hub, new FakeLogger<HandlerNotificationGenreUpdated>()).Handle(new(UserID, genre), Token);

        AssertPushed(nameof(MediaHubEvent.GenreUpdated), genre);
    }

    [Fact]
    public async Task AlbumUpdated_PushesTheEventWithThePayloadToThatUserOnly()
    {
        ResponseAlbum album = new() { ID = Guid.CreateVersion7(), Title = "Album", ArtistIDList = [], MediaFileCount = 1, TimeModified = DateTime.UtcNow };

        await new HandlerNotificationAlbumUpdated(_hub, new FakeLogger<HandlerNotificationAlbumUpdated>()).Handle(new(UserID, album), Token);

        AssertPushed(nameof(MediaHubEvent.AlbumUpdated), album);
    }

    [Fact]
    public async Task ArtistGenreAndAlbumDeleted_PushTheEventWithTheIdToThatUserOnly()
    {
        Guid artistID = Guid.CreateVersion7();
        Guid genreID = Guid.CreateVersion7();
        Guid albumID = Guid.CreateVersion7();

        await new HandlerNotificationArtistDeleted(_hub, new FakeLogger<HandlerNotificationArtistDeleted>()).Handle(new(UserID, artistID), Token);
        await new HandlerNotificationGenreDeleted(_hub, new FakeLogger<HandlerNotificationGenreDeleted>()).Handle(new(UserID, genreID), Token);
        await new HandlerNotificationAlbumDeleted(_hub, new FakeLogger<HandlerNotificationAlbumDeleted>()).Handle(new(UserID, albumID), Token);

        Assert.Equal(
            [(nameof(MediaHubEvent.ArtistDeleted), artistID), (nameof(MediaHubEvent.GenreDeleted), genreID), (nameof(MediaHubEvent.AlbumDeleted), albumID)],
            _hub.SendList.Select(x => (x.Method, (Guid)Assert.Single(x.ArgumentList)!)).ToList());
        Assert.All(_hub.SendList, x => Assert.Equal(UserID.ToString(), x.UserID));
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

    [Fact]
    public async Task WhenThePushIsCancelledBecauseTheCallerCancelled_ItPropagatesAndIsNotLogged()
    {
        using CancellationTokenSource source = new();
        await source.CancelAsync();
        _hub.Failure = new OperationCanceledException(source.Token);
        FakeLogger<HandlerNotificationMediaDeleted> logger = new();

        await Assert.ThrowsAsync<OperationCanceledException>(() => new HandlerNotificationMediaDeleted(_hub, logger).Handle(new(UserID, Guid.CreateVersion7()), source.Token).AsTask());

        Assert.Empty(logger.EntryList);
    }

    [Fact]
    public async Task WhenThePushTimesOutWhileTheCallerHasNotCancelled_ItIsLikeAnyOtherFailure()
    {
        _hub.Failure = new OperationCanceledException();
        FakeLogger<HandlerNotificationMediaDeleted> logger = new();

        await new HandlerNotificationMediaDeleted(_hub, logger).Handle(new(UserID, Guid.CreateVersion7()), Token);

        Assert.Contains(logger.EntryList, x => x.Level == LogLevel.Warning);
    }
}
