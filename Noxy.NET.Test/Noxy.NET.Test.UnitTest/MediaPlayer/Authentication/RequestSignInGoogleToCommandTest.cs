using Noxy.NET.MediaPlayer.Application.Commands.Authentication;
using Noxy.NET.MediaPlayer.Application.Requests.Authentication;

namespace Noxy.NET.Test.MediaPlayer.Authentication;

public class RequestSignInGoogleToCommandTest
{
    [Fact]
    public void ToCommand_CarriesTheIdToken()
    {
        CommandAuthenticationSignInGoogle command = new RequestSignInGoogle { IdToken = "id-token" }.ToCommand();

        Assert.Equal("id-token", command.IdToken);
    }
}
