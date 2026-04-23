using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Noxy.NET.EntityManagement.Domain.Interfaces;
using Noxy.NET.EntityManagement.Persistence.Abstractions.Tables;
using Noxy.NET.EntityManagement.Persistence.Tables.Schemas.Junctions;

namespace Noxy.NET.EntityManagement.Persistence.Tables.Schemas;

[Table(nameof(TableSchemaContext))]
public class TableSchemaContext : BaseTableSchema, ISchemaMetadata, ISchemaPresentation
{
    [Required]
    [MaxLength(DefaultNameLength)]
    public required string Name { get; set; }

    [Required]
    [MaxLength(DefaultNoteLength)]
    public string Note { get; set; } = string.Empty;

    [Required]
    public TableSchemaParameterText? TitleTextParameter { get; set; }
    public Guid TitleTextParameterID { get; set; }

    public TableSchemaParameterText? DescriptionTextParameter { get; set; }
    public Guid? DescriptionTextParameterID { get; set; }

    public ICollection<TableJunctionSchemaContextHasElement>? ElementList { get; set; }

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
