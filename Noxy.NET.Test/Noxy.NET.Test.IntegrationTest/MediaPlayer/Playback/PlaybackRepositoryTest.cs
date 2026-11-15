using Noxy.NET.MediaPlayer.Application.Interfaces;
using Noxy.NET.MediaPlayer.Domain.Entities;
using Noxy.NET.MediaPlayer.Domain.Exceptions;
using Noxy.NET.Test.MediaPlayer.Fixtures;

namespace Noxy.NET.Test.MediaPlayer.Playback;

public class PlaybackRepositoryTest : IDisposable
{
    private readonly DatabaseFixture _fixture = new();

    public void Dispose()
    {
        _fixture.Dispose();
    }

    private static CancellationToken Token => TestContext.Current.CancellationToken;

    private async Task<EntityUser> CreateUser(string email = "user@example.com")
    {
        EntityUser user = new() { Email = email };

        await using IUnitOfWork uow = _fixture.UoWFactory.Create();
        uow.Authentication.AddUser(user);
        await uow.Commit(Token);

        return user;
    }

    private async Task<Guid> CreateMediaFile(Guid userID)
    {
        EntityMediaFile file = new()
        {
            FilePath = $"{Guid.CreateVersion7()}.mp3",
            ContentType = "audio/mpeg",
            SizeBytes = 1234,
            ContentHash = new string('a', 64),
            UserID = userID
        };

        file.Metadata = new() { Title = "Media", MediaFileID = file.ID };

        await using IUnitOfWork uow = _fixture.UoWFactory.Create();
        uow.Media.AddMediaFile(file);
        await uow.Commit(Token);

        return file.ID;
    }

    private async Task Add(EntityPlaybackSession session)
    {
        await using IUnitOfWork uow = _fixture.UoWFactory.Create();
        uow.Playback.AddSession(session);
        await uow.Commit(Token);
    }

    [Fact]
    public async Task AddSession_ThenFind_ReturnsEveryFieldWithTheQueueInOrderAndRepeats()
    {
        EntityUser user = await CreateUser();
        Guid first = await CreateMediaFile(user.ID);
        Guid second = await CreateMediaFile(user.ID);
        DateTime modified = new(2026, 4, 5, 6, 7, 8, DateTimeKind.Utc);
        EntityPlaybackSession session = new()
        {
            UserID = user.ID,
            QueueMediaFileIDList = [second, first, second],
            CurrentIndex = 2,
            PositionSeconds = 61.25,
            PlaylistID = Guid.CreateVersion7(),
            TimeModified = modified
        };
        await Add(session);

        await using IUnitOfWork uow = _fixture.UoWFactory.Create();
        EntityPlaybackSession? found = await uow.Playback.FindSession(user.ID, Token);

        Assert.NotNull(found);
        Assert.Equal(session.ID, found.ID);
        Assert.Equal([second, first, second], found.QueueMediaFileIDList);
        Assert.Equal(2, found.CurrentIndex);
        Assert.Equal(61.25, found.PositionSeconds);
        Assert.Equal(session.PlaylistID, found.PlaylistID);
        Assert.Equal(modified, found.TimeModified);
    }

    [Fact]
    public async Task AnEmptyQueueAndNullIndexAndPlaylist_AreSavedAsTheyAre()
    {
        EntityUser user = await CreateUser();
        await Add(new() { UserID = user.ID });

        await using IUnitOfWork uow = _fixture.UoWFactory.Create();
        EntityPlaybackSession? found = await uow.Playback.FindSession(user.ID, Token);

        Assert.NotNull(found);
        Assert.Empty(found.QueueMediaFileIDList);
        Assert.Null(found.CurrentIndex);
        Assert.Null(found.PlaylistID);
    }

    [Fact]
    public async Task FindSession_IsScopedToTheUser_AndNullWhenThereIsNone()
    {
        EntityUser user = await CreateUser();
        EntityUser other = await CreateUser("other@example.com");
        await Add(new() { UserID = user.ID, PositionSeconds = 1 });

        await using IUnitOfWork uow = _fixture.UoWFactory.Create();

        Assert.Null(await uow.Playback.FindSession(other.ID, Token));
        Assert.NotNull(await uow.Playback.FindSession(user.ID, Token));
    }

