using System.Text.Json;
using Noxy.NET.MediaPlayer.Application.Commands.Setting;
using Noxy.NET.MediaPlayer.Application.Handlers.Setting;
using Noxy.NET.MediaPlayer.Application.Notifications.Setting;
using Noxy.NET.MediaPlayer.Domain.Declarations;
using Noxy.NET.MediaPlayer.Domain.Entities;
using Noxy.NET.MediaPlayer.Domain.Enums;
using Noxy.NET.MediaPlayer.Domain.Exceptions;
using Noxy.NET.MediaPlayer.Domain.Responses.Setting;
using Noxy.NET.Test.MediaPlayer.Fakes;

namespace Noxy.NET.Test.MediaPlayer.Metadata;

public class HandlerSettingUpdateTest
{
    private static readonly Guid UserID = Guid.CreateVersion7();
    private static readonly DateTime Earlier = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    private readonly FakeStore _store = new();
    private readonly FakePublisher _publisher = new();
    private readonly FakeTimeProvider _time = new();
    private readonly HandlerSettingUpdate _handler;

    public HandlerSettingUpdateTest()
    {
        _time.Now = new(2027, 5, 6, 7, 8, 9, TimeSpan.Zero);
        _handler = new(new FakeUnitOfWorkFactory(_store), _publisher, _time);
    }

    private static CancellationToken Token => TestContext.Current.CancellationToken;

    private static JsonElement Json(string text)
    {
        return JsonDocument.Parse(text).RootElement.Clone();
    }

    private Task<ResponseSetting> Update(string key, string json, Guid? userID = null)
    {
        return _handler.Handle(new CommandSettingUpdate(userID ?? UserID, key, Json(json)), Token).AsTask();
    }

    [Fact]
    public async Task Update_TheFirstChange_CreatesTheRowOfTheUserWithTheClock()
    {
        ResponseSetting response = await Update(SettingKey.Volume, "40");

        EntityUserSettingInteger row = Assert.IsType<EntityUserSettingInteger>(Assert.Single(_store.UserSettingList));
        Assert.Equal(UserID, row.UserID);
        Assert.Equal(SettingKey.Volume, row.Key);
        Assert.Equal(40, row.Value);
        Assert.Equal(_time.Now.UtcDateTime, row.TimeModified);
        Assert.Equal((SettingKey.Volume, SettingValueType.Integer, (object)40, _time.Now.UtcDateTime), (response.Key, response.ValueType, response.Value, response.TimeModified));
        Assert.Equal(1, _store.CommitCount);
    }

    [Theory]
    [InlineData(SettingKey.MergeOnRename, "true", true, SettingValueType.Boolean)]
    [InlineData(SettingKey.Volume, "0", 0, SettingValueType.Integer)]
    [InlineData(SettingKey.Repeat, "\"Forever\"", "Forever", SettingValueType.Enum)]
    [InlineData(SettingKey.Shuffle, "\"SpreadArtists\"", "SpreadArtists", SettingValueType.Enum)]
    public async Task Update_EachType_SavesAndReturnsTheRightResponse(string key, string json, object expected, SettingValueType type)
    {
        ResponseSetting response = await Update(key, json);

        Assert.Equal(key, response.Key);
        Assert.Equal(type, response.ValueType);
        Assert.Equal(expected, response.Value);
        Assert.Equal(key, Assert.Single(_store.UserSettingList).Key);
    }

    [Fact]
    public async Task Update_AFurtherChange_UpdatesTheSameRowAndBumpsItsModifiedTime()
    {
        await Update(SettingKey.Volume, "40");
        Guid rowID = Assert.Single(_store.UserSettingList).ID;
        _time.Now = new(2028, 1, 1, 0, 0, 0, TimeSpan.Zero);

        ResponseSetting response = await Update(SettingKey.Volume, "70");

        EntityUserSettingInteger row = Assert.IsType<EntityUserSettingInteger>(Assert.Single(_store.UserSettingList));
        Assert.Equal(rowID, row.ID);
        Assert.Equal(70, row.Value);
        Assert.Equal(_time.Now.UtcDateTime, row.TimeModified);
        Assert.Equal(70, response.Value);
    }

    [Fact]
    public async Task Update_BackToTheDefaultOfASavedSetting_StillSavesTheRow()
    {
        await Update(SettingKey.Volume, "40");

        await Update(SettingKey.Volume, "100");

        Assert.Equal(100, Assert.IsType<EntityUserSettingInteger>(Assert.Single(_store.UserSettingList)).Value);
    }

    [Fact]
    public async Task Update_TheSameValueAgain_SavesNothing_PushesNothing_AndKeepsTheModifiedTime()
    {
        _store.UserSettingList.Add(new EntityUserSettingInteger { UserID = UserID, Key = SettingKey.Volume, Value = 40, TimeModified = Earlier });

        ResponseSetting response = await Update(SettingKey.Volume, "40");

        Assert.Equal(0, _store.CommitCount);
        Assert.Empty(_publisher.PublishedList);
        Assert.Equal(Earlier, Assert.Single(_store.UserSettingList).TimeModified);
        Assert.Equal(40, response.Value);
        Assert.Equal(Earlier, response.TimeModified);
    }

    [Theory]
    [InlineData(SettingKey.Volume, "100", 100)]
    [InlineData(SettingKey.MergeOnRename, "false", false)]
    [InlineData(SettingKey.Repeat, "\"No\"", "No")]
    [InlineData(SettingKey.Shuffle, "\"Off\"", "Off")]
    public async Task Update_TheDefaultForAUserWithNoRow_CreatesNoRowAndPushesNothing(string key, string json, object expected)
    {
        ResponseSetting response = await Update(key, json);

        Assert.Equal(expected, response.Value);
        Assert.Equal(DateTime.UnixEpoch, response.TimeModified);
        Assert.Empty(_store.UserSettingList);
        Assert.Equal(0, _store.CommitCount);
        Assert.Empty(_publisher.PublishedList);
    }

