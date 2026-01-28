using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Noxy.NET.CaseManagement.Application.Interfaces.Repositories;
using Noxy.NET.CaseManagement.Domain.Entities.Schemas.Junctions;
using Noxy.NET.CaseManagement.Persistence.Abstractions;
using Noxy.NET.CaseManagement.Persistence.Abstractions.Tables;
using Noxy.NET.CaseManagement.Persistence.Interfaces.Services;
using Noxy.NET.CaseManagement.Persistence.Tables.Schemas;
using Noxy.NET.CaseManagement.Persistence.Tables.Schemas.Discriminators;
using Noxy.NET.CaseManagement.Persistence.Tables.Schemas.Junctions;

namespace Noxy.NET.CaseManagement.Persistence.Repositories;

public class JunctionRepository(DataContext context, IEntityToTableMapper mapperE2T, ITableToEntityMapper mapperT2E) : BaseRepository(context, mapperE2T, mapperT2E), IJunctionRepository
{
    public async Task ClearSchemaActionHasActionStepByEntityID(Guid id)
    {
        await Context.SchemaActionHasActionStep.Where(x => x.EntityID == id).ExecuteDeleteAsync();
    }

    public async Task<EntityJunctionSchemaActionStepHasActionInput> Create(EntityJunctionSchemaActionStepHasActionInput entity)
    {
        EntityEntry<TableJunctionSchemaActionStepHasActionInput> result = await Context.SchemaActionStepHasActionInput.AddAsync(MapperE2T.Map(entity));
        return MapperT2E.Map(result.Entity);
    }
    
    public async Task ClearSchemaActionStepHasActionInputByEntityID(Guid id)
    {
        await Context.SchemaActionStepHasActionInput.Where(x => x.EntityID == id).ExecuteDeleteAsync();
    }

    public async Task<EntityJunctionSchemaActionHasActionStep> Create(EntityJunctionSchemaActionHasActionStep entity)
    {
        EntityEntry<TableJunctionSchemaActionHasActionStep> result = await Context.SchemaActionHasActionStep.AddAsync(MapperE2T.Map(entity));
        return MapperT2E.Map(result.Entity);
    }

    public async Task ClearSchemaContextHasActionByEntityID(Guid id)
    {
        await Context.SchemaActionStepHasActionInput.Where(x => x.EntityID == id).ExecuteDeleteAsync();
    }

    public async Task<EntityJunctionSchemaContextHasAction> Create(EntityJunctionSchemaContextHasAction entity)
    {
        EntityEntry<TableJunctionSchemaContextHasAction> result = await Context.SchemaContextHasAction.AddAsync(MapperE2T.Map(entity));
        return MapperT2E.Map(result.Entity);
    }

    public async Task ClearSchemaContextHasElementByEntityID(Guid id)
    {
        await Context.SchemaActionStepHasActionInput.Where(x => x.EntityID == id).ExecuteDeleteAsync();
    }

    public async Task<EntityJunctionSchemaContextHasElement> Create(EntityJunctionSchemaContextHasElement entity)
    {
        EntityEntry<TableJunctionSchemaContextHasElement> result = await Context.SchemaContextHasElement.AddAsync(MapperE2T.Map(entity));
        return MapperT2E.Map(result.Entity);
    }

    public async Task ClearSchemaElementHasPropertyByEntityID(Guid id)
    {
        await Context.SchemaActionStepHasActionInput.Where(x => x.EntityID == id).ExecuteDeleteAsync();
    }

    public async Task<EntityJunctionSchemaElementHasProperty> Create(EntityJunctionSchemaElementHasProperty entity)
    {
        EntityEntry<TableJunctionSchemaElementHasProperty> result = await Context.SchemaElementHasProperty.AddAsync(MapperE2T.Map(entity));
        return MapperT2E.Map(result.Entity);
    }

