using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Noxy.NET.CaseManagement.Persistence.Tables.Schemas.Discriminators;

namespace Noxy.NET.CaseManagement.Persistence.Tables.Schemas;

[Table(nameof(TableSchemaPropertyString))]
[Index(nameof(SchemaID), nameof(SchemaIdentifier), IsUnique = true)]
public class TableSchemaPropertyString : TableSchemaProperty.Primitive
{
    public TableSchemaDynamicValueCode? MinDynamic { get; set; }
    public required Guid? MinDynamicID { get; set; }

    public TableSchemaDynamicValueCode? MaxDynamic { get; set; }
    public required Guid? MaxDynamicID { get; set; }
}
