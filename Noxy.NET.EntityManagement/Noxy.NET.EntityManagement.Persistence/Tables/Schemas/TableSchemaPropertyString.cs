using Noxy.NET.EntityManagement.Persistence.Tables.Schemas.Discriminators;

namespace Noxy.NET.EntityManagement.Persistence.Tables.Schemas;

public class TableSchemaPropertyString : TableSchemaProperty
{
    public override TableSchemaPropertyString Clone(Guid? schemaID = null)
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
