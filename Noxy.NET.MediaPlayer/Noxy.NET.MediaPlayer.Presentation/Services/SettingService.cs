using System.Text.Json;
using Noxy.NET.MediaPlayer.Domain.Declarations;
using Noxy.NET.MediaPlayer.Domain.Enums;
using Noxy.NET.MediaPlayer.Domain.Responses.Setting;
using Noxy.NET.MediaPlayer.Presentation.Extensions;
using Noxy.NET.MediaPlayer.Presentation.Interfaces.Services;
using Noxy.NET.MediaPlayer.Presentation.Interfaces.Stores;

namespace Noxy.NET.MediaPlayer.Presentation.Services;

public class SettingService : ISettingService
{
    private readonly IApiClient _clientApi;
    private readonly SyncedList<ResponseSetting, string> _settingList;

    public SettingService(IApiClient clientApi, IListStore<ResponseSetting, string> storeSetting)
    {
        _clientApi = clientApi;
        _settingList = new(storeSetting, clientApi.GetSettingList, x => x.Key, x => x.TimeModified);
        _settingList.Changed += RaiseChanged;
    }

    public event Action? Changed;

    public async Task<List<ResponseSetting>> GetSettingList()
    {
        return await _settingList.GetList();
    }

    public async Task<ShuffleModeEnum> GetShuffle()
    {
        return await GetEnum<ShuffleModeEnum>(SettingKey.Shuffle, SettingCatalog.Shuffle.DefaultValue);
    }

    public async Task<RepeatModeEnum> GetRepeat()
    {
        return await GetEnum<RepeatModeEnum>(SettingKey.Repeat, SettingCatalog.Repeat.DefaultValue);
    }

    public async Task SyncSettingList(bool force = false)
    {
        await _settingList.Sync(force);
    }

    public async Task Update(string key, JsonElement value)
    {
        ResponseSetting saved = await _clientApi.UpdateSetting(key, value);

        // A newer value may have arrived by push while the write was on its way: the older answer must not overwrite it.
        await _settingList.ApplyUpdated(saved, true);
    }

    public async Task ApplySettingUpdated(ResponseSetting setting)
    {
        await _settingList.ApplyUpdated(setting);
    }

    // A stored name that is not a member of the enum (a newer server) counts as no value.
    private async Task<TEnum> GetEnum<TEnum>(string key, object defaultValue) where TEnum : struct, Enum
    {
        ResponseSetting? setting = await _settingList.Find(key);
        try
        {
            if (setting is not null && Enum.TryParse(setting.GetEnumName(), out TEnum stored) && Enum.IsDefined(stored)) return stored;
        }
        catch (InvalidOperationException)
        {
            // Not a name at all: the default below.
        }

        return Enum.Parse<TEnum>((string)defaultValue);
    }

    private void RaiseChanged()
    {
        Changed?.Invoke();
    }
}
