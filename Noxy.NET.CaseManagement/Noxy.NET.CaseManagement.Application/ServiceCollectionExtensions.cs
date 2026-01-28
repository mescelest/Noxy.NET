using Microsoft.Extensions.Hosting;
using Noxy.NET.CaseManagement.Application.Interfaces.Services;
using Noxy.NET.CaseManagement.Application.Services;
using Noxy.NET.CaseManagement.Domain.Entities.Data;
using Noxy.NET.CaseManagement.Domain.Entities.Schemas;

#pragma warning disable IDE0130, S1200
// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IAssociationService, AssociationService>();
        services.AddScoped<IAuthenticationService, AuthenticationService>();
        services.AddScoped<IDataService, DataService>();
        services.AddScoped<IJWTService, JWTService>();
        services.AddScoped<ISchemaService, SchemaService>();
        services.AddScoped<ISchemaBuilderService, SchemaBuilderService>();
        services.AddScoped<ITemplateService, TemplateService>();
        services.AddScoped<IViewModelFactoryService, ViewModelFactoryService>();
        
        services.AddSingleton<IApplicationService, ApplicationService>();
        services.AddSingleton<IDynamicValueService, DynamicValueService>();
        services.AddSingleton<IDynamicValueAPIService, DynamicValueAPIService>();

        return services;
    }

    public static async Task<IHost> UseApplication(this IHost app)
    {
        try
        {
            using IServiceScope scope = app.Services.CreateScope();
            IDataService serviceData = scope.ServiceProvider.GetRequiredService<IDataService>();
            IApplicationService serviceApplication = scope.ServiceProvider.GetRequiredService<IApplicationService>();
            IDynamicValueService serviceDynamicValue = scope.ServiceProvider.GetRequiredService<IDynamicValueService>();
            ISchemaBuilderService serviceSchemaBuilderService = scope.ServiceProvider.GetRequiredService<ISchemaBuilderService>();

            List<EntityDataSystemParameter> listSystemParameter = await serviceData.GetSystemParameterList();
            List<EntityDataTextParameter> listTextParameter = await serviceData.GetTextParameterList();
            EntitySchema schema = await serviceSchemaBuilderService.ConstructSchema();
            serviceApplication.SetSchema(schema);
            serviceDynamicValue.Initialize(schema, listSystemParameter, listTextParameter);
        }
        catch
        {
            // ignored
        }
        
        return app;
    }
}

