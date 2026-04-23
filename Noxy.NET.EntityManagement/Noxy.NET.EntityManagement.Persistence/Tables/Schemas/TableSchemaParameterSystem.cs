using Noxy.NET.EntityManagement.Persistence.Tables.Schemas.Discriminators;

namespace Noxy.NET.EntityManagement.Persistence.Tables.Schemas;

public class TableSchemaParameterSystem : TableSchemaParameter
{
    public override TableSchemaParameterSystem Clone(Guid? schemaID = null)
    {
        return new()
        {
            SchemaID = schemaID ?? SchemaID,
            SchemaIdentifier = SchemaIdentifier,
            Name = Name,
            Note = Note,
            IsApprovalRequired = IsApprovalRequired,
            IsSystemDefined = IsSystemDefined,
        };
    }
}
