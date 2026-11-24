using Noxy.NET.MediaPlayer.Application.Commands.Playlist;
using Noxy.NET.MediaPlayer.Application.Requests.Playlist;

namespace Noxy.NET.Test.MediaPlayer.Playlists;

public class RequestCreatePlaylistToCommandTest
{
    private static readonly Guid UserID = Guid.CreateVersion7();

    [Fact]
    public void ToCommand_CarriesTheUserTheNameAndTheFlag()
    {
        CommandPlaylistCreate command = new RequestCreatePlaylist { Name = "Road trip", AllowDuplicates = false }.ToCommand(UserID);

        Assert.Equal(UserID, command.UserID);
        Assert.Equal("Road trip", command.Name);
        Assert.False(command.AllowDuplicates);
    }

    [Fact]
    public void ToCommand_CarriesTheArtHue_ThatMayBeNull()
    {
        Assert.Equal(200, new RequestCreatePlaylist { Name = "Mix", ArtHue = 200 }.ToCommand(UserID).ArtHue);
        Assert.Null(new RequestCreatePlaylist { Name = "Mix" }.ToCommand(UserID).ArtHue);
    }
}
