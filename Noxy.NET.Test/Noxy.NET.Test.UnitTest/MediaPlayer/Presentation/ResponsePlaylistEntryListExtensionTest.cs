using Noxy.NET.MediaPlayer.Domain.Enums;
using Noxy.NET.MediaPlayer.Domain.Responses.Media;
using Noxy.NET.MediaPlayer.Domain.Responses.Playlist;
using Noxy.NET.MediaPlayer.Presentation.Extensions;

namespace Noxy.NET.Test.MediaPlayer.Presentation;

public class ResponsePlaylistEntryListExtensionTest
{
    private static readonly DateTime Start = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    private static ResponseMediaFile Media(string title, string? artist = null, string? album = null, int? year = null, string? genre = null, string? albumArtist = null, int addedDays = 0)
    {
        return new()
        {
            ID = Guid.CreateVersion7(),
            Title = title,
            Artist = artist,
            Album = album,
            Year = year,
            Genre = genre,
            AlbumArtist = albumArtist,
            ContentType = "audio/mpeg",
            SizeBytes = 1,
            ContentHash = "h",
            TimeCreated = Start.AddDays(addedDays),
            TimeModified = Start
        };
    }

    private static ResponsePlaylistEntry Entry(ResponseMediaFile media, int position)
    {
        return new() { ID = Guid.CreateVersion7(), MediaFileID = media.ID, Position = position, MediaFile = media };
    }

    private static List<ResponsePlaylistEntry> EntryListOf(params ResponseMediaFile[] mediaList)
    {
        return [.. mediaList.Select(Entry)];
    }

    private static List<string> TitleListOf(List<ResponsePlaylistEntry> entryList)
    {
        return [.. entryList.Select(x => x.MediaFile.Title)];
    }

    [Fact]
    public void ToDisplayList_WithNoSortAndNoSearch_KeepsTheStoredOrder()
    {
        List<ResponsePlaylistEntry> entryList = EntryListOf(Media("C"), Media("A"), Media("B"));

        Assert.Equal(["C", "A", "B"], TitleListOf(entryList.ToDisplayList(null, null, false)));
        Assert.Equal(["C", "A", "B"], TitleListOf(entryList.ToDisplayList("  ", null, true)));
    }

    [Theory]
    [InlineData(LibrarySortEnum.Title)]
    [InlineData(LibrarySortEnum.Artist)]
    [InlineData(LibrarySortEnum.Album)]
    [InlineData(LibrarySortEnum.Year)]
    [InlineData(LibrarySortEnum.TimeCreated)]
    public void ToDisplayList_SortsByEachColumn_AscendingAndDescending(LibrarySortEnum sort)
    {
        ResponseMediaFile low = Media("Low", "A", "A", 1990, addedDays: 1);
        ResponseMediaFile mid = Media("Mid", "B", "B", 2000, addedDays: 2);
        ResponseMediaFile high = Media("Nigh", "C", "C", 2010, addedDays: 3);
        List<ResponsePlaylistEntry> entryList = EntryListOf(mid, high, low);

        Assert.Equal(["Low", "Mid", "Nigh"], TitleListOf(entryList.ToDisplayList(null, sort, false)));
        Assert.Equal(["Nigh", "Mid", "Low"], TitleListOf(entryList.ToDisplayList(null, sort, true)));
    }

    [Fact]
    public void ToDisplayList_KeepsBothEntriesOfADuplicatedMediaFile()
    {
        ResponseMediaFile twice = Media("Twice", "Artist");
        ResponseMediaFile other = Media("Other", "Zed");
        List<ResponsePlaylistEntry> entryList = [Entry(twice, 0), Entry(other, 1), Entry(twice, 2)];

        List<ResponsePlaylistEntry> sorted = entryList.ToDisplayList(null, LibrarySortEnum.Title, false);
        List<ResponsePlaylistEntry> filtered = entryList.ToDisplayList("twice", null, false);

        Assert.Equal(3, sorted.Count);
        Assert.Equal([entryList[1].ID, entryList[0].ID, entryList[2].ID], sorted.Select(x => x.ID));
        Assert.Equal([entryList[0].ID, entryList[2].ID], filtered.Select(x => x.ID));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ToDisplayList_PutsEntriesWithNoKeyLast_InBothDirections(bool descending)
    {
        List<ResponsePlaylistEntry> entryList = EntryListOf(Media("NoArtist"), Media("B", "B"), Media("A", "A"));

        List<string> titleList = TitleListOf(entryList.ToDisplayList(null, LibrarySortEnum.Artist, descending));

        Assert.Equal("NoArtist", titleList[^1]);
    }

    [Fact]
    public void ToDisplayList_FiltersByEveryWordAcrossTitleArtistAlbumAlbumArtistAndGenre()
    {
        List<ResponsePlaylistEntry> entryList = EntryListOf(
            Media("Blue Monday", "New Order", "Power", genre: "Synth"),
            Media("Other", albumArtist: "New Order", genre: "Synth"),
            Media("Third", "Someone", "Else"));

        Assert.Equal(["Blue Monday", "Other"], TitleListOf(entryList.ToDisplayList("new synth", null, false)));
        Assert.Equal(["Blue Monday"], TitleListOf(entryList.ToDisplayList("monday power", null, false)));
        Assert.Empty(entryList.ToDisplayList("nothing here", null, false));
    }

    [Fact]
    public void ToDisplayList_IsStable_ForEqualKeys()
    {
        List<ResponsePlaylistEntry> entryList = EntryListOf(Media("First", "Same"), Media("Second", "Same"), Media("Third", "Same"));

        Assert.Equal(["First", "Second", "Third"], TitleListOf(entryList.ToDisplayList(null, LibrarySortEnum.Artist, false)));
        Assert.Equal(["First", "Second", "Third"], TitleListOf(entryList.ToDisplayList(null, LibrarySortEnum.Artist, true)));
    }

    [Fact]
    public void ToDisplayList_ReturnsANewList()
    {
        List<ResponsePlaylistEntry> entryList = EntryListOf(Media("A"));

        Assert.NotSame(entryList, entryList.ToDisplayList(null, null, false));
    }

    [Theory]
    [InlineData(null, null, true)]
    [InlineData("", null, true)]
    [InlineData("   ", null, true)]
    [InlineData("word", null, false)]
    [InlineData(null, LibrarySortEnum.Title, false)]
    [InlineData("word", LibrarySortEnum.Title, false)]
    public void CanReorder_IsTrueOnlyWithNoSearchAndNoSort(string? search, LibrarySortEnum? sort, bool expected)
    {
        Assert.Equal(expected, ResponsePlaylistEntryListExtension.CanReorder(search, sort));
    }
}
