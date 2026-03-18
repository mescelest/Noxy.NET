using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Noxy.NET.EntityManagement.Persistence.Abstractions.Tables;
using Noxy.NET.EntityManagement.Persistence.Tables.Schemas.Discriminators;

namespace Noxy.NET.EntityManagement.Persistence.Tables.Schemas;

[Table(nameof(TableSchema))]
public class TableSchema : BaseTable
{
    public required FeatureDescription Description { get; set; }

    [Required]
    public required bool IsActive { get; set; }

    public required DateTime? TimeActivated { get; set; }

    public ICollection<TableSchemaContext>? ContextList { get; set; }
    public ICollection<TableSchemaElement>? ElementList { get; set; }
    public ICollection<TableSchemaParameter>? ParameterList { get; set; }
    public ICollection<TableSchemaProperty>? PropertyList { get; set; }

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
}
