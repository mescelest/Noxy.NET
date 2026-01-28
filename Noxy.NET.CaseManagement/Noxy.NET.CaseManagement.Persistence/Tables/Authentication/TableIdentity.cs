using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Noxy.NET.CaseManagement.Persistence.Abstractions.Tables;

namespace Noxy.NET.CaseManagement.Persistence.Tables.Authentication;

[Table(nameof(TableIdentity))]
public class TableIdentity : BaseTable
{
    [Required]
    [Column(TypeName = "varchar(32)")]
    public required string Handle { get; set; }

    [Required]
    [Column(TypeName = "varchar(32)")]
    public required string Username { get; set; }
    
    [Required]
    public required int Order { get; set; }
    
    [Required]
    public DateTime TimeSignIn { get; set; } = DateTime.UtcNow;
    
    [Required]
    public TableUser? User { get; set; }
    public required Guid UserID { get; set; }
}
