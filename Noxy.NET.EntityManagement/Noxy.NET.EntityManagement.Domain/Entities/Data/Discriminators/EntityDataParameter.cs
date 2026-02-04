using System.Text.Json.Serialization;
using Noxy.NET.EntityManagement.Domain.Abstractions.Entities;

namespace Noxy.NET.EntityManagement.Domain.Entities.Data.Discriminators;

public abstract class EntityDataParameter : BaseEntityData
{
    public required string Value { get; set; }
    public required DateTime? TimeApproved { get; set; }
    public required DateTime TimeEffective { get; set; }

    public class Discriminator
    {
        [JsonConstructor]
        public Discriminator()
        {
        }

        public Discriminator(EntityDataParameter? entity)
        {
            switch (entity)
            {
                case EntityDataParameterSystem system:
                    System = system;
                    break;
                case EntityDataParameterStyle style:
                    Style = style;
                    break;
                case EntityDataParameterText text:
                    Text = text;
                    break;
                default:
                    throw new InvalidOperationException();
            }
        }

        public Guid ID { get; init; }
        public string SchemaIdentifier { get; init; } = string.Empty;

        public EntityDataParameterStyle? Style { get; init; }
        public EntityDataParameterSystem? System { get; init; }
        public EntityDataParameterText? Text { get; init; }

        public EntityDataParameter GetValue()
        {
            if (System != null) return System;
            if (Text != null) return Text;
            if (Style != null) return Style;
            throw new();
        }
    }
}
