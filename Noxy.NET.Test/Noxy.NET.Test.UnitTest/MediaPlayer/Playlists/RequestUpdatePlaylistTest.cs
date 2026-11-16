using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Noxy.NET.MediaPlayer.Application.Requests.Playlist;
using Noxy.NET.MediaPlayer.Domain.Enums;

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
        Assert.NotEmpty(Validate(new() { Name = new(new string('a', 256)) }));
    }

    [Theory]
    [InlineData("""{"allowDuplicates":null}""")]
    [InlineData("""{"allowDuplicates":"yes"}""")]
    public void AFlagThatIsNotABoolean_IsRejectedWhileReading(string json)
    {
        Assert.Throws<JsonException>(() => Read(json));
    }

    [Fact]
    public void Modes_AreLeftOutWhenNotSent_ValuesWhenSent_AndNullWhenExplicitlyNull()
    {
        Assert.Equal("{}", JsonSerializer.Serialize(new RequestUpdatePlaylist(), JsonSerializerOptions.Web));
        Assert.Equal("""{"shuffle":"Random"}""", JsonSerializer.Serialize(new RequestUpdatePlaylist { Shuffle = new(ShuffleModeEnum.Random) }, JsonSerializerOptions.Web));
        Assert.Equal("""{"repeat":null}""", JsonSerializer.Serialize(new RequestUpdatePlaylist { Repeat = new(null) }, JsonSerializerOptions.Web));

        RequestUpdatePlaylist named = Read("""{"name":"x"}""");
        Assert.False(named.Shuffle.HasValue);
        Assert.False(named.Repeat.HasValue);

        RequestUpdatePlaylist sent = Read("""{"shuffle":"SpreadArtists","repeat":null}""");
        Assert.Equal(ShuffleModeEnum.SpreadArtists, sent.Shuffle.Value);
        Assert.True(sent.Repeat.HasValue);
        Assert.Null(sent.Repeat.Value);
    }

    [Fact]
    public void ANameThatIsNotAMode_FailsToRead()
    {
        Assert.ThrowsAny<JsonException>(() => Read("""{"shuffle":"Sometimes"}"""));
        Assert.ThrowsAny<JsonException>(() => Read("""{"repeat":"Never"}"""));
    }

    [Theory]
    [InlineData("""{"shuffle":1}""")]
    [InlineData("""{"shuffle":99}""")]
    [InlineData("""{"shuffle":"1"}""")]
    [InlineData("""{"repeat":2}""")]
    [InlineData("""{"repeat":"99"}""")]
    public void AModeGivenAsANumber_FailsToRead(string json)
    {
        Assert.ThrowsAny<JsonException>(() => Read(json));
    }

    [Fact]
    public void AModeIsWrittenByName()
    {
        Assert.Equal("""{"shuffle":"SpreadArtists","repeat":"Forever"}""", JsonSerializer.Serialize(new RequestUpdatePlaylist { Shuffle = new(ShuffleModeEnum.SpreadArtists), Repeat = new(RepeatModeEnum.Forever) }, JsonSerializerOptions.Web));
    }
}
