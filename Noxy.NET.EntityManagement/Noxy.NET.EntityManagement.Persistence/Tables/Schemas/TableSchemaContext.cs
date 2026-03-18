using System.ComponentModel.DataAnnotations.Schema;
using Noxy.NET.EntityManagement.Persistence.Abstractions.Tables;
using Noxy.NET.EntityManagement.Persistence.Tables.Schemas.Junctions;

namespace Noxy.NET.EntityManagement.Persistence.Tables.Schemas;

[Table(nameof(TableSchemaContext))]
public class TableSchemaContext : BaseTableSchema
{
    public required FeatureDescription Description { get; set; }
    public required FeaturePresentation Presentation { get; set; }
    public required FeatureOrdering Ordering { get; set; }

    public ICollection<TableJunctionSchemaContextHasElement>? ElementList { get; set; }

    [NotMapped]
    public string Name
    {
        get => Description.Name;
        set => Description.Name = value;
    }

    [NotMapped]
    public string Note
    {
        get => Description.Name;
        set => Description.Name = value;
    }

    [NotMapped]
    public TableSchemaParameterText? TitleTextParameter
    {
        get => Presentation.TitleTextParameter;
        set => Presentation.TitleTextParameter = value;
    }

    [NotMapped]
    public Guid TitleTextParameterID
    {
        get => Presentation.TitleTextParameterID;
        set => Presentation.TitleTextParameterID = value;
    }

    [NotMapped]
    public TableSchemaParameterText? DescriptionTextParameter
    {
        get => Presentation.DescriptionTextParameter;
        set => Presentation.DescriptionTextParameter = value;
    }

    [NotMapped]
    public Guid? DescriptionTextParameterID
    {
        get => Presentation.DescriptionTextParameterID;
        set => Presentation.DescriptionTextParameterID = value;
    }

    [NotMapped]
    public int Order
    {
        get => Ordering.Value;
        set => Ordering.Value = value;
    }
}
