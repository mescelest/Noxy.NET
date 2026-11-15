using Noxy.NET.MediaPlayer.Application.Commands.Playlist;
using Noxy.NET.MediaPlayer.Application.Requests.Playlist;

namespace Noxy.NET.Test.MediaPlayer.Playlists;

public class RequestRemovePlaylistEntryToCommandTest
{
    [Fact]
    public void ToCommand_CarriesTheUserTheIdAndTheEntryIDList()
    {
        Guid userID = Guid.CreateVersion7();
        Guid id = Guid.CreateVersion7();
        List<Guid> entryIDList = [Guid.CreateVersion7(), Guid.CreateVersion7()];

        CommandPlaylistEntryListRemove command = new RequestRemovePlaylistEntry { EntryIDList = entryIDList }.ToCommand(userID, id);

        Assert.Equal(userID, command.UserID);
        Assert.Equal(id, command.ID);
        Assert.Equal(entryIDList, command.EntryIDList);
    }
}
