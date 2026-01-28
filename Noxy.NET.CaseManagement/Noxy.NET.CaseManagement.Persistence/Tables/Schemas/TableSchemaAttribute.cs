using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Noxy.NET.CaseManagement.Domain.Enums;
using Noxy.NET.CaseManagement.Persistence.Abstractions.Tables;
using Noxy.NET.CaseManagement.Persistence.Tables.Schemas.Discriminators;
using Noxy.NET.CaseManagement.Persistence.Tables.Schemas.Junctions;

namespace Noxy.NET.CaseManagement.Persistence.Tables.Schemas;

[Table(nameof(TableSchemaAttribute))]
[Index(nameof(SchemaID), nameof(SchemaIdentifier), IsUnique = true)]
public class TableSchemaAttribute : BaseTableSchema
{
    [Required]
    public required AttributeTypeEnum Type { get; set; }
    
    [Required]
    public required bool IsList { get; set; }
    
    public ICollection<TableAssociationSchemaActionInputHasAttribute>? RelationActionInputList { get; set; }
    public ICollection<TableJunctionSchemaInputHasAttribute>? RelationInputList { get; set; }
}
