using System.ComponentModel;
using System.Text.Json.Serialization;
using Noxy.NET.EntityManagement.Domain.Abstractions.Entities;
using Noxy.NET.EntityManagement.Domain.Constants;
using Noxy.NET.EntityManagement.Domain.Interfaces;

namespace Noxy.NET.EntityManagement.Domain.Entities.Schemas.Discriminators;

public class EntitySchemaParameter : BaseEntitySchemaPresentation, ISchemaMetadata
{
    [DisplayName(TextConstants.LabelFormName)]
    [Description(TextConstants.HelpFormName)]
    public required string Name { get; set; }

    [DisplayName(TextConstants.LabelFormNote)]
    [Description(TextConstants.HelpFormNote)]
    public string Note { get; set; } = string.Empty;

    public required bool IsSystemDefined { get; set; }
    public required bool IsApprovalRequired { get; set; }

    public class Discriminator : BaseEntity
    {
        [JsonConstructor]
        public Discriminator()
        {
        }

        public Discriminator(EntitySchemaParameter? entity)
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
