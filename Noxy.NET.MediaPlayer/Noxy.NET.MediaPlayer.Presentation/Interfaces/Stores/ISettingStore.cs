using Noxy.NET.MediaPlayer.Domain.Responses.Setting;

namespace Noxy.NET.MediaPlayer.Presentation.Interfaces.Stores;

public interface ISettingStore
{
    Task<List<ResponseSetting>> GetSettingList();

    // One atomic write that keeps the order of the list.
    Task ReplaceSettingList(List<ResponseSetting> settingList);

    // Inserts the setting or replaces the one with the same key, in place.
    Task SaveSetting(ResponseSetting setting);

    // Only ever one that arrived with a full 200 response the device downloaded. Null clears it.
    Task<string?> GetSettingListETag();

    Task SetSettingListETag(string? eTag);
}
