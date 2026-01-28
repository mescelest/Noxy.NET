using Noxy.NET.EntityManagement.Application.Interfaces;
using Noxy.NET.EntityManagement.Application.Interfaces.Services;
using Noxy.NET.EntityManagement.Domain.Entities.Schemas;
using Noxy.NET.EntityManagement.Domain.Entities.Schemas.Discriminators;
using Noxy.NET.EntityManagement.Domain.Entities.Schemas.Junctions;

namespace Noxy.NET.EntityManagement.Application.Services;

public class SchemaBuilderService(IUnitOfWorkFactory serviceUoWFactory) : ISchemaBuilderService
{
    private readonly Dictionary<Guid, List<EntityJunctionSchemaContextHasElement>> _collectionContextHasElement = [];
    private readonly Dictionary<Guid, List<EntityJunctionSchemaElementHasProperty>> _collectionElementHasProperty = [];
    private Dictionary<Guid, EntitySchemaContext> _collectionContext = [];
    private Dictionary<Guid, EntitySchemaDynamicValue.Discriminator> _collectionDynamicValue = [];
    private Dictionary<Guid, EntitySchemaElement> _collectionElement = [];
    private Dictionary<Guid, EntitySchemaProperty.Discriminator> _collectionProperty = [];

    public async Task<EntitySchema> ConstructSchema(Guid? id = null)
    {
        await using IUnitOfWork uow = await serviceUoWFactory.Create();

        EntitySchema schema;
        if (!id.HasValue)
        {
            schema = await uow.Template.GetCurrentSchema();
            id = schema.ID;
        }
        else
        {
            schema = await uow.Template.GetSchemaByID(id.Value);
        }

        schema.ContextList = await uow.Schema.GetSchemaContextListBySchemaID(id.Value);
        schema.DynamicValueList = await uow.Schema.GetSchemaDynamicValueListBySchemaID(id.Value);
        schema.ElementList = await uow.Schema.GetSchemaElementListBySchemaID(id.Value);
        schema.PropertyList = await uow.Schema.GetSchemaPropertyListBySchemaID(id.Value);

        _collectionContext = schema.ContextList.ToDictionary(x => x.ID, y => y);
        _collectionDynamicValue = schema.DynamicValueList.ToDictionary(x => x.ID, y => y);
        _collectionElement = schema.ElementList.ToDictionary(x => x.ID, y => y);
        _collectionProperty = schema.PropertyList.ToDictionary(x => x.ID, y => y);

        // Base

        foreach (EntitySchemaContext entity in schema.ContextList)
        {
            entity.Schema = schema;
        }

        foreach (EntitySchemaDynamicValue.Discriminator entity in schema.DynamicValueList)
        {
            EntitySchemaDynamicValue value = entity.GetValue();
            value.Schema = schema;
        }

        foreach (EntitySchemaElement entity in schema.ElementList)
        {
            entity.Schema = schema;
        }

        foreach (EntitySchemaProperty.Discriminator entity in schema.PropertyList)
        {
            EntitySchemaProperty value = entity.GetValue();
            value.Schema = schema;
        }

        // Junction

        foreach (EntityJunctionSchemaContextHasElement junction in await uow.Schema.GetSchemaContextHasElementListBySchemaID(id.Value))
        {
            if (!_collectionContextHasElement.TryGetValue(junction.EntityID, out List<EntityJunctionSchemaContextHasElement>? listEntity))
            {
                _collectionContextHasElement[junction.EntityID] = listEntity = [];
            }

            junction.Entity = _collectionContext[junction.EntityID];
            junction.Entity.ElementList = listEntity;
            listEntity.Add(junction);

            if (!_collectionContextHasElement.TryGetValue(junction.RelationID, out List<EntityJunctionSchemaContextHasElement>? listRelation))
            {
                _collectionContextHasElement[junction.RelationID] = listRelation = [];
            }

            junction.Relation = _collectionElement[junction.RelationID];
            junction.Relation.ContextList = listRelation;
            listRelation.Add(junction);
        }

        foreach (EntityJunctionSchemaElementHasProperty junction in await uow.Schema.GetSchemaElementHasPropertyListBySchemaID(id.Value))
        {
            if (!_collectionElementHasProperty.TryGetValue(junction.EntityID, out List<EntityJunctionSchemaElementHasProperty>? listEntity))
            {
                _collectionElementHasProperty[junction.EntityID] = listEntity = [];
            }

            junction.Entity = _collectionElement[junction.EntityID];
            junction.Entity.PropertyList = listEntity;
            listEntity.Add(junction);

            if (!_collectionElementHasProperty.TryGetValue(junction.RelationID, out List<EntityJunctionSchemaElementHasProperty>? listRelation))
            {
                _collectionElementHasProperty[junction.RelationID] = listRelation = [];
            }

            junction.Relation = _collectionProperty[junction.RelationID];
            junction.Relation.GetValue().RelationElementList = listRelation;
            listRelation.Add(junction);
        }

        return schema;
    }
}
