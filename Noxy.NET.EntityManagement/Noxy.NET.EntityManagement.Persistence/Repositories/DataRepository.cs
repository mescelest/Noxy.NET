using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Noxy.NET.EntityManagement.Application.Interfaces.Services;
using Noxy.NET.EntityManagement.Domain.Entities.Data;
using Noxy.NET.EntityManagement.Domain.Entities.Data.Discriminators;
using Noxy.NET.EntityManagement.Domain.Interfaces.Repositories;
using Noxy.NET.EntityManagement.Persistence.Abstractions;
using Noxy.NET.EntityManagement.Persistence.Tables.Data;
using Noxy.NET.EntityManagement.Persistence.Tables.Data.Discriminators;

namespace Noxy.NET.EntityManagement.Persistence.Repositories;

public class DataRepository(DataContext context, IDependencyInjectionService serviceDependencyInjection) : BaseRepository(context, serviceDependencyInjection), IDataRepository
{
    public async Task<EntityDataParameterStyle> CreateParameterStyle(EntityDataParameterStyle entity)
    {
        EntityEntry<TableDataParameterStyle> result = await Context.DataParameterStyle.AddAsync(MapperE2T.Map(entity));
        return MapperT2E.Map(result.Entity);
    }

    public async Task<EntityDataParameterSystem> CreateParameterSystem(EntityDataParameterSystem entity)
    {
        EntityEntry<TableDataParameterSystem> result = await Context.DataParameterSystem.AddAsync(MapperE2T.Map(entity));
        return MapperT2E.Map(result.Entity);
    }

    public async Task<EntityDataParameterText> CreateParameterText(EntityDataParameterText entity)
    {
        EntityEntry<TableDataParameterText> result = await Context.DataParameterText.AddAsync(MapperE2T.Map(entity));
        return MapperT2E.Map(result.Entity);
    }

    public async Task<Guid> RemoveParameterByID(Guid id)
    {
        TableDataParameter entity = await Context.DataParameter.FirstAsync(x => x.ID == id);
        if (entity.TimeEffective <= DateTime.UtcNow) throw new InvalidOperationException("Cannot delete parameter that is already effective.");

        Context.DataParameter.Remove(entity);
        return entity.ID;
    }

    public async Task<Dictionary<string, EntityDataParameter.Discriminator>> GetCurrentParameterByIdentifierList(IEnumerable<string> identifiers)
    {
        List<TableDataParameter> result = await Context.DataParameter
            .Where(x =>
                identifiers.Contains(x.SchemaIdentifier) &&
                x.TimeApproved != null &&
                x.TimeEffective < DateTime.UtcNow)
            .GroupBy(x => x.SchemaIdentifier)
            .Select(g => g.OrderByDescending(x => x.TimeCreated).First())
            .ToListAsync();

        return result.ToDictionary(x => x.SchemaIdentifier, x => MapperT2E.Map(x));
    }

    public async Task<List<EntityDataParameter.Discriminator>> GetParameterListWithIdentifier(string identifier)
    {
        List<TableDataParameter> result = await Context.DataParameter
            .AsNoTracking()
            .Where(x => x.SchemaIdentifier == identifier)
            .ToListAsync();

        return [.. result.Select(MapperT2E.Map)];
    }

    public async Task<EntityDataParameterText?> GetCurrentTextParameterByIdentifier(string identifier)
    {
        TableDataParameterText? result = await Context.DataParameterText
            .OrderBy(x => x.TimeCreated)
            .FirstOrDefaultAsync(x => x.SchemaIdentifier == identifier && x.TimeApproved != null && x.TimeEffective < DateTime.UtcNow);

        return result != null ? MapperT2E.Map(result) : null;
    }

    public async Task<Dictionary<string, EntityDataParameterText?>> GetCurrentTextParameterByIdentifierList(IEnumerable<string> identifiers)
    {
        List<TableDataParameterText?> newestRows = await Context.DataParameterText
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
