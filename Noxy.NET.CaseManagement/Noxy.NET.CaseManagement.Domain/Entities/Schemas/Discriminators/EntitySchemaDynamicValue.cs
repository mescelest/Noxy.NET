using System.Text.Json.Serialization;
using Noxy.NET.CaseManagement.Domain.Abstractions.Entities;
using Noxy.NET.CaseManagement.Domain.ViewModels;

namespace Noxy.NET.CaseManagement.Domain.Entities.Schemas.Discriminators;

public class EntitySchemaDynamicValue : BaseEntitySchema
{
    public class Discriminator : BaseEntity
    {
        public Guid SchemaID { get; init; }
        public string SchemaIdentifier { get; init; } = string.Empty;

        public EntitySchemaDynamicValueCode? Code { get; init; }
        public EntitySchemaDynamicValueStyleParameter? StyleParameter { get; init; }
        public EntitySchemaDynamicValueSystemParameter? SystemParameter { get; init; }
        public EntitySchemaDynamicValueTextParameter? TextParameter { get; init; }

        [JsonConstructor]
        public Discriminator()
        {
        }

        public Discriminator(EntitySchemaDynamicValue? entity)
        {
            switch (entity)
            {
                case EntitySchemaDynamicValueCode value:
                    Code = value;
                    break;
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

        public EntitySchemaDynamicValue GetValue()
        {
            if (Code != null) return Code;
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
