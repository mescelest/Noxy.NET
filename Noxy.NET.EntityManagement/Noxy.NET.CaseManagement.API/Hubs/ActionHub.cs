using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Noxy.NET.CaseManagement.Application.Interfaces.Hubs;
using Noxy.NET.CaseManagement.Application.Interfaces.Services;
using Noxy.NET.CaseManagement.Domain.Entities.Schemas;
using Noxy.NET.CaseManagement.Domain.Entities.Schemas.Associations;
using Noxy.NET.CaseManagement.Domain.Entities.Schemas.Discriminators;
using Noxy.NET.CaseManagement.Domain.Entities.Schemas.Junctions;
using Noxy.NET.CaseManagement.Domain.Models;
using Noxy.NET.Models;

namespace Noxy.NET.CaseManagement.API.Hubs;

[Authorize]
public class ActionHub(IApplicationService serviceApplication, IDynamicValueService serviceDynamicValue) : Hub<IActionClientHub>, IActionServerHub
{
    private static readonly Dictionary<string, ActionManager> Data = new();

    private string UserIdentifier => Context.User?.Claims.Single(x => x.Type == ClaimTypes.Email).Value ?? throw new ArgumentNullException();

    public StateAction Register(Guid id, string identifier, Dictionary<string, object?>? context = null)
    {
        if (!Data.TryGetValue(UserIdentifier, out ActionManager? manager))
        {
            manager = Data[UserIdentifier] = new();
        }

        manager.Register(id, serviceApplication.GetSchemaAction(identifier), context);
        return GenerateActionState(id, manager);
    }

    public void Deregister(Guid id)
    {
        if (!Data.TryGetValue(UserIdentifier, out ActionManager? manager)) throw new InvalidOperationException();
        manager.Deregister(id);
    }

    public StateAction CommitField(Guid id, string identifier, JsonProperty value)
    {
        if (!Data.TryGetValue(UserIdentifier, out ActionManager? manager)) throw new InvalidOperationException();
        manager.SetDataValue(id, identifier, value.GetValue());

        return GenerateActionState(id, manager);
    }

    public object? Submit(Guid id)
    {
        if (!Data.TryGetValue(UserIdentifier, out ActionManager? manager)) throw new InvalidOperationException();

        EntitySchemaAction action = manager.GetAction(id);
        IDictionary<string, object?> data = manager.GetData(id);
        IDictionary<string, object?> context = manager.GetContext(id);

        foreach (EntityJunctionSchemaActionHasDynamicValueCode item in action.DynamicValueCodeList ?? [])
        {
            if (item.Relation == null) continue;
            serviceDynamicValue.Execute(item.Relation.SchemaIdentifier, data, context);
        }

        return null;
    }

    private StateAction GenerateActionState(Guid id, ActionManager manager)
    {
        EntitySchemaAction entityAction = manager.GetAction(id);
        IDictionary<string, object?> data = manager.GetData(id);
        IDictionary<string, object?> context = manager.GetContext(id);

        Dictionary<string, StateActionField> collectionField = [];
        foreach (EntityJunctionSchemaActionHasActionStep junctionStep in entityAction.ActionStepList ?? [])
        {
            EntitySchemaActionStep entityActionStep = junctionStep.Relation ?? throw new InvalidOperationException();
            foreach (EntityJunctionSchemaActionStepHasActionInput junctionInput in entityActionStep.ActionInputList ?? [])
            {
                EntitySchemaActionInput entityActionInput = junctionInput.Relation ?? throw new InvalidOperationException();

                collectionField[entityActionInput.SchemaIdentifier] = new()
                {
                    Title = serviceDynamicValue.ResolveAsString(entityActionInput.TitleDynamic?.GetValue()) ?? string.Empty,
                    Description = serviceDynamicValue.ResolveAsString(entityActionInput.DescriptionDynamic?.GetValue()) ?? string.Empty,
                    ErrorList = [],
                    IsActive = true,
                    Value = new(data[entityActionInput.SchemaIdentifier]),
                    AttributeCollection = GenerateStateActionFieldAttributeCollection(entityActionInput.AttributeList)
                };
            }
        }

        return new()
        {
            Title = serviceDynamicValue.ResolveAsString(entityAction.TitleDynamic?.GetValue(), data, context) ?? string.Empty,
            Description = serviceDynamicValue.ResolveAsString(entityAction.DescriptionDynamic?.GetValue()) ?? string.Empty,
            FieldCollection = collectionField
        };
    }

    private Dictionary<string, StateActionFieldAttribute> GenerateStateActionFieldAttributeCollection(List<EntityAssociationSchemaActionInputHasAttribute.Discriminator>? list)
    {
        if (list == null) return [];

        Dictionary<string, (EntitySchemaAttribute Attribute, List<JsonProperty> List)> result = [];

        foreach (EntityAssociationSchemaActionInputHasAttribute.Discriminator junctionActionInputAttribute in list)
        {
            EntityAssociationSchemaActionInputHasAttribute entityActionInputAttribute = junctionActionInputAttribute.GetValue();
            EntitySchemaAttribute entityAttribute = entityActionInputAttribute.Relation ?? throw new InvalidOperationException();

            if (!result.TryGetValue(entityAttribute.SchemaIdentifier, out (EntitySchemaAttribute Attribute, List<JsonProperty> List) stateActionFieldAttribute))
            {
                stateActionFieldAttribute = result[entityAttribute.SchemaIdentifier] = (entityAttribute, []);
            }

            stateActionFieldAttribute.List.Add(GetAttributeValue(junctionActionInputAttribute));
        }

        return result.ToDictionary(x => x.Key, y => new StateActionFieldAttribute
        {
            Value = y.Value.List,
            Type = y.Value.Attribute.Type,
            IsList = y.Value.Attribute.IsValueList,
            Order = y.Value.Attribute.Order
        });
    }

    private JsonProperty GetAttributeValue(EntityAssociationSchemaActionInputHasAttribute.Discriminator attribute)
    {
        return new(attribute.GetValue() switch
        {
            EntityAssociationSchemaActionInputHasAttributeDynamicValue value => serviceDynamicValue.Resolve(value.Value?.GetValue()),
            EntityAssociationSchemaActionInputHasAttributeInteger value => value.Value,
            EntityAssociationSchemaActionInputHasAttributeString value => value.Value,
            _ => throw new InvalidOperationException()
        });
    }
}
