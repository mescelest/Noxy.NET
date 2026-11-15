using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Noxy.NET.MediaPlayer.Application.BackgroundServices;
using Noxy.NET.MediaPlayer.Application.Interfaces;
using Noxy.NET.MediaPlayer.Application.Services;
using Noxy.NET.MediaPlayer.Domain.Entities;
using Noxy.NET.MediaPlayer.Domain.Exceptions;
using Noxy.NET.MediaPlayer.Domain.Extensions;
using Noxy.NET.MediaPlayer.Domain.Responses.Media;
using Noxy.NET.MediaPlayer.Persistence;
using Noxy.NET.Test.MediaPlayer.Fakes;
using Noxy.NET.Test.MediaPlayer.Fixtures;

namespace Noxy.NET.Test.MediaPlayer.Metadata;

public class MetadataBackfillServiceTest : IDisposable
{
    private readonly DatabaseFixture _fixture = new();
    private readonly MetadataBackfillService _service;

    public MetadataBackfillServiceTest()
    {
        NameKeyService serviceNameKey = new();
        _service = new(_fixture.UoWFactory, new MetadataResolutionService(serviceNameKey, new CreditSplittingService(serviceNameKey)), new FakeTimeProvider(), NullLogger<MetadataBackfillService>.Instance);
    }

    public void Dispose()
    {
        _fixture.Dispose();
    }

    private async Task<EntityUser> CreateUser(string email)
    {
        EntityUser user = new() { Email = email };

        await using IUnitOfWork uow = _fixture.UoWFactory.Create();
        uow.Authentication.AddUser(user);
        await uow.Commit(TestContext.Current.CancellationToken);

        return user;
    }

    private async Task<List<EntityMediaFile>> SeedFileList(Guid userID, int count, Func<int, (string? Artist, string? Album, string? AlbumArtist, string? Genre)> text)
    {
        List<EntityMediaFile> fileList = [];

        await using IUnitOfWork uow = _fixture.UoWFactory.Create();
        for (int index = 0; index < count; index++)
        {
            (string? artist, string? album, string? albumArtist, string? genre) = text(index);
            EntityMediaFile file = new() { FilePath = $"{Guid.CreateVersion7()}.mp3", ContentType = "audio/mpeg", SizeBytes = 1, ContentHash = new string('a', 64), UserID = userID };
            file.Metadata = new() { Title = $"Title {index}", Artist = artist, Album = album, AlbumArtist = albumArtist, Genre = genre, MediaFileID = file.ID };
            uow.Media.AddMediaFile(file);
            fileList.Add(file);
        }

        await uow.Commit(TestContext.Current.CancellationToken);
        return fileList;
    }

    private async Task<EntityMediaFile> Load(Guid id, Guid userID)
    {
        await using IUnitOfWork uow = _fixture.UoWFactory.Create();
        return await uow.Media.FindMediaFile(id, userID, TestContext.Current.CancellationToken) ?? throw new InvalidOperationException("The file is missing.");
    }

    [Fact]
    public async Task Run_OverARealDatabase_LinksTheLegacyTextOfEveryFileInBatches()
    {
        EntityUser user = await CreateUser("user@example.com");
        List<EntityMediaFile> fileList = await SeedFileList(user.ID, 450, index => ($"Artist {index % 5} feat. Guest", $"Album {index % 3}", $"Artist {index % 5}", "Rock; Pop"));

        int changed = await _service.Run(TestContext.Current.CancellationToken);

        Assert.Equal(450, changed);
        await using DataContext context = _fixture.CreateContext();
        Assert.Equal(6, await context.Artist.CountAsync(TestContext.Current.CancellationToken));
        Assert.Equal(15, await context.Album.CountAsync(TestContext.Current.CancellationToken));
        Assert.Equal(2, await context.Genre.CountAsync(TestContext.Current.CancellationToken));
        Assert.Equal(900, await context.MediaFileArtist.CountAsync(TestContext.Current.CancellationToken));
        Assert.Equal(900, await context.MediaFileGenre.CountAsync(TestContext.Current.CancellationToken));
        Assert.Equal(0, await context.MediaFileMetadata.CountAsync(x => x.Artist != null || x.Genre != null || x.Album != null || x.AlbumArtist != null, TestContext.Current.CancellationToken));
        EntityMediaFile sample = await Load(fileList[7].ID, user.ID);
        Assert.Equal(["Artist 2", "Guest"], sample.GetArtistNameList());
        Assert.Equal("Album 1", sample.Album?.Title);
        Assert.Equal(["Artist 2"], sample.GetAlbumArtistNameList());
        Assert.Equal(["Rock", "Pop"], sample.GetGenreNameList());
        Assert.Equal("Title 7", sample.Metadata?.Title);
    }

