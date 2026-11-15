using Noxy.NET.MediaPlayer.Application.Commands.Metadata;
using Noxy.NET.MediaPlayer.Application.Handlers.Metadata.Genre;
using Noxy.NET.MediaPlayer.Application.Notifications.Metadata;
using Noxy.NET.MediaPlayer.Application.Services;
using Noxy.NET.MediaPlayer.Domain.Entities;
using Noxy.NET.MediaPlayer.Domain.Exceptions;
using Noxy.NET.MediaPlayer.Domain.Responses.Metadata;
using Noxy.NET.Test.MediaPlayer.Fakes;

namespace Noxy.NET.Test.MediaPlayer.Metadata;

public class HandlerGenreCreateTest
{
    private static readonly Guid UserID = Guid.CreateVersion7();

    private readonly FakeStore _store = new();
    private readonly FakePublisher _publisher = new();
    private readonly HandlerGenreCreate _handler;

    public HandlerGenreCreateTest()
    {
        _handler = new(new FakeUnitOfWorkFactory(_store), new GenreItemService(), new NameKeyService(), _publisher, new FakeTimeProvider());
    }

    private static CancellationToken Token => TestContext.Current.CancellationToken;

    private Task<ResponseGenre> Create(string name, Guid? userID = null)
    {
        return _handler.Handle(new CommandGenreCreate(userID ?? UserID, name), Token).AsTask();
    }

    [Fact]
    public async Task Create_StoresTheTrimmedNameAndKey_AndPushesTheGenreToTheOwner()
    {
        ResponseGenre response = await Create("  Trip   Hop ");

        EntityGenre genre = Assert.Single(_store.GenreList);
        Assert.Equal("Trip   Hop", genre.Name);
        Assert.Equal("trip hop", genre.NameKey);
        Assert.Equal(genre.ID, response.ID);
        Assert.Equal(0, response.MediaFileCount);
        NotificationGenreUpdated pushed = Assert.Single(_publisher.GetList<NotificationGenreUpdated>());
        Assert.Equal(UserID, pushed.UserID);
        Assert.Equal(genre.ID, pushed.Genre.ID);
        Assert.Equal(1, _store.CommitCount);
    }

    [Fact]
    public async Task Create_ABlankName_IsRejected()
    {
        await Assert.ThrowsAsync<ArgumentException>(() => Create("  "));

        Assert.Empty(_store.GenreList);
    }

    [Fact]
    public async Task Create_ANameWhoseKeyExists_IsAConflictNamingTheExistingGenre()
    {
        await Create("Rock");

        NameAlreadyInUseException exception = await Assert.ThrowsAsync<NameAlreadyInUseException>(() => Create(" ROCK"));

        Assert.Equal(Assert.Single(_store.GenreList).ID, exception.ExistingID);
    }

    [Fact]
    public async Task Create_TheSameNameForAnotherUser_IsAllowed()
    {
        await Create("Rock");
        await Create("Rock", Guid.CreateVersion7());

        Assert.Equal(2, _store.GenreList.Count);
    }
}
