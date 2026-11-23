using Noxy.NET.EntityManagement.Domain.Entities;

namespace Noxy.NET.EntityManagement.Domain.Interfaces;

// Stores the data. Nothing is ever changed or deleted in place: a change ends the row, which moves to the history, and a new row takes its place.
// Every call that writes does all of its work or none of it. The repository does not check the data against the schema; that is for the layer above.
public interface IDataRepository
{
    Task<DataElement> CreateElement(DataElement element, CancellationToken cancellationToken = default);

    // The element and its first values, all or nothing, so there is never an element that is half made. The values must be of the element and must not repeat a property.
    Task<DataElement> CreateElement(DataElement element, IReadOnlyList<DataProperty> propertyList, CancellationToken cancellationToken = default);

    Task<DataElement?> FindElement(Guid id, CancellationToken cancellationToken = default);

    // The elements of one kind in the schema, oldest first, a page at a time.
    Task<List<DataElement>> GetElementList(string schemaIdentifier, int skip, int take, CancellationToken cancellationToken = default);

    Task<int> GetElementCount(string schemaIdentifier, CancellationToken cancellationToken = default);

    // Ends the element and every property of it: all of their rows move to the history. False when there is no such element.
    Task<bool> EndElement(Guid id, Guid endedByUserID, CancellationToken cancellationToken = default);

    // The current value of every property of the owner, of every type.
    Task<List<DataProperty>> GetPropertyList(Guid ownerID, CancellationToken cancellationToken = default);

    // The same for several owners in one query, for a list of elements.
    Task<List<DataProperty>> GetPropertyList(IReadOnlyCollection<Guid> ownerIDList, CancellationToken cancellationToken = default);

    // Sets and clears several properties of an element in one step, all or nothing, and only while the element exists (it is checked in the same transaction, so a value is never added to an element that has ended). False when it does not exist.
    // Each value in setList becomes the current one and the one it replaces moves to the history, as in SetProperty; each identifier in clearList ends its value, as in ClearProperty, and is ignored when it has none.
    // A property cannot be in both lists or twice in one, and every value must be of the element.
    Task<bool> ChangeElementProperties(Guid elementID, IReadOnlyList<DataProperty> setList, IReadOnlyList<string> clearList, Guid endedByUserID, CancellationToken cancellationToken = default);

    // The new row becomes the current value of that property of that owner. The row it replaces, if there is one, moves to the history, ended by the user and at the time of the new row.
    Task<TProperty> SetProperty<TProperty>(TProperty property, CancellationToken cancellationToken = default) where TProperty : DataProperty;

    // Ends the current value without a replacement. False when the property has no current value.
    Task<bool> ClearProperty<TProperty>(Guid ownerID, string propertyIdentifier, Guid endedByUserID, CancellationToken cancellationToken = default) where TProperty : DataProperty;

    Task<HistoryElement?> FindElementHistory(Guid id, CancellationToken cancellationToken = default);

    // The values that have ended, of every property of the owner, in the order they ended.
    Task<List<HistoryProperty>> GetPropertyHistoryList(Guid ownerID, CancellationToken cancellationToken = default);
}