    [Fact]
    public async Task Run_Twice_GivesTheSameItemsAndLinks()
    {
        EntityUser user = await CreateUser("user@example.com");
        await SeedFileList(user.ID, 30, index => ($"Artist {index % 4}", $"Album {index % 2}", $"Artist {index % 4}", "Rock"));

        await _service.Run(TestContext.Current.CancellationToken);
        await using DataContext first = _fixture.CreateContext();
        List<Guid> artistIDList = await first.Artist.Select(x => x.ID).OrderBy(x => x).ToListAsync(TestContext.Current.CancellationToken);
        List<Guid> linkIDList = await first.MediaFileArtist.Select(x => x.ID).OrderBy(x => x).ToListAsync(TestContext.Current.CancellationToken);
        int second = await _service.Run(TestContext.Current.CancellationToken);

        Assert.Equal(0, second);
        await using DataContext context = _fixture.CreateContext();
        Assert.Equal(artistIDList, await context.Artist.Select(x => x.ID).OrderBy(x => x).ToListAsync(TestContext.Current.CancellationToken));
        Assert.Equal(linkIDList, await context.MediaFileArtist.Select(x => x.ID).OrderBy(x => x).ToListAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task Run_TwoUsersWithTheSameText_GetSeparateItems_AndOddTextCreatesNothingBroken()
    {
        EntityUser first = await CreateUser("first@example.com");
        EntityUser second = await CreateUser("second@example.com");
        await SeedFileList(first.ID, 1, _ => ("Radiohead", "OK Computer", null, "Rock"));
        await SeedFileList(second.ID, 1, _ => ("radiohead", "OK Computer", null, "Rock"));
        await SeedFileList(first.ID, 1, _ => (";", " ", "", new string('g', 400)));

        await _service.Run(TestContext.Current.CancellationToken);

        await using DataContext context = _fixture.CreateContext();
        Assert.Equal(2, await context.Artist.CountAsync(TestContext.Current.CancellationToken));
        Assert.Equal(2, await context.Album.CountAsync(TestContext.Current.CancellationToken));
        Assert.Equal(3, await context.Genre.CountAsync(TestContext.Current.CancellationToken));
        Assert.Equal(255, await context.Genre.MaxAsync(x => x.Name.Length, TestContext.Current.CancellationToken));
        Assert.Equal(2, await context.Artist.Select(x => x.UserID).Distinct().CountAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task Run_TheResponseTextOfAPlainFile_IsTheSameBeforeAndAfter()
    {
        EntityUser user = await CreateUser("user@example.com");
        List<EntityMediaFile> fileList = await SeedFileList(user.ID, 1, _ => ("Earth, Wind & Fire", "Greatest Hits", "Earth, Wind & Fire", "R&B/Soul"));
        ResponseMediaFile before = (await Load(fileList[0].ID, user.ID)).ToResponse();

        await _service.Run(TestContext.Current.CancellationToken);

        ResponseMediaFile after = (await Load(fileList[0].ID, user.ID)).ToResponse();
        Assert.Equal((before.Artist, before.Album, before.AlbumArtist, before.Genre), (after.Artist, after.Album, after.AlbumArtist, after.Genre));
        Assert.Single(after.ArtistIDList);
        Assert.NotNull(after.AlbumID);
    }

    [Fact]
    public async Task Run_ConcurrentlyWithAnUploadThatCreatesTheSameArtist_StillMakesOneArtist()
    {
        EntityUser user = await CreateUser("user@example.com");
        await SeedFileList(user.ID, 5, _ => ("Radiohead", null, null, null));
        NameKeyService serviceNameKey = new();
        MetadataResolutionService resolution = new(serviceNameKey, new CreditSplittingService(serviceNameKey));

        Task<int> backfill = _service.Run(TestContext.Current.CancellationToken);
        for (int attempt = 0; attempt < 3; attempt++)
        {
            try
            {
                await using IUnitOfWork uow = _fixture.UoWFactory.Create();
                await resolution.Resolve(uow, user.ID, new(null, ["Radiohead"], null, [], [], null, null, null, null, null, null), TestContext.Current.CancellationToken);
                await uow.Commit(TestContext.Current.CancellationToken);
                break;
            }
            catch (MetadataKeyConflictException)
            {
                // The backfill got there first: the next attempt finds its artist.
            }
        }

        await backfill;

        await using DataContext context = _fixture.CreateContext();
        Assert.Equal(1, await context.Artist.CountAsync(TestContext.Current.CancellationToken));
        Assert.Equal(5, await context.MediaFileArtist.CountAsync(TestContext.Current.CancellationToken));
    }
}
