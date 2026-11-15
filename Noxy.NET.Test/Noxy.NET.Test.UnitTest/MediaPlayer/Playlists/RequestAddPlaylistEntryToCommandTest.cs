using Noxy.NET.MediaPlayer.Application.Commands.Playlist;
using Noxy.NET.MediaPlayer.Application.Requests.Playlist;

namespace Noxy.NET.Test.MediaPlayer.Playlists;

public class RequestAddPlaylistEntryToCommandTest
{
    private static readonly Guid UserID = Guid.CreateVersion7();
    private static readonly Guid ID = Guid.CreateVersion7();

    [Fact]
    public void ToCommand_CarriesTheUserTheIdTheListAndThePosition()
    {
        List<Guid> mediaFileIDList = [Guid.CreateVersion7(), Guid.CreateVersion7()];

        CommandPlaylistEntryListAdd command = new RequestAddPlaylistEntry { MediaFileIDList = mediaFileIDList, Position = 3 }.ToCommand(UserID, ID);

        Assert.Equal(UserID, command.UserID);
        Assert.Equal(ID, command.ID);
        Assert.Equal(mediaFileIDList, command.MediaFileIDList);
        Assert.Equal(3, command.Position);
    }

    [Fact]
    public void ToCommand_KeepsALeftOutPositionNull()
    {
        CommandPlaylistEntryListAdd command = new RequestAddPlaylistEntry { MediaFileIDList = [Guid.CreateVersion7()] }.ToCommand(UserID, ID);

        Assert.Null(command.Position);
    }
}
