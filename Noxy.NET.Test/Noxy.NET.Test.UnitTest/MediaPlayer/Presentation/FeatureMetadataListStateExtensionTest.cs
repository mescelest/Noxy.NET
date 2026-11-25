using System.Collections;
using System.ComponentModel;
using Noxy.NET.MediaPlayer.Domain.Responses.Playlist;
using Noxy.NET.Test.MediaPlayer.Fakes;

namespace Noxy.NET.Test.MediaPlayer.Presentation;

public class FeatureMetadataListStateExtensionTest
{
    private static FeatureMetadataListState State()
    {
        return new()
        {
            ArtistList = [FakeApiClient.ArtistFor(Guid.CreateVersion7(), "bravo"), FakeApiClient.ArtistFor(Guid.CreateVersion7(), "Alpha")],
            AlbumList = [FakeApiClient.AlbumFor(Guid.CreateVersion7(), "Zed"), FakeApiClient.AlbumFor(Guid.CreateVersion7(), "Yan")],
            GenreList = [FakeApiClient.GenreFor(Guid.CreateVersion7(), "Rock")]
        };
    }

    [Fact]
    public void ItemListOf_Artist_IsInNameOrder_IgnoringCase()
    {
        Assert.Equal(["Alpha", "bravo"], State().ItemListOf(MetadataKindEnum.Artist).Select(x => x.Name));
    }

    [Fact]
    public void ItemListOf_Album_NamesAnAlbumByItsTitle()
    {
        Assert.Equal(["Yan", "Zed"], State().ItemListOf(MetadataKindEnum.Album).Select(x => x.Name));
    }

    [Fact]
    public void ItemListOf_Genre_ListsTheGenres()
    {
        Assert.Equal(["Rock"], State().ItemListOf(MetadataKindEnum.Genre).Select(x => x.Name));
    }

    [Fact]
    public void ItemListOf_AKindOutsideTheEnum_Throws()
    {
        Assert.Throws<InvalidEnumArgumentException>(() => State().ItemListOf((MetadataKindEnum)99));
    }

    [Fact]
    public void FilterByName_KeepsTheItemsWhoseNameContainsTheText_IgnoringCase_AndKeepsAllForBlank()
    {
        List<MetadataItem> itemList = State().ItemListOf(MetadataKindEnum.Artist);

        Assert.Equal(["bravo"], Enumerable.Select<ResponsePlaylist, string>(itemList.FilterByName(" RAV "), x => x.Name));
        Assert.Equal(2, itemList.FilterByName("  ").Count);
        Assert.Equal(2, itemList.FilterByName(null).Count);
        Assert.Empty((IEnumerable)itemList.FilterByName("zzz"));
    }
}
