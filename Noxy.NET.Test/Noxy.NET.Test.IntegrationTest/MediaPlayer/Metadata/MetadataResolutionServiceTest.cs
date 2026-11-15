using Microsoft.EntityFrameworkCore;
using Noxy.NET.MediaPlayer.Application.Interfaces;
using Noxy.NET.MediaPlayer.Application.Models.Metadata;
using Noxy.NET.MediaPlayer.Application.Services;
using Noxy.NET.MediaPlayer.Domain.Entities;
using Noxy.NET.MediaPlayer.Domain.Exceptions;
using Noxy.NET.MediaPlayer.Persistence;
using Noxy.NET.Test.MediaPlayer.Fixtures;

namespace Noxy.NET.Test.MediaPlayer.Metadata;

public class MetadataResolutionServiceTest : IDisposable
{
    private readonly DatabaseFixture _fixture = new();
    private readonly MetadataResolutionService _service;

    public MetadataResolutionServiceTest()
    {
        NameKeyService serviceNameKey = new();
        _service = new(serviceNameKey, new CreditSplittingService(serviceNameKey));
    }

    public void Dispose()
    {
        _fixture.Dispose();
    }

    private async Task<EntityUser> CreateUser()
    {
        EntityUser user = new() { Email = "user@example.com" };

        await using IUnitOfWork uow = _fixture.UoWFactory.Create();
        uow.Authentication.AddUser(user);
        await uow.Commit(TestContext.Current.CancellationToken);

        return user;
    }

    private static MediaTagSet Tags()
    {
        return new(null, ["Radiohead"], "OK Computer", ["Radiohead"], ["Rock"], null, null, null, null, null, null);
    }

    [Fact]
    public async Task Resolve_TwoRequestsNamingTheSameNewItems_TheSecondCommitConflictsAndItsRetryReusesTheWinner()
    {
        EntityUser user = await CreateUser();

        await using IUnitOfWork winner = _fixture.UoWFactory.Create();
        await using IUnitOfWork loser = _fixture.UoWFactory.Create();
        ResolvedMetadata winning = await _service.Resolve(winner, user.ID, Tags(), TestContext.Current.CancellationToken);
        ResolvedMetadata losing = await _service.Resolve(loser, user.ID, Tags(), TestContext.Current.CancellationToken);

        await winner.Commit(TestContext.Current.CancellationToken);
        await Assert.ThrowsAsync<MetadataKeyConflictException>(() => loser.Commit(TestContext.Current.CancellationToken));

        await using IUnitOfWork retry = _fixture.UoWFactory.Create();
        ResolvedMetadata retried = await _service.Resolve(retry, user.ID, Tags(), TestContext.Current.CancellationToken);
        await retry.Commit(TestContext.Current.CancellationToken);

        Assert.NotEqual(winning.ArtistList[0].ID, losing.ArtistList[0].ID);
        Assert.Equal(winning.ArtistList[0].ID, retried.ArtistList[0].ID);
        Assert.Equal(winning.Album?.ID, retried.Album?.ID);
        Assert.Equal(winning.GenreList[0].ID, retried.GenreList[0].ID);

        await using DataContext context = _fixture.CreateContext();
        Assert.Equal(1, await context.Artist.CountAsync(TestContext.Current.CancellationToken));
        Assert.Equal(1, await context.Album.CountAsync(TestContext.Current.CancellationToken));
        Assert.Equal(1, await context.Genre.CountAsync(TestContext.Current.CancellationToken));
        Assert.Equal(1, await context.AlbumArtist.CountAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task Resolve_AgainstARealDatabase_FindsTheCommittedItemsAndLinksTheAlbumArtist()
    {
        EntityUser user = await CreateUser();
        await using (IUnitOfWork first = _fixture.UoWFactory.Create())
        {
            await _service.Resolve(first, user.ID, Tags(), TestContext.Current.CancellationToken);
            await first.Commit(TestContext.Current.CancellationToken);
        }

        await using IUnitOfWork second = _fixture.UoWFactory.Create();
        ResolvedMetadata again = await _service.Resolve(second, user.ID, new(null, ["radiohead"], "ok computer", ["RADIOHEAD"], ["rock"], null, null, null, null, null, null), TestContext.Current.CancellationToken);
        await second.Commit(TestContext.Current.CancellationToken);

        await using DataContext context = _fixture.CreateContext();
        Assert.Equal(1, await context.Artist.CountAsync(TestContext.Current.CancellationToken));
        Assert.Equal(1, await context.Album.CountAsync(TestContext.Current.CancellationToken));
        Assert.Equal(again.Album?.ID, (await context.Album.SingleAsync(TestContext.Current.CancellationToken)).ID);
    }
}
