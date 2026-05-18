using Noxy.NET.EntityManagement.Persistence.Tables.Schemas.Discriminators;
using Noxy.NET.EntityManagement.Persistence.Tables.Schemas.Junctions;

namespace Noxy.NET.EntityManagement.Persistence.Tables.Schemas;

public class TableSchemaPropertyTable : TableSchemaProperty
{
    public ICollection<TableSchemaPropertyTableHasProperty>? PropertyList { get; set; }

    public override TableSchemaPropertyTable Clone(Guid? schemaID = null)
    {
        return new()
        {
            SchemaID = schemaID ?? SchemaID,
            SchemaIdentifier = SchemaIdentifier,
            Name = Name,
            Note = Note,
            Weight = Weight,
            TitleParameterTextID = TitleParameterTextID,
            DescriptionParameterTextID = DescriptionParameterTextID,
        };
    }
}
