using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Noxy.NET.MediaPlayer.Domain.Requests.Playlist;

namespace Noxy.NET.Test.MediaPlayer.Playlists;

public class RequestUpdatePlaylistTest
{
    private static List<ValidationResult> Validate(RequestUpdatePlaylist request)
    {
        List<ValidationResult> results = [];
        Validator.TryValidateObject(request, new(request), results, true);
        return results;
    }

    private static RequestUpdatePlaylist Read(string json)
    {
        return JsonSerializer.Deserialize<RequestUpdatePlaylist>(json, JsonSerializerOptions.Web)!;
    }

    [Fact]
    public void Empty_IsValid_AndNothingIsPresent()
    {
        RequestUpdatePlaylist request = Read("{}");

        Assert.Empty(Validate(request));
        Assert.False(request.Name.HasValue);
        Assert.False(request.AllowDuplicates.HasValue);
    }

    [Fact]
    public void SentFields_ArePresent()
    {
        RequestUpdatePlaylist request = Read("""{"name":"Mix","allowDuplicates":false}""");

        Assert.Equal("Mix", request.Name.Value);
        Assert.True(request.AllowDuplicates.HasValue);
        Assert.False(request.AllowDuplicates.Value);
        Assert.Empty(Validate(request));
    }

    [Theory]
    [InlineData("""{"name":null}""")]
    [InlineData("""{"name":" "}""")]
    public void ABlankOrNullName_IsInvalid(string json)
    {
        Assert.NotEmpty(Validate(Read(json)));
    }

    [Fact]
    public void ANameOver255Characters_IsInvalid()
    {
        Assert.NotEmpty(Validate(new() { Name = new(new string('a', 256)) }));
    }

    [Theory]
    [InlineData("""{"allowDuplicates":null}""")]
    [InlineData("""{"allowDuplicates":"yes"}""")]
    public void AFlagThatIsNotABoolean_IsRejectedWhileReading(string json)
    {
        Assert.Throws<JsonException>(() => Read(json));
    }
}
