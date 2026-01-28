using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Noxy.NET.CaseManagement.Application.Interfaces.Repositories;
using Noxy.NET.CaseManagement.Domain.Abstractions.Entities;
using Noxy.NET.CaseManagement.Domain.Entities.Schemas;
using Noxy.NET.CaseManagement.Domain.Entities.Schemas.Associations;
using Noxy.NET.CaseManagement.Domain.Entities.Schemas.Discriminators;
using Noxy.NET.CaseManagement.Domain.Entities.Schemas.Junctions;
using Noxy.NET.CaseManagement.Persistence.Abstractions;
using Noxy.NET.CaseManagement.Persistence.Abstractions.Tables;
using Noxy.NET.CaseManagement.Persistence.Interfaces.Services;
using Noxy.NET.CaseManagement.Persistence.Tables.Schemas;
using Noxy.NET.CaseManagement.Persistence.Tables.Schemas.Discriminators;
using Noxy.NET.CaseManagement.Persistence.Tables.Schemas.Junctions;

namespace Noxy.NET.CaseManagement.Persistence.Repositories;

public class SchemaRepository(DataContext context, IEntityToTableMapper mapperE2T, ITableToEntityMapper mapperT2E) : BaseRepository(context, mapperE2T, mapperT2E), ISchemaRepository
{
    public async Task<EntitySchemaAction> GetSchemaActionByID(Guid id)
    {
        return MapperT2E.Map(await Context.SchemaAction.AsNoTracking().SingleAsync(x => x.ID == id));
    }

    public async Task<EntitySchemaActionInput> GetSchemaActionInputByID(Guid id)
    {
        return MapperT2E.Map(await Context.SchemaActionInput.AsNoTracking().SingleAsync(x => x.ID == id));
    }

    public async Task<EntitySchemaActionStep> GetSchemaActionStepByID(Guid id)
    {
        return MapperT2E.Map(await Context.SchemaActionStep.AsNoTracking().SingleAsync(x => x.ID == id));
    }

    public async Task<EntitySchemaAttribute> GetSchemaAttributeByID(Guid id)
    {
        return MapperT2E.Map(await Context.SchemaAttribute.AsNoTracking().SingleAsync(x => x.ID == id));
    }

    public async Task<EntitySchemaContext> GetSchemaContextByID(Guid id)
    {
        return MapperT2E.Map(await Context.SchemaContext.AsNoTracking().SingleAsync(x => x.ID == id));
    }

    public async Task<EntitySchemaDynamicValue.Discriminator> GetSchemaDynamicValueByID(Guid id)
    {
        return MapperT2E.Map(await Context.SchemaDynamicValue.AsNoTracking().SingleAsync(x => x.ID == id));
    }

    public async Task<EntitySchemaElement> GetSchemaElementByID(Guid id)
    {
        return MapperT2E.Map(await Context.SchemaElement.AsNoTracking().SingleAsync(x => x.ID == id));
    }

    public async Task<EntitySchemaProperty.Discriminator> GetSchemaPropertyByID(Guid id)
    {
        return MapperT2E.Map(await Context.SchemaProperty.AsNoTracking().SingleAsync(x => x.ID == id));
    }

    public async Task<EntitySchemaInput> GetSchemaInputByID(Guid id)
    {
        return MapperT2E.Map(await Context.SchemaInput.AsNoTracking().SingleAsync(x => x.ID == id));
    }

    public async Task<List<EntitySchemaAction>> GetSchemaActionListBySchemaID(Guid id)
    {
        List<TableSchemaAction> result = await Context.SchemaAction.AsNoTracking().Where(x => x.SchemaID == id).ToListAsync();
        return result.Select(MapperT2E.Map).ToList();
    }

    public async Task<List<EntitySchemaActionInput>> GetSchemaActionInputListBySchemaID(Guid id)
    {
        List<TableSchemaActionInput> result = await Context.SchemaActionInput.AsNoTracking().Where(x => x.SchemaID == id).ToListAsync();
        return result.Select(MapperT2E.Map).ToList();
    }

