using Noxy.NET.EntityManagement.Domain.Entities.Schemas.Junctions;

namespace Noxy.NET.EntityManagement.Domain.Interfaces.Repositories;

public interface IJunctionRepository
{
    Task ClearSchemaContextHasElementByEntityID(Guid id);
    Task<EntitySchemaContextHasElement> Create(EntitySchemaContextHasElement entity);

    Task ClearSchemaElementHasPropertyByEntityID(Guid id);
    Task<EntitySchemaElementHasProperty> Create(EntitySchemaElementHasProperty entity);

    Task<List<EntitySchemaElementHasProperty>> RelateElementToPropertyList(Guid entityGuid, IEnumerable<Guid> listGuid);
    Task<List<EntitySchemaContextHasElement>> RelateContextToElement(Guid entityGuid, IEnumerable<Guid> listGuid);
}
