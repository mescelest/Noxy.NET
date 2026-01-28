using System.ComponentModel.DataAnnotations;
using Noxy.NET.CaseManagement.Persistence.Tables.Schemas.Discriminators;

namespace Noxy.NET.CaseManagement.Persistence.Abstractions.Tables;

public abstract class BaseTableSchemaComponent : BaseTableSchema
{
    [Required]
    public TableSchemaDynamicValue? TitleDynamic { get; set; }
    public required Guid TitleDynamicID { get; set; }

    public TableSchemaDynamicValue? DescriptionDynamic { get; set; }
    public required Guid? DescriptionDynamicID { get; set; }
}
