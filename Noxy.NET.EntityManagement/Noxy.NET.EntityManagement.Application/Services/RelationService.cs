using Noxy.NET.EntityManagement.Application.Enums;
using Noxy.NET.EntityManagement.Application.Interfaces;
using Noxy.NET.EntityManagement.Application.Models;
using Noxy.NET.EntityManagement.Definitions;
using Noxy.NET.EntityManagement.Definitions.Enums;
using Noxy.NET.EntityManagement.Definitions.Models;
using Noxy.NET.EntityManagement.Domain.Entities;
using Noxy.NET.EntityManagement.Domain.Enums;
using Noxy.NET.EntityManagement.Domain.Interfaces;
using Noxy.NET.EntityManagement.Domain.Models;

namespace Noxy.NET.EntityManagement.Application.Services;

public sealed class RelationService(DefinitionRegistry definition, IDataRepository dataRepository, IRelationRepository repository, TimeProvider timeProvider) : IRelationService
{
    public async Task<RelationModel> CreateRelation(string relationIdentifier, Guid fromElementID, Guid toElementID, IReadOnlyDictionary<string, object?> attributeMap, Guid userID, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(attributeMap);

        DefinitionRelation relationDefinition = definition.GetRelation(relationIdentifier);
        DateTime now = timeProvider.GetUtcNow().UtcDateTime;
        DataRelation relation = new() { RelationIdentifier = relationIdentifier, FromElementID = fromElementID, ToElementID = toElementID, CreatedByUserID = userID, TimeCreated = now };

        List<DataError> errorList = [];
        await CheckElement(fromElementID, relationDefinition.FromElement, "reads from", errorList, cancellationToken);
        await CheckElement(toElementID, relationDefinition.ToElement, "reads to", errorList, cancellationToken);

        List<DataProperty> attributeList = ValueChanger.ForCreate(definition, OwnerLabel(relationIdentifier), relationDefinition.AttributeList, attributeMap, relation.ID, userID, now, errorList);

        if (errorList.Count > 0) throw new DataValidationException(errorList);

        // The existence of the elements and the limits are checked again when the relation is stored, in the same transaction, because they can change between the check above and the insert.
        (int? maxLinksOfFrom, int? maxLinksOfTo) = GetLimits(relationDefinition.Cardinality);
        CreateRelationResult result = await repository.CreateRelation(relation, attributeList, maxLinksOfFrom, maxLinksOfTo, cancellationToken);

        if (result.Status != RelationCreateStatusEnum.Created)
        {
            throw new DataValidationException([ToError(result.Status, relationIdentifier)]);
        }

        return ToModel(relation, attributeList);
    }

    public async Task<RelationModel?> FindRelation(Guid id, CancellationToken cancellationToken = default)
    {
        DataRelation? relation = await repository.FindRelation(id, cancellationToken);
        if (relation is null) return null;

        return ToModel(relation, await dataRepository.GetPropertyList(id, cancellationToken));
    }

    public async Task<IReadOnlyList<RelationModel>> GetRelationList(Guid elementID, string? relationIdentifier = null, CancellationToken cancellationToken = default)
    {
        List<DataRelation> relationList = await repository.GetRelationList(elementID, relationIdentifier, cancellationToken);

        // The attributes of all of the relations in one query.
        List<DataProperty> attributeList = await dataRepository.GetPropertyList(relationList.Select(x => x.ID).ToList(), cancellationToken);
        ILookup<Guid, DataProperty> attributeLookup = attributeList.ToLookup(x => x.OwnerID);

        return relationList.Select(x => ToModel(x, attributeLookup[x.ID])).ToList();
    }

    public async Task<RelationModel?> ChangeRelation(Guid id, IReadOnlyDictionary<string, object?> attributeMap, Guid userID, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(attributeMap);

        DataRelation? relation = await repository.FindRelation(id, cancellationToken);
        if (relation is null) return null;

        DefinitionRelation relationDefinition = definition.GetRelation(relation.RelationIdentifier);
        List<DataProperty> currentList = await dataRepository.GetPropertyList(id, cancellationToken);

        List<DataError> errorList = [];
        (List<DataProperty> setList, List<string> clearList) =
            ValueChanger.ForChange(definition, OwnerLabel(relation.RelationIdentifier), relationDefinition.AttributeList, attributeMap, currentList, id, userID, timeProvider.GetUtcNow().UtcDateTime, errorList);

        if (errorList.Count > 0) throw new DataValidationException(errorList);
        if (setList.Count == 0 && clearList.Count == 0) return ToModel(relation, currentList);

        if (!await repository.ChangeRelationAttributes(id, setList, clearList, userID, cancellationToken)) return null;

        return ToModel(relation, await dataRepository.GetPropertyList(id, cancellationToken));
    }

