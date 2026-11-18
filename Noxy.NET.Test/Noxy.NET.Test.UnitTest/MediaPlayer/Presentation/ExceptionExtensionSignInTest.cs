using System.Net;
using Noxy.NET.MediaPlayer.Presentation;
using Noxy.NET.MediaPlayer.Presentation.Exceptions;
using Noxy.NET.MediaPlayer.Presentation.Extensions;

namespace Noxy.NET.Test.MediaPlayer.Presentation;

public class ExceptionExtensionSignInTest
{
    [Fact]
    public void ApiException_ShowsItsTitle()
    {
        Assert.Equal("Wrong email or password.", new ApiException(HttpStatusCode.Unauthorized, "Wrong email or password.").ToSignInMessage());
    }

    [Fact]
    public void AnApiExceptionWithoutATitle_ShowsTheFixedSentence()
    {
        Assert.Equal(Constants.SignInMessageFailed, new ApiException(HttpStatusCode.InternalServerError, " ").ToSignInMessage());
    }

    [Fact]
    public void AnyOtherException_ShowsTheFixedSentence_AndNoExceptionText()
    {
        string message = new InvalidOperationException("secret token abc").ToSignInMessage();

        Assert.Equal(Constants.SignInMessageFailed, message);
        Assert.DoesNotContain("secret", message);
    }

    [Fact]
    public void AnOfflineFailure_SaysTheServerCannotBeReached()
    {
        Assert.Equal(Constants.SignInMessageOffline, new HttpRequestException("connect to http://10.0.0.5 failed").ToSignInMessage());
    }

    [Fact]
    public void AMissingAddress_AsksForTheAddress()
    {
        Assert.Equal(Constants.SignInMessageNoAddress, new ServerAddressMissingException().ToSignInMessage());
    }

    [Fact]
    public void ToActionMessage_ShowsTheTitle_TheOfflineSentence_Or_TheFixedSentence()
    {
        Assert.Equal("The playlist is full.", new ApiException(HttpStatusCode.BadRequest, "The playlist is full.").ToActionMessage());
        Assert.Equal(Constants.ActionMessageOffline, new HttpRequestException("http://10.0.0.5").ToActionMessage());
        string other = new InvalidOperationException("secret").ToActionMessage();
        Assert.Equal(Constants.ActionMessageFailed, other);
    }
}
