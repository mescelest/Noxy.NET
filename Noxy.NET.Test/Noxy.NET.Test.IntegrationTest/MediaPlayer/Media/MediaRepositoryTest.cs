using Microsoft.EntityFrameworkCore;
using Noxy.NET.MediaPlayer.Application.Interfaces;
using Noxy.NET.MediaPlayer.Domain.Entities;
using Noxy.NET.Test.MediaPlayer.Fixtures;

namespace Noxy.NET.Test.MediaPlayer.Media;

public class MediaRepositoryTest : IDisposable
{
    private readonly DatabaseFixture _fixture = new();

    public void Dispose()
    {
        _fixture.Dispose();
    }

    private async Task<User> CreateUser(string email = "user@example.com")
    {
        User user = new() { Email = email };

        await using IUnitOfWork uow = _fixture.UoWFactory.Create();
        uow.Authentication.AddUser(user);
        await uow.Commit(TestContext.Current.CancellationToken);

        return user;
    }

    private static MediaFile CreateMediaFile(Guid userID, string title = "Song", DateTime? timeCreated = null)
    {
        MediaFile file = new()
        {
            TimeCreated = timeCreated ?? DateTime.UtcNow,
            FilePath = $"{Guid.CreateVersion7()}.mp3",
            ContentType = "audio/mpeg",
            SizeBytes = 1234,
            UserID = userID
        };

        file.Metadata = new() { Title = title, MediaFileID = file.ID };
        return file;
    }

    private async Task Add(params MediaFile[] files)
    {
        await using IUnitOfWork uow = _fixture.UoWFactory.Create();
        foreach (MediaFile file in files)
        {
            uow.Media.AddMediaFile(file);
        }

        await uow.Commit(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task Add_ThenFind_ReturnsTheFileWithItsMetadata()
    {
        User user = await CreateUser();
        MediaFile file = CreateMediaFile(user.ID, "My Song");
        file.Metadata!.Artist = "An Artist";
        file.Metadata.Year = 1999;
        file.Metadata.Duration = TimeSpan.FromSeconds(215);
        await Add(file);

        await using IUnitOfWork uow = _fixture.UoWFactory.Create();
        MediaFile? found = await uow.Media.FindMediaFile(file.ID, user.ID, TestContext.Current.CancellationToken);

        Assert.NotNull(found);
        Assert.Equal(file.FilePath, found.FilePath);
        Assert.Equal("audio/mpeg", found.ContentType);
        Assert.Equal(1234, found.SizeBytes);
        Assert.NotNull(found.Metadata);
        Assert.Equal("My Song", found.Metadata.Title);
        Assert.Equal("An Artist", found.Metadata.Artist);
        Assert.Equal(1999, found.Metadata.Year);
        Assert.Equal(TimeSpan.FromSeconds(215), found.Metadata.Duration);
        Assert.Null(found.Metadata.Album);
    }

    [Fact]
    public async Task Find_AnotherUsersFile_ReturnsNull()
    {
        User owner = await CreateUser("owner@example.com");
        User other = await CreateUser("other@example.com");
        MediaFile file = CreateMediaFile(owner.ID);
        await Add(file);

        await using IUnitOfWork uow = _fixture.UoWFactory.Create();

        Assert.Null(await uow.Media.FindMediaFile(file.ID, other.ID, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task Find_UnknownID_ReturnsNull()
    {
        User user = await CreateUser();

        await using IUnitOfWork uow = _fixture.UoWFactory.Create();

        Assert.Null(await uow.Media.FindMediaFile(Guid.CreateVersion7(), user.ID, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task List_ReturnsOnlyTheUsersFiles_NewestFirst()
    {
        User user = await CreateUser("user@example.com");
        User other = await CreateUser("other@example.com");
        DateTime now = new(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);
        await Add(
            CreateMediaFile(user.ID, "Oldest", now.AddMinutes(-2)),
            CreateMediaFile(user.ID, "Newest", now),
            CreateMediaFile(user.ID, "Middle", now.AddMinutes(-1)),
            CreateMediaFile(other.ID, "Someone else's", now));

        await using IUnitOfWork uow = _fixture.UoWFactory.Create();
        List<MediaFile> list = await uow.Media.ListMediaFiles(user.ID, 0, 10, TestContext.Current.CancellationToken);

        Assert.Equal(["Newest", "Middle", "Oldest"], list.Select(x => x.Metadata!.Title));
    }

    [Fact]
    public async Task List_PagesWithSkipAndTake()
    {
        User user = await CreateUser();
        DateTime now = new(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);
        await Add(Enumerable.Range(0, 5).Select(i => CreateMediaFile(user.ID, $"Song {i}", now.AddMinutes(i))).ToArray());

        await using IUnitOfWork uow = _fixture.UoWFactory.Create();
        List<MediaFile> page = await uow.Media.ListMediaFiles(user.ID, 1, 2, TestContext.Current.CancellationToken);

        Assert.Equal(["Song 3", "Song 2"], page.Select(x => x.Metadata!.Title));
    }

    [Fact]
    public async Task Add_DuplicateFilePath_Throws()
    {
        User user = await CreateUser();
        MediaFile first = CreateMediaFile(user.ID);
        await Add(first);

        MediaFile duplicate = CreateMediaFile(user.ID);
        duplicate.FilePath = first.FilePath;

        await Assert.ThrowsAsync<DbUpdateException>(() => Add(duplicate));
    }

    [Fact]
    public async Task Add_ForAnUnknownUser_Throws()
    {
        await Assert.ThrowsAsync<DbUpdateException>(() => Add(CreateMediaFile(Guid.CreateVersion7())));
    }
}
