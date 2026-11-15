using System.Text.Json;
using Noxy.NET.MediaPlayer.Domain.Requests.Media;

namespace Noxy.NET.Test.MediaPlayer.Media;

public class OptionalJsonTest
{
    private static RequestUpdateMedia Read(string json)
    {
        return JsonSerializer.Deserialize<RequestUpdateMedia>(json, JsonSerializerOptions.Web)!;
    }

    [Fact]
    public void Absent_IsLeftOut()
    {
        RequestUpdateMedia request = Read("{}");

        Assert.False(request.Title.HasValue);
        Assert.False(request.Artist.HasValue);
        Assert.False(request.Year.HasValue);
    }

    [Fact]
    public void Null_IsPresentAndNull()
    {
        RequestUpdateMedia request = Read("""{"artist":null,"year":null}""");

        Assert.True(request.Artist.HasValue);
        Assert.Null(request.Artist.Value);
        Assert.True(request.Year.HasValue);
        Assert.Null(request.Year.Value);
        Assert.False(request.Album.HasValue);
    }

    [Fact]
    public void Value_IsPresent()
    {
        RequestUpdateMedia request = Read("""{"title":"Media","albumArtist":"Someone","trackNumber":3,"year":1999}""");

        Assert.Equal("Media", request.Title.Value);
        Assert.Equal("Someone", request.AlbumArtist.Value);
        Assert.Equal(3, request.TrackNumber.Value);
        Assert.Equal(1999, request.Year.Value);
    }

    [Theory]
    [InlineData("""{"title":5}""")]
    [InlineData("""{"artist":true}""")]
    [InlineData("""{"year":"abc"}""")]
    [InlineData("""{"year":1.5}""")]
    [InlineData("""{"trackNumber":[1]}""")]
    public void WrongType_ThrowsJsonException(string json)
    {
        Assert.ThrowsAny<JsonException>(() => Read(json));
    }

    [Theory]
    [InlineData("[]")]
    [InlineData("\"x\"")]
    [InlineData("5")]
    public void NotAnObject_ThrowsJsonException(string json)
    {
        Assert.ThrowsAny<JsonException>(() => Read(json));
    }
}
