using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Noxy.NET.EntityManagement.Application.Interfaces.Repositories;
using Noxy.NET.EntityManagement.Application.Interfaces.Services;
using Noxy.NET.EntityManagement.Domain.Abstractions.Entities;
using Noxy.NET.EntityManagement.Domain.Entities.Schemas;
using Noxy.NET.EntityManagement.Persistence.Abstractions;
using Noxy.NET.EntityManagement.Persistence.Abstractions.Tables;
using Noxy.NET.EntityManagement.Persistence.Tables.Schemas;
using Noxy.NET.EntityManagement.Persistence.Tables.Schemas.Discriminators;

namespace Noxy.NET.EntityManagement.Persistence.Repositories;

public class TemplateRepository(DataContext context, IDependencyInjectionService serviceDependencyInjection) : BaseRepository(context, serviceDependencyInjection), ITemplateRepository
{
    public async Task<List<EntitySchema>> GetSchemaList()
    {
        List<TableSchema> result = await Context.Schema.AsNoTracking().ToListAsync();

        return result.Select(MapperT2E.Map).ToList();
    }

    public async Task<EntitySchema> Populate(EntitySchema entity)
    {
        ITaskBundlingService serviceTaskBundling = DI.GetService<ITaskBundlingService>();

        (List<TableSchemaContext> contextList, List<TableSchemaParameter> parameterList, List<TableSchemaElement> elementList, List<TableSchemaProperty> propertyList) = await serviceTaskBundling.WhenAll(
            Context.SchemaContext.AsNoTracking().Where(x => x.SchemaID == entity.ID).ToListAsync(),
            Context.SchemaParameter.AsNoTracking().Where(x => x.SchemaID == entity.ID).ToListAsync(),
            Context.SchemaElement.AsNoTracking().Where(x => x.SchemaID == entity.ID).ToListAsync(),
            Context.SchemaProperty.AsNoTracking().Where(x => x.SchemaID == entity.ID).ToListAsync()
        );

        entity.ContextList ??= contextList.Select(MapperT2E.Map).ToList();
        entity.ParameterList ??= parameterList.Select(MapperT2E.Map).ToList();
        entity.ElementList ??= elementList.Select(MapperT2E.Map).ToList();
        entity.PropertyList ??= propertyList.Select(MapperT2E.Map).ToList();


        entity.ContextList ??= (await Context.SchemaContext.AsNoTracking().Where(x => x.SchemaID == entity.ID).ToListAsync()).Select(MapperT2E.Map).ToList();
        entity.ParameterList ??= (await Context.SchemaParameter.AsNoTracking().Where(x => x.SchemaID == entity.ID).ToListAsync()).Select(MapperT2E.Map).ToList();
        entity.ElementList ??= (await Context.SchemaElement.AsNoTracking().Where(x => x.SchemaID == entity.ID).ToListAsync()).Select(MapperT2E.Map).ToList();
        entity.PropertyList ??= (await Context.SchemaProperty.AsNoTracking().Where(x => x.SchemaID == entity.ID).ToListAsync()).Select(x => MapperT2E.Map(x)).ToList();

        return entity;
    }

    public async Task<EntitySchema> GetSchemaByID(Guid id)
    {
        TableSchema result = await Context.Schema.AsNoTracking().SingleAsync(x => x.ID == id);
        return MapperT2E.Map(result);
    }

    public async Task<EntitySchema> GetCurrentSchema()
    {
        TableSchema result = await Context.Schema.AsNoTracking().OrderByDescending(x => x.TimeActivated).FirstAsync(x => x.IsActive);

        return MapperT2E.Map(result);
    }

    public async Task<EntitySchema> Create(EntitySchema entity)
    {
        return await CreateEntity(entity, MapperE2T.Map, MapperT2E.Map);
    }

    public void Update(EntitySchema entity)
    {
        Context.Schema.Update(MapperE2T.Map(entity));
    }

    private async Task<TEntity> CreateEntity<TEntity, TTable>(TEntity entity, Func<TEntity, TTable> mapE2T, Func<TTable, TEntity> mapT2E) where TEntity : BaseEntityTemplate where TTable : BaseTableTemplate
    {
        if (entity.Order == 0) entity.Order = await Context.Set<TTable>().CountAsync();
        EntityEntry<TTable> result = await Context.Set<TTable>().AddAsync(mapE2T(entity));
        return mapT2E(result.Entity);
    }
}
