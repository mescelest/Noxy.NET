using Noxy.NET.MediaPlayer.Application.Commands.Authentication;
using Noxy.NET.MediaPlayer.Application.Requests.Authentication;

namespace Noxy.NET.Test.MediaPlayer.Authentication;

public class RequestSignUpToCommandTest
{
    [Fact]
    public void ToCommand_CarriesTheEmailAndThePassword()
    {
        CommandAuthenticationSignUp command = new RequestSignUp { Email = "user@example.com", Password = "a-long-enough-password" }.ToCommand();

        Assert.Equal("user@example.com", command.Email);
        Assert.Equal("a-long-enough-password", command.Password);
    }
}
