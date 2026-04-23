using Noxy.NET.EntityManagement.Persistence.Tables.Schemas.Discriminators;
using Noxy.NET.EntityManagement.Persistence.Tables.Schemas.Junctions;

namespace Noxy.NET.EntityManagement.Persistence.Tables.Schemas;

public class TableSchemaPropertyTable : TableSchemaProperty
{
    public ICollection<TableJunctionSchemaPropertyTableHasProperty>? PropertyList { get; set; }

    public override TableSchemaPropertyTable Clone(Guid? schemaID = null)
    {
        return new()
        {
            SchemaID = schemaID ?? SchemaID,
            SchemaIdentifier = SchemaIdentifier,
            Name = Name,
            Note = Note,
            Order = Order,
            DescriptionTextParameterID = DescriptionTextParameterID,
            TitleTextParameterID = TitleTextParameterID,
        };
    }
}
