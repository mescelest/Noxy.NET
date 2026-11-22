using Noxy.NET.MediaPlayer.Domain.Enums;
using Noxy.NET.MediaPlayer.Domain.Responses.Media;
using Noxy.NET.MediaPlayer.Presentation.Extensions;

namespace Noxy.NET.Test.MediaPlayer.Presentation;

public class ResponseMediaFileListExtensionTest
{
    private static readonly DateTime Start = new(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);

    private static ResponseMediaFile Media(string title, string? artist = null, string? album = null, string? albumArtist = null, string? genre = null, int? year = null, int hoursOld = 0)
    {
        return new()
        {
            ID = Guid.CreateVersion7(),
            Title = title,
            ContentType = "audio/mpeg",
            SizeBytes = 1,
            ContentHash = new('a', 64),
            TimeCreated = Start.AddHours(-hoursOld),
            TimeModified = Start,
            Artist = artist,
            Album = album,
            AlbumArtist = albumArtist,
            Genre = genre,
            Year = year
        };
    }

    private static List<string> Titles(List<ResponseMediaFile> list)
    {
        return list.Select(x => x.Title).ToList();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Filter_WithABlankQuery_ReturnsEverythingInTheGivenOrder(string? query)
    {
        List<ResponseMediaFile> list = [Media("B"), Media("A"), Media("C")];

        Assert.Equal(["B", "A", "C"], Titles(list.Filter(query)));
    }

    [Fact]
    public void Filter_MatchesTitleArtistAlbumAlbumArtistAndGenre()
    {
        List<ResponseMediaFile> list =
        [
            Media("zzz-title"),
            Media("Other", artist: "zzz-artist"),
            Media("Other2", album: "zzz-album"),
            Media("Other3", albumArtist: "zzz-albumartist"),
            Media("Other4", genre: "zzz-genre"),
            Media("Unrelated", artist: "Nobody")
        ];

        Assert.Equal(["zzz-title", "Other", "Other2", "Other3", "Other4"], Titles(list.Filter("zzz")));
    }

    [Fact]
    public void Filter_IsCaseInsensitive_AndBySubstring()
    {
        List<ResponseMediaFile> list = [Media("Moonlight Sonata"), Media("Other")];

        Assert.Equal(["Moonlight Sonata"], Titles(list.Filter("NLIGHT")));
    }

    [Fact]
    public void Filter_WithSeveralWords_NeedsEveryWordInAnyField_InAnyOrder()
    {
        List<ResponseMediaFile> list =
        [
            Media("Sonata", artist: "Beethoven"),
            Media("Sonata", artist: "Mozart"),
            Media("Symphony", artist: "Beethoven")
        ];

        Assert.Equal(["Sonata"], Titles(list.Filter("beethoven   sonata")));
        Assert.Equal(["Sonata"], Titles(list.Filter("sonata beethoven")));
    }

    [Fact]
    public void Filter_WithANullField_NeverThrows()
    {
        List<ResponseMediaFile> list = [Media("Only a title")];

        Assert.Empty(list.Filter("artist"));
        Assert.Single(list.Filter("title"));
    }

    [Fact]
    public void Filter_MatchesAccentsAsTyped()
    {
        List<ResponseMediaFile> list = [Media("Café"), Media("Cafe")];

        Assert.Equal(["Café"], Titles(list.Filter("café")));
        Assert.Equal(["Cafe"], Titles(list.Filter("cafe")));
    }

    [Fact]
    public void Filter_NeverChangesTheInputList()
    {
        List<ResponseMediaFile> list = [Media("A"), Media("B")];

        list.Filter("a");

        Assert.Equal(2, list.Count);
    }

    [Fact]
    public void Sort_ByTitle_Ascending_AndDescending()
    {
        List<ResponseMediaFile> list = [Media("b"), Media("A"), Media("c")];

        Assert.Equal(["A", "b", "c"], Titles(list.Sort(LibrarySortEnum.Title, false)));
        Assert.Equal(["c", "b", "A"], Titles(list.Sort(LibrarySortEnum.Title, true)));
    }

    [Fact]
    public void Sort_ByArtist_PutsNullKeysLast_InBothDirections()
    {
        List<ResponseMediaFile> list = [Media("none"), Media("b", artist: "Beta"), Media("a", artist: "Alpha")];

        Assert.Equal(["a", "b", "none"], Titles(list.Sort(LibrarySortEnum.Artist, false)));
        Assert.Equal(["b", "a", "none"], Titles(list.Sort(LibrarySortEnum.Artist, true)));
    }

    [Fact]
    public void Sort_ByAlbum_PutsNullKeysLast_InBothDirections()
    {
        List<ResponseMediaFile> list = [Media("none"), Media("b", album: "Beta"), Media("a", album: "Alpha")];

        Assert.Equal(["a", "b", "none"], Titles(list.Sort(LibrarySortEnum.Album, false)));
        Assert.Equal(["b", "a", "none"], Titles(list.Sort(LibrarySortEnum.Album, true)));
    }

    [Fact]
    public void Sort_ByYear_PutsNullKeysLast_InBothDirections()
    {
        List<ResponseMediaFile> list = [Media("none"), Media("new", year: 2020), Media("old", year: 1990)];

        Assert.Equal(["old", "new", "none"], Titles(list.Sort(LibrarySortEnum.Year, false)));
        Assert.Equal(["new", "old", "none"], Titles(list.Sort(LibrarySortEnum.Year, true)));
    }

    [Fact]
    public void Sort_ByTimeCreated_Ascending_AndDescending()
    {
        List<ResponseMediaFile> list = [Media("middle", hoursOld: 5), Media("newest", hoursOld: 1), Media("oldest", hoursOld: 9)];

        Assert.Equal(["oldest", "middle", "newest"], Titles(list.Sort(LibrarySortEnum.TimeCreated, false)));
        Assert.Equal(["newest", "middle", "oldest"], Titles(list.Sort(LibrarySortEnum.TimeCreated, true)));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Sort_IsStable_EqualKeysKeepTheInputOrder_InBothDirections(bool descending)
    {
        List<ResponseMediaFile> list = [Media("first", artist: "Same"), Media("second", artist: "same"), Media("third", artist: "Same")];

        Assert.Equal(["first", "second", "third"], Titles(list.Sort(LibrarySortEnum.Artist, descending)));
    }

    [Fact]
    public void Sort_NullKeysKeepTheInputOrderAmongThemselves()
    {
        List<ResponseMediaFile> list = [Media("n1"), Media("n2"), Media("a", artist: "A"), Media("n3")];

        Assert.Equal(["a", "n1", "n2", "n3"], Titles(list.Sort(LibrarySortEnum.Artist, false)));
        Assert.Equal(["a", "n1", "n2", "n3"], Titles(list.Sort(LibrarySortEnum.Artist, true)));
    }

    [Fact]
    public void Sort_NeverChangesTheInputList()
    {
        List<ResponseMediaFile> list = [Media("b"), Media("a")];

        list.Sort(LibrarySortEnum.Title, false);

        Assert.Equal(["b", "a"], Titles(list));
    }
}
