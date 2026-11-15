using Noxy.NET.MediaPlayer.Domain.Entities;

namespace Noxy.NET.MediaPlayer.Application.Interfaces.Repositories;

public interface ISettingRepository
{
    // The saved rows of the owner, each as its own derived type. A setting with no row is simply absent.
    Task<List<EntityUserSetting>> GetSettingList(Guid userID, CancellationToken cancellationToken = default);

    // Scoped to the owner, and the key is exact.
    Task<EntityUserSetting?> FindSetting(Guid userID, string key, CancellationToken cancellationToken = default);

    // A second row for the same user and key makes the commit throw MetadataKeyConflictException.
    void AddSetting(EntityUserSetting setting);

    void UpdateSetting(EntityUserSetting setting);
}
