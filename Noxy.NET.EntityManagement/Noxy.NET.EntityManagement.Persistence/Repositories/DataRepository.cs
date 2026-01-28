using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Noxy.NET.EntityManagement.Application.Interfaces.Repositories;
using Noxy.NET.EntityManagement.Domain.Entities.Data;
using Noxy.NET.EntityManagement.Domain.Entities.Data.Discriminators;
using Noxy.NET.EntityManagement.Persistence.Abstractions;
using Noxy.NET.EntityManagement.Persistence.Interfaces.Services;
using Noxy.NET.EntityManagement.Persistence.Tables.Data;
using Noxy.NET.EntityManagement.Persistence.Tables.Data.Discriminators;
using Noxy.NET.EntityManagement.Persistence.Tables.Schemas;
using Noxy.NET.EntityManagement.Persistence.Tables.Schemas.Discriminators;

namespace Noxy.NET.EntityManagement.Persistence.Repositories;

public class DataRepository(DataContext context, IEntityToTableMapper mapperE2T, ITableToEntityMapper mapperT2E) : BaseRepository(context, mapperE2T, mapperT2E), IDataRepository
{
    public async Task<List<EntityDataElement>> GetElementListWithIdentifier(string identifier)
    {
        List<TableDataElement> result = await Context.DataElement.Where(x => x.SchemaIdentifier == identifier).ToListAsync();
        return result.Select(x => MapperT2E.Map(x)).ToList();
    }

    public async Task<List<EntityDataProperty.Discriminator>> GetPropertyListWithIdentifierAndElementID(string identifier, Guid idElement)
    {
        List<TableDataProperty> result = await Context.DataProperty.Where(x => x.SchemaIdentifier == identifier && x.ElementID == idElement).ToListAsync();
        return result.Select(x => new EntityDataProperty.Discriminator(MapperT2E.Map(x))).ToList();
    }

    public async Task<List<EntityDataSystemParameter>> GetCurrentSystemParameterList()
    {
        List<TableDataSystemParameter> result = await Context.DataSystemParameter.ToListAsync();
        return result.Select(MapperT2E.Map).ToList();
    }

    public async Task<List<EntityDataTextParameter>> GetCurrentTextParameterList()
    {
        List<TableDataTextParameter> result = await Context.DataTextParameter.ToListAsync();
        return result.Select(MapperT2E.Map).ToList();
    }

    public async Task<EntityDataTextParameter> GetCurrentTextParameterByIdentifier(string identifier)
    {
        TableDataTextParameter result = await Context.DataTextParameter
            .OrderBy(x => x.TimeCreated)
            .FirstAsync(x => x.SchemaIdentifier == identifier && x.TimeApproved != null && x.TimeEffective < DateTime.Now);

        return MapperT2E.Map(result);
    }

    public async Task<EntityDataElement> CreateElement(string identifier)
    {
        EntityEntry<TableDataElement> result = await Context.DataElement.AddAsync(new()
        {
            SchemaIdentifier = identifier
        });

        return MapperT2E.Map(result.Entity);
    }

    public async Task UpdateElement(Guid id)
    {
        TableDataElement result = await Context.DataElement.FirstAsync(x => x.ID == id);
        result.TimeUpdated = DateTime.UtcNow;

        Context.DataElement.Update(result);
    }

    public async Task<EntityDataProperty.Discriminator> CreateProperty(Guid idElement, string identifier, object? value)
    {
        TableSchemaProperty schema = await Context.SchemaProperty.FirstAsync(x => x.SchemaIdentifier == identifier);

        EntityEntry<TableDataProperty> result = await Context.DataProperty.AddAsync(schema switch
        {
            TableSchemaPropertyBoolean property => CreatePropertyBoolean(idElement, property, value),
            TableSchemaPropertyDateTime property => CreatePropertyDateTime(idElement, property, value),
            TableSchemaPropertyString property => CreatePropertyString(idElement, property, value),
            _ => throw new ArgumentOutOfRangeException(nameof(schema))
        });

        return new(MapperT2E.Map(result.Entity));
    }

    private static TableDataPropertyBoolean CreatePropertyBoolean(Guid idElement, TableSchemaPropertyBoolean entity, object? value)
    {
        if (value is not bool parsed) throw new InvalidCastException();

        return new()
        {
            ElementID = idElement,
            SchemaIdentifier = entity.SchemaIdentifier,
            Value = parsed
        };
    }

    private static TableDataPropertyDateTime CreatePropertyDateTime(Guid idElement, TableSchemaPropertyDateTime entity, object? value)
    {
        if (value is not DateTime parsed) throw new InvalidCastException();

        return new()
        {
            ElementID = idElement,
            SchemaIdentifier = entity.SchemaIdentifier,
            Value = parsed
        };
    }

    private static TableDataPropertyString CreatePropertyString(Guid idElement, TableSchemaPropertyString entity, object? value)
    {
        if (value is not string parsed) throw new InvalidCastException();

        return new()
        {
            ElementID = idElement,
            SchemaIdentifier = entity.SchemaIdentifier,
            Value = parsed
        };
    }
}
