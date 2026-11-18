using Noxy.NET.UI.Interfaces;
using Noxy.NET.UI.Services;

#pragma warning disable IDE0130, S1200
// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddNoxyNETUICommonServices()
        {
            return services.AddNoxyNETUIThemeService();
        }

        public IServiceCollection AddNoxyNETUIThemeService()
        {
            services.AddScoped(typeof(IThemeService<>), typeof(ThemeService<>));
            return services;
        }
    }
}
