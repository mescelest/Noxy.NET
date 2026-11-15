using Noxy.NET.MediaPlayer.Application.Interfaces.Repositories;
using Noxy.NET.MediaPlayer.Domain.Entities;

namespace Noxy.NET.Test.MediaPlayer.Fakes;

public sealed class FakeSettingsRepository(FakeStore store) : ISettingsRepository
{
    private readonly List<EntityUserSettings> _pendingAddList = [];
    private readonly List<EntityUserSettings> _pendingUpdateList = [];

    public void Flush()
    {
        store.UserSettingsList.AddRange(_pendingAddList);
        _pendingAddList.Clear();

        foreach (EntityUserSettings updated in _pendingUpdateList)
        {
            int index = store.UserSettingsList.FindIndex(x => x.ID == updated.ID);
            if (index >= 0) store.UserSettingsList[index] = updated;
        }

        _pendingUpdateList.Clear();
    }

    public Task<EntityUserSettings?> FindSettings(Guid userID, CancellationToken cancellationToken = default)
    {
        EntityUserSettings? found = store.UserSettingsList.FirstOrDefault(x => x.UserID == userID);

        return Task.FromResult(found is null ? null : Copy(found));
    }

    public void AddSettings(EntityUserSettings settings)
    {
        _pendingAddList.Add(Copy(settings));
    }

    public void UpdateSettings(EntityUserSettings settings)
    {
        _pendingUpdateList.Add(Copy(settings));
    }

    private static EntityUserSettings Copy(EntityUserSettings settings)
    {
        return new()
        {
            ID = settings.ID,
            TimeCreated = settings.TimeCreated,
            UserID = settings.UserID,
            AutoMergeOnRename = settings.AutoMergeOnRename,
            TimeModified = settings.TimeModified
        };
    }
}
