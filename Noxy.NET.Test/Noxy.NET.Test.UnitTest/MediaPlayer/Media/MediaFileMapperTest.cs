using Noxy.NET.MediaPlayer.Application.Services;
using Noxy.NET.MediaPlayer.Domain.Entities;
using Noxy.NET.MediaPlayer.Domain.Responses.Media;

namespace Noxy.NET.Test.MediaPlayer.Media;

public class MediaFileMapperTest
{
    private static MediaFile CreateFile(DateTimeKind kind = DateTimeKind.Unspecified)
    {
        MediaFile file = new()
        {
            TimeCreated = new(2026, 1, 2, 3, 4, 5, kind),
            TimeModified = new(2026, 2, 3, 4, 5, 6, kind),
            FilePath = "stored.mp3",
            ContentType = "audio/mpeg",
            SizeBytes = 1234,
            ContentHash = new string('a', 64),
            UserID = Guid.CreateVersion7()
        };
        file.Metadata = new() { Title = "Song", MediaFileID = file.ID };
        return file;
    }

    [Fact]
    public void ToResponse_CopiesEveryTagFromTheMetadata()
    {
        MediaFile file = CreateFile();
        file.Metadata!.Artist = "Artist";
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

        Assert.Equal("Song", response.Title);
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
        MediaFile file = CreateFile();
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
}
