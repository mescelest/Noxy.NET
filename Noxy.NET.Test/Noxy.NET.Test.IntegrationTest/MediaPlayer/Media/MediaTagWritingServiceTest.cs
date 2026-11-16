using Noxy.NET.MediaPlayer.Application.Services;
using Noxy.NET.MediaPlayer.Domain.Entities;
using Noxy.NET.Test.MediaPlayer.Fixtures;

namespace Noxy.NET.Test.MediaPlayer.Media;

public class MediaTagWritingServiceTest : IDisposable
{
    private readonly string _directory = Path.Combine(Path.GetTempPath(), $"tagwriter-{Guid.CreateVersion7()}");
    private readonly MediaTagWritingService _writer = new();

    public MediaTagWritingServiceTest()
    {
        Directory.CreateDirectory(_directory);
    }

    public void Dispose()
    {
        Directory.Delete(_directory, true);
    }

    private string WriteFile(byte[] bytes, string name = "media.wav")
    {
        string path = Path.Combine(_directory, name);
        File.WriteAllBytes(path, bytes);
        return path;
    }

    private static EntityMediaFile NewFile()
    {
        return new()
        {
            FilePath = "stored.mp3",
            ContentType = "audio/mpeg",
            SizeBytes = 1,
            ContentHash = new string('a', 64),
            UserID = Guid.CreateVersion7()
        };
    }

    // A file as it was before items existed: the text is in the metadata row.
    private static EntityMediaFile Full()
    {
        EntityMediaFile file = NewFile();
        file.Metadata = new()
        {
            Title = "Title",
            Artist = "Artist",
            Album = "Album",
            AlbumArtist = "Album Artist",
            Subtitle = "Subtitle",
            Comment = "Comment",
            Genre = "Genre",
            TrackNumber = 3,
            DiscNumber = 2,
            Year = 1999,
            MediaFileID = file.ID
        };

        return file;
    }

    private static EntityMediaFile Linked(string title, string[] artistList, string? album, string[] albumArtistList, string[] genreList)
    {
        EntityMediaFile file = NewFile();
        file.Metadata = new() { Title = title, MediaFileID = file.ID };
        file.ArtistLinkList = [.. artistList.Select((x, index) => new EntityMediaFileArtist { MediaFileID = file.ID, ArtistID = Guid.CreateVersion7(), Position = index, Artist = new() { UserID = file.UserID, Name = x, NameKey = x.ToLowerInvariant() } })];
        file.GenreLinkList = [.. genreList.Select((x, index) => new EntityMediaFileGenre { MediaFileID = file.ID, GenreID = Guid.CreateVersion7(), Position = index, Genre = new() { UserID = file.UserID, Name = x, NameKey = x.ToLowerInvariant() } })];
        if (album is not null)
        {
            EntityAlbum entity = new() { UserID = file.UserID, Title = album, TitleKey = album.ToLowerInvariant(), IdentityKey = $"{album}|" };
            entity.ArtistLinkList = [.. albumArtistList.Select((x, index) => new EntityAlbumArtist { AlbumID = entity.ID, ArtistID = Guid.CreateVersion7(), Position = index, Artist = new() { UserID = file.UserID, Name = x, NameKey = x.ToLowerInvariant() } })];
            file.AlbumID = entity.ID;
            file.Album = entity;
        }

        return file;
    }

    private static EntityMediaFile TitleOnly()
    {
        EntityMediaFile file = NewFile();
        file.Metadata = new() { Title = "Only", MediaFileID = file.ID };
        return file;
    }

    [Theory]
    [InlineData("media.mp3")]
    [InlineData("media.flac")]
    public void Write_LinkedItems_AreWrittenAsSeparateValues(string name)
    {
        string path = WriteFile(name.EndsWith(".mp3") ? MediaFileFixture.CreateMp3() : MediaFileFixture.CreateFlac(), name);

        _writer.Write(path, Linked("Title", ["Main", "Guest"], "OK Computer", ["Main"], ["Rock", "Pop"]));

        using TagLib.File file = TagLib.File.Create(path);
        Assert.Equal(["Main", "Guest"], file.Tag.Performers);
        Assert.Equal("OK Computer", file.Tag.Album);
        Assert.Equal(["Main"], file.Tag.AlbumArtists);
        Assert.Equal(["Rock", "Pop"], file.Tag.Genres);
    }

