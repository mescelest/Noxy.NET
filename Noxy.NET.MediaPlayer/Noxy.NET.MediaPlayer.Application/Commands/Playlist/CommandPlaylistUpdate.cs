using Mediator;
using Noxy.NET.MediaPlayer.Application.Models;
using Noxy.NET.MediaPlayer.Domain.Responses.Playlist;

namespace Noxy.NET.MediaPlayer.Application.Commands.Playlist;

// A member left at its default was not sent, which is different from a member sent as null.
public record CommandPlaylistUpdate(
    Guid UserID,
    Guid ID,
    Optional<string?> Name = default,
    Optional<bool> AllowDuplicates = default) : ICommand<ResponsePlaylist>;
