using Noxy.NET.CaseManagement.Domain.Entities.Schemas;

namespace Noxy.NET.CaseManagement.Application.Interfaces.Services;

public interface ISchemaBuilderService
{
    Task<EntitySchema> ConstructSchema(Guid? id = null);
}
