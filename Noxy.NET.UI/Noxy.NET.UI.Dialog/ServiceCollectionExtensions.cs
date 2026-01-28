using Noxy.NET.UI.Interfaces;
using Noxy.NET.UI.Services;

#pragma warning disable IDE0130, S1200

#pragma warning disable IDE0130, S1200
// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddNoxyNETUIDialogService(this IServiceCollection services)
    {
        services.AddScoped<IDialogService, DialogService>();
        return services;
    }
}
