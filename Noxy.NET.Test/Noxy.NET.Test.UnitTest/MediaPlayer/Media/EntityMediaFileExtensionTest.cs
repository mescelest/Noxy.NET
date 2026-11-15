using Noxy.NET.MediaPlayer.Domain.Entities;
using Noxy.NET.MediaPlayer.Domain.Extensions;
using Noxy.NET.MediaPlayer.Domain.Responses.Media;

namespace Noxy.NET.Test.MediaPlayer.Media;

public class EntityMediaFileExtensionTest
{
    private static EntityMediaFile CreateFile(DateTimeKind kind = DateTimeKind.Unspecified)
    {
        EntityMediaFile file = new()
        {
            TimeCreated = new(2026, 1, 2, 3, 4, 5, kind),
            TimeModified = new(2026, 2, 3, 4, 5, 6, kind),
            FilePath = "stored.mp3",
            ContentType = "audio/mpeg",
            SizeBytes = 1234,
            ContentHash = new string('a', 64),
            UserID = Guid.CreateVersion7()
        };
        file.Metadata = new() { Title = "Media", MediaFileID = file.ID };
        return file;
    }

    [Fact]
    public void ToResponse_CopiesEveryTagFromTheMetadata()
    {
        EntityMediaFile file = CreateFile();
        Assert.NotNull(file.Metadata);
        file.Metadata.Artist = "Artist";
        file.Metadata.Album = "Album";
        file.Metadata.AlbumArtist = "Album Artist";
        file.Metadata.Subtitle = "Subtitle";
        file.Metadata.Comment = "Comment";
        file.Metadata.Genre = "Genre";
        file.Metadata.TrackNumber = 3;
        file.Metadata.DiscNumber = 2;
        file.Metadata.Year = 1999;
        file.Metadata.Duration = TimeSpan.FromSeconds(215);

        ResponseMediaFile response = file.ToResponse();

        Assert.Equal("Media", response.Title);
        Assert.Equal("Artist", response.Artist);
        Assert.Equal("Album", response.Album);
        Assert.Equal("Album Artist", response.AlbumArtist);
        Assert.Equal("Subtitle", response.Subtitle);
        Assert.Equal("Comment", response.Comment);
        Assert.Equal("Genre", response.Genre);
        Assert.Equal(3, response.TrackNumber);
        Assert.Equal(2, response.DiscNumber);
        Assert.Equal(1999, response.Year);
        Assert.Equal(TimeSpan.FromSeconds(215), response.Duration);
    }

    [Fact]
    public void ToResponse_NoMetadata_LeavesTheTagsNullAndTitleIsTheFilePath()
    {
        EntityMediaFile file = CreateFile();
        file.Metadata = null;

        ResponseMediaFile response = file.ToResponse();

        Assert.Equal("stored.mp3", response.Title);
        Assert.Null(response.Artist);
        Assert.Null(response.Album);
        Assert.Null(response.AlbumArtist);
        Assert.Null(response.Subtitle);
        Assert.Null(response.Comment);
        Assert.Null(response.Genre);
        Assert.Null(response.TrackNumber);
        Assert.Null(response.DiscNumber);
        Assert.Null(response.Year);
        Assert.Null(response.Duration);
    }

    [Theory]
    [InlineData(DateTimeKind.Unspecified)]
    [InlineData(DateTimeKind.Local)]
    [InlineData(DateTimeKind.Utc)]
    public void ToResponse_TimesAreUtc(DateTimeKind kind)
    {
        ResponseMediaFile response = CreateFile(kind).ToResponse();

        Assert.Equal(DateTimeKind.Utc, response.TimeCreated.Kind);
        Assert.Equal(DateTimeKind.Utc, response.TimeModified.Kind);
    }

    private static EntityArtist Artist(string name)
    {
        return new() { UserID = Guid.CreateVersion7(), Name = name, NameKey = name.ToLowerInvariant() };
    }

    private static void LinkArtistList(EntityMediaFile file, params EntityArtist[] artistList)
    {
        file.ArtistLinkList = [.. artistList.Select((x, index) => new EntityMediaFileArtist { MediaFileID = file.ID, ArtistID = x.ID, Position = index, Artist = x })];
    }

    private static void LinkGenreList(EntityMediaFile file, params string[] nameList)
    {
        file.GenreLinkList = [.. nameList.Select((x, index) => new EntityMediaFileGenre { MediaFileID = file.ID, GenreID = Guid.CreateVersion7(), Position = index, Genre = new() { UserID = file.UserID, Name = x, NameKey = x.ToLowerInvariant() } })];
    }

