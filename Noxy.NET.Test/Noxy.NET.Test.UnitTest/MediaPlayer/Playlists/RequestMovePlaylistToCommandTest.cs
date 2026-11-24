using Noxy.NET.MediaPlayer.Application.Commands.Playlist;
using Noxy.NET.MediaPlayer.Application.Requests.Playlist;

namespace Noxy.NET.Test.MediaPlayer.Playlists;

public class RequestMovePlaylistToCommandTest
{
    [Fact]
    public void ToCommand_TakesTheUserAndThePlaylistFromTheirParametersAndThePositionFromTheRequest()
    {
        Guid userID = Guid.CreateVersion7();
        Guid id = Guid.CreateVersion7();

        CommandPlaylistMove command = new RequestMovePlaylist { Position = 3 }.ToCommand(userID, id);

        Assert.Equal(userID, command.UserID);
        Assert.Equal(id, command.ID);
        Assert.Equal(3, command.Position);
    }
}
