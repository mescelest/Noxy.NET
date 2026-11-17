using System.Net;
using System.Text.Json;
using Noxy.NET.MediaPlayer.Domain.Declarations;
using Noxy.NET.MediaPlayer.Domain.Enums;
using Noxy.NET.MediaPlayer.Domain.Responses.Setting;
using Noxy.NET.MediaPlayer.Presentation.Exceptions;
using Noxy.NET.MediaPlayer.Presentation.Extensions;
using Noxy.NET.MediaPlayer.Presentation.Services;
using Noxy.NET.Test.MediaPlayer.Fakes;

namespace Noxy.NET.Test.MediaPlayer.Presentation;

public class SettingServiceTest
{
    private static readonly DateTime Start = new(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);

    // An integer setting the server does not declare: the service keeps whatever it is given.
    private const string SampleKey = "SampleInteger";

    private readonly FakeApiClient _api = new();
    private readonly FakeMetadataStore _store = new();
    private readonly SettingService _service;

    public SettingServiceTest()
    {
        _service = new(_api, _store.Setting);
    }

    private static ResponseSetting Setting(string key, object value, SettingValueTypeEnum valueType, DateTime? timeModified = null)
    {
        return new() { Key = key, ValueType = valueType, Value = JsonSerializer.SerializeToElement(value), TimeModified = timeModified ?? Start };
    }

    private static ResponseSetting Sample(int value, DateTime? timeModified = null)
    {
        return Setting(SampleKey, value, SettingValueTypeEnum.Integer, timeModified);
    }

    private async Task<int> StoredSample()
    {
        return (await _service.GetSettingList()).Single(x => x.Key == SampleKey).GetInteger();
    }

    private async Task Seed(params ResponseSetting[] settingList)
    {
        await _store.Setting.ReplaceList([.. settingList]);
    }

    #region -- Sync --

    [Fact]
    public async Task SyncSettingList_FirstRun_SendsNoETag_StoresTheListAndTheETag()
    {
        _api.SettingListHandler = _ => new(HttpStatusCode.OK, [Sample(40)], "\"s1\"");

        await _service.SyncSettingList();

        Assert.Equal([null], _api.SettingListETagList);
        Assert.Equal(40, (await _service.GetSettingList()).Single().GetInteger());
        Assert.Equal("\"s1\"", await _store.Setting.GetETag());
    }

    [Fact]
    public async Task SyncSettingList_On304_ChangesNothing()
    {
        await Seed(Sample(40));
        await _store.Setting.SetETag("\"s1\"");
        _api.SettingListHandler = _ => new(HttpStatusCode.NotModified, null, null);
        int changed = 0;
        _service.Changed += () => changed++;

        await _service.SyncSettingList();

        Assert.Equal(["\"s1\""], _api.SettingListETagList);
        Assert.Equal(40, (await _service.GetSettingList()).Single().GetInteger());
        Assert.Equal("\"s1\"", await _store.Setting.GetETag());
        Assert.Equal(0, changed);
    }

    [Fact]
    public async Task SyncSettingList_On200_ReplacesTheLocalList()
    {
        await Seed(Sample(40), Setting(SettingKey.Shuffle, "Random", SettingValueTypeEnum.Enum));
        await _store.Setting.SetETag("\"old\"");
        _api.SettingListHandler = _ => new(HttpStatusCode.OK, [Sample(70)], "\"new\"");

        await _service.SyncSettingList();

        ResponseSetting only = Assert.Single(await _service.GetSettingList());
        Assert.Equal(70, only.GetInteger());
        Assert.Equal("\"new\"", await _store.Setting.GetETag());
    }

    [Fact]
    public async Task SyncSettingList_WhenTheLocalListIsEmptyButAnETagIsStored_SendsNoETag()
    {
        await _store.Setting.SetETag("\"stale\"");
        _api.SettingListHandler = _ => new(HttpStatusCode.OK, [Sample(40)], "\"fresh\"");

        await _service.SyncSettingList();

        Assert.Equal([null], _api.SettingListETagList);
        Assert.Equal("\"fresh\"", await _store.Setting.GetETag());
    }

