using Noxy.NET.CaseManagement.Persistence.Abstractions.Tables;
using Noxy.NET.CaseManagement.Persistence.Tables.Schemas.Associations;

namespace Noxy.NET.CaseManagement.Persistence.Tables.Schemas.Discriminators;

public abstract class TableSchemaDynamicValue : BaseTableSchema
{
    public ICollection<TableAssociationSchemaActionInputHasAttributeDynamicValue>? RelationActionInputAttributeList { get; set; }
}
