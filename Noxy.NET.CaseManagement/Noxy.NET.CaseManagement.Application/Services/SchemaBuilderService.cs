using Noxy.NET.CaseManagement.Application.Interfaces;
using Noxy.NET.CaseManagement.Application.Interfaces.Services;
using Noxy.NET.CaseManagement.Domain.Entities.Schemas;
using Noxy.NET.CaseManagement.Domain.Entities.Schemas.Associations;
using Noxy.NET.CaseManagement.Domain.Entities.Schemas.Discriminators;
using Noxy.NET.CaseManagement.Domain.Entities.Schemas.Junctions;

namespace Noxy.NET.CaseManagement.Application.Services;

public class SchemaBuilderService(IUnitOfWorkFactory serviceUoWFactory) : ISchemaBuilderService
{
    private Dictionary<Guid, EntitySchemaAction> _collectionAction = [];
    private Dictionary<Guid, EntitySchemaActionInput> _collectionActionInput = [];
    private Dictionary<Guid, EntitySchemaActionStep> _collectionActionStep = [];
    private Dictionary<Guid, EntitySchemaAttribute> _collectionAttribute = [];
    private Dictionary<Guid, EntitySchemaContext> _collectionContext = [];
    private Dictionary<Guid, EntitySchemaDynamicValue.Discriminator> _collectionDynamicValue = [];
    private Dictionary<Guid, EntitySchemaElement> _collectionElement = [];
    private Dictionary<Guid, EntitySchemaInput> _collectionInput = [];
    private Dictionary<Guid, EntitySchemaProperty.Discriminator> _collectionProperty = [];

    private readonly Dictionary<Guid, List<EntityJunctionSchemaActionHasActionStep>> _collectionActionHasActionStep = [];
    private readonly Dictionary<Guid, List<EntityJunctionSchemaActionHasDynamicValueCode>> _collectionActionHasDynamicValueCode = [];
    private readonly Dictionary<Guid, List<EntityAssociationSchemaActionInputHasAttribute.Discriminator>> _collectionActionInputHasAttribute = [];
    private readonly Dictionary<Guid, List<EntityJunctionSchemaActionStepHasActionInput>> _collectionActionStepHasActionInput = [];
    private readonly Dictionary<Guid, List<EntityJunctionSchemaContextHasAction>> _collectionContextHasAction = [];
    private readonly Dictionary<Guid, List<EntityJunctionSchemaContextHasElement>> _collectionContextHasElement = [];
    private readonly Dictionary<Guid, List<EntityJunctionSchemaElementHasAction>> _collectionElementHasAction = [];
    private readonly Dictionary<Guid, List<EntityJunctionSchemaElementHasProperty>> _collectionElementHasProperty = [];
    private readonly Dictionary<Guid, List<EntityJunctionSchemaInputHasAttribute>> _collectionInputHasAttribute = [];

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

        schema.ActionList = await uow.Schema.GetSchemaActionListBySchemaID(id.Value);
        schema.ActionInputList = await uow.Schema.GetSchemaActionInputListBySchemaID(id.Value);
        schema.ActionStepList = await uow.Schema.GetSchemaActionStepListBySchemaID(id.Value);
        schema.AttributeList = await uow.Schema.GetSchemaAttributeListBySchemaID(id.Value);
        schema.ContextList = await uow.Schema.GetSchemaContextListBySchemaID(id.Value);
        schema.DynamicValueList = await uow.Schema.GetSchemaDynamicValueListBySchemaID(id.Value);
        schema.ElementList = await uow.Schema.GetSchemaElementListBySchemaID(id.Value);
        schema.InputList = await uow.Schema.GetSchemaInputListBySchemaID(id.Value);
        schema.PropertyList = await uow.Schema.GetSchemaPropertyListBySchemaID(id.Value);

