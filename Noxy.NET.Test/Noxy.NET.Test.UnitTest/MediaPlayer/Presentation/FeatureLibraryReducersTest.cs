using Noxy.NET.MediaPlayer.Domain.Enums;
using Noxy.NET.MediaPlayer.Domain.Responses.Media;
using Noxy.NET.MediaPlayer.Presentation.Enums;
using Noxy.NET.MediaPlayer.Presentation.Features.Library;
using Noxy.NET.Test.MediaPlayer.Fixtures;

namespace Noxy.NET.Test.MediaPlayer.Presentation;

public class FeatureLibraryReducersTest
{
    private static List<ResponseMediaFile> MediaList()
    {
        return
        [
            MediaFileFactory.Create("Bravo", "Zed"),
            MediaFileFactory.Create("Alpha", "Yan"),
            MediaFileFactory.Create("Charlie bravo", "Xi"),
            MediaFileFactory.Create("Delta", "Wu")
        ];
    }

    private static FeatureLibraryState Loaded(List<ResponseMediaFile> mediaList)
    {
        return FeatureLibraryReducers.ReduceMediaLoaded(new(), new(mediaList));
    }

    private static List<string> Titles(FeatureLibraryState state)
    {
        return state.VisibleList.Select(x => x.Title).ToList();
    }

    [Fact]
    public void MediaLoaded_ShowsEveryMediaFile_SortedByTitle()
    {
        FeatureLibraryState state = Loaded(MediaList());

        Assert.Equal(["Alpha", "Bravo", "Charlie bravo", "Delta"], Titles(state));
    }

    [Fact]
    public void SearchChanged_FiltersTheVisibleList()
    {
        FeatureLibraryState state = FeatureLibraryReducers.ReduceSearchChanged(Loaded(MediaList()), new("bravo"));

        Assert.Equal(["Bravo", "Charlie bravo"], Titles(state));
        Assert.Equal("bravo", state.Search);
    }

    [Fact]
    public void SortChanged_WithANewSort_SortsAscending_AndWithTheSameSortReversesIt()
    {
        FeatureLibraryState state = Loaded(MediaList());

        state = FeatureLibraryReducers.ReduceSortChanged(state, new(LibrarySortEnum.Artist));
        Assert.Equal(["Delta", "Charlie bravo", "Alpha", "Bravo"], Titles(state));
        Assert.False(state.Descending);

        state = FeatureLibraryReducers.ReduceSortChanged(state, new(LibrarySortEnum.Artist));
        Assert.Equal(["Bravo", "Alpha", "Charlie bravo", "Delta"], Titles(state));
        Assert.True(state.Descending);

        state = FeatureLibraryReducers.ReduceSortChanged(state, new(LibrarySortEnum.Title));
        Assert.False(state.Descending);
        Assert.Equal(LibrarySortEnum.Title, state.Sort);
    }

    [Fact]
    public void SelectionToggled_AddsAndRemoves_AndKeepsTheSelectionWhenTheSortChanges()
    {
        List<ResponseMediaFile> mediaList = MediaList();
        FeatureLibraryState state = Loaded(mediaList);

        state = FeatureLibraryReducers.ReduceSelectionToggled(state, new(mediaList[0].ID));
        state = FeatureLibraryReducers.ReduceSelectionToggled(state, new(mediaList[1].ID));
        state = FeatureLibraryReducers.ReduceSortChanged(state, new(LibrarySortEnum.Artist));
        Assert.Equal(2, state.SelectedCount);

        state = FeatureLibraryReducers.ReduceSelectionToggled(state, new(mediaList[0].ID));

        Assert.Equal([mediaList[1].ID], state.SelectedSet);
    }

    [Fact]
    public void VisibleSelected_SelectsOnlyTheRowsThatMatchTheSearch()
    {
        List<ResponseMediaFile> mediaList = MediaList();
        FeatureLibraryState state = FeatureLibraryReducers.ReduceSearchChanged(Loaded(mediaList), new("bravo"));

        state = FeatureLibraryReducers.ReduceVisibleSelected(state, new());

        Assert.Equal(new[] { mediaList[0].ID, mediaList[2].ID }.Order(), state.SelectedSet.Order());
        Assert.DoesNotContain(mediaList[1].ID, state.SelectedSet);
    }

    [Fact]
    public void HiddenSelectedCount_CountsSelectedRowsTheSearchHides_AndSelectedCount_IsTheRealCount()
    {
        List<ResponseMediaFile> mediaList = MediaList();
        FeatureLibraryState state = Loaded(mediaList);
        state = FeatureLibraryReducers.ReduceSelectionToggled(state, new(mediaList[1].ID));
        state = FeatureLibraryReducers.ReduceSelectionToggled(state, new(mediaList[3].ID));
        state = FeatureLibraryReducers.ReduceSelectionToggled(state, new(mediaList[0].ID));

        state = FeatureLibraryReducers.ReduceSearchChanged(state, new("bravo"));

        Assert.Equal(3, state.SelectedCount);
        Assert.Equal(2, state.HiddenSelectedCount);
    }

    [Fact]
    public void AllVisibleSelected_IsTrueOnlyWhenEveryVisibleRowIsTicked_AndFalseForAnEmptyList()
    {
        List<ResponseMediaFile> mediaList = MediaList();
        FeatureLibraryState state = Loaded(mediaList);
        Assert.False(state.AllVisibleSelected);

        state = FeatureLibraryReducers.ReduceVisibleSelected(state, new());
        Assert.True(state.AllVisibleSelected);

        state = FeatureLibraryReducers.ReduceSelectionToggled(state, new(mediaList[2].ID));
        Assert.False(state.AllVisibleSelected);

        Assert.False(FeatureLibraryReducers.ReduceSearchChanged(Loaded(mediaList), new("nothing matches")).AllVisibleSelected);
        Assert.False(new FeatureLibraryState().AllVisibleSelected);
    }

