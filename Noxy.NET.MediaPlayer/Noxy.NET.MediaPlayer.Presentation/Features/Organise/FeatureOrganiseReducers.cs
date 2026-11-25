using System.ComponentModel;
using Fluxor;
using Noxy.NET.MediaPlayer.Presentation.Enums;
using Noxy.NET.MediaPlayer.Presentation.Features.Library;
using Noxy.NET.MediaPlayer.Presentation.Features.MetadataList;

namespace Noxy.NET.MediaPlayer.Presentation.Features.Organise;

public static class FeatureOrganiseReducers
{
    [ReducerMethod]
    public static FeatureOrganiseState ReduceTabChanged(FeatureOrganiseState state, ActionOrganiseTabChanged action)
    {
        return state with { Tab = action.Tab };
    }

    [ReducerMethod]
    public static FeatureOrganiseState ReduceSearchChanged(FeatureOrganiseState state, ActionOrganiseSearchChanged action)
    {
        return state with { Search = action.Search };
    }

    // The same sort again reverses it; a new sort starts ascending.
    [ReducerMethod]
    public static FeatureOrganiseState ReduceSortChanged(FeatureOrganiseState state, ActionOrganiseSortChanged action)
    {
        return state with { Sort = action.Sort, Descending = action.Sort == state.Sort && !state.Descending };
    }

    // The same item again clears the filter; another item of the kind replaces it. The files shown change, so the ticks go.
    [ReducerMethod]
    public static FeatureOrganiseState ReduceFilterToggled(FeatureOrganiseState state, ActionOrganiseFilterToggled action)
    {
        Guid? next = state.FilterOf(action.Kind) == action.ItemID ? null : action.ItemID;

        return WithFilter(state, action.Kind, next) with { SelectedSet = [] };
    }

    [ReducerMethod]
    public static FeatureOrganiseState ReduceFilterCleared(FeatureOrganiseState state, ActionOrganiseFilterCleared action)
    {
        FeatureOrganiseState cleared = action.Kind is null
            ? state with { ArtistFilterID = null, AlbumFilterID = null, GenreFilterID = null }
            : WithFilter(state, action.Kind.Value, null);

        return cleared with { SelectedSet = [] };
    }

    // Unticking always works; ticking is ignored at the limit.
    [ReducerMethod]
    public static FeatureOrganiseState ReduceSelectionToggled(FeatureOrganiseState state, ActionOrganiseSelectionToggled action)
    {
        HashSet<Guid> selectedSet = [.. state.SelectedSet];
        if (!selectedSet.Remove(action.MediaFileID) && state.SelectionRoom > 0) selectedSet.Add(action.MediaFileID);

        return state with { SelectedSet = selectedSet };
    }

    // Unticks the shown ones when all of them are ticked; else ticks them in order until the limit. A hidden tick is never touched.
    [ReducerMethod]
    public static FeatureOrganiseState ReduceVisibleSelected(FeatureOrganiseState state, ActionOrganiseVisibleSelected action)
    {
        HashSet<Guid> selectedSet = [.. state.SelectedSet];

        if (action.IsAllSelected)
        {
            selectedSet.ExceptWith(action.VisibleIDList);
        }
        else
        {
            foreach (Guid id in action.VisibleIDList)
            {
                if (selectedSet.Count >= PresentationConstants.OrganiseSelectionLimit) break;

                selectedSet.Add(id);
            }
        }

        return state with { SelectedSet = selectedSet };
    }

    [ReducerMethod]
    public static FeatureOrganiseState ReduceSelectionCleared(FeatureOrganiseState state, ActionOrganiseSelectionCleared action)
    {
        return state with { SelectedSet = [] };
    }

    [ReducerMethod]
    public static FeatureOrganiseState ReduceReset(FeatureOrganiseState state, ActionOrganiseReset action)
    {
        return new();
    }

    // A filter whose item is gone from the lists cannot stay.
    [ReducerMethod]
    public static FeatureOrganiseState ReduceMetadataListLoaded(FeatureOrganiseState state, ActionMetadataListLoaded action)
    {
        HashSet<Guid> artistSet = action.ArtistList.Select(x => x.ID).ToHashSet();
        HashSet<Guid> albumSet = action.AlbumList.Select(x => x.ID).ToHashSet();
        HashSet<Guid> genreSet = action.GenreList.Select(x => x.ID).ToHashSet();

        return state with
        {
            ArtistFilterID = state.ArtistFilterID is { } artist && artistSet.Contains(artist) ? artist : null,
            AlbumFilterID = state.AlbumFilterID is { } album && albumSet.Contains(album) ? album : null,
            GenreFilterID = state.GenreFilterID is { } genre && genreSet.Contains(genre) ? genre : null
        };
    }

    // A media file that left the library cannot stay ticked.
    [ReducerMethod]
    public static FeatureOrganiseState ReduceLibraryMediaLoaded(FeatureOrganiseState state, ActionLibraryMediaLoaded action)
    {
        HashSet<Guid> idSet = action.MediaList.Select(x => x.ID).ToHashSet();

        return state with { SelectedSet = state.SelectedSet.Where(idSet.Contains).ToHashSet() };
    }

    private static FeatureOrganiseState WithFilter(FeatureOrganiseState state, MetadataKindEnum kind, Guid? itemID)
    {
        return kind switch
        {
            MetadataKindEnum.Artist => state with { ArtistFilterID = itemID },
            MetadataKindEnum.Album => state with { AlbumFilterID = itemID },
            MetadataKindEnum.Genre => state with { GenreFilterID = itemID },
            _ => throw new InvalidEnumArgumentException(nameof(kind), (int)kind, typeof(MetadataKindEnum))
        };
    }
}
