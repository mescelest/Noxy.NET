using Noxy.NET.MediaPlayer.Application.Commands.Playlist;
using Noxy.NET.MediaPlayer.Application.Requests.Playlist;

namespace Noxy.NET.Test.MediaPlayer.Playlists;

public class RequestMovePlaylistEntryToCommandTest
{
    [Fact]
    public void ToCommand_TakesTheUserThePlaylistAndTheEntryFromTheirParametersAndThePositionFromTheRequest()
    {
        Guid userID = Guid.CreateVersion7();
        Guid id = Guid.CreateVersion7();
        Guid entryID = Guid.CreateVersion7();

        CommandPlaylistEntryMove command = new RequestMovePlaylistEntry { Position = 4 }.ToCommand(userID, id, entryID);

        Assert.Equal(userID, command.UserID);
        Assert.Equal(id, command.ID);
        Assert.Equal(entryID, command.EntryID);
        Assert.Equal(4, command.Position);
    }
}