        _collectionAction = schema.ActionList.ToDictionary(x => x.ID, y => y);
        _collectionActionInput = schema.ActionInputList.ToDictionary(x => x.ID, y => y);
        _collectionActionStep = schema.ActionStepList.ToDictionary(x => x.ID, y => y);
        _collectionAttribute = schema.AttributeList.ToDictionary(x => x.ID, y => y);
        _collectionContext = schema.ContextList.ToDictionary(x => x.ID, y => y);
        _collectionDynamicValue = schema.DynamicValueList.ToDictionary(x => x.ID, y => y);
        _collectionElement = schema.ElementList.ToDictionary(x => x.ID, y => y);
        _collectionInput = schema.InputList.ToDictionary(x => x.ID, y => y);
        _collectionProperty = schema.PropertyList.ToDictionary(x => x.ID, y => y);

        // Base

        foreach (EntitySchemaAction entity in schema.ActionList)
        {
            entity.Schema = schema;
            entity.TitleDynamic = _collectionDynamicValue[entity.TitleDynamicID];

            if (entity.DescriptionDynamicID.HasValue)
            {
                entity.DescriptionDynamic = _collectionDynamicValue[entity.DescriptionDynamicID.Value];
            }

            if (entity.IsActiveDynamicID.HasValue)
            {
                entity.IsActiveDynamic = _collectionDynamicValue[entity.IsActiveDynamicID.Value].Code;
            }
        }

        foreach (EntitySchemaActionInput entity in schema.ActionInputList)
        {
            entity.Schema = schema;
            entity.Input = _collectionInput[entity.InputID];
            entity.TitleDynamic = _collectionDynamicValue[entity.TitleDynamicID];
            if (entity.DescriptionDynamicID.HasValue)
            {
                entity.DescriptionDynamic = _collectionDynamicValue[entity.DescriptionDynamicID.Value];
            }

            if (entity.IsActiveDynamicID.HasValue)
            {
                entity.IsActiveDynamic = _collectionDynamicValue[entity.IsActiveDynamicID.Value].Code;
            }

            if (entity.IsValidDynamicID.HasValue)
            {
                entity.IsValidDynamic = _collectionDynamicValue[entity.IsValidDynamicID.Value].Code;
            }
        }

        foreach (EntitySchemaActionStep entity in schema.ActionStepList)
        {
            entity.Schema = schema;
            entity.TitleDynamic = _collectionDynamicValue[entity.TitleDynamicID];
            if (entity.DescriptionDynamicID.HasValue)
            {
                entity.DescriptionDynamic = _collectionDynamicValue[entity.DescriptionDynamicID.Value];
            }
        }

        foreach (EntitySchemaAttribute entity in schema.AttributeList)
        {
            entity.Schema = schema;
        }

        foreach (EntitySchemaContext entity in schema.ContextList)
        {
            entity.Schema = schema;
            entity.TitleDynamic = _collectionDynamicValue[entity.TitleDynamicID];
            if (entity.DescriptionDynamicID.HasValue)
            {
                entity.DescriptionDynamic = _collectionDynamicValue[entity.DescriptionDynamicID.Value];
            }
        }

        foreach (EntitySchemaDynamicValue.Discriminator entity in schema.DynamicValueList)
        {
            EntitySchemaDynamicValue value = entity.GetValue();
            value.Schema = schema;
        }

        foreach (EntitySchemaElement entity in schema.ElementList)
        {
            entity.Schema = schema;
            entity.TitleDynamic = _collectionDynamicValue[entity.TitleDynamicID];
            if (entity.DescriptionDynamicID.HasValue)
            {
                entity.DescriptionDynamic = _collectionDynamicValue[entity.DescriptionDynamicID.Value];
            }
        }

        foreach (EntitySchemaInput entity in schema.InputList)
        {
            entity.Schema = schema;
        }

        foreach (EntitySchemaProperty.Discriminator entity in schema.PropertyList)
        {
            EntitySchemaProperty value = entity.GetValue();
            value.Schema = schema;
            value.TitleDynamic = _collectionDynamicValue[value.TitleDynamicID];
            if (value.DescriptionDynamicID.HasValue)
            {
                value.DescriptionDynamic = _collectionDynamicValue[value.DescriptionDynamicID.Value];
            }
        }

