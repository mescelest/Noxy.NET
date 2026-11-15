using Mediator;
using Noxy.NET.MediaPlayer.Application.Interfaces;
using Noxy.NET.MediaPlayer.Application.Queries.Media;
using Noxy.NET.MediaPlayer.Domain.Entities;
using Noxy.NET.MediaPlayer.Domain.Extensions;
using Noxy.NET.MediaPlayer.Domain.Responses.Media;

namespace Noxy.NET.MediaPlayer.Application.Handlers.Media;

public class HandlerMediaList(IUnitOfWorkFactory factoryUnitOfWork) : IQueryHandler<QueryMediaList, List<ResponseMediaFile>>
{
    // Newest first.
    public async ValueTask<List<ResponseMediaFile>> Handle(QueryMediaList query, CancellationToken cancellationToken)
    {
        await using IUnitOfWork uow = factoryUnitOfWork.Create();

        List<EntityMediaFile> fileList = await uow.Media.GetMediaFileList(query.UserID, query.Skip, query.Take, cancellationToken);
        return [.. fileList.Select(x => x.ToResponse())];
    }
}
