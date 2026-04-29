using System.ComponentModel.DataAnnotations;
using Noxy.NET.EntityManagement.Domain.Enums;
using Noxy.NET.EntityManagement.Persistence.Tables.Schemas.Discriminators;

namespace Noxy.NET.EntityManagement.Persistence.Tables.Schemas;

public class TableSchemaParameterText : TableSchemaParameter
{
    [Required]
    public required ParameterTextTypeEnum Type { get; set; }

    public override TableSchemaParameterText Clone(Guid? schemaID = null)
    {
        return new()
        {
            SchemaID = schemaID ?? SchemaID,
            SchemaIdentifier = SchemaIdentifier,
            Name = Name,
            Note = Note,
            Type = Type,
            IsApprovalRequired = IsApprovalRequired,
            IsSystemDefined = IsSystemDefined,
        };
    }
}