    public async Task<List<EntityJunctionSchemaElementHasProperty>> RelateElementToPropertyList(Guid entityGuid, IEnumerable<Guid> listGuid)
    {
        List<TableJunctionSchemaElementHasProperty> list = await Relate<TableJunctionSchemaElementHasProperty, TableSchemaElement, TableSchemaProperty>(entityGuid, listGuid, (x, y) => new() { EntityID = x, RelationID = y, Order = 0 });
        return list.Select(MapperT2E.Map).ToList();
    }

    public async Task<List<EntityJunctionSchemaActionHasActionStep>> RelateActionToActionStepList(Guid entityGuid, IEnumerable<Guid> listGuid)
    {
        List<TableJunctionSchemaActionHasActionStep> list = await Relate<TableJunctionSchemaActionHasActionStep, TableSchemaAction, TableSchemaActionStep>(entityGuid, listGuid, (x, y) => new() { EntityID = x, RelationID = y, Order = 0 });
        return list.Select(MapperT2E.Map).ToList();
    }

    public async Task<List<EntityJunctionSchemaActionHasDynamicValueCode>> RelateActionToDynamicValueCode(Guid entityGuid, IEnumerable<Guid> listGuid)
    {
        List<TableJunctionSchemaActionHasDynamicValueCode> list = await Relate<TableJunctionSchemaActionHasDynamicValueCode, TableSchemaAction, TableSchemaDynamicValueCode>(entityGuid, listGuid, (x, y) => new() { EntityID = x, RelationID = y, Order = 0 });
        return list.Select(MapperT2E.Map).ToList();
    }

    public async Task<List<EntityJunctionSchemaActionStepHasActionInput>> RelateActionStepToActionInputList(Guid entityGuid, IEnumerable<Guid> listGuid)
    {
        List<TableJunctionSchemaActionStepHasActionInput> list = await Relate<TableJunctionSchemaActionStepHasActionInput, TableSchemaActionStep, TableSchemaActionInput>(entityGuid, listGuid, (x, y) => new() { EntityID = x, RelationID = y, Order = 0 });
        return list.Select(MapperT2E.Map).ToList();
    }

    public async Task<List<EntityJunctionSchemaContextHasAction>> RelateContextToAction(Guid entityGuid, IEnumerable<Guid> listGuid)
    {
        List<TableJunctionSchemaContextHasAction> list = await Relate<TableJunctionSchemaContextHasAction, TableSchemaContext, TableSchemaAction>(entityGuid, listGuid, (x, y) => new() { EntityID = x, RelationID = y, Order = 0 });
        return list.Select(MapperT2E.Map).ToList();
    }

    public async Task<List<EntityJunctionSchemaContextHasElement>> RelateContextToElement(Guid entityGuid, IEnumerable<Guid> listGuid)
    {
        List<TableJunctionSchemaContextHasElement> list = await Relate<TableJunctionSchemaContextHasElement, TableSchemaContext, TableSchemaElement>(entityGuid, listGuid, (x, y) => new() { EntityID = x, RelationID = y, Order = 0 });
        return list.Select(MapperT2E.Map).ToList();
    }

    private async Task<List<TJunction>> Relate<TJunction, TEntity, TRelation>(Guid entityGuid, IEnumerable<Guid> listGuid, Func<Guid, Guid, TJunction> callback) where TJunction : BaseTableManyToMany<TEntity, TRelation>
    {
        List<TJunction> list = listGuid.Select(x => callback(entityGuid, x)).ToList();
        List<TJunction> result = await Context.Set<TJunction>().AsNoTracking().Where(x => x.EntityID == entityGuid).ToListAsync();

        List<TJunction> toRemove = result.Where(item => list.All(x => x.RelationID != item.RelationID)).ToList();
        List<TJunction> toAdd = list
            .Where(item => result.All(x => x.RelationID != item.RelationID))
            .Select(item => callback(entityGuid, item.RelationID))
            .ToList();

        await Context.AddRangeAsync(toAdd);
        Context.RemoveRange(toRemove);

        return list;
    }
}
