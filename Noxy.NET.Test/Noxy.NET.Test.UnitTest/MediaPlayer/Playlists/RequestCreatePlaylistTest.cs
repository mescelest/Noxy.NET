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
        Assert.Equal(valid, Validate(new() { Name = new string('a', length) }).Count == 0);
    }
}
