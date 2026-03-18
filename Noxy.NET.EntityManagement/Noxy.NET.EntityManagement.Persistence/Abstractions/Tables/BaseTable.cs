using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Noxy.NET.EntityManagement.Domain.Abstractions.Entities;
using Noxy.NET.EntityManagement.Persistence.Tables.Schemas;

namespace Noxy.NET.EntityManagement.Persistence.Abstractions.Tables;

[PrimaryKey(nameof(ID))]
[Index(nameof(TimeCreated))]
public abstract class BaseTable
{
    public Guid ID { get; init; } = BaseEntity.CreateID();

    [Required]
    public DateTime TimeCreated { get; set; } = DateTime.UtcNow;

    public DateTime? TimeUpdated { get; set; }

    [Owned]
    public class FeatureDescription(string name, string note)
    {
        [Required]
        [MaxLength(128)]
        public string Name { get; set; } = name;

        [Required]
        [MaxLength(1024)]
        public string Note { get; set; } = note;
    }

    [Owned]
    public class FeaturePresentation(Guid title, Guid? description = null)
    {
        [Required]
        public TableSchemaParameterText? TitleTextParameter { get; set; }
        public Guid TitleTextParameterID { get; set; } = title;

        public TableSchemaParameterText? DescriptionTextParameter { get; set; }
        public Guid? DescriptionTextParameterID { get; set; } = description;
    }

    [Owned]
    public class FeatureOrdering(int value)
    {
        [Required]
        public int Value { get; set; } = value;
    }
}
