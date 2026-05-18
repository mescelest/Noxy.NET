using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Noxy.NET.EntityManagement.Domain.Interfaces;
using Noxy.NET.EntityManagement.Persistence.Abstractions.Tables;
using Noxy.NET.EntityManagement.Persistence.Tables.Schemas.Junctions;

namespace Noxy.NET.EntityManagement.Persistence.Tables.Schemas;

[Table(nameof(TableSchemaElement))]
[Index(nameof(Name))]
[Index(nameof(Weight))]
public class TableSchemaElement : BaseTableSchemaPresentation, ISchemaMetadata, ISchemaOrdering
{
    [Required]
    [MaxLength(DefaultNameLength)]
    public required string Name { get; set; }

    [Required]
    [MaxLength(DefaultNoteLength)]
    public string Note { get; set; } = string.Empty;

    [Required]
    public required int Weight { get; set; }

    public ICollection<TableSchemaElementHasProperty>? PropertyList { get; set; }

    public ICollection<TableSchemaContextHasElement>? RelationContextList { get; set; }

    public TableSchemaElement Clone(Guid? schemaID = null)
    {
        return new()
        {
            SchemaID = schemaID ?? SchemaID,
            SchemaIdentifier = SchemaIdentifier,
            Name = Name,
            Weight = Weight,
            Note = Note,
            TitleParameterTextID = TitleParameterTextID,
            DescriptionParameterTextID = DescriptionParameterTextID,
        };
    }
}
