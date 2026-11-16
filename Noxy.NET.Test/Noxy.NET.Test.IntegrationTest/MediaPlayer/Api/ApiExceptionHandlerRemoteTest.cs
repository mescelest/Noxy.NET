using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Noxy.NET.MediaPlayer.API;
using Noxy.NET.MediaPlayer.Domain.Exceptions;

namespace Noxy.NET.Test.MediaPlayer.Api;

public class ApiExceptionHandlerRemoteTest
{
    private static async Task<int> StatusOf(Exception exception)
    {
        ServiceCollection services = new();
        services.AddOptions();
        services.AddProblemDetails();
        await using ServiceProvider provider = services.BuildServiceProvider();
        DefaultHttpContext context = new() { RequestServices = provider };
        context.Response.Body = new MemoryStream();
        ApiExceptionHandler handler = new(provider.GetRequiredService<IProblemDetailsService>());

        bool handled = await handler.TryHandleAsync(context, exception, TestContext.Current.CancellationToken);

        Assert.True(handled);
        return context.Response.StatusCode;
    }

    [Fact]
    public async Task InvalidQueueEditException_IsMappedTo400()
    {
        Assert.Equal(StatusCodes.Status400BadRequest, await StatusOf(new InvalidQueueEditException("The edit is not valid.")));
    }

    [Fact]
    public async Task TooManyDevicesException_IsMappedTo400()
    {
        Assert.Equal(StatusCodes.Status400BadRequest, await StatusOf(new TooManyDevicesException()));
    }
}
