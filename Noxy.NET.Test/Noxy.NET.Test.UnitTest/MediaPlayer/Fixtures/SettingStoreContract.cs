using System.Text.Json;
using Noxy.NET.MediaPlayer.Domain.Declarations;
using Noxy.NET.MediaPlayer.Domain.Enums;
using Noxy.NET.MediaPlayer.Domain.Responses.Setting;
using Noxy.NET.MediaPlayer.Presentation.Interfaces.Stores;

namespace Noxy.NET.Test.MediaPlayer.Fixtures;

public abstract class SettingStoreContract
{
    private static readonly DateTime Start = new(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);

    // Any key will do: the store keeps what it is given.
    private const string SampleKey = "SampleInteger";

    protected abstract ISettingStore CreateStore();

    private static ResponseSetting Setting(string key, object value, SettingValueTypeEnum valueType, DateTime? timeModified = null)
    {
        return new() { Key = key, ValueType = valueType, Value = JsonSerializer.SerializeToElement(value), TimeModified = timeModified ?? Start };
    }

    private static ResponseSetting Volume(int value, DateTime? timeModified = null)
    {
        return Setting(SampleKey, value, SettingValueTypeEnum.Integer, timeModified);
    }

    [Fact]
    public async Task AnEmptyStore_HasNoSettingsAndNoETag()
    {
        ISettingStore store = CreateStore();

        Assert.Empty(await store.GetSettingList());
        Assert.Null(await store.GetSettingListETag());
    }

    [Fact]
    public async Task ReplaceSettingList_ReplacesRatherThanMerges_AndKeepsTheOrder()
    {
        ISettingStore store = CreateStore();
        await store.ReplaceSettingList([Volume(10), Setting(SettingKey.Shuffle, "Random", SettingValueTypeEnum.Enum)]);

        await store.ReplaceSettingList([Setting(SettingKey.MergeOnRename, true, SettingValueTypeEnum.Boolean), Setting(SettingKey.Repeat, "Forever", SettingValueTypeEnum.Enum), Volume(55)]);

        List<ResponseSetting> list = await store.GetSettingList();
        Assert.Equal([SettingKey.MergeOnRename, SettingKey.Repeat, SampleKey], list.Select(x => x.Key));
    }

    [Fact]
    public async Task ReplaceSettingList_WithAnEmptyList_ClearsTheStore()
    {
        ISettingStore store = CreateStore();
        await store.ReplaceSettingList([Volume(10)]);

        await store.ReplaceSettingList([]);

        Assert.Empty(await store.GetSettingList());
    }

    [Fact]
    public async Task SaveSetting_InsertsANewKeyAtTheEnd_AndReplacesAnExistingKeyInPlace()
    {
        ISettingStore store = CreateStore();
        await store.ReplaceSettingList([Volume(10), Setting(SettingKey.Shuffle, "Off", SettingValueTypeEnum.Enum)]);

        await store.SaveSetting(Setting(SettingKey.MergeOnRename, true, SettingValueTypeEnum.Boolean));
        await store.SaveSetting(Volume(90, Start.AddMinutes(1)));

        List<ResponseSetting> list = await store.GetSettingList();
        Assert.Equal([SampleKey, SettingKey.Shuffle, SettingKey.MergeOnRename], list.Select(x => x.Key));
        Assert.Equal(90, ((JsonElement)list[0].Value).GetInt32());
        Assert.Equal(Start.AddMinutes(1), list[0].TimeModified);
    }

    [Fact]
    public async Task ABoolean_AnInteger_AndAnEnumValue_ComeBackEqual()
    {
        ISettingStore store = CreateStore();
        await store.ReplaceSettingList([Setting(SettingKey.MergeOnRename, true, SettingValueTypeEnum.Boolean), Volume(40), Setting(SettingKey.Repeat, "Forever", SettingValueTypeEnum.Enum)]);

        List<ResponseSetting> list = await store.GetSettingList();

        JsonElement boolean = Assert.IsType<JsonElement>(list[0].Value);
        JsonElement integer = Assert.IsType<JsonElement>(list[1].Value);
        JsonElement text = Assert.IsType<JsonElement>(list[2].Value);
        Assert.Equal(JsonValueKind.True, boolean.ValueKind);
        Assert.Equal(40, integer.GetInt32());
        Assert.Equal("Forever", text.GetString());
        Assert.Equal([SettingValueTypeEnum.Boolean, SettingValueTypeEnum.Integer, SettingValueTypeEnum.Enum], list.Select(x => x.ValueType));
        Assert.All(list, x => Assert.Equal(Start, x.TimeModified));
    }

    [Fact]
    public async Task TheETag_RoundTrips_AndNullClearsIt()
    {
        ISettingStore store = CreateStore();

        await store.SetSettingListETag("\"abc\"");
        Assert.Equal("\"abc\"", await store.GetSettingListETag());

        await store.SetSettingListETag(null);
        Assert.Null(await store.GetSettingListETag());
    }

    [Fact]
    public async Task TheETag_IsNotTouchedByWritingSettings()
    {
        ISettingStore store = CreateStore();
        await store.SetSettingListETag("\"abc\"");

        await store.SaveSetting(Volume(1));
        await store.ReplaceSettingList([Volume(2)]);

        Assert.Equal("\"abc\"", await store.GetSettingListETag());
    }
}
