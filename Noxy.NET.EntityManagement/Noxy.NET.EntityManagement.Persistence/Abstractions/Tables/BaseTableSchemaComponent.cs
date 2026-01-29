using Noxy.NET.EntityManagement.Persistence.Tables.Schemas;

namespace Noxy.NET.EntityManagement.Persistence.Abstractions.Tables;

public abstract class BaseTableSchemaComponent : BaseTableSchema
{
    public TableSchemaParameterText? TitleTextParameter { get; set; }
    public required Guid TitleTextParameterID { get; set; }

    public TableSchemaParameterText? DescriptionTextParameter { get; set; }
    public Guid? DescriptionTextParameterID { get; set; }
}
