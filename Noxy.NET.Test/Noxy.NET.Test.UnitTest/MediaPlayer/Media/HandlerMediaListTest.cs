using System.Security.Cryptography;
using Noxy.NET.MediaPlayer.Application.Handlers.Media;
using Noxy.NET.MediaPlayer.Domain.Entities;
using Noxy.NET.MediaPlayer.Domain.Responses.Media;
using Noxy.NET.Test.MediaPlayer.Fakes;

namespace Noxy.NET.Test.MediaPlayer.Media;

public class HandlerMediaListTest
{
    private static readonly Guid UserID = Guid.CreateVersion7();

    private readonly FakeStore _store = new();
    private readonly HandlerMediaList _handler;

    public HandlerMediaListTest()
    {
        _handler = new(new FakeUnitOfWorkFactory(_store));
    }

    // A file that is already in the library, as the worker would have left it.
    private EntityMediaFile Seed(Guid userID, string title, DateTime? timeCreated = null)
    {
        byte[] bytes = [1, 2, 3];
        EntityMediaFile file = new()
        {
            TimeCreated = timeCreated ?? DateTime.UtcNow,
            FilePath = $"{Guid.CreateVersion7()}.mp3",
            ContentType = "audio/mpeg",
            SizeBytes = bytes.Length,
            ContentHash = Convert.ToHexStringLower(SHA256.HashData(bytes)),
            UserID = userID
        };
        file.Metadata = new() { Title = title, MediaFileID = file.ID };

        _store.MediaFileList.Add(file);
        return file;
    }

    private async Task<List<ResponseMediaFile>> Handle(int skip, int take)
    {
        return await _handler.Handle(new(UserID, skip, take), TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task Handle_ReturnsOnlyTheUsersFilesNewestFirst()
    {
        DateTime now = DateTime.UtcNow;
        EntityMediaFile older = Seed(UserID, "old", now.AddMinutes(-5));
        EntityMediaFile newer = Seed(UserID, "new", now);
        Seed(Guid.CreateVersion7(), "other");

        List<ResponseMediaFile> list = await Handle(0, 10);

        Assert.Equal([newer.ID, older.ID], list.Select(x => x.ID));
        Assert.Equal(["new", "old"], list.Select(x => x.Title));
    }

    [Fact]
    public async Task Handle_AppliesSkipAndTake()
    {
        DateTime now = DateTime.UtcNow;
        Seed(UserID, "media0", now.AddMinutes(-2));
        Seed(UserID, "media1", now.AddMinutes(-1));
        Seed(UserID, "media2", now);

        List<ResponseMediaFile> page = await Handle(1, 1);

        Assert.Equal("media1", Assert.Single(page).Title);
    }

    [Fact]
    public async Task Handle_ReturnsUtcTimes()
    {
        Seed(UserID, "media", new(2026, 1, 2, 3, 4, 5, DateTimeKind.Unspecified));

        ResponseMediaFile response = Assert.Single(await Handle(0, 10));

        Assert.Equal(DateTimeKind.Utc, response.TimeCreated.Kind);
        Assert.Equal(DateTimeKind.Utc, response.TimeModified.Kind);
    }
}
