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
            .AddNoxyNETUIWeightManager();
    }

    public static IServiceCollection AddNoxyNETUIWeightManager(this IServiceCollection services)
    {
        services.AddScoped(typeof(IWeightManager<>), typeof(WeightManager<>));
        return services;
    }
}
