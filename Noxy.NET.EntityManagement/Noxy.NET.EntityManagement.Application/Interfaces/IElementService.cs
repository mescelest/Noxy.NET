using Noxy.NET.EntityManagement.Application.Models;

namespace Noxy.NET.EntityManagement.Application.Interfaces;

// Creates, changes and ends the elements, and reads them, with every value checked against the definition.
// The values are given by the identifier of the property. A value that is null, or a string that is empty, means "no value": on a new element the property is left out, and on a change it is cleared.
// A bad value is not an exception of its own kind: the whole call throws an DataValidationException with every problem, and nothing is stored. An element or property that is not defined is a mistake of the code, so it throws KeyNotFoundException.
public interface IElementService
{
    // Every required property must have a value. The element and its values are stored together, or not at all.
    Task<ElementModel> CreateElement(string elementIdentifier, IReadOnlyDictionary<string, object?> valueMap, Guid userID, CancellationToken cancellationToken = default);

    Task<ElementModel?> FindElement(Guid id, CancellationToken cancellationToken = default);

    // The elements of one kind, oldest first, a page at a time, each with its values.
    Task<ElementPageModel> GetElementPage(string elementIdentifier, int skip, int take, CancellationToken cancellationToken = default);

    // Changes the values that are given and leaves the others as they are. A value that is the same as the current one is not a change and leaves nothing in the history. The changes are stored together, or not at all.
    // A required property cannot be cleared. Null when there is no such element, or it has ended.
    Task<ElementModel?> ChangeElement(Guid id, IReadOnlyDictionary<string, object?> valueMap, Guid userID, CancellationToken cancellationToken = default);

    // Ends the element and all of its values, which move to the history. False when there is no such element.
    Task<bool> EndElement(Guid id, Guid userID, CancellationToken cancellationToken = default);

    // The values that have ended, of a current element or of one that has ended, in the order they ended.
    Task<IReadOnlyList<PropertyHistoryModel>> GetPropertyHistory(Guid elementID, CancellationToken cancellationToken = default);
}
