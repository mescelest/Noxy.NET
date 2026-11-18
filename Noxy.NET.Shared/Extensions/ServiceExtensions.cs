using Noxy.NET.Interfaces;
using Noxy.NET.Services;

#pragma warning disable IDE0130, S1200
// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddNoxyNETCommonServices()
        {
            return services
                .AddNoxyNETTaskBundlingService()
                .AddNoxyNETDebouncerService();
        }

        public IServiceCollection AddNoxyNETTaskBundlingService()
        {
            return services.AddScoped<ITaskBundlingService, TaskBundlingService>();
        }

        public IServiceCollection AddNoxyNETDebouncerService()
        {
            return services.AddScoped<IDebouncerService, DebouncerService>();
        }
    }
}
