using Microsoft.EntityFrameworkCore;
using Noxy.NET.MediaPlayer.Application.Interfaces.Repositories;
using Noxy.NET.MediaPlayer.Domain.Entities;
using Noxy.NET.MediaPlayer.Persistence.Interfaces.Mappers;
using Noxy.NET.MediaPlayer.Persistence.Tables.Setting;

namespace Noxy.NET.MediaPlayer.Persistence.Repositories;

public class SettingRepository(DataContext context, IEntityToTableMapper mapperE2T, ITableToEntityMapper mapperT2E) : ISettingRepository
{
    public async Task<List<EntityUserSetting>> GetSettingList(Guid userID, CancellationToken cancellationToken = default)
    {
        List<TableUserSetting> list = await context.UserSetting.AsNoTracking()
            .Where(x => x.UserID == userID)
            .ToListAsync(cancellationToken);

        return [.. list.Select(mapperT2E.Map)];
    }

    public async Task<EntityUserSetting?> FindSetting(Guid userID, string key, CancellationToken cancellationToken = default)
    {
        TableUserSetting? table = await context.UserSetting.AsNoTracking()
            .FirstOrDefaultAsync(x => x.UserID == userID && x.Key == key, cancellationToken);

        return table is null ? null : mapperT2E.Map(table);
    }

    public void AddSetting(EntityUserSetting setting)
    {
        context.UserSetting.Add(mapperE2T.Map(setting));
    }

    public void UpdateSetting(EntityUserSetting setting)
    {
        context.UserSetting.Update(mapperE2T.Map(setting));
    }
}
