using Noxy.NET.CaseManagement.Persistence.Abstractions.Tables;
using Noxy.NET.CaseManagement.Persistence.Tables.Schemas.Junctions;

namespace Noxy.NET.CaseManagement.Persistence.Tables.Schemas.Discriminators;

public abstract class TableSchemaProperty : BaseTableSchemaComponent
{
    public ICollection<TableJunctionSchemaElementHasProperty>? RelationElementList { get; set; }
    public ICollection<TableJunctionSchemaPropertyCollectionHasProperty>? RelationPropertyCollectionList { get; set; }
    public ICollection<TableJunctionSchemaPropertyTableHasProperty>? RelationPropertyTableList { get; set; }

    public abstract class Primitive : TableSchemaProperty
    {
        public TableSchemaDynamicValueCode? DefaultValueDynamic { get; set; }
        public required Guid? DefaultValueDynamicID { get; set; }
    }
}
