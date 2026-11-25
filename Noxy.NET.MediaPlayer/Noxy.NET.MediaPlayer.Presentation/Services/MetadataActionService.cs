using System.ComponentModel;
using Noxy.NET.MediaPlayer.Application.Requests.Metadata;
using Noxy.NET.MediaPlayer.Presentation.Enums;
using Noxy.NET.MediaPlayer.Presentation.Interfaces.Services;
using Noxy.NET.MediaPlayer.Presentation.Models;

namespace Noxy.NET.MediaPlayer.Presentation.Services;

public class MetadataActionService(IApiClient clientApi) : IMetadataActionService
{
    public bool IsBusy { get; private set; }

    public event Action? BusyChanged;

    public async Task<Guid> Create(MetadataKindEnum kind, string name, List<Guid>? albumArtistIDList)
    {
        return await Run(async () => kind switch
        {
            MetadataKindEnum.Artist => (await clientApi.CreateArtist(new() { Name = name })).ID,
            MetadataKindEnum.Album => (await clientApi.CreateAlbum(new() { Title = name, ArtistIDList = albumArtistIDList ?? [] })).ID,
            MetadataKindEnum.Genre => (await clientApi.CreateGenre(new() { Name = name })).ID,
            _ => throw new InvalidEnumArgumentException(nameof(kind), (int)kind, typeof(MetadataKindEnum))
        });
    }

    public async Task Rename(MetadataKindEnum kind, Guid id, string name, List<Guid>? albumArtistIDList)
    {
        await Run(async () =>
        {
            switch (kind)
            {
                case MetadataKindEnum.Artist:
                    await clientApi.UpdateArtist(id, new() { Name = name });
                    break;
                case MetadataKindEnum.Album:
                    RequestUpdateAlbum request = new() { Title = new(name) };
                    if (albumArtistIDList is not null) request.ArtistIDList = new(albumArtistIDList);
                    await clientApi.UpdateAlbum(id, request);
                    break;
                case MetadataKindEnum.Genre:
                    await clientApi.UpdateGenre(id, new() { Name = name });
                    break;
                default:
                    throw new InvalidEnumArgumentException(nameof(kind), (int)kind, typeof(MetadataKindEnum));
            }

            return 0;
        });
    }

    public async Task Delete(MetadataKindEnum kind, Guid id)
    {
        await Run(async () =>
        {
            switch (kind)
            {
                case MetadataKindEnum.Artist:
                    await clientApi.DeleteArtist(id);
                    break;
                case MetadataKindEnum.Album:
                    await clientApi.DeleteAlbum(id);
                    break;
                case MetadataKindEnum.Genre:
                    await clientApi.DeleteGenre(id);
                    break;
                default:
                    throw new InvalidEnumArgumentException(nameof(kind), (int)kind, typeof(MetadataKindEnum));
            }

            return 0;
        });
    }

    public async Task Merge(MetadataKindEnum kind, Guid sourceID, Guid targetID)
    {
        await Run(async () =>
        {
            await SendMerge(kind, sourceID, targetID);
            return 0;
        });
    }

    public async Task MergeList(MetadataKindEnum kind, List<Guid> sourceIDList, Guid targetID)
    {
        if (sourceIDList.Contains(targetID)) throw new ArgumentException("An item cannot be merged into itself.", nameof(sourceIDList));
        if (sourceIDList.Count == 0) return;

        await Run(async () =>
        {
            foreach (Guid sourceID in sourceIDList)
            {
                await SendMerge(kind, sourceID, targetID);
            }

            return 0;
        });
    }

    private async Task SendMerge(MetadataKindEnum kind, Guid sourceID, Guid targetID)
    {
        RequestMergeItem request = new() { TargetID = targetID };

        switch (kind)
        {
            case MetadataKindEnum.Artist:
                await clientApi.MergeArtist(sourceID, request);
                break;
            case MetadataKindEnum.Album:
                await clientApi.MergeAlbum(sourceID, request);
                break;
            case MetadataKindEnum.Genre:
                await clientApi.MergeGenre(sourceID, request);
                break;
            default:
                throw new InvalidEnumArgumentException(nameof(kind), (int)kind, typeof(MetadataKindEnum));
        }
    }

    public async Task SetAlbumArt(Guid albumID, byte[] picture, string contentType)
    {
        await Run(async () =>
        {
            await clientApi.SetAlbumArt(albumID, picture, contentType);
            return 0;
        });
    }

    public async Task Apply(MetadataKindEnum kind, List<MetadataChange> changeList)
    {
        foreach (MetadataChange change in changeList)
        {
            EnsureWithinLimit(change.MediaFileIDList);
        }

        await Run(async () =>
        {
            foreach (MetadataChange change in changeList)
            {
                if (change.IsAdd) await SendAdd(kind, change.ItemID, change.MediaFileIDList);
                else await SendRemove(kind, change.ItemID, change.MediaFileIDList);
            }

            return 0;
        });
    }

    // Adds an artist or a genre to the media files, and sets the album of them.
    private async Task SendAdd(MetadataKindEnum kind, Guid itemID, List<Guid> mediaFileIDList)
    {
        RequestAddMediaFileList request = new() { MediaFileIDList = mediaFileIDList };

        switch (kind)
        {
            case MetadataKindEnum.Artist:
                await clientApi.AddArtistMediaFileList(itemID, request);
                break;
            case MetadataKindEnum.Album:
                await clientApi.AddAlbumMediaFileList(itemID, request);
                break;
            case MetadataKindEnum.Genre:
                await clientApi.AddGenreMediaFileList(itemID, request);
                break;
            default:
                throw new InvalidEnumArgumentException(nameof(kind), (int)kind, typeof(MetadataKindEnum));
        }
    }

    private async Task SendRemove(MetadataKindEnum kind, Guid itemID, List<Guid> mediaFileIDList)
    {
        RequestRemoveMediaFileList request = new() { MediaFileIDList = mediaFileIDList };

        switch (kind)
        {
            case MetadataKindEnum.Artist:
                await clientApi.RemoveArtistMediaFileList(itemID, request);
                break;
            case MetadataKindEnum.Album:
                await clientApi.RemoveAlbumMediaFileList(itemID, request);
                break;
            case MetadataKindEnum.Genre:
                await clientApi.RemoveGenreMediaFileList(itemID, request);
                break;
            default:
                throw new InvalidEnumArgumentException(nameof(kind), (int)kind, typeof(MetadataKindEnum));
        }
    }

    private static void EnsureWithinLimit(List<Guid> mediaFileIDList)
    {
        if (mediaFileIDList.Count > PresentationConstants.OrganiseSelectionLimit)
        {
            throw new ArgumentException($"At most {PresentationConstants.OrganiseSelectionLimit} media files can be sent at once, not {mediaFileIDList.Count}.", nameof(mediaFileIDList));
        }
    }

    // Busy is true from the send until the answer, a failed one included.
    private async Task<T> Run<T>(Func<Task<T>> send)
    {
        SetBusy(true);

        try
        {
            return await send();
        }
        finally
        {
            SetBusy(false);
        }
    }

    private void SetBusy(bool isBusy)
    {
        IsBusy = isBusy;
        BusyChanged?.Invoke();
    }
}
