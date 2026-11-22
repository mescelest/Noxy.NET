using Noxy.NET.MediaPlayer.Application.Interfaces;
using Noxy.NET.MediaPlayer.Application.Notifications.Media;
using Noxy.NET.MediaPlayer.Domain.Entities;
using Noxy.NET.MediaPlayer.Domain.Responses.Media;
using Noxy.NET.Test.MediaPlayer.Fakes;

namespace Noxy.NET.Test.MediaPlayer.Metadata;

public class HandlerMetadataChangeTest
{
    private static readonly Guid UserID = Guid.CreateVersion7();
    private static readonly DateTime Original = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    private readonly FakeStore _store = new();
    private readonly FakePublisher _publisher = new();
    private readonly FakeTimeProvider _time = new();
    private readonly FakeMetadataChangeHandler _handler;

    public HandlerMetadataChangeTest()
    {
        _handler = new(new FakeUnitOfWorkFactory(_store), _publisher, _time);
    }

    private static CancellationToken Token => TestContext.Current.CancellationToken;

    private EntityMediaFile Seed(string title)
    {
        EntityMediaFile file = new() { FilePath = $"{Guid.CreateVersion7()}.mp3", ContentType = "audio/mpeg", SizeBytes = 1, ContentHash = new('a', 64), UserID = UserID, TimeModified = Original };
        file.Metadata = new() { Title = title, MediaFileID = file.ID };
        _store.MediaFileList.Add(file);
        return file;
    }

    private EntityMediaFile Stored(EntityMediaFile file)
    {
        return _store.MediaFileList.Single(x => x.ID == file.ID);
    }

    [Fact]
    public void Now_IsTheClockInUtc()
    {
        _time.Now = new(2027, 5, 6, 7, 8, 9, TimeSpan.Zero);

        Assert.Equal(_time.Now.UtcDateTime, _handler.GetNow());
    }

    [Fact]
    public async Task TouchFiles_SetsTheModifiedTimeOfExactlyTheGivenFilesAndNoOtherFile()
    {
        EntityMediaFile changed = Seed("Changed");
        EntityMediaFile untouched = Seed("Untouched");
        DateTime now = new(2027, 5, 6, 7, 8, 9, DateTimeKind.Utc);

        await using IUnitOfWork uow = new FakeUnitOfWork(_store);
        _handler.Touch(uow, [changed], now);
        await uow.Commit(Token);

        Assert.Equal(now, Stored(changed).TimeModified);
        Assert.Equal(Original, Stored(untouched).TimeModified);
    }

    [Fact]
    public async Task TouchFiles_ReturnsTheResponsesWithTheNewTime()
    {
        EntityMediaFile file = Seed("One");
        DateTime now = new(2027, 5, 6, 7, 8, 9, DateTimeKind.Utc);

        await using IUnitOfWork uow = new FakeUnitOfWork(_store);
        ResponseMediaFile response = Assert.Single(_handler.Touch(uow, [file], now));

        Assert.Equal(file.ID, response.ID);
        Assert.Equal(now, response.TimeModified);
    }

    [Fact]
    public async Task TouchFiles_AFileListedTwice_IsTouchedOnce()
    {
        EntityMediaFile file = Seed("One");

        await using IUnitOfWork uow = new FakeUnitOfWork(_store);
        List<ResponseMediaFile> responseList = _handler.Touch(uow, [file, file], new(2027, 5, 6, 7, 8, 9, DateTimeKind.Utc));

        Assert.Single(responseList);
    }

    [Fact]
    public async Task TouchFiles_SavesNothingUntilTheCommit()
    {
        EntityMediaFile file = Seed("One");

        await using IUnitOfWork uow = new FakeUnitOfWork(_store);
        EntityMediaFile loaded = await uow.Media.FindMediaFile(file.ID, UserID, Token) ?? throw new InvalidOperationException("The seeded file is missing.");
        _handler.Touch(uow, [loaded], new(2027, 5, 6, 7, 8, 9, DateTimeKind.Utc));

        Assert.Equal(Original, Stored(file).TimeModified);
        Assert.Equal(0, _store.CommitCount);
    }

    [Fact]
    public async Task PublishFiles_PushesMediaUpdatedForEachFileToTheUserWithTheToken()
    {
        EntityMediaFile first = Seed("One");
        EntityMediaFile second = Seed("Two");
        await using IUnitOfWork uow = new FakeUnitOfWork(_store);
        List<ResponseMediaFile> responseList = _handler.Touch(uow, [first, second], new(2027, 5, 6, 7, 8, 9, DateTimeKind.Utc));

        await _handler.PublishFiles(UserID, responseList, Token);

        List<NotificationMediaUpdated> pushed = _publisher.GetList<NotificationMediaUpdated>();
        Assert.Equal([first.ID, second.ID], pushed.Select(x => x.MediaFile.ID).ToList());
        Assert.All(pushed, x => Assert.Equal(UserID, x.UserID));
        Assert.Equal([Token, Token], _publisher.TokenList);
    }

    [Fact]
    public async Task PublishFiles_WithNoFiles_PublishesNothing()
    {
        await _handler.PublishFiles(UserID, [], Token);

        Assert.Empty(_publisher.PublishedList);
    }
}
