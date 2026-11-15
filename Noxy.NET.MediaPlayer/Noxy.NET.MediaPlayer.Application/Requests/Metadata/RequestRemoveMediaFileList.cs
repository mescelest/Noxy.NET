using System.ComponentModel.DataAnnotations;
using Noxy.NET.MediaPlayer.Application.Commands.Metadata;

namespace Noxy.NET.MediaPlayer.Application.Requests.Metadata;

public class RequestRemoveMediaFileList
{
    [Required]
    [MinLength(1)]
    [MaxLength(RequestAddMediaFileList.MaxMediaFileCount)]
    public List<Guid> MediaFileIDList { get; set; } = [];

    public CommandArtistMediaFileListRemove ToArtistCommand(Guid userID, Guid id)
    {
        return new(userID, id, MediaFileIDList);
    }

    public CommandGenreMediaFileListRemove ToGenreCommand(Guid userID, Guid id)
    {
        return new(userID, id, MediaFileIDList);
    }

    public CommandAlbumMediaFileListRemove ToAlbumCommand(Guid userID, Guid id)
    {
        return new(userID, id, MediaFileIDList);
    }
}
