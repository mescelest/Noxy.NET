using Noxy.NET.MediaPlayer.Application.Handlers.Playback;
using Noxy.NET.MediaPlayer.Application.Models.Playback;
using Noxy.NET.MediaPlayer.Application.Services;
using Noxy.NET.MediaPlayer.Domain.Entities;
using Noxy.NET.Test.MediaPlayer.Fakes;

namespace Noxy.NET.Test.MediaPlayer.Playback;

public class HandlerPlaybackSessionTest
{
    private static readonly Guid UserID = Guid.CreateVersion7();
    private static readonly DateTime Start = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    private readonly FakeStore _store = new();
    private readonly HandlerPlaybackSession _handler;

    public HandlerPlaybackSessionTest()
    {
        _handler = new(new FakeUnitOfWorkFactory(_store), new ETagService());
    }

    private static CancellationToken Token => TestContext.Current.CancellationToken;

    private EntityPlaybackSession Seed(Guid? userID = null, DateTime? modified = null)
    {
        EntityPlaybackSession session = new()
        {
            UserID = userID ?? UserID,
            QueueMediaFileIDList = [Guid.CreateVersion7(), Guid.CreateVersion7()],
            CurrentIndex = 1,
            PositionSeconds = 33.5,
            TimeModified = modified ?? Start
        };

        _store.PlaybackSessionList.Add(session);
        return session;
    }

    private async Task<PlaybackSessionView> View(string? ifNoneMatch = null, Guid? userID = null)
    {
        return await _handler.Handle(new(userID ?? UserID, ifNoneMatch), Token);
    }

    [Fact]
    public async Task Handle_WithNoSession_HasNeitherAnETagNorASession()
    {
        PlaybackSessionView view = await View();

        Assert.Null(view.ETag);
        Assert.Null(view.Session);
    }

    [Fact]
    public async Task Handle_ReturnsTheSessionWithAStrongETag()
    {
        EntityPlaybackSession session = Seed();

        PlaybackSessionView view = await View();

        Assert.Matches("^\"[0-9a-f]{64}\"$", view.ETag);
        Assert.NotNull(view.Session);
        Assert.Equal(session.ID, view.Session.ID);
        Assert.Equal(session.QueueMediaFileIDList, view.Session.QueueMediaFileIDList);
        Assert.Equal(1, view.Session.CurrentIndex);
        Assert.Equal(33.5, view.Session.PositionSeconds);
        Assert.Equal(DateTimeKind.Utc, view.Session.TimeModified.Kind);
    }

    [Fact]
    public async Task Handle_AMatchingIfNoneMatchOrTheWildcard_HasTheETagButNoSession()
    {
        Seed();
        string eTag = (await View()).ETag ?? throw new InvalidOperationException("There was no ETag.");

        PlaybackSessionView matched = await View(eTag);
        PlaybackSessionView wildcard = await View("*");

        Assert.Equal(eTag, matched.ETag);
        Assert.Null(matched.Session);
        Assert.Null(wildcard.Session);
    }

    [Theory]
    [InlineData("\"other\"")]
    [InlineData("garbage")]
    [InlineData("")]
    [InlineData(null)]
    public async Task Handle_AMismatchedIfNoneMatch_ReturnsTheSession(string? ifNoneMatch)
    {
        Seed();

        Assert.NotNull((await View(ifNoneMatch)).Session);
    }

    [Fact]
    public async Task Handle_TheETagChangesWithTheModifiedTime_AndStaysTheSameWithout()
    {
        EntityPlaybackSession session = Seed();
        string before = (await View()).ETag ?? throw new InvalidOperationException("There was no ETag.");

        Assert.Equal(before, (await View()).ETag);

        session.TimeModified = Start.AddSeconds(1);

        Assert.NotEqual(before, (await View()).ETag);
        Assert.NotNull((await View(before)).Session);
    }

    [Fact]
    public async Task Handle_NeverReturnsAnotherUsersSession()
    {
        Seed(Guid.CreateVersion7());

        PlaybackSessionView view = await View();

        Assert.Null(view.ETag);
        Assert.Null(view.Session);
    }
}
