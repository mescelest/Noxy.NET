using Noxy.NET.EntityManagement.Application.Enums;
using Noxy.NET.EntityManagement.Application.Interfaces;
using Noxy.NET.EntityManagement.Application.Models;
using Noxy.NET.EntityManagement.Definitions;
using Noxy.NET.EntityManagement.Definitions.Models;
using Noxy.NET.EntityManagement.Domain.Entities;
using Noxy.NET.EntityManagement.Domain.Interfaces;

namespace Noxy.NET.EntityManagement.Application.Services;

public sealed class ContextService(DefinitionRegistry definition, IElementService serviceElement, IDataRepository dataRepository, IRelationRepository relationRepository) : IContextService
{
    public Task<ElementPageModel> GetElementPage(string contextIdentifier, string elementIdentifier, int skip, int take, CancellationToken cancellationToken = default)
    {
        DefinitionContext contextDefinition = definition.GetContext(contextIdentifier);
        definition.GetElement(elementIdentifier);
        EnsureInContext(contextDefinition, elementIdentifier);

        return serviceElement.GetElementPage(elementIdentifier, skip, take, cancellationToken);
    }

    public async Task<ContextModel?> GetContext(string contextIdentifier, Guid elementID, CancellationToken cancellationToken = default)
    {
        DefinitionContext contextDefinition = definition.GetContext(contextIdentifier);

        DataElement? start = await dataRepository.FindElement(elementID, cancellationToken);
        if (start is null) return null;

        EnsureInContext(contextDefinition, start.SchemaIdentifier);

        // Query 1: every current relation of the starting element, in both directions. What is in the context is decided here, from the definitions.
        List<DataRelation> relationList = await relationRepository.GetRelationList(elementID, null, cancellationToken);
        List<GroupPlan> planList = GetPlanList(contextDefinition, start.SchemaIdentifier);

        List<(GroupPlan Plan, DataRelation Relation, Guid OtherID)> linkList = [];
        foreach (GroupPlan plan in planList)
        {
            foreach (DataRelation relation in relationList.Where(x => x.RelationIdentifier == plan.Relation.Identifier))
            {
                (Guid thisEndID, Guid otherEndID) = plan.Direction == RelationDirectionEnum.Forward ? (relation.FromElementID, relation.ToElementID) : (relation.ToElementID, relation.FromElementID);
                if (thisEndID == elementID) linkList.Add((plan, relation, otherEndID));
            }
        }

        // Query 2: the related elements. Query 3: the values of those elements and the attributes of those relations, together.
        List<Guid> elementIDList = linkList.Select(x => x.OtherID).Append(elementID).Distinct().ToList();
        Dictionary<Guid, DataElement> elementMap = (await dataRepository.GetElementList(elementIDList, cancellationToken)).ToDictionary(x => x.ID);

        List<Guid> ownerIDList = elementIDList.Concat(linkList.Select(x => x.Relation.ID)).ToList();
        ILookup<Guid, DataProperty> propertyLookup = (await dataRepository.GetPropertyList(ownerIDList, cancellationToken)).ToLookup(x => x.OwnerID);

        List<ContextRelationGroupModel> groupList = planList
            .Select(plan => new ContextRelationGroupModel(
                plan.Relation.Identifier,
                plan.Direction,
                plan.ElementIdentifier,
                linkList
                    .Where(x => x.Plan == plan && elementMap.ContainsKey(x.OtherID))
                    .Select(x => new ContextRelatedModel(x.Relation.ID, x.Relation.TimeCreated, ValueChanger.ToValueMap(propertyLookup[x.Relation.ID]), ToModel(elementMap[x.OtherID], propertyLookup)))
                    .ToList()))
            .ToList();

        return new(contextIdentifier, ToModel(start, propertyLookup), groupList);
    }

    // The relations the definition allows from this kind of element, within the context, each way they read. Some of them may have no links yet.
    private List<GroupPlan> GetPlanList(DefinitionContext contextDefinition, string startElementIdentifier)
    {
        List<GroupPlan> planList = [];

        foreach (DefinitionRelation relation in definition.RelationList)
        {
            if (relation.FromElement == startElementIdentifier && contextDefinition.ElementList.Contains(relation.ToElement))
            {
                planList.Add(new(relation, RelationDirectionEnum.Forward, relation.ToElement));
            }

            if (relation.ToElement == startElementIdentifier && contextDefinition.ElementList.Contains(relation.FromElement))
            {
                planList.Add(new(relation, RelationDirectionEnum.Inverse, relation.FromElement));
            }
        }

        return planList;
    }

    private static void EnsureInContext(DefinitionContext contextDefinition, string elementIdentifier)
    {
        if (contextDefinition.ElementList.Contains(elementIdentifier)) return;

        throw new DataValidationException([new(null, DataErrorEnum.WrongElement, $"The element '{elementIdentifier}' is not in the context '{contextDefinition.Identifier}'.")]);
    }

    private static ElementModel ToModel(DataElement element, ILookup<Guid, DataProperty> propertyLookup)
    {
        return new(element.ID, element.SchemaIdentifier, element.TimeCreated, element.CreatedByUserID, ValueChanger.ToValueMap(propertyLookup[element.ID]));
    }

    private sealed record GroupPlan(DefinitionRelation Relation, RelationDirectionEnum Direction, string ElementIdentifier);
}
