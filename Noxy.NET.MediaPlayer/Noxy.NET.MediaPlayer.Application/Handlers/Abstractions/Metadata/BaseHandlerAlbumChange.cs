using Mediator;
using Noxy.NET.MediaPlayer.Application.Extensions;
using Noxy.NET.MediaPlayer.Application.Interfaces;
using Noxy.NET.MediaPlayer.Application.Interfaces.Services;
using Noxy.NET.MediaPlayer.Domain.Declarations;
using Noxy.NET.MediaPlayer.Domain.Entities;
using Noxy.NET.MediaPlayer.Domain.Exceptions;

namespace Noxy.NET.MediaPlayer.Application.Handlers.Abstractions.Metadata;

public abstract class BaseHandlerAlbumChange<TCommand, TResponse>(
    IUnitOfWorkFactory factoryUnitOfWork,
    INameKeyService serviceNameKey,
    IMetadataChangeService serviceMetadataChange,
    IPublisher publisher,
    TimeProvider time) : BaseHandlerMetadataChange<TCommand, TResponse>(factoryUnitOfWork, publisher, time) where TCommand : ICommand<TResponse>
{
    public const int MaxTitleLength = 255;

    protected INameKeyService ServiceNameKey { get; } = serviceNameKey;
    protected IMetadataChangeService ServiceMetadataChange { get; } = serviceMetadataChange;

    protected async Task<EntityAlbum> Load(IUnitOfWork uow, Guid userID, Guid id, CancellationToken cancellationToken)
    {
        return await uow.Metadata.FindAlbum(id, userID, cancellationToken) ?? throw new MetadataItemNotFoundException();
    }

    // The title as it is stored and its key. A title with no key (blank) is never stored.
    protected (string Title, string Key) NormalizeTitle(string title)
    {
        string trimmed = title.Trim();
        string key = ServiceNameKey.GetKey(trimmed);
        if (key.Length == 0) throw new ArgumentException("The title cannot be blank.", nameof(title));
        if (trimmed.Length > MaxTitleLength) throw new ArgumentException($"The title can be at most {MaxTitleLength} characters.", nameof(title));

        return (trimmed, key);
    }

    // The artists in the order given, without repeats. An unknown or another user's artist is a MetadataItemNotFoundException.
    protected static async Task<List<EntityArtist>> LoadArtistList(IUnitOfWork uow, Guid userID, IEnumerable<Guid> idList, CancellationToken cancellationToken)
    {
        List<EntityArtist> result = [];
        foreach (Guid id in idList.Distinct())
        {
            result.Add(await uow.Metadata.FindArtist(id, userID, cancellationToken) ?? throw new MetadataItemNotFoundException());
        }

        return result;
    }

    protected static async Task<bool> IsMergeOnRename(IUnitOfWork uow, Guid userID, CancellationToken cancellationToken)
    {
        return await uow.GetBoolean(userID, SettingCatalog.MergeOnRename, cancellationToken);
    }
}
