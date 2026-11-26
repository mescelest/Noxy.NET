using Noxy.NET.MediaPlayer.Domain.Declarations;

namespace Noxy.NET.MediaPlayer.Presentation.Models;

// An artist, an album or a genre as the Organise page lists it: an album is named by its title.
public record MetadataItem(Guid ID, string Name, int MediaFileCount, int Weight = MetadataWeight.Default);
