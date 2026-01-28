using System.Text.Json.Serialization;
using Noxy.NET.EntityManagement.Domain.Abstractions.Entities;

namespace Noxy.NET.EntityManagement.Domain.Entities.Data.Discriminators;

public abstract class EntityDataProperty : BaseEntityData
{
    public class Discriminator
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
        }

        public Guid ID { get; init; }
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
