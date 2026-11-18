using Fluxor;
using Noxy.NET.MediaPlayer.Domain.Enums;
using Noxy.NET.MediaPlayer.Domain.Responses.Media;
using Noxy.NET.MediaPlayer.Presentation.Enums;

namespace Noxy.NET.MediaPlayer.Presentation.Features.Library;

[FeatureState]
public record FeatureLibraryState
{
    public List<ResponseMediaFile> MediaList { get; init; } = [];
    public Dictionary<Guid, FileSyncStateEnum> StateMap { get; init; } = [];
    public string Search { get; init; } = string.Empty;
    public LibrarySortEnum Sort { get; init; } = LibrarySortEnum.Title;
    public bool Descending { get; init; }
    public HashSet<Guid> SelectedSet { get; init; } = [];

    // The media files that match the search, in the sort order: what the page shows.
    public List<ResponseMediaFile> VisibleList { get; init; } = [];

    // The real number of ticked rows, the ones a search hides included.
    public int SelectedCount => SelectedSet.Count;

    // The ticked media files in the order of the page, the ones a search hides after them: what an action on the selection works through.
    public List<Guid> SelectedIDList => [.. VisibleList.Where(x => SelectedSet.Contains(x.ID)).Select(x => x.ID), .. SelectedSet.Except(VisibleList.Select(x => x.ID))];

    public int HiddenSelectedCount => SelectedSet.Count - VisibleList.Count(x => SelectedSet.Contains(x.ID));

    public bool AllVisibleSelected => VisibleList.Count > 0 && VisibleList.All(x => SelectedSet.Contains(x.ID));
}
