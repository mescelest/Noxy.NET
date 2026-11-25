using System.ComponentModel;
using Fluxor;
using Noxy.NET.MediaPlayer.Domain.Enums;
using Noxy.NET.MediaPlayer.Presentation.Enums;

namespace Noxy.NET.MediaPlayer.Presentation.Features.Organise;

[FeatureState]
public record FeatureOrganiseState
{
    public MetadataKindEnum Tab { get; init; } = MetadataKindEnum.Artist;
    public string Search { get; init; } = string.Empty;
    public LibrarySortEnum Sort { get; init; } = LibrarySortEnum.Title;
    public bool Descending { get; init; }
    public HashSet<Guid> SelectedSet { get; init; } = [];
    public Guid? ArtistFilterID { get; init; }
    public Guid? AlbumFilterID { get; init; }
    public Guid? GenreFilterID { get; init; }

    public bool HasFilter => ArtistFilterID is not null || AlbumFilterID is not null || GenreFilterID is not null;

    // How many more media files can be ticked before the limit.
    public int SelectionRoom => Math.Max(0, PresentationConstants.OrganiseSelectionLimit - SelectedSet.Count);

    public Guid? FilterOf(MetadataKindEnum kind)
    {
        return kind switch
        {
            MetadataKindEnum.Artist => ArtistFilterID,
            MetadataKindEnum.Album => AlbumFilterID,
            MetadataKindEnum.Genre => GenreFilterID,
            _ => throw new InvalidEnumArgumentException(nameof(kind), (int)kind, typeof(MetadataKindEnum))
        };
    }

    // How many of the media files are not ticked yet: what ticking them adds.
    public int NewTickCount(IEnumerable<Guid> mediaFileIDList)
    {
        return mediaFileIDList.Distinct().Count(x => !SelectedSet.Contains(x));
    }

    public bool AllFit(IEnumerable<Guid> mediaFileIDList)
    {
        return NewTickCount(mediaFileIDList) <= SelectionRoom;
    }
}
