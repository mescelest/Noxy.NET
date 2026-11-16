using System.Text.Json;
using Noxy.NET.MediaPlayer.Domain.Declarations;
using Noxy.NET.MediaPlayer.Domain.Enums;
using Noxy.NET.MediaPlayer.Domain.Responses.Setting;
using Noxy.NET.MediaPlayer.Presentation.Extensions;
using Noxy.NET.MediaPlayer.Presentation.Interfaces.Services;
using Noxy.NET.MediaPlayer.Presentation.Interfaces.Stores;
using Noxy.NET.MediaPlayer.Presentation.Models;

namespace Noxy.NET.MediaPlayer.Presentation.Services;

public class SettingService(IApiClient clientApi, ISettingStore storeSetting) : ISettingService
{
    public event Action? Changed;

    public async Task<List<ResponseSetting>> GetSettingList()
    {
        return await storeSetting.GetSettingList();
    }

    public async Task<int> GetVolume()
    {
        ResponseSetting? setting = await Find(SettingKey.Volume);

        // A stored value of the wrong kind (a damaged row, a newer server) counts as no value.
        try
        {
            return setting is null ? (int)SettingCatalog.Volume.DefaultValue : setting.GetInteger();
        }
        catch (InvalidOperationException)
        {
            return (int)SettingCatalog.Volume.DefaultValue;
        }
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
        string? eTag = force || (await storeSetting.GetSettingList()).Count == 0 ? null : await storeSetting.GetSettingListETag();

        ApiResponse<List<ResponseSetting>>? response = await Fetch(() => clientApi.GetSettingList(eTag));
        if (response?.Value is null) return;

        await storeSetting.ReplaceSettingList(response.Value);
        await storeSetting.SetSettingListETag(response.ETag);
        Changed?.Invoke();
    }

    public async Task Update(string key, JsonElement value)
    {
        ResponseSetting saved = await clientApi.UpdateSetting(key, value);

        // A newer value may have arrived by push while the write was on its way: the older answer must not overwrite it.
        ResponseSetting? local = await Find(key);
        if (local is not null && saved.TimeModified < local.TimeModified) return;

        await storeSetting.SaveSetting(saved);
        Changed?.Invoke();
    }

    public async Task ApplySettingUpdated(ResponseSetting setting)
    {
        ResponseSetting? local = await Find(setting.Key);
        if (local is not null && setting.TimeModified <= local.TimeModified) return;

        await storeSetting.SaveSetting(setting);
        Changed?.Invoke();
    }

    private async Task<ResponseSetting?> Find(string key)
    {
        return (await storeSetting.GetSettingList()).FirstOrDefault(x => x.Key == key);
    }

    // A stored name that is not a member of the enum (a newer server) counts as no value.
    private async Task<TEnum> GetEnum<TEnum>(string key, object defaultValue) where TEnum : struct, Enum
    {
        ResponseSetting? setting = await Find(key);
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

    // Null when the server could not be reached or answered 304. Any other failure propagates.
    private static async Task<ApiResponse<T>?> Fetch<T>(Func<Task<ApiResponse<T>>> call)
    {
        try
        {
            return await call();
        }
        catch (Exception exception) when (exception.IsOfflineFailure)
        {
            return null;
        }
    }
}
