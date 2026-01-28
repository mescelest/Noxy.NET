using System.Text.Json.Serialization;
using Noxy.NET.CaseManagement.Domain.Abstractions.Entities;
using Noxy.NET.CaseManagement.Domain.Entities.Schemas.Associations;

namespace Noxy.NET.CaseManagement.Domain.Entities.Schemas.Discriminators;

public class EntityAssociationSchemaActionInputHasAttribute : BaseEntityManyToMany<EntitySchemaActionInput, EntitySchemaAttribute>
{
    public class Discriminator : BaseEntity
    {
        public Guid EntityID { get; init; }
        public Guid RelationID { get; init; }

        public EntityAssociationSchemaActionInputHasAttributeDynamicValue? DynamicValue { get; init; }
        public EntityAssociationSchemaActionInputHasAttributeInteger? Integer { get; init; }
        public EntityAssociationSchemaActionInputHasAttributeString? String { get; init; }

        [JsonConstructor]
        public Discriminator()
        {
        }

        public Discriminator(EntityAssociationSchemaActionInputHasAttribute? entity)
        {
            switch (entity)
            {
                case EntityAssociationSchemaActionInputHasAttributeDynamicValue propertyDynamicValue:
                    DynamicValue = propertyDynamicValue;
                    break;
                case EntityAssociationSchemaActionInputHasAttributeInteger propertyInteger:
                    Integer = propertyInteger;
                    break;
                case EntityAssociationSchemaActionInputHasAttributeString propertyString:
                    String = propertyString;
                    break;
                default:
                    throw new InvalidOperationException();
            }

            ID = entity.ID;
            EntityID = entity.EntityID;
            RelationID = entity.RelationID;
        }

        public EntityAssociationSchemaActionInputHasAttribute GetValue()
        {
            if (DynamicValue != null) return DynamicValue;
            if (Integer != null) return Integer;
            if (String != null) return String;
            throw new();
        }
    }
}
