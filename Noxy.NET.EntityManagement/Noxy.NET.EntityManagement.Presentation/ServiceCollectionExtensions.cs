using System.Text.Json;
using System.Text.Json.Serialization;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using Noxy.NET.EntityManagement.Application.Interfaces.Services;
using Noxy.NET.EntityManagement.Application.Services;
using Noxy.NET.EntityManagement.Presentation.Services;
using Noxy.NET.UI.Services;

#pragma warning disable IDE0130, S1200
// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPresentation(this IServiceCollection services, string url)
    {
        services.AddAuthorizationCore();
        services.AddCascadingAuthenticationState();
        services.AddBlazoredLocalStorage(config =>
        {
            config.JsonSerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
            config.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
            config.JsonSerializerOptions.IgnoreReadOnlyProperties = true;
            config.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
            config.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            config.JsonSerializerOptions.ReadCommentHandling = JsonCommentHandling.Skip;
            config.JsonSerializerOptions.WriteIndented = false;
        });

        services.AddHttpClient<APIHttpClient>(client => client.BaseAddress = new(url));

        services.AddScoped<TextService>();
        services.AddScoped<LoadingService>();
        services.AddScoped<UserAuthenticationStateProvider>();
        services.AddScoped<AuthenticationStateProvider>(p => p.GetRequiredService<UserAuthenticationStateProvider>());
        services.AddScoped<IJWTService, JWTService>();

        return services;
    }
}
