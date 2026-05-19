using System.ComponentModel;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Noxy.NET.EntityManagement.Application.Interfaces.Services;
using Noxy.NET.EntityManagement.Domain.Entities.Schemas;
using Noxy.NET.EntityManagement.Domain.Entities.Schemas.Discriminators;
using Noxy.NET.EntityManagement.Domain.Entities.Schemas.Junctions;
using Noxy.NET.EntityManagement.Domain.Interfaces.Repositories;
using Noxy.NET.EntityManagement.Domain.Models.Filters;
using Noxy.NET.EntityManagement.Persistence.Abstractions;
using Noxy.NET.EntityManagement.Persistence.Extensions;
using Noxy.NET.EntityManagement.Persistence.Tables.Schemas;
using Noxy.NET.EntityManagement.Persistence.Tables.Schemas.Discriminators;
using Noxy.NET.EntityManagement.Persistence.Tables.Schemas.Junctions;

namespace Noxy.NET.EntityManagement.Persistence.Repositories;

public class SchemaRepository(DataContext context, IDependencyInjectionService serviceDependencyInjection) : BaseRepository(context, serviceDependencyInjection), ISchemaRepository
{
    public async Task<Guid> GetCurrentSchemaID()
    {
        return await Context.Schema.AsNoTracking().Where(x => x.IsActive).Select(x => x.ID).SingleAsync();
    }

    public async Task<EntitySchema> GetCurrentSchema()
    {
        return MapperT2E.Map(await Context.Schema.AsNoTracking().SingleAsync(x => x.IsActive));
    }

    #region -- Schema --

    public async Task<EntitySchema> GetSchemaByID(Guid id)
    {
        return MapperT2E.Map(await Context.Schema.AsNoTracking().SingleAsync(x => x.ID == id));
    }

    public async Task<List<EntitySchema>> GetSchemaList(FilterSchemaList filter)
    {
        IQueryable<TableSchema> query = Context.Schema.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(filter.Search)) query = query.Where(x => EF.Functions.Like(x.Name, $"%{filter.Search}%"));
        if (filter.IsActivated.HasValue) query = filter.IsActivated.Value ? query.Where(x => x.TimeActivated != null) : query.Where(x => x.TimeActivated == null);

        List<TableSchema> result = await query
            .OrderByDynamic(filter.SortColumn, filter.SortDirection == ListSortDirection.Descending)
            .Skip(filter.PageNumber * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        return [.. result.Select(MapperT2E.Map)];
    }

    public async Task<int> GetSchemaCount(FilterSchemaCount filter)
    {
        IQueryable<TableSchema> query = Context.Schema.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(filter.Search)) query = query.Where(x => EF.Functions.Like(x.Name, $"%{filter.Search}%"));

