using Noxy.NET.EntityManagement.Domain.Abstractions.Entities;
using Noxy.NET.EntityManagement.Domain.Entities.Schemas.Junctions;

namespace Noxy.NET.EntityManagement.Domain.Entities.Schemas;

public class EntitySchemaContext : BaseEntitySchema
{
    public required FeatureDescription Description { get; set; }
    public required FeaturePresentation Presentation { get; set; }
    public required FeatureOrdering Ordering { get; set; }

    public List<EntityJunctionSchemaContextHasElement>? ElementList { get; set; }

    public string Name
    {
        get => Description.Name;
        set => Description.Name = value;
    }

    public string Note
    {
        get => Description.Name;
        set => Description.Name = value;
    }

    public EntitySchemaParameterText? TitleTextParameter
    {
        get => Presentation.TitleTextParameter;
        set => Presentation.TitleTextParameter = value;
    }

    public Guid TitleTextParameterID
    {
        get => Presentation.TitleTextParameterID;
        set => Presentation.TitleTextParameterID = value;
    }

    public EntitySchemaParameterText? DescriptionTextParameter
    {
        get => Presentation.DescriptionTextParameter;
        set => Presentation.DescriptionTextParameter = value;
    }

    public Guid? DescriptionTextParameterID
    {
        get => Presentation.DescriptionTextParameterID;
        set => Presentation.DescriptionTextParameterID = value;
    }

    public int Order
    {
        get => Ordering.Value;
        set => Ordering.Value = value;
    }
}
