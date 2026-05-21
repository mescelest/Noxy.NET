using Microsoft.Extensions.Hosting;
using Noxy.NET.EntityManagement.Application.Interfaces;
using Noxy.NET.EntityManagement.Application.Interfaces.Services;
using Noxy.NET.EntityManagement.Application.Services;
using Noxy.NET.EntityManagement.Domain.Entities.Data.Discriminators;
using Noxy.NET.EntityManagement.Domain.Entities.Schemas;
using Noxy.NET.EntityManagement.Domain.Entities.Schemas.Discriminators;

#pragma warning disable IDE0130, S1200
// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IJWTService, JWTService>();
        services.AddScoped<ISchemaBuilderService, SchemaBuilderService>();
        services.AddScoped<ISchemaValidatorService, SchemaValidatorService>();
        services.AddScoped<ITaskBundlingService, TaskBundlingService>();

        services.AddSingleton<IParameterService, ParameterService>();

        return services;
    }

    public static async Task<IHost> UseApplication(this IHost app)
    {
        try
        {
            using IServiceScope scope = app.Services.CreateScope();

            ISchemaBuilderService serviceSchemaBuilder = scope.ServiceProvider.GetRequiredService<ISchemaBuilderService>();
            EntitySchema schema = await serviceSchemaBuilder.ConstructSchema();
            List<EntitySchemaParameter> listSchemaParameter = schema.ParameterList?.Select(x => x.GetValue()).ToList() ?? [];

            IUnitOfWorkFactory serviceUoWFactory = scope.ServiceProvider.GetRequiredService<IUnitOfWorkFactory>();
            await using IUnitOfWork uow = await serviceUoWFactory.Create();

            List<EntityDataParameter> listDataParameter = await uow.Data.GetParameterList();
            IParameterService serviceParameter = scope.ServiceProvider.GetRequiredService<IParameterService>();
            serviceParameter.Initialize(listSchemaParameter, listDataParameter);
        }
        catch
        {
            // ignored
        }

        return app;
    }
}
