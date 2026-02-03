using Noxy.NET.UI.Interfaces;
using Noxy.NET.UI.Services;

#pragma warning disable IDE0130, S1200
// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddNoxyNETUICommonServices(this IServiceCollection services)
    {
        return services
            .AddNoxyNETCommonServices()
            .AddNoxyNETUIThemeService();
    }

    public static IServiceCollection AddNoxyNetUILoadingService(this IServiceCollection services)
    {
        services.AddScoped<PageLoadingService>();
        return services;
    }

    public static IServiceCollection AddNoxyNETUIThemeService(this IServiceCollection services)
    {
        services.AddScoped(typeof(IThemeService<>), typeof(ThemeService<>));
        return services;
    }
}
