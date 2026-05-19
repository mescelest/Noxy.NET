using System.ComponentModel.DataAnnotations;
using Noxy.NET.EntityManagement.Domain.Enums;
using Noxy.NET.EntityManagement.Persistence.Tables.Schemas.Discriminators;

namespace Noxy.NET.EntityManagement.Persistence.Tables.Schemas;

public class TableSchemaParameterSystem : TableSchemaParameter
{
    [Required]
    public required ParameterSystemTypeEnum Type { get; set; }

    [Required]
    public required bool IsPublic { get; set; }

    public override TableSchemaParameterSystem Clone(Guid? schemaID = null)
    {
        return new()
        {
            SchemaID = schemaID ?? SchemaID,
            SchemaIdentifier = SchemaIdentifier,
            Name = Name,
            Note = Note,
            Type = Type,
            IsPublic = IsPublic,
            IsApprovalRequired = IsApprovalRequired,
            IsSystemDefined = IsSystemDefined,
        };
    }
}
