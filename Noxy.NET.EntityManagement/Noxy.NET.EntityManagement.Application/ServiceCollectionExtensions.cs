using Microsoft.Extensions.Hosting;
using Noxy.NET.EntityManagement.Application.Interfaces.Services;
using Noxy.NET.EntityManagement.Application.Services;
using Noxy.NET.EntityManagement.Domain.Entities.Schemas;

#pragma warning disable IDE0130, S1200
// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IJWTService, JWTService>();
        services.AddScoped<ISchemaService, SchemaService>();
        services.AddScoped<ISchemaBuilderService, SchemaBuilderService>();
        services.AddScoped<ITaskBundlingService, TaskBundlingService>();
        services.AddScoped<ITemplateService, TemplateService>();

        services.AddSingleton<IApplicationService, ApplicationService>();

        return services;
    }

    public static async Task<IHost> UseApplication(this IHost app)
    {
        try
        {
            using IServiceScope scope = app.Services.CreateScope();

            ISchemaBuilderService serviceSchemaBuilderService = scope.ServiceProvider.GetRequiredService<ISchemaBuilderService>();
            EntitySchema schema = await serviceSchemaBuilderService.ConstructSchema();

            IApplicationService serviceApplication = scope.ServiceProvider.GetRequiredService<IApplicationService>();
            serviceApplication.SetSchema(schema);
        }
        catch
        {
            // ignored
        }

        return app;
    }
}
