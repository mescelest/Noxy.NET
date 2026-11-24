using Microsoft.EntityFrameworkCore;
using Noxy.NET.MediaPlayer.Application.Interfaces;
using Noxy.NET.MediaPlayer.Application.Models.Media;
using Noxy.NET.MediaPlayer.Domain.Entities;
using Noxy.NET.MediaPlayer.Domain.Enums;
using Noxy.NET.MediaPlayer.Persistence;
using Noxy.NET.Test.MediaPlayer.Fixtures;

namespace Noxy.NET.Test.MediaPlayer.Media;

public class MediaRepositoryTest : IDisposable
{
    private readonly DatabaseFixture _fixture = new();

    public void Dispose()
    {
        _fixture.Dispose();
    }

    private async Task<EntityUser> CreateUser(string email = "user@example.com")
    {
        EntityUser user = new() { Email = email };

        await using IUnitOfWork uow = _fixture.UoWFactory.Create();
        uow.Authentication.AddUser(user);
        await uow.Commit(TestContext.Current.CancellationToken);

        return user;
    }

    private static EntityMediaFile CreateMediaFile(Guid userID, string title = "Media", DateTime? timeCreated = null)
    {
        EntityMediaFile file = new()
        {
            TimeCreated = timeCreated ?? DateTime.UtcNow,
            FilePath = $"{Guid.CreateVersion7()}.mp3",
            ContentType = "audio/mpeg",
            SizeBytes = 1234,
            ContentHash = new('a', 64),
            UserID = userID
        };

        file.Metadata = new() { Title = title, MediaFileID = file.ID };
        return file;
    }

    private async Task Add(params EntityMediaFile[] files)
    {
        await using IUnitOfWork uow = _fixture.UoWFactory.Create();
        foreach (EntityMediaFile file in files)
        {
            uow.Media.AddMediaFile(file);
        }

        await uow.Commit(TestContext.Current.CancellationToken);
    }


    private async Task<(EntityMediaFile File, EntityArtist Main, EntityArtist Guest, EntityAlbum Album)> AddLinkedFile(Guid userID)
    {
        EntityMediaFile file = CreateMediaFile(userID, "Linked");
        EntityArtist main = new() { UserID = userID, Name = "Main", NameKey = "main" };
        EntityArtist guest = new() { UserID = userID, Name = "Guest", NameKey = "guest" };
        EntityGenre genre = new() { UserID = userID, Name = "Rock", NameKey = "rock" };
        EntityAlbum album = new() { UserID = userID, Title = "Album", TitleKey = "album", IdentityKey = "album|x" };
        album.ArtistLinkList = [new() { AlbumID = album.ID, ArtistID = main.ID, Position = 0 }];
        file.AlbumID = album.ID;

        await using IUnitOfWork uow = _fixture.UoWFactory.Create();
        uow.Metadata.AddArtist(main);
        uow.Metadata.AddArtist(guest);
        uow.Metadata.AddGenre(genre);
        uow.Metadata.AddAlbum(album);
        uow.Media.AddMediaFile(file);
        uow.Metadata.AddMediaFileArtist(new() { MediaFileID = file.ID, ArtistID = guest.ID, Position = 1 });
        uow.Metadata.AddMediaFileArtist(new() { MediaFileID = file.ID, ArtistID = main.ID, Position = 0 });
        uow.Metadata.AddMediaFileGenre(new() { MediaFileID = file.ID, GenreID = genre.ID, Position = 0 });
        await uow.Commit(TestContext.Current.CancellationToken);

        return (file, main, guest, album);
    }

    [Fact]
    public async Task Find_LoadsTheLinksInPositionOrderAndTheAlbumWithItsArtists()
    {
        EntityUser user = await CreateUser();
        (EntityMediaFile file, EntityArtist main, EntityArtist guest, EntityAlbum album) = await AddLinkedFile(user.ID);

        await using IUnitOfWork uow = _fixture.UoWFactory.Create();
        EntityMediaFile? found = await uow.Media.FindMediaFile(file.ID, user.ID, TestContext.Current.CancellationToken);

        Assert.NotNull(found);
        Assert.Equal([main.ID, guest.ID], found.ArtistLinkList.Select(x => x.ArtistID).ToList());
        Assert.Equal(["Main", "Guest"], found.ArtistLinkList.Select(x => x.Artist?.Name).ToList());
        Assert.Equal("Rock", Assert.Single(found.GenreLinkList).Genre?.Name);
        Assert.Equal(album.ID, found.AlbumID);
        Assert.Equal("Album", found.Album?.Title);
        Assert.Equal("Main", Assert.Single(found.Album?.ArtistLinkList ?? []).Artist?.Name);
    }

