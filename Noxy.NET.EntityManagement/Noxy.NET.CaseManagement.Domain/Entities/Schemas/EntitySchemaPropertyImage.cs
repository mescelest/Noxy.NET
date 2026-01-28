using Noxy.NET.CaseManagement.Domain.Entities.Schemas.Discriminators;

namespace Noxy.NET.CaseManagement.Domain.Entities.Schemas;

public class EntitySchemaPropertyImage : EntitySchemaProperty.Primitive
{
    public required string AllowedExtensions { get; set; }

    public EntitySchemaDynamicValueCode? WidthDynamic { get; set; }
    public required Guid? WidthDynamicID { get; set; }

    public EntitySchemaDynamicValueCode? HeightDynamic { get; set; }
    public required Guid? HeightDynamicID { get; set; }
}