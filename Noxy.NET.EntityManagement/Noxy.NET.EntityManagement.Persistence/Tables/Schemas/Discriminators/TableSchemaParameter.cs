using System.ComponentModel.DataAnnotations;
using Noxy.NET.EntityManagement.Domain.Entities.Schemas;
using Noxy.NET.EntityManagement.Persistence.Abstractions.Tables;

namespace Noxy.NET.EntityManagement.Persistence.Tables.Schemas.Discriminators;

public abstract class TableSchemaParameter : BaseTableSchema
{
    [Required]
    public required bool IsSystemDefined { get; set; }

    [Required]
    public required bool IsApprovalRequired { get; set; }

    public static readonly IReadOnlyDictionary<string, Type> TypeMap = new Dictionary<string, Type>
    {
        { nameof(EntitySchemaParameterStyle), typeof(TableSchemaParameterStyle) },
        { nameof(EntitySchemaParameterSystem), typeof(TableSchemaParameterSystem) },
        { nameof(EntitySchemaParameterText), typeof(TableSchemaParameterText) }
    };
}
