using Microsoft.Extensions.DependencyInjection.Extensions;
using Noxy.NET.MediaPlayer.Application.Configurations;
using Noxy.NET.MediaPlayer.Application.Interfaces.Services;
using Noxy.NET.MediaPlayer.Application.Services;

#pragma warning disable IDE0130, S1200
// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddOptions<ConfigJwt>()
            .BindConfiguration(ConfigJwt.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.TryAddSingleton(TimeProvider.System);
        services.AddSingleton<IPasswordHashService, PasswordHashService>();
        services.AddSingleton<ITokenService, TokenService>();
        services.AddScoped<IAuthenticationService, AuthenticationService>();

        return services;
    }
}
