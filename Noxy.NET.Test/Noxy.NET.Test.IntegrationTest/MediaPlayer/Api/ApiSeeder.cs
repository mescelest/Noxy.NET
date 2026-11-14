using Noxy.NET.MediaPlayer.Application.Interfaces.Signals;
using Microsoft.Extensions.DependencyInjection;
using Noxy.NET.MediaPlayer.Application.Interfaces;
using Noxy.NET.MediaPlayer.Application.Interfaces.Services;
using Noxy.NET.MediaPlayer.Application.Models;
using Noxy.NET.MediaPlayer.Domain.Entities;
using Noxy.NET.MediaPlayer.Domain.Enums;

namespace Noxy.NET.Test.MediaPlayer.Api;

public sealed class ApiSeeder(ApiFactory factory)
{
    public async Task SeedMediaFile(Guid userID)
    {
        MediaFile file = new()
        {
            FilePath = $"{Guid.CreateVersion7()}.mp3",
            ContentType = "audio/mpeg",
            SizeBytes = 10,
            ContentHash = new string('d', 64),
            UserID = userID
        };
        file.Metadata = new() { Title = "Already here", MediaFileID = file.ID };

        await using IUnitOfWork uow = factory.Services.GetRequiredService<IUnitOfWorkFactory>().Create();
        uow.Media.AddMediaFile(file);
        await uow.Commit(TestContext.Current.CancellationToken);
    }

    // A job as an earlier request would have left it, with its file in staging when asked.
    public async Task<MediaUploadJob> SeedJob(Guid userID, MediaUploadJobStatus status, bool withStagedFile)
    {
        byte[] content = MediaApiClient.Mp3(50);
        string stagedName = $"{Guid.CreateVersion7()}.mp3";

        if (withStagedFile)
        {
            StoredFile stored = await factory.Services.GetRequiredService<IFileStorageService>().StageAsync(new MemoryStream(content), "mp3", TestContext.Current.CancellationToken);
            stagedName = stored.FileName;
        }

        MediaUploadJob job = new()
        {
            UserID = userID,
            StagedFileName = stagedName,
            Title = "Seeded",
            ContentType = "audio/mpeg",
            SizeBytes = content.Length,
            ContentHash = new string('c', 64),
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