    [Fact]
    public async Task SyncSettingList_Force_SendsNoETag()
    {
        await Seed(Sample(40));
        await _store.Setting.SetETag("\"s1\"");
        _api.SettingListHandler = _ => new(HttpStatusCode.OK, [Sample(41)], "\"s2\"");

        await _service.SyncSettingList(true);

        Assert.Equal([null], _api.SettingListETagList);
        Assert.Equal(41, (await _service.GetSettingList()).Single().GetInteger());
    }

    [Theory]
    [InlineData(typeof(HttpRequestException))]
    [InlineData(typeof(ServerAddressMissingException))]
    [InlineData(typeof(TaskCanceledException))]
    public async Task SyncSettingList_WhenOffline_LeavesListAndETagAlone(Type failure)
    {
        await Seed(Sample(40));
        await _store.Setting.SetETag("\"s1\"");
        _api.SettingListHandler = _ => throw (Exception)(Activator.CreateInstance(failure) ?? throw new InvalidOperationException("No failure."));

        await _service.SyncSettingList();

        Assert.Equal(40, (await _service.GetSettingList()).Single().GetInteger());
        Assert.Equal("\"s1\"", await _store.Setting.GetETag());
    }

    [Fact]
    public async Task SyncSettingList_WhenTheServerAnswersWithAnError_Throws_AndLeavesTheListAlone()
    {
        await Seed(Sample(40));
        _api.SettingListHandler = _ => throw new ApiException(HttpStatusCode.InternalServerError, "Broken.");

        await Assert.ThrowsAsync<ApiException>(() => _service.SyncSettingList());

        Assert.Equal(40, (await _service.GetSettingList()).Single().GetInteger());
    }

    [Fact]
    public async Task Changed_IsRaisedAfterEachStoreWrite()
    {
        List<int> countList = [];
        _service.Changed += () => countList.Add(_store.Setting.GetList().GetAwaiter().GetResult().Count);
        _api.SettingListHandler = _ => new(HttpStatusCode.OK, [Sample(40)], "\"s1\"");
        _api.UpdateSettingHandler = (_, _) => Setting(SettingKey.MergeOnRename, true, SettingValueTypeEnum.Boolean);

        await _service.SyncSettingList();
        await _service.Update(SettingKey.MergeOnRename, JsonSerializer.SerializeToElement(true));
        await _service.ApplySettingUpdated(Sample(60, Start.AddMinutes(1)));

        Assert.Equal([1, 2, 2], countList);
    }

    #endregion -- Sync --

    #region -- Update --

    [Fact]
    public async Task Update_SendsTheValue_SavesTheServersAnswer_AndRaisesChanged()
    {
        await Seed(Sample(40));
        _api.UpdateSettingHandler = (_, _) => Sample(55, Start.AddMinutes(1));
        int changed = 0;
        _service.Changed += () => changed++;

        await _service.Update(SampleKey, JsonSerializer.SerializeToElement(55));

        (string key, JsonElement sent) = Assert.Single(_api.UpdateSettingRequestList);
        Assert.Equal(SampleKey, key);
        Assert.Equal(55, sent.GetInt32());
        ResponseSetting stored = Assert.Single(await _service.GetSettingList());
        Assert.Equal(55, stored.GetInteger());
        Assert.Equal(Start.AddMinutes(1), stored.TimeModified);
        Assert.Equal(1, changed);
    }

    [Fact]
    public async Task Update_WhenOffline_Throws_AndTheLocalValueIsUnchanged()
    {
        await Seed(Sample(40));
        _api.UpdateSettingHandler = (_, _) => throw new HttpRequestException("The network is down.");
        int changed = 0;
        _service.Changed += () => changed++;

        await Assert.ThrowsAsync<HttpRequestException>(() => _service.Update(SampleKey, JsonSerializer.SerializeToElement(55)));

        Assert.Equal(40, (await _service.GetSettingList()).Single().GetInteger());
        Assert.Equal(40, await StoredSample());
        Assert.Equal(0, changed);
    }

    [Fact]
    public async Task Update_WhenTheServerRefuses_ThrowsApiException_AndTheLocalValueIsUnchanged()
    {
        await Seed(Sample(40));
        _api.UpdateSettingHandler = (_, _) => throw new ApiException(HttpStatusCode.BadRequest, "The value is not valid for the setting Volume.");

        ApiException exception = await Assert.ThrowsAsync<ApiException>(() => _service.Update(SampleKey, JsonSerializer.SerializeToElement(500)));

        Assert.Equal(HttpStatusCode.BadRequest, exception.Status);
        Assert.Equal(40, await StoredSample());
    }

