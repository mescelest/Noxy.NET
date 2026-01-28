using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Noxy.NET.CaseManagement.Domain.Enums;
using Noxy.NET.CaseManagement.Persistence.Tables.Schemas.Discriminators;

namespace Noxy.NET.CaseManagement.Persistence.Tables.Schemas;

[Table(nameof(TableSchemaPropertyDateTime))]
[Index(nameof(SchemaID), nameof(SchemaIdentifier), IsUnique = true)]
public class TableSchemaPropertyDateTime : TableSchemaProperty.Primitive
{
    [Required]
    public required DateTimeTypeEnum Type { get; set; }
    
    public TableSchemaDynamicValueCode? MinDynamic { get; set; }
    public required Guid? MinDynamicID { get; set; }

    public TableSchemaDynamicValueCode? MaxDynamic { get; set; }
    public required Guid? MaxDynamicID { get; set; }
}
