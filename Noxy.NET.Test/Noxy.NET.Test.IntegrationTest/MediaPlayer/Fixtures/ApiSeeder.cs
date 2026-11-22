using Microsoft.Extensions.DependencyInjection;
using Noxy.NET.MediaPlayer.Application.Interfaces;
using Noxy.NET.MediaPlayer.Application.Interfaces.Models.Upload;
using Noxy.NET.MediaPlayer.Application.Interfaces.Services;
using Noxy.NET.MediaPlayer.Application.Models.Upload;
using Noxy.NET.MediaPlayer.Domain.Entities;
using Noxy.NET.MediaPlayer.Domain.Enums;

namespace Noxy.NET.Test.MediaPlayer.Fixtures;

public sealed class ApiSeeder(ApiFactory factory)
{
    public async Task SeedMediaFile(Guid userID)
    {
        EntityMediaFile file = new()
        {
            FilePath = $"{Guid.CreateVersion7()}.mp3",
            ContentType = "audio/mpeg",
            SizeBytes = 10,
            ContentHash = new('d', 64),
            UserID = userID
        };
        file.Metadata = new() { Title = "Already here", MediaFileID = file.ID };

        await using IUnitOfWork uow = factory.Services.GetRequiredService<IUnitOfWorkFactory>().Create();
        uow.Media.AddMediaFile(file);
        await uow.Commit(TestContext.Current.CancellationToken);
    }

    // Many media files at once (no bytes behind them), in one commit: for a library that is too big to upload.
    public async Task SeedMediaFileList(Guid userID, int count)
    {
        await using IUnitOfWork uow = factory.Services.GetRequiredService<IUnitOfWorkFactory>().Create();
        for (int index = 0; index < count; index++)
        {
            EntityMediaFile file = new()
            {
                FilePath = $"{Guid.CreateVersion7()}.mp3",
                ContentType = "audio/mpeg",
                SizeBytes = 10,
                ContentHash = index.ToString("x64"),
                UserID = userID
            };
            file.Metadata = new() { Title = $"Seeded {index:00000}", MediaFileID = file.ID };
            uow.Media.AddMediaFile(file);
        }

        await uow.Commit(TestContext.Current.CancellationToken);
    }

    // A job as an earlier request would have left it, with its file in staging when asked.
    public async Task<EntityMediaUploadJob> SeedJob(Guid userID, MediaUploadJobStatusEnum status, bool withStagedFile)
    {
        byte[] content = MediaApiClient.Mp3(50);
        string stagedName = $"{Guid.CreateVersion7()}.mp3";

        if (withStagedFile)
        {
            StoredFile stored = await factory.Services.GetRequiredService<IFileStorageService>().StageAsync(new MemoryStream(content), "mp3", TestContext.Current.CancellationToken);
            stagedName = stored.FileName;
        }

        EntityMediaUploadJob job = new()
        {
            UserID = userID,
            StagedFileName = stagedName,
            Title = "Seeded",
            ContentType = "audio/mpeg",
            SizeBytes = content.Length,
            ContentHash = new('c', 64),
            Status = status
        };

        await using IUnitOfWork uow = factory.Services.GetRequiredService<IUnitOfWorkFactory>().Create();
        uow.Media.AddMediaUploadJob(job);
        await uow.Commit(TestContext.Current.CancellationToken);
        return job;
    }

    public void WakeTheWorker()
    {
        factory.Services.GetRequiredService<IUploadSignal>().Notify();
    }
}