    [Fact]
    public async Task Update_AChange_PublishesSettingUpdatedToTheOwnerAfterTheCommitWithTheToken()
    {
        int commitsAtPublish = -1;
        _publisher.OnPublish = _ => commitsAtPublish = _store.CommitCount;

        ResponseSetting response = await Update(SettingKey.Repeat, "\"Once\"");

        NotificationSettingUpdated pushed = Assert.Single(_publisher.GetList<NotificationSettingUpdated>());
        Assert.Equal(UserID, pushed.UserID);
        Assert.Equal(response.Key, pushed.Setting.Key);
        Assert.Equal("Once", pushed.Setting.Value);
        Assert.Equal(1, commitsAtPublish);
        Assert.Equal(Token, Assert.Single(_publisher.TokenList));
    }

    [Theory]
    [InlineData("Nope")]
    [InlineData("volume")]
    [InlineData("VOLUME")]
    [InlineData("")]
    [InlineData(" Volume")]
    public async Task Update_AnUnknownOrWrongCaseKey_IsNotFound_AndNothingChanges(string key)
    {
        await Assert.ThrowsAsync<SettingNotFoundException>(() => Update(key, "40"));

        Assert.Empty(_store.UserSettingList);
        Assert.Equal(0, _store.CommitCount);
        Assert.Empty(_publisher.PublishedList);
    }

    [Theory]
    [InlineData(SettingKey.Volume, "\"loud\"")]
    [InlineData(SettingKey.Volume, "101")]
    [InlineData(SettingKey.Volume, "-1")]
    [InlineData(SettingKey.Volume, "3.5")]
    [InlineData(SettingKey.Volume, "null")]
    [InlineData(SettingKey.Volume, "true")]
    [InlineData(SettingKey.Repeat, "\"Sometimes\"")]
    [InlineData(SettingKey.Repeat, "\"forever\"")]
    [InlineData(SettingKey.Repeat, "1")]
    [InlineData(SettingKey.Shuffle, "\"Spread\"")]
    [InlineData(SettingKey.MergeOnRename, "1")]
    [InlineData(SettingKey.MergeOnRename, "\"true\"")]
    [InlineData(SettingKey.MergeOnRename, "null")]
    public async Task Update_AValueThatDoesNotMatchTheDeclaration_IsInvalid_AndSavesAndPushesNothing(string key, string json)
    {
        InvalidSettingValueException exception = await Assert.ThrowsAsync<InvalidSettingValueException>(() => Update(key, json));

        Assert.Equal(key, exception.Key);
        Assert.Empty(_store.UserSettingList);
        Assert.Equal(0, _store.CommitCount);
        Assert.Empty(_publisher.PublishedList);
    }

    [Fact]
    public async Task Update_AnInvalidValue_LeavesTheSavedValueAsItWas()
    {
        await Update(SettingKey.Volume, "40");

        await Assert.ThrowsAsync<InvalidSettingValueException>(() => Update(SettingKey.Volume, "500"));

        Assert.Equal(40, Assert.IsType<EntityUserSettingInteger>(Assert.Single(_store.UserSettingList)).Value);
    }

    [Fact]
    public async Task Update_NeverReadsOrChangesAnotherUsersRow()
    {
        Guid otherID = Guid.CreateVersion7();
        _store.UserSettingList.Add(new EntityUserSettingInteger { UserID = otherID, Key = SettingKey.Volume, Value = 10, TimeModified = Earlier });

        await Update(SettingKey.Volume, "60");

        Assert.Equal(10, Assert.IsType<EntityUserSettingInteger>(_store.UserSettingList.Single(x => x.UserID == otherID)).Value);
        Assert.Equal(60, Assert.IsType<EntityUserSettingInteger>(_store.UserSettingList.Single(x => x.UserID == UserID)).Value);
    }

    [Fact]
    public async Task Update_ALostRaceToCreateTheRow_RunsAgainAndStillEndsWithOneRow()
    {
        _store.KeyConflictCommitCount = 1;

        await Update(SettingKey.Volume, "40");

        Assert.Single(_store.UserSettingList);
        Assert.Single(_publisher.GetList<NotificationSettingUpdated>());
    }

    [Fact]
    public async Task Update_WhenAnotherDeviceCreatedTheRowFirst_TheRetryUpdatesIt()
    {
        // The row appears between the first run, which saw none, and its commit: that commit conflicts and the second run finds the row.
        _store.OnCommit = () =>
        {
            _store.OnCommit = null;
            _store.UserSettingList.Add(new EntityUserSettingInteger { UserID = UserID, Key = SettingKey.Volume, Value = 55, TimeModified = Earlier });
        };

        ResponseSetting response = await Update(SettingKey.Volume, "70");

        Assert.Equal(70, response.Value);
        EntityUserSettingInteger row = Assert.IsType<EntityUserSettingInteger>(Assert.Single(_store.UserSettingList));
        Assert.Equal(70, row.Value);
        Assert.Equal(_time.Now.UtcDateTime, row.TimeModified);
        Assert.Single(_publisher.GetList<NotificationSettingUpdated>());
    }

    [Fact]
    public async Task Update_WhenTheCommitFails_PublishesNothing()
    {
        _store.FailCommit = true;

        await Assert.ThrowsAsync<InvalidOperationException>(() => Update(SettingKey.Volume, "40"));

        Assert.Empty(_publisher.PublishedList);
    }
}
