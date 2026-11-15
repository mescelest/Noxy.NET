using Noxy.NET.MediaPlayer.Application.Commands.Authentication;
using Noxy.NET.MediaPlayer.Application.Requests.Authentication;

namespace Noxy.NET.Test.MediaPlayer.Authentication;

public class RequestRefreshToCommandTest
{
    [Fact]
    public void ToCommand_CarriesTheRefreshToken()
    {
        CommandAuthenticationRefresh command = new RequestRefresh { RefreshToken = "refresh-token" }.ToCommand();

        Assert.Equal("refresh-token", command.RefreshToken);
    }
}
