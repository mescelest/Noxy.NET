using Noxy.NET.MediaPlayer.Domain.Entities;
using Noxy.NET.MediaPlayer.Domain.Extensions;
using Noxy.NET.MediaPlayer.Domain.Models;
using Noxy.NET.MediaPlayer.Domain.Responses.Media;

namespace Noxy.NET.Test.MediaPlayer.Media;

public class MediaTagValuesTest
{
    private static EntityMediaFile CreateFile()
    {
        EntityMediaFile file = new()
        {
            FilePath = "stored.mp3",
            ContentType = "audio/mpeg",
            SizeBytes = 1234,
            ContentHash = new('a', 64),
            UserID = Guid.CreateVersion7()
        };
        file.Metadata = new() { Title = "Media", Subtitle = "Subtitle", Comment = "Comment", TrackNumber = 3, DiscNumber = 2, Year = 1999, MediaFileID = file.ID };

        return file;
    }

    private static EntityArtist Artist(string name)
    {
        return new() { UserID = Guid.CreateVersion7(), Name = name, NameKey = name.ToLowerInvariant() };
    }

    private static ResponseMediaFile CreateResponse(List<Guid>? artistIDList = null, List<Guid>? albumArtistIDList = null, List<Guid>? genreIDList = null)
    {
        return new()
        {
            ID = Guid.CreateVersion7(),
            Title = "Media",
            ContentType = "audio/mpeg",
            SizeBytes = 1,
            TimeCreated = DateTime.UtcNow,
            TimeModified = DateTime.UtcNow,
            Album = "Album",
            Subtitle = "Subtitle",
            Comment = "Comment",
            TrackNumber = 3,
            DiscNumber = 2,
            Year = 1999,
            ArtistIDList = artistIDList ?? [],
            AlbumArtistIDList = albumArtistIDList ?? [],
            GenreIDList = genreIDList ?? []
        };
    }

    [Fact]
    public void Entity_WithLinkedItems_UsesTheirNamesInPositionOrder()
    {
        EntityMediaFile file = CreateFile();
        EntityArtist main = Artist("Main");
        EntityArtist guest = Artist("Guest");
        file.ArtistLinkList =
        [
            new() { MediaFileID = file.ID, ArtistID = guest.ID, Position = 1, Artist = guest },
            new() { MediaFileID = file.ID, ArtistID = main.ID, Position = 0, Artist = main }
        ];
        file.GenreLinkList = [new() { MediaFileID = file.ID, GenreID = Guid.CreateVersion7(), Position = 0, Genre = new() { UserID = file.UserID, Name = "Rock", NameKey = "rock" } }];
        EntityArtist albumArtist = Artist("Queen");
        EntityAlbum album = new() { UserID = file.UserID, Title = "Greatest Hits", TitleKey = "greatest hits", IdentityKey = "greatest hits|x" };
        album.ArtistLinkList = [new() { AlbumID = album.ID, ArtistID = albumArtist.ID, Position = 0, Artist = albumArtist }];
        file.AlbumID = album.ID;
        file.Album = album;

        MediaTagValues values = file.ToTagValues();

        Assert.Equal("Media", values.Title);
        Assert.Equal(["Main", "Guest"], values.ArtistNameList);
        Assert.Equal("Greatest Hits", values.Album);
        Assert.Equal(["Queen"], values.AlbumArtistNameList);
        Assert.Equal(["Rock"], values.GenreNameList);
        Assert.Equal("Subtitle", values.Subtitle);
        Assert.Equal("Comment", values.Comment);
        Assert.Equal(3, values.TrackNumber);
        Assert.Equal(2, values.DiscNumber);
        Assert.Equal(1999, values.Year);
    }

    [Fact]
    public void Entity_WithNoLinks_HasEmptyListsAndNoAlbum()
    {
        MediaTagValues values = CreateFile().ToTagValues();

        Assert.Empty(values.ArtistNameList);
        Assert.Empty(values.AlbumArtistNameList);
        Assert.Empty(values.GenreNameList);
        Assert.Null(values.Album);
    }

    [Fact]
    public void Entity_WithoutMetadata_Throws()
    {
        EntityMediaFile file = CreateFile();
        file.Metadata = null;

        Assert.Throws<ArgumentException>(() => file.ToTagValues());
    }

    [Fact]
    public void Response_ResolvesNamesFromTheIDs_InListOrder()
    {
        Guid main = Guid.CreateVersion7();
        Guid guest = Guid.CreateVersion7();
        Guid rock = Guid.CreateVersion7();
        ResponseMediaFile response = CreateResponse([guest, main], [main], [rock]);
        Dictionary<Guid, string> artistMap = new() { [main] = "Main", [guest] = "Guest" };
        Dictionary<Guid, string> genreMap = new() { [rock] = "Rock" };

        MediaTagValues? values = response.ToTagValues(artistMap, genreMap);

        Assert.NotNull(values);
        Assert.Equal("Media", values.Title);
        Assert.Equal(["Guest", "Main"], values.ArtistNameList);
        Assert.Equal("Album", values.Album);
        Assert.Equal(["Main"], values.AlbumArtistNameList);
        Assert.Equal(["Rock"], values.GenreNameList);
        Assert.Equal("Subtitle", values.Subtitle);
        Assert.Equal("Comment", values.Comment);
        Assert.Equal(3, values.TrackNumber);
        Assert.Equal(2, values.DiscNumber);
        Assert.Equal(1999, values.Year);
    }

    [Fact]
    public void Response_AnArtistNameContainingASemicolon_StaysOneName()
    {
        Guid id = Guid.CreateVersion7();

        MediaTagValues? values = CreateResponse([id]).ToTagValues(new Dictionary<Guid, string> { [id] = "Foo; Bar" }, new Dictionary<Guid, string>());

        Assert.Equal(["Foo; Bar"], values?.ArtistNameList);
    }

    [Fact]
    public void Response_WithNoIDsOfAKind_GivesAnEmptyList()
    {
        MediaTagValues? values = CreateResponse().ToTagValues(new Dictionary<Guid, string>(), new Dictionary<Guid, string>());

        Assert.NotNull(values);
        Assert.Empty(values.ArtistNameList);
        Assert.Empty(values.AlbumArtistNameList);
        Assert.Empty(values.GenreNameList);
    }

    [Theory]
    [InlineData("artist")]
    [InlineData("albumArtist")]
    [InlineData("genre")]
    public void Response_WithAnIDMissingFromTheMap_ReturnsNull(string kind)
    {
        Guid known = Guid.CreateVersion7();
        List<Guid> idList = [known, Guid.CreateVersion7()];
        ResponseMediaFile response = CreateResponse(kind == "artist" ? idList : null, kind == "albumArtist" ? idList : null, kind == "genre" ? idList : null);
        Dictionary<Guid, string> map = new() { [known] = "Known" };

        Assert.Null(response.ToTagValues(map, map));
    }
}
