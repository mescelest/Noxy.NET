using System.Text.Json.Serialization;
using Noxy.NET.EntityManagement.Domain.Abstractions.Entities;

namespace Noxy.NET.EntityManagement.Domain.Entities.Schemas.Discriminators;

public class EntitySchemaParameter : BaseEntitySchema
{
    public required bool IsSystemDefined { get; set; }
    public required bool IsApprovalRequired { get; set; }

    public class Discriminator : BaseEntity
    {
        [JsonConstructor]
        public Discriminator()
        {
        }

        public Discriminator(EntitySchemaParameter entity)
        {
            switch (entity)
            {
                case EntitySchemaParameterStyle value:
                    StyleParameter = value;
                    break;
                case EntitySchemaParameterSystem value:
                    SystemParameter = value;
                    break;
                case EntitySchemaParameterText value:
                    TextParameter = value;
                    break;
                default:
                    throw new InvalidOperationException();
            }

            ID = entity.ID;
            SchemaID = entity.ID;
            SchemaIdentifier = entity.SchemaIdentifier;
        }

        public Guid SchemaID { get; init; }
        public string SchemaIdentifier { get; init; } = string.Empty;

        public EntitySchemaParameterStyle? StyleParameter { get; init; }
        public EntitySchemaParameterSystem? SystemParameter { get; init; }
        public EntitySchemaParameterText? TextParameter { get; init; }

        public EntitySchemaParameter GetValue()
        {
            if (StyleParameter != null) return StyleParameter;
            if (SystemParameter != null) return SystemParameter;
            if (TextParameter != null) return TextParameter;
            throw new();
        }
    }
}
