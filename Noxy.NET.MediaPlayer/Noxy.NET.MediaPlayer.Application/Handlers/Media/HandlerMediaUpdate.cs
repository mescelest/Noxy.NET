using Mediator;
using Noxy.NET.MediaPlayer.Application.Commands.Media;
using Noxy.NET.MediaPlayer.Application.Interfaces;
using Noxy.NET.MediaPlayer.Application.Models;
using Noxy.NET.MediaPlayer.Application.Notifications.Media;
using Noxy.NET.MediaPlayer.Domain.Entities;
using Noxy.NET.MediaPlayer.Domain.Exceptions;
using Noxy.NET.MediaPlayer.Domain.Extensions;
using Noxy.NET.MediaPlayer.Domain.Responses.Media;

namespace Noxy.NET.MediaPlayer.Application.Handlers.Media;

public class HandlerMediaUpdate(IUnitOfWorkFactory factoryUnitOfWork, IPublisher publisher, TimeProvider time) : ICommandHandler<CommandMediaUpdate, ResponseMediaFile>
{
    // Changes only the fields that were sent. Throws MediaNotFoundException for an unknown media file or another user's media file.
    public async ValueTask<ResponseMediaFile> Handle(CommandMediaUpdate command, CancellationToken cancellationToken)
    {
        await using IUnitOfWork uow = factoryUnitOfWork.Create();

        EntityMediaFile file = await uow.Media.FindMediaFile(command.ID, command.UserID, cancellationToken) ?? throw new MediaNotFoundException();
        EntityMediaFileMetadata metadata = file.Metadata ?? throw new MediaNotFoundException();

        bool changed = false;
        if (command.Title.HasValue)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(command.Title.Value, nameof(command));

            changed |= Apply(command.Title.Value.Trim(), metadata.Title, value => metadata.Title = value);
        }

        changed |= ApplyText(command.Artist, metadata.Artist, value => metadata.Artist = value);
        changed |= ApplyText(command.Album, metadata.Album, value => metadata.Album = value);
        changed |= ApplyText(command.AlbumArtist, metadata.AlbumArtist, value => metadata.AlbumArtist = value);
        changed |= ApplyText(command.Subtitle, metadata.Subtitle, value => metadata.Subtitle = value);
        changed |= ApplyText(command.Comment, metadata.Comment, value => metadata.Comment = value);
        changed |= ApplyText(command.Genre, metadata.Genre, value => metadata.Genre = value);
        changed |= ApplyNumber(command.TrackNumber, metadata.TrackNumber, value => metadata.TrackNumber = value);
        changed |= ApplyNumber(command.DiscNumber, metadata.DiscNumber, value => metadata.DiscNumber = value);
        changed |= ApplyNumber(command.Year, metadata.Year, value => metadata.Year = value);

        // An edit that changes nothing leaves the modified time alone, so it can't change the library ETag or push anything.
        if (!changed) return file.ToResponse();

        file.TimeModified = time.GetUtcNow().UtcDateTime;
        uow.Media.UpdateMediaFile(file);
        await uow.Commit(cancellationToken);

        ResponseMediaFile response = file.ToResponse();
        await publisher.Publish(new NotificationMediaUpdated(command.UserID, response), cancellationToken);
        return response;
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
