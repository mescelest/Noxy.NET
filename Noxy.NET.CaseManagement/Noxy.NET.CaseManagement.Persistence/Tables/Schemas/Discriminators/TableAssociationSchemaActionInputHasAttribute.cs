using Noxy.NET.CaseManagement.Persistence.Abstractions.Tables;

namespace Noxy.NET.CaseManagement.Persistence.Tables.Schemas.Discriminators;

public abstract class TableAssociationSchemaActionInputHasAttribute : BaseTableManyToMany<TableSchemaActionInput, TableSchemaAttribute>;