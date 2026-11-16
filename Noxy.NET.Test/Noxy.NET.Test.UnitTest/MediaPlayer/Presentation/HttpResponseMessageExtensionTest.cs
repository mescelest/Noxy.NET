using System.Net;
using Noxy.NET.MediaPlayer.Presentation.Exceptions;
using Noxy.NET.MediaPlayer.Presentation.Extensions;
using Noxy.NET.Test.MediaPlayer.Fakes;

namespace Noxy.NET.Test.MediaPlayer.Presentation;

public class HttpResponseMessageExtensionTest
{
    [Fact]
    public async Task ToApiException_TakesTheStatusAndTheProblemDetailsTitle()
    {
        using HttpResponseMessage response = FakeHttpMessageHandler.Problem(HttpStatusCode.Conflict, "The email is already in use.");

        ApiException exception = await response.ToApiException(TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Conflict, exception.Status);
        Assert.Equal("The email is already in use.", exception.Title);
        Assert.Equal("The email is already in use.", exception.Message);
    }

    [Fact]
    public async Task ToApiException_WithABodyThatIsNotProblemDetails_FallsBackToAGenericTitle_WithoutEchoingTheBody()
    {
        using HttpResponseMessage response = new(HttpStatusCode.BadGateway);
        response.Content = new StringContent("<html>secret internal page</html>");

        ApiException exception = await response.ToApiException(TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadGateway, exception.Status);
        Assert.DoesNotContain("secret", exception.Title);
        Assert.False(string.IsNullOrWhiteSpace(exception.Title));
    }

    [Fact]
    public async Task ToApiException_WithNoBody_FallsBackToAGenericTitle()
    {
        using HttpResponseMessage response = new(HttpStatusCode.Unauthorized);

        ApiException exception = await response.ToApiException(TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, exception.Status);
        Assert.False(string.IsNullOrWhiteSpace(exception.Title));
    }

    [Fact]
    public async Task ToApiException_WithAProblemWhoseTitleIsMissing_FallsBackToAGenericTitle()
    {
        using HttpResponseMessage response = FakeHttpMessageHandler.Json(HttpStatusCode.BadRequest, "{\"status\":400}");

        ApiException exception = await response.ToApiException(TestContext.Current.CancellationToken);

        Assert.False(string.IsNullOrWhiteSpace(exception.Title));
    }
}
