using Noxy.NET.EntityManagement.Domain.Entities.Schemas;

namespace Noxy.NET.EntityManagement.Application.Interfaces.Services;

public interface ISchemaValidatorService
{
    void ValidateSchemaChange(EntitySchema schema, string inactiveParameter, string deactivatedParameter);
    void ValidateIsInactive(EntitySchema schema);
    void ValidateInactiveSchemaSystemParameter(EntitySchema schema, string parameter);
    void ValidateDeactivedSchemaSystemParameter(EntitySchema schema, string parameter);
}
