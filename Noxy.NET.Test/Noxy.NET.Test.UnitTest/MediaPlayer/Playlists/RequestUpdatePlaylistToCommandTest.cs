using Noxy.NET.MediaPlayer.Application.Commands.Playlist;
using Noxy.NET.MediaPlayer.Application.Requests.Playlist;

namespace Noxy.NET.Test.MediaPlayer.Playlists;

public class RequestUpdatePlaylistToCommandTest
{
    private static readonly Guid UserID = Guid.CreateVersion7();
    private static readonly Guid ID = Guid.CreateVersion7();

    [Fact]
    public void ToCommand_CarriesTheUserAndTheId()
    {
        CommandPlaylistUpdate command = new RequestUpdatePlaylist().ToCommand(UserID, ID);

        Assert.Equal(UserID, command.UserID);
        Assert.Equal(ID, command.ID);
    }

    [Fact]
    public void ToCommand_KeepsAFieldThatWasLeftOutAbsent()
    {
        CommandPlaylistUpdate command = new RequestUpdatePlaylist { Name = new("Mix") }.ToCommand(UserID, ID);

        Assert.True(command.Name.HasValue);
        Assert.False(command.AllowDuplicates.HasValue);
    }

    [Fact]
    public void ToCommand_KeepsANullNamePresentWithNull()
    {
        CommandPlaylistUpdate command = new RequestUpdatePlaylist { Name = new(null) }.ToCommand(UserID, ID);

        Assert.True(command.Name.HasValue);
        Assert.Null(command.Name.Value);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void ToCommand_CarriesTheFlagAsSent(bool flag)
    {
        CommandPlaylistUpdate command = new RequestUpdatePlaylist { AllowDuplicates = new(flag) }.ToCommand(UserID, ID);

        Assert.True(command.AllowDuplicates.HasValue);
        Assert.Equal(flag, command.AllowDuplicates.Value);
        Assert.False(command.Name.HasValue);
    }

    [Fact]
    public void ToCommand_CarriesTheName()
    {
        CommandPlaylistUpdate command = new RequestUpdatePlaylist { Name = new("Road trip") }.ToCommand(UserID, ID);

        Assert.Equal("Road trip", command.Name.Value);
    }

    [Fact]
    public void ToCommand_CarriesTheArtHueAsSent_AndKeepsItAbsentWhenLeftOut()
    {
        CommandPlaylistUpdate chosen = new RequestUpdatePlaylist { ArtHue = new(200) }.ToCommand(UserID, ID);
        CommandPlaylistUpdate cleared = new RequestUpdatePlaylist { ArtHue = new(null) }.ToCommand(UserID, ID);
        CommandPlaylistUpdate untouched = new RequestUpdatePlaylist { Name = new("Mix") }.ToCommand(UserID, ID);

        Assert.Equal(200, chosen.ArtHue.Value);
        Assert.True(cleared.ArtHue.HasValue);
        Assert.Null(cleared.ArtHue.Value);
        Assert.False(untouched.ArtHue.HasValue);
    }
}
