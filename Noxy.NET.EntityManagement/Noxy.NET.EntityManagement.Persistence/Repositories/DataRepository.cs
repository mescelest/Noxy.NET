using System.ComponentModel;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Noxy.NET.EntityManagement.Application.Interfaces.Services;
using Noxy.NET.EntityManagement.Domain.Entities.Data;
using Noxy.NET.EntityManagement.Domain.Entities.Data.Discriminators;
using Noxy.NET.EntityManagement.Domain.Interfaces.Repositories;
using Noxy.NET.EntityManagement.Domain.Models.Filters.Data;
using Noxy.NET.EntityManagement.Persistence.Abstractions;
using Noxy.NET.EntityManagement.Persistence.Extensions;
using Noxy.NET.EntityManagement.Persistence.Tables.Data;
using Noxy.NET.EntityManagement.Persistence.Tables.Data.Discriminators;

namespace Noxy.NET.EntityManagement.Persistence.Repositories;

public class DataRepository(DataContext context, IDependencyInjectionService serviceDependencyInjection) : BaseRepository(context, serviceDependencyInjection), IDataRepository
{
    public async Task<List<EntityDataParameter>> GetParameterList()
    {
        List<TableDataParameter> result = await Context.DataParameter.AsNoTracking().ToListAsync();
        return result.Select(MapperT2E.Map).ToList();
    }

    public async Task<EntityDataParameter> GetParameterByID(Guid id)
    {
        TableDataParameter result = await Context.DataParameter.AsNoTracking().SingleAsync(x => x.ID == id);
        return MapperT2E.Map(result);
    }

    public async Task<List<EntityDataParameter>> GetParameterListByIdentifier(string identifier, FilterDataParameterList filter)
    {
        IQueryable<TableDataParameter> query = Context.DataParameter.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(filter.Search)) query = query.Where(x => EF.Functions.Like(x.Value, $"%{filter.Search}%"));

        List<TableDataParameter> result = await query
            .Where(x => x.SchemaIdentifier == identifier)
            .OrderByDynamic(filter.SortColumn, filter.SortDirection == ListSortDirection.Descending)
            .Skip(filter.PageNumber * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        return [.. result.Select(MapperT2E.Map)];
    }

    public async Task<int> GetParameterCountByIdentifier(string identifier, FilterDataParameterCount filter)
    {
        IQueryable<TableDataParameter> query = Context.DataParameter.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(filter.Search)) query = query.Where(x => EF.Functions.Like(x.Value, $"%{filter.Search}%"));

        return await query.Where(x => x.SchemaIdentifier == identifier).CountAsync();
    }

    public async Task<EntityDataParameterStyle> GetParameterStyleByID(Guid id)
    {
        TableDataParameterStyle result = await Context.DataParameterStyle.AsNoTracking().SingleAsync(x => x.ID == id);
        return MapperT2E.Map(result);
    }

    public async Task<EntityDataParameterSystem> GetParameterSystemByID(Guid id)
    {
        TableDataParameterSystem result = await Context.DataParameterSystem.AsNoTracking().SingleAsync(x => x.ID == id);
        return MapperT2E.Map(result);
    }

    public async Task<EntityDataParameterText> GetParameterTextByID(Guid id)
    {
        TableDataParameterText result = await Context.DataParameterText.AsNoTracking().SingleAsync(x => x.ID == id);
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
}
