using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Noxy.NET.EntityManagement.Domain.Entities.Schemas;
using Noxy.NET.EntityManagement.Persistence.Abstractions.Tables;

namespace Noxy.NET.EntityManagement.Persistence.Tables.Schemas.Discriminators;

public abstract class TableSchemaParameter : BaseTableSchema
{
    public required FeatureDescription Description { get; set; }
    public required FeatureOrdering Ordering { get; set; }

    [Required]
    public required bool IsSystemDefined { get; set; }

    [Required]
    public required bool IsApprovalRequired { get; set; }

    [NotMapped]
    public string Name
    {
        get => Description.Name;
        set => Description.Name = value;
    }

    [NotMapped]
    public string Note
    {
        get => Description.Name;
        set => Description.Name = value;
    }

    [NotMapped]
    public int Order
    {
        get => Ordering.Value;
        set => Ordering.Value = value;
    }

    public static readonly IReadOnlyDictionary<string, Type> TypeMap = new Dictionary<string, Type>
    {
        { nameof(EntitySchemaParameterStyle), typeof(TableSchemaParameterStyle) },
        { nameof(EntitySchemaParameterSystem), typeof(TableSchemaParameterSystem) },
        { nameof(EntitySchemaParameterText), typeof(TableSchemaParameterText) }
    };
}
