using System.ComponentModel;
using Noxy.NET.EntityManagement.Domain.Constants;
using Noxy.NET.EntityManagement.Domain.Entities.Schemas;

namespace Noxy.NET.EntityManagement.Domain.Abstractions.Entities;

public abstract class BaseEntity
{
    public Guid ID { get; init; } = CreateID();
    public DateTime? TimeCreated { get; set; } = DateTime.UtcNow;

    public override string ToString()
    {
        return ID.ToString();
    }

    public static Guid CreateID()
    {
        return Guid.CreateVersion7();
    }

    public class FeatureDescription(string name, string note)
    {
        [DisplayName(TextConstants.LabelFormName)]
        [Description(TextConstants.HelpFormName)]
        public string Name { get; set; } = name;

        [DisplayName(TextConstants.LabelFormNote)]
        [Description(TextConstants.HelpFormNote)]
        public string Note { get; set; } = note;
    }

    public class FeaturePresentation(Guid title, Guid? description = null)
    {
        [DisplayName(TextConstants.LabelFormTitle)]
        [Description(TextConstants.HelpFormTitle)]
        public EntitySchemaParameterText? TitleTextParameter { get; set; }
        public Guid TitleTextParameterID { get; set; } = title;

        [DisplayName(TextConstants.LabelFormDescription)]
        [Description(TextConstants.HelpFormDescription)]
        public EntitySchemaParameterText? DescriptionTextParameter { get; set; }
        public Guid? DescriptionTextParameterID { get; set; } = description;
    }

    public class FeatureOrdering(int value)
    {
        [DisplayName(TextConstants.LabelFormOrder)]
        [Description(TextConstants.HelpFormOrder)]
        public int Value { get; set; } = value;

        public bool IsDefault => Value == DefaultOrder;

        public const int DefaultOrder = -1;
    }
}
