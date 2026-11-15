using Noxy.NET.MediaPlayer.Application.Commands.Metadata;
using Noxy.NET.MediaPlayer.Application.Handlers.Metadata;
using Noxy.NET.MediaPlayer.Application.Notifications.Metadata;
using Noxy.NET.MediaPlayer.Application.Services;
using Noxy.NET.MediaPlayer.Domain.Entities;
using Noxy.NET.MediaPlayer.Domain.Exceptions;
using Noxy.NET.MediaPlayer.Domain.Responses.Metadata;
using Noxy.NET.Test.MediaPlayer.Fakes;

namespace Noxy.NET.Test.MediaPlayer.Metadata;

public class HandlerArtistCreateTest
{
    private static readonly Guid UserID = Guid.CreateVersion7();

    private readonly FakeStore _store = new();
    private readonly FakePublisher _publisher = new();
    private readonly FakeTimeProvider _time = new();
    private readonly HandlerArtistCreate _handler;

    public HandlerArtistCreateTest()
    {
        _handler = new(new FakeUnitOfWorkFactory(_store), new ArtistItemService(new MetadataChangeService()), new NameKeyService(), _publisher, _time);
    }

    private static CancellationToken Token => TestContext.Current.CancellationToken;

    private Task<ResponseArtist> Create(string name, Guid? userID = null)
    {
        return _handler.Handle(new CommandArtistCreate(userID ?? UserID, name), Token).AsTask();
    }

    [Fact]
    public async Task Create_StoresTheTrimmedNameAndItsKeyForTheUser()
    {
        _time.Now = new(2027, 5, 6, 7, 8, 9, TimeSpan.Zero);

        ResponseArtist response = await Create("  Massive   Attack ");

        EntityArtist artist = Assert.Single(_store.ArtistList);
        Assert.Equal("Massive   Attack", artist.Name);
        Assert.Equal("massive attack", artist.NameKey);
        Assert.Equal(UserID, artist.UserID);
        Assert.Equal(_time.Now.UtcDateTime, artist.TimeModified);
        Assert.Equal(artist.ID, response.ID);
        Assert.Equal(0, response.MediaFileCount);
        Assert.Equal(_time.Now.UtcDateTime, response.TimeModified);
    }

    [Fact]
    public async Task Create_CommitsOnceAndPushesTheNewArtistToTheOwnerAfterTheCommit()
    {
        int commitsAtPublish = -1;
        _publisher.OnPublish = _ => commitsAtPublish = _store.CommitCount;

        ResponseArtist response = await Create("A");

        Assert.Equal(1, _store.CommitCount);
        NotificationArtistUpdated pushed = Assert.Single(_publisher.GetList<NotificationArtistUpdated>());
        Assert.Equal(UserID, pushed.UserID);
        Assert.Equal(response.ID, pushed.Artist.ID);
        Assert.Equal(1, commitsAtPublish);
        Assert.Equal(Token, Assert.Single(_publisher.TokenList));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t")]
    public async Task Create_ABlankName_IsRejectedAndNothingIsSaved(string name)
    {
        await Assert.ThrowsAsync<ArgumentException>(() => Create(name));

        Assert.Empty(_store.ArtistList);
        Assert.Equal(0, _store.CommitCount);
        Assert.Empty(_publisher.PublishedList);
    }

    [Fact]
    public async Task Create_ANameOver255Characters_IsRejected()
    {
        await Assert.ThrowsAsync<ArgumentException>(() => Create(new string('a', 256)));

        Assert.Empty(_store.ArtistList);
    }

    [Fact]
    public async Task Create_ANameOf255Characters_IsAccepted()
    {
        ResponseArtist response = await Create(new string('a', 255));

        Assert.Equal(255, response.Name.Length);
    }

    [Theory]
    [InlineData("radiohead")]
    [InlineData("  RADIOHEAD ")]
    public async Task Create_ANameWhoseKeyExists_IsAConflictNamingTheExistingArtist(string name)
    {
        await Create("Radiohead");
        _publisher.PublishedList.Clear();
        int commits = _store.CommitCount;

        NameAlreadyInUseException exception = await Assert.ThrowsAsync<NameAlreadyInUseException>(() => Create(name));

        Assert.Equal(Assert.Single(_store.ArtistList).ID, exception.ExistingID);
        Assert.Equal(commits, _store.CommitCount);
        Assert.Empty(_publisher.PublishedList);
    }

    [Fact]
    public async Task Create_TheSameNameForAnotherUser_IsAllowed()
    {
        await Create("Radiohead");

        await Create("Radiohead", Guid.CreateVersion7());

        Assert.Equal(2, _store.ArtistList.Count);
    }

    [Fact]
    public async Task Create_ALostRaceOnCommit_RunsAgainAndStillCreatesOneArtist()
    {
        _store.KeyConflictCommitCount = 1;

        await Create("Newcomer");

        Assert.Single(_store.ArtistList);
        Assert.Single(_publisher.GetList<NotificationArtistUpdated>());
    }

    [Fact]
    public async Task Create_WhenTheCommitFails_PublishesNothing()
    {
        _store.FailCommit = true;

        await Assert.ThrowsAsync<InvalidOperationException>(() => Create("A"));

        Assert.Empty(_publisher.PublishedList);
    }
}
