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
            IEnumerable<string> list = schema.ParameterList?.Select(x => x.SchemaIdentifier) ?? [];

            IUnitOfWorkFactory serviceUoWFactory = scope.ServiceProvider.GetRequiredService<IUnitOfWorkFactory>();
            await using IUnitOfWork uow = await serviceUoWFactory.Create();

            IParameterService serviceParameter = scope.ServiceProvider.GetRequiredService<IParameterService>();
            Dictionary<string, EntityDataParameter.Discriminator> collection = await uow.Data.GetCurrentParameterByIdentifierList(list);

            Dictionary<EntitySchemaParameter.Discriminator, EntityDataParameter.Discriminator> mapped = [];
            foreach (EntitySchemaParameter.Discriminator item in schema.ParameterList ?? [])
            {
                if (!collection.TryGetValue(item.SchemaIdentifier, out EntityDataParameter.Discriminator? value)) continue;
                mapped[item] = value;
            }

            serviceParameter.SetParameterCollection(mapped);
        }
        catch
        {
            // ignored
        }

        return app;
    }
}
