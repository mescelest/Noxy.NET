using System.ComponentModel.DataAnnotations.Schema;
using Noxy.NET.CaseManagement.Persistence.Abstractions.Tables;

namespace Noxy.NET.CaseManagement.Persistence.Tables.Data;

[Table(nameof(TableDataStyleParameter))]
public class TableDataStyleParameter : BaseTableDataParameter;
