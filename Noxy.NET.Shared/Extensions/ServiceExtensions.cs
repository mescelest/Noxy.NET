using Noxy.NET.Interfaces;
using Noxy.NET.Services;

#pragma warning disable IDE0130, S1200
// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddNoxyNETCommonServices(this IServiceCollection services)
    {
        return services
            .AddNoxyNETTaskBundlingService()
            .AddNoxyNETDebouncerService();
    }

    public static IServiceCollection AddNoxyNETTaskBundlingService(this IServiceCollection services)
    {
        return services.AddScoped<ITaskBundlingService, TaskBundlingService>();
    }

    public static IServiceCollection AddNoxyNETDebouncerService(this IServiceCollection services)
    {
        return services.AddScoped<IDebouncerService, DebouncerService>();
    }
}
