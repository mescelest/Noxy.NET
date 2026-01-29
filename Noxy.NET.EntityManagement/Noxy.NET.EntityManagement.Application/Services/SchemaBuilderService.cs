using Noxy.NET.EntityManagement.Application.Interfaces;
using Noxy.NET.EntityManagement.Application.Interfaces.Services;
using Noxy.NET.EntityManagement.Domain.Abstractions.Entities;
using Noxy.NET.EntityManagement.Domain.Entities.Schemas;
using Noxy.NET.EntityManagement.Domain.Entities.Schemas.Discriminators;
using Noxy.NET.EntityManagement.Domain.Entities.Schemas.Junctions;

namespace Noxy.NET.EntityManagement.Application.Services;

public class SchemaBuilderService(IUnitOfWorkFactory serviceUoWFactory, ITaskBundlingService serviceTaskBundling) : ISchemaBuilderService
{
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

        (schema.ContextList, schema.ParameterList, schema.ElementList, schema.PropertyList) = await serviceTaskBundling.WhenAll(
            uow.Schema.GetSchemaContextListBySchemaID(id.Value),
            uow.Schema.GetSchemaDynamicValueListBySchemaID(id.Value),
            uow.Schema.GetSchemaElementListBySchemaID(id.Value),
            uow.Schema.GetSchemaPropertyListBySchemaID(id.Value)
        );

        Dictionary<Guid, List<EntityJunctionSchemaContextHasElement>> collectionContextHasElement = new(schema.ContextList.Count + schema.ElementList.Count);
        Dictionary<Guid, List<EntityJunctionSchemaElementHasProperty>> collectionElementHasProperty = new(schema.ElementList.Count + schema.PropertyList.Count);

        Dictionary<Guid, EntitySchemaContext> collectionContext = schema.ContextList.ToDictionary(x => x.ID, y => y);
        Dictionary<Guid, EntitySchemaElement> collectionElement = schema.ElementList.ToDictionary(x => x.ID, y => y);
        Dictionary<Guid, EntitySchemaParameter.Discriminator> collectionParameter = schema.ParameterList.ToDictionary(x => x.ID, y => y);
        Dictionary<Guid, EntitySchemaProperty.Discriminator> collectionProperty = schema.PropertyList.ToDictionary(x => x.ID, y => y);

        // Base
        foreach (EntitySchemaContext entity in schema.ContextList)
        {
            entity.Schema = schema;
            AssignTextParameters(entity, collectionParameter);
        }

        foreach (EntitySchemaElement entity in schema.ElementList)
        {
            entity.Schema = schema;
            AssignTextParameters(entity, collectionParameter);
        }

        IEnumerable<EntitySchemaParameter> listParameter = schema.ParameterList.Select(x => x.GetValue());
        foreach (EntitySchemaParameter entity in listParameter)
        {
            entity.Schema = schema;
        }

        IEnumerable<EntitySchemaProperty> listProperty = schema.PropertyList.Select(x => x.GetValue());
        foreach (EntitySchemaProperty entity in listProperty)
        {
            entity.Schema = schema;
            AssignTextParameters(entity, collectionParameter);
        }

        // Junction

        foreach (EntityJunctionSchemaContextHasElement junction in await uow.Schema.GetSchemaContextHasElementListBySchemaID(id.Value))
        {
            if (!collectionContextHasElement.TryGetValue(junction.EntityID, out List<EntityJunctionSchemaContextHasElement>? listEntity))
            {
                collectionContextHasElement[junction.EntityID] = listEntity = [];
            }

            junction.Entity = collectionContext[junction.EntityID];
            junction.Entity.ElementList = listEntity;
            listEntity.Add(junction);

            if (!collectionContextHasElement.TryGetValue(junction.RelationID, out List<EntityJunctionSchemaContextHasElement>? listRelation))
            {
                collectionContextHasElement[junction.RelationID] = listRelation = [];
            }

            junction.Relation = collectionElement[junction.RelationID];
            junction.Relation.ContextList = listRelation;
            listRelation.Add(junction);
        }

        foreach (EntityJunctionSchemaElementHasProperty junction in await uow.Schema.GetSchemaElementHasPropertyListBySchemaID(id.Value))
        {
            if (!collectionElementHasProperty.TryGetValue(junction.EntityID, out List<EntityJunctionSchemaElementHasProperty>? listEntity))
            {
                collectionElementHasProperty[junction.EntityID] = listEntity = [];
            }

            junction.Entity = collectionElement[junction.EntityID];
            junction.Entity.PropertyList = listEntity;
            listEntity.Add(junction);

            if (!collectionElementHasProperty.TryGetValue(junction.RelationID, out List<EntityJunctionSchemaElementHasProperty>? listRelation))
            {
                collectionElementHasProperty[junction.RelationID] = listRelation = [];
            }

            junction.Relation = collectionProperty[junction.RelationID];
            junction.Relation.GetValue().RelationElementList = listRelation;
            listRelation.Add(junction);
        }

        return schema;
    }

    private static void AssignTextParameters<T>(T entity, Dictionary<Guid, EntitySchemaParameter.Discriminator> collectionParameter) where T : BaseEntitySchemaComponent
    {
        entity.TitleTextParameter = collectionParameter.TryGetValue(entity.TitleTextParameterID, out EntitySchemaParameter.Discriminator? titleParam)
            ? titleParam.TextParameter
            : throw new KeyNotFoundException($"Title parameter {entity.TitleTextParameterID} not found for {typeof(T).Name} {entity.ID}");

        if (!entity.DescriptionTextParameterID.HasValue) return;

        entity.DescriptionTextParameter = collectionParameter.TryGetValue(entity.DescriptionTextParameterID.Value, out EntitySchemaParameter.Discriminator? descParam)
            ? descParam.TextParameter
            : throw new KeyNotFoundException($"Description parameter {entity.DescriptionTextParameterID} not found for {typeof(T).Name} {entity.ID}");
    }
}
