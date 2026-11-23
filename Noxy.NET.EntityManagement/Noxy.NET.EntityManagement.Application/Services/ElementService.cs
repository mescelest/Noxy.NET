using Noxy.NET.EntityManagement.Application.Interfaces;
using Noxy.NET.EntityManagement.Application.Models;
using Noxy.NET.EntityManagement.Definitions;
using Noxy.NET.EntityManagement.Definitions.Models;
using Noxy.NET.EntityManagement.Domain.Entities;
using Noxy.NET.EntityManagement.Domain.Interfaces;

namespace Noxy.NET.EntityManagement.Application.Services;

public sealed class ElementService(DefinitionRegistry definition, IDataRepository repository, TimeProvider timeProvider) : IElementService
{
    public async Task<ElementModel> CreateElement(string elementIdentifier, IReadOnlyDictionary<string, object?> valueMap, Guid userID, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(valueMap);

        DefinitionElement elementDefinition = definition.GetElement(elementIdentifier);
        DateTime now = timeProvider.GetUtcNow().UtcDateTime;
        DataElement element = new() { SchemaIdentifier = elementIdentifier, CreatedByUserID = userID, TimeCreated = now };

        List<DataError> errorList = [];
        List<DataProperty> setList = ValueChanger.ForCreate(definition, OwnerLabel(elementIdentifier), elementDefinition.PropertyList, valueMap, element.ID, userID, now, errorList);

        if (errorList.Count > 0) throw new DataValidationException(errorList);

        await repository.CreateElement(element, setList, cancellationToken);

        return ToModel(element, setList);
    }

    public async Task<ElementModel?> FindElement(Guid id, CancellationToken cancellationToken = default)
    {
        DataElement? element = await repository.FindElement(id, cancellationToken);
        if (element is null) return null;

        return ToModel(element, await repository.GetPropertyList(id, cancellationToken));
    }

    public async Task<ElementPageModel> GetElementPage(string elementIdentifier, int skip, int take, CancellationToken cancellationToken = default)
    {
        definition.GetElement(elementIdentifier);

        List<DataElement> elementList = await repository.GetElementList(elementIdentifier, skip, take, cancellationToken);
        int totalCount = await repository.GetElementCount(elementIdentifier, cancellationToken);

        List<DataProperty> propertyList = await repository.GetPropertyList(elementList.Select(x => x.ID).ToList(), cancellationToken);
        ILookup<Guid, DataProperty> propertyLookup = propertyList.ToLookup(x => x.OwnerID);

        return new(elementList.Select(x => ToModel(x, propertyLookup[x.ID])).ToList(), totalCount);
    }

    public async Task<ElementModel?> ChangeElement(Guid id, IReadOnlyDictionary<string, object?> valueMap, Guid userID, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(valueMap);

        DataElement? element = await repository.FindElement(id, cancellationToken);
        if (element is null) return null;

        DefinitionElement elementDefinition = definition.GetElement(element.SchemaIdentifier);
        List<DataProperty> currentList = await repository.GetPropertyList(id, cancellationToken);

        List<DataError> errorList = [];
        (List<DataProperty> setList, List<string> clearList) = ValueChanger.ForChange(definition, OwnerLabel(element.SchemaIdentifier), elementDefinition.PropertyList, valueMap, currentList, id, userID, timeProvider.GetUtcNow().UtcDateTime, errorList);

        if (errorList.Count > 0) throw new DataValidationException(errorList);
        if (setList.Count == 0 && clearList.Count == 0) return ToModel(element, currentList);

        if (!await repository.ChangeElementProperties(id, setList, clearList, userID, cancellationToken)) return null;

        return ToModel(element, await repository.GetPropertyList(id, cancellationToken));
    }

    public Task<bool> EndElement(Guid id, Guid userID, CancellationToken cancellationToken = default)
    {
        return repository.EndElement(id, userID, cancellationToken);
    }

    public async Task<IReadOnlyList<PropertyHistoryModel>> GetPropertyHistory(Guid elementID, CancellationToken cancellationToken = default)
    {
        List<HistoryProperty> historyList = await repository.GetPropertyHistoryList(elementID, cancellationToken);

        return historyList
            .Select(x => new PropertyHistoryModel(x.PropertyIdentifier, PropertyValueConverter.GetKind(x), PropertyValueConverter.GetValue(x), x.TimeCreated, x.CreatedByUserID, x.TimeEnded, x.EndedByUserID))
            .ToList();
    }

    private static string OwnerLabel(string elementIdentifier) => $"The element '{elementIdentifier}'";

    private static ElementModel ToModel(DataElement element, IEnumerable<DataProperty> propertyList)
    {
        return new(element.ID, element.SchemaIdentifier, element.TimeCreated, element.CreatedByUserID, ValueChanger.ToValueMap(propertyList));
    }
}
