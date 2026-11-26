using Noxy.NET.MediaPlayer.Application.Services;
using Noxy.NET.MediaPlayer.Domain.Entities;
using Noxy.NET.MediaPlayer.Domain.Extensions;
using Noxy.NET.Test.MediaPlayer.Fixtures;
using TagLib;
using TagLib.Id3v2;
using File = System.IO.File;
using Tag = TagLib.Id3v2.Tag;

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
            ContentHash = new('a', 64),
            UserID = Guid.CreateVersion7()
        };
    }

    // Every tag the writer writes, with the artists, album, album artists and genre as items.
    private static EntityMediaFile Full()
    {
        EntityMediaFile file = Linked("Title", ["Artist"], "Album", ["Album Artist"], ["Genre"]);
        EntityMediaFileMetadata metadata = file.Metadata ?? throw new InvalidOperationException("The media file has no metadata.");
        metadata.Subtitle = "Subtitle";
        metadata.Comment = "Comment";
        metadata.TrackNumber = 3;
        metadata.DiscNumber = 2;
        metadata.Year = 1999;

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

        _writer.Write(path, Linked("Title", ["Main", "Guest"], "OK Computer", ["Main"], ["Rock", "Pop"]).ToTagValues());

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

        _writer.Write(path, Linked("Title", ["AC/DC", "Guest"], null, [], ["R&B/Soul"]).ToTagValues());

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

        _writer.Write(path, mediaFile.ToTagValues());

        using TagLib.File file = TagLib.File.Create(path);
        Assert.Equal("New", Assert.Single(file.Tag.Performers));
    }

    [Fact]
    public void Write_AFileWithoutLinks_ClearsTheKinds()
    {
        string path = WriteFile(MediaFileFixture.CreateWav());
        _writer.Write(path, Full().ToTagValues());

        _writer.Write(path, TitleOnly().ToTagValues());

        using TagLib.File file = TagLib.File.Create(path);
        Assert.Empty(file.Tag.Performers);
        Assert.Empty(file.Tag.AlbumArtists);
        Assert.Empty(file.Tag.Genres);
        Assert.Null(file.Tag.Album);
    }

    [Fact]
    public void Write_SetsEveryTagAndReadingItBackGivesThem()
    {
        string path = WriteFile(MediaFileFixture.CreateWav());

        _writer.Write(path, Full().ToTagValues());

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
        _writer.Write(path, Full().ToTagValues());

        _writer.Write(path, TitleOnly().ToTagValues());

        using TagLib.File file = TagLib.File.Create(path);
        Assert.Equal("Only", file.Tag.Title);
        Assert.Empty(file.Tag.Performers);
        Assert.Null(file.Tag.Album);
        Assert.Empty(file.Tag.Genres);
        Assert.Equal(0u, file.Tag.Year);
        Assert.Equal(0u, file.Tag.Track);
    }

    [Fact]
    public void Write_NullTrackAndDisc_RemoveTheFramesEvenWhenTheFileHadATotal()
    {
        string path = WriteFile(MediaFileFixture.CreateWav());
        using (TagLib.File seed = TagLib.File.Create(path))
        {
            seed.Tag.Track = 5;
            seed.Tag.TrackCount = 12;
            seed.Tag.Disc = 1;
            seed.Tag.DiscCount = 2;
            seed.Save();
        }

        _writer.Write(path, TitleOnly().ToTagValues());

        using TagLib.File file = TagLib.File.Create(path);
        Tag id3 = (Tag)file.GetTag(TagTypes.Id3v2);
        Assert.Null(id3.GetTextAsString("TRCK"));
        Assert.Null(id3.GetTextAsString("TPOS"));
    }

    [Fact]
    public void Write_StandardGenreNames_AreWrittenAsTextNotAsID3v1Numbers()
    {
        string path = WriteFile(MediaFileFixture.CreateWav());
        _writer.Write(path, Linked("T", ["A"], null, [], ["Rock", "Trip Hop", "Synthwave"]).ToTagValues());

        using TagLib.File file = TagLib.File.Create(path);
        Tag id3 = (Tag)file.GetTag(TagTypes.Id3v2);
        TextInformationFrame frame = TextInformationFrame.Get(id3, "TCON", false);
        Assert.Equal(["Rock", "Trip Hop", "Synthwave"], frame.Text);
    }

    [Fact]
    public void Write_KeepsTheGivenGenreOrder()
    {
        string path = WriteFile(MediaFileFixture.CreateWav());

        _writer.Write(path, Linked("T", ["A"], null, [], ["Rock", "Soundtrack", "Ambient"]).ToTagValues());

        using TagLib.File file = TagLib.File.Create(path);
        Tag id3 = (Tag)file.GetTag(TagTypes.Id3v2);
        Assert.Equal(["Rock", "Soundtrack", "Ambient"], TextInformationFrame.Get(id3, "TCON", false).Text);
    }

    [Fact]
    public void Write_APicture_ReplacesExistingPictures_AndReadsBackAsTheSameBytes()
    {
        string path = WriteFile(MediaFileFixture.CreateWav());
        using (TagLib.File seed = TagLib.File.Create(path))
        {
            seed.Tag.Pictures = [new Picture(new ByteVector([1, 1, 1])), new Picture(new ByteVector([2, 2, 2]))];
            seed.Save();
        }

        byte[] cover = [9, 8, 7, 6, 5];
        _writer.Write(path, TitleOnly().ToTagValues(), cover);

        using TagLib.File file = TagLib.File.Create(path);
        IPicture picture = Assert.Single(file.Tag.Pictures);
        Assert.Equal(PictureType.FrontCover, picture.Type);
        Assert.Equal(cover, picture.Data.Data);
    }

    [Fact]
    public void Write_NoPicture_LeavesThePicturesAlone()
    {
        string path = WriteFile(MediaFileFixture.CreateWav());
        using (TagLib.File seed = TagLib.File.Create(path))
        {
            seed.Tag.Pictures = [new Picture(new ByteVector([1, 1, 1]))];
            seed.Save();
        }

        _writer.Write(path, TitleOnly().ToTagValues());

        using TagLib.File file = TagLib.File.Create(path);
        Assert.Equal([1, 1, 1], Assert.Single(file.Tag.Pictures).Data.Data);
    }

    [Fact]
    public void Write_UnparseableFile_Throws()
    {
        string path = WriteFile("RIFF-not-really-a-wave-file"u8.ToArray());

        Assert.ThrowsAny<Exception>(() => _writer.Write(path, Full().ToTagValues()));
    }

    [Fact]
    public void Write_LeavesTheAudioBytesUntouched()
    {
        byte[] pattern = MediaFileFixture.Pattern();
        string path = WriteFile(MediaFileFixture.CreateWav());

        _writer.Write(path, Full().ToTagValues());

        Assert.True(File.ReadAllBytes(path).AsSpan().IndexOf(pattern) >= 0);
    }
}
