using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Noxy.NET.CaseManagement.Persistence.Abstractions.Tables;

namespace Noxy.NET.CaseManagement.Persistence.Tables.Authentication;

[Index(nameof(Email), IsUnique = true)]
[Table(nameof(TableUser))]
public class TableUser : BaseTable
{
    [Required]
    [Column(TypeName = "varchar(256)")]
    public required string Email { get; set; }

    public DateTime TimeSignIn { get; set; } = DateTime.UtcNow;

    public DateTime? TimeVerified { get; set; }

    [Required]
    public required Guid AuthenticationID { get; set; }
    public TableAuthentication? Authentication { get; set; }

    public Collection<TableIdentity>? IdentityList { get; set; }
}