    public async Task<List<EntitySchemaActionStep>> GetSchemaActionStepListBySchemaID(Guid id)
    {
        List<TableSchemaActionStep> result = await Context.SchemaActionStep.AsNoTracking().Where(x => x.SchemaID == id).ToListAsync();
        return result.Select(MapperT2E.Map).ToList();
    }

    public async Task<List<EntitySchemaAttribute>> GetSchemaAttributeListBySchemaID(Guid id)
    {
        List<TableSchemaAttribute> result = await Context.SchemaAttribute.AsNoTracking().Where(x => x.SchemaID == id).ToListAsync();
        return result.Select(MapperT2E.Map).ToList();
    }

    public async Task<List<EntitySchemaContext>> GetSchemaContextListBySchemaID(Guid id)
    {
        List<TableSchemaContext> result = await Context.SchemaContext.AsNoTracking().Where(x => x.SchemaID == id).ToListAsync();
        return result.Select(MapperT2E.Map).ToList();
    }

    public async Task<List<EntitySchemaDynamicValue.Discriminator>> GetSchemaDynamicValueListBySchemaID(Guid id)
    {
        List<TableSchemaDynamicValue> result = await Context.SchemaDynamicValue.AsNoTracking().Where(x => x.SchemaID == id).ToListAsync();
        return result.Select(MapperT2E.Map).ToList();
    }

    public async Task<List<EntitySchemaElement>> GetSchemaElementListBySchemaID(Guid id)
    {
        List<TableSchemaElement> result = await Context.SchemaElement.AsNoTracking().Where(x => x.SchemaID == id).ToListAsync();
        return result.Select(MapperT2E.Map).ToList();
    }

    public async Task<List<EntitySchemaProperty.Discriminator>> GetSchemaPropertyListBySchemaID(Guid id)
    {
        List<TableSchemaProperty> result = await Context.SchemaProperty.AsNoTracking().Where(x => x.SchemaID == id).ToListAsync();
        return result.Select(MapperT2E.Map).ToList();
    }

    public async Task<List<EntitySchemaInput>> GetSchemaInputListBySchemaID(Guid id)
    {
        List<TableSchemaInput> result = await Context.SchemaInput.AsNoTracking().Where(x => x.SchemaID == id).ToListAsync();
        return result.Select(MapperT2E.Map).ToList();
    }

    public async Task<List<EntityJunctionSchemaActionHasActionStep>> GetSchemaActionHasActionStepListBySchemaID(Guid id)
    {
        List<TableJunctionSchemaActionHasActionStep> result = await Context.SchemaActionHasActionStep.AsNoTracking().Where(x => x.Entity!.SchemaID == id).ToListAsync();
        return result.Select(MapperT2E.Map).ToList();
    }

    public async Task<List<EntityJunctionSchemaActionHasDynamicValueCode>> GetSchemaActionHasDynamicValueCodeListBySchemaID(Guid id)
    {
        List<TableJunctionSchemaActionHasDynamicValueCode> result = await Context.SchemaActionHasDynamicValueCode.AsNoTracking().Where(x => x.Entity!.SchemaID == id).ToListAsync();
        return result.Select(MapperT2E.Map).ToList();
    }

    public async Task<List<EntityAssociationSchemaActionInputHasAttribute.Discriminator>> GetSchemaActionInputHasAttributeListBySchemaID(Guid id)
    {
        List<TableAssociationSchemaActionInputHasAttribute> result = await Context.SchemaActionInputHasAttribute.AsNoTracking().Where(x => x.Entity!.SchemaID == id).ToListAsync();
        return result.Select(MapperT2E.Map).ToList();
    }

    public async Task<List<EntityJunctionSchemaActionStepHasActionInput>> GetSchemaActionStepHasActionInputListBySchemaID(Guid id)
    {
        List<TableJunctionSchemaActionStepHasActionInput> result = await Context.SchemaActionStepHasActionInput.AsNoTracking().Where(x => x.Entity!.SchemaID == id).ToListAsync();
        return result.Select(MapperT2E.Map).ToList();
    }

    public async Task<List<EntityJunctionSchemaContextHasAction>> GetSchemaContextHasActionListBySchemaID(Guid id)
    {
        List<TableJunctionSchemaContextHasAction> result = await Context.SchemaContextHasAction.AsNoTracking().Where(x => x.Entity!.SchemaID == id).ToListAsync();
        return result.Select(MapperT2E.Map).ToList();
    }

