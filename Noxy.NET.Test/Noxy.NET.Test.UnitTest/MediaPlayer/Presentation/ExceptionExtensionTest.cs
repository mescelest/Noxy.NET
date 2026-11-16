using System.Net;
using Noxy.NET.MediaPlayer.Presentation.Exceptions;
using Noxy.NET.MediaPlayer.Presentation.Extensions;

namespace Noxy.NET.Test.MediaPlayer.Presentation;

public class ExceptionExtensionTest
{
    public static TheoryData<Exception, bool> Cases => new()
    {
        { new HttpRequestException("The network is down."), true },
        { new ServerAddressMissingException(), true },
        { new TaskCanceledException(), true },
        { new ApiException(HttpStatusCode.NotFound, "Not found."), false },
        { new InvalidOperationException("Not a network failure."), false },
        { new OperationCanceledException(), false }
    };

    [Theory]
    [MemberData(nameof(Cases))]
    public void IsOfflineFailure_IsTrueOnlyForTheOfflineKinds(Exception exception, bool expected)
    {
        Assert.Equal(expected, exception.IsOfflineFailure);
    }
}
