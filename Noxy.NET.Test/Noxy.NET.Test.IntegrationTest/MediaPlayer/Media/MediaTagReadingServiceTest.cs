using Microsoft.Extensions.Logging;
using Noxy.NET.MediaPlayer.Application.Models.Metadata;
using Noxy.NET.MediaPlayer.Application.Services;
using Noxy.NET.Test.MediaPlayer.Fakes;
using Noxy.NET.Test.MediaPlayer.Fixtures;
using TagLib;
using TagLib.Mpeg;
using File = System.IO.File;

namespace Noxy.NET.Test.MediaPlayer.Media;

public class MediaTagReadingServiceTest : IDisposable
{
    private readonly string _directory = Path.Combine(Path.GetTempPath(), $"tagreader-{Guid.CreateVersion7()}");
    private readonly FakeLogger<MediaTagReadingService> _logger = new();
    private readonly MediaTagReadingService _reader;

    public MediaTagReadingServiceTest()
    {
        Directory.CreateDirectory(_directory);
        _reader = new(_logger);
    }

    public void Dispose()
    {
        Directory.Delete(_directory, true);
    }

    private string WriteFile(byte[] bytes, string name)
    {
        string path = Path.Combine(_directory, name);
        File.WriteAllBytes(path, bytes);
        return path;
    }

    // An mp3 gets an ID3v2.4 tag unless told otherwise: only that version can hold several values in one frame.
    private static void Tag(string path, Action<Tag> apply, byte id3Version = 4)
    {
        using TagLib.File file = TagLib.File.Create(path);
        if (file is AudioFile)
        {
            TagLib.Id3v2.Tag id3 = (TagLib.Id3v2.Tag)file.GetTag(TagTypes.Id3v2, true);
            id3.Version = id3Version;
        }

        apply(file.Tag);
        file.Save();
    }

    private static void FillEveryTag(Tag tag)
    {
        tag.Title = "Title";
        tag.Performers = ["Artist One", "Artist Two"];
        tag.Album = "Album";
        tag.AlbumArtists = ["Album Artist"];
        tag.Genres = ["Rock", "Pop"];
        tag.Track = 3;
        tag.Disc = 2;
        tag.Year = 1999;
        tag.Comment = "Comment";
        tag.Subtitle = "Subtitle";
    }

    private static void AssertEveryTag(MediaTagSet set)
    {
        Assert.Equal("Title", set.Title);
        Assert.Equal(["Artist One", "Artist Two"], set.ArtistList);
        Assert.Equal("Album", set.Album);
        Assert.Equal(["Album Artist"], set.AlbumArtistList);
        Assert.Equal(["Rock", "Pop"], set.GenreList);
        Assert.Equal(3, set.TrackNumber);
        Assert.Equal(2, set.DiscNumber);
        Assert.Equal(1999, set.Year);
        Assert.Equal("Comment", set.Comment);
    }

    private static void AssertEmpty(MediaTagSet set)
    {
        Assert.Null(set.Title);
        Assert.Empty(set.ArtistList);
        Assert.Null(set.Album);
        Assert.Empty(set.AlbumArtistList);
        Assert.Empty(set.GenreList);
        Assert.Null(set.TrackNumber);
        Assert.Null(set.DiscNumber);
        Assert.Null(set.Year);
        Assert.Null(set.Comment);
        Assert.Null(set.Subtitle);
    }

    [Fact]
    public void Read_Mp3_ReturnsEveryTagIncludingSeveralArtistsAndGenres()
    {
        string path = WriteFile(MediaFileFixture.CreateMp3(), "media.mp3");
        Tag(path, FillEveryTag);

        MediaTagSet set = _reader.Read(path);

        AssertEveryTag(set);
    }

    [Fact]
    public void Read_Flac_ReturnsEveryTagIncludingSeveralArtistsAndGenres()
    {
        string path = WriteFile(MediaFileFixture.CreateFlac(), "media.flac");
        Tag(path, FillEveryTag);

        MediaTagSet set = _reader.Read(path);

        AssertEveryTag(set);
        Assert.Equal("Subtitle", set.Subtitle);
    }

