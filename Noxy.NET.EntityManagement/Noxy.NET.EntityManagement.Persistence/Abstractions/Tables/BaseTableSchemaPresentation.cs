using System.ComponentModel.DataAnnotations;
using Noxy.NET.EntityManagement.Persistence.Tables.Schemas;

namespace Noxy.NET.EntityManagement.Persistence.Abstractions.Tables;

public abstract class BaseTableSchemaPresentation : BaseTableSchema
{
    [Required]
    public TableSchemaParameterText? TitleTextParameter { get; set; }
    public required Guid TitleTextParameterID { get; set; }

    public TableSchemaParameterText? DescriptionTextParameter { get; set; }
    public Guid? DescriptionTextParameterID { get; set; }
}
