using System.ComponentModel;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Noxy.NET.EntityManagement.Application.Interfaces.Repositories;
using Noxy.NET.EntityManagement.Application.Interfaces.Services;
using Noxy.NET.EntityManagement.Application.Models;
using Noxy.NET.EntityManagement.Domain.Entities.Schemas;
using Noxy.NET.EntityManagement.Domain.Entities.Schemas.Discriminators;
using Noxy.NET.EntityManagement.Domain.Entities.Schemas.Junctions;
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

    public async Task<EntitySchema> UpdateSchema(EntitySchema entity)
    {
        TableSchema result = await Context.Schema.AsNoTracking().FirstAsync(x => x.ID == entity.ID);

        result.Name = entity.Name;
        result.Note = entity.Note;
        result.TimeUpdated = DateTime.UtcNow;
        Context.Schema.Update(result);

        return MapperT2E.Map(result);
    }

    public async Task<EntitySchema> CloneSchema(Guid id)
    {
        TableSchema entitySchema = await Context.Schema.AsNoTracking().FirstAsync(x => x.ID == id);
        TableSchema entitySchemaClone = entitySchema.Clone();
        await Context.Schema.AddAsync(entitySchemaClone);

        List<TableSchemaElement> listElement = await Context.SchemaElement.AsNoTracking().Where(x => x.SchemaID == id).ToListAsync();
        List<TableSchemaElement> listElementClone = [.. listElement.Select(x => x.Clone(entitySchemaClone.ID))];
        await Context.SchemaElement.AddRangeAsync(listElementClone);

        List<TableSchemaContext> listContext = await Context.SchemaContext.AsNoTracking().Where(x => x.SchemaID == id).ToListAsync();
        List<TableSchemaContext> listContextClone = [.. listContext.Select(x => x.Clone(entitySchemaClone.ID))];
        await Context.SchemaContext.AddRangeAsync(listContextClone);

        List<TableSchemaParameter> listParameter = await Context.SchemaParameter.AsNoTracking().Where(x => x.SchemaID == id).ToListAsync();
        List<TableSchemaParameter> listParameterClone = [.. listParameter.Select(x => x.Clone(entitySchemaClone.ID))];
        await Context.SchemaParameter.AddRangeAsync(listParameterClone);

        List<TableSchemaProperty> listProperty = await Context.SchemaProperty.AsNoTracking().Where(x => x.SchemaID == id).ToListAsync();
        List<TableSchemaProperty> listPropertyClone = [.. listProperty.Select(x => x.Clone(entitySchemaClone.ID))];
        await Context.SchemaProperty.AddRangeAsync(listPropertyClone);

        Dictionary<Guid, Guid> mapElement = listElement.Zip(listElementClone, (old, clone) => (Old: old.ID, Clone: clone.ID)).ToDictionary(x => x.Old, x => x.Clone);
        Dictionary<Guid, Guid> mapContext = listContext.Zip(listContextClone, (old, clone) => (Old: old.ID, Clone: clone.ID)).ToDictionary(x => x.Old, x => x.Clone);
        Dictionary<Guid, Guid> mapParameter = listParameter.Zip(listParameterClone, (old, clone) => (Old: old.ID, Clone: clone.ID)).ToDictionary(x => x.Old, x => x.Clone);
        Dictionary<Guid, Guid> mapProperty = listProperty.Zip(listPropertyClone, (old, clone) => (Old: old.ID, Clone: clone.ID)).ToDictionary(x => x.Old, x => x.Clone);

        foreach (TableSchemaElement element in listElementClone)
        {
            element.TitleTextParameterID = GetClonedParameterID(mapParameter, element.TitleTextParameterID);
            if (element.DescriptionTextParameterID is not null) element.DescriptionTextParameterID = mapParameter[element.DescriptionTextParameterID.Value];
        }

        foreach (TableSchemaContext context in listContextClone)
        {
            context.TitleTextParameterID = GetClonedParameterID(mapParameter, context.TitleTextParameterID);
            if (context.DescriptionTextParameterID is not null) context.DescriptionTextParameterID = mapParameter[context.DescriptionTextParameterID.Value];
        }

        foreach (TableSchemaProperty property in listPropertyClone)
        {
            property.TitleTextParameterID = GetClonedParameterID(mapParameter, property.TitleTextParameterID);
            if (property.DescriptionTextParameterID is not null) property.DescriptionTextParameterID = mapParameter[property.DescriptionTextParameterID.Value];
        }

        List<TableSchemaElementHasProperty> listJunctionElement = await Context.SchemaElementHasProperty.AsNoTracking().Where(x => mapElement.Keys.Contains(x.EntityID)).ToListAsync();
        await Context.SchemaElementHasProperty.AddRangeAsync(listJunctionElement.Select(j => j.Clone(mapElement[j.EntityID], mapProperty[j.RelationID])));

        List<TableSchemaContextHasElement> listJunctionContext = await Context.SchemaContextHasElement.AsNoTracking().Where(x => mapContext.Keys.Contains(x.EntityID)).ToListAsync();
        await Context.SchemaContextHasElement.AddRangeAsync(listJunctionContext.Select(j => j.Clone(mapContext[j.EntityID], mapElement[j.RelationID])));

        return MapperT2E.Map(entitySchemaClone);
    }

    public async Task<Guid> DeleteSchema(Guid id)
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

    public async Task<EntitySchema> ActivateSchema(Guid id)
    {
        TableSchema entity = await Context.Schema.AsNoTracking().FirstAsync(x => x.ID == id);
        if (entity.IsActive) throw new InvalidOperationException("Cannot activate schema that is already active.");

        await Context.Schema.Where(x => x.ID != id && x.IsActive).ExecuteUpdateAsync(setters => setters.SetProperty(x => x.IsActive, false));

        entity.IsActive = true;
        entity.TimeActivated = DateTime.UtcNow;
        Context.Schema.Update(entity);

        return MapperT2E.Map(entity);
    }

    #endregion -- Schema --

    #region -- SchemaContext --

    public async Task<EntitySchemaContext> GetSchemaContextByID(Guid id)
    {
        TableSchemaContext result = await Context.SchemaContext
            .AsNoTracking()
            .Include(x => x.TitleTextParameter)
            .Include(x => x.DescriptionTextParameter)
            .SingleAsync(x => x.ID == id);
        return MapperT2E.Map(result);
    }

    public async Task<List<EntitySchemaContext>> GetSchemaContextList(FilterSchemaContextList filter)
    {
        IQueryable<TableSchemaContext> query = Context.SchemaContext.AsNoTracking().Where(x => x.SchemaID == filter.SchemaID);

        if (!string.IsNullOrWhiteSpace(filter.Search)) query = query.Where(x => EF.Functions.Like(x.Name, $"%{filter.Search}%"));

        List<TableSchemaContext> result = await query
            .Include(x => x.TitleTextParameter)
            .Include(x => x.DescriptionTextParameter)
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
        await ThrowIfSchemaActivated(entity.SchemaID);
        EntityEntry<TableSchemaContext> result = await Context.SchemaContext.AddAsync(MapperE2T.Map(entity));
        return MapperT2E.Map(result.Entity);
    }

    public async Task<EntitySchemaContext> UpdateSchemaContext(EntitySchemaContext entity)
    {
        TableSchemaContext result = await Context.SchemaContext.AsNoTracking().FirstAsync(x => x.ID == entity.ID);
        await ThrowIfSchemaActivated(result.SchemaID);

        result.SchemaIdentifier = entity.SchemaIdentifier;
        result.Name = entity.Name;
        result.Note = entity.Note;
        result.TitleTextParameterID = entity.TitleTextParameterID;
        result.DescriptionTextParameterID = entity.DescriptionTextParameterID;
        result.TimeUpdated = DateTime.UtcNow;
        Context.SchemaContext.Update(result);

        return MapperT2E.Map(result);
    }

    public async Task<EntitySchemaContext> CloneSchemaContext(Guid id)
    {
        TableSchemaContext entity = await Context.SchemaContext.AsNoTracking().FirstAsync(x => x.ID == id);
        await ThrowIfSchemaActivated(entity.SchemaID);

        entity = entity.Clone();
        entity.SchemaIdentifier = Guid.NewGuid().ToString("N");
        await Context.SchemaContext.AddAsync(entity);

        return MapperT2E.Map(entity);
    }

    public async Task<Guid> DeleteSchemaContext(Guid id)
    {
        TableSchemaContext entity = await Context.SchemaContext.AsNoTracking().FirstAsync(x => x.ID == id);
        await ThrowIfSchemaActivated(entity.SchemaID);

        Context.SchemaContext.Remove(entity);

        return entity.ID;
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
        TableSchemaContext entityContext = await Context.SchemaContext.AsNoTracking().FirstAsync(x => x.ID == entity.EntityID);
        TableSchemaElement entityElement = await Context.SchemaElement.AsNoTracking().FirstAsync(x => x.ID == entity.RelationID);
        if (entityContext.SchemaID != entityElement.SchemaID) throw new InvalidOperationException("SchemaElement and SchemaContext must be in same schema.");

        await ThrowIfSchemaActivated(entityContext.SchemaID);

        EntityEntry<TableSchemaContextHasElement> result = await Context.SchemaContextHasElement.AddAsync(MapperE2T.Map(entity));

        return MapperT2E.Map(result.Entity);
    }

    public async Task<Guid> DeleteSchemaContextHasElement(Guid id)
    {
        bool schemaActivated = await Context.SchemaContextHasElement
            .AsNoTracking()
            .AnyAsync(x => x.ID == id && x.Entity!.Schema!.TimeActivated != null);
        if (schemaActivated) throw new InvalidOperationException("Cannot delete schema element from schema that has been activated.");

        await Context.SchemaContextHasElement.Where(x => x.ID == id).ExecuteDeleteAsync();

        return id;
    }

    #endregion -- SchemaContextHasElement --

    #region -- SchemaElement --

    public async Task<EntitySchemaElement> GetSchemaElementByID(Guid id)
    {
        TableSchemaElement result = await Context.SchemaElement
            .AsNoTracking()
            .Include(x => x.TitleTextParameter)
            .Include(x => x.DescriptionTextParameter)
            .SingleAsync(x => x.ID == id);
        return MapperT2E.Map(result);
    }

    public async Task<List<EntitySchemaElement>> GetSchemaElementList(FilterSchemaElementList filter)
    {
        IQueryable<TableSchemaElement> query = Context.SchemaElement.AsNoTracking().Where(x => x.SchemaID == filter.SchemaID);

        if (!string.IsNullOrWhiteSpace(filter.Search)) query = query.Where(x => EF.Functions.Like(x.Name, $"%{filter.Search}%"));

        List<TableSchemaElement> result = await query
            .Include(x => x.TitleTextParameter)
            .Include(x => x.DescriptionTextParameter)
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
        await ThrowIfSchemaActivated(entity.SchemaID);
        EntityEntry<TableSchemaElement> result = await Context.SchemaElement.AddAsync(MapperE2T.Map(entity));
        return MapperT2E.Map(result.Entity);
    }

    public async Task<EntitySchemaElement> UpdateSchemaElement(EntitySchemaElement entity)
    {
        TableSchemaElement result = await Context.SchemaElement.AsNoTracking().FirstAsync(x => x.ID == entity.ID);
        await ThrowIfSchemaActivated(result.SchemaID);

        result.SchemaIdentifier = entity.SchemaIdentifier;
        result.Name = entity.Name;
        result.Note = entity.Note;
        result.Weight = entity.Weight;
        result.TitleTextParameterID = entity.TitleTextParameterID;
        result.DescriptionTextParameterID = entity.DescriptionTextParameterID;
        result.TimeUpdated = DateTime.UtcNow;
        Context.SchemaElement.Update(result);

        return MapperT2E.Map(result);
    }

    public async Task<EntitySchemaElement> CloneSchemaElement(Guid id)
    {
        TableSchemaElement entity = await Context.SchemaElement.AsNoTracking().FirstAsync(x => x.ID == id);
        await ThrowIfSchemaActivated(entity.SchemaID);

        entity = entity.Clone();
        entity.SchemaIdentifier = Guid.NewGuid().ToString("N");
        await Context.SchemaElement.AddAsync(entity);

        return MapperT2E.Map(entity);
    }

    public async Task<Guid> DeleteSchemaElement(Guid id)
    {
        TableSchemaElement entity = await Context.SchemaElement.AsNoTracking().FirstAsync(x => x.ID == id);
        await ThrowIfSchemaActivated(entity.SchemaID);

        Context.SchemaElement.Remove(entity);

        return entity.ID;
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
        TableSchemaElement entityElement = await Context.SchemaElement.AsNoTracking().FirstAsync(x => x.ID == entity.EntityID);
        TableSchemaProperty entityProperty = await Context.SchemaProperty.AsNoTracking().FirstAsync(x => x.ID == entity.RelationID);
        if (entityElement.SchemaID != entityProperty.SchemaID) throw new InvalidOperationException("Entities must be in same schema.");

        await ThrowIfSchemaActivated(entityElement.SchemaID);

        EntityEntry<TableSchemaElementHasProperty> result = await Context.SchemaElementHasProperty.AddAsync(MapperE2T.Map(entity));

        return MapperT2E.Map(result.Entity);
    }

    public async Task<Guid> DeleteSchemaElementHasProperty(Guid id)
    {
        bool schemaActivated = await Context.SchemaElementHasProperty
            .AsNoTracking()
            .Where(x => x.ID == id)
            .AnyAsync(x => x.Entity!.Schema!.TimeActivated != null);
        if (schemaActivated) throw new InvalidOperationException("Cannot delete an entity from a schema that has been activated.");

        await Context.SchemaElementHasProperty.Where(x => x.ID == id).ExecuteDeleteAsync();

        return id;
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
        await ThrowIfSchemaActivated(entity.SchemaID);
        EntityEntry<TableSchemaParameterStyle> result = await Context.SchemaParameterStyle.AddAsync(MapperE2T.Map(entity));
        return MapperT2E.Map(result.Entity);
    }

    public async Task<EntitySchemaParameterSystem> CreateSchemaParameterSystem(EntitySchemaParameterSystem entity)
    {
        await ThrowIfSchemaActivated(entity.SchemaID);
        EntityEntry<TableSchemaParameterSystem> result = await Context.SchemaParameterSystem.AddAsync(MapperE2T.Map(entity));
        return MapperT2E.Map(result.Entity);
    }

    public async Task<EntitySchemaParameterText> CreateSchemaParameterText(EntitySchemaParameterText entity)
    {
        await ThrowIfSchemaActivated(entity.SchemaID);
        EntityEntry<TableSchemaParameterText> result = await Context.SchemaParameterText.AddAsync(MapperE2T.Map(entity));
        return MapperT2E.Map(result.Entity);
    }

    public async Task<EntitySchemaParameterStyle> UpdateSchemaParameterStyle(EntitySchemaParameterStyle entity)
    {
        TableSchemaParameterStyle result = await Context.SchemaParameterStyle.AsNoTracking().FirstAsync(x => x.ID == entity.ID);
        await ThrowIfSchemaActivated(result.SchemaID);

        AssignSchemaParameter(result, entity);
        Context.SchemaParameter.Update(result);

        return MapperT2E.Map(result);
    }

    public async Task<EntitySchemaParameterSystem> UpdateSchemaParameterSystem(EntitySchemaParameterSystem entity)
    {
        TableSchemaParameterSystem result = await Context.SchemaParameterSystem.AsNoTracking().FirstAsync(x => x.ID == entity.ID);
        await ThrowIfSchemaActivated(result.SchemaID);

        AssignSchemaParameter(result, entity);
        Context.SchemaParameter.Update(result);

        return MapperT2E.Map(result);
    }

    public async Task<EntitySchemaParameterText> UpdateSchemaParameterText(EntitySchemaParameterText entity)
    {
        TableSchemaParameterText result = await Context.SchemaParameterText.AsNoTracking().FirstAsync(x => x.ID == entity.ID);
        await ThrowIfSchemaActivated(result.SchemaID);

        AssignSchemaParameter(result, entity);
        result.Type = entity.Type;
        Context.SchemaParameter.Update(result);

        return MapperT2E.Map(result);
    }

    public async Task<EntitySchemaParameter.Discriminator> CloneSchemaParameter(Guid id)
    {
        TableSchemaParameter entity = await Context.SchemaParameter.AsNoTracking().FirstAsync(x => x.ID == id);
        await ThrowIfSchemaActivated(entity.SchemaID);

        entity = entity.Clone();
        entity.SchemaIdentifier = Guid.NewGuid().ToString("N");
        await Context.SchemaParameter.AddAsync(entity);

        return MapperT2E.Map(entity);
    }

    public async Task<Guid> DeleteSchemaParameter(Guid id)
    {
        TableSchemaParameter entity = await Context.SchemaParameter.AsNoTracking().FirstAsync(x => x.ID == id);
        await ThrowIfSchemaActivated(entity.SchemaID);

        Context.SchemaParameter.Remove(entity);

        return entity.ID;
    }

    #endregion -- SchemaParameter --

    #region -- SchemaProperty --

    public async Task<EntitySchemaProperty.Discriminator> GetSchemaPropertyByID(Guid id)
    {
        TableSchemaProperty result = await Context.SchemaProperty
            .AsNoTracking()
            .Include(x => x.TitleTextParameter)
            .Include(x => x.DescriptionTextParameter)
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
            .Include(x => x.TitleTextParameter)
            .Include(x => x.DescriptionTextParameter)
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
        await ThrowIfSchemaActivated(entity.SchemaID);
        EntityEntry<TableSchemaPropertyBoolean> result = await Context.SchemaPropertyBoolean.AddAsync(MapperE2T.Map(entity));
        return MapperT2E.Map(result.Entity);
    }

    public async Task<EntitySchemaPropertyDateTime> CreateSchemaPropertyDateTime(EntitySchemaPropertyDateTime entity)
    {
        await ThrowIfSchemaActivated(entity.SchemaID);
        EntityEntry<TableSchemaPropertyDateTime> result = await Context.SchemaPropertyDateTime.AddAsync(MapperE2T.Map(entity));
        return MapperT2E.Map(result.Entity);
    }

    public async Task<EntitySchemaPropertyDecimal> CreateSchemaPropertyDecimal(EntitySchemaPropertyDecimal entity)
    {
        await ThrowIfSchemaActivated(entity.SchemaID);
        EntityEntry<TableSchemaPropertyDecimal> result = await Context.SchemaPropertyDecimal.AddAsync(MapperE2T.Map(entity));
        return MapperT2E.Map(result.Entity);
    }

    public async Task<EntitySchemaPropertyImage> CreateSchemaPropertyImage(EntitySchemaPropertyImage entity)
    {
        await ThrowIfSchemaActivated(entity.SchemaID);
        EntityEntry<TableSchemaPropertyImage> result = await Context.SchemaPropertyImage.AddAsync(MapperE2T.Map(entity));
        return MapperT2E.Map(result.Entity);
    }

    public async Task<EntitySchemaPropertyInteger> CreateSchemaPropertyInteger(EntitySchemaPropertyInteger entity)
    {
        await ThrowIfSchemaActivated(entity.SchemaID);
        EntityEntry<TableSchemaPropertyInteger> result = await Context.SchemaPropertyInteger.AddAsync(MapperE2T.Map(entity));
        return MapperT2E.Map(result.Entity);
    }

    public async Task<EntitySchemaPropertyString> CreateSchemaPropertyString(EntitySchemaPropertyString entity)
    {
        await ThrowIfSchemaActivated(entity.SchemaID);
        EntityEntry<TableSchemaPropertyString> result = await Context.SchemaPropertyString.AddAsync(MapperE2T.Map(entity));
        return MapperT2E.Map(result.Entity);
    }

    public async Task<EntitySchemaPropertyBoolean> UpdateSchemaPropertyBoolean(EntitySchemaPropertyBoolean entity)
    {
        TableSchemaPropertyBoolean result = await Context.SchemaPropertyBoolean.AsNoTracking().FirstAsync(x => x.ID == entity.ID);
        await ThrowIfSchemaActivated(result.SchemaID);

        AssignSchemaProperty(result, entity);
        Context.SchemaProperty.Update(result);

        return MapperT2E.Map(result);
    }

    public async Task<EntitySchemaPropertyDateTime> UpdateSchemaPropertyDateTime(EntitySchemaPropertyDateTime entity)
    {
        TableSchemaPropertyDateTime result = await Context.SchemaPropertyDateTime.AsNoTracking().FirstAsync(x => x.ID == entity.ID);
        await ThrowIfSchemaActivated(result.SchemaID);

        AssignSchemaProperty(result, entity);
        result.Type = entity.Type;
        Context.SchemaProperty.Update(result);

        return MapperT2E.Map(result);
    }

    public async Task<EntitySchemaPropertyDecimal> UpdateSchemaPropertyDecimal(EntitySchemaPropertyDecimal entity)
    {
        TableSchemaPropertyDecimal result = await Context.SchemaPropertyDecimal.AsNoTracking().FirstAsync(x => x.ID == entity.ID);
        await ThrowIfSchemaActivated(result.SchemaID);

        AssignSchemaProperty(result, entity);
        Context.SchemaProperty.Update(result);

        return MapperT2E.Map(result);
    }

    public async Task<EntitySchemaPropertyImage> UpdateSchemaPropertyImage(EntitySchemaPropertyImage entity)
    {
        TableSchemaPropertyImage result = await Context.SchemaPropertyImage.AsNoTracking().FirstAsync(x => x.ID == entity.ID);
        await ThrowIfSchemaActivated(result.SchemaID);

        AssignSchemaProperty(result, entity);
        Context.SchemaProperty.Update(result);

        return MapperT2E.Map(result);
    }

    public async Task<EntitySchemaPropertyInteger> UpdateSchemaPropertyInteger(EntitySchemaPropertyInteger entity)
    {
        TableSchemaPropertyInteger result = await Context.SchemaPropertyInteger.AsNoTracking().FirstAsync(x => x.ID == entity.ID);
        await ThrowIfSchemaActivated(result.SchemaID);

        AssignSchemaProperty(result, entity);
        Context.SchemaProperty.Update(result);

        return MapperT2E.Map(result);
    }

    public async Task<EntitySchemaPropertyString> UpdateSchemaPropertyString(EntitySchemaPropertyString entity)
    {
        TableSchemaPropertyString result = await Context.SchemaPropertyString.AsNoTracking().FirstAsync(x => x.ID == entity.ID);
        await ThrowIfSchemaActivated(result.SchemaID);

        AssignSchemaProperty(result, entity);
        Context.SchemaProperty.Update(result);

        return MapperT2E.Map(result);
    }

    public async Task<EntitySchemaProperty.Discriminator> CloneSchemaProperty(Guid id)
    {
        TableSchemaProperty entity = await Context.SchemaProperty.AsNoTracking().FirstAsync(x => x.ID == id);
        await ThrowIfSchemaActivated(entity.SchemaID);

        entity = entity.Clone();
        entity.SchemaIdentifier = Guid.NewGuid().ToString("N");
        await Context.SchemaProperty.AddAsync(entity);

        return MapperT2E.Map(entity);
    }

    public async Task<Guid> DeleteSchemaProperty(Guid id)
    {
        TableSchemaProperty entity = await Context.SchemaProperty.AsNoTracking().FirstAsync(x => x.ID == id);
        await ThrowIfSchemaActivated(entity.SchemaID);

        Context.SchemaProperty.Remove(entity);

        return entity.ID;
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

    private async Task ThrowIfSchemaActivated(Guid schemaId)
    {
        bool isActivated = await Context.Schema
            .AsNoTracking()
            .AnyAsync(x => x.ID == schemaId && x.TimeActivated != null);

        if (isActivated) throw new InvalidOperationException("Cannot delete from a schema that has been activated.");
    }

    private static void AssignSchemaParameter(TableSchemaParameter table, EntitySchemaParameter entity)
    {
        table.SchemaIdentifier = entity.SchemaIdentifier;
        table.Name = entity.Name;
        table.Note = entity.Note;
        table.IsApprovalRequired = entity.IsApprovalRequired;
        table.IsSystemDefined = entity.IsSystemDefined;
        table.TimeUpdated = DateTime.UtcNow;
    }

    private static void AssignSchemaProperty(TableSchemaProperty table, EntitySchemaProperty entity)
    {
        table.SchemaIdentifier = entity.SchemaIdentifier;
        table.Name = entity.Name;
        table.Note = entity.Note;
        table.Weight = entity.Weight;
        table.TitleTextParameterID = entity.TitleTextParameterID;
        table.DescriptionTextParameterID = entity.DescriptionTextParameterID;
        table.TimeUpdated = DateTime.UtcNow;
    }
}