    [Fact]
    public void Read_Wav_ReturnsTheTags()
    {
        string path = WriteFile(MediaFileFixture.CreateWav(), "media.wav");
        Tag(path, tag =>
        {
            tag.Title = "Title";
            tag.Performers = ["Artist"];
            tag.Album = "Album";
        });

        MediaTagSet set = _reader.Read(path);

        Assert.Equal("Title", set.Title);
        Assert.Equal(["Artist"], set.ArtistList);
        Assert.Equal("Album", set.Album);
    }

    [Theory]
    [InlineData((byte)3)]
    [InlineData((byte)4)]
    public void Read_Mp3_KeepsARealNameWithASlashAsOneValue(byte id3Version)
    {
        string path = WriteFile(MediaFileFixture.CreateMp3(), "media.mp3");
        Tag(path, tag =>
        {
            tag.Performers = ["AC/DC"];
            tag.AlbumArtists = ["Simon & Garfunkel"];
            tag.Genres = ["R&B/Soul"];
        }, id3Version);

        MediaTagSet set = _reader.Read(path);

        Assert.Equal(["AC/DC"], set.ArtistList);
        Assert.Equal(["Simon & Garfunkel"], set.AlbumArtistList);
        Assert.Equal(["R&B/Soul"], set.GenreList);
    }

    [Fact]
    public void Read_Flac_KeepsARealNameWithASlashAsOneValue()
    {
        string path = WriteFile(MediaFileFixture.CreateFlac(), "media.flac");
        Tag(path, tag => tag.Performers = ["AC/DC"]);

        MediaTagSet set = _reader.Read(path);

        Assert.Equal(["AC/DC"], set.ArtistList);
    }

    [Fact]
    public void Read_TakesTheDurationFromTheFileProperties()
    {
        string path = WriteFile(MediaFileFixture.CreateWav(32000), "media.wav");

        MediaTagSet set = _reader.Read(path);

        Assert.NotNull(set.Duration);
        Assert.Equal(2.0, set.Duration.Value.TotalSeconds, 0.05);
    }

    [Fact]
    public void Read_FlacDuration_ComesFromTheStreamInfo()
    {
        string path = WriteFile(MediaFileFixture.CreateFlac(), "media.flac");

        MediaTagSet set = _reader.Read(path);

        Assert.NotNull(set.Duration);
        Assert.Equal(2.0, set.Duration.Value.TotalSeconds, 0.05);
    }

    [Fact]
    public void Read_AFileWithNoTags_ReturnsAnEmptySetWithItsDuration()
    {
        string path = WriteFile(MediaFileFixture.CreateWav(32000), "media.wav");

        MediaTagSet set = _reader.Read(path);

        AssertEmpty(set);
        Assert.NotNull(set.Duration);
    }

    [Theory]
    [InlineData("media.mp3")]
    [InlineData("media.flac")]
    [InlineData("media.wav")]
    public void Read_ACorruptFile_ReturnsAnEmptySetAndDoesNotThrow(string name)
    {
        string path = WriteFile([1, 2, 3, 4, 5, 6, 7, 8, 9, 10], name);

        MediaTagSet set = _reader.Read(path);

        AssertEmpty(set);
        Assert.Null(set.Duration);
    }

    [Fact]
    public void Read_ANonAudioFile_ReturnsAnEmptySetAndDoesNotThrow()
    {
        string path = WriteFile("just text"u8.ToArray(), "notes.txt");

        MediaTagSet set = _reader.Read(path);

        AssertEmpty(set);
    }

    [Fact]
    public void Read_AMissingFile_ReturnsAnEmptySetAndDoesNotThrow()
    {
        MediaTagSet set = _reader.Read(Path.Combine(_directory, "missing.mp3"));

        AssertEmpty(set);
    }