        // Junction

        List<EntityJunctionSchemaActionHasActionStep> listJunctionActionHasActionStep = await uow.Schema.GetSchemaActionHasActionStepListBySchemaID(id.Value);
        foreach (EntityJunctionSchemaActionHasActionStep junction in listJunctionActionHasActionStep)
        {
            if (!_collectionActionHasActionStep.TryGetValue(junction.EntityID, out List<EntityJunctionSchemaActionHasActionStep>? listEntity))
            {
                _collectionActionHasActionStep[junction.EntityID] = listEntity = [];
            }

            junction.Entity = _collectionAction[junction.EntityID];
            junction.Entity.ActionStepList = listEntity;
            listEntity.Add(junction);

            if (!_collectionActionHasActionStep.TryGetValue(junction.RelationID, out List<EntityJunctionSchemaActionHasActionStep>? listRelation))
            {
                _collectionActionHasActionStep[junction.RelationID] = listRelation = [];
            }

            junction.Relation = _collectionActionStep[junction.RelationID];
            junction.Relation.ActionList = listRelation;
            listRelation.Add(junction);
        }

        List<EntityJunctionSchemaActionHasDynamicValueCode> listJunctionActionHasDynamicValueCode = await uow.Schema.GetSchemaActionHasDynamicValueCodeListBySchemaID(id.Value);
        foreach (EntityJunctionSchemaActionHasDynamicValueCode junction in listJunctionActionHasDynamicValueCode)
        {
            if (!_collectionActionHasDynamicValueCode.TryGetValue(junction.EntityID, out List<EntityJunctionSchemaActionHasDynamicValueCode>? listEntity))
            {
                _collectionActionHasDynamicValueCode[junction.EntityID] = listEntity = [];
            }

            junction.Entity = _collectionAction[junction.EntityID];
            junction.Entity.DynamicValueCodeList = listEntity;
            listEntity.Add(junction);

            if (!_collectionActionHasDynamicValueCode.TryGetValue(junction.RelationID, out List<EntityJunctionSchemaActionHasDynamicValueCode>? listRelation))
            {
                _collectionActionHasDynamicValueCode[junction.RelationID] = listRelation = [];
            }

            junction.Relation = _collectionDynamicValue[junction.RelationID].Code ?? throw new InvalidOperationException();
            junction.Relation.RelationActionList = listRelation;
            listRelation.Add(junction);
        }

        foreach (EntityAssociationSchemaActionInputHasAttribute.Discriminator junction in await uow.Schema.GetSchemaActionInputHasAttributeListBySchemaID(id.Value))
        {
            EntityAssociationSchemaActionInputHasAttribute value = junction.GetValue();
            if (value is EntityAssociationSchemaActionInputHasAttributeDynamicValue dynamic)
            {
                dynamic.Value = dynamic.ValueID.HasValue ? _collectionDynamicValue[dynamic.ValueID.Value] : null;
            }

            if (!_collectionActionInputHasAttribute.TryGetValue(junction.EntityID, out List<EntityAssociationSchemaActionInputHasAttribute.Discriminator>? listEntity))
            {
                _collectionActionInputHasAttribute[junction.EntityID] = listEntity = [];
            }

            value.Entity = _collectionActionInput[junction.EntityID];
            value.Entity.AttributeList = listEntity;
            listEntity.Add(junction);

            if (!_collectionActionInputHasAttribute.TryGetValue(junction.RelationID, out List<EntityAssociationSchemaActionInputHasAttribute.Discriminator>? listRelation))
            {
                _collectionActionInputHasAttribute[junction.RelationID] = listRelation = [];
            }

            value.Relation = _collectionAttribute[junction.RelationID];
            value.Relation.ActionInputList = listRelation;
            listRelation.Add(junction);
        }

