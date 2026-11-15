using Noxy.NET.MediaPlayer.Application.Commands.Metadata;
using Noxy.NET.MediaPlayer.Application.Requests.Metadata;

namespace Noxy.NET.Test.MediaPlayer.Metadata;

public class RequestUpdateSettingsToCommandTest
{
    private static readonly Guid UserID = Guid.CreateVersion7();

    [Fact]
    public void ToCommand_CarriesTheUserFromTheParameter()
    {
        CommandSettingsUpdate command = new RequestUpdateSettings().ToCommand(UserID);

        Assert.Equal(UserID, command.UserID);
    }

    [Fact]
    public void ToCommand_KeepsAFieldThatWasLeftOutAbsent()
    {
        Assert.False(new RequestUpdateSettings().ToCommand(UserID).AutoMergeOnRename.HasValue);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void ToCommand_CarriesTheValueThatWasSent(bool value)
    {
        CommandSettingsUpdate command = new RequestUpdateSettings { AutoMergeOnRename = new(value) }.ToCommand(UserID);

        Assert.True(command.AutoMergeOnRename.HasValue);
        Assert.Equal(value, command.AutoMergeOnRename.Value);
    }
}