    [Fact]
    public async Task UpdateSession_SavesTheNewQueueIndexAndPosition()
    {
        EntityUser user = await CreateUser();
        Guid media = await CreateMediaFile(user.ID);
        EntityPlaybackSession session = new() { UserID = user.ID, QueueMediaFileIDList = [media, media], CurrentIndex = 0 };
        await Add(session);

        await using (IUnitOfWork uow = _fixture.UoWFactory.Create())
        {
            EntityPlaybackSession found = await uow.Playback.FindSession(user.ID, Token) ?? throw new InvalidOperationException("The session was not saved.");
            found.QueueMediaFileIDList = [media];
            found.CurrentIndex = null;
            found.PositionSeconds = 9;
            found.TimeModified = new(2026, 5, 5, 5, 5, 5, DateTimeKind.Utc);
            uow.Playback.UpdateSession(found);
            await uow.Commit(Token);
        }

        await using IUnitOfWork check = _fixture.UoWFactory.Create();
        EntityPlaybackSession? updated = await check.Playback.FindSession(user.ID, Token);
        Assert.NotNull(updated);
        Assert.Equal([media], updated.QueueMediaFileIDList);
        Assert.Null(updated.CurrentIndex);
        Assert.Equal(9, updated.PositionSeconds);
        Assert.Equal(session.ID, updated.ID);
    }

    [Fact]
    public async Task RemoveSession_DeletesIt()
    {
        EntityUser user = await CreateUser();
        await Add(new() { UserID = user.ID });

        await using (IUnitOfWork uow = _fixture.UoWFactory.Create())
        {
            EntityPlaybackSession found = await uow.Playback.FindSession(user.ID, Token) ?? throw new InvalidOperationException("The session was not saved.");
            uow.Playback.RemoveSession(found);
            await uow.Commit(Token);
        }

        await using IUnitOfWork check = _fixture.UoWFactory.Create();
        Assert.Null(await check.Playback.FindSession(user.ID, Token));
    }

    [Fact]
    public async Task ASecondSessionForTheSameUser_IsAKeyConflict()
    {
        EntityUser user = await CreateUser();
        await Add(new() { UserID = user.ID });

        await Assert.ThrowsAsync<MetadataKeyConflictException>(() => Add(new() { UserID = user.ID }));
    }

    [Fact]
    public async Task GetOwnedMediaFileIDList_WithTheLongestAllowedQueue_StillAnswers()
    {
        EntityUser user = await CreateUser();
        Guid mine = await CreateMediaFile(user.ID);
        List<Guid> asked = [.. Enumerable.Range(0, 10_000).Select(_ => Guid.CreateVersion7())];
        asked[5000] = mine;

        await using IUnitOfWork uow = _fixture.UoWFactory.Create();

        Assert.Equal([mine], await uow.Media.GetOwnedMediaFileIDList(user.ID, asked, Token));
    }

    [Fact]
    public async Task GetOwnedMediaFileIDList_ReturnsOnlyTheAskedForIDsTheUserOwns()
    {
        EntityUser user = await CreateUser();
        EntityUser other = await CreateUser("other@example.com");
        Guid mine = await CreateMediaFile(user.ID);
        Guid alsoMine = await CreateMediaFile(user.ID);
        Guid notAsked = await CreateMediaFile(user.ID);
        Guid foreign = await CreateMediaFile(other.ID);

        await using IUnitOfWork uow = _fixture.UoWFactory.Create();
        List<Guid> owned = await uow.Media.GetOwnedMediaFileIDList(user.ID, [mine, alsoMine, foreign, Guid.CreateVersion7(), mine], Token);

        Assert.Equal(new HashSet<Guid> { mine, alsoMine }, owned.ToHashSet());
        Assert.DoesNotContain(notAsked, owned);
        Assert.Empty(await uow.Media.GetOwnedMediaFileIDList(user.ID, [], Token));
    }
}
