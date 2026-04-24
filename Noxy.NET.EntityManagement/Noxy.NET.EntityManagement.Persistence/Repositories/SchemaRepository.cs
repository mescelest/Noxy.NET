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
using Noxy.NET.EntityManagement.Domain.Interfaces;
using Noxy.NET.EntityManagement.Persistence.Abstractions;
using Noxy.NET.EntityManagement.Persistence.Abstractions.Tables;
using Noxy.NET.EntityManagement.Persistence.Tables.Schemas;
using Noxy.NET.EntityManagement.Persistence.Tables.Schemas.Discriminators;
using Noxy.NET.EntityManagement.Persistence.Tables.Schemas.Junctions;

namespace Noxy.NET.EntityManagement.Persistence.Repositories;

public class SchemaRepository(DataContext context, IDependencyInjectionService serviceDependencyInjection) : BaseRepository(context, serviceDependencyInjection), ISchemaRepository
{
    public async Task<Guid> GetCurrentSchemaID()
    {
        return await Context.Schema.AsNoTracking().Where(x => x.IsActive).OrderByDescending(x => x.TimeActivated).Select(x => x.ID).SingleAsync();
    }

    public async Task<EntitySchema> GetCurrentSchema()
    {
        return MapperT2E.Map(await Context.Schema.AsNoTracking().OrderByDescending(x => x.TimeActivated).SingleAsync(x => x.IsActive));
    }

    public async Task<EntitySchema> GetSchemaByID(Guid id)
    {
        return MapperT2E.Map(await Context.Schema.AsNoTracking().SingleAsync(x => x.ID == id));
    }

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

    public async Task<EntitySchemaPropertyInteger> GetSchemaPropertyIntegerByID(Guid id) => await GetEntityByID<TableSchemaPropertyInteger, EntitySchemaPropertyInteger>(id, MapperT2E.Map);
    public async Task<EntitySchemaPropertyString> GetSchemaPropertyStringByID(Guid id) => await GetEntityByID<TableSchemaPropertyString, EntitySchemaPropertyString>(id, MapperT2E.Map);

    public async Task<EntitySchemaPropertyTable> GetSchemaPropertyTableByID(Guid id)
    {
        return MapperT2E.Map(await Context.SchemaPropertyTable.AsNoTracking().SingleAsync(x => x.ID == id));
    }

    public async Task<List<EntitySchemaContext>> GetSchemaContextListBySchemaID(Guid id)
    {
        List<TableSchemaContext> result = await Context.SchemaContext.AsNoTracking().Where(x => x.SchemaID == id).ToListAsync();
        return [.. result.Select(MapperT2E.Map)];
    }

    public async Task<List<EntitySchemaElement>> GetSchemaElementListBySchemaID(Guid id)
    {
        List<TableSchemaElement> result = await Context.SchemaElement.AsNoTracking().Where(x => x.SchemaID == id).ToListAsync();
        return [.. result.Select(MapperT2E.Map)];
    }

    public async Task<List<EntitySchemaParameter.Discriminator>> GetSchemaParameterListBySchemaID(Guid id)
    {
        List<TableSchemaParameter> result = await Context.SchemaParameter.AsNoTracking().Where(x => x.SchemaID == id).ToListAsync();
        return [.. result.Select(MapperT2E.Map)];
    }

    public async Task<List<EntitySchema>> GetSchemaList(FilterSchemaList filter)
    {
        IQueryable<TableSchema> query = Context.Schema.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(filter.Search)) query = query.Where(x => EF.Functions.Like(x.Name, $"%{filter.Search}%"));

        List<TableSchema> result = await query
            .OrderBy(x => x.Name)
            .Skip(filter.PageNumber * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        return [.. result.Select(MapperT2E.Map)];
    }