    public async Task<List<EntityJunctionSchemaContextHasElement>> GetSchemaContextHasElementListBySchemaID(Guid id)
    {
        List<TableJunctionSchemaContextHasElement> result = await Context.SchemaContextHasElement.AsNoTracking().Where(x => x.Entity!.SchemaID == id).ToListAsync();
        return result.Select(MapperT2E.Map).ToList();
    }

    public async Task<List<EntityJunctionSchemaElementHasAction>> GetSchemaElementHasActionListBySchemaID(Guid id)
    {
        List<TableJunctionSchemaElementHasAction> result = await Context.SchemaElementHasAction.AsNoTracking().Where(x => x.Entity!.SchemaID == id).ToListAsync();
        return result.Select(MapperT2E.Map).ToList();
    }

    public async Task<List<EntityJunctionSchemaElementHasProperty>> GetSchemaElementHasPropertyListBySchemaID(Guid id)
    {
        List<TableJunctionSchemaElementHasProperty> result = await Context.SchemaElementHasProperty.AsNoTracking().Where(x => x.Entity!.SchemaID == id).ToListAsync();
        return result.Select(MapperT2E.Map).ToList();
    }

    public async Task<List<EntityJunctionSchemaInputHasAttribute>> GetSchemaInputHasAttributeListBySchemaID(Guid id)
    {
        List<TableJunctionSchemaInputHasAttribute> result = await Context.SchemaInputHasAttribute.AsNoTracking().Where(x => x.Entity!.SchemaID == id).ToListAsync();
        return result.Select(MapperT2E.Map).ToList();
    }


    #region -- Populate --

    public async Task<EntitySchemaAction> Populate(EntitySchemaAction entity)
    {
        entity.TitleDynamic ??= MapperT2E.Map(await Context.SchemaDynamicValue.AsNoTracking().SingleAsync(x => x.ID == entity.TitleDynamicID));
        entity.ActionStepList ??= (await Context.SchemaActionHasActionStep.AsNoTracking().Where(x => x.EntityID == entity.ID).ToListAsync()).Select(MapperT2E.Map).ToList();
        entity.DynamicValueCodeList ??= (await Context.SchemaActionHasDynamicValueCode.AsNoTracking().Where(x => x.EntityID == entity.ID).ToListAsync()).Select(MapperT2E.Map).ToList();

        return entity;
    }

    public async Task<EntitySchemaActionStep> Populate(EntitySchemaActionStep entity)
    {
        entity.ActionInputList ??= (await Context.SchemaActionStepHasActionInput.AsNoTracking().Where(x => x.EntityID == entity.ID).ToListAsync()).Select(MapperT2E.Map).ToList();

        return entity;
    }

    public async Task<EntitySchemaActionInput> Populate(EntitySchemaActionInput entity)
    {
        entity.Input ??= MapperT2E.Map(await Context.SchemaInput.AsNoTracking().SingleAsync(x => x.ID == entity.InputID));
        entity.ActionStepList ??= (await Context.SchemaActionStepHasActionInput.AsNoTracking().Where(x => x.EntityID == entity.ID).ToListAsync()).Select(MapperT2E.Map).ToList();
        entity.AttributeList ??= (await Context.SchemaActionInputHasAttribute.AsNoTracking().Where(x => x.EntityID == entity.ID).ToListAsync()).Select(MapperT2E.Map).ToList();

        return entity;
    }

    public async Task<EntitySchemaAttribute> Populate(EntitySchemaAttribute entity)
    {
        entity.InputList ??= (await Context.SchemaInputHasAttribute.AsNoTracking().Where(x => x.EntityID == entity.ID).ToListAsync()).Select(MapperT2E.Map).ToList();

        return entity;
    }

    public async Task<EntitySchemaContext> Populate(EntitySchemaContext entity)
    {
        entity.ActionList ??= (await Context.SchemaContextHasAction.AsNoTracking().Where(x => x.EntityID == entity.ID).ToListAsync()).Select(MapperT2E.Map).ToList();
        entity.ElementList ??= (await Context.SchemaContextHasElement.AsNoTracking().Where(x => x.EntityID == entity.ID).ToListAsync()).Select(MapperT2E.Map).ToList();

        return entity;
    }

