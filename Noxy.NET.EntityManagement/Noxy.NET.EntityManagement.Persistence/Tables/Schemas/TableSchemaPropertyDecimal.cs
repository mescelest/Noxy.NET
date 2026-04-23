using Noxy.NET.EntityManagement.Persistence.Tables.Schemas.Discriminators;

namespace Noxy.NET.EntityManagement.Persistence.Tables.Schemas;

public class TableSchemaPropertyDecimal : TableSchemaProperty
{
    public override TableSchemaPropertyCollection Clone(Guid? schemaID = null)
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