    public async Task<List<EntitySchemaContext>> GetSchemaContextList(FilterSchemaContextList filter)
    {
        IQueryable<TableSchemaContext> query = Context.SchemaContext.AsNoTracking().Where(x => x.SchemaID == filter.SchemaID);

        if (!string.IsNullOrWhiteSpace(filter.Search)) query = query.Where(x => EF.Functions.Like(x.Name, $"%{filter.Search}%"));

        List<TableSchemaContext> result = await query
            .OrderBy(x => x.Name)
            .Skip(filter.PageNumber * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        return [.. result.Select(MapperT2E.Map)];
    }

    public async Task<List<EntitySchemaElement>> GetSchemaElementList(FilterSchemaElementList filter)
    {
        IQueryable<TableSchemaElement> query = Context.SchemaElement.AsNoTracking().Where(x => x.SchemaID == filter.SchemaID);

        if (!string.IsNullOrWhiteSpace(filter.Search)) query = query.Where(x => EF.Functions.Like(x.Name, $"%{filter.Search}%"));

        List<TableSchemaElement> result = await query
            .OrderBy(x => x.Name)
            .Skip(filter.PageNumber * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        return [.. result.Select(MapperT2E.Map)];
    }

    public async Task<List<EntitySchemaParameter.Discriminator>> GetSchemaParameterList(FilterSchemaParameterList filter)
    {
        IQueryable<TableSchemaParameter> query = Context.SchemaParameter.AsNoTracking().Where(x => x.SchemaID == filter.SchemaID);

        if (filter.ParameterType is { Count: > 0 })
        {
            List<Type> types = [.. filter.ParameterType.Select(key => TableSchemaParameter.TypeMap[key])];

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

        return [.. result.Select(MapperT2E.Map)];
    }

    public async Task<List<EntitySchemaProperty.Discriminator>> GetSchemaPropertyListBySchemaID(Guid id)
    {
        List<TableSchemaProperty> result = await Context.SchemaProperty.AsNoTracking().Where(x => x.SchemaID == id).ToListAsync();
        return [.. result.Select(MapperT2E.Map)];
    }

    public async Task<List<EntityJunctionSchemaContextHasElement>> GetSchemaContextHasElementListBySchemaID(Guid id)
    {
        List<TableJunctionSchemaContextHasElement> result = await Context.SchemaContextHasElement.AsNoTracking().Where(x => x.Entity!.SchemaID == id).ToListAsync();
        return [.. result.Select(MapperT2E.Map)];
    }

    public async Task<List<EntityJunctionSchemaElementHasProperty>> GetSchemaElementHasPropertyListBySchemaID(Guid id)
    {
        List<TableJunctionSchemaElementHasProperty> result = await Context.SchemaElementHasProperty.AsNoTracking().Where(x => x.Entity!.SchemaID == id).ToListAsync();
        return [.. result.Select(MapperT2E.Map)];
    }

    #region -- Schema --

    public async Task<EntitySchema> Create(EntitySchema entity)
    {
        EntityEntry<TableSchema> result = await Context.Schema.AddAsync(MapperE2T.Map(entity));
        return MapperT2E.Map(result.Entity);
    }

    public async Task<EntitySchema> Update(EntitySchema entity)
    {
        TableSchema result = await Context.Schema.AsNoTracking().FirstAsync(x => x.ID == entity.ID);

        result.Name = entity.Name;
        result.Note = entity.Note;
        result.TimeUpdated = DateTime.UtcNow;
        Context.Schema.Update(result);

        return MapperT2E.Map(result);
    }

    public async Task<EntitySchema> Clone(Guid id)
    {
        TableSchema entity = await Context.Schema.AsNoTracking().FirstAsync(x => x.ID == id);
        entity = entity.Clone();
        await Context.Schema.AddAsync(entity);

        List<TableSchemaElement> listElement = await Context.SchemaElement.Where(x => x.SchemaID == id).ToListAsync();
        List<TableSchemaElement> listElementClone = [.. listElement.Select(x => x.Clone(entity.ID))];
        await Context.SchemaElement.AddRangeAsync(listElementClone);

        List<TableSchemaContext> listContext = await Context.SchemaContext.Where(x => x.SchemaID == id).ToListAsync();
        List<TableSchemaContext> listContextClone = [.. listContext.Select(x => x.Clone(entity.ID))];
        await Context.SchemaContext.AddRangeAsync(listContextClone);

        List<TableSchemaParameter> listParameter = await Context.SchemaParameter.Where(x => x.SchemaID == id).ToListAsync();
        List<TableSchemaParameter> listParameterClone = [.. listParameter.Select(x => x.Clone(entity.ID))];
        await Context.SchemaParameter.AddRangeAsync(listParameterClone);

        List<TableSchemaProperty> listProperty = await Context.SchemaProperty.Where(x => x.SchemaID == id).ToListAsync();
        List<TableSchemaProperty> listPropertyClone = [.. listProperty.Select(x => x.Clone(entity.ID))];
        await Context.SchemaProperty.AddRangeAsync(listPropertyClone);

        Dictionary<Guid, Guid> mapElement = listElement.Zip(listElementClone, (old, clone) => (Old: old.ID, Clone: clone.ID)).ToDictionary(x => x.Old, x => x.Clone);
        Dictionary<Guid, Guid> mapContext = listContext.Zip(listContextClone, (old, clone) => (Old: old.ID, Clone: clone.ID)).ToDictionary(x => x.Old, x => x.Clone);
        // Dictionary<Guid, Guid> mapParameter = listParameter.Zip(listParameterClone, (old, clone) => (Old: old.ID, Clone: clone.ID)).ToDictionary(x => x.Old, x => x.Clone);
        Dictionary<Guid, Guid> mapProperty = listProperty.Zip(listPropertyClone, (old, clone) => (Old: old.ID, Clone: clone.ID)).ToDictionary(x => x.Old, x => x.Clone);

        List<TableJunctionSchemaElementHasProperty> listJunctionElement = await Context.SchemaElementHasProperty.Where(x => mapElement.Keys.Contains(x.EntityID)).ToListAsync();
        await Context.SchemaElementHasProperty.AddRangeAsync(listJunctionElement.Select(j => j.Clone(mapElement[j.EntityID], mapProperty[j.RelationID])));

        List<TableJunctionSchemaContextHasElement> listJunctionContext = await Context.SchemaContextHasElement.Where(x => mapContext.Keys.Contains(x.EntityID)).ToListAsync();
        await Context.SchemaContextHasElement.AddRangeAsync(listJunctionContext.Select(j => j.Clone(mapElement[j.EntityID], mapProperty[j.RelationID])));

        return MapperT2E.Map(entity);
    }

    public async Task<Guid> Delete(Guid id)
    {
        TableSchema entity = await Context.Schema.AsNoTracking().FirstAsync(x => x.ID == id);
        if (entity.TimeActivated != null) throw new InvalidOperationException("Cannot delete schema that has been activated.");

        entity = await Context.Schema
            .AsNoTracking()
            .Include(x => x.ContextList!)
            .ThenInclude(e => e.ElementList)
            .Include(x => x.ElementList!)
            .ThenInclude(e => e.PropertyList)
            .Include(x => x.PropertyList)
            .Include(x => x.ParameterList)
            .FirstAsync(x => x.ID == id);

        foreach (TableSchemaElement element in entity.ElementList ?? [])
        {
            Context.SchemaElementHasProperty.RemoveRange(element.PropertyList ?? []);
        }

        foreach (TableSchemaContext context in entity.ContextList ?? [])
        {
            Context.SchemaContextHasElement.RemoveRange(context.ElementList ?? []);
        }

        Context.SchemaElement.RemoveRange(entity.ElementList ?? []);
        Context.SchemaContext.RemoveRange(entity.ContextList ?? []);
        Context.SchemaParameter.RemoveRange(entity.ParameterList ?? []);
        Context.SchemaProperty.RemoveRange(entity.PropertyList ?? []);

        Context.Schema.Remove(entity);

        return entity.ID;
    }

    public async Task<EntitySchema> Activate(Guid id)
    {
        TableSchema entity = await Context.Schema.AsNoTracking().FirstAsync(x => x.ID == id);
        if (entity.IsActive) throw new InvalidOperationException("Cannot activate schema that is already active.");

        entity.IsActive = true;
        entity.TimeActivated = DateTime.UtcNow;
        Context.Schema.Update(entity);

        List<TableSchema> list = await Context.Schema.AsNoTracking().Where(x => x.ID != id && x.IsActive).ToListAsync();
        foreach (TableSchema item in list)
        {
            item.IsActive = false;
            Context.Schema.Update(item);
        }

        return MapperT2E.Map(entity);
    }

    public async Task<EntitySchemaParameter.Discriminator> Create(EntitySchemaParameter entity)
    {
        EntityEntry<TableSchemaParameter> result = await Context.SchemaParameter.AddAsync(MapperE2T.Map(entity));
        return MapperT2E.Map(result.Entity);
    }

    public async Task<EntitySchemaContext> Create(EntitySchemaContext entity)
    {
        return await CreateEntity(entity, MapperE2T.Map, MapperT2E.Map);
    }

    public async Task<EntitySchemaElement> Create(EntitySchemaElement entity)
    {
        await UpdateOrder<TableSchemaElement, EntitySchemaElement>(entity);
        return await CreateEntity(entity, MapperE2T.Map, MapperT2E.Map);
    }

    public async Task<EntitySchemaProperty.Discriminator> Create(EntitySchemaProperty entity)
    {
        await UpdateOrder<TableSchemaProperty, EntitySchemaProperty>(entity);
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

    private async Task<TEntity> GetEntityByID<TTable, TEntity>(Guid id, Func<TTable, TEntity> mapT2E) where TTable : BaseTableSchema where TEntity : BaseEntitySchema
    {
        return mapT2E(await Context.Set<TTable>().AsNoTracking().SingleAsync(x => x.ID == id));
    }

    private async Task<TEntity> CreateEntity<TEntity, TTable>(TEntity entity, Func<TEntity, TTable> mapE2T, Func<TTable, TEntity> mapT2E) where TEntity : BaseEntitySchema where TTable : BaseTableSchema
    {
        EntityEntry<TTable> result = await Context.Set<TTable>().AddAsync(mapE2T(entity));
        return mapT2E(result.Entity);
    }

    private async Task UpdateOrder<TTable, TEntity>(TEntity entity) where TTable : BaseTableSchema where TEntity : BaseEntitySchema, ISchemaOrdering
    {
        if (entity.Order != BaseEntity.DefaultOrder) return;
        entity.Order = await Context.Set<TTable>().CountAsync(x => x.SchemaID == entity.SchemaID);
    }

    #endregion
}
