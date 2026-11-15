using Mediator;
using Noxy.NET.MediaPlayer.Application.Handlers.Abstractions.Metadata;
using Noxy.NET.MediaPlayer.Application.Interfaces;
using Noxy.NET.MediaPlayer.Domain.Entities;
using Noxy.NET.MediaPlayer.Domain.Responses.Media;

namespace Noxy.NET.Test.MediaPlayer.Fakes;

public sealed class FakeMetadataChangeHandler(IUnitOfWorkFactory factoryUnitOfWork, IPublisher publisher, TimeProvider time) : BaseHandlerMetadataChange<FakeCommandMetadataChange, Unit>(factoryUnitOfWork, publisher, time)
{
    public override ValueTask<Unit> Handle(FakeCommandMetadataChange command, CancellationToken cancellationToken)
    {
        return ValueTask.FromResult(Unit.Value);
    }

    public DateTime GetNow()
    {
        return Now;
    }

    public List<ResponseMediaFile> Touch(IUnitOfWork uow, IEnumerable<EntityMediaFile> fileList, DateTime now)
    {
        return TouchFiles(uow, fileList, now);
    }

    public Task PublishFiles(Guid userID, IEnumerable<ResponseMediaFile> responseList, CancellationToken cancellationToken)
    {
        return PublishFileUpdatedList(userID, responseList, cancellationToken);
    }
}
