using System.Text.Json;
using Noxy.NET.MediaPlayer.Domain.Enums;
using Noxy.NET.MediaPlayer.Domain.Responses.Setting;

namespace Noxy.NET.MediaPlayer.Presentation.Interfaces.Services;

public interface ISettingService
{
    // Raised after the local copy changed.
    event Action? Changed;

    // The stored settings, in the order the server sent them.
    Task<List<ResponseSetting>> GetSettingList();

    Task<ShuffleModeEnum> GetShuffle();

    Task<RepeatModeEnum> GetRepeat();

    Task<bool> GetKeepQueuedItems();

    // Whether this device writes the current metadata into the files it holds by itself. A stored value of the wrong kind counts as no value.
    Task<bool> GetUpdateFileMetadata();

    // How many queue entries ahead of the current one are downloaded in advance. A stored value of the wrong kind or outside the range counts as no value.
    Task<int> GetPrefetchCount();

    // Sends the stored ETag unless force is set or the local list is empty. A failed network call changes nothing.
    Task SyncSettingList(bool force = false);

    // Throws when the server cannot be reached (HttpRequestException and the like) or refuses the value (ApiException): nothing is saved then.
    // The server is the authority: only its answer is saved, so a refused or offline write leaves the local value as it was.
    Task Update(string key, JsonElement value);

    // The push handler: never writes an ETag, and an older or equal TimeModified is ignored.
    Task ApplySettingUpdated(ResponseSetting setting);
}
