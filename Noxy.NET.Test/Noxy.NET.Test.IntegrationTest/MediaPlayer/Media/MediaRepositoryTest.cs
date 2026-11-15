using Microsoft.EntityFrameworkCore;
using Noxy.NET.MediaPlayer.Application.Interfaces;
using Noxy.NET.MediaPlayer.Application.Models;
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

    private async Task<User> CreateUser(string email = "user@example.com")
    {
        User user = new() { Email = email };

        await using IUnitOfWork uow = _fixture.UoWFactory.Create();
        uow.Authentication.AddUser(user);
        await uow.Commit(TestContext.Current.CancellationToken);

        return user;
    }

    private static MediaFile CreateMediaFile(Guid userID, string title = "Media", DateTime? timeCreated = null)
    {
        MediaFile file = new()
        {
            TimeCreated = timeCreated ?? DateTime.UtcNow,
            FilePath = $"{Guid.CreateVersion7()}.mp3",
            ContentType = "audio/mpeg",
            SizeBytes = 1234,
            ContentHash = new string('a', 64),
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
        MediaFile file = CreateMediaFile(user.ID, "My Media");
        Assert.NotNull(file.Metadata);
        file.Metadata.Artist = "An Artist";
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
        Assert.Equal("My Media", found.Metadata.Title);
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
    public async Task GetMediaFileList_ReturnsOnlyTheUsersFiles_NewestFirst()
    {
        User user = await CreateUser();
        User other = await CreateUser("other@example.com");
        DateTime now = new(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);
        await Add(
            CreateMediaFile(user.ID, "Oldest", now.AddMinutes(-2)),
            CreateMediaFile(user.ID, "Newest", now),
            CreateMediaFile(user.ID, "Middle", now.AddMinutes(-1)),
            CreateMediaFile(other.ID, "Someone else's", now));

        await using IUnitOfWork uow = _fixture.UoWFactory.Create();
        List<MediaFile> list = await uow.Media.GetMediaFileList(user.ID, 0, 10, TestContext.Current.CancellationToken);

        Assert.Equal(["Newest", "Middle", "Oldest"], list.Select(x => x.Metadata?.Title));
    }

    [Fact]
    public async Task GetMediaFileList_PagesWithSkipAndTake()
    {
        User user = await CreateUser();
        DateTime now = new(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);
        await Add(Enumerable.Range(0, 5).Select(i => CreateMediaFile(user.ID, $"Media {i}", now.AddMinutes(i))).ToArray());

        await using IUnitOfWork uow = _fixture.UoWFactory.Create();
        List<MediaFile> page = await uow.Media.GetMediaFileList(user.ID, 1, 2, TestContext.Current.CancellationToken);

        Assert.Equal(["Media 3", "Media 2"], page.Select(x => x.Metadata?.Title));
    }

    [Fact]
    public async Task Add_ThenFind_KeepsTheHashAndModifiedTime()
    {
        User user = await CreateUser();
        MediaFile file = CreateMediaFile(user.ID);
        file.ContentHash = new string('b', 64);
        file.TimeModified = new(2026, 1, 2, 3, 4, 5, DateTimeKind.Utc);
        await Add(file);

        await using IUnitOfWork uow = _fixture.UoWFactory.Create();
        MediaFile? found = await uow.Media.FindMediaFile(file.ID, user.ID, TestContext.Current.CancellationToken);

        Assert.Equal(new string('b', 64), found?.ContentHash);
        Assert.Equal(file.TimeModified, found?.TimeModified);
    }

    [Fact]
    public async Task CountAndSum_CoverOnlyTheUsersFiles()
    {
        User user = await CreateUser();
        User other = await CreateUser("other@example.com");
        MediaFile first = CreateMediaFile(user.ID);
        first.SizeBytes = 1000;
        MediaFile second = CreateMediaFile(user.ID);
        second.SizeBytes = 2500;
        await Add(first, second, CreateMediaFile(other.ID));

        await using IUnitOfWork uow = _fixture.UoWFactory.Create();

        Assert.Equal(2, await uow.Media.CountMediaFiles(user.ID, TestContext.Current.CancellationToken));
        Assert.Equal(3500, await uow.Media.SumMediaFileBytes(user.ID, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task CountAndSum_NoFiles_AreZero()
    {
        User user = await CreateUser();

        await using IUnitOfWork uow = _fixture.UoWFactory.Create();

        Assert.Equal(0, await uow.Media.CountMediaFiles(user.ID, TestContext.Current.CancellationToken));
        Assert.Equal(0, await uow.Media.SumMediaFileBytes(user.ID, TestContext.Current.CancellationToken));
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

    private static MediaUploadJob CreateJob(Guid userID, DateTime? timeCreated = null)
    {
        return new()
        {
            TimeCreated = timeCreated ?? DateTime.UtcNow,
            UserID = userID,
            StagedFileName = $"{Guid.CreateVersion7()}.mp3",
            Title = "Media",
            ContentType = "audio/mpeg",
            SizeBytes = 100,
            ContentHash = new string('c', 64)
        };
    }

    private async Task AddJobs(params MediaUploadJob[] jobs)
    {
        await using IUnitOfWork uow = _fixture.UoWFactory.Create();
        foreach (MediaUploadJob job in jobs)
        {
            uow.Media.AddMediaUploadJob(job);
        }

        await uow.Commit(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task AddMediaUploadJob_ThenFindNext_ReturnsTheOldestPending()
    {
        User user = await CreateUser();
        DateTime now = DateTime.UtcNow;
        MediaUploadJob newer = CreateJob(user.ID, now);
        MediaUploadJob older = CreateJob(user.ID, now.AddMinutes(-5));
        await AddJobs(newer, older);

        await using IUnitOfWork uow = _fixture.UoWFactory.Create();
        MediaUploadJob? next = await uow.Media.FindNextPendingMediaUploadJob(TestContext.Current.CancellationToken);

        Assert.Equal(older.ID, next?.ID);
        Assert.Equal(older.StagedFileName, next?.StagedFileName);
        Assert.Equal("Media", next?.Title);
        Assert.Equal(MediaUploadJobStatus.Pending, next?.Status);
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
        User user = await CreateUser();
        DateTime now = DateTime.UtcNow;
        MediaUploadJob first = CreateJob(user.ID, now.AddMinutes(-5));
        MediaUploadJob second = CreateJob(user.ID, now);
        await AddJobs(first, second);

        await using IUnitOfWork uow = _fixture.UoWFactory.Create();
        await uow.Media.MarkMediaUploadJobProcessing(first.ID, TestContext.Current.CancellationToken);
        MediaUploadJob? next = await uow.Media.FindNextPendingMediaUploadJob(TestContext.Current.CancellationToken);

        Assert.Equal(second.ID, next?.ID);
    }

    [Fact]
    public async Task MarkProcessing_SecondCallReturnsFalse()
    {
        User user = await CreateUser();
        MediaUploadJob job = CreateJob(user.ID);
        await AddJobs(job);

        await using IUnitOfWork uow = _fixture.UoWFactory.Create();

        Assert.True(await uow.Media.MarkMediaUploadJobProcessing(job.ID, TestContext.Current.CancellationToken));
        Assert.False(await uow.Media.MarkMediaUploadJobProcessing(job.ID, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task CountMediaUploadJobs_CoversOnlyTheUsersJobs()
    {
        User user = await CreateUser();
        User other = await CreateUser("other@example.com");
        await AddJobs(CreateJob(user.ID), CreateJob(user.ID), CreateJob(other.ID));

        await using IUnitOfWork uow = _fixture.UoWFactory.Create();

        Assert.Equal(2, await uow.Media.CountMediaUploadJobs(user.ID, TestContext.Current.CancellationToken));
        Assert.Equal(1, await uow.Media.CountMediaUploadJobs(other.ID, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task ResetProcessing_MakesThemPendingAgain()
    {
        User user = await CreateUser();
        MediaUploadJob job = CreateJob(user.ID);
        MediaUploadJob untouched = CreateJob(user.ID, DateTime.UtcNow.AddMinutes(1));
        await AddJobs(job, untouched);

        await using IUnitOfWork uow = _fixture.UoWFactory.Create();
        await uow.Media.MarkMediaUploadJobProcessing(job.ID, TestContext.Current.CancellationToken);

        int reset = await uow.Media.ResetProcessingMediaUploadJobs(TestContext.Current.CancellationToken);
        MediaUploadJob? next = await uow.Media.FindNextPendingMediaUploadJob(TestContext.Current.CancellationToken);

        Assert.Equal(1, reset);
        Assert.Equal(job.ID, next?.ID);
    }

    [Fact]
    public async Task RemoveMediaUploadJob_InTheSameCommitAsAddMediaFile_AppliesBoth()
    {
        User user = await CreateUser();
        MediaUploadJob job = CreateJob(user.ID);
        await AddJobs(job);
        MediaFile file = CreateMediaFile(user.ID);

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
        User user = await CreateUser();
        User other = await CreateUser("other@example.com");
        MediaUploadJob first = CreateJob(user.ID);
        first.SizeBytes = 1000;
        MediaUploadJob second = CreateJob(user.ID);
        second.SizeBytes = 2500;
        await AddJobs(first, second, CreateJob(other.ID));

        await using IUnitOfWork uow = _fixture.UoWFactory.Create();

        Assert.Equal(3500, await uow.Media.SumMediaUploadJobBytes(user.ID, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task SumMediaUploadJobBytes_NoJobs_IsZero()
    {
        User user = await CreateUser();

        await using IUnitOfWork uow = _fixture.UoWFactory.Create();

        Assert.Equal(0, await uow.Media.SumMediaUploadJobBytes(user.ID, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task GetMediaUploadJobFileNameList_ReturnsAllStagedNames()
    {
        User user = await CreateUser();
        User other = await CreateUser("other@example.com");
        MediaUploadJob first = CreateJob(user.ID);
        MediaUploadJob second = CreateJob(other.ID);
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
        User user = await CreateUser();
        User other = await CreateUser("other@example.com");
        DateTime now = DateTime.UtcNow;
        await Add(CreateMediaFile(user.ID, "Oldest", now.AddMinutes(-3)), CreateMediaFile(user.ID, "Newest", now), CreateMediaFile(user.ID, "Middle", now.AddMinutes(-1)), CreateMediaFile(other.ID, "Not mine", now));

        await using IUnitOfWork uow = _fixture.UoWFactory.Create();
        List<MediaFile> list = await uow.Media.GetMediaFileList(user.ID, TestContext.Current.CancellationToken);

        Assert.Equal(["Newest", "Middle", "Oldest"], list.Select(x => x.Metadata?.Title));
    }

    [Fact]
    public async Task GetMediaFileStampList_ReturnsIdAndModifiedTimeOfOnlyTheUsersFiles()
    {
        User user = await CreateUser();
        User other = await CreateUser("other@example.com");
        MediaFile first = CreateMediaFile(user.ID);
        first.TimeModified = new(2026, 1, 2, 3, 4, 5, DateTimeKind.Utc);
        MediaFile second = CreateMediaFile(user.ID);
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
        User user = await CreateUser();
        MediaFile file = CreateMediaFile(user.ID, "Before");
        await Add(file);

        await using (IUnitOfWork uow = _fixture.UoWFactory.Create())
        {
            MediaFile loaded = await uow.Media.FindMediaFile(file.ID, user.ID, TestContext.Current.CancellationToken) ?? throw new InvalidOperationException("The media file was not found.");
            Assert.NotNull(loaded.Metadata);
            loaded.Metadata.Title = "After";
            loaded.Metadata.Artist = "Someone";
            loaded.Metadata.Year = 2001;
            loaded.TimeModified = new(2030, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            uow.Media.UpdateMediaFile(loaded);
            await uow.Commit(TestContext.Current.CancellationToken);
        }

        await using IUnitOfWork check = _fixture.UoWFactory.Create();
        MediaFile saved = await check.Media.FindMediaFile(file.ID, user.ID, TestContext.Current.CancellationToken) ?? throw new InvalidOperationException("The media file was not found.");
        Assert.Equal("After", saved.Metadata?.Title);
        Assert.Equal("Someone", saved.Metadata?.Artist);
        Assert.Equal(2001, saved.Metadata?.Year);
        Assert.Equal(new DateTime(2030, 1, 1, 0, 0, 0, DateTimeKind.Utc), saved.TimeModified);
        Assert.Equal(file.FilePath, saved.FilePath);
    }

    [Fact]
    public async Task UpdateMediaFile_NotCommitted_ChangesNothing()
    {
        User user = await CreateUser();
        MediaFile file = CreateMediaFile(user.ID, "Before");
        await Add(file);

        await using (IUnitOfWork uow = _fixture.UoWFactory.Create())
        {
            MediaFile loaded = await uow.Media.FindMediaFile(file.ID, user.ID, TestContext.Current.CancellationToken) ?? throw new InvalidOperationException("The media file was not found.");
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
        User user = await CreateUser();
        MediaFile file = CreateMediaFile(user.ID);
        MediaFile kept = CreateMediaFile(user.ID);
        await Add(file, kept);

        await using (IUnitOfWork uow = _fixture.UoWFactory.Create())
        {
            MediaFile loaded = await uow.Media.FindMediaFile(file.ID, user.ID, TestContext.Current.CancellationToken) ?? throw new InvalidOperationException("The media file was not found.");
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
