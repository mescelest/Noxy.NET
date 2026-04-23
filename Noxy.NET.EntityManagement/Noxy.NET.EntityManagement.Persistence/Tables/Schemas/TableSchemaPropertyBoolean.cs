using Noxy.NET.EntityManagement.Persistence.Tables.Schemas.Discriminators;

namespace Noxy.NET.EntityManagement.Persistence.Tables.Schemas;

public class TableSchemaPropertyBoolean : TableSchemaProperty
{
    public override TableSchemaPropertyBoolean Clone(Guid? schemaID = null)
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
