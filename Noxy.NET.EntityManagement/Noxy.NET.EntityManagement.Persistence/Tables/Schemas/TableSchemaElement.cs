using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Noxy.NET.EntityManagement.Domain.Interfaces;
using Noxy.NET.EntityManagement.Persistence.Abstractions.Tables;
using Noxy.NET.EntityManagement.Persistence.Tables.Schemas.Junctions;

namespace Noxy.NET.EntityManagement.Persistence.Tables.Schemas;

[Table(nameof(TableSchemaElement))]
public class TableSchemaElement : BaseTableSchema, ISchemaMetadata, ISchemaOrdering, ISchemaPresentation
{
    [Required]
    [MaxLength(DefaultNameLength)]
    public required string Name { get; set; }

    [Required]
    [MaxLength(DefaultNoteLength)]
    public string Note { get; set; } = string.Empty;

    [Required]
    public required int Order { get; set; }

    [Required]
    public TableSchemaParameterText? TitleTextParameter { get; set; }
    public Guid TitleTextParameterID { get; set; }

    public TableSchemaParameterText? DescriptionTextParameter { get; set; }
    public Guid? DescriptionTextParameterID { get; set; }

    public ICollection<TableJunctionSchemaElementHasElement>? ElementList { get; set; }
    public ICollection<TableJunctionSchemaElementHasProperty>? PropertyList { get; set; }

    public ICollection<TableJunctionSchemaContextHasElement>? RelationContextList { get; set; }
    public ICollection<TableJunctionSchemaElementHasElement>? RelationElementList { get; set; }
}
