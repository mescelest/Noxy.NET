using Microsoft.EntityFrameworkCore;
using Noxy.NET.EntityManagement.Application.Interfaces.Repositories;
using Noxy.NET.EntityManagement.Application.Interfaces.Services;
using Noxy.NET.EntityManagement.Domain.Entities.Data;
using Noxy.NET.EntityManagement.Domain.Entities.Data.Discriminators;
using Noxy.NET.EntityManagement.Persistence.Abstractions;
using Noxy.NET.EntityManagement.Persistence.Tables.Data;
using Noxy.NET.EntityManagement.Persistence.Tables.Data.Discriminators;

namespace Noxy.NET.EntityManagement.Persistence.Repositories;

public class DataRepository(DataContext context, IDependencyInjectionService serviceDependencyInjection) : BaseRepository(context, serviceDependencyInjection), IDataRepository
{
    public async Task<List<EntityDataParameter.Discriminator>> GetParameterListWithIdentifier(string identifier)
    {
        List<TableDataParameter> result = await Context.DataParameter
            .AsNoTracking()
            .Where(x => x.SchemaIdentifier == identifier)
            .OrderBy(x => x.TimeCreated)
            .ToListAsync();

        return result.Select(MapperT2E.Map).ToList();
    }

    public async Task<EntityDataParameterText?> GetCurrentTextParameterByIdentifier(string identifier)
    {
        TableDataParameterText? result = await Context.DataTextParameter
            .OrderBy(x => x.TimeCreated)
            .FirstOrDefaultAsync(x => x.SchemaIdentifier == identifier && x.TimeApproved != null && x.TimeEffective < DateTime.Now);

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
