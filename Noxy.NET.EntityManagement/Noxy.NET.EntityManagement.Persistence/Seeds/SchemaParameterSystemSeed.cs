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
        Register(ParameterSystemConstants.SchemaDeactivatedAddContext, bool.FalseString, ParameterSystemTypeEnum.Boolean, "Can you add new Context entities to a Schema that used to be active?", true);
        Register(ParameterSystemConstants.SchemaDeactivatedAddContextHasElement, bool.FalseString, ParameterSystemTypeEnum.Boolean, "Can you add new ContextHasElement entities to a Schema that used to be active?", true);
        Register(ParameterSystemConstants.SchemaDeactivatedAddElement, bool.FalseString, ParameterSystemTypeEnum.Boolean, "Can you add new Element entities to a Schema that used to be active?", true);
        Register(ParameterSystemConstants.SchemaDeactivatedAddElementHasProperty, bool.FalseString, ParameterSystemTypeEnum.Boolean, "Can you add new ElementHasProperty entities to a Schema that used to be active?", true);
        Register(ParameterSystemConstants.SchemaDeactivatedAddParameter, bool.FalseString, ParameterSystemTypeEnum.Boolean, "Can you add new Parameter entities to a Schema that used to be active?", true);
        Register(ParameterSystemConstants.SchemaDeactivatedAddProperty, bool.FalseString, ParameterSystemTypeEnum.Boolean, "Can you add new Property entities to a Schema that used to be active?", true);

        Register(ParameterSystemConstants.SchemaDeactivatedCloneContext, bool.FalseString, ParameterSystemTypeEnum.Boolean, "Can you clone a Context entity from a Schema that used to be active?", true);
        Register(ParameterSystemConstants.SchemaDeactivatedCloneElement, bool.FalseString, ParameterSystemTypeEnum.Boolean, "Can you clone an Element entity from a Schema that used to be active?", true);
        Register(ParameterSystemConstants.SchemaDeactivatedCloneParameter, bool.FalseString, ParameterSystemTypeEnum.Boolean, "Can you clone a Parameter entity from a Schema that used to be active?", true);
        Register(ParameterSystemConstants.SchemaDeactivatedCloneProperty, bool.FalseString, ParameterSystemTypeEnum.Boolean, "Can you clone a Property entity from a Schema that used to be active?", true);

        Register(ParameterSystemConstants.SchemaDeactivatedEditEntity, bool.FalseString, ParameterSystemTypeEnum.Boolean, "Can you edit a Schema entity that used to be active?", true);
        Register(ParameterSystemConstants.SchemaDeactivatedEditContext, bool.FalseString, ParameterSystemTypeEnum.Boolean, "Can you edit a Context entity from a Schema that used to be active?", true);
        Register(ParameterSystemConstants.SchemaDeactivatedEditElement, bool.FalseString, ParameterSystemTypeEnum.Boolean, "Can you edit an Element entity from a Schema that used to be active?", true);
        Register(ParameterSystemConstants.SchemaDeactivatedEditParameter, bool.FalseString, ParameterSystemTypeEnum.Boolean, "Can you edit a Parameter entity from a Schema that used to be active?", true);
        Register(ParameterSystemConstants.SchemaDeactivatedEditProperty, bool.FalseString, ParameterSystemTypeEnum.Boolean, "Can you edit a Property entity from a Schema that used to be active?", true);

        Register(ParameterSystemConstants.SchemaDeactivatedDeleteEntity, bool.FalseString, ParameterSystemTypeEnum.Boolean, "Can you remove a Schema entity that used to be active?", true);
        Register(ParameterSystemConstants.SchemaDeactivatedDeleteContext, bool.FalseString, ParameterSystemTypeEnum.Boolean, "Can you remove a Context entity from a Schema that used to be active?", true);
        Register(ParameterSystemConstants.SchemaDeactivatedDeleteElement, bool.FalseString, ParameterSystemTypeEnum.Boolean, "Can you remove an Element entity from a Schema that used to be active?", true);
        Register(ParameterSystemConstants.SchemaDeactivatedDeleteParameter, bool.FalseString, ParameterSystemTypeEnum.Boolean, "Can you remove a Parameter entity from a Schema that used to be active?", true);
        Register(ParameterSystemConstants.SchemaDeactivatedDeleteProperty, bool.FalseString, ParameterSystemTypeEnum.Boolean, "Can you remove a Property entity from a Schema that used to be active?", true);


        Register(ParameterSystemConstants.SchemaInactiveAddContext, bool.TrueString, ParameterSystemTypeEnum.Boolean, "Can you add new Context entities to a Schema that has not been activated yet?", true);
        Register(ParameterSystemConstants.SchemaInactiveAddContextHasElement, bool.TrueString, ParameterSystemTypeEnum.Boolean, "Can you add new ContextHasElement entities to a Schema that has not been activated yet?", true);
        Register(ParameterSystemConstants.SchemaInactiveAddElement, bool.TrueString, ParameterSystemTypeEnum.Boolean, "Can you add new Element entities to a Schema that has not been activated yet?", true);
        Register(ParameterSystemConstants.SchemaInactiveAddElementHasProperty, bool.TrueString, ParameterSystemTypeEnum.Boolean, "Can you add new ElementHasProperty entities to a Schema that has not been activated yet?", true);
        Register(ParameterSystemConstants.SchemaInactiveAddParameter, bool.TrueString, ParameterSystemTypeEnum.Boolean, "Can you add new Property entities to a Schema that has not been activated yet?", true);
        Register(ParameterSystemConstants.SchemaInactiveAddProperty, bool.TrueString, ParameterSystemTypeEnum.Boolean, "Can you add new Parameter entities to a Schema that has not been activated yet?", true);

        Register(ParameterSystemConstants.SchemaInactiveCloneContext, bool.TrueString, ParameterSystemTypeEnum.Boolean, "Can you clone a Context entity from a Schema that has not been activated yet?", true);
        Register(ParameterSystemConstants.SchemaInactiveCloneElement, bool.TrueString, ParameterSystemTypeEnum.Boolean, "Can you clone an Element entity from a Schema that has not been activated yet?", true);
        Register(ParameterSystemConstants.SchemaInactiveCloneParameter, bool.TrueString, ParameterSystemTypeEnum.Boolean, "Can you clone a Parameter entity from a Schema that has not been activated yet?", true);
        Register(ParameterSystemConstants.SchemaInactiveCloneProperty, bool.TrueString, ParameterSystemTypeEnum.Boolean, "Can you clone a Property entity from a Schema that has not been activated yet?", true);

        Register(ParameterSystemConstants.SchemaInactiveEditEntity, bool.TrueString, ParameterSystemTypeEnum.Boolean, "Can you edit a Schema entity that has not been activated yet?", true);
        Register(ParameterSystemConstants.SchemaInactiveEditContext, bool.TrueString, ParameterSystemTypeEnum.Boolean, "Can you edit a Context entity from a Schema that has not been activated yet?", true);
        Register(ParameterSystemConstants.SchemaInactiveEditElement, bool.TrueString, ParameterSystemTypeEnum.Boolean, "Can you edit a Element entity from a Schema that has not been activated yet?", true);
        Register(ParameterSystemConstants.SchemaInactiveEditParameter, bool.TrueString, ParameterSystemTypeEnum.Boolean, "Can you edit a Property entity from a Schema that has not been activated yet?", true);
        Register(ParameterSystemConstants.SchemaInactiveEditProperty, bool.TrueString, ParameterSystemTypeEnum.Boolean, "Can you edit a Parameter entity from a Schema that has not been activated yet?", true);

        Register(ParameterSystemConstants.SchemaInactiveDeleteEntity, bool.TrueString, ParameterSystemTypeEnum.Boolean, "Can you remove a Schema entity that has not been activated yet?", true);
        Register(ParameterSystemConstants.SchemaInactiveDeleteContext, bool.TrueString, ParameterSystemTypeEnum.Boolean, "Can you remove a Context entity from a Schema that has not been activated yet?", true);
        Register(ParameterSystemConstants.SchemaInactiveDeleteElement, bool.TrueString, ParameterSystemTypeEnum.Boolean, "Can you remove an Element entity from a Schema that has not been activated yet?", true);
        Register(ParameterSystemConstants.SchemaInactiveDeleteParameter, bool.TrueString, ParameterSystemTypeEnum.Boolean, "Can you remove a Property entity from a Schema that has not been activated yet?", true);
        Register(ParameterSystemConstants.SchemaInactiveDeleteProperty, bool.TrueString, ParameterSystemTypeEnum.Boolean, "Can you remove a Parameter entity from a Schema that has not been activated yet?", true);
    }

    protected void Register(string constant, string value, ParameterSystemTypeEnum type = ParameterSystemTypeEnum.String, string note = "", bool isPublic = false, bool isApprovalRequired = true)
    {
        TableSchemaParameterSystem entitySchema = new()
        {
            ID = constant.ToDeterministicGuid(),
            SchemaIdentifier = constant,
            Name = constant,
            Note = note,
            Type = type,
            IsPublic = isPublic,
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
