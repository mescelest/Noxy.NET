using System.Text.Json;
using System.Text.Json.Serialization;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Noxy.NET.CaseManagement.Application.Interfaces.Services;
using Noxy.NET.CaseManagement.Application.Services;
using Noxy.NET.CaseManagement.Domain.Constants;
using Noxy.NET.CaseManagement.Presentation.Components.Controls;
using Noxy.NET.CaseManagement.Presentation.Services;

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

        services.AddHttpClient<AuthenticationHttpClient>(client => client.BaseAddress = new(url));
        services.AddHttpClient<SchemaAPIService>(client => client.BaseAddress = new(url));
        services.AddHttpClient<DataAPIService>(client => client.BaseAddress = new(url));

        services.AddScoped<TextService>();
        services.AddScoped<ActionHubService>();
        services.AddScoped<UserAuthenticationStateProvider>();
        services.AddScoped<AuthenticationStateProvider>(p => p.GetRequiredService<UserAuthenticationStateProvider>());
        services.AddScoped<IJWTService, JWTService>();

        return services;
    }

    public static IServiceCollection AddBaseToPresentation(this IServiceCollection services)
    {
        return services;
    }

    public static WebAssemblyHost UseBaseWithPresentation(this WebAssemblyHost app)
    {
        ActionHubService serviceActionHub = app.Services.GetRequiredService<ActionHubService>();

        serviceActionHub.RegisterActionInput<ActionInputCheckbox>(SchemaActionInputConstants.SchemaIdentifierCheckbox);
        serviceActionHub.RegisterActionInput<ActionInputDate>(SchemaActionInputConstants.SchemaIdentifierDate);
        serviceActionHub.RegisterActionInput<ActionInputDecimal>(SchemaActionInputConstants.SchemaIdentifierDecimal);
        serviceActionHub.RegisterActionInput<ActionInputInteger>(SchemaActionInputConstants.SchemaIdentifierInteger);
        serviceActionHub.RegisterActionInput<ActionInputPassword>(SchemaActionInputConstants.SchemaIdentifierPassword);
        serviceActionHub.RegisterActionInput<ActionInputSingleChoice>(SchemaActionInputConstants.SchemaIdentifierSingleChoice);
        serviceActionHub.RegisterActionInput<ActionInputText>(SchemaActionInputConstants.SchemaIdentifierText);

        return app;
    }
}
