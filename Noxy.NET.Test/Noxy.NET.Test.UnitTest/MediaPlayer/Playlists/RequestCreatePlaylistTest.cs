using System.ComponentModel.DataAnnotations;
using Noxy.NET.MediaPlayer.Application.Requests.Playlist;

namespace Noxy.NET.Test.MediaPlayer.Playlists;

public class RequestCreatePlaylistTest
{
    private static List<ValidationResult> Validate(RequestCreatePlaylist request)
    {
        List<ValidationResult> results = [];
        Validator.TryValidateObject(request, new(request), results, true);
        return results;
    }

    [Fact]
    public void ANamedRequest_IsValid_AndAllowsDuplicatesByDefault()
    {
        RequestCreatePlaylist request = new() { Name = "Road trip" };

        Assert.Empty(Validate(request));
        Assert.True(request.AllowDuplicates);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void ABlankOrMissingName_IsInvalid(string? name)
    {
        Assert.NotEmpty(Validate(new() { Name = name! }));
    }

    [Theory]
    [InlineData(255, true)]
    [InlineData(256, false)]
    public void TheNameIsLimitedTo255Characters(int length, bool valid)
    {
        Assert.Equal(valid, Validate(new() { Name = new('a', length) }).Count == 0);
    }

    [Fact]
    public void WithNoArtHue_IsValid_AndTheHueIsNull()
    {
        RequestCreatePlaylist request = new() { Name = "Road trip" };

        Assert.Empty(Validate(request));
        Assert.Null(request.ArtHue);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(200)]
    [InlineData(359)]
    public void AnArtHueWithinTheWheel_IsValid(int hue)
    {
        Assert.Empty(Validate(new() { Name = "Mix", ArtHue = hue }));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(360)]
    public void AnArtHueOutsideTheWheel_IsInvalid(int hue)
    {
        Assert.NotEmpty(Validate(new() { Name = "Mix", ArtHue = hue }));
    }
}