    [Fact]
    public void ToResponse_ArtistIsTheLinkedNamesJoinedInPositionOrder()
    {
        EntityMediaFile file = CreateFile();
        EntityArtist first = Artist("Main");
        EntityArtist second = Artist("Guest");
        LinkArtistList(file, first, second);

        ResponseMediaFile response = file.ToResponse();

        Assert.Equal("Main; Guest", response.Artist);
        Assert.Equal([first.ID, second.ID], response.ArtistIDList);
    }

    [Fact]
    public void ToResponse_GenreIsTheLinkedNamesJoinedInPositionOrder()
    {
        EntityMediaFile file = CreateFile();
        LinkGenreList(file, "Rock", "Pop");

        ResponseMediaFile response = file.ToResponse();

        Assert.Equal("Rock; Pop", response.Genre);
        Assert.Equal(file.GenreLinkList.Select(x => x.GenreID).ToList(), response.GenreIDList);
    }

    [Fact]
    public void ToResponse_AlbumAndAlbumArtistComeFromTheAlbumAndItsArtists()
    {
        EntityMediaFile file = CreateFile();
        EntityArtist albumArtist = Artist("Queen");
        EntityAlbum album = new() { UserID = file.UserID, Title = "Greatest Hits", TitleKey = "greatest hits", IdentityKey = "greatest hits|x" };
        album.ArtistLinkList = [new() { AlbumID = album.ID, ArtistID = albumArtist.ID, Position = 0, Artist = albumArtist }];
        file.AlbumID = album.ID;
        file.Album = album;

        ResponseMediaFile response = file.ToResponse();

        Assert.Equal("Greatest Hits", response.Album);
        Assert.Equal("Queen", response.AlbumArtist);
        Assert.Equal(album.ID, response.AlbumID);
        Assert.Equal([albumArtist.ID], response.AlbumArtistIDList);
    }

    [Fact]
    public void ToResponse_AFileWithNoLinksAndNoLegacyText_HasNullTextAndEmptyIDLists()
    {
        EntityMediaFile file = CreateFile();

        ResponseMediaFile response = file.ToResponse();

        Assert.Null(response.Artist);
        Assert.Null(response.Album);
        Assert.Null(response.AlbumArtist);
        Assert.Null(response.Genre);
        Assert.Empty(response.ArtistIDList);
        Assert.Null(response.AlbumID);
        Assert.Empty(response.AlbumArtistIDList);
        Assert.Empty(response.GenreIDList);
    }

    [Fact]
    public void ToResponse_LinkedNames_WinOverTheLegacyText()
    {
        EntityMediaFile file = CreateFile();
        Assert.NotNull(file.Metadata);
        file.Metadata.Artist = "Legacy";
        file.Metadata.Genre = "Legacy genre";
        LinkArtistList(file, Artist("Linked"));
        LinkGenreList(file, "Linked genre");

        ResponseMediaFile response = file.ToResponse();

        Assert.Equal("Linked", response.Artist);
        Assert.Equal("Linked genre", response.Genre);
    }

    [Fact]
    public void ToResponse_AFileNotYetBackfilled_StillShowsItsLegacyText()
    {
        EntityMediaFile file = CreateFile();
        Assert.NotNull(file.Metadata);
        file.Metadata.Artist = "Legacy";
        file.Metadata.Album = "Legacy album";
        file.Metadata.AlbumArtist = "Legacy album artist";
        file.Metadata.Genre = "Legacy genre";

        ResponseMediaFile response = file.ToResponse();

        Assert.Equal("Legacy", response.Artist);
        Assert.Equal("Legacy album", response.Album);
        Assert.Equal("Legacy album artist", response.AlbumArtist);
        Assert.Equal("Legacy genre", response.Genre);
        Assert.Empty(response.ArtistIDList);
    }

    [Fact]
    public void ToResponse_ALinkWhoseItemWasNotLoaded_IsABug_NamingTheMediaFile()
    {
        EntityMediaFile file = CreateFile();
        file.ArtistLinkList = [new() { MediaFileID = file.ID, ArtistID = Guid.CreateVersion7(), Position = 0 }];

        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() => file.ToResponse());

        Assert.Contains(file.ID.ToString(), exception.Message);
    }

    [Fact]
    public void GetDownloadFileName_UsesTheTitleAndTheStoredExtension()
    {
        EntityMediaFile file = CreateFile();

        Assert.Equal("Media.mp3", file.GetDownloadFileName());
    }

    [Fact]
    public void GetDownloadFileName_WithoutMetadata_UsesTheStoredFileName()
    {
        EntityMediaFile file = CreateFile();
        file.Metadata = null;

        Assert.Equal("stored.mp3", file.GetDownloadFileName());
    }
}
