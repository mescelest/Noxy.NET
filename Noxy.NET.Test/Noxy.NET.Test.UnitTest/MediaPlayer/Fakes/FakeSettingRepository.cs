using Noxy.NET.MediaPlayer.Application.Interfaces.Repositories;
using Noxy.NET.MediaPlayer.Domain.Entities;
using Noxy.NET.MediaPlayer.Domain.Exceptions;

namespace Noxy.NET.Test.MediaPlayer.Fakes;

public sealed class FakeSettingRepository(FakeStore store) : ISettingRepository
{
    private readonly List<EntityUserSetting> _pendingAddList = [];
    private readonly List<EntityUserSetting> _pendingUpdateList = [];

    public void Flush()
    {
        // The unique index: a second row with the same user and key is rejected, whatever its type.
        foreach (EntityUserSetting added in _pendingAddList)
        {
            if (store.UserSettingList.Any(x => x.UserID == added.UserID && x.Key == added.Key)) throw new MetadataKeyConflictException();

            store.UserSettingList.Add(added);
        }

        _pendingAddList.Clear();

        foreach (EntityUserSetting updated in _pendingUpdateList)
        {
            int index = store.UserSettingList.FindIndex(x => x.ID == updated.ID);
            if (index >= 0) store.UserSettingList[index] = updated;
        }

        _pendingUpdateList.Clear();
    }

    public Task<List<EntityUserSetting>> GetSettingList(Guid userID, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(store.UserSettingList.Where(x => x.UserID == userID).Select(Copy).ToList());
    }

    public Task<EntityUserSetting?> FindSetting(Guid userID, string key, CancellationToken cancellationToken = default)
    {
        EntityUserSetting? found = store.UserSettingList.FirstOrDefault(x => x.UserID == userID && x.Key == key);

        return Task.FromResult(found is null ? null : Copy(found));
    }

    public void AddSetting(EntityUserSetting setting)
    {
        _pendingAddList.Add(Copy(setting));
    }

    public void UpdateSetting(EntityUserSetting setting)
    {
        _pendingUpdateList.Add(Copy(setting));
    }

    private static EntityUserSetting Copy(EntityUserSetting setting)
    {
        return setting switch
        {
            EntityUserSettingBoolean boolean => new EntityUserSettingBoolean { ID = boolean.ID, TimeCreated = boolean.TimeCreated, UserID = boolean.UserID, Key = boolean.Key, TimeModified = boolean.TimeModified, Value = boolean.Value },
            EntityUserSettingInteger integer => new EntityUserSettingInteger { ID = integer.ID, TimeCreated = integer.TimeCreated, UserID = integer.UserID, Key = integer.Key, TimeModified = integer.TimeModified, Value = integer.Value },
            EntityUserSettingEnum member => new EntityUserSettingEnum { ID = member.ID, TimeCreated = member.TimeCreated, UserID = member.UserID, Key = member.Key, TimeModified = member.TimeModified, Value = member.Value },
            _ => throw new NotSupportedException($"The fake cannot copy {setting.GetType().Name}.")
        };
    }
}