    [Fact]
    public void Write_ARealNameWithASlash_StaysOneValueInAnMp3()
    {
        string path = WriteFile(MediaFileFixture.CreateMp3(), "media.mp3");

        _writer.Write(path, Linked("Title", ["AC/DC", "Guest"], null, [], ["R&B/Soul"]));

        using TagLib.File file = TagLib.File.Create(path);
        Assert.Equal(["AC/DC", "Guest"], file.Tag.Performers);
        Assert.Equal(["R&B/Soul"], file.Tag.Genres);
    }

    [Fact]
    public void Write_AnItemRenamedAfterTheUpload_WritesTheNewName()
    {
        EntityMediaFile mediaFile = Linked("Title", ["Old"], null, [], []);
        string path = WriteFile(MediaFileFixture.CreateWav());
        (mediaFile.ArtistLinkList[0].Artist ?? throw new InvalidOperationException("The link has no artist.")).Name = "New";

        _writer.Write(path, mediaFile);

        using TagLib.File file = TagLib.File.Create(path);
        Assert.Equal("New", Assert.Single(file.Tag.Performers));
    }

    [Fact]
    public void Write_AFileWithoutLinksOrLegacyText_ClearsTheKinds()
    {
        string path = WriteFile(MediaFileFixture.CreateWav());
        _writer.Write(path, Full());

        _writer.Write(path, TitleOnly());

        using TagLib.File file = TagLib.File.Create(path);
        Assert.Empty(file.Tag.Performers);
        Assert.Empty(file.Tag.AlbumArtists);
        Assert.Empty(file.Tag.Genres);
        Assert.Null(file.Tag.Album);
    }

    [Fact]
    public void Write_AFileWithoutMetadata_Throws()
    {
        string path = WriteFile(MediaFileFixture.CreateWav());

        Assert.Throws<ArgumentException>(() => _writer.Write(path, NewFile()));
    }

    [Fact]
    public void Write_SetsEveryTagAndReadingItBackGivesThem()
    {
        string path = WriteFile(MediaFileFixture.CreateWav());

        _writer.Write(path, Full());

        using TagLib.File file = TagLib.File.Create(path);
        Assert.Equal("Title", file.Tag.Title);
        Assert.Equal("Artist", Assert.Single(file.Tag.Performers));
        Assert.Equal("Album", file.Tag.Album);
        Assert.Equal("Album Artist", Assert.Single(file.Tag.AlbumArtists));
        Assert.Equal("Subtitle", file.Tag.Subtitle);
        Assert.Equal("Comment", file.Tag.Comment);
        Assert.Equal("Genre", Assert.Single(file.Tag.Genres));
        Assert.Equal(3u, file.Tag.Track);
        Assert.Equal(2u, file.Tag.Disc);
        Assert.Equal(1999u, file.Tag.Year);
    }

    [Fact]
    public void Write_NullFields_ClearExistingTags()
    {
        string path = WriteFile(MediaFileFixture.CreateWav());
        _writer.Write(path, Full());

        _writer.Write(path, TitleOnly());

        using TagLib.File file = TagLib.File.Create(path);
        Assert.Equal("Only", file.Tag.Title);
        Assert.Empty(file.Tag.Performers);
        Assert.Null(file.Tag.Album);
        Assert.Empty(file.Tag.Genres);
        Assert.Equal(0u, file.Tag.Year);
        Assert.Equal(0u, file.Tag.Track);
    }

    [Fact]
    public void Write_UnparseableFile_Throws()
    {
        string path = WriteFile("RIFF-not-really-a-wave-file"u8.ToArray());

        Assert.ThrowsAny<Exception>(() => _writer.Write(path, Full()));
    }

    [Fact]
    public void Write_LeavesTheAudioBytesUntouched()
    {
        byte[] pattern = MediaFileFixture.Pattern();
        string path = WriteFile(MediaFileFixture.CreateWav());

        _writer.Write(path, Full());

        Assert.True(File.ReadAllBytes(path).AsSpan().IndexOf(pattern) >= 0);
    }
}
