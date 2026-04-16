using System.ComponentModel;
using System.Text.Json.Serialization;
using Noxy.NET.EntityManagement.Domain.Abstractions.Entities;
using Noxy.NET.EntityManagement.Domain.Constants;
using Noxy.NET.EntityManagement.Domain.Entities.Schemas.Junctions;
using Noxy.NET.EntityManagement.Domain.Interfaces;

namespace Noxy.NET.EntityManagement.Domain.Entities.Schemas.Discriminators;

public abstract class EntitySchemaProperty : BaseEntitySchema, ISchemaMetadata, ISchemaPresentation, ISchemaOrdering
{
    [DisplayName(TextConstants.LabelFormName)]
    [Description(TextConstants.HelpFormName)]
    public required string Name { get; set; }

    [DisplayName(TextConstants.LabelFormNote)]
    [Description(TextConstants.HelpFormNote)]
    public string Note { get; set; } = string.Empty;

    [DisplayName(TextConstants.LabelFormTitle)]
    [Description(TextConstants.HelpFormTitle)]
    public EntitySchemaParameterText? TitleTextParameter { get; set; }
    public Guid TitleTextParameterID { get; set; }

    [DisplayName(TextConstants.LabelFormDescription)]
    [Description(TextConstants.HelpFormDescription)]
    public EntitySchemaParameterText? DescriptionTextParameter { get; set; }
    public Guid? DescriptionTextParameterID { get; set; }

    [DisplayName(TextConstants.LabelFormOrder)]
    [Description(TextConstants.HelpFormOrder)]
    public int Order { get; set; } = DefaultOrder;

    [JsonIgnore]
    public ICollection<EntityJunctionSchemaElementHasProperty>? RelationElementList { get; set; }

    [JsonIgnore]
    public ICollection<EntityJunctionSchemaPropertyCollectionHasProperty>? RelationPropertyCollectionList { get; set; }

    [JsonIgnore]
    public ICollection<EntityJunctionSchemaPropertyTableHasProperty>? RelationPropertyTableList { get; set; }

    public class Discriminator : BaseEntity
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
