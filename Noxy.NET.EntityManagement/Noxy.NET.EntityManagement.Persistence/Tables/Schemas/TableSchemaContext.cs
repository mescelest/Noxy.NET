using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Noxy.NET.EntityManagement.Domain.Interfaces;
using Noxy.NET.EntityManagement.Persistence.Abstractions.Tables;
using Noxy.NET.EntityManagement.Persistence.Tables.Schemas.Junctions;

namespace Noxy.NET.EntityManagement.Persistence.Tables.Schemas;

[Table(nameof(TableSchemaContext))]
[Index(nameof(Name))]
public class TableSchemaContext : BaseTableSchemaPresentation, ISchemaMetadata, ISchemaPresentation
{
    [Required]
    [MaxLength(DefaultNameLength)]
    public required string Name { get; set; }

    [Required]
    [MaxLength(DefaultNoteLength)]
    public string Note { get; set; } = string.Empty;

    public ICollection<TableSchemaContextHasElement>? ElementList { get; set; }

    public TableSchemaContext Clone(Guid? schemaID = null)
    {
        return new()
        {
            SchemaID = schemaID ?? SchemaID,
            SchemaIdentifier = SchemaIdentifier,
            Name = Name,
            Note = Note,
            TitleTextParameterID = TitleTextParameterID,
            DescriptionTextParameterID = DescriptionTextParameterID,
        };
    }
}
