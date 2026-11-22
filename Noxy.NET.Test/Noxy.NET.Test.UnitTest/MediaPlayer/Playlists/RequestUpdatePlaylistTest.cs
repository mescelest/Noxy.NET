using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Noxy.NET.MediaPlayer.Application.Requests.Playlist;

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
        return JsonSerializer.Deserialize<RequestUpdatePlaylist>(json, JsonSerializerOptions.Web) ?? throw new InvalidOperationException("The json is not an update playlist request.");
    }

    [Fact]
    public void Write_OmitsAFieldThatWasLeftOut_AndKeepsOneThatWasSent_EvenWhenItIsNullOrFalse()
    {
        Assert.Equal("{}", JsonSerializer.Serialize(new RequestUpdatePlaylist(), JsonSerializerOptions.Web));
        Assert.Equal("""{"name":"Mix"}""", JsonSerializer.Serialize(new RequestUpdatePlaylist { Name = new("Mix") }, JsonSerializerOptions.Web));
        Assert.Equal("""{"name":null}""", JsonSerializer.Serialize(new RequestUpdatePlaylist { Name = new(null) }, JsonSerializerOptions.Web));
        Assert.Equal("""{"allowDuplicates":false}""", JsonSerializer.Serialize(new RequestUpdatePlaylist { AllowDuplicates = new(false) }, JsonSerializerOptions.Web));
    }

    [Fact]
    public void Write_ThenRead_GivesTheSameRequest()
    {
        RequestUpdatePlaylist sent = new() { AllowDuplicates = new(false) };

        RequestUpdatePlaylist received = Read(JsonSerializer.Serialize(sent, JsonSerializerOptions.Web));

        Assert.False(received.Name.HasValue);
        Assert.True(received.AllowDuplicates.HasValue);
        Assert.False(received.AllowDuplicates.Value);
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
        Assert.NotEmpty(Validate(new() { Name = new(new('a', 256)) }));
    }

    [Theory]
    [InlineData("""{"allowDuplicates":null}""")]
    [InlineData("""{"allowDuplicates":"yes"}""")]
    public void AFlagThatIsNotABoolean_IsRejectedWhileReading(string json)
    {
        Assert.Throws<JsonException>(() => Read(json));
    }
}
