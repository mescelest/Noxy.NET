using Noxy.NET.EntityManagement.Domain.Entities.Schemas.Junctions;

namespace Noxy.NET.EntityManagement.Application.Interfaces.Repositories;

public interface IJunctionRepository
{
    Task ClearSchemaContextHasElementByEntityID(Guid id);
    Task<EntityJunctionSchemaContextHasElement> Create(EntityJunctionSchemaContextHasElement entity);

    Task ClearSchemaElementHasPropertyByEntityID(Guid id);
    Task<EntityJunctionSchemaElementHasProperty> Create(EntityJunctionSchemaElementHasProperty entity);

    Task<List<EntityJunctionSchemaElementHasProperty>> RelateElementToPropertyList(Guid entityGuid, IEnumerable<Guid> listGuid);
    Task<List<EntityJunctionSchemaContextHasElement>> RelateContextToElement(Guid entityGuid, IEnumerable<Guid> listGuid);
}
