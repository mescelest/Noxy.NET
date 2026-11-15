using System.ComponentModel.DataAnnotations;
using Noxy.NET.MediaPlayer.Application.Commands.Playback;
using Noxy.NET.MediaPlayer.Application.Requests.Playback;

namespace Noxy.NET.Test.MediaPlayer.Playback;

public class RequestSavePlaybackSessionTest
{
    private static List<ValidationResult> Validate(object request)
    {
        List<ValidationResult> results = [];
        Validator.TryValidateObject(request, new(request), results, true);
        return results;
    }

    [Fact]
    public void ToCommand_CarriesTheUserTheQueueTheIndexThePositionAndThePlaylist()
    {
        Guid userID = Guid.CreateVersion7();
        Guid playlistID = Guid.CreateVersion7();
        List<Guid> queue = [Guid.CreateVersion7(), Guid.CreateVersion7()];

        CommandPlaybackSessionSave command = new RequestSavePlaybackSession { QueueMediaFileIDList = queue, CurrentIndex = 1, PositionSeconds = 4.5, PlaylistID = playlistID }.ToCommand(userID);

        Assert.Equal(userID, command.UserID);
        Assert.Equal(queue, command.QueueMediaFileIDList);
        Assert.Equal(1, command.CurrentIndex);
        Assert.Equal(4.5, command.PositionSeconds);
        Assert.Equal(playlistID, command.PlaylistID);
    }

    [Fact]
    public void ToCommand_KeepsALeftOutIndexAndPlaylistNull()
    {
        CommandPlaybackSessionSave command = new RequestSavePlaybackSession().ToCommand(Guid.CreateVersion7());

        Assert.Empty(command.QueueMediaFileIDList);
        Assert.Null(command.CurrentIndex);
        Assert.Null(command.PlaylistID);
        Assert.Equal(0, command.PositionSeconds);
    }

    [Fact]
    public void AValidRequestAndTheDefaults_AreValid()
    {
        Assert.Empty(Validate(new RequestSavePlaybackSession()));
        Assert.Empty(Validate(new RequestSavePlaybackSession { QueueMediaFileIDList = [Guid.CreateVersion7()], CurrentIndex = 0, PositionSeconds = 12 }));
    }

    [Fact]
    public void ANullQueue_IsInvalid()
    {
        // A client can send an explicit null, which the non-nullable property type does not show.
        Assert.NotEmpty(Validate(new RequestSavePlaybackSession { QueueMediaFileIDList = null! }));
    }

    [Fact]
    public void ANegativeIndexOrPosition_IsInvalid()
    {
        Assert.NotEmpty(Validate(new RequestSavePlaybackSession { CurrentIndex = -1 }));
        Assert.NotEmpty(Validate(new RequestSavePlaybackSession { PositionSeconds = -0.1 }));
    }
}
