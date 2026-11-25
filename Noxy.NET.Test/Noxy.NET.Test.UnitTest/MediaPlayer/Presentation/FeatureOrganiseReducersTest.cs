using Noxy.NET.MediaPlayer.Domain.Enums;
using Noxy.NET.MediaPlayer.Presentation;
using Noxy.NET.MediaPlayer.Presentation.Enums;
using Noxy.NET.MediaPlayer.Presentation.Features.Organise;
using Noxy.NET.Test.MediaPlayer.Fakes;
using Noxy.NET.Test.MediaPlayer.Fixtures;

namespace Noxy.NET.Test.MediaPlayer.Presentation;

public class FeatureOrganiseReducersTest
{
    private static List<Guid> NewIDList(int count) => Enumerable.Range(0, count).Select(_ => Guid.CreateVersion7()).ToList();

    [Fact]
    public void FilterToggled_ClearsTheSelection()
    {
        FeatureOrganiseState state = new() { SelectedSet = [Guid.CreateVersion7()] };

        state = FeatureOrganiseReducers.ReduceFilterToggled(state, new(MetadataKindEnum.Artist, Guid.CreateVersion7()));

        Assert.Empty(state.SelectedSet);
    }

    [Fact]
    public void FilterCleared_ClearsTheSelection_OfAKindAndOfAll()
    {
        FeatureOrganiseState state = new() { ArtistFilterID = Guid.CreateVersion7(), SelectedSet = [Guid.CreateVersion7()] };

        Assert.Empty(FeatureOrganiseReducers.ReduceFilterCleared(state, new(MetadataKindEnum.Artist)).SelectedSet);
        Assert.Empty(FeatureOrganiseReducers.ReduceFilterCleared(state, new(null)).SelectedSet);
    }

    [Fact]
    public void FilterToggled_SameItemTwice_Clears()
    {
        Guid id = Guid.CreateVersion7();
        FeatureOrganiseState state = FeatureOrganiseReducers.ReduceFilterToggled(new(), new(MetadataKindEnum.Artist, id));
        Assert.Equal(id, state.ArtistFilterID);

        state = FeatureOrganiseReducers.ReduceFilterToggled(state, new(MetadataKindEnum.Artist, id));

        Assert.Null(state.ArtistFilterID);
        Assert.False(state.HasFilter);
    }

    [Fact]
    public void FilterToggled_AnotherItemOfTheKind_Replaces()
    {
        Guid first = Guid.CreateVersion7();
        Guid second = Guid.CreateVersion7();
        FeatureOrganiseState state = FeatureOrganiseReducers.ReduceFilterToggled(new(), new(MetadataKindEnum.Genre, first));

        state = FeatureOrganiseReducers.ReduceFilterToggled(state, new(MetadataKindEnum.Genre, second));

        Assert.Equal(second, state.GenreFilterID);
    }

    [Fact]
    public void FilterToggled_DifferentKinds_Combine()
    {
        Guid artist = Guid.CreateVersion7();
        Guid album = Guid.CreateVersion7();
        FeatureOrganiseState state = FeatureOrganiseReducers.ReduceFilterToggled(new(), new(MetadataKindEnum.Artist, artist));

        state = FeatureOrganiseReducers.ReduceFilterToggled(state, new(MetadataKindEnum.Album, album));

        Assert.Equal(artist, state.FilterOf(MetadataKindEnum.Artist));
        Assert.Equal(album, state.FilterOf(MetadataKindEnum.Album));
        Assert.Null(state.FilterOf(MetadataKindEnum.Genre));
    }

    [Fact]
    public void FilterCleared_OfAKind_LeavesTheOthers()
    {
        FeatureOrganiseState state = new() { ArtistFilterID = Guid.CreateVersion7(), GenreFilterID = Guid.CreateVersion7() };

        state = FeatureOrganiseReducers.ReduceFilterCleared(state, new(MetadataKindEnum.Artist));

        Assert.Null(state.ArtistFilterID);
        Assert.NotNull(state.GenreFilterID);
    }

    [Fact]
    public void FilterCleared_Null_ClearsAll()
    {
        FeatureOrganiseState state = new() { ArtistFilterID = Guid.CreateVersion7(), AlbumFilterID = Guid.CreateVersion7(), GenreFilterID = Guid.CreateVersion7() };

        state = FeatureOrganiseReducers.ReduceFilterCleared(state, new(null));

        Assert.False(state.HasFilter);
    }

    [Fact]
    public void MetadataListLoaded_ClearsAFilterWhoseItemIsGone()
    {
        Guid kept = Guid.CreateVersion7();
        Guid gone = Guid.CreateVersion7();
        FeatureOrganiseState state = new() { ArtistFilterID = kept, AlbumFilterID = gone, GenreFilterID = gone };

        state = FeatureOrganiseReducers.ReduceMetadataListLoaded(state, new([FakeApiClient.ArtistFor(kept, "Kept")], [], []));

        Assert.Equal(kept, state.ArtistFilterID);
        Assert.Null(state.AlbumFilterID);
        Assert.Null(state.GenreFilterID);
    }

    [Fact]
    public void LibraryMediaLoaded_DropsTickedIDsNotInTheLibrary()
    {
        var media = MediaFileFactory.Create("Kept");
        FeatureOrganiseState state = new() { SelectedSet = [media.ID, Guid.CreateVersion7()] };

        state = FeatureOrganiseReducers.ReduceLibraryMediaLoaded(state, new([media]));

        Assert.Equal([media.ID], state.SelectedSet);
    }

