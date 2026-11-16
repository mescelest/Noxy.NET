using Noxy.NET.MediaPlayer.Application.Commands.Playlist;
using Noxy.NET.MediaPlayer.Application.Requests.Playlist;
using Noxy.NET.MediaPlayer.Domain.Enums;

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
    public void ToCommand_CarriesTheModes_AndKeepsOnesLeftOutAbsent()
    {
        CommandPlaylistUpdate command = new RequestUpdatePlaylist { Shuffle = new(ShuffleModeEnum.Random), Repeat = new(null) }.ToCommand(UserID, ID);

        Assert.Equal(ShuffleModeEnum.Random, command.Shuffle.Value);
        Assert.True(command.Repeat.HasValue);
        Assert.Null(command.Repeat.Value);
        Assert.False(new RequestUpdatePlaylist().ToCommand(UserID, ID).Shuffle.HasValue);
        Assert.False(new RequestUpdatePlaylist().ToCommand(UserID, ID).Repeat.HasValue);
    }
}
