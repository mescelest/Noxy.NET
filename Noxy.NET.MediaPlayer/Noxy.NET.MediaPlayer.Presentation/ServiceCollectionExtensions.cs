using Noxy.NET.MediaPlayer.Presentation.Services;

#pragma warning disable IDE0130, S1200
// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPresentation(this IServiceCollection services, string url)
    {
        services.AddHttpClient<APIHttpClient>(client => client.BaseAddress = new(url));

        return services;
    }
}
