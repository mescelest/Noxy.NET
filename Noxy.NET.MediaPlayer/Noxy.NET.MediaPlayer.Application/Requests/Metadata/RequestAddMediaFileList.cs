using System.ComponentModel.DataAnnotations;
using Noxy.NET.MediaPlayer.Application.Commands.Metadata;

namespace Noxy.NET.MediaPlayer.Application.Requests.Metadata;

public class RequestAddMediaFileList
{
    public const int MaxMediaFileCount = 1000;

    [Required]
    [MinLength(1)]
    [MaxLength(MaxMediaFileCount)]
    public List<Guid> MediaFileIDList { get; set; } = [];

    public CommandArtistMediaFileListAdd ToArtistCommand(Guid userID, Guid id)
    {
        return new(userID, id, MediaFileIDList);
    }

    public CommandGenreMediaFileListAdd ToGenreCommand(Guid userID, Guid id)
    {
        return new(userID, id, MediaFileIDList);
    }

    public CommandAlbumMediaFileListAdd ToAlbumCommand(Guid userID, Guid id)
    {
        return new(userID, id, MediaFileIDList);
    }
}
