using Noxy.NET.Test.MediaPlayer.Fakes;

namespace Noxy.NET.Test.MediaPlayer.Presentation;

public class FeatureMetadataListReducersTest
{
    [Fact]
    public void MetadataListLoaded_ReplacesTheThreeLists()
    {
        FeatureMetadataListState state = FeatureMetadataListReducers.ReduceLoaded(new(), new([FakeApiClient.ArtistFor(Guid.CreateVersion7(), "Old")], [], []));

        state = FeatureMetadataListReducers.ReduceLoaded(state, new([], [FakeApiClient.AlbumFor(Guid.CreateVersion7(), "Album")], [FakeApiClient.GenreFor(Guid.CreateVersion7(), "Genre")]));

        Assert.Empty(state.ArtistList);
        Assert.Equal("Album", Assert.Single(state.AlbumList).Title);
        Assert.Equal("Genre", Assert.Single(state.GenreList).Name);
    }
}