    [Fact]
    public async Task GetMediaFileList_LoadsTheLinks()
    {
        EntityUser user = await CreateUser();
        await AddLinkedFile(user.ID);

        await using IUnitOfWork uow = _fixture.UoWFactory.Create();
        EntityMediaFile found = Assert.Single(await uow.Media.GetMediaFileList(user.ID, TestContext.Current.CancellationToken));
        EntityMediaFile paged = Assert.Single(await uow.Media.GetMediaFileList(user.ID, 0, 10, TestContext.Current.CancellationToken));

        Assert.Equal(2, found.ArtistLinkList.Count);
        Assert.Equal(2, paged.ArtistLinkList.Count);
        Assert.Equal("Album", paged.Album?.Title);
    }

    [Fact]
    public async Task Add_AFileWithoutLinks_LoadsEmptyLists()
    {
        EntityUser user = await CreateUser();
        EntityMediaFile file = CreateMediaFile(user.ID);
        await Add(file);

        await using IUnitOfWork uow = _fixture.UoWFactory.Create();
        EntityMediaFile? found = await uow.Media.FindMediaFile(file.ID, user.ID, TestContext.Current.CancellationToken);

        Assert.NotNull(found);
        Assert.Empty(found.ArtistLinkList);
        Assert.Empty(found.GenreLinkList);
        Assert.Null(found.Album);
        Assert.Null(found.AlbumID);
    }

    [Fact]
    public async Task Add_ThenFind_ReturnsTheFileWithItsMetadata()
    {
        EntityUser user = await CreateUser();
        EntityMediaFile file = CreateMediaFile(user.ID, "My Media");
        Assert.NotNull(file.Metadata);
        file.Metadata.Artist = "An Artist";
        file.Metadata.Year = 1999;
        file.Metadata.Duration = TimeSpan.FromSeconds(215);
        await Add(file);

        await using IUnitOfWork uow = _fixture.UoWFactory.Create();
        EntityMediaFile? found = await uow.Media.FindMediaFile(file.ID, user.ID, TestContext.Current.CancellationToken);

        Assert.NotNull(found);
        Assert.Equal(file.FilePath, found.FilePath);
        Assert.Equal("audio/mpeg", found.ContentType);
        Assert.Equal(1234, found.SizeBytes);
        Assert.NotNull(found.Metadata);
        Assert.Equal("My Media", found.Metadata.Title);
        Assert.Equal("An Artist", found.Metadata.Artist);
        Assert.Equal(1999, found.Metadata.Year);
        Assert.Equal(TimeSpan.FromSeconds(215), found.Metadata.Duration);
        Assert.Null(found.Metadata.Album);
    }

