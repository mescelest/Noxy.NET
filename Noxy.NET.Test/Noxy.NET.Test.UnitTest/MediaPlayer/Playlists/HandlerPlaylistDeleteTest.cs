using Noxy.NET.MediaPlayer.Application.Handlers.Playlist;
using Noxy.NET.MediaPlayer.Application.Notifications.Playlist;
using Noxy.NET.MediaPlayer.Domain.Entities;
using Noxy.NET.MediaPlayer.Domain.Exceptions;
using Noxy.NET.Test.MediaPlayer.Fakes;

namespace Noxy.NET.Test.MediaPlayer.Playlists;

public class HandlerPlaylistDeleteTest
{
    private static readonly Guid UserID = Guid.CreateVersion7();

    private readonly FakeStore _store = new();
    private readonly FakePublisher _publisher = new();
    private readonly HandlerPlaylistDelete _handler;

    public HandlerPlaylistDeleteTest()
    {
        _handler = new(new FakeUnitOfWorkFactory(_store), _publisher);
    }

    private static CancellationToken Token => TestContext.Current.CancellationToken;

    private Guid Create(int entryCount = 0)
    {
        EntityPlaylist playlist = new() { UserID = UserID, Name = "Mix", TimeCreated = DateTime.UtcNow, TimeModified = DateTime.UtcNow };

        for (int index = 0; index < entryCount; index++)
        {
            EntityMediaFile file = new()
            {
                FilePath = $"{Guid.CreateVersion7()}.mp3",
                ContentType = "audio/mpeg",
                SizeBytes = 3,
                ContentHash = new('a', 64),
                UserID = UserID
            };

            _store.MediaFileList.Add(file);
            playlist.EntryList.Add(new() { PlaylistID = playlist.ID, MediaFileID = file.ID, Position = index });
        }

        _store.PlaylistList.Add(playlist);
        return playlist.ID;
    }

    [Fact]
    public async Task Delete_WhenTheCommitFails_PublishesNothing()
    {
        Guid id = Create();
        _store.FailCommit = true;

        await Assert.ThrowsAsync<InvalidOperationException>(() => _handler.Handle(new(UserID, id), Token).AsTask());

        Assert.Empty(_publisher.PublishedList);
    }

    [Fact]
    public async Task Delete_PublishesOnlyAfterTheCommit()
    {
        Guid id = Create();
        List<int> commitsAtPublishList = [];
        _publisher.OnPublish = _ => commitsAtPublishList.Add(_store.CommitCount);
        int commits = _store.CommitCount;

        await _handler.Handle(new(UserID, id), Token);

        Assert.Equal([commits + 1], commitsAtPublishList);
    }

    [Fact]
    public async Task Delete_PassesTheCancellationTokenToThePublish()
    {
        Guid id = Create();
        using CancellationTokenSource source = new();

        await _handler.Handle(new(UserID, id), source.Token);

        Assert.Equal(source.Token, Assert.Single(_publisher.TokenList));
    }

    [Fact]
    public async Task Delete_RemovesThePlaylistAndPushesDeleted()
    {
        Guid id = Create();

        await _handler.Handle(new(UserID, id), Token);

        Assert.Empty(_store.PlaylistList);
        NotificationPlaylistDeleted deleted = Assert.Single(_publisher.GetList<NotificationPlaylistDeleted>());
        Assert.Equal(UserID, deleted.UserID);
        Assert.Equal(id, deleted.ID);
    }

    [Fact]
    public async Task Delete_KeepsTheMediaFiles()
    {
        Guid id = Create(2);

        await _handler.Handle(new(UserID, id), Token);

        Assert.Equal(2, _store.MediaFileList.Count);
    }

    [Fact]
    public async Task Delete_AnotherUsersPlaylist_ThrowsNotFound()
    {
        Guid id = Create();

        await Assert.ThrowsAsync<PlaylistNotFoundException>(() => _handler.Handle(new(Guid.CreateVersion7(), id), Token).AsTask());
        Assert.Single(_store.PlaylistList);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public async Task Delete_ClosesTheGap_SoPositionsStayWhole(int positionToDelete)
    {
        List<EntityPlaylist> list = [];
        for (int position = 0; position < 3; position++)
        {
            EntityPlaylist playlist = new() { UserID = UserID, Name = $"P{position}", Position = position, TimeCreated = DateTime.UtcNow, TimeModified = DateTime.UtcNow };
            _store.PlaylistList.Add(playlist);
            list.Add(playlist);
        }

        Guid otherUser = Guid.CreateVersion7();
        _store.PlaylistList.Add(new() { UserID = otherUser, Name = "Theirs", Position = 1, TimeCreated = DateTime.UtcNow, TimeModified = DateTime.UtcNow });

        await _handler.Handle(new(UserID, list[positionToDelete].ID), Token);

        Assert.Equal([0, 1], _store.PlaylistList.Where(x => x.UserID == UserID).OrderBy(x => x.Position).Select(x => x.Position));
        Assert.Equal(1, _store.PlaylistList.Single(x => x.UserID == otherUser).Position);
    }
}