    [Fact]
    public void VisibleSelected_WhenEveryVisibleRowIsTicked_UnticksThemButNotTheHiddenOnes()
    {
        List<ResponseMediaFile> mediaList = MediaList();
        FeatureLibraryState state = Loaded(mediaList);
        state = FeatureLibraryReducers.ReduceSelectionToggled(state, new(mediaList[1].ID));
        state = FeatureLibraryReducers.ReduceSearchChanged(state, new("bravo"));
        state = FeatureLibraryReducers.ReduceVisibleSelected(state, new());
        Assert.True(state.AllVisibleSelected);

        state = FeatureLibraryReducers.ReduceVisibleSelected(state, new());

        Assert.Equal([mediaList[1].ID], state.SelectedSet);
    }

    [Fact]
    public void SelectionCleared_UnticksEveryRow_TheHiddenOnesToo()
    {
        List<ResponseMediaFile> mediaList = MediaList();
        FeatureLibraryState state = FeatureLibraryReducers.ReduceSearchChanged(FeatureLibraryReducers.ReduceVisibleSelected(Loaded(mediaList), new()), new("bravo"));

        state = FeatureLibraryReducers.ReduceSelectionCleared(state, new());

        Assert.Equal(0, state.SelectedCount);
    }

    [Fact]
    public void MediaLoaded_DropsSelectedIDsThatLeftTheLibrary()
    {
        List<ResponseMediaFile> mediaList = MediaList();
        FeatureLibraryState state = FeatureLibraryReducers.ReduceVisibleSelected(Loaded(mediaList), new());

        state = FeatureLibraryReducers.ReduceMediaLoaded(state, new([mediaList[0], mediaList[1]]));

        Assert.Equal(new[] { mediaList[0].ID, mediaList[1].ID }.Order(), state.SelectedSet.Order());
    }

    [Fact]
    public void MediaLoaded_KeepsTheSearchAndSort()
    {
        List<ResponseMediaFile> mediaList = MediaList();
        FeatureLibraryState state = FeatureLibraryReducers.ReduceSortChanged(FeatureLibraryReducers.ReduceSearchChanged(Loaded(mediaList), new("bravo")), new(LibrarySortEnum.Title));

        state = FeatureLibraryReducers.ReduceMediaLoaded(state, new([.. mediaList, MediaFileFactory.Create("Bravo two")]));

        Assert.Equal("bravo", state.Search);
        Assert.True(state.Descending);
        Assert.Equal(["Charlie bravo", "Bravo two", "Bravo"], Titles(state));
    }

    [Fact]
    public void StateLoaded_ReplacesTheFileStates()
    {
        List<ResponseMediaFile> mediaList = MediaList();
        FeatureLibraryState state = Loaded(mediaList);

        state = FeatureLibraryReducers.ReduceStateLoaded(state, new([new(mediaList[0].ID, FileSyncStateEnum.OnDevice, null), new(mediaList[1].ID, FileSyncStateEnum.Downloading, null)]));
        state = FeatureLibraryReducers.ReduceStateLoaded(state, new([new(mediaList[2].ID, FileSyncStateEnum.Failed, "x")]));

        Assert.Equal([mediaList[2].ID], state.StateMap.Keys);
        Assert.Equal(FileSyncStateEnum.Failed, state.StateMap[mediaList[2].ID]);
    }

    [Fact]
    public void OutdatedLoaded_ReplacesTheSet()
    {
        Guid first = Guid.CreateVersion7();
        Guid second = Guid.CreateVersion7();
        FeatureLibraryState state = new();

        state = FeatureLibraryReducers.ReduceOutdatedLoaded(state, new([first]));
        state = FeatureLibraryReducers.ReduceOutdatedLoaded(state, new([second]));

        Assert.Equal([second], state.OutdatedSet);
    }

    [Fact]
    public void SelectedIDList_FollowsTheOrderOnScreen_NotTheOrderOfTheTicks_AndKeepsTheHiddenOnesLast()
    {
        List<ResponseMediaFile> mediaList = MediaList();
        FeatureLibraryState state = Loaded(mediaList);
        ResponseMediaFile delta = mediaList.Single(x => x.Title == "Delta");
        ResponseMediaFile alpha = mediaList.Single(x => x.Title == "Alpha");
        ResponseMediaFile charlie = mediaList.Single(x => x.Title == "Charlie bravo");
        state = FeatureLibraryReducers.ReduceSelectionToggled(state, new(delta.ID));
        state = FeatureLibraryReducers.ReduceSelectionToggled(state, new(alpha.ID));
        state = FeatureLibraryReducers.ReduceSelectionToggled(state, new(charlie.ID));

        // A search hides Alpha and Delta; the ticks stay.
        state = FeatureLibraryReducers.ReduceSearchChanged(state, new("bravo"));

        List<Guid> list = state.SelectedIDList;

        Assert.Equal(charlie.ID, list[0]);
        Assert.Equal(3, list.Count);
        Assert.Equal(new[] { alpha.ID, delta.ID }.Order(), list.Skip(1).Order());
    }

    [Fact]
    public void SelectedIDList_WithNothingTicked_IsEmpty()
    {
        Assert.Empty(Loaded(MediaList()).SelectedIDList);
    }
}
