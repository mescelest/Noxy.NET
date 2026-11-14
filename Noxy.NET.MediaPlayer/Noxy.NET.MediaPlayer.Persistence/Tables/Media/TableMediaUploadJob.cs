using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Noxy.NET.MediaPlayer.Domain.Enums;
using Noxy.NET.MediaPlayer.Persistence.Abstractions.Tables;
using Noxy.NET.MediaPlayer.Persistence.Tables.Authentication;

namespace Noxy.NET.MediaPlayer.Persistence.Tables.Media;

[Table("MediaUploadJob")]
[Index(nameof(UserID))]
[Index(nameof(Status), nameof(TimeCreated))]
public class TableMediaUploadJob : BaseTable
{
    [Required]
    public Guid UserID { get; set; }

    [ForeignKey(nameof(UserID))]
    public TableUser? User { get; set; }

    [Required]
    [MaxLength(512)]
    public required string StagedFileName { get; set; }

    [Required]
    [MaxLength(255)]
    public required string Title { get; set; }

    [Required]
    [MaxLength(127)]
    public required string ContentType { get; set; }

    [Required]
    public required long SizeBytes { get; set; }

    [Required]
    [MaxLength(64)]
    public required string ContentHash { get; set; }

    [Required]
    public MediaUploadJobStatus Status { get; set; }
}
