using System.ComponentModel.DataAnnotations;
using Noxy.NET.MediaPlayer.Application.Commands.Metadata;
using Noxy.NET.MediaPlayer.Application.Requests.Metadata;

namespace Noxy.NET.Test.MediaPlayer.Metadata;

public class RequestUpdateGenreTest
{
    private static readonly Guid UserID = Guid.CreateVersion7();
    private static readonly Guid ID = Guid.CreateVersion7();

    private static List<ValidationResult> Validate(RequestUpdateGenre request)
    {
        List<ValidationResult> results = [];
        Validator.TryValidateObject(request, new(request), results, true);
        return results;
    }

    [Fact]
    public void ANameUpTo255Characters_IsValid()
    {
        Assert.Empty(Validate(new() { Name = new string('a', 255) }));
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
        Assert.Single(Validate(new() { Name = new string('a', 256) }));
    }

    [Fact]
    public void ToCommand_CarriesTheUserTheIdAndTheName()
    {
        CommandGenreRename command = new RequestUpdateGenre { Name = "Name" }.ToCommand(UserID, ID);

        Assert.Equal(UserID, command.UserID);
        Assert.Equal(ID, command.ID);
        Assert.Equal("Name", command.Name);
    }
}
