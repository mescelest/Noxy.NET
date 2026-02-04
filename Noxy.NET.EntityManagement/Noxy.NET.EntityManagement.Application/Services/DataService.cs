using Noxy.NET.EntityManagement.Application.Interfaces;
using Noxy.NET.EntityManagement.Application.Interfaces.Services;
using Noxy.NET.EntityManagement.Domain.Entities.Data;
using Noxy.NET.EntityManagement.Domain.Entities.Schemas;
using Noxy.NET.EntityManagement.Domain.Entities.Schemas.Discriminators;
using Noxy.NET.EntityManagement.Domain.Models.Forms.Data;
using Noxy.NET.EntityManagement.Domain.ViewModels;

namespace Noxy.NET.EntityManagement.Application.Services;

public class DataService(IUnitOfWorkFactory serviceUoWFactory, IApplicationService serviceApplication) : IDataService
{
    public ViewModelSchemaContext[] GetContextList()
    {
        return [];
    }

    public ViewModelSchemaContext GetContextListWithIdentifier(string identifier)
    {
        throw new NotImplementedException();
        // return serviceViewModelFactory.Create(serviceApplication.GetSchemaContext(identifier));
    }

    public async Task<ViewModelDataElement[]> GetElementListWithContextIdentifier(string identifier)
    {
        await using IUnitOfWork uow = await serviceUoWFactory.Create();
        EntitySchemaContext context = serviceApplication.GetSchemaContext(identifier);

        List<ViewModelDataElement> result = [];
        foreach (EntitySchemaElement entitySchemaElement in context.ElementList?.Select(x => x.Relation).OfType<EntitySchemaElement>() ?? [])
        {
            foreach (EntityDataElement entityDataElement in await uow.Data.GetElementListWithIdentifier(entitySchemaElement.SchemaIdentifier))
            {
                entityDataElement.PropertyList = [];
                foreach (EntitySchemaProperty entitySchemaProperty in entitySchemaElement.PropertyList?.Select(x => x.Relation?.GetValue()).OfType<EntitySchemaProperty>() ?? [])
                {
                    entityDataElement.PropertyList.AddRange(await uow.Data.GetPropertyListWithIdentifierAndElementID(entitySchemaProperty.SchemaIdentifier, entityDataElement.ID));
                }
                // result.Add(serviceViewModelFactory.Create(entityDataElement));
            }
        }

        return result.ToArray();
    }

    public async Task<List<EntityDataSystemParameter>> GetSystemParameterList()
    {
        await using IUnitOfWork uow = await serviceUoWFactory.Create();
        return await uow.Data.GetCurrentSystemParameterList();
    }

    public async Task<List<EntityDataTextParameter>> GetTextParameterList()
    {
        await using IUnitOfWork uow = await serviceUoWFactory.Create();
        return await uow.Data.GetCurrentTextParameterList();
    }

    public async Task<ViewModelParameter[]> GetParameterList(DataParameterListFormModel model)
    {
        throw new NotImplementedException();
    }

    public async Task<Dictionary<string, string>> ResolveTextParameterList(IEnumerable<string> list)
    {
        await using IUnitOfWork uow = await serviceUoWFactory.Create();

        Dictionary<string, string> result = [];
        foreach (string identifier in list)
        {
            try
            {
                EntityDataTextParameter entityTextParameter = await uow.Data.GetCurrentTextParameterByIdentifier(identifier);
                result.Add(identifier, entityTextParameter.Value);
            }
            catch
            {
                result.Add(identifier, "[MISSING]");
            }
        }

        return result;
    }
}
