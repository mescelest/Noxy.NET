using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Noxy.NET.EntityManagement.Application.Interfaces.Repositories;
using Noxy.NET.EntityManagement.Application.Interfaces.Services;
using Noxy.NET.EntityManagement.Application.Models;
using Noxy.NET.EntityManagement.Domain.Abstractions.Entities;
using Noxy.NET.EntityManagement.Domain.Entities.Schemas;
using Noxy.NET.EntityManagement.Domain.Entities.Schemas.Discriminators;
using Noxy.NET.EntityManagement.Domain.Entities.Schemas.Junctions;
using Noxy.NET.EntityManagement.Persistence.Abstractions;
using Noxy.NET.EntityManagement.Persistence.Abstractions.Tables;
using Noxy.NET.EntityManagement.Persistence.Tables.Schemas;
using Noxy.NET.EntityManagement.Persistence.Tables.Schemas.Discriminators;
using Noxy.NET.EntityManagement.Persistence.Tables.Schemas.Junctions;

namespace Noxy.NET.EntityManagement.Persistence.Repositories;

public class SchemaRepository(DataContext context, IDependencyInjectionService serviceDependencyInjection) : BaseRepository(context, serviceDependencyInjection), ISchemaRepository
{
    public async Task<EntitySchemaContext> GetSchemaContextByID(Guid id)
    {
        return MapperT2E.Map(await Context.SchemaContext.AsNoTracking().SingleAsync(x => x.ID == id));
    }

    public async Task<EntitySchemaParameter.Discriminator> GetSchemaParameterByID(Guid id)
    {
        return MapperT2E.Map(await Context.SchemaParameter.AsNoTracking().SingleAsync(x => x.ID == id));
    }

    public async Task<EntitySchemaParameterStyle> GetSchemaParameterStyleByID(Guid id)
    {
        return MapperT2E.Map(await Context.SchemaParameterStyle.AsNoTracking().SingleAsync(x => x.ID == id));
    }

    public async Task<EntitySchemaParameterSystem> GetSchemaParameterSystemByID(Guid id)
    {
        return MapperT2E.Map(await Context.SchemaParameterSystem.AsNoTracking().SingleAsync(x => x.ID == id));
    }

    public async Task<EntitySchemaParameterText> GetSchemaParameterTextByID(Guid id)
    {
        return MapperT2E.Map(await Context.SchemaParameterText.AsNoTracking().SingleAsync(x => x.ID == id));
    }

    public async Task<EntitySchemaElement> GetSchemaElementByID(Guid id)
    {
        return MapperT2E.Map(await Context.SchemaElement.AsNoTracking().SingleAsync(x => x.ID == id));
    }

    public async Task<EntitySchemaProperty.Discriminator> GetSchemaPropertyByID(Guid id)
    {
        return MapperT2E.Map(await Context.SchemaProperty.AsNoTracking().SingleAsync(x => x.ID == id));
    }

    public async Task<EntitySchemaPropertyBoolean> GetSchemaPropertyBooleanByID(Guid id)
    {
        return MapperT2E.Map(await Context.SchemaPropertyBoolean.AsNoTracking().SingleAsync(x => x.ID == id));
    }

    public async Task<EntitySchemaPropertyCollection> GetSchemaPropertyCollectionByID(Guid id)
    {
        return MapperT2E.Map(await Context.SchemaPropertyCollection.AsNoTracking().SingleAsync(x => x.ID == id));
    }

    public async Task<EntitySchemaPropertyDateTime> GetSchemaPropertyDateTimeByID(Guid id)
    {
        return MapperT2E.Map(await Context.SchemaPropertyDateTime.AsNoTracking().SingleAsync(x => x.ID == id));
    }

    public async Task<EntitySchemaPropertyDecimal> GetSchemaPropertyDecimalByID(Guid id)
    {
        return MapperT2E.Map(await Context.SchemaPropertyDecimal.AsNoTracking().SingleAsync(x => x.ID == id));
    }

    public async Task<EntitySchemaPropertyImage> GetSchemaPropertyImageByID(Guid id)
    {
        return MapperT2E.Map(await Context.SchemaPropertyImage.AsNoTracking().SingleAsync(x => x.ID == id));
    }

    public async Task<EntitySchemaPropertyInteger> GetSchemaPropertyIntegerByID(Guid id)
    {
        return MapperT2E.Map(await Context.SchemaPropertyInteger.AsNoTracking().SingleAsync(x => x.ID == id));
    }

    public async Task<EntitySchemaPropertyString> GetSchemaPropertyStringByID(Guid id)
    {
        return MapperT2E.Map(await Context.SchemaPropertyString.AsNoTracking().SingleAsync(x => x.ID == id));
    }

