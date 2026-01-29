using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Noxy.NET.EntityManagement.Persistence.Tables.Schemas.Discriminators;

namespace Noxy.NET.EntityManagement.Persistence.Tables.Schemas;

[Table(nameof(TableSchemaParameterSystem))]
[Index(nameof(SchemaID), nameof(SchemaIdentifier), IsUnique = true)]
public class TableSchemaParameterSystem : TableSchemaParameter;