    public async Task<EntitySchemaDynamicValue.Discriminator> Populate(EntitySchemaDynamicValue.Discriminator entity)
    {
        return entity.GetValue() switch
        {
            EntitySchemaDynamicValueCode entityBoolean => new(await Populate(entityBoolean)),
            EntitySchemaDynamicValueSystemParameter entityDateTime => new(await Populate(entityDateTime)),
            EntitySchemaDynamicValueTextParameter entityString => new(await Populate(entityString)),
            _ => entity
        };
    }

    public Task<EntitySchemaDynamicValueCode> Populate(EntitySchemaDynamicValueCode entity)
    {
        // TODO: Fill this
        return Task.FromResult(entity);
    }

    public Task<EntitySchemaDynamicValueSystemParameter> Populate(EntitySchemaDynamicValueSystemParameter entity)
    {
        // TODO: Fill this
        return Task.FromResult(entity);
    }

    public Task<EntitySchemaDynamicValueTextParameter> Populate(EntitySchemaDynamicValueTextParameter entity)
    {
        // TODO: Fill this
        return Task.FromResult(entity);
    }

    public async Task<EntitySchemaElement> Populate(EntitySchemaElement entity)
    {
        entity.PropertyList ??= (await Context.SchemaElementHasProperty.AsNoTracking().Where(x => x.EntityID == entity.ID).ToListAsync()).Select(MapperT2E.Map).ToList();

        return entity;
    }

