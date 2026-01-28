using System.ComponentModel.DataAnnotations.Schema;
using Noxy.NET.EntityManagement.Persistence.Abstractions.Tables;

namespace Noxy.NET.EntityManagement.Persistence.Tables.Data;

[Table(nameof(TableDataSystemParameter))]
public class TableDataSystemParameter : BaseTableDataParameter;