    [Fact]
    public async Task Find_AnotherUsersFile_ReturnsNull()
    {
        EntityUser owner = await CreateUser("owner@example.com");
        EntityUser other = await CreateUser("other@example.com");
        EntityMediaFile file = CreateMediaFile(owner.ID);
        await Add(file);

        await using IUnitOfWork uow = _fixture.UoWFactory.Create();

        Assert.Null(await uow.Media.FindMediaFile(file.ID, other.ID, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task Find_UnknownID_ReturnsNull()
    {
        EntityUser user = await CreateUser();

        await using IUnitOfWork uow = _fixture.UoWFactory.Create();

        Assert.Null(await uow.Media.FindMediaFile(Guid.CreateVersion7(), user.ID, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task GetMediaFileList_ReturnsOnlyTheUsersFiles_NewestFirst()
    {
        EntityUser user = await CreateUser();
        EntityUser other = await CreateUser("other@example.com");
        DateTime now = new(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);
        await Add(
            CreateMediaFile(user.ID, "Oldest", now.AddMinutes(-2)),
            CreateMediaFile(user.ID, "Newest", now),
            CreateMediaFile(user.ID, "Middle", now.AddMinutes(-1)),
            CreateMediaFile(other.ID, "Someone else's", now));

        await using IUnitOfWork uow = _fixture.UoWFactory.Create();
        List<EntityMediaFile> list = await uow.Media.GetMediaFileList(user.ID, 0, 10, TestContext.Current.CancellationToken);

        Assert.Equal(["Newest", "Middle", "Oldest"], list.Select(x => x.Metadata?.Title));
    }

    [Fact]
    public async Task GetMediaFileList_PagesWithSkipAndTake()
    {
        EntityUser user = await CreateUser();
        DateTime now = new(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);
        await Add(Enumerable.Range(0, 5).Select(i => CreateMediaFile(user.ID, $"Media {i}", now.AddMinutes(i))).ToArray());

        await using IUnitOfWork uow = _fixture.UoWFactory.Create();
        List<EntityMediaFile> page = await uow.Media.GetMediaFileList(user.ID, 1, 2, TestContext.Current.CancellationToken);

        Assert.Equal(["Media 3", "Media 2"], page.Select(x => x.Metadata?.Title));
    }

    [Fact]
    public async Task Add_ThenFind_KeepsTheHashAndModifiedTime()
    {
        EntityUser user = await CreateUser();
        EntityMediaFile file = CreateMediaFile(user.ID);
        file.ContentHash = new('b', 64);
        file.TimeModified = new(2026, 1, 2, 3, 4, 5, DateTimeKind.Utc);
        await Add(file);

        await using IUnitOfWork uow = _fixture.UoWFactory.Create();
        EntityMediaFile? found = await uow.Media.FindMediaFile(file.ID, user.ID, TestContext.Current.CancellationToken);

        Assert.Equal(new('b', 64), found?.ContentHash);
        Assert.Equal(file.TimeModified, found?.TimeModified);
    }

    [Fact]
    public async Task CountAndSum_CoverOnlyTheUsersFiles()
    {
        EntityUser user = await CreateUser();
        EntityUser other = await CreateUser("other@example.com");
        EntityMediaFile first = CreateMediaFile(user.ID);
        first.SizeBytes = 1000;
        EntityMediaFile second = CreateMediaFile(user.ID);
        second.SizeBytes = 2500;
        await Add(first, second, CreateMediaFile(other.ID));

        await using IUnitOfWork uow = _fixture.UoWFactory.Create();

        Assert.Equal(2, await uow.Media.CountMediaFiles(user.ID, TestContext.Current.CancellationToken));
        Assert.Equal(3500, await uow.Media.SumMediaFileBytes(user.ID, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task FindMediaFileByHash_FindsOnlyTheUsersOwnFileWithThatHash()
    {
        EntityUser user = await CreateUser();
        EntityUser other = await CreateUser("other@example.com");
        EntityMediaFile mine = CreateMediaFile(user.ID);
        mine.ContentHash = new('a', 64);
        EntityMediaFile theirs = CreateMediaFile(other.ID);
        theirs.ContentHash = new('f', 64);
        await Add(mine, theirs);

        await using IUnitOfWork uow = _fixture.UoWFactory.Create();

        Assert.Equal(mine.ID, (await uow.Media.FindMediaFileByHash(user.ID, new('a', 64), TestContext.Current.CancellationToken))?.ID);
        Assert.Null(await uow.Media.FindMediaFileByHash(user.ID, new('f', 64), TestContext.Current.CancellationToken));
        Assert.Null(await uow.Media.FindMediaFileByHash(user.ID, new('0', 64), TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task CountAndSum_NoFiles_AreZero()
    {
        EntityUser user = await CreateUser();

        await using IUnitOfWork uow = _fixture.UoWFactory.Create();

        Assert.Equal(0, await uow.Media.CountMediaFiles(user.ID, TestContext.Current.CancellationToken));
        Assert.Equal(0, await uow.Media.SumMediaFileBytes(user.ID, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task Add_DuplicateFilePath_Throws()
    {
        EntityUser user = await CreateUser();
        EntityMediaFile first = CreateMediaFile(user.ID);
        await Add(first);

        EntityMediaFile duplicate = CreateMediaFile(user.ID);
        duplicate.FilePath = first.FilePath;

        await Assert.ThrowsAsync<DbUpdateException>(() => Add(duplicate));
    }

    [Fact]
    public async Task Add_ForAnUnknownUser_Throws()
    {
        await Assert.ThrowsAsync<DbUpdateException>(() => Add(CreateMediaFile(Guid.CreateVersion7())));
    }

    private static EntityMediaUploadJob CreateJob(Guid userID, DateTime? timeCreated = null)
    {
        return new()
        {
            TimeCreated = timeCreated ?? DateTime.UtcNow,
            UserID = userID,
            StagedFileName = $"{Guid.CreateVersion7()}.mp3",
            Title = "Media",
            ContentType = "audio/mpeg",
            SizeBytes = 100,
            ContentHash = new('c', 64)
        };
    }

    private async Task AddJobs(params EntityMediaUploadJob[] jobs)
    {
        await using IUnitOfWork uow = _fixture.UoWFactory.Create();
        foreach (EntityMediaUploadJob job in jobs)
        {
            uow.Media.AddMediaUploadJob(job);
        }

        await uow.Commit(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task AddMediaUploadJob_ThenFindNext_ReturnsTheOldestPending()
    {
        EntityUser user = await CreateUser();
        DateTime now = DateTime.UtcNow;
        EntityMediaUploadJob newer = CreateJob(user.ID, now);
        EntityMediaUploadJob older = CreateJob(user.ID, now.AddMinutes(-5));
        await AddJobs(newer, older);

        await using IUnitOfWork uow = _fixture.UoWFactory.Create();
        EntityMediaUploadJob? next = await uow.Media.FindNextPendingMediaUploadJob(TestContext.Current.CancellationToken);

        Assert.Equal(older.ID, next?.ID);
        Assert.Equal(older.StagedFileName, next?.StagedFileName);
        Assert.Equal("Media", next?.Title);
        Assert.Equal(MediaUploadJobStatusEnum.Pending, next?.Status);
    }

    [Fact]
    public async Task FindNext_NoJobs_ReturnsNull()
    {
        await using IUnitOfWork uow = _fixture.UoWFactory.Create();

        Assert.Null(await uow.Media.FindNextPendingMediaUploadJob(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task FindNext_SkipsProcessingJobs()
    {
        EntityUser user = await CreateUser();
        DateTime now = DateTime.UtcNow;
        EntityMediaUploadJob first = CreateJob(user.ID, now.AddMinutes(-5));
        EntityMediaUploadJob second = CreateJob(user.ID, now);
        await AddJobs(first, second);

        await using IUnitOfWork uow = _fixture.UoWFactory.Create();
        await uow.Media.MarkMediaUploadJobProcessing(first.ID, TestContext.Current.CancellationToken);
        EntityMediaUploadJob? next = await uow.Media.FindNextPendingMediaUploadJob(TestContext.Current.CancellationToken);

        Assert.Equal(second.ID, next?.ID);
    }

    [Fact]
    public async Task MarkProcessing_SecondCallReturnsFalse()
    {
        EntityUser user = await CreateUser();
        EntityMediaUploadJob job = CreateJob(user.ID);
        await AddJobs(job);

        await using IUnitOfWork uow = _fixture.UoWFactory.Create();

        Assert.True(await uow.Media.MarkMediaUploadJobProcessing(job.ID, TestContext.Current.CancellationToken));
        Assert.False(await uow.Media.MarkMediaUploadJobProcessing(job.ID, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task CountMediaUploadJobs_CoversOnlyTheUsersJobs()
    {
        EntityUser user = await CreateUser();
        EntityUser other = await CreateUser("other@example.com");
        await AddJobs(CreateJob(user.ID), CreateJob(user.ID), CreateJob(other.ID));

        await using IUnitOfWork uow = _fixture.UoWFactory.Create();

        Assert.Equal(2, await uow.Media.CountMediaUploadJobs(user.ID, TestContext.Current.CancellationToken));
        Assert.Equal(1, await uow.Media.CountMediaUploadJobs(other.ID, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task ResetProcessing_MakesThemPendingAgain()
    {
        EntityUser user = await CreateUser();
        EntityMediaUploadJob job = CreateJob(user.ID);
        EntityMediaUploadJob untouched = CreateJob(user.ID, DateTime.UtcNow.AddMinutes(1));
        await AddJobs(job, untouched);

        await using IUnitOfWork uow = _fixture.UoWFactory.Create();
        await uow.Media.MarkMediaUploadJobProcessing(job.ID, TestContext.Current.CancellationToken);

        int reset = await uow.Media.ResetProcessingMediaUploadJobs(TestContext.Current.CancellationToken);
        EntityMediaUploadJob? next = await uow.Media.FindNextPendingMediaUploadJob(TestContext.Current.CancellationToken);

        Assert.Equal(1, reset);
        Assert.Equal(job.ID, next?.ID);
    }

    [Fact]
    public async Task RemoveMediaUploadJob_InTheSameCommitAsAddMediaFile_AppliesBoth()
    {
        EntityUser user = await CreateUser();
        EntityMediaUploadJob job = CreateJob(user.ID);
        await AddJobs(job);
        EntityMediaFile file = CreateMediaFile(user.ID);

        await using (IUnitOfWork uow = _fixture.UoWFactory.Create())
        {
            uow.Media.AddMediaFile(file);
            uow.Media.RemoveMediaUploadJob(job);
            await uow.Commit(TestContext.Current.CancellationToken);
        }

        await using IUnitOfWork check = _fixture.UoWFactory.Create();
        Assert.NotNull(await check.Media.FindMediaFile(file.ID, user.ID, TestContext.Current.CancellationToken));
        Assert.Equal(0, await check.Media.CountMediaUploadJobs(user.ID, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task SumMediaUploadJobBytes_CoversOnlyTheUsersJobs()
    {
        EntityUser user = await CreateUser();
        EntityUser other = await CreateUser("other@example.com");
        EntityMediaUploadJob first = CreateJob(user.ID);
        first.SizeBytes = 1000;
        EntityMediaUploadJob second = CreateJob(user.ID);
        second.SizeBytes = 2500;
        await AddJobs(first, second, CreateJob(other.ID));

        await using IUnitOfWork uow = _fixture.UoWFactory.Create();

        Assert.Equal(3500, await uow.Media.SumMediaUploadJobBytes(user.ID, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task SumMediaUploadJobBytes_NoJobs_IsZero()
    {
        EntityUser user = await CreateUser();

        await using IUnitOfWork uow = _fixture.UoWFactory.Create();

        Assert.Equal(0, await uow.Media.SumMediaUploadJobBytes(user.ID, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task GetMediaUploadJobFileNameList_ReturnsAllStagedNames()
    {
        EntityUser user = await CreateUser();
        EntityUser other = await CreateUser("other@example.com");
        EntityMediaUploadJob first = CreateJob(user.ID);
        EntityMediaUploadJob second = CreateJob(other.ID);
        await AddJobs(first, second);

        await using IUnitOfWork uow = _fixture.UoWFactory.Create();
        List<string> names = await uow.Media.GetMediaUploadJobFileNameList(TestContext.Current.CancellationToken);

        // Both names are made in the same millisecond, so which one sorts first is not fixed.
        Assert.Equal(new[] { first.StagedFileName, second.StagedFileName }.Order(), names.Order());
    }

    [Fact]
    public async Task AddMediaUploadJob_ForAnUnknownUser_Throws()
    {
        await Assert.ThrowsAsync<DbUpdateException>(() => AddJobs(CreateJob(Guid.CreateVersion7())));
    }

    [Fact]
    public async Task GetMediaFileList_ReturnsEveryFileOfTheUserNewestFirst()
    {
        EntityUser user = await CreateUser();
        EntityUser other = await CreateUser("other@example.com");
        DateTime now = DateTime.UtcNow;
        await Add(CreateMediaFile(user.ID, "Oldest", now.AddMinutes(-3)), CreateMediaFile(user.ID, "Newest", now), CreateMediaFile(user.ID, "Middle", now.AddMinutes(-1)), CreateMediaFile(other.ID, "Not mine", now));

        await using IUnitOfWork uow = _fixture.UoWFactory.Create();
        List<EntityMediaFile> list = await uow.Media.GetMediaFileList(user.ID, TestContext.Current.CancellationToken);

        Assert.Equal(["Newest", "Middle", "Oldest"], list.Select(x => x.Metadata?.Title));
    }

    [Fact]
    public async Task GetMediaFileStampList_ReturnsIdAndModifiedTimeOfOnlyTheUsersFiles()
    {
        EntityUser user = await CreateUser();
        EntityUser other = await CreateUser("other@example.com");
        EntityMediaFile first = CreateMediaFile(user.ID);
        first.TimeModified = new(2026, 1, 2, 3, 4, 5, DateTimeKind.Utc);
        EntityMediaFile second = CreateMediaFile(user.ID);
        second.TimeModified = new(2026, 2, 3, 4, 5, 6, DateTimeKind.Utc);
        await Add(first, second, CreateMediaFile(other.ID));

        await using IUnitOfWork uow = _fixture.UoWFactory.Create();
        List<MediaFileStamp> stamps = await uow.Media.GetMediaFileStampList(user.ID, TestContext.Current.CancellationToken);

        Assert.Equal(2, stamps.Count);
        Assert.Equal(first.TimeModified, stamps.Single(x => x.ID == first.ID).TimeModified);
        Assert.Equal(second.TimeModified, stamps.Single(x => x.ID == second.ID).TimeModified);
    }

    [Fact]
    public async Task UpdateMediaFile_SavesTagsAndTimeModifiedTogether()
    {
        EntityUser user = await CreateUser();
        EntityMediaFile file = CreateMediaFile(user.ID, "Before");
        await Add(file);

        await using (IUnitOfWork uow = _fixture.UoWFactory.Create())
        {
            EntityMediaFile loaded = await uow.Media.FindMediaFile(file.ID, user.ID, TestContext.Current.CancellationToken) ?? throw new InvalidOperationException("The media file was not found.");
            Assert.NotNull(loaded.Metadata);
            loaded.Metadata.Title = "After";
            loaded.Metadata.Artist = "Someone";
            loaded.Metadata.Year = 2001;
            loaded.TimeModified = new(2030, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            uow.Media.UpdateMediaFile(loaded);
            await uow.Commit(TestContext.Current.CancellationToken);
        }

        await using IUnitOfWork check = _fixture.UoWFactory.Create();
        EntityMediaFile saved = await check.Media.FindMediaFile(file.ID, user.ID, TestContext.Current.CancellationToken) ?? throw new InvalidOperationException("The media file was not found.");
        Assert.Equal("After", saved.Metadata?.Title);
        Assert.Equal("Someone", saved.Metadata?.Artist);
        Assert.Equal(2001, saved.Metadata?.Year);
        Assert.Equal(new(2030, 1, 1, 0, 0, 0, DateTimeKind.Utc), saved.TimeModified);
        Assert.Equal(file.FilePath, saved.FilePath);
    }

    [Fact]
    public async Task UpdateMediaFile_NotCommitted_ChangesNothing()
    {
        EntityUser user = await CreateUser();
        EntityMediaFile file = CreateMediaFile(user.ID, "Before");
        await Add(file);

        await using (IUnitOfWork uow = _fixture.UoWFactory.Create())
        {
            EntityMediaFile loaded = await uow.Media.FindMediaFile(file.ID, user.ID, TestContext.Current.CancellationToken) ?? throw new InvalidOperationException("The media file was not found.");
            Assert.NotNull(loaded.Metadata);
            loaded.Metadata.Title = "After";
            uow.Media.UpdateMediaFile(loaded);
        }

        await using IUnitOfWork check = _fixture.UoWFactory.Create();
        Assert.Equal("Before", (await check.Media.FindMediaFile(file.ID, user.ID, TestContext.Current.CancellationToken))?.Metadata?.Title);
    }

    [Fact]
    public async Task RemoveMediaFile_DeletesTheFileAndItsMetadata()
    {
        EntityUser user = await CreateUser();
        EntityMediaFile file = CreateMediaFile(user.ID);
        EntityMediaFile kept = CreateMediaFile(user.ID);
        await Add(file, kept);

        await using (IUnitOfWork uow = _fixture.UoWFactory.Create())
        {
            EntityMediaFile loaded = await uow.Media.FindMediaFile(file.ID, user.ID, TestContext.Current.CancellationToken) ?? throw new InvalidOperationException("The media file was not found.");
            uow.Media.RemoveMediaFile(loaded);
            await uow.Commit(TestContext.Current.CancellationToken);
        }

        await using IUnitOfWork check = _fixture.UoWFactory.Create();
        Assert.Null(await check.Media.FindMediaFile(file.ID, user.ID, TestContext.Current.CancellationToken));
        Assert.NotNull(await check.Media.FindMediaFile(kept.ID, user.ID, TestContext.Current.CancellationToken));
        await using DataContext context = _fixture.CreateContext();
        Assert.Equal(0, await context.MediaFileMetadata.CountAsync(x => x.MediaFileID == file.ID, TestContext.Current.CancellationToken));
        Assert.Equal(1, await context.MediaFileMetadata.CountAsync(x => x.MediaFileID == kept.ID, TestContext.Current.CancellationToken));
    }
}
