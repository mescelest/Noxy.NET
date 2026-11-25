using Noxy.NET.MediaPlayer.Presentation.Enums;
using Noxy.NET.MediaPlayer.Presentation.Models;

namespace Noxy.NET.MediaPlayer.Presentation.Interfaces.Services;

// The writes on artists, albums and genres. It does not apply an answer itself: the hub events and the sync update the lists and the files. A refusal or a lost connection is thrown (ApiException, HttpRequestException).
public interface IMetadataActionService
{
    // True from the send until the answer.
    bool IsBusy { get; }

    event Action? BusyChanged;

    // Returns the ID of the new item. The artist IDs are used for an album only.
    Task<Guid> Create(MetadataKindEnum kind, string name, List<Guid>? albumArtistIDList);

    // For an album the name is the title, and the artists change only when the list is not null.
    Task Rename(MetadataKindEnum kind, Guid id, string name, List<Guid>? albumArtistIDList);

    Task Delete(MetadataKindEnum kind, Guid id);

    // Moves every media file of the item to the target and removes the item; the target keeps its own name and artists.
    Task Merge(MetadataKindEnum kind, Guid sourceID, Guid targetID);

    // Merges each of the items into the target, one after the other, as one piece of work (busy once); a refusal stops the rest. The target among the items throws ArgumentException before anything is sent.
    Task MergeList(MetadataKindEnum kind, List<Guid> sourceIDList, Guid targetID);

    // Sets the picture of an album from the bytes of an image; the album itself changes through the hub push, as with the other writes.
    Task SetAlbumArt(Guid albumID, byte[] picture, string contentType);

    // Sends each change of the "Set" dialog, one after the other, as one piece of work (busy once): an add puts an artist or a genre on the media files and sets an album, a remove takes the item off them. A change over PresentationConstants.OrganiseSelectionLimit throws ArgumentException before anything is sent; a refusal stops the rest.
    Task Apply(MetadataKindEnum kind, List<MetadataChange> changeList);
}