    public async Task<EntitySchemaPropertyTable> GetSchemaPropertyTableByID(Guid id)
    {
        return MapperT2E.Map(await Context.SchemaPropertyTable.AsNoTracking().SingleAsync(x => x.ID == id));
    }

    public async Task<List<EntitySchemaContext>> GetSchemaContextListBySchemaID(Guid id)
    {
        List<TableSchemaContext> result = await Context.SchemaContext.AsNoTracking().Where(x => x.SchemaID == id).ToListAsync();
        return result.Select(MapperT2E.Map).ToList();
    }

    public async Task<List<EntitySchemaElement>> GetSchemaElementListBySchemaID(Guid id)
    {
        List<TableSchemaElement> result = await Context.SchemaElement.AsNoTracking().Where(x => x.SchemaID == id).ToListAsync();
        return result.Select(MapperT2E.Map).ToList();
    }

    public async Task<List<EntitySchemaParameter.Discriminator>> GetSchemaParameterListBySchemaID(Guid id)
    {
        List<TableSchemaParameter> result = await Context.SchemaParameter.AsNoTracking().Where(x => x.SchemaID == id).ToListAsync();
        return result.Select(MapperT2E.Map).ToList();
    }

    public async Task<List<EntitySchemaParameter.Discriminator>> GetSchemaParameterList(FilterSchemaParameterList filter)
    {
        IQueryable<TableSchemaParameter> query = Context.SchemaParameter.AsNoTracking();

        if (filter.ParameterType is { Count: > 0 })
        {
            List<Type> types = filter.ParameterType.Select(key => TableSchemaParameter.TypeMap[key]).ToList();

            ParameterExpression param = Expression.Parameter(typeof(TableSchemaParameter), "x");
            Expression typeChecks = types.Select(Expression (t) => Expression.TypeIs(param, t)).Aggregate(Expression.OrElse);
            Expression<Func<TableSchemaParameter, bool>> lambda = Expression.Lambda<Func<TableSchemaParameter, bool>>(typeChecks, param);
            query = query.Where(lambda);
        }

        if (!string.IsNullOrWhiteSpace(filter.Search)) query = query.Where(x => EF.Functions.Like(x.Name, $"%{filter.Search}%"));
        if (filter.IsSystemDefined is not null) query = query.Where(x => x.IsSystemDefined == filter.IsSystemDefined);
        if (filter.IsApprovalRequired is not null) query = query.Where(x => x.IsApprovalRequired == filter.IsApprovalRequired);

        List<TableSchemaParameter> result = await query
            .OrderBy(x => x.Name)
            .Skip(filter.PageNumber * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        return result.Select(MapperT2E.Map).ToList();
    }

    public async Task<List<EntitySchemaProperty.Discriminator>> GetSchemaPropertyListBySchemaID(Guid id)
    {
        List<TableSchemaProperty> result = await Context.SchemaProperty.AsNoTracking().Where(x => x.SchemaID == id).ToListAsync();
        return result.Select(MapperT2E.Map).ToList();
    }

    public async Task<List<EntityJunctionSchemaContextHasElement>> GetSchemaContextHasElementListBySchemaID(Guid id)
    {
        List<TableJunctionSchemaContextHasElement> result = await Context.SchemaContextHasElement.AsNoTracking().Where(x => x.Entity!.SchemaID == id).ToListAsync();
        return result.Select(MapperT2E.Map).ToList();
    }

    public async Task<List<EntityJunctionSchemaElementHasProperty>> GetSchemaElementHasPropertyListBySchemaID(Guid id)
    {
        List<TableJunctionSchemaElementHasProperty> result = await Context.SchemaElementHasProperty.AsNoTracking().Where(x => x.Entity!.SchemaID == id).ToListAsync();
        return result.Select(MapperT2E.Map).ToList();
    }

    #region -- Populate --

    public async Task<EntitySchemaContext> Populate(EntitySchemaContext entity)
    {
        entity.ElementList ??= (await Context.SchemaContextHasElement.AsNoTracking().Where(x => x.EntityID == entity.ID).ToListAsync()).Select(MapperT2E.Map).ToList();

        return entity;
    }

    public async Task<EntitySchemaParameter.Discriminator> Populate(EntitySchemaParameter.Discriminator entity)
    {
        return entity.GetValue() switch
        {
            EntitySchemaParameterSystem entityDateTime => new(await Populate(entityDateTime)),
            EntitySchemaParameterText entityString => new(await Populate(entityString)),
            _ => entity
        };
    }

    public Task<EntitySchemaParameterSystem> Populate(EntitySchemaParameterSystem entity)
    {
        // TODO: Fill this
        return Task.FromResult(entity);
    }

    public Task<EntitySchemaParameterText> Populate(EntitySchemaParameterText entity)
    {
        // TODO: Fill this
        return Task.FromResult(entity);
    }

    public async Task<EntitySchemaElement> Populate(EntitySchemaElement entity)
    {
        entity.PropertyList ??= (await Context.SchemaElementHasProperty.AsNoTracking().Where(x => x.EntityID == entity.ID).ToListAsync()).Select(MapperT2E.Map).ToList();

        return entity;
    }

    public async Task<EntitySchemaProperty.Discriminator> Populate(EntitySchemaProperty.Discriminator entity)
    {
        return entity.GetValue() switch
        {
            EntitySchemaPropertyBoolean entityBoolean => new(await Populate(entityBoolean)),
            EntitySchemaPropertyDateTime entityDateTime => new(await Populate(entityDateTime)),
            EntitySchemaPropertyString entityString => new(await Populate(entityString)),
            _ => entity
        };
    }

    public Task<EntitySchemaPropertyBoolean> Populate(EntitySchemaPropertyBoolean entity)
    {
        // TODO: Fill this
        return Task.FromResult(entity);
    }

    public Task<EntitySchemaPropertyDateTime> Populate(EntitySchemaPropertyDateTime entity)
    {
        // TODO: Fill this
        return Task.FromResult(entity);
    }

    public Task<EntitySchemaPropertyString> Populate(EntitySchemaPropertyString entity)
    {
        // TODO: Fill this
        return Task.FromResult(entity);
    }

    public async Task<EntityJunctionSchemaContextHasElement> Populate(EntityJunctionSchemaContextHasElement entity)
    {
        entity.Relation ??= MapperT2E.Map(await Context.SchemaElement.AsNoTracking().SingleAsync(x => x.ID == entity.RelationID));

        return entity;
    }

    public async Task<EntityJunctionSchemaElementHasProperty> Populate(EntityJunctionSchemaElementHasProperty entity)
    {
        entity.Relation ??= MapperT2E.Map(await Context.SchemaProperty.AsNoTracking().SingleAsync(x => x.ID == entity.RelationID));

        return entity;
    }

    #endregion -- Populate --

    #region -- Create --

    public async Task<EntitySchemaParameter.Discriminator> Create(EntitySchemaParameter entity)
    {
        await UpdateOrder<TableSchemaParameter>(entity);
        EntityEntry<TableSchemaParameter> result = await Context.SchemaParameter.AddAsync(MapperE2T.Map(entity));
        return MapperT2E.Map(result.Entity);
    }

    public async Task<EntitySchemaContext> Create(EntitySchemaContext entity)
    {
        return await CreateEntity(entity, MapperE2T.Map, MapperT2E.Map);
    }

    public async Task<EntitySchemaElement> Create(EntitySchemaElement entity)
    {
        return await CreateEntity(entity, MapperE2T.Map, MapperT2E.Map);
    }

    public async Task<EntitySchemaProperty.Discriminator> Create(EntitySchemaProperty entity)
    {
        await UpdateOrder<TableSchemaProperty>(entity);
        EntityEntry<TableSchemaProperty> result = await Context.SchemaProperty.AddAsync(MapperE2T.Map(entity));
        return MapperT2E.Map(result.Entity);
    }

    #endregion -- Create --

    #region -- Update --

    public void Update(EntitySchemaContext entity)
    {
        Context.SchemaContext.Update(MapperE2T.Map(entity));
    }

    public void Update(EntitySchemaParameter entity)
    {
        Context.SchemaParameter.Update(MapperE2T.Map(entity));
    }

    public void Update(EntitySchemaElement entity)
    {
        Context.SchemaElement.Update(MapperE2T.Map(entity));
    }

    public void Update(EntitySchemaProperty baseEntity)
    {
        Context.SchemaProperty.Update(MapperE2T.Map(baseEntity));
    }

    #endregion -- Update --

    #region -- Private methods --

    private async Task<TEntity> CreateEntity<TEntity, TTable>(TEntity entity, Func<TEntity, TTable> mapE2T, Func<TTable, TEntity> mapT2E) where TEntity : BaseEntitySchema where TTable : BaseTableSchema
    {
        await UpdateOrder<TTable>(entity);
        EntityEntry<TTable> result = await Context.Set<TTable>().AddAsync(mapE2T(entity));
        return mapT2E(result.Entity);
    }

    private async Task UpdateOrder<TTable>(BaseEntitySchema entity) where TTable : BaseTableSchema
    {
        if (entity.Order == 0) entity.Order = await Context.Set<TTable>().CountAsync(x => x.SchemaID == entity.SchemaID);
    }

    #endregion
}
