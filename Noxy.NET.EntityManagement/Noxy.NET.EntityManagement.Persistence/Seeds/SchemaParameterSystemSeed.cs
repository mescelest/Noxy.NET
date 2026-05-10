using Microsoft.EntityFrameworkCore;
using Noxy.NET.EntityManagement.Domain.Constants;
using Noxy.NET.EntityManagement.Domain.Enums;
using Noxy.NET.EntityManagement.Persistence.Abstractions;
using Noxy.NET.EntityManagement.Persistence.Tables.Data;
using Noxy.NET.EntityManagement.Persistence.Tables.Schemas;
using Noxy.NET.Extensions;

namespace Noxy.NET.EntityManagement.Persistence.Seeds;

public class SchemaParameterSystemSeed(ModelBuilder builder, TableSchema refSchema) : BaseSeed(builder, refSchema)
{
    public void Apply()
    {
        Register(ParameterSystemConstants.SchemaDeactivatedAddContext, false.ToString(), ParameterSystemTypeEnum.Boolean, "Is it possible to add new contexts to a previously activated schema?", true);
    }

    protected void Register(string constant, string value, ParameterSystemTypeEnum type = ParameterSystemTypeEnum.String, string note = "", bool isApprovalRequired = false)
    {
        TableSchemaParameterSystem entitySchema = new()
        {
            ID = constant.ToDeterministicGuid(),
            SchemaIdentifier = constant,
            Name = constant,
            Note = note,
            Type = type,
            IsSystemDefined = true,
            IsApprovalRequired = isApprovalRequired,
            TimeCreated = Now,
            SchemaID = Schema.ID
        };
        Builder.Entity<TableSchemaParameterSystem>().HasData(entitySchema);

        TableDataParameterSystem entityValue = new()
        {
            ID = $"{constant}:Value".ToDeterministicGuid(),
            SchemaIdentifier = constant,
            Value = value,
            TimeApproved = Now,
            TimeEffective = Now,
            TimeCreated = Now
        };
        Builder.Entity<TableDataParameterSystem>().HasData(entityValue);
    }
}
