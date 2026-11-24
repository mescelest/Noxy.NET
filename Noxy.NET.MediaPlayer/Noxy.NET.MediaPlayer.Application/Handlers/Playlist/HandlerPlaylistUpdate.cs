using Mediator;
using Noxy.NET.MediaPlayer.Application.Commands.Playlist;
using Noxy.NET.MediaPlayer.Application.Extensions;
using Noxy.NET.MediaPlayer.Application.Handlers.Abstractions;
using Noxy.NET.MediaPlayer.Application.Interfaces;
using Noxy.NET.MediaPlayer.Domain.Declarations;
using Noxy.NET.MediaPlayer.Domain.Entities;
using Noxy.NET.MediaPlayer.Domain.Exceptions;
using Noxy.NET.MediaPlayer.Domain.Extensions;
using Noxy.NET.MediaPlayer.Domain.Responses.Playlist;

namespace Noxy.NET.MediaPlayer.Application.Handlers.Playlist;

public class HandlerPlaylistUpdate(IUnitOfWorkFactory factoryUnitOfWork, IPublisher publisher, TimeProvider time) : BaseHandlerPlaylistChange<CommandPlaylistUpdate, ResponsePlaylist>(factoryUnitOfWork, publisher, time)
{
    public override async ValueTask<ResponsePlaylist> Handle(CommandPlaylistUpdate command, CancellationToken cancellationToken)
    {
        await using IUnitOfWork uow = FactoryUnitOfWork.Create();

        EntityPlaylist playlist = await uow.LoadPlaylistWithEntries(command.UserID, command.ID, cancellationToken);

        bool changed = false;
        if (command.Name.HasValue)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(command.Name.Value, nameof(command));

            string name = command.Name.Value.Trim();
            if (name != playlist.Name)
            {
                playlist.Name = name;
                changed = true;
            }
        }

        if (command.AllowDuplicates.HasValue && command.AllowDuplicates.Value != playlist.AllowDuplicates)
        {
            // Duplicates can be turned off only once there are none, so a playlist never breaks its own rule.
            if (!command.AllowDuplicates.Value && playlist.HasDuplicates()) throw new PlaylistHasDuplicatesException();

            playlist.AllowDuplicates = command.AllowDuplicates.Value;
            changed = true;
        }

        if (command.ArtHue.HasValue && command.ArtHue.Value != playlist.ArtHue)
        {
            // The request refuses these already; this keeps a command from anywhere else from storing one.
            if (command.ArtHue.Value is { } hue)
            {
                ArgumentOutOfRangeException.ThrowIfLessThan(hue, PlaylistArtHue.Min, nameof(command));
                ArgumentOutOfRangeException.ThrowIfGreaterThan(hue, PlaylistArtHue.Max, nameof(command));
            }

            playlist.ArtHue = command.ArtHue.Value;
            changed = true;
        }

        // An edit that changes nothing leaves the modified time alone, so it can't change the ETag or push anything.
        if (!changed) return playlist.ToResponse(playlist.EntryList.Count);

        await Save(uow, command.UserID, playlist, cancellationToken);
        return playlist.ToResponse(playlist.EntryList.Count);
    }
}
