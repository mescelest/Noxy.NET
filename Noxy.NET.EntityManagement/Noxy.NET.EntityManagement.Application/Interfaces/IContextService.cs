using Noxy.NET.EntityManagement.Application.Models;

namespace Noxy.NET.EntityManagement.Application.Interfaces;

// Reads the data a page of a context needs. A context is a flat group of elements that belong together; it has no depth.
// A context or element that is not defined is a mistake of the code, so it throws KeyNotFoundException. A kind that is not in the context is a bad request: DataValidationException with WrongElement.
public interface IContextService
{
    // Without a starting element: the elements of one kind of the context, oldest first, a page at a time, each with its values.
    Task<ElementPageModel> GetElementPage(string contextIdentifier, string elementIdentifier, int skip, int take, CancellationToken cancellationToken = default);

    // With a starting element, which must be of a kind in the context: the element with its values, and the elements directly related to it (one step, either way) whose kind is in the context, grouped by relation and direction.
    // A fixed number of queries however many elements are related. Null when there is no such element, or it has ended.
    // Nothing is guessed: two relations that join the same kinds are two groups. The groups follow the order of the definitions, forward before inverse.
    Task<ContextModel?> GetContext(string contextIdentifier, Guid elementID, CancellationToken cancellationToken = default);
}
