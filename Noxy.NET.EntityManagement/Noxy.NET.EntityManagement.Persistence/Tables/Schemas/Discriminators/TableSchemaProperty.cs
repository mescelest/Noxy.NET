using System.ComponentModel.DataAnnotations;
using Noxy.NET.EntityManagement.Domain.Entities.Schemas;
using Noxy.NET.EntityManagement.Domain.Interfaces;
using Noxy.NET.EntityManagement.Persistence.Abstractions.Tables;
using Noxy.NET.EntityManagement.Persistence.Tables.Schemas.Junctions;

namespace Noxy.NET.EntityManagement.Persistence.Tables.Schemas.Discriminators;

public abstract class TableSchemaProperty : BaseTableSchemaPresentation, ISchemaMetadata, ISchemaOrdering, ISchemaPresentation
{
    [Required]
    [MaxLength(DefaultNameLength)]
    public required string Name { get; set; }

    [Required]
    [MaxLength(DefaultNoteLength)]
    public string Note { get; set; } = string.Empty;

    [Required]
    public required int Weight { get; set; }

    [Required]
    public TableSchemaParameterText? TitleTextParameter { get; set; }
    public Guid TitleTextParameterID { get; set; }

    public TableSchemaParameterText? DescriptionTextParameter { get; set; }
    public Guid? DescriptionTextParameterID { get; set; }

    public ICollection<TableJunctionSchemaElementHasProperty>? RelationElementList { get; set; }
    public ICollection<TableJunctionSchemaPropertyCollectionHasProperty>? RelationPropertyCollectionList { get; set; }
    public ICollection<TableJunctionSchemaPropertyTableHasProperty>? RelationPropertyTableList { get; set; }

    public static readonly IReadOnlyDictionary<string, Type> TypeMap = new Dictionary<string, Type>
    {
        { nameof(EntitySchemaPropertyBoolean), typeof(TableSchemaPropertyBoolean) },
        { nameof(EntitySchemaPropertyCollection), typeof(TableSchemaPropertyCollection) },
        { nameof(EntitySchemaPropertyDateTime), typeof(TableSchemaPropertyDateTime) },
        { nameof(EntitySchemaPropertyDecimal), typeof(TableSchemaPropertyDecimal) },
        { nameof(EntitySchemaPropertyImage), typeof(TableSchemaPropertyImage) },
        { nameof(EntitySchemaPropertyInteger), typeof(TableSchemaPropertyInteger) },
        { nameof(EntitySchemaPropertyString), typeof(TableSchemaPropertyString) },
        { nameof(EntitySchemaPropertyTable), typeof(TableSchemaPropertyTable) },
    };

    public abstract TableSchemaProperty Clone(Guid? schemaID = null);
}