    [Fact]
    public void VisibleSelected_TicksTheShown_AndLeavesHiddenTicksAlone()
    {
        Guid hidden = Guid.CreateVersion7();
        List<Guid> shown = NewIDList(3);
        FeatureOrganiseState state = new() { SelectedSet = [hidden] };

        state = FeatureOrganiseReducers.ReduceVisibleSelected(state, new(shown, false));

        Assert.Equal(4, state.SelectedSet.Count);
        Assert.Contains(hidden, state.SelectedSet);
    }

    [Fact]
    public void VisibleSelected_WhenAllShownAreTicked_UntickThemOnly()
    {
        Guid hidden = Guid.CreateVersion7();
        List<Guid> shown = NewIDList(3);
        FeatureOrganiseState state = new() { SelectedSet = [hidden, .. shown] };

        state = FeatureOrganiseReducers.ReduceVisibleSelected(state, new(shown, true));

        Assert.Equal([hidden], state.SelectedSet);
    }

    [Fact]
    public void VisibleSelected_With1500Shown_TicksOnlyTheFirst1000InOrder()
    {
        List<Guid> shown = NewIDList(1500);

        FeatureOrganiseState state = FeatureOrganiseReducers.ReduceVisibleSelected(new(), new(shown, false));

        Assert.Equal(PresentationConstants.OrganiseSelectionLimit, state.SelectedSet.Count);
        Assert.True(shown.Take(1000).All(state.SelectedSet.Contains));
        Assert.DoesNotContain(shown[1000], state.SelectedSet);
    }

    [Fact]
    public void VisibleSelected_WithSomeAlreadyTicked_TicksOnlyWhatFits()
    {
        List<Guid> ticked = NewIDList(990);
        List<Guid> shown = NewIDList(30);
        FeatureOrganiseState state = new() { SelectedSet = [.. ticked] };

        state = FeatureOrganiseReducers.ReduceVisibleSelected(state, new(shown, false));

        Assert.Equal(1000, state.SelectedSet.Count);
        Assert.True(shown.Take(10).All(state.SelectedSet.Contains));
        Assert.DoesNotContain(shown[10], state.SelectedSet);
    }

    [Fact]
    public void SelectionToggled_At1000_IgnoresATick_ButAllowsAnUntick()
    {
        List<Guid> ticked = NewIDList(1000);
        FeatureOrganiseState state = new() { SelectedSet = [.. ticked] };

        FeatureOrganiseState ignored = FeatureOrganiseReducers.ReduceSelectionToggled(state, new(Guid.CreateVersion7()));
        FeatureOrganiseState unticked = FeatureOrganiseReducers.ReduceSelectionToggled(state, new(ticked[0]));

        Assert.Equal(1000, ignored.SelectedSet.Count);
        Assert.Equal(999, unticked.SelectedSet.Count);
    }

    [Fact]
    public void SelectionToggled_BelowTheLimit_TicksAndUnticks()
    {
        Guid id = Guid.CreateVersion7();

        FeatureOrganiseState state = FeatureOrganiseReducers.ReduceSelectionToggled(new(), new(id));
        Assert.Contains(id, state.SelectedSet);

        state = FeatureOrganiseReducers.ReduceSelectionToggled(state, new(id));
        Assert.Empty(state.SelectedSet);
    }

    [Fact]
    public void SelectionRoom_NeverBelowZero()
    {
        FeatureOrganiseState over = new() { SelectedSet = [.. NewIDList(1200)] };

        Assert.Equal(0, over.SelectionRoom);
        Assert.Equal(1000, new FeatureOrganiseState().SelectionRoom);
    }

    [Fact]
    public void AllFit_IsFalseOnlyWhenTheNewTicksPassTheRoom()
    {
        List<Guid> ticked = NewIDList(990);
        FeatureOrganiseState state = new() { SelectedSet = [.. ticked] };

        Assert.True(state.AllFit(ticked.Concat(NewIDList(10))));
        Assert.False(state.AllFit(NewIDList(11)));
        Assert.Equal(11, state.NewTickCount(NewIDList(11)));
    }

    [Fact]
    public void SelectionCleared_UnticksEverything()
    {
        FeatureOrganiseState state = FeatureOrganiseReducers.ReduceSelectionCleared(new() { SelectedSet = [.. NewIDList(3)] }, new());

        Assert.Empty(state.SelectedSet);
    }

    [Fact]
    public void Reset_ReturnsTheStartState()
    {
        FeatureOrganiseState state = new()
        {
            Tab = MetadataKindEnum.Genre, Search = "x", Sort = LibrarySortEnum.Artist, Descending = true, SelectedSet = [Guid.CreateVersion7()], ArtistFilterID = Guid.CreateVersion7()
        };

        FeatureOrganiseState result = FeatureOrganiseReducers.ReduceReset(state, new());

        Assert.Equal(MetadataKindEnum.Artist, result.Tab);
        Assert.False(result.HasFilter);
        Assert.Empty(result.SelectedSet);
        Assert.Equal(string.Empty, result.Search);
        Assert.False(result.Descending);
    }

    [Fact]
    public void SortChanged_SameSortReverses_NewSortStartsAscending()
    {
        FeatureOrganiseState state = FeatureOrganiseReducers.ReduceSortChanged(new(), new(LibrarySortEnum.Title));
        Assert.True(state.Descending);

        state = FeatureOrganiseReducers.ReduceSortChanged(state, new(LibrarySortEnum.Artist));

        Assert.Equal(LibrarySortEnum.Artist, state.Sort);
        Assert.False(state.Descending);
    }

    [Fact]
    public void TabAndSearchChanged_SetTheirValues()
    {
        FeatureOrganiseState state = FeatureOrganiseReducers.ReduceTabChanged(new(), new(MetadataKindEnum.Album));
        state = FeatureOrganiseReducers.ReduceSearchChanged(state, new("abc"));

        Assert.Equal(MetadataKindEnum.Album, state.Tab);
        Assert.Equal("abc", state.Search);
    }
}