    [Fact]
    public void Read_AFailure_IsLoggedAsAWarningWithoutTheFileContents()
    {
        string path = WriteFile([1, 2, 3, 4, 5, 6, 7, 8, 9, 10], "secret-title.mp3");

        _reader.Read(path);

        (LogLevel Level, string Message) entry = Assert.Single(_logger.EntryList);
        Assert.Equal(LogLevel.Warning, entry.Level);
        Assert.DoesNotContain("secret-title", entry.Message);
    }

    [Fact]
    public void Read_AVeryLongValue_IsReturnedAsIs()
    {
        string path = WriteFile(MediaFileFixture.CreateMp3(), "media.mp3");
        string longName = new('x', 5000);
        Tag(path, tag => tag.Performers = [longName]);

        MediaTagSet set = _reader.Read(path);

        Assert.Equal([longName], set.ArtistList);
    }

    [Fact]
    public void Read_ZeroNumbers_AreNotSet()
    {
        string path = WriteFile(MediaFileFixture.CreateMp3(), "media.mp3");
        Tag(path, tag => tag.Title = "Only a title");

        MediaTagSet set = _reader.Read(path);

        Assert.Equal("Only a title", set.Title);
        Assert.Null(set.TrackNumber);
        Assert.Null(set.DiscNumber);
        Assert.Null(set.Year);
    }

    private static Picture MakePicture(byte[] bytes, PictureType type)
    {
        return new(new ByteVector(bytes)) { Type = type, MimeType = "image/jpeg" };
    }

    [Fact]
    public void Read_ReturnsTheFrontCover_WhenThereAreSeveralPictures()
    {
        string path = WriteFile(MediaFileFixture.CreateMp3(), "media.mp3");
        Tag(path, tag => tag.Pictures = [MakePicture([1, 1, 1], PictureType.BackCover), MakePicture([2, 2, 2], PictureType.FrontCover), MakePicture([3, 3, 3], PictureType.Artist)]);

        MediaTagSet set = _reader.Read(path);

        Assert.Equal<byte[]?>([2, 2, 2], set.Picture);
    }

    [Fact]
    public void Read_ReturnsTheFirstPicture_WhenNoneIsMarkedFrontCover()
    {
        string path = WriteFile(MediaFileFixture.CreateMp3(), "media.mp3");
        Tag(path, tag => tag.Pictures = [MakePicture([1, 1, 1], PictureType.BackCover), MakePicture([2, 2, 2], PictureType.Artist)]);

        MediaTagSet set = _reader.Read(path);

        Assert.Equal<byte[]?>([1, 1, 1], set.Picture);
    }

    [Fact]
    public void Read_ReturnsTheFrontCover_OfAFlacFile()
    {
        string path = WriteFile(MediaFileFixture.CreateFlac(), "media.flac");
        Tag(path, tag => tag.Pictures = [MakePicture([4, 4], PictureType.FrontCover)]);

        MediaTagSet set = _reader.Read(path);

        Assert.Equal<byte[]?>([4, 4], set.Picture);
    }

    [Fact]
    public void Read_ReturnsNoPicture_WhenTheFileHasNone()
    {
        string path = WriteFile(MediaFileFixture.CreateMp3(), "media.mp3");
        Tag(path, tag => tag.Title = "Only a title");

        MediaTagSet set = _reader.Read(path);

        Assert.Null(set.Picture);
    }

    [Fact]
    public void Read_StillReadsTheTags_WhenThePictureIsEmpty()
    {
        string path = WriteFile(MediaFileFixture.CreateMp3(), "media.mp3");
        Tag(path, tag =>
        {
            tag.Title = "Title";
            tag.Pictures = [new Picture(new ByteVector([])) { Type = PictureType.FrontCover }];
        });

        MediaTagSet set = _reader.Read(path);

        Assert.Equal("Title", set.Title);
        Assert.Null(set.Picture);
    }

    [Fact]
    public void Read_AFileThatFails_HasNoPicture()
    {
        Assert.Null(_reader.Read(Path.Combine(_directory, "missing.mp3")).Picture);
    }
}
