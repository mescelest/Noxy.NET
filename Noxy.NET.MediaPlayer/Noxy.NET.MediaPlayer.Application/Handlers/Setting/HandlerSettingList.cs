using Mediator;
using Noxy.NET.MediaPlayer.Application.Interfaces;
using Noxy.NET.MediaPlayer.Application.Interfaces.Services;
using Noxy.NET.MediaPlayer.Application.Models.Setting;
using Noxy.NET.MediaPlayer.Application.Queries.Setting;
using Noxy.NET.MediaPlayer.Domain.Declarations;
using Noxy.NET.MediaPlayer.Domain.Entities;
using Noxy.NET.MediaPlayer.Domain.Extensions;

namespace Noxy.NET.MediaPlayer.Application.Handlers.Setting;

public class HandlerSettingList(IUnitOfWorkFactory factoryUnitOfWork, IETagService serviceETag) : IQueryHandler<QuerySettingList, SettingLibrary>
{
    // Every declared setting of the user in catalog order, the saved value or the default. The ETag follows the saved rows that belong to a declared setting, so it changes with every change and ignores rows the code no longer knows.
    public async ValueTask<SettingLibrary> Handle(QuerySettingList query, CancellationToken cancellationToken)
    {
        await using IUnitOfWork uow = factoryUnitOfWork.Create();

        List<EntityUserSetting> savedList = [.. (await uow.Setting.GetSettingList(query.UserID, cancellationToken)).Where(x => SettingCatalog.Find(x.Key) is not null)];
        string eTag = serviceETag.Compute(savedList.Select(x => (x.ID, x.TimeModified)));

        if (!string.IsNullOrWhiteSpace(query.IfNoneMatch) && serviceETag.Matches(query.IfNoneMatch, eTag)) return new(eTag, null);

        return new(eTag, [.. SettingCatalog.DeclarationList.Select(x => x.ToResponse(savedList.FirstOrDefault(s => s.Key == x.Key)))]);
    }
}