        return await query.CountAsync();
    }

    public async Task<EntitySchema> CreateSchema(EntitySchema entity)
    {
        EntityEntry<TableSchema> result = await Context.Schema.AddAsync(MapperE2T.Map(entity));
        return MapperT2E.Map(result.Entity);
    }

    public void UpdateSchema(EntitySchema entity)
    {
        Context.Schema.Update(MapperE2T.Map(entity));
    }

    public async Task<EntitySchema> CloneSchema(EntitySchema entity)
    {
        TableSchema entitySchemaClone = MapperE2T.Map(entity).Clone();
        await Context.Schema.AddAsync(entitySchemaClone);

        List<TableSchemaElement> listElement = await Context.SchemaElement.AsNoTracking().Where(x => x.SchemaID == entity.ID).ToListAsync();
        List<TableSchemaElement> listElementClone = [.. listElement.Select(x => x.Clone(entitySchemaClone.ID))];
        await Context.SchemaElement.AddRangeAsync(listElementClone);

        List<TableSchemaContext> listContext = await Context.SchemaContext.AsNoTracking().Where(x => x.SchemaID == entity.ID).ToListAsync();
        List<TableSchemaContext> listContextClone = [.. listContext.Select(x => x.Clone(entitySchemaClone.ID))];
        await Context.SchemaContext.AddRangeAsync(listContextClone);

        List<TableSchemaParameter> listParameter = await Context.SchemaParameter.AsNoTracking().Where(x => x.SchemaID == entity.ID).ToListAsync();
        List<TableSchemaParameter> listParameterClone = [.. listParameter.Select(x => x.Clone(entitySchemaClone.ID))];
        await Context.SchemaParameter.AddRangeAsync(listParameterClone);

        List<TableSchemaProperty> listProperty = await Context.SchemaProperty.AsNoTracking().Where(x => x.SchemaID == entity.ID).ToListAsync();
        List<TableSchemaProperty> listPropertyClone = [.. listProperty.Select(x => x.Clone(entitySchemaClone.ID))];
        await Context.SchemaProperty.AddRangeAsync(listPropertyClone);

        Dictionary<Guid, Guid> mapElement = listElement.Zip(listElementClone, (old, clone) => (Old: old.ID, Clone: clone.ID)).ToDictionary(x => x.Old, x => x.Clone);
        Dictionary<Guid, Guid> mapContext = listContext.Zip(listContextClone, (old, clone) => (Old: old.ID, Clone: clone.ID)).ToDictionary(x => x.Old, x => x.Clone);
        Dictionary<Guid, Guid> mapParameter = listParameter.Zip(listParameterClone, (old, clone) => (Old: old.ID, Clone: clone.ID)).ToDictionary(x => x.Old, x => x.Clone);
        Dictionary<Guid, Guid> mapProperty = listProperty.Zip(listPropertyClone, (old, clone) => (Old: old.ID, Clone: clone.ID)).ToDictionary(x => x.Old, x => x.Clone);

        foreach (TableSchemaElement item in listElementClone)
        {
            item.TitleParameterTextID = GetClonedParameterID(mapParameter, item.TitleParameterTextID);
            if (item.DescriptionParameterTextID is not null) item.DescriptionParameterTextID = mapParameter.TryGetValue(item.DescriptionParameterTextID.Value, out Guid value) ? value : null;
        }

        foreach (TableSchemaContext item in listContextClone)
        {
            item.TitleParameterTextID = GetClonedParameterID(mapParameter, item.TitleParameterTextID);
            if (item.DescriptionParameterTextID is not null) item.DescriptionParameterTextID = mapParameter.TryGetValue(item.DescriptionParameterTextID.Value, out Guid value) ? value : null;
        }

        foreach (TableSchemaProperty item in listPropertyClone)
        {
            item.TitleParameterTextID = GetClonedParameterID(mapParameter, item.TitleParameterTextID);
            if (item.DescriptionParameterTextID is not null) item.DescriptionParameterTextID = mapParameter.TryGetValue(item.DescriptionParameterTextID.Value, out Guid value) ? value : null;
        }

        List<TableSchemaElementHasProperty> listJunctionElement = await Context.SchemaElementHasProperty.AsNoTracking().Where(x => mapElement.Keys.Contains(x.EntityID)).ToListAsync();
        await Context.SchemaElementHasProperty.AddRangeAsync(listJunctionElement.Select(j => j.Clone(mapElement[j.EntityID], mapProperty[j.RelationID])));

        List<TableSchemaContextHasElement> listJunctionContext = await Context.SchemaContextHasElement.AsNoTracking().Where(x => mapContext.Keys.Contains(x.EntityID)).ToListAsync();
        await Context.SchemaContextHasElement.AddRangeAsync(listJunctionContext.Select(j => j.Clone(mapContext[j.EntityID], mapElement[j.RelationID])));

        return MapperT2E.Map(entitySchemaClone);
    }

    public async void DeleteSchema(EntitySchema entity)
    {
        Context.Schema.Remove(MapperE2T.Map(entity));
    }

    public async Task DeactivateSchemaExcept(Guid id)
    {
        await Context.Schema.Where(x => x.ID != id && x.IsActive).ExecuteUpdateAsync(setters => setters.SetProperty(x => x.IsActive, false));
    }

    #endregion -- Schema --

    #region -- SchemaContext --

    public async Task<EntitySchemaContext> GetSchemaContextByID(Guid id)
    {
        TableSchemaContext result = await Context.SchemaContext
            .AsNoTracking()
            .Include(x => x.TitleParameterText)
            .Include(x => x.DescriptionParameterText)
            .SingleAsync(x => x.ID == id);
        return MapperT2E.Map(result);
    }

    public async Task<List<EntitySchemaContext>> GetSchemaContextList(FilterSchemaContextList filter)
    {
        IQueryable<TableSchemaContext> query = Context.SchemaContext.AsNoTracking().Where(x => x.SchemaID == filter.SchemaID);

        if (!string.IsNullOrWhiteSpace(filter.Search)) query = query.Where(x => EF.Functions.Like(x.Name, $"%{filter.Search}%"));

        List<TableSchemaContext> result = await query
            .Include(x => x.TitleParameterText)
            .Include(x => x.DescriptionParameterText)
            .OrderBy(x => x.Name)
            .Skip(filter.PageNumber * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        return [.. result.Select(MapperT2E.Map)];
    }

    public async Task<int> GetSchemaContextCount(FilterSchemaContextCount filter)
    {
        IQueryable<TableSchemaContext> query = Context.SchemaContext.AsNoTracking().Where(x => x.SchemaID == filter.SchemaID);

        if (!string.IsNullOrWhiteSpace(filter.Search)) query = query.Where(x => EF.Functions.Like(x.Name, $"%{filter.Search}%"));

        return await query.CountAsync();
    }

    public async Task<EntitySchemaContext> CreateSchemaContext(EntitySchemaContext entity)
    {
        EntityEntry<TableSchemaContext> result = await Context.SchemaContext.AddAsync(MapperE2T.Map(entity));
        return MapperT2E.Map(result.Entity);
    }

    public void UpdateSchemaContext(EntitySchemaContext entity)
    {
        Context.SchemaContext.Update(MapperE2T.Map(entity));
    }

    public async void DeleteSchemaContext(EntitySchemaContext entity)
    {
        Context.SchemaContext.Remove(MapperE2T.Map(entity));
    }

    #endregion -- SchemaContext --

    #region -- SchemaContextHasElement --

    public async Task<EntitySchemaContextHasElement> GetSchemaContextHasElementByID(Guid id)
    {
        TableSchemaContextHasElement result = await Context.SchemaContextHasElement
            .AsNoTracking()
            .Include(x => x.Entity)
            .Include(x => x.Relation)
            .SingleAsync(x => x.ID == id);
        return MapperT2E.Map(result);
    }

    public async Task<List<EntitySchemaContextHasElement>> GetSchemaContextHasElementList(FilterSchemaContextHasElementList filter)
    {
        IQueryable<TableSchemaContextHasElement> query = Context.SchemaContextHasElement.AsNoTracking();

        if (filter.SchemaContextID.HasValue) query = query.Where(x => x.EntityID == filter.SchemaContextID);
        if (filter.SchemaElementID.HasValue) query = query.Where(x => x.RelationID == filter.SchemaElementID);

        List<TableSchemaContextHasElement> result = await query.ToListAsync();

        return [.. result.Select(MapperT2E.Map)];
    }

    public async Task<EntitySchemaContextHasElement> CreateSchemaContextHasElement(EntitySchemaContextHasElement entity)
    {
        EntityEntry<TableSchemaContextHasElement> result = await Context.SchemaContextHasElement.AddAsync(MapperE2T.Map(entity));
        return MapperT2E.Map(result.Entity);
    }

    public void DeleteSchemaContextHasElement(EntitySchemaContextHasElement entity)
    {
        Context.SchemaContextHasElement.Remove(MapperE2T.Map(entity));
    }

    #endregion -- SchemaContextHasElement --

    #region -- SchemaElement --

    public async Task<EntitySchemaElement> GetSchemaElementByID(Guid id)
    {
        TableSchemaElement result = await Context.SchemaElement
            .AsNoTracking()
            .Include(x => x.TitleParameterText)
            .Include(x => x.DescriptionParameterText)
            .SingleAsync(x => x.ID == id);
        return MapperT2E.Map(result);
    }

    public async Task<List<EntitySchemaElement>> GetSchemaElementList(FilterSchemaElementList filter)
    {
        IQueryable<TableSchemaElement> query = Context.SchemaElement.AsNoTracking().Where(x => x.SchemaID == filter.SchemaID);

        if (!string.IsNullOrWhiteSpace(filter.Search)) query = query.Where(x => EF.Functions.Like(x.Name, $"%{filter.Search}%"));

        List<TableSchemaElement> result = await query
            .Include(x => x.TitleParameterText)
            .Include(x => x.DescriptionParameterText)
            .OrderBy(x => x.Name)
            .Skip(filter.PageNumber * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        return [.. result.Select(MapperT2E.Map)];
    }

    public async Task<int> GetSchemaElementCount(FilterSchemaElementCount filter)
    {
        IQueryable<TableSchemaElement> query = Context.SchemaElement.AsNoTracking().Where(x => x.SchemaID == filter.SchemaID);

        if (!string.IsNullOrWhiteSpace(filter.Search)) query = query.Where(x => EF.Functions.Like(x.Name, $"%{filter.Search}%"));

        return await query.CountAsync();
    }

    public async Task<EntitySchemaElement> CreateSchemaElement(EntitySchemaElement entity)
    {
        EntityEntry<TableSchemaElement> result = await Context.SchemaElement.AddAsync(MapperE2T.Map(entity));
        return MapperT2E.Map(result.Entity);
    }

    public void UpdateSchemaElement(EntitySchemaElement entity)
    {
        Context.SchemaElement.Update(MapperE2T.Map(entity));
    }

    public void DeleteSchemaElement(EntitySchemaElement entity)
    {
        Context.SchemaElement.Remove(MapperE2T.Map(entity));
    }

    #endregion -- SchemaElement --

    #region -- SchemaElementHasProperty --

    public async Task<EntitySchemaElementHasProperty> GetSchemaElementHasPropertyByID(Guid id)
    {
        TableSchemaElementHasProperty result = await Context.SchemaElementHasProperty
            .AsNoTracking()
            .Include(x => x.Entity)
            .Include(x => x.Relation)
            .SingleAsync(x => x.ID == id);
        return MapperT2E.Map(result);
    }

    public async Task<List<EntitySchemaElementHasProperty>> GetSchemaElementHasPropertyList(FilterSchemaElementHasPropertyList filter)
    {
        IQueryable<TableSchemaElementHasProperty> query = Context.SchemaElementHasProperty.AsNoTracking();

        if (filter.SchemaElementID.HasValue) query = query.Where(x => x.EntityID == filter.SchemaElementID);
        if (filter.SchemaPropertyID.HasValue) query = query.Where(x => x.RelationID == filter.SchemaPropertyID);

        List<TableSchemaElementHasProperty> result = await query.ToListAsync();

        return [.. result.Select(MapperT2E.Map)];
    }

    public async Task<EntitySchemaElementHasProperty> CreateSchemaElementHasProperty(EntitySchemaElementHasProperty entity)
    {
        EntityEntry<TableSchemaElementHasProperty> result = await Context.SchemaElementHasProperty.AddAsync(MapperE2T.Map(entity));
        return MapperT2E.Map(result.Entity);
    }

    public void DeleteSchemaElementHasProperty(EntitySchemaElementHasProperty entity)
    {
        Context.SchemaElementHasProperty.Remove(MapperE2T.Map(entity));
    }

    #endregion -- SchemaElementHasProperty --

    #region -- SchemaParameter --

    public async Task<EntitySchemaParameter.Discriminator> GetSchemaParameterByID(Guid id)
    {
        TableSchemaParameter result = await Context.SchemaParameter
            .AsNoTracking()
            .SingleAsync(x => x.ID == id);
        return MapperT2E.Map(result);
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

    public async Task<int> GetSchemaParameterCount(FilterSchemaParameterCount filter)
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

        return await query.CountAsync();
    }

    public async Task<EntitySchemaParameterStyle> CreateSchemaParameterStyle(EntitySchemaParameterStyle entity)
    {
        EntityEntry<TableSchemaParameterStyle> result = await Context.SchemaParameterStyle.AddAsync(MapperE2T.Map(entity));
        return MapperT2E.Map(result.Entity);
    }

    public async Task<EntitySchemaParameterSystem> CreateSchemaParameterSystem(EntitySchemaParameterSystem entity)
    {
        EntityEntry<TableSchemaParameterSystem> result = await Context.SchemaParameterSystem.AddAsync(MapperE2T.Map(entity));
        return MapperT2E.Map(result.Entity);
    }

    public async Task<EntitySchemaParameterText> CreateSchemaParameterText(EntitySchemaParameterText entity)
    {
        EntityEntry<TableSchemaParameterText> result = await Context.SchemaParameterText.AddAsync(MapperE2T.Map(entity));
        return MapperT2E.Map(result.Entity);
    }

    public void UpdateSchemaParameterStyle(EntitySchemaParameterStyle entity)
    {
        Context.SchemaParameterStyle.Update(MapperE2T.Map(entity));
    }

    public void UpdateSchemaParameterSystem(EntitySchemaParameterSystem entity)
    {
        Context.SchemaParameterSystem.Update(MapperE2T.Map(entity));
    }

    public void UpdateSchemaParameterText(EntitySchemaParameterText entity)
    {
        Context.SchemaParameterText.Update(MapperE2T.Map(entity));
    }

    public void DeleteSchemaParameter(EntitySchemaParameter entity)
    {
        Context.SchemaParameter.Remove(MapperE2T.Map(entity));
    }

    #endregion -- SchemaParameter --

    #region -- SchemaProperty --

    public async Task<EntitySchemaProperty.Discriminator> GetSchemaPropertyByID(Guid id)
    {
        TableSchemaProperty result = await Context.SchemaProperty
            .AsNoTracking()
            .Include(x => x.TitleParameterText)
            .Include(x => x.DescriptionParameterText)
            .SingleAsync(x => x.ID == id);
        return MapperT2E.Map(result);
    }

    public async Task<List<EntitySchemaProperty.Discriminator>> GetSchemaPropertyList(FilterSchemaPropertyList filter)
    {
        IQueryable<TableSchemaProperty> query = Context.SchemaProperty.AsNoTracking().Where(x => x.SchemaID == filter.SchemaID);

        if (filter.PropertyType is { Count: > 0 })
        {
            List<Type> types = [.. filter.PropertyType.Select(key => TableSchemaProperty.TypeMap[key])];

            ParameterExpression param = Expression.Parameter(typeof(TableSchemaProperty), "x");
            Expression typeChecks = types.Select(Expression (t) => Expression.TypeIs(param, t)).Aggregate(Expression.OrElse);
            Expression<Func<TableSchemaProperty, bool>> lambda = Expression.Lambda<Func<TableSchemaProperty, bool>>(typeChecks, param);
            query = query.Where(lambda);
        }

        if (!string.IsNullOrWhiteSpace(filter.Search)) query = query.Where(x => EF.Functions.Like(x.Name, $"%{filter.Search}%"));

        List<TableSchemaProperty> result = await query
            .Include(x => x.TitleParameterText)
            .Include(x => x.DescriptionParameterText)
            .OrderBy(x => x.Name)
            .Skip(filter.PageNumber * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        return [.. result.Select(MapperT2E.Map)];
    }

    public async Task<int> GetSchemaPropertyCount(FilterSchemaPropertyCount filter)
    {
        IQueryable<TableSchemaProperty> query = Context.SchemaProperty.AsNoTracking().Where(x => x.SchemaID == filter.SchemaID);

        if (filter.PropertyType is { Count: > 0 })
        {
            List<Type> types = [.. filter.PropertyType.Select(key => TableSchemaProperty.TypeMap[key])];

            ParameterExpression param = Expression.Parameter(typeof(TableSchemaProperty), "x");
            Expression typeChecks = types.Select(Expression (t) => Expression.TypeIs(param, t)).Aggregate(Expression.OrElse);
            Expression<Func<TableSchemaProperty, bool>> lambda = Expression.Lambda<Func<TableSchemaProperty, bool>>(typeChecks, param);
            query = query.Where(lambda);
        }

        if (!string.IsNullOrWhiteSpace(filter.Search)) query = query.Where(x => EF.Functions.Like(x.Name, $"%{filter.Search}%"));

        return await query.CountAsync();
    }

    public async Task<EntitySchemaPropertyBoolean> CreateSchemaPropertyBoolean(EntitySchemaPropertyBoolean entity)
    {
        EntityEntry<TableSchemaPropertyBoolean> result = await Context.SchemaPropertyBoolean.AddAsync(MapperE2T.Map(entity));
        return MapperT2E.Map(result.Entity);
    }

    public async Task<EntitySchemaPropertyDateTime> CreateSchemaPropertyDateTime(EntitySchemaPropertyDateTime entity)
    {
        EntityEntry<TableSchemaPropertyDateTime> result = await Context.SchemaPropertyDateTime.AddAsync(MapperE2T.Map(entity));
        return MapperT2E.Map(result.Entity);
    }

    public async Task<EntitySchemaPropertyDecimal> CreateSchemaPropertyDecimal(EntitySchemaPropertyDecimal entity)
    {
        EntityEntry<TableSchemaPropertyDecimal> result = await Context.SchemaPropertyDecimal.AddAsync(MapperE2T.Map(entity));
        return MapperT2E.Map(result.Entity);
    }

    public async Task<EntitySchemaPropertyImage> CreateSchemaPropertyImage(EntitySchemaPropertyImage entity)
    {
        EntityEntry<TableSchemaPropertyImage> result = await Context.SchemaPropertyImage.AddAsync(MapperE2T.Map(entity));
        return MapperT2E.Map(result.Entity);
    }

    public async Task<EntitySchemaPropertyInteger> CreateSchemaPropertyInteger(EntitySchemaPropertyInteger entity)
    {
        EntityEntry<TableSchemaPropertyInteger> result = await Context.SchemaPropertyInteger.AddAsync(MapperE2T.Map(entity));
        return MapperT2E.Map(result.Entity);
    }

    public async Task<EntitySchemaPropertyString> CreateSchemaPropertyString(EntitySchemaPropertyString entity)
    {
        EntityEntry<TableSchemaPropertyString> result = await Context.SchemaPropertyString.AddAsync(MapperE2T.Map(entity));
        return MapperT2E.Map(result.Entity);
    }

    public void UpdateSchemaPropertyBoolean(EntitySchemaPropertyBoolean entity)
    {
        Context.SchemaPropertyBoolean.Update(MapperE2T.Map(entity));
    }

    public void UpdateSchemaPropertyDateTime(EntitySchemaPropertyDateTime entity)
    {
        Context.SchemaPropertyDateTime.Update(MapperE2T.Map(entity));
    }

    public void UpdateSchemaPropertyDecimal(EntitySchemaPropertyDecimal entity)
    {
        Context.SchemaPropertyDecimal.Update(MapperE2T.Map(entity));
    }

    public void UpdateSchemaPropertyImage(EntitySchemaPropertyImage entity)
    {
        Context.SchemaPropertyImage.Update(MapperE2T.Map(entity));
    }

    public void UpdateSchemaPropertyInteger(EntitySchemaPropertyInteger entity)
    {
        Context.SchemaPropertyInteger.Update(MapperE2T.Map(entity));
    }

    public void UpdateSchemaPropertyString(EntitySchemaPropertyString entity)
    {
        Context.SchemaPropertyString.Update(MapperE2T.Map(entity));
    }

    public void DeleteSchemaProperty(EntitySchemaProperty entity)
    {
        Context.SchemaProperty.Remove(MapperE2T.Map(entity));
    }

    #endregion -- SchemaProperty --

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

    public async Task<List<EntitySchemaProperty.Discriminator>> GetSchemaPropertyListBySchemaID(Guid id)
    {
        List<TableSchemaProperty> result = await Context.SchemaProperty.AsNoTracking().Where(x => x.SchemaID == id).ToListAsync();
        return [.. result.Select(MapperT2E.Map)];
    }

    public async Task<List<EntitySchemaContextHasElement>> GetSchemaContextHasElementListBySchemaID(Guid id)
    {
        List<TableSchemaContextHasElement> result = await Context.SchemaContextHasElement.AsNoTracking().Where(x => x.Entity!.SchemaID == id).ToListAsync();
        return [.. result.Select(MapperT2E.Map)];
    }

    public async Task<List<EntitySchemaElementHasProperty>> GetSchemaElementHasPropertyListBySchemaID(Guid id)
    {
        List<TableSchemaElementHasProperty> result = await Context.SchemaElementHasProperty.AsNoTracking().Where(x => x.Entity!.SchemaID == id).ToListAsync();
        return [.. result.Select(MapperT2E.Map)];
    }

    private static Guid GetClonedParameterID(Dictionary<Guid, Guid> mapParameter, Guid parameterId)
    {
        return mapParameter.TryGetValue(parameterId, out Guid clonedParameterId)
            ? clonedParameterId
            : throw new InvalidOperationException($"Unable to clone schema because parameter '{parameterId}' was not found in the cloned parameter map.");
    }
}
