using Microsoft.Extensions.DependencyInjection.Extensions;
using Noxy.NET.EntityManagement.Application;
using Noxy.NET.EntityManagement.Application.Interfaces;
using Noxy.NET.EntityManagement.Application.Services;
using Noxy.NET.EntityManagement.Definitions;

#pragma warning disable IDE0130, S1200
// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    // Runs the definitions and the texts, and registers the result and the services that use it. If they are not valid this throws now, with every problem, so the application does not start.
    // The services read from the repositories of the persistence library, so that has to be added too (AddEntityManagementPersistence).
    public static IServiceCollection AddEntityManagementApplication(this IServiceCollection services, Action<EntityManagementApplicationOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);

        EntityManagementApplicationOptions options = new();
        configure(options);

        DefinitionRegistry definition = options.BuildDefinition();
        DefinitionTextRegistry texts = options.BuildTexts(definition);

        services.TryAddSingleton(TimeProvider.System);
        services.AddSingleton(definition);
        services.AddSingleton(texts);
        services.AddScoped<ITextService, TextService>();
        services.AddScoped<IElementService, ElementService>();
        services.AddScoped<IRelationService, RelationService>();

        return services;
    }
}
