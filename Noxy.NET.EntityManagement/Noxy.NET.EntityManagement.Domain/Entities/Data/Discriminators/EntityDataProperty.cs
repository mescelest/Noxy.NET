using System.Text.Json.Serialization;
using Noxy.NET.EntityManagement.Domain.Abstractions.Entities;

namespace Noxy.NET.EntityManagement.Domain.Entities.Data.Discriminators;

public abstract class EntityDataProperty : BaseEntityData
{
    public EntityDataElement? Element { get; set; }
    public required Guid ElementID { get; set; }

    public class Discriminator : BaseEntity
    {
        [JsonConstructor]
        public Discriminator()
        {
        }

        public Discriminator(EntityDataProperty? entity)
        {
            switch (entity)
            {
                case EntityDataPropertyBoolean propertyBoolean:
                    Boolean = propertyBoolean;
                    break;
                case EntityDataPropertyString propertyString:
                    String = propertyString;
                    break;
                case EntityDataPropertyDateTime propertyDateTime:
                    DateTime = propertyDateTime;
                    break;
                default:
                    throw new InvalidOperationException();
            }

            ID = entity.ID;
            SchemaIdentifier = entity.SchemaIdentifier;
        }

        public string SchemaIdentifier { get; init; } = string.Empty;

        public EntityDataPropertyBoolean? Boolean { get; init; }
        public EntityDataPropertyDateTime? DateTime { get; init; }
        public EntityDataPropertyString? String { get; init; }

        public EntityDataProperty GetValue()
        {
            if (Boolean != null) return Boolean;
            if (DateTime != null) return DateTime;
            if (String != null) return String;
            throw new();
        }
    }
}
