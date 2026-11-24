using System.Text.Json;
using Noxy.NET.MediaPlayer.Domain.Enums;
using Noxy.NET.MediaPlayer.Domain.Responses.Setting;
using Noxy.NET.MediaPlayer.Presentation.Interfaces.Services;

namespace Noxy.NET.Test.MediaPlayer.Fakes;

public sealed class FakeSettingService : ISettingService
{
    public event Action? Changed;

    public List<ResponseSetting> SettingList { get; } = [];

    // What the typed getters answer, as the stored value or the default would.
    public ShuffleModeEnum Shuffle { get; set; } = ShuffleModeEnum.Off;

    public RepeatModeEnum Repeat { get; set; } = RepeatModeEnum.No;

    public bool KeepQueuedItems { get; set; }

    public int PrefetchCount { get; set; } = 3;

    // Answers each Update call, or throws to script a failure. The default accepts it.
    public Func<string, JsonElement, Task> UpdateHandler { get; set; } = (_, _) => Task.CompletedTask;

    public List<(string Key, JsonElement Value)> UpdateRequestList { get; } = [];

    public List<bool> SyncRequestList { get; } = [];

    // Runs inside SyncSettingList, so a test can record what had happened by then.
    public Action? OnSyncSettingList { get; set; }

    // Thrown by SyncSettingList, after it is recorded, when set.
    public Exception? SyncSettingListException { get; set; }

    public List<ResponseSetting> AppliedList { get; } = [];

    public void RaiseChanged()
    {
        Changed?.Invoke();
    }

    public Task<List<ResponseSetting>> GetSettingList()
    {
        return Task.FromResult(SettingList.ToList());
    }

    public Task<ShuffleModeEnum> GetShuffle()
    {
        return Task.FromResult(Shuffle);
    }

    public Task<RepeatModeEnum> GetRepeat()
    {
        return Task.FromResult(Repeat);
    }

    public Task<bool> GetKeepQueuedItems()
    {
        return Task.FromResult(KeepQueuedItems);
    }

    public Task<int> GetPrefetchCount()
    {
        return Task.FromResult(PrefetchCount);
    }

    public Task SyncSettingList(bool force = false)
    {
        SyncRequestList.Add(force);
        OnSyncSettingList?.Invoke();
        if (SyncSettingListException is not null) throw SyncSettingListException;

        return Task.CompletedTask;
    }

    public Task Update(string key, JsonElement value)
    {
        UpdateRequestList.Add((key, value));
        return UpdateHandler(key, value);
    }

    public Task ApplySettingUpdated(ResponseSetting setting)
    {
        AppliedList.Add(setting);
        return Task.CompletedTask;
    }
}
