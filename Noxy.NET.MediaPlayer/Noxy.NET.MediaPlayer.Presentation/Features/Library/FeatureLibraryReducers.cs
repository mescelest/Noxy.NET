using Fluxor;
using Noxy.NET.MediaPlayer.Presentation.Extensions;

namespace Noxy.NET.MediaPlayer.Presentation.Features.Library;

public static class FeatureLibraryReducers
{
    [ReducerMethod]
    public static FeatureLibraryState ReduceMediaLoaded(FeatureLibraryState state, ActionLibraryMediaLoaded action)
    {
        // A media file that left the library cannot stay ticked.
        HashSet<Guid> idSet = action.MediaList.Select(x => x.ID).ToHashSet();

        return Rebuild(state with { MediaList = action.MediaList, SelectedSet = state.SelectedSet.Where(idSet.Contains).ToHashSet() });
    }

    [ReducerMethod]
    public static FeatureLibraryState ReduceStateLoaded(FeatureLibraryState state, ActionLibraryStateLoaded action)
    {
        return state with { StateMap = action.StateList.ToDictionary(x => x.MediaFileID, x => x.State) };
    }

    [ReducerMethod]
    public static FeatureLibraryState ReduceOutdatedLoaded(FeatureLibraryState state, ActionLibraryOutdatedLoaded action)
    {
        return state with { OutdatedSet = [.. action.IDList] };
    }

    [ReducerMethod]
    public static FeatureLibraryState ReduceSearchChanged(FeatureLibraryState state, ActionLibrarySearchChanged action)
    {
        return Rebuild(state with { Search = action.Search });
    }

    // The same sort again reverses it; a new sort starts ascending.
    [ReducerMethod]
    public static FeatureLibraryState ReduceSortChanged(FeatureLibraryState state, ActionLibrarySortChanged action)
    {
        return Rebuild(state with { Sort = action.Sort, Descending = action.Sort == state.Sort && !state.Descending });
    }

    [ReducerMethod]
    public static FeatureLibraryState ReduceSelectionToggled(FeatureLibraryState state, ActionLibrarySelectionToggled action)
    {
        HashSet<Guid> selectedSet = [.. state.SelectedSet];
        if (!selectedSet.Remove(action.MediaFileID)) selectedSet.Add(action.MediaFileID);

        return state with { SelectedSet = selectedSet };
    }

    // Select all ticks the rows that match the search; when they are all ticked already it unticks them. A hidden row is never touched.
    [ReducerMethod]
    public static FeatureLibraryState ReduceVisibleSelected(FeatureLibraryState state, ActionLibraryVisibleSelected action)
    {
        HashSet<Guid> selectedSet = [.. state.SelectedSet];
        if (state.AllVisibleSelected) selectedSet.ExceptWith(state.VisibleList.Select(x => x.ID));
        else selectedSet.UnionWith(state.VisibleList.Select(x => x.ID));

        return state with { SelectedSet = selectedSet };
    }

    [ReducerMethod]
    public static FeatureLibraryState ReduceSelectionCleared(FeatureLibraryState state, ActionLibrarySelectionCleared action)
    {
        return state with { SelectedSet = [] };
    }

    private static FeatureLibraryState Rebuild(FeatureLibraryState state)
    {
        return state with { VisibleList = state.MediaList.Filter(state.Search).Sort(state.Sort, state.Descending) };
    }
}