        foreach (EntityJunctionSchemaActionStepHasActionInput junction in await uow.Schema.GetSchemaActionStepHasActionInputListBySchemaID(id.Value))
        {
            if (!_collectionActionStepHasActionInput.TryGetValue(junction.EntityID, out List<EntityJunctionSchemaActionStepHasActionInput>? listEntity))
            {
                _collectionActionStepHasActionInput[junction.EntityID] = listEntity = [];
            }

            junction.Entity = _collectionActionStep[junction.EntityID];
            junction.Entity.ActionInputList = listEntity;
            listEntity.Add(junction);

            if (!_collectionActionStepHasActionInput.TryGetValue(junction.RelationID, out List<EntityJunctionSchemaActionStepHasActionInput>? listRelation))
            {
                _collectionActionStepHasActionInput[junction.RelationID] = listRelation = [];
            }

            junction.Relation = _collectionActionInput[junction.RelationID];
            junction.Relation.ActionStepList = listRelation;
            listRelation.Add(junction);
        }

        foreach (EntityJunctionSchemaContextHasAction junction in await uow.Schema.GetSchemaContextHasActionListBySchemaID(id.Value))
        {
            if (!_collectionContextHasAction.TryGetValue(junction.EntityID, out List<EntityJunctionSchemaContextHasAction>? listEntity))
            {
                _collectionContextHasAction[junction.EntityID] = listEntity = [];
            }

            junction.Entity = _collectionContext[junction.EntityID];
            junction.Entity.ActionList = listEntity;
            listEntity.Add(junction);

            if (!_collectionContextHasAction.TryGetValue(junction.RelationID, out List<EntityJunctionSchemaContextHasAction>? listRelation))
            {
                _collectionContextHasAction[junction.RelationID] = listRelation = [];
            }

            junction.Relation = _collectionAction[junction.RelationID];
            junction.Relation.ContextList = listRelation;
            listRelation.Add(junction);
        }

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

        foreach (EntityJunctionSchemaElementHasAction junction in await uow.Schema.GetSchemaElementHasActionListBySchemaID(id.Value))
        {
            if (!_collectionElementHasAction.TryGetValue(junction.EntityID, out List<EntityJunctionSchemaElementHasAction>? listEntity))
            {
                _collectionElementHasAction[junction.EntityID] = listEntity = [];
            }

            junction.Entity = _collectionElement[junction.EntityID];
            junction.Entity.ActionList = listEntity;
            listEntity.Add(junction);

            if (!_collectionElementHasAction.TryGetValue(junction.RelationID, out List<EntityJunctionSchemaElementHasAction>? listRelation))
            {
                _collectionElementHasAction[junction.RelationID] = listRelation = [];
            }

            junction.Relation = _collectionAction[junction.RelationID];
            junction.Relation.ActionList = listRelation;
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

        List<EntityJunctionSchemaInputHasAttribute> listJunctionInputHasAttribute = await uow.Schema.GetSchemaInputHasAttributeListBySchemaID(id.Value);
        foreach (EntityJunctionSchemaInputHasAttribute junction in listJunctionInputHasAttribute)
        {
            if (!_collectionInputHasAttribute.TryGetValue(junction.EntityID, out List<EntityJunctionSchemaInputHasAttribute>? listEntity))
            {
                _collectionInputHasAttribute[junction.EntityID] = listEntity = [];
            }

            junction.Entity = _collectionInput[junction.EntityID];
            junction.Entity.AttributeList = listEntity;
            listEntity.Add(junction);

            if (!_collectionInputHasAttribute.TryGetValue(junction.RelationID, out List<EntityJunctionSchemaInputHasAttribute>? listRelation))
            {
                _collectionInputHasAttribute[junction.RelationID] = listRelation = [];
            }

            junction.Relation = _collectionAttribute[junction.RelationID];
            junction.Relation.InputList = listRelation;
            listRelation.Add(junction);
        }

        return schema;
    }
}
