using Noxy.NET.MediaPlayer.Application.Commands.Media;
using Noxy.NET.MediaPlayer.Application.Requests.Media;

namespace Noxy.NET.Test.MediaPlayer.Media;

public class RequestUpdateMediaToCommandTest
{
    private static readonly Guid UserID = Guid.CreateVersion7();
    private static readonly Guid ID = Guid.CreateVersion7();

    [Fact]
    public void ToCommand_CarriesTheUserAndTheIdFromTheirParameters()
    {
        CommandMediaUpdate command = new RequestUpdateMedia().ToCommand(UserID, ID);

        Assert.Equal(UserID, command.UserID);
        Assert.Equal(ID, command.ID);
    }

    [Fact]
    public void ToCommand_KeepsAFieldThatWasLeftOutAbsent()
    {
        CommandMediaUpdate command = new RequestUpdateMedia { Artist = new("Someone") }.ToCommand(UserID, ID);

        Assert.True(command.Artist.HasValue);
        Assert.False(command.Title.HasValue);
        Assert.False(command.Album.HasValue);
        Assert.False(command.Year.HasValue);
    }

    [Fact]
    public void ToCommand_KeepsANullFieldPresentWithNull()
    {
        CommandMediaUpdate command = new RequestUpdateMedia { Artist = new(null), Year = new(null) }.ToCommand(UserID, ID);

        Assert.True(command.Artist.HasValue);
        Assert.Null(command.Artist.Value);
        Assert.True(command.Year.HasValue);
        Assert.Null(command.Year.Value);
    }

    [Fact]
    public void ToCommand_KeepsABlankFieldPresentWithTheBlank()
    {
        CommandMediaUpdate command = new RequestUpdateMedia { Genre = new("   ") }.ToCommand(UserID, ID);

        Assert.True(command.Genre.HasValue);
        Assert.Equal("   ", command.Genre.Value);
    }

    [Fact]
    public void ToCommand_CarriesEveryFieldAndNumber()
    {
        CommandMediaUpdate command = new RequestUpdateMedia
        {
            Title = new("T"),
            Artist = new("Ar"),
            Album = new("Al"),
            AlbumArtist = new("AA"),
            Subtitle = new("S"),
            Comment = new("C"),
            Genre = new("G"),
            TrackNumber = new(4),
            DiscNumber = new(2),
            Year = new(2024)
        }.ToCommand(UserID, ID);

        Assert.Equal(["T", "Ar", "Al", "AA", "S", "C", "G"], [command.Title.Value, command.Artist.Value, command.Album.Value, command.AlbumArtist.Value, command.Subtitle.Value, command.Comment.Value, command.Genre.Value]);
        Assert.Equal([4, 2, 2024], [command.TrackNumber.Value, command.DiscNumber.Value, command.Year.Value]);
        Assert.All(
        [
            command.Title.HasValue, command.Artist.HasValue, command.Album.HasValue, command.AlbumArtist.HasValue, command.Subtitle.HasValue, command.Comment.HasValue, command.Genre.HasValue, command.TrackNumber.HasValue, command.DiscNumber.HasValue,
            command.Year.HasValue
        ], Assert.True);
    }
}
