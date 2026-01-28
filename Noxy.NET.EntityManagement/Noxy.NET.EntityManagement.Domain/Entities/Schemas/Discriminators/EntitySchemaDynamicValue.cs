using System.Text.Json.Serialization;
using Noxy.NET.EntityManagement.Domain.Abstractions.Entities;
using Noxy.NET.EntityManagement.Domain.ViewModels;

namespace Noxy.NET.EntityManagement.Domain.Entities.Schemas.Discriminators;

public class EntitySchemaDynamicValue : BaseEntitySchema
{
    public class Discriminator : BaseEntity
    {
        [JsonConstructor]
        public Discriminator()
        {
        }

        public Discriminator(EntitySchemaDynamicValue? entity)
        {
            switch (entity)
            {
                case EntitySchemaDynamicValueStyleParameter value:
                    StyleParameter = value;
                    break;
                case EntitySchemaDynamicValueSystemParameter value:
                    SystemParameter = value;
                    break;
                case EntitySchemaDynamicValueTextParameter value:
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

        public EntitySchemaDynamicValueStyleParameter? StyleParameter { get; init; }
        public EntitySchemaDynamicValueSystemParameter? SystemParameter { get; init; }
        public EntitySchemaDynamicValueTextParameter? TextParameter { get; init; }

        public EntitySchemaDynamicValue GetValue()
        {
            if (StyleParameter != null) return StyleParameter;
            if (SystemParameter != null) return SystemParameter;
            if (TextParameter != null) return TextParameter;
            throw new();
        }

        public ViewModelSchemaDynamicValue ToViewModel(object value)
        {
            return new()
            {
                ID = ID,
                SchemaIdentifier = SchemaIdentifier,
                Value = new(value)
            };
        }
    }
}
