using System.ComponentModel.DataAnnotations;
using Noxy.NET.EntityManagement.Domain.Enums;
using Noxy.NET.EntityManagement.Persistence.Tables.Schemas.Discriminators;

namespace Noxy.NET.EntityManagement.Persistence.Tables.Schemas;

public class TableSchemaPropertyDateTime : TableSchemaProperty
{
    [Required]
    public required DateTimeTypeEnum Type { get; set; }

    public override TableSchemaPropertyDateTime Clone(Guid? schemaID = null)
    {
        return new()
        {
            SchemaID = schemaID ?? SchemaID,
            SchemaIdentifier = SchemaIdentifier,
            Name = Name,
            Note = Note,
            Weight = Weight,
            Type = Type,
            TitleParameterTextID = TitleParameterTextID,
            DescriptionParameterTextID = DescriptionParameterTextID,
        };
    }
}
