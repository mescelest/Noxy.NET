using System.Text.Json;
using Noxy.NET.MediaPlayer.Domain.Declarations;
using Noxy.NET.MediaPlayer.Domain.Enums;
using Noxy.NET.MediaPlayer.Domain.Responses.Setting;
using Noxy.NET.MediaPlayer.Presentation.Interfaces.Stores;

namespace Noxy.NET.Test.MediaPlayer.Fixtures;

public abstract class SettingListStoreContract : ListStoreContract<ResponseSetting, string>
{
    // The label is the value: the store keeps whatever it is given.
    protected override string NewKey()
    {
        return $"Key-{Guid.CreateVersion7()}";
    }

    protected override ResponseSetting MakeItem(string label, string key)
    {
        return new() { Key = key, ValueType = SettingValueTypeEnum.Enum, Value = JsonSerializer.SerializeToElement(label), TimeModified = Start };
    }

    protected override string KeyOf(ResponseSetting item)
    {
        return item.Key;
    }

    protected override string LabelOf(ResponseSetting item)
    {
        return ((JsonElement)item.Value).GetString() ?? string.Empty;
    }

    [Fact]
    public async Task ABoolean_AnInteger_AndAnEnumValue_ComeBackEqual()
    {
        IListStore<ResponseSetting, string> store = CreateStore();
        await store.ReplaceList(
        [
            new() { Key = SettingKey.MergeOnRename, ValueType = SettingValueTypeEnum.Boolean, Value = JsonSerializer.SerializeToElement(true), TimeModified = Start },
            new() { Key = "SampleInteger", ValueType = SettingValueTypeEnum.Integer, Value = JsonSerializer.SerializeToElement(40), TimeModified = Start },
            new() { Key = SettingKey.Repeat, ValueType = SettingValueTypeEnum.Enum, Value = JsonSerializer.SerializeToElement("Forever"), TimeModified = Start }
        ]);

        List<ResponseSetting> list = await store.GetList();

        Assert.Equal(JsonValueKind.True, Assert.IsType<JsonElement>(list[0].Value).ValueKind);
        Assert.Equal(40, Assert.IsType<JsonElement>(list[1].Value).GetInt32());
        Assert.Equal("Forever", Assert.IsType<JsonElement>(list[2].Value).GetString());
        Assert.Equal([SettingValueTypeEnum.Boolean, SettingValueTypeEnum.Integer, SettingValueTypeEnum.Enum], list.Select(x => x.ValueType));
        Assert.All(list, x => Assert.Equal(Start, x.TimeModified));
    }
}
