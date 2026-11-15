using Microsoft.Extensions.Options;
using Noxy.NET.MediaPlayer.Application.Configurations;
using Noxy.NET.MediaPlayer.Application.Handlers.Playlist;
using Noxy.NET.MediaPlayer.Application.Notifications.Playlist;
using Noxy.NET.MediaPlayer.Domain.Exceptions;
using Noxy.NET.MediaPlayer.Domain.Responses.Playlist;
using Noxy.NET.Test.MediaPlayer.Fakes;

namespace Noxy.NET.Test.MediaPlayer.Playlists;

public class HandlerPlaylistCreateTest
{
    private static readonly Guid UserID = Guid.CreateVersion7();

    private readonly FakeStore _store = new();
    private readonly FakePublisher _publisher = new();
    private readonly FakeTimeProvider _time = new();
    private readonly ConfigPlaylist _config = new();
    private readonly HandlerPlaylistCreate _handler;

    public HandlerPlaylistCreateTest()
    {
        _handler = new(new FakeUnitOfWorkFactory(_store), Options.Create(_config), _publisher, _time);
    }

    private static CancellationToken Token => TestContext.Current.CancellationToken;

    private async Task<Guid> Create(string name = "Mix", bool allowDuplicates = true, Guid? userID = null)
    {
        ResponsePlaylist created = await _handler.Handle(new(userID ?? UserID, name, allowDuplicates), Token);
        return created.ID;
    }

    [Fact]
    public async Task Create_WhenTheCommitFails_PublishesNothing()
    {
        _store.FailCommit = true;

        await Assert.ThrowsAsync<InvalidOperationException>(() => _handler.Handle(new(UserID, "Other", true), Token).AsTask());

        Assert.Empty(_publisher.PublishedList);
    }

    [Fact]
    public async Task Create_PublishesOnlyAfterTheCommit()
    {
        List<int> commitsAtPublishList = [];
        _publisher.OnPublish = _ => commitsAtPublishList.Add(_store.CommitCount);
        int commits = _store.CommitCount;

        await Create();

        Assert.Equal([commits + 1], commitsAtPublishList);
    }

    [Fact]
    public async Task Create_PassesTheCancellationTokenToThePublish()
    {
        using CancellationTokenSource source = new();

        await _handler.Handle(new(UserID, "Mix", true), source.Token);

        Assert.Equal(source.Token, Assert.Single(_publisher.TokenList));
    }

    [Fact]
    public async Task Create_TrimsTheNameAndDefaultsDuplicatesToAllowed()
    {
        ResponsePlaylist created = await _handler.Handle(new(UserID, "  Road trip  ", true), Token);

        Assert.Equal("Road trip", created.Name);
        Assert.True(created.AllowDuplicates);
        Assert.Equal(0, created.EntryCount);
        Assert.Equal(DateTimeKind.Utc, created.TimeModified.Kind);
        Assert.Equal("Road trip", _store.PlaylistList.Single(x => x.ID == created.ID).Name);
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

        NotificationPlaylistUpdated pushed = Assert.Single(_publisher.GetList<NotificationPlaylistUpdated>());
        Assert.Equal(UserID, pushed.UserID);
        Assert.Equal(id, pushed.Playlist.ID);
    }

    [Fact]
    public async Task Create_ABlankName_Throws()
    {
        await Assert.ThrowsAsync<ArgumentException>(() => Create("   "));
    }
}
