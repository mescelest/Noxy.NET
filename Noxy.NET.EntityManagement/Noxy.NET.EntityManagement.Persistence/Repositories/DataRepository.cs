using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Noxy.NET.EntityManagement.Application.Interfaces.Repositories;
using Noxy.NET.EntityManagement.Application.Interfaces.Services;
using Noxy.NET.EntityManagement.Domain.Entities.Data;
using Noxy.NET.EntityManagement.Domain.Entities.Data.Discriminators;
using Noxy.NET.EntityManagement.Persistence.Abstractions;
using Noxy.NET.EntityManagement.Persistence.Tables.Data;
using Noxy.NET.EntityManagement.Persistence.Tables.Data.Discriminators;
using Noxy.NET.EntityManagement.Persistence.Tables.Schemas;

namespace Noxy.NET.EntityManagement.Persistence.Repositories;

public class DataRepository(DataContext context, IDependencyInjectionService serviceDependencyInjection) : BaseRepository(context, serviceDependencyInjection), IDataRepository
{
    public async Task<EntityDataParameterStyle> CreateStyleParameter(string identifier, string value, DateTime? timeEffective = null)
    {
        TableSchemaParameterStyle schema = await Context.SchemaParameterStyle.SingleAsync(x => x.SchemaIdentifier == identifier);
        EntityEntry<TableDataParameterStyle> entry = await Context.DataStyleParameter.AddAsync(new()
        {
            SchemaIdentifier = identifier,
            Value = value,
            TimeApproved = schema.IsApprovalRequired ? null : DateTime.UtcNow,
            TimeEffective = timeEffective ?? DateTime.UtcNow,
        });

        return MapperT2E.Map(entry.Entity);
    }

    public async Task<EntityDataParameterSystem> CreateSystemParameter(string identifier, string value, DateTime? timeEffective = null)
    {
        TableSchemaParameterSystem schema = await Context.SchemaParameterSystem.SingleAsync(x => x.SchemaIdentifier == identifier);
        EntityEntry<TableDataParameterSystem> entry = await Context.DataSystemParameter.AddAsync(new()
        {
            SchemaIdentifier = identifier,
            Value = value,
            TimeApproved = schema.IsApprovalRequired ? null : DateTime.UtcNow,
            TimeEffective = timeEffective ?? DateTime.UtcNow,
        });

        return MapperT2E.Map(entry.Entity);
    }

    public async Task<EntityDataParameterText> CreateTextParameter(string identifier, string culture, string value, DateTime? timeEffective = null)
    {
        TableSchemaParameterText schema = await Context.SchemaParameterText.SingleAsync(x => x.SchemaIdentifier == identifier);
        EntityEntry<TableDataParameterText> entry = await Context.DataTextParameter.AddAsync(new()
        {
            SchemaIdentifier = identifier,
            Culture = culture,
            Value = value,
            TimeApproved = schema.IsApprovalRequired ? null : DateTime.UtcNow,
            TimeEffective = timeEffective ?? DateTime.UtcNow,
        });

        return MapperT2E.Map(entry.Entity);
    }

    public async Task<Guid> RemoveParameterByID(Guid id)
    {
        TableDataParameter entity = await Context.DataParameter.FirstAsync(x => x.ID == id);
        if (entity.TimeEffective <= DateTime.UtcNow) throw new InvalidOperationException("Cannot delete parameter that is already effective.");

        Context.DataParameter.Remove(entity);
        return entity.ID;
    }

    public async Task<List<EntityDataParameter.Discriminator>> GetParameterListWithIdentifier(string identifier)
    {
        List<TableDataParameter> result = await Context.DataParameter
            .AsNoTracking()
            .Where(x => x.SchemaIdentifier == identifier)
            .ToListAsync();

        return result.Select(MapperT2E.Map).ToList();
    }

    public async Task<EntityDataParameterText?> GetCurrentTextParameterByIdentifier(string identifier)
    {
        TableDataParameterText? result = await Context.DataTextParameter
            .OrderBy(x => x.TimeCreated)
            .FirstOrDefaultAsync(x => x.SchemaIdentifier == identifier && x.TimeApproved != null && x.TimeEffective < DateTime.UtcNow);

        return result != null ? MapperT2E.Map(result) : null;
    }

    public async Task<Dictionary<string, EntityDataParameterText?>> GetCurrentTextParameterByIdentifierList(IEnumerable<string> identifiers)
    {
        List<TableDataParameterText?> newestRows = await Context.DataTextParameter
            .Where(x =>
                identifiers.Contains(x.SchemaIdentifier) &&
                x.TimeApproved != null &&
                x.TimeEffective < DateTime.UtcNow)
            .GroupBy(x => x.SchemaIdentifier)
            .Select(g => g.OrderByDescending(x => x.TimeCreated).FirstOrDefault())
            .ToListAsync();

        Dictionary<string, TableDataParameterText> lookup = newestRows
            .OfType<TableDataParameterText>()
            .ToDictionary(x => x.SchemaIdentifier, x => x);

        return identifiers.ToDictionary(
            id => id,
            id => lookup.TryGetValue(id, out TableDataParameterText? row) ? MapperT2E.Map(row) : null
        );
    }
}
