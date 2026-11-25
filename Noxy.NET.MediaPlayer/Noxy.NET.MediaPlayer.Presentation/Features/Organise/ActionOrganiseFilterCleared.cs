using Noxy.NET.MediaPlayer.Presentation.Enums;

namespace Noxy.NET.MediaPlayer.Presentation.Features.Organise;

// A null kind clears every filter.
public record ActionOrganiseFilterCleared(MetadataKindEnum? Kind);