    public Task<bool> EndRelation(Guid id, Guid userID, CancellationToken cancellationToken = default)
    {
        return repository.EndRelation(id, userID, cancellationToken);
    }

    public async Task<IReadOnlyList<RelationHistoryModel>> GetRelationHistory(Guid elementID, CancellationToken cancellationToken = default)
    {
        List<HistoryRelation> historyList = await repository.GetRelationHistoryList(elementID, cancellationToken);

        return historyList
            .Select(x => new RelationHistoryModel(x.ID, x.RelationIdentifier, x.FromElementID, x.ToElementID, x.TimeCreated, x.CreatedByUserID, x.TimeEnded, x.EndedByUserID))
            .ToList();
    }

    public async Task<IReadOnlyList<PropertyHistoryModel>> GetAttributeHistory(Guid relationID, CancellationToken cancellationToken = default)
    {
        List<HistoryProperty> historyList = await dataRepository.GetPropertyHistoryList(relationID, cancellationToken);

        return historyList
            .Select(x => new PropertyHistoryModel(x.PropertyIdentifier, PropertyValueConverter.GetKind(x), PropertyValueConverter.GetValue(x), x.TimeCreated, x.CreatedByUserID, x.TimeEnded, x.EndedByUserID))
            .ToList();
    }

    private async Task CheckElement(Guid elementID, string expectedElementIdentifier, string role, List<DataError> errorList, CancellationToken cancellationToken)
    {
        DataElement? element = await dataRepository.FindElement(elementID, cancellationToken);

        if (element is null)
        {
            errorList.Add(new(null, DataErrorEnum.ElementNotFound, $"The element the relation {role} ({elementID}) does not exist."));
        }
        else if (element.SchemaIdentifier != expectedElementIdentifier)
        {
            errorList.Add(new(null, DataErrorEnum.WrongElement, $"The element the relation {role} must be a '{expectedElementIdentifier}', not a '{element.SchemaIdentifier}'."));
        }
    }

    // How many relations of the kind an element can have, counting the new one. OneToMany reads: one element it reads from has many it reads to, so each element it reads to can only be linked from one.
    private static (int? MaxLinksOfFrom, int? MaxLinksOfTo) GetLimits(RelationCardinalityEnum cardinality) => cardinality switch
    {
        RelationCardinalityEnum.ManyToMany => (null, null),
        RelationCardinalityEnum.OneToMany => (null, 1),
        RelationCardinalityEnum.ManyToOne => (1, null),
        RelationCardinalityEnum.OneToOne => (1, 1),
        _ => throw new NotSupportedException($"{cardinality} is not a cardinality.")
    };

    private static DataError ToError(RelationCreateStatusEnum status, string relationIdentifier) => status switch
    {
        RelationCreateStatusEnum.FromElementNotFound => new(null, DataErrorEnum.ElementNotFound, "The element the relation reads from does not exist."),
        RelationCreateStatusEnum.ToElementNotFound => new(null, DataErrorEnum.ElementNotFound, "The element the relation reads to does not exist."),
        RelationCreateStatusEnum.AlreadyExists => new(null, DataErrorEnum.AlreadyRelated, $"The two elements already have the relation '{relationIdentifier}'."),
        RelationCreateStatusEnum.FromLimitReached => new(null, DataErrorEnum.RelationLimitReached, $"The element the relation reads from cannot have more relations '{relationIdentifier}'."),
        RelationCreateStatusEnum.ToLimitReached => new(null, DataErrorEnum.RelationLimitReached, $"The element the relation reads to cannot have more relations '{relationIdentifier}'."),
        _ => throw new NotSupportedException($"{status} is not a failed status.")
    };

    private static string OwnerLabel(string relationIdentifier) => $"The relation '{relationIdentifier}'";

    private static RelationModel ToModel(DataRelation relation, IEnumerable<DataProperty> attributeList)
    {
        return new(relation.ID, relation.RelationIdentifier, relation.FromElementID, relation.ToElementID, relation.TimeCreated, relation.CreatedByUserID, ValueChanger.ToValueMap(attributeList));
    }
}
