using System.Text.Json.Serialization;
using Noxy.NET.EntityManagement.Domain.Abstractions.Entities;
using Noxy.NET.EntityManagement.Domain.Entities.Schemas.Junctions;
using Noxy.NET.EntityManagement.Domain.Interfaces;

namespace Noxy.NET.EntityManagement.Domain.Entities.Schemas.Discriminators;

public abstract class EntitySchemaProperty : BaseEntitySchemaComponent
{
    [JsonIgnore]
    public ICollection<EntityJunctionSchemaElementHasProperty>? RelationElementList { get; set; }

    [JsonIgnore]
    public ICollection<EntityJunctionSchemaPropertyCollectionHasProperty>? RelationPropertyCollectionList { get; set; }

    [JsonIgnore]
    public ICollection<EntityJunctionSchemaPropertyTableHasProperty>? RelationPropertyTableList { get; set; }

    public abstract class Primitive : EntitySchemaProperty;

    public class Discriminator : BaseEntity, IOrderedEntity
    {
        [JsonConstructor]
        public Discriminator()
        {
        }

        public Discriminator(EntitySchemaProperty? entity)
        {
            switch (entity)
            {
                case EntitySchemaPropertyBoolean property:
                    Boolean = property;
                    break;
                case EntitySchemaPropertyCollection property:
                    Collection = property;
                    break;
                case EntitySchemaPropertyDateTime property:
                    DateTime = property;
                    break;
                case EntitySchemaPropertyDecimal property:
                    Decimal = property;
                    break;
                case EntitySchemaPropertyImage property:
                    Image = property;
                    break;
                case EntitySchemaPropertyInteger property:
                    Integer = property;
                    break;
                case EntitySchemaPropertyString property:
                    String = property;
                    break;
                case EntitySchemaPropertyTable property:
                    Table = property;
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

        public EntitySchemaPropertyBoolean? Boolean { get; init; }
        public EntitySchemaPropertyCollection? Collection { get; init; }
        public EntitySchemaPropertyDateTime? DateTime { get; init; }
        public EntitySchemaPropertyDecimal? Decimal { get; init; }
        public EntitySchemaPropertyImage? Image { get; init; }
        public EntitySchemaPropertyInteger? Integer { get; init; }
        public EntitySchemaPropertyString? String { get; init; }
        public EntitySchemaPropertyTable? Table { get; init; }

        public int Order
        {
            get => GetValue().Order;
            set => GetValue().Order = value;
        }

        public EntitySchemaProperty GetValue()
        {
            if (Boolean != null) return Boolean;
            if (Collection != null) return Collection;
            if (DateTime != null) return DateTime;
            if (Decimal != null) return Decimal;
            if (Image != null) return Image;
            if (Integer != null) return Integer;
            if (String != null) return String;
            if (Table != null) return Table;
            throw new();
        }
    }
}
