using Noxy.NET.EntityManagement.Application.Interfaces.Services;
using Noxy.NET.EntityManagement.Domain.Entities.Schemas;

namespace Noxy.NET.EntityManagement.Application.Services;

public class SchemaValidatorService(IParameterService serviceParameter) : ISchemaValidatorService
{
    public void ValidateSchemaChange(EntitySchema schema, string inactiveParameter, string deactivatedParameter)
    {
        ValidateIsInactive(schema);
        ValidateInactiveSchemaSystemParameter(schema, inactiveParameter);
        ValidateDeactivedSchemaSystemParameter(schema, deactivatedParameter);
    }

    public void ValidateIsInactive(EntitySchema schema)
    {
        if (schema.IsActive)
        {
            throw new InvalidOperationException("Cannot change a Schema that is currently active.");
        }
    }

    public void ValidateInactiveSchemaSystemParameter(EntitySchema schema, string parameter)
    {
        if (schema.TimeActivated == null && serviceParameter.TryGetParameterSystemValueBoolean(parameter, out bool valueInactive) && !valueInactive)
        {
            throw new InvalidOperationException($"System parameter '{parameter}' prevents this action from being completed.");
        }
    }

    public void ValidateDeactivedSchemaSystemParameter(EntitySchema schema, string parameter)
    {
        if (schema.TimeActivated != null && serviceParameter.TryGetParameterSystemValueBoolean(parameter, out bool valueDeactivated) && !valueDeactivated)
        {
            throw new InvalidOperationException($"System parameter '{parameter}' prevents this action from being completed.");
        }
    }
}
