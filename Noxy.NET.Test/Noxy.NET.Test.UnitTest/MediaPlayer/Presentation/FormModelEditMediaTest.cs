using System.ComponentModel.DataAnnotations;
using Noxy.NET.MediaPlayer.Domain.Responses.Media;
using Noxy.NET.MediaPlayer.Presentation.Models.FormModel;

namespace Noxy.NET.Test.MediaPlayer.Presentation;

public class FormModelEditMediaTest
{
    private static bool IsValid(FormModelEditMedia model)
    {
        return Validator.TryValidateObject(model, new(model), [], true);
    }

    [Fact]
    public void Title_OfOnlySpaces_IsInvalid()
    {
        Assert.False(IsValid(new() { Title = "   " }));
    }

    [Fact]
    public void Title_Of255Characters_IsValid_And256IsNot()
    {
        Assert.True(IsValid(new() { Title = new('a', 255) }));
        Assert.False(IsValid(new() { Title = new('a', 256) }));
    }

    [Fact]
    public void Artist_Album_AlbumArtist_Subtitle_AreLimitedTo256()
    {
        string text = new('a', 257);

        Assert.False(IsValid(new() { Title = "T", Artist = text }));
        Assert.False(IsValid(new() { Title = "T", Album = text }));
        Assert.False(IsValid(new() { Title = "T", AlbumArtist = text }));
        Assert.False(IsValid(new() { Title = "T", Subtitle = text }));
        Assert.True(IsValid(new() { Title = "T", Artist = new('a', 256) }));
    }

    [Fact]
    public void Genre_Of128_IsValid_And129IsNot()
    {
        Assert.True(IsValid(new() { Title = "T", Genre = new('a', 128) }));
        Assert.False(IsValid(new() { Title = "T", Genre = new('a', 129) }));
    }

    [Fact]
    public void Comment_Of2048_IsValid_And2049IsNot()
    {
        Assert.True(IsValid(new() { Title = "T", Comment = new('a', 2048) }));
        Assert.False(IsValid(new() { Title = "T", Comment = new('a', 2049) }));
    }

    [Fact]
    public void Year_Track_Disc_AreBetween0And9999_OrEmpty()
    {
        Assert.True(IsValid(new() { Title = "T", Year = null, TrackNumber = 0, DiscNumber = 9999 }));
        Assert.False(IsValid(new() { Title = "T", Year = 10000 }));
        Assert.False(IsValid(new() { Title = "T", TrackNumber = -1 }));
        Assert.False(IsValid(new() { Title = "T", DiscNumber = 10000 }));
    }

    [Fact]
    public void From_CopiesTheTagsOfTheMediaFile()
    {
        ResponseMediaFile media = new()
        {
            ID = Guid.CreateVersion7(), Title = "Title", ContentType = "audio/mpeg", SizeBytes = 1, TimeCreated = DateTime.UtcNow, TimeModified = DateTime.UtcNow,
            Artist = "Artist", Album = "Album", AlbumArtist = "Album artist", Subtitle = "Sub", Comment = "Note", Genre = "Rock", TrackNumber = 3, DiscNumber = 1, Year = 2021
        };

        FormModelEditMedia model = FormModelEditMedia.From(media);

        Assert.Equal(("Title", "Artist", "Album", "Album artist", "Sub", "Note", "Rock", 3, 1, 2021), (model.Title, model.Artist, model.Album, model.AlbumArtist, model.Subtitle, model.Comment, model.Genre, model.TrackNumber, model.DiscNumber, model.Year));
    }
}
