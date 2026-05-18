using System.ComponentModel.DataAnnotations;
using Noxy.NET.EntityManagement.Domain.Interfaces;
using Noxy.NET.EntityManagement.Persistence.Tables.Schemas;

namespace Noxy.NET.EntityManagement.Persistence.Abstractions.Tables;

public abstract class BaseTableSchemaPresentation : BaseTableSchema, ISchemaPresentation
{
    [Required]
    public TableSchemaParameterText? TitleParameterText { get; set; }
    public required Guid TitleParameterTextID { get; set; }

    public TableSchemaParameterText? DescriptionParameterText { get; set; }
    public Guid? DescriptionParameterTextID { get; set; }
}
