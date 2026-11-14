using System.ComponentModel.DataAnnotations;
using Noxy.NET.EntityManagement.Persistence.Tables.Schemas.Discriminators;

namespace Noxy.NET.EntityManagement.Persistence.Tables.Schemas;

public class TableSchemaPropertyImage : TableSchemaProperty
{
    // A comma separated list, so this is far above any realistic length. It exists to give the column a bound.
    public const int AllowedExtensionsLength = 1024;

    [Required]
    [MaxLength(AllowedExtensionsLength)]
    public required string AllowedExtensions { get; set; }

    public override TableSchemaPropertyImage Clone(Guid? schemaID = null)
    {
        return new()
        {
            SchemaID = schemaID ?? SchemaID,
            SchemaIdentifier = SchemaIdentifier,
            Name = Name,
            Note = Note,
            Weight = Weight,
            AllowedExtensions = AllowedExtensions,
            TitleParameterTextID = TitleParameterTextID,
            DescriptionParameterTextID = DescriptionParameterTextID,
        };
    }
}
