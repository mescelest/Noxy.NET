using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Noxy.NET.CaseManagement.Application.Interfaces.Repositories;
using Noxy.NET.CaseManagement.Domain.Abstractions.Entities;
using Noxy.NET.CaseManagement.Domain.Entities.Schemas;
using Noxy.NET.CaseManagement.Persistence.Abstractions;
using Noxy.NET.CaseManagement.Persistence.Abstractions.Tables;
using Noxy.NET.CaseManagement.Persistence.Interfaces.Services;
using Noxy.NET.CaseManagement.Persistence.Tables.Schemas;

namespace Noxy.NET.CaseManagement.Persistence.Repositories;

public class TemplateRepository(DataContext context, IEntityToTableMapper mapperE2T, ITableToEntityMapper mapperT2E) : BaseRepository(context, mapperE2T, mapperT2E), ITemplateRepository
{
    public async Task<List<EntitySchema>> GetSchemaList()
    {
        List<TableSchema> result = await Context.Schema.AsNoTracking().ToListAsync();

        return result.Select(MapperT2E.Map).ToList();
    }

    public async Task<EntitySchema> Populate(EntitySchema entity)
    {
        entity.ActionList ??= (await Context.SchemaAction.AsNoTracking().Where(x => x.SchemaID == entity.ID).ToListAsync()).Select(MapperT2E.Map).ToList();
        entity.ActionStepList ??= (await Context.SchemaActionStep.AsNoTracking().Where(x => x.SchemaID == entity.ID).ToListAsync()).Select(MapperT2E.Map).ToList();
        entity.ActionInputList ??= (await Context.SchemaActionInput.AsNoTracking().Where(x => x.SchemaID == entity.ID).ToListAsync()).Select(MapperT2E.Map).ToList();
        entity.AttributeList ??= (await Context.SchemaAttribute.AsNoTracking().Where(x => x.SchemaID == entity.ID).ToListAsync()).Select(MapperT2E.Map).ToList();
        entity.ContextList ??= (await Context.SchemaContext.AsNoTracking().Where(x => x.SchemaID == entity.ID).ToListAsync()).Select(MapperT2E.Map).ToList();
        entity.DynamicValueList ??= (await Context.SchemaDynamicValue.AsNoTracking().Where(x => x.SchemaID == entity.ID).ToListAsync()).Select(MapperT2E.Map).ToList();
        entity.ElementList ??= (await Context.SchemaElement.AsNoTracking().Where(x => x.SchemaID == entity.ID).ToListAsync()).Select(MapperT2E.Map).ToList();
        entity.InputList ??= (await Context.SchemaInput.AsNoTracking().Where(x => x.SchemaID == entity.ID).ToListAsync()).Select(x => MapperT2E.Map(x)).ToList();
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
