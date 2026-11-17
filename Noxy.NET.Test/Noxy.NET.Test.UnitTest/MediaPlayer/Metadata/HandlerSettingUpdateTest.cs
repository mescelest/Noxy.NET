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
        ResponseSetting response = await Update(SettingKey.Shuffle, "\"Random\"");

        EntityUserSettingEnum row = Assert.IsType<EntityUserSettingEnum>(Assert.Single(_store.UserSettingList));
        Assert.Equal(UserID, row.UserID);
        Assert.Equal(SettingKey.Shuffle, row.Key);
        Assert.Equal("Random", row.Value);
        Assert.Equal(_time.Now.UtcDateTime, row.TimeModified);
        Assert.Equal((SettingKey.Shuffle, SettingValueTypeEnum.Enum, (object)"Random", _time.Now.UtcDateTime), (response.Key, response.ValueType, response.Value, response.TimeModified));
        Assert.Equal(1, _store.CommitCount);
    }

    [Theory]
    [InlineData(SettingKey.MergeOnRename, "true", true, SettingValueTypeEnum.Boolean)]
    [InlineData(SettingKey.KeepQueuedItems, "true", true, SettingValueTypeEnum.Boolean)]
    [InlineData(SettingKey.Repeat, "\"Forever\"", "Forever", SettingValueTypeEnum.Enum)]
    [InlineData(SettingKey.Shuffle, "\"SpreadArtists\"", "SpreadArtists", SettingValueTypeEnum.Enum)]
    public async Task Update_EachType_SavesAndReturnsTheRightResponse(string key, string json, object expected, SettingValueTypeEnum type)
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
        await Update(SettingKey.Shuffle, "\"Random\"");
        Guid rowID = Assert.Single(_store.UserSettingList).ID;
        _time.Now = new(2028, 1, 1, 0, 0, 0, TimeSpan.Zero);

        ResponseSetting response = await Update(SettingKey.Shuffle, "\"SpreadArtists\"");

        EntityUserSettingEnum row = Assert.IsType<EntityUserSettingEnum>(Assert.Single(_store.UserSettingList));
        Assert.Equal(rowID, row.ID);
        Assert.Equal("SpreadArtists", row.Value);
        Assert.Equal(_time.Now.UtcDateTime, row.TimeModified);
        Assert.Equal("SpreadArtists", response.Value);
    }

    [Fact]
    public async Task Update_BackToTheDefaultOfASavedSetting_StillSavesTheRow()
    {
        await Update(SettingKey.Shuffle, "\"Random\"");

        await Update(SettingKey.Shuffle, "\"Off\"");

        Assert.Equal("Off", Assert.IsType<EntityUserSettingEnum>(Assert.Single(_store.UserSettingList)).Value);
    }

    [Fact]
    public async Task Update_TheSameValueAgain_SavesNothing_PushesNothing_AndKeepsTheModifiedTime()
    {
        _store.UserSettingList.Add(new EntityUserSettingEnum { UserID = UserID, Key = SettingKey.Shuffle, Value = "Random", TimeModified = Earlier });

        ResponseSetting response = await Update(SettingKey.Shuffle, "\"Random\"");

        Assert.Equal(0, _store.CommitCount);
        Assert.Empty(_publisher.PublishedList);
        Assert.Equal(Earlier, Assert.Single(_store.UserSettingList).TimeModified);
        Assert.Equal("Random", response.Value);
        Assert.Equal(Earlier, response.TimeModified);
    }

    [Theory]
    [InlineData(SettingKey.MergeOnRename, "false", false)]
    [InlineData(SettingKey.KeepQueuedItems, "false", false)]
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
    [InlineData("shuffle")]
    [InlineData("SHUFFLE")]
    [InlineData("")]
    [InlineData(" Shuffle")]
    [InlineData("Volume")]
    public async Task Update_AnUnknownOrWrongCaseKey_IsNotFound_AndNothingChanges(string key)
    {
        await Assert.ThrowsAsync<SettingNotFoundException>(() => Update(key, "\"Off\""));

        Assert.Empty(_store.UserSettingList);
        Assert.Equal(0, _store.CommitCount);
        Assert.Empty(_publisher.PublishedList);
    }

    [Theory]
    [InlineData(SettingKey.Repeat, "\"Sometimes\"")]
    [InlineData(SettingKey.Repeat, "\"forever\"")]
    [InlineData(SettingKey.Repeat, "1")]
    [InlineData(SettingKey.Shuffle, "\"Spread\"")]
    [InlineData(SettingKey.Shuffle, "true")]
    [InlineData(SettingKey.Shuffle, "null")]
    [InlineData(SettingKey.MergeOnRename, "1")]
    [InlineData(SettingKey.MergeOnRename, "\"true\"")]
    [InlineData(SettingKey.MergeOnRename, "null")]
    [InlineData(SettingKey.KeepQueuedItems, "\"yes\"")]
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
        await Update(SettingKey.Shuffle, "\"Random\"");

        await Assert.ThrowsAsync<InvalidSettingValueException>(() => Update(SettingKey.Shuffle, "\"Nope\""));

        Assert.Equal("Random", Assert.IsType<EntityUserSettingEnum>(Assert.Single(_store.UserSettingList)).Value);
    }

    [Fact]
    public async Task Update_NeverReadsOrChangesAnotherUsersRow()
    {
        Guid otherID = Guid.CreateVersion7();
        _store.UserSettingList.Add(new EntityUserSettingEnum { UserID = otherID, Key = SettingKey.Shuffle, Value = "Random", TimeModified = Earlier });

        await Update(SettingKey.Shuffle, "\"SpreadArtists\"");

        Assert.Equal("Random", Assert.IsType<EntityUserSettingEnum>(_store.UserSettingList.Single(x => x.UserID == otherID)).Value);
        Assert.Equal("SpreadArtists", Assert.IsType<EntityUserSettingEnum>(_store.UserSettingList.Single(x => x.UserID == UserID)).Value);
    }

    [Fact]
    public async Task Update_ALostRaceToCreateTheRow_RunsAgainAndStillEndsWithOneRow()
    {
        _store.KeyConflictCommitCount = 1;

        await Update(SettingKey.Shuffle, "\"Random\"");

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
            _store.UserSettingList.Add(new EntityUserSettingEnum { UserID = UserID, Key = SettingKey.Shuffle, Value = "Random", TimeModified = Earlier });
        };

        ResponseSetting response = await Update(SettingKey.Shuffle, "\"SpreadArtists\"");

        Assert.Equal("SpreadArtists", response.Value);
        EntityUserSettingEnum row = Assert.IsType<EntityUserSettingEnum>(Assert.Single(_store.UserSettingList));
        Assert.Equal("SpreadArtists", row.Value);
        Assert.Equal(_time.Now.UtcDateTime, row.TimeModified);
        Assert.Single(_publisher.GetList<NotificationSettingUpdated>());
    }

    [Fact]
    public async Task Update_WhenTheCommitFails_PublishesNothing()
    {
        _store.FailCommit = true;

        await Assert.ThrowsAsync<InvalidOperationException>(() => Update(SettingKey.Shuffle, "\"Random\""));

        Assert.Empty(_publisher.PublishedList);
    }
}
