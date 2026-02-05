using Microsoft.EntityFrameworkCore;
using Noxy.NET.EntityManagement.Application.Interfaces.Repositories;
using Noxy.NET.EntityManagement.Application.Interfaces.Services;
using Noxy.NET.EntityManagement.Domain.Entities.Data;
using Noxy.NET.EntityManagement.Persistence.Abstractions;
using Noxy.NET.EntityManagement.Persistence.Tables.Data;

namespace Noxy.NET.EntityManagement.Persistence.Repositories;

public class DataRepository(DataContext context, IDependencyInjectionService serviceDependencyInjection) : BaseRepository(context, serviceDependencyInjection), IDataRepository
{
    public async Task<EntityDataParameterText> GetCurrentTextParameterByIdentifier(string identifier)
    {
        TableDataParameterText result = await Context.DataTextParameter
            .OrderBy(x => x.TimeCreated)
            .FirstAsync(x => x.SchemaIdentifier == identifier && x.TimeApproved != null && x.TimeEffective < DateTime.Now);

        return MapperT2E.Map(result);
    }
}
