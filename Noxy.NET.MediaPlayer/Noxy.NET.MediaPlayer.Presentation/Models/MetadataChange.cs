namespace Noxy.NET.MediaPlayer.Presentation.Models;

// One request the dialog sends: the item is added to (or, for an album, set on) the media files, or taken off them.
public record MetadataChange(Guid ItemID, bool IsAdd, List<Guid> MediaFileIDList);
