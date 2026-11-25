using Mediator;
using Noxy.NET.MediaPlayer.Application.Extensions;
using Noxy.NET.MediaPlayer.Application.Interfaces;
using Noxy.NET.MediaPlayer.Application.Models.Metadata;
using Noxy.NET.MediaPlayer.Application.Notifications.Media;
using Noxy.NET.MediaPlayer.Domain.Entities;
using Noxy.NET.MediaPlayer.Domain.Exceptions;
using Noxy.NET.MediaPlayer.Domain.Extensions;
using Noxy.NET.MediaPlayer.Domain.Responses.Media;

namespace Noxy.NET.MediaPlayer.Application.Handlers.Abstractions.Metadata;

public abstract class BaseHandlerMetadataChange<TCommand, TResponse>(IUnitOfWorkFactory factoryUnitOfWork, IPublisher publisher, TimeProvider time) : ICommandHandler<TCommand, TResponse> where TCommand : ICommand<TResponse>
{
    protected IUnitOfWorkFactory FactoryUnitOfWork { get; } = factoryUnitOfWork;
    protected IPublisher Publisher { get; } = publisher;

    protected DateTime Now => time.GetUtcNow().UtcDateTime;

    public abstract ValueTask<TResponse> Handle(TCommand command, CancellationToken cancellationToken);

    // A file whose response changes must change its modified time in the same commit, which is what keeps the library ETag right. The files are saved with the unit of work. The responses are built from the files as they are now, so the links are changed in memory first and this is called after. A file listed twice is touched once.
    protected List<ResponseMediaFile> TouchFiles(IUnitOfWork uow, IEnumerable<EntityMediaFile> fileList, DateTime now)
    {
        List<ResponseMediaFile> responseList = [];

        foreach (EntityMediaFile file in fileList.DistinctBy(x => x.ID))
        {
            file.TimeModified = now;
            uow.Media.UpdateMediaFile(file);
            responseList.Add(file.ToResponse());
        }

        return responseList;
    }

    // Runs the use case in a fresh unit of work (again when it loses a race to create the same item), and publishes what it changed after it committed. A use case that changed nothing publishes nothing.
    protected async Task<TResult> Execute<TResult>(Guid userID, Func<IUnitOfWork, Task<MetadataChangeResult<TResult>>> work, CancellationToken cancellationToken)
    {
        MetadataChangeResult<TResult> result = await FactoryUnitOfWork.RunWithKeyConflictRetry(work, cancellationToken);

        if (result.Changed) await PublishChangeSet(userID, result.ChangeSet, result.MediaFileList, cancellationToken);

        return result.Response;
    }

    // The files of the user with the given IDs, in that order without repeats, as the change set own copies. An unknown file or the file of another user is a MediaNotFoundException, so nothing changes.
    protected static async Task<List<EntityMediaFile>> LoadMediaFileList(IUnitOfWork uow, Guid userID, IEnumerable<Guid> idList, MetadataChangeSet changeSet, CancellationToken cancellationToken)
    {
        List<EntityMediaFile> result = [];
        foreach (Guid id in idList.Distinct())
        {
            result.Add(changeSet.Adopt(await uow.Media.FindMediaFile(id, userID, cancellationToken) ?? throw new MediaNotFoundException()));
        }

        return result;
    }

    // The files the change set marked get their new modified time.
    protected List<ResponseMediaFile> TouchChangeSet(IUnitOfWork uow, MetadataChangeSet changeSet, DateTime now)
    {
        return TouchFiles(uow, changeSet.TouchedFileList, now);
    }

    // After the commit, so a device is never told about a change that was not saved.
    protected async Task PublishFileUpdatedList(Guid userID, IEnumerable<ResponseMediaFile> responseList, CancellationToken cancellationToken)
    {
        foreach (ResponseMediaFile response in responseList)
        {
            await Publisher.Publish(new NotificationMediaUpdated(userID, response), cancellationToken);
        }
    }

    // Items first, then the files whose text changed with them.
    protected async Task PublishChangeSet(Guid userID, MetadataChangeSet changeSet, IEnumerable<ResponseMediaFile> mediaResponseList, CancellationToken cancellationToken)
    {
        await Publisher.PublishItemList(userID, changeSet, cancellationToken);

        await PublishFileUpdatedList(userID, mediaResponseList, cancellationToken);
    }
}
