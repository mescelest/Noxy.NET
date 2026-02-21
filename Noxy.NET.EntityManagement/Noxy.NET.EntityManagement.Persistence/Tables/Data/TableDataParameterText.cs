using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Noxy.NET.EntityManagement.Persistence.Tables.Data.Discriminators;

namespace Noxy.NET.EntityManagement.Persistence.Tables.Data;

public class TableDataParameterText : TableDataParameter
{
    [Required]
    [Column(TypeName = "varchar(5)")]
    public required string Culture { get; set; }
}