    public async Task<EntitySchemaInput> Populate(EntitySchemaInput entity)
    {
        entity.AttributeList ??= (await Context.SchemaInputHasAttribute.AsNoTracking().Where(x => x.EntityID == entity.ID).ToListAsync()).Select(MapperT2E.Map).ToList();

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

    public async Task<EntityJunctionSchemaActionHasActionStep> Populate(EntityJunctionSchemaActionHasActionStep entity)
    {
        entity.Relation ??= MapperT2E.Map(await Context.SchemaActionStep.AsNoTracking().SingleAsync(x => x.ID == entity.RelationID));

        return entity;
    }

    public async Task<EntityJunctionSchemaActionHasDynamicValueCode> Populate(EntityJunctionSchemaActionHasDynamicValueCode entity)
    {
        entity.Relation ??= MapperT2E.Map(await Context.SchemaDynamicValueCode.AsNoTracking().SingleAsync(x => x.ID == entity.RelationID));

        return entity;
    }

    public async Task<EntityJunctionSchemaActionStepHasActionInput> Populate(EntityJunctionSchemaActionStepHasActionInput entity)
    {
        entity.Relation ??= MapperT2E.Map(await Context.SchemaActionInput.AsNoTracking().SingleAsync(x => x.ID == entity.RelationID));

        return entity;
    }

    public async Task<EntityAssociationSchemaActionInputHasAttribute.Discriminator> Populate(EntityAssociationSchemaActionInputHasAttribute.Discriminator entity)
    {
        return entity.GetValue() switch
        {
            EntityAssociationSchemaActionInputHasAttributeDynamicValue value => new(await Populate(value)),
            EntityAssociationSchemaActionInputHasAttributeInteger value => new(await Populate(value)),
            EntityAssociationSchemaActionInputHasAttributeString value => new(await Populate(value)),
            _ => entity
        };
    }

    public async Task<EntityAssociationSchemaActionInputHasAttributeDynamicValue> Populate(EntityAssociationSchemaActionInputHasAttributeDynamicValue entity)
    {
        entity.Value ??= MapperT2E.Map(await Context.SchemaDynamicValue.AsNoTracking().SingleAsync(x => x.ID == entity.ValueID));
        entity.Relation ??= MapperT2E.Map(await Context.SchemaAttribute.AsNoTracking().SingleAsync(x => x.ID == entity.RelationID));

        return entity;
    }

    public async Task<EntityAssociationSchemaActionInputHasAttributeInteger> Populate(EntityAssociationSchemaActionInputHasAttributeInteger entity)
    {
        entity.Relation ??= MapperT2E.Map(await Context.SchemaAttribute.AsNoTracking().SingleAsync(x => x.ID == entity.RelationID));

        return entity;
    }

    public async Task<EntityAssociationSchemaActionInputHasAttributeString> Populate(EntityAssociationSchemaActionInputHasAttributeString entity)
    {
        entity.Relation ??= MapperT2E.Map(await Context.SchemaAttribute.AsNoTracking().SingleAsync(x => x.ID == entity.RelationID));

        return entity;
    }

    public async Task<EntityJunctionSchemaContextHasAction> Populate(EntityJunctionSchemaContextHasAction entity)
    {
        entity.Relation ??= MapperT2E.Map(await Context.SchemaAction.AsNoTracking().SingleAsync(x => x.ID == entity.RelationID));

        return entity;
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

    public async Task<EntityJunctionSchemaInputHasAttribute> Populate(EntityJunctionSchemaInputHasAttribute entity)
    {
        entity.Relation ??= MapperT2E.Map(await Context.SchemaAttribute.AsNoTracking().SingleAsync(x => x.ID == entity.RelationID));

        return entity;
    }

    #endregion -- Populate --

    #region -- Create --

    public async Task<EntitySchemaAction> Create(EntitySchemaAction entity)
    {
        return await CreateEntity(entity, MapperE2T.Map, MapperT2E.Map);
    }

    public async Task<EntitySchemaActionStep> Create(EntitySchemaActionStep entity)
    {
        return await CreateEntity(entity, MapperE2T.Map, MapperT2E.Map);
    }

    public async Task<EntitySchemaAttribute> Create(EntitySchemaAttribute entity)
    {
        return await CreateEntity(entity, MapperE2T.Map, MapperT2E.Map);
    }

    public async Task<EntitySchemaDynamicValue.Discriminator> Create(EntitySchemaDynamicValue entity)
    {
        await UpdateOrder<TableSchemaDynamicValue>(entity);
        EntityEntry<TableSchemaDynamicValue> result = await Context.SchemaDynamicValue.AddAsync(MapperE2T.Map(entity));
        return MapperT2E.Map(result.Entity);
    }

    public async Task<EntitySchemaContext> Create(EntitySchemaContext entity)
    {
        return await CreateEntity(entity, MapperE2T.Map, MapperT2E.Map);
    }

    public async Task<EntitySchemaActionInput> Create(EntitySchemaActionInput entity)
    {
        return await CreateEntity(entity, MapperE2T.Map, MapperT2E.Map);
    }

    public async Task<EntitySchemaElement> Create(EntitySchemaElement entity)
    {
        return await CreateEntity(entity, MapperE2T.Map, MapperT2E.Map);
    }

    public async Task<EntitySchemaInput> Create(EntitySchemaInput entity)
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

    public void Update(EntitySchemaAction entity)
    {
        Context.SchemaAction.Update(MapperE2T.Map(entity));
    }

    public void Update(EntitySchemaActionInput entity)
    {
        Context.SchemaActionInput.Update(MapperE2T.Map(entity));
    }

    public void Update(EntitySchemaActionStep entity)
    {
        Context.SchemaActionStep.Update(MapperE2T.Map(entity));
    }

    public void Update(EntitySchemaAttribute entity)
    {
        Context.SchemaAttribute.Update(MapperE2T.Map(entity));
    }

    public void Update(EntitySchemaContext entity)
    {
        Context.SchemaContext.Update(MapperE2T.Map(entity));
    }

    public void Update(EntitySchemaDynamicValue entity)
    {
        Context.SchemaDynamicValue.Update(MapperE2T.Map(entity));
    }

    public void Update(EntitySchemaElement entity)
    {
        Context.SchemaElement.Update(MapperE2T.Map(entity));
    }

    public void Update(EntitySchemaInput entity)
    {
        Context.SchemaInput.Update(MapperE2T.Map(entity));
    }

    public void Update(EntitySchemaProperty baseEntity)
    {
        Context.SchemaProperty.Update(MapperE2T.Map(baseEntity));
    }

    #endregion -- Update --

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
}