    [Fact]
    public async Task Update_OfASettingThatWasNeverStored_NeverShowsAPhantomValueWhenItFails()
    {
        _api.UpdateSettingHandler = (_, _) => throw new HttpRequestException("The network is down.");

        await Assert.ThrowsAsync<HttpRequestException>(() => _service.Update(SettingKey.KeepQueuedItems, JsonSerializer.SerializeToElement(true)));

        Assert.Empty(await _service.GetSettingList());
    }

    #endregion -- Update --

    #region -- Push --

    [Fact]
    public async Task ApplySettingUpdated_SavesIt_AndKeepsTheStoredETag()
    {
        await Seed(Sample(40, Start));
        await _store.Setting.SetETag("\"s1\"");

        await _service.ApplySettingUpdated(Sample(80, Start.AddMinutes(1)));

        Assert.Equal(80, (await _service.GetSettingList()).Single().GetInteger());
        Assert.Equal("\"s1\"", await _store.Setting.GetETag());
    }

    [Fact]
    public async Task ApplySettingUpdated_ForANewKey_AddsIt()
    {
        await Seed(Sample(40));

        await _service.ApplySettingUpdated(Setting(SettingKey.KeepQueuedItems, true, SettingValueTypeEnum.Boolean, Start.AddMinutes(1)));

        Assert.Equal([SampleKey, SettingKey.KeepQueuedItems], (await _service.GetSettingList()).Select(x => x.Key));
    }

    [Fact]
    public async Task ApplySettingUpdated_WithAnOlderOrEqualTimeModified_IsIgnored()
    {
        await Seed(Sample(40, Start));
        int changed = 0;
        _service.Changed += () => changed++;

        await _service.ApplySettingUpdated(Sample(10, Start.AddMinutes(-1)));
        await _service.ApplySettingUpdated(Sample(20, Start));

        Assert.Equal(40, (await _service.GetSettingList()).Single().GetInteger());
        Assert.Equal(0, changed);
    }

    #endregion -- Push --

    #region -- Typed getters --

    [Fact]
    public async Task GetShuffle_GetRepeat_ReturnTheStoredValueOrTheDefault()
    {
        Assert.Equal(ShuffleModeEnum.Off, await _service.GetShuffle());
        Assert.Equal(RepeatModeEnum.No, await _service.GetRepeat());

        await Seed(Setting(SettingKey.Shuffle, "SpreadArtists", SettingValueTypeEnum.Enum), Setting(SettingKey.Repeat, "Forever", SettingValueTypeEnum.Enum));

        Assert.Equal(ShuffleModeEnum.SpreadArtists, await _service.GetShuffle());
        Assert.Equal(RepeatModeEnum.Forever, await _service.GetRepeat());
    }

    [Fact]
    public async Task AStoredValueThatIsNotAKnownMember_FallsBackToTheDefault()
    {
        await Seed(Setting(SettingKey.Shuffle, "NotAMode", SettingValueTypeEnum.Enum));

        Assert.Equal(ShuffleModeEnum.Off, await _service.GetShuffle());
    }

    #endregion -- Typed getters --

    #region -- Review fixes --

    [Fact]
    public async Task AStoredValueOfTheWrongKind_FallsBackToTheDefault_InsteadOfThrowing()
    {
        await Seed(Setting(SettingKey.Shuffle, 3, SettingValueTypeEnum.Enum), Setting(SettingKey.Repeat, true, SettingValueTypeEnum.Enum));


        Assert.Equal(ShuffleModeEnum.Off, await _service.GetShuffle());
        Assert.Equal(RepeatModeEnum.No, await _service.GetRepeat());
    }

    [Fact]
    public async Task Update_WhenTheLocalCopyIsNewerThanTheAnswer_KeepsTheNewerOne_AndRaisesNothing()
    {
        await Seed(Sample(50, Start.AddMinutes(5)));
        _api.UpdateSettingHandler = (_, _) => Sample(30, Start.AddMinutes(1));
        int changed = 0;
        _service.Changed += () => changed++;

        await _service.Update(SampleKey, JsonSerializer.SerializeToElement(30));

        Assert.Equal(50, await StoredSample());
        Assert.Equal(0, changed);
    }

    #endregion -- Review fixes --
}
