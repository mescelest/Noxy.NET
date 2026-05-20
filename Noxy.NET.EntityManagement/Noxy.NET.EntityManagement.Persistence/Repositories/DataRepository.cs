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
    public async Task<EntityDataParameter.Discriminator> GetParameterByID(Guid id)
    {
        TableDataParameter result = await Context.DataParameter.SingleAsync(x => x.ID == id);
        return MapperT2E.Map(result);
    }

    public async Task<EntityDataParameterStyle> GetParameterStyleByID(Guid id)
    {
        TableDataParameterStyle result = await Context.DataParameterStyle.SingleAsync(x => x.ID == id);
        return MapperT2E.Map(result);
    }

    public async Task<EntityDataParameterSystem> GetParameterSystemByID(Guid id)
    {
        TableDataParameterSystem result = await Context.DataParameterSystem.SingleAsync(x => x.ID == id);
        return MapperT2E.Map(result);
    }

    public async Task<EntityDataParameterText> GetParameterTextByID(Guid id)
    {
        TableDataParameterText result = await Context.DataParameterText.SingleAsync(x => x.ID == id);
        return MapperT2E.Map(result);
    }

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

    public void UpdateParameter(EntityDataParameter entity)
    {
        Context.DataParameter.Update(MapperE2T.Map(entity));
    }

    public void UpdateParameterStyle(EntityDataParameterStyle entity)
    {
        Context.DataParameterStyle.Update(MapperE2T.Map(entity));
    }

    public void UpdateParameterSystem(EntityDataParameterSystem entity)
    {
        Context.DataParameterSystem.Update(MapperE2T.Map(entity));
    }

    public void UpdateParameterText(EntityDataParameterText entity)
    {
        Context.DataParameterText.Update(MapperE2T.Map(entity));
    }

    public void RemoveParameter(EntityDataParameter entity)
    {
        Context.DataParameter.Remove(MapperE2T.Map(entity));
    }

    public async Task<EntityDataParameter.Discriminator?> GetEffectiveParameterByIdentifier(string identifier)
    {
        var result = await Context.DataParameter
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.SchemaIdentifier == identifier && x.TimeApproved != null && x.TimeEffective < DateTime.UtcNow);
        return result != null ? MapperT2E.Map(result) : null;
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
