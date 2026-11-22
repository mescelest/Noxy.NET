using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Noxy.NET.MediaPlayer.Application.Commands.Metadata;
using Noxy.NET.MediaPlayer.Application.Requests.Metadata;

namespace Noxy.NET.Test.MediaPlayer.Metadata;

public class RequestUpdateAlbumTest
{
    private static readonly Guid UserID = Guid.CreateVersion7();
    private static readonly Guid ID = Guid.CreateVersion7();

    private static List<ValidationResult> Validate(RequestUpdateAlbum request)
    {
        List<ValidationResult> results = [];
        Validator.TryValidateObject(request, new(request), results, true);
        return results;
    }

    [Fact]
    public void Write_OmitsEveryFieldThatWasLeftOut_AndKeepsOnesThatWereSent()
    {
        Assert.Equal("{}", JsonSerializer.Serialize(new RequestUpdateAlbum(), JsonSerializerOptions.Web));
        Assert.Equal("""{"title":"Album"}""", JsonSerializer.Serialize(new RequestUpdateAlbum { Title = new("Album") }, JsonSerializerOptions.Web));
    }

    [Fact]
    public void Empty_IsValid()
    {
        Assert.Empty(Validate(new()));
    }

    [Fact]
    public void ANullArtistList_IsValid()
    {
        Assert.Empty(Validate(new() { ArtistIDList = new(null) }));
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public void ABlankTitle_IsInvalid(string title)
    {
        Assert.Contains("Title", Assert.Single(Validate(new() { Title = new(title) })).MemberNames);
    }

    [Fact]
    public void ANullTitle_IsInvalid()
    {
        Assert.Single(Validate(new() { Title = new(null) }));
    }

    [Fact]
    public void ATitleOver255Characters_IsInvalid()
    {
        Assert.Single(Validate(new() { Title = new(new('a', 256)) }));
    }

    [Fact]
    public void TooManyArtists_IsInvalid()
    {
        List<Guid> idList = [.. Enumerable.Range(0, RequestCreateAlbum.MaxArtistCount + 1).Select(_ => Guid.CreateVersion7())];

        Assert.Contains("ArtistIDList", Assert.Single(Validate(new() { ArtistIDList = new(idList) })).MemberNames);
    }

    [Fact]
    public void ToCommand_KeepsAFieldThatWasLeftOutAbsentAndCarriesTheOnesSent()
    {
        Guid artistID = Guid.CreateVersion7();

        CommandAlbumUpdate onlyTitle = new RequestUpdateAlbum { Title = new("Record") }.ToCommand(UserID, ID);
        CommandAlbumUpdate onlyArtists = new RequestUpdateAlbum { ArtistIDList = new([artistID]) }.ToCommand(UserID, ID);

        Assert.Equal(UserID, onlyTitle.UserID);
        Assert.Equal(ID, onlyTitle.ID);
        Assert.Equal("Record", onlyTitle.Title.Value);
        Assert.False(onlyTitle.ArtistIDList.HasValue);
        Assert.False(onlyArtists.Title.HasValue);
        Assert.Equal([artistID], onlyArtists.ArtistIDList.Value);
    }
}
