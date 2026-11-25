using Fluxor;

namespace Noxy.NET.MediaPlayer.Presentation.Features.MetadataList;

public static class FeatureMetadataListReducers
{
    [ReducerMethod]
    public static FeatureMetadataListState ReduceLoaded(FeatureMetadataListState state, ActionMetadataListLoaded action)
    {
        return state with { ArtistList = action.ArtistList, AlbumList = action.AlbumList, GenreList = action.GenreList };
    }
}
