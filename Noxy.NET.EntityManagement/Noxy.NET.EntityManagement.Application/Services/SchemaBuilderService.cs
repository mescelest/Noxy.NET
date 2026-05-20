using Noxy.NET.EntityManagement.Application.Interfaces;
using Noxy.NET.EntityManagement.Application.Interfaces.Services;
using Noxy.NET.EntityManagement.Domain.Entities.Schemas;
using Noxy.NET.EntityManagement.Domain.Entities.Schemas.Discriminators;
using Noxy.NET.EntityManagement.Domain.Entities.Schemas.Junctions;

namespace Noxy.NET.EntityManagement.Application.Services;

public class SchemaBuilderService(IUnitOfWorkFactory serviceUoWFactory) : ISchemaBuilderService
{
    public async Task<EntitySchema> ConstructSchema(Guid? id = null)
    {
        await using IUnitOfWork uow = await serviceUoWFactory.Create();

        EntitySchema schema;
        if (!id.HasValue)
        {
            schema = await uow.Schema.GetCurrentSchema();
            id = schema.ID;
        }
        else
        {
            schema = await uow.Schema.GetSchemaByID(id.Value);
        }

        schema.ContextList = await uow.Schema.GetSchemaContextListBySchemaID(id.Value);
        schema.ElementList = await uow.Schema.GetSchemaElementListBySchemaID(id.Value);
        schema.ParameterList = (await uow.Schema.GetSchemaParameterListBySchemaID(id.Value)).Select(x => new EntitySchemaParameter.Discriminator(x)).ToList();
        schema.PropertyList = (await uow.Schema.GetSchemaPropertyListBySchemaID(id.Value)).Select(x => new EntitySchemaProperty.Discriminator(x)).ToList();

        Dictionary<Guid, List<EntitySchemaContextHasElement>> collectionContextHasElement = new(schema.ContextList.Count + schema.ElementList.Count);
        Dictionary<Guid, List<EntitySchemaElementHasProperty>> collectionElementHasProperty = new(schema.ElementList.Count + schema.PropertyList.Count);

        Dictionary<Guid, EntitySchemaContext> collectionContext = schema.ContextList.ToDictionary(x => x.ID, y => y);
        Dictionary<Guid, EntitySchemaElement> collectionElement = schema.ElementList.ToDictionary(x => x.ID, y => y);
        Dictionary<Guid, EntitySchemaParameter.Discriminator> collectionParameter = schema.ParameterList.ToDictionary(x => x.ID, y => y);
        Dictionary<Guid, EntitySchemaProperty.Discriminator> collectionProperty = schema.PropertyList.ToDictionary(x => x.ID, y => y);

        // Base
        foreach (EntitySchemaContext entity in schema.ContextList)
        {
            entity.Schema = schema;
            entity.TitleTextParameter = collectionParameter.TryGetValue(entity.TitleTextParameterID, out EntitySchemaParameter.Discriminator? titleParam)
                ? titleParam.TextParameter
                : throw new KeyNotFoundException($"Title parameter {entity.TitleTextParameterID} not found for {nameof(EntitySchemaContext)} {entity.ID}");

            if (entity.DescriptionTextParameterID.HasValue)
            {
                entity.DescriptionTextParameter = collectionParameter.TryGetValue(entity.DescriptionTextParameterID.Value, out EntitySchemaParameter.Discriminator? descParam)
                    ? descParam.TextParameter
                    : throw new KeyNotFoundException($"Description parameter {entity.DescriptionTextParameterID} not found for {nameof(EntitySchemaContext)} {entity.ID}");
            }
        }

        foreach (EntitySchemaElement entity in schema.ElementList)
        {
            entity.Schema = schema;
            entity.TitleTextParameter = collectionParameter.TryGetValue(entity.TitleParameterTextID, out EntitySchemaParameter.Discriminator? titleParam)
                ? titleParam.TextParameter
                : throw new KeyNotFoundException($"Title parameter {entity.TitleParameterTextID} not found for {nameof(EntitySchemaElement)} {entity.ID}");

            if (entity.DescriptionParameterTextID.HasValue)
            {
                entity.DescriptionTextParameter = collectionParameter.TryGetValue(entity.DescriptionParameterTextID.Value, out EntitySchemaParameter.Discriminator? descParam)
                    ? descParam.TextParameter
                    : throw new KeyNotFoundException($"Description parameter {entity.DescriptionParameterTextID} not found for {nameof(EntitySchemaElement)} {entity.ID}");
            }
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
            entity.TitleParameterText = collectionParameter.TryGetValue(entity.TitleParameterTextID, out EntitySchemaParameter.Discriminator? titleParam)
                ? titleParam.TextParameter
                : throw new KeyNotFoundException($"Title parameter {entity.TitleParameterTextID} not found for {nameof(EntitySchemaProperty)} {entity.ID}");

            if (entity.DescriptionParameterTextID.HasValue)
            {
                entity.DescriptionParameterText = collectionParameter.TryGetValue(entity.DescriptionParameterTextID.Value, out EntitySchemaParameter.Discriminator? descParam)
                    ? descParam.TextParameter
                    : throw new KeyNotFoundException($"Description parameter {entity.DescriptionParameterTextID} not found for {nameof(EntitySchemaProperty)} {entity.ID}");
            }
        }

        // Junction

        foreach (EntitySchemaContextHasElement junction in await uow.Schema.GetSchemaContextHasElementListBySchemaID(id.Value))
        {
            if (!collectionContextHasElement.TryGetValue(junction.EntityID, out List<EntitySchemaContextHasElement>? listEntity))
            {
                collectionContextHasElement[junction.EntityID] = listEntity = [];
            }

            junction.Entity = collectionContext[junction.EntityID];
            junction.Entity.ElementList = listEntity;
            listEntity.Add(junction);

            if (!collectionContextHasElement.TryGetValue(junction.RelationID, out List<EntitySchemaContextHasElement>? listRelation))
            {
                collectionContextHasElement[junction.RelationID] = listRelation = [];
            }

            junction.Relation = collectionElement[junction.RelationID];
            junction.Relation.ContextList = listRelation;
            listRelation.Add(junction);
        }

        foreach (EntitySchemaElementHasProperty junction in await uow.Schema.GetSchemaElementHasPropertyListBySchemaID(id.Value))
        {
            if (!collectionElementHasProperty.TryGetValue(junction.EntityID, out List<EntitySchemaElementHasProperty>? listEntity))
            {
                collectionElementHasProperty[junction.EntityID] = listEntity = [];
            }

            junction.Entity = collectionElement[junction.EntityID];
            junction.Entity.PropertyList = listEntity;
            listEntity.Add(junction);

            if (!collectionElementHasProperty.TryGetValue(junction.RelationID, out List<EntitySchemaElementHasProperty>? listRelation))
            {
                collectionElementHasProperty[junction.RelationID] = listRelation = [];
            }

            junction.Relation = collectionProperty[junction.RelationID];
            junction.Relation.GetValue().RelationElementList = listRelation;
            listRelation.Add(junction);
        }

        return schema;
    }
}
