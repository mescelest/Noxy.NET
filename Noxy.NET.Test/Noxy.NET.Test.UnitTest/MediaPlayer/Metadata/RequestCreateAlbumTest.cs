using System.ComponentModel.DataAnnotations;
using Noxy.NET.MediaPlayer.Application.Commands.Metadata;
using Noxy.NET.MediaPlayer.Application.Requests.Metadata;

namespace Noxy.NET.Test.MediaPlayer.Metadata;

public class RequestCreateAlbumTest
{
    private static readonly Guid UserID = Guid.CreateVersion7();

    private static List<ValidationResult> Validate(RequestCreateAlbum request)
    {
        List<ValidationResult> results = [];
        Validator.TryValidateObject(request, new(request), results, true);
        return results;
    }

    [Fact]
    public void ATitleAndNoArtists_IsValid()
    {
        Assert.Empty(Validate(new() { Title = "Record" }));
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public void ABlankTitle_IsInvalid(string title)
    {
        Assert.Contains("Title", Assert.Single(Validate(new() { Title = title })).MemberNames);
    }

    [Fact]
    public void ATitleOver255Characters_IsInvalid()
    {
        Assert.Single(Validate(new() { Title = new string('a', 256) }));
    }

    [Fact]
    public void TooManyArtists_IsInvalid()
    {
        List<Guid> idList = [.. Enumerable.Range(0, RequestCreateAlbum.MaxArtistCount + 1).Select(_ => Guid.CreateVersion7())];

        Assert.Contains("ArtistIDList", Assert.Single(Validate(new() { Title = "Record", ArtistIDList = idList })).MemberNames);
    }

    [Fact]
    public void ToCommand_CarriesTheUserTheTitleAndTheArtists()
    {
        Guid artistID = Guid.CreateVersion7();

        CommandAlbumCreate command = new RequestCreateAlbum { Title = "Record", ArtistIDList = [artistID] }.ToCommand(UserID);

        Assert.Equal(UserID, command.UserID);
        Assert.Equal("Record", command.Title);
        Assert.Equal([artistID], command.ArtistIDList);
    }
}
