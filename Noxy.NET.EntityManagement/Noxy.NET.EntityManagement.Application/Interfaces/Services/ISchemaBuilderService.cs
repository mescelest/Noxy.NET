using Noxy.NET.EntityManagement.Domain.Entities.Schemas;

namespace Noxy.NET.EntityManagement.Application.Interfaces.Services;

public interface ISchemaBuilderService
{
    Task<EntitySchema> ConstructSchema(Guid? id = null);
}
