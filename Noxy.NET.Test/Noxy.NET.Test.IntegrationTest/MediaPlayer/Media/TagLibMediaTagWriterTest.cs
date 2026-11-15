using Noxy.NET.MediaPlayer.Application.Writers;
using Noxy.NET.MediaPlayer.Domain.Entities;
using Noxy.NET.Test.MediaPlayer.Fixtures;

namespace Noxy.NET.Test.MediaPlayer.Media;

public class TagLibMediaTagWriterTest : IDisposable
{
    private readonly string _directory = Path.Combine(Path.GetTempPath(), $"tagwriter-{Guid.CreateVersion7()}");
    private readonly TagLibMediaTagWriter _writer = new();

    public TagLibMediaTagWriterTest()
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

    private static EntityMediaFileMetadata Full()
    {
        return new()
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
            MediaFileID = Guid.CreateVersion7()
        };
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

        _writer.Write(path, new() { Title = "Only", MediaFileID = Guid.CreateVersion7() });

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
