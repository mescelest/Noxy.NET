using Mediator;
using Noxy.NET.MediaPlayer.Application.Commands.Media;
using Noxy.NET.MediaPlayer.Application.Extensions;
using Noxy.NET.MediaPlayer.Application.Handlers.Abstractions.Metadata;
using Noxy.NET.MediaPlayer.Application.Interfaces;
using Noxy.NET.MediaPlayer.Application.Interfaces.Services;
using Noxy.NET.MediaPlayer.Application.Models;
using Noxy.NET.MediaPlayer.Application.Models.Metadata;
using Noxy.NET.MediaPlayer.Domain.Entities;
using Noxy.NET.MediaPlayer.Domain.Exceptions;
using Noxy.NET.MediaPlayer.Domain.Extensions;
using Noxy.NET.MediaPlayer.Domain.Responses.Media;

namespace Noxy.NET.MediaPlayer.Application.Handlers.Media;

public class HandlerMediaUpdate(IUnitOfWorkFactory factoryUnitOfWork, IMetadataResolutionService serviceMetadataResolution, IPublisher publisher, TimeProvider time)
    : BaseHandlerMetadataChange<CommandMediaUpdate, ResponseMediaFile>(factoryUnitOfWork, publisher, time)
{
    // Changes only the fields that were sent. Artists, album, album artists and genres can be sent as text (resolved to items exactly like an upload) or as item IDs. Items the file joined or left are pushed with their new file counts, a new one included, before the file itself. Throws MediaNotFoundException for an unknown media file or another user's media file, and MetadataItemNotFoundException for an unknown or another user's item ID.
    public override async ValueTask<ResponseMediaFile> Handle(CommandMediaUpdate command, CancellationToken cancellationToken)
    {
        ThrowIfConflicting(command);

        // Another request may create the same new artist first: then this whole edit runs again and reuses it.
        return await Execute(command.UserID, uow => Apply(uow, command, cancellationToken), cancellationToken);
    }

    private async Task<MetadataChangeResult<ResponseMediaFile>> Apply(IUnitOfWork uow, CommandMediaUpdate command, CancellationToken cancellationToken)
    {
        EntityMediaFile file = await uow.Media.FindMediaFile(command.ID, command.UserID, cancellationToken) ?? throw new MediaNotFoundException();
        EntityMediaFileMetadata metadata = file.Metadata ?? throw new MediaNotFoundException();

        MediaFileItemSet itemsBefore = MediaFileItemSet.Of(file);

        bool changed = false;
        if (command.Title.HasValue)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(command.Title.Value, nameof(command));

            changed |= Apply(command.Title.Value.Trim(), metadata.Title, value => metadata.Title = value);
        }

        changed |= ApplyText(command.Subtitle, metadata.Subtitle, value => metadata.Subtitle = value);
        changed |= ApplyText(command.Comment, metadata.Comment, value => metadata.Comment = value);
        changed |= ApplyNumber(command.TrackNumber, metadata.TrackNumber, value => metadata.TrackNumber = value);
        changed |= ApplyNumber(command.DiscNumber, metadata.DiscNumber, value => metadata.DiscNumber = value);
        changed |= ApplyNumber(command.Year, metadata.Year, value => metadata.Year = value);
        changed |= await ApplyItems(uow, command, file, metadata, cancellationToken);

        // An edit that changes nothing leaves the modified time alone, so it can't change the library ETag or push anything.
        if (!changed) return new(file.ToResponse(), false, new(), []);

        MetadataChangeSet changeSet = new();
        await uow.AddItemCountList(changeSet, command.UserID, file, itemsBefore, cancellationToken);

        file.TimeModified = Now;
        uow.Media.UpdateMediaFile(file);
        await uow.Commit(cancellationToken);

        ResponseMediaFile response = file.ToResponse();
        return new(response, true, changeSet, [response]);
    }

    private async Task<bool> ApplyItems(IUnitOfWork uow, CommandMediaUpdate command, EntityMediaFile file, EntityMediaFileMetadata metadata, CancellationToken cancellationToken)
    {
        bool artistGiven = command.Artist.HasValue || command.ArtistIDList.HasValue;
        bool genreGiven = command.Genre.HasValue || command.GenreIDList.HasValue;
        bool albumByTitleGiven = command.Album.HasValue || command.AlbumArtist.HasValue || command.AlbumArtistIDList.HasValue;
        bool albumGiven = albumByTitleGiven || command.AlbumID.HasValue;
        if (!artistGiven && !genreGiven && !albumGiven) return false;

        // Items sent by ID are looked up first, so an unknown one fails before anything is added.
        List<EntityArtist>? artistByIDList = command.ArtistIDList.HasValue ? await LoadArtistList(uow, command.UserID, command.ArtistIDList.Value, cancellationToken) : null;
        List<EntityGenre>? genreByIDList = command.GenreIDList.HasValue ? await LoadGenreList(uow, command.UserID, command.GenreIDList.Value, cancellationToken) : null;
        List<EntityArtist>? albumArtistByIDList = command.AlbumArtistIDList.HasValue ? await LoadArtistList(uow, command.UserID, command.AlbumArtistIDList.Value, cancellationToken) : null;
        EntityAlbum? albumByID = command.AlbumID is { HasValue: true, Value: { } albumID } ? await uow.Metadata.FindAlbum(albumID, command.UserID, cancellationToken) ?? throw new MetadataItemNotFoundException() : null;

        // A file belongs to the album with a title and a set of album artists, so a change to either one moves it to the matching album, which is created when it does not exist yet.
        MediaTagSet tags = new(
            null,
            command.Artist.HasValue ? ToValueList(command.Artist.Value) : [],
            albumByTitleGiven ? command.Album.HasValue ? command.Album.Value : file.Album?.Title : null,
            command.AlbumArtist.HasValue ? ToValueList(command.AlbumArtist.Value) : albumArtistByIDList is not null ? [.. albumArtistByIDList.Select(x => x.Name)] : file.GetAlbumArtistNameList(),
            command.Genre.HasValue ? ToValueList(command.Genre.Value) : [],
            null, null, null, null, null, null);
        ResolvedMetadata resolved = await serviceMetadataResolution.Resolve(uow, command.UserID, tags, cancellationToken);

        bool changed = false;
        if (artistGiven) changed |= uow.SetArtistList(file, command.Artist.HasValue ? resolved.ArtistList : artistByIDList ?? []);
        if (genreGiven) changed |= uow.SetGenreList(file, command.Genre.HasValue ? resolved.GenreList : genreByIDList ?? []);
        if (albumGiven) changed |= ApplyAlbum(file, albumByTitleGiven ? resolved.Album : albumByID);

        // The old free text is replaced by the items, so it must not come back when the items are later removed.
        if (artistGiven) changed |= ClearLegacy(metadata.Artist, () => metadata.Artist = null);
        if (genreGiven) changed |= ClearLegacy(metadata.Genre, () => metadata.Genre = null);
        if (albumGiven)
        {
            changed |= ClearLegacy(metadata.Album, () => metadata.Album = null);
            changed |= ClearLegacy(metadata.AlbumArtist, () => metadata.AlbumArtist = null);
        }

        return changed;
    }

    private static bool ApplyAlbum(EntityMediaFile file, EntityAlbum? desired)
    {
        if (desired?.ID == file.AlbumID) return false;

        file.AlbumID = desired?.ID;
        file.Album = desired;
        return true;
    }

    private static bool ClearLegacy(string? current, Action clear)
    {
        if (current is null) return false;

        clear();
        return true;
    }

    private static async Task<List<EntityArtist>> LoadArtistList(IUnitOfWork uow, Guid userID, List<Guid>? idList, CancellationToken cancellationToken)
    {
        List<EntityArtist> result = [];
        foreach (Guid id in (idList ?? []).Distinct())
        {
            result.Add(await uow.Metadata.FindArtist(id, userID, cancellationToken) ?? throw new MetadataItemNotFoundException());
        }

        return result;
    }

    private static async Task<List<EntityGenre>> LoadGenreList(IUnitOfWork uow, Guid userID, List<Guid>? idList, CancellationToken cancellationToken)
    {
        List<EntityGenre> result = [];
        foreach (Guid id in (idList ?? []).Distinct())
        {
            result.Add(await uow.Metadata.FindGenre(id, userID, cancellationToken) ?? throw new MetadataItemNotFoundException());
        }

        return result;
    }

    private static List<string> ToValueList(string? text)
    {
        return text is null ? [] : [text];
    }

    // The request is validated before it gets here, so this only guards a command built by hand.
    private static void ThrowIfConflicting(CommandMediaUpdate command)
    {
        bool conflicting = (command.Artist.HasValue && command.ArtistIDList.HasValue)
                           || (command.Genre.HasValue && command.GenreIDList.HasValue)
                           || (command.AlbumArtist.HasValue && command.AlbumArtistIDList.HasValue)
                           || (command.AlbumID.HasValue && (command.Album.HasValue || command.AlbumArtist.HasValue || command.AlbumArtistIDList.HasValue));

        if (conflicting) throw new ArgumentException("A field was sent both as text and as IDs.", nameof(command));
    }

    private static bool Apply(string value, string? current, Action<string> set)
    {
        if (value == current) return false;

        set(value);
        return true;
    }

    // A blank value clears the tag, so a tag never holds an empty string.
    private static bool ApplyText(Optional<string?> requested, string? current, Action<string?> set)
    {
        if (!requested.HasValue) return false;

        string? value = string.IsNullOrWhiteSpace(requested.Value) ? null : requested.Value.Trim();
        if (value == current) return false;

        set(value);
        return true;
    }

    private static bool ApplyNumber(Optional<int?> requested, int? current, Action<int?> set)
    {
        if (!requested.HasValue || requested.Value == current) return false;

        set(requested.Value);
        return true;
    }
}
