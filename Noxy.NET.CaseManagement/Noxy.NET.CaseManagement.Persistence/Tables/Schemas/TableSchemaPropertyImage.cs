using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Noxy.NET.CaseManagement.Persistence.Tables.Schemas.Discriminators;

namespace Noxy.NET.CaseManagement.Persistence.Tables.Schemas;

[Table(nameof(TableSchemaPropertyImage))]
[Index(nameof(SchemaID), nameof(SchemaIdentifier), IsUnique = true)]
public class TableSchemaPropertyImage : TableSchemaProperty.Primitive
{    
    [Required]
    public required string AllowedExtensions { get; set; }
    
    public TableSchemaDynamicValueCode? WidthDynamic { get; set; }
    public required Guid? WidthDynamicID { get; set; }

    public TableSchemaDynamicValueCode? HeightDynamic { get; set; }
    public required Guid? HeightDynamicID { get; set; }
}