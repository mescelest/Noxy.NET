using System.ComponentModel.DataAnnotations;
using Noxy.NET.MediaPlayer.Application.Commands.Metadata;
using Noxy.NET.MediaPlayer.Application.Requests.Metadata;

namespace Noxy.NET.Test.MediaPlayer.Metadata;

public class RequestCreateArtistTest
{
    private static readonly Guid UserID = Guid.CreateVersion7();

    private static List<ValidationResult> Validate(RequestCreateArtist request)
    {
        List<ValidationResult> results = [];
        Validator.TryValidateObject(request, new(request), results, true);
        return results;
    }

    [Fact]
    public void ANameUpTo255Characters_IsValid()
    {
        Assert.Empty(Validate(new() { Name = new('a', 255) }));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void ABlankName_IsInvalid(string name)
    {
        Assert.Contains("Name", Assert.Single(Validate(new() { Name = name })).MemberNames);
    }

    [Fact]
    public void ANameOver255Characters_IsInvalid()
    {
        Assert.Single(Validate(new() { Name = new('a', 256) }));
    }

    [Fact]
    public void ToCommand_CarriesTheUserAndTheName()
    {
        CommandArtistCreate command = new RequestCreateArtist { Name = "Name" }.ToCommand(UserID);

        Assert.Equal(UserID, command.UserID);
        Assert.Equal("Name", command.Name);
    }
}
