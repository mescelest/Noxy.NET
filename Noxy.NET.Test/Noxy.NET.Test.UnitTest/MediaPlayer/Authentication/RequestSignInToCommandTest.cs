using Noxy.NET.MediaPlayer.Application.Commands.Authentication;
using Noxy.NET.MediaPlayer.Application.Requests.Authentication;

namespace Noxy.NET.Test.MediaPlayer.Authentication;

public class RequestSignInToCommandTest
{
    [Fact]
    public void ToCommand_CarriesTheEmailAndThePassword()
    {
        CommandAuthenticationSignIn command = new RequestSignIn { Email = "user@example.com", Password = "a-long-enough-password" }.ToCommand();

        Assert.Equal("user@example.com", command.Email);
        Assert.Equal("a-long-enough-password", command.Password);
    }
}
