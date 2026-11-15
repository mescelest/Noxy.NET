using Noxy.NET.MediaPlayer.Application.Commands.Metadata;
using Noxy.NET.MediaPlayer.Application.Handlers.Metadata;
using Noxy.NET.MediaPlayer.Application.Notifications.Metadata;
using Noxy.NET.MediaPlayer.Domain.Entities;
using Noxy.NET.MediaPlayer.Domain.Responses.Metadata;
using Noxy.NET.Test.MediaPlayer.Fakes;

namespace Noxy.NET.Test.MediaPlayer.Metadata;

public class HandlerSettingsUpdateTest
{
    private static readonly Guid UserID = Guid.CreateVersion7();

    private readonly FakeStore _store = new();
    private readonly FakePublisher _publisher = new();
    private readonly FakeTimeProvider _time = new();
    private readonly HandlerSettingsUpdate _handler;

    public HandlerSettingsUpdateTest()
    {
        _time.Now = new(2027, 5, 6, 7, 8, 9, TimeSpan.Zero);
        _handler = new(new FakeUnitOfWorkFactory(_store), _publisher, _time);
    }

    private static CancellationToken Token => TestContext.Current.CancellationToken;

    private Task<ResponseSettings> Update(CommandSettingsUpdate command)
    {
        return _handler.Handle(command, Token).AsTask();
    }

    [Fact]
    public async Task Update_TheFirstChange_CreatesTheRowOfTheUser()
    {
        ResponseSettings response = await Update(new(UserID, new(true)));

        EntityUserSettings row = Assert.Single(_store.UserSettingsList);
        Assert.Equal(UserID, row.UserID);
        Assert.True(row.AutoMergeOnRename);
        Assert.Equal(_time.Now.UtcDateTime, row.TimeModified);
        Assert.True(response.AutoMergeOnRename);
        Assert.Equal(_time.Now.UtcDateTime, response.TimeModified);
        Assert.Equal(1, _store.CommitCount);
    }

    [Fact]
    public async Task Update_AChange_PublishesSettingsUpdatedToTheOwnerAfterTheCommit()
    {
        int commitsAtPublish = -1;
        _publisher.OnPublish = _ => commitsAtPublish = _store.CommitCount;

        ResponseSettings response = await Update(new(UserID, new(true)));

        NotificationSettingsUpdated pushed = Assert.Single(_publisher.GetList<NotificationSettingsUpdated>());
        Assert.Equal(UserID, pushed.UserID);
        Assert.Equal(response.AutoMergeOnRename, pushed.Settings.AutoMergeOnRename);
        Assert.Equal(1, commitsAtPublish);
        Assert.Equal(Token, Assert.Single(_publisher.TokenList));
    }

    [Fact]
    public async Task Update_AFurtherChange_UpdatesTheSameRowAndBumpsItsModifiedTime()
    {
        await Update(new(UserID, new(true)));
        _time.Now = new(2028, 1, 1, 0, 0, 0, TimeSpan.Zero);

        ResponseSettings response = await Update(new(UserID, new(false)));

        EntityUserSettings row = Assert.Single(_store.UserSettingsList);
        Assert.False(row.AutoMergeOnRename);
        Assert.Equal(_time.Now.UtcDateTime, row.TimeModified);
        Assert.False(response.AutoMergeOnRename);
        Assert.Equal(2, _publisher.GetList<NotificationSettingsUpdated>().Count);
    }

    [Fact]
    public async Task Update_AFieldLeftOut_IsUntouched()
    {
        _store.UserSettingsList.Add(new() { UserID = UserID, AutoMergeOnRename = true, TimeModified = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) });

        ResponseSettings response = await Update(new(UserID));

        Assert.True(response.AutoMergeOnRename);
        Assert.Equal(0, _store.CommitCount);
        Assert.Empty(_publisher.PublishedList);
    }

    [Fact]
    public async Task Update_TheSameValueAgain_SavesAndPublishesNothing()
    {
        _store.UserSettingsList.Add(new() { UserID = UserID, AutoMergeOnRename = true, TimeModified = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) });

        await Update(new(UserID, new(true)));

        Assert.Equal(0, _store.CommitCount);
        Assert.Empty(_publisher.PublishedList);
        Assert.Equal(new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc), Assert.Single(_store.UserSettingsList).TimeModified);
    }

    [Fact]
    public async Task Update_TheDefaultValueWithNoRow_CreatesNoRowAndPublishesNothing()
    {
        ResponseSettings response = await Update(new(UserID, new(false)));

        Assert.False(response.AutoMergeOnRename);
        Assert.Empty(_store.UserSettingsList);
        Assert.Equal(0, _store.CommitCount);
        Assert.Empty(_publisher.PublishedList);
    }

    [Fact]
    public async Task Update_NeverTouchesAnotherUsersSettings()
    {
        Guid otherID = Guid.CreateVersion7();
        _store.UserSettingsList.Add(new() { UserID = otherID, AutoMergeOnRename = false });

        await Update(new(UserID, new(true)));

        Assert.False(_store.UserSettingsList.Single(x => x.UserID == otherID).AutoMergeOnRename);
        Assert.True(_store.UserSettingsList.Single(x => x.UserID == UserID).AutoMergeOnRename);
    }

    [Fact]
    public async Task Update_WhenTheCommitFails_PublishesNothing()
    {
        _store.FailCommit = true;

        await Assert.ThrowsAsync<InvalidOperationException>(() => Update(new(UserID, new(true))));

        Assert.Empty(_publisher.PublishedList);
    }
}
