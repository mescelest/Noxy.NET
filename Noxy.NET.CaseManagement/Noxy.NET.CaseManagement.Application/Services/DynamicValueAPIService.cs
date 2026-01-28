using Microsoft.Extensions.DependencyInjection;
using Noxy.NET.CaseManagement.Application.Interfaces;
using Noxy.NET.CaseManagement.Application.Interfaces.Services;
using Noxy.NET.CaseManagement.Domain.Entities.Data;
using Noxy.NET.CaseManagement.Domain.Entities.Data.Discriminators;

namespace Noxy.NET.CaseManagement.Application.Services;

public class DynamicValueAPIService(IServiceProvider serviceProvider) : IDynamicValueAPIService
{
    public async Task<EntityDataElement> CreateElement(string identifier, Dictionary<string, object?>? data = null)
    {
        await using IUnitOfWork uow = await GetUnitOfWork();

        EntityDataElement entityElement = await uow.Data.CreateElement(identifier);
        entityElement.PropertyList = [];

        foreach (KeyValuePair<string, object?> item in data ?? [])
        {
            EntityDataProperty.Discriminator entityProperty = await uow.Data.CreateProperty(entityElement.ID, item.Key, item.Value);
            entityElement.PropertyList.Add(entityProperty);
        }

        await uow.Commit(true);

        return entityElement;
    }    
    
    public async Task UpdateElement(Guid id, Dictionary<string, object?>? data = null)
    {
        await using IUnitOfWork uow = await GetUnitOfWork();

        foreach (KeyValuePair<string, object?> item in data ?? [])
        {
            await uow.Data.CreateProperty(id, item.Key, item.Value);
        }
        await uow.Data.UpdateElement(id);
        await uow.Commit(true);
    }

    private async Task<IUnitOfWork> GetUnitOfWork()
    {
        using IServiceScope scope = serviceProvider.CreateScope();

        IUnitOfWorkFactory factory = scope.ServiceProvider.GetRequiredService<IUnitOfWorkFactory>();
        IUnitOfWork context = await factory.Create();

        return context;
    }
}
