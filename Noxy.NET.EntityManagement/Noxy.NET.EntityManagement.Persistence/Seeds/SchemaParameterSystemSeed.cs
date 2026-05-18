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
        Register(ParameterSystemConstants.SchemaDeactivatedAddContext, false.ToString(), ParameterSystemTypeEnum.Boolean, "Can you add new Context entities to a Schema that used to be active?", true);
        Register(ParameterSystemConstants.SchemaDeactivatedAddContextHasElement, false.ToString(), ParameterSystemTypeEnum.Boolean, "Can you add new ContextHasElement entities to a Schema that used to be active?", true);
        Register(ParameterSystemConstants.SchemaDeactivatedAddElement, false.ToString(), ParameterSystemTypeEnum.Boolean, "Can you add new Element entities to a Schema that used to be active?", true);
        Register(ParameterSystemConstants.SchemaDeactivatedAddElementHasProperty, false.ToString(), ParameterSystemTypeEnum.Boolean, "Can you add new ElementHasProperty entities to a Schema that used to be active?", true);
        Register(ParameterSystemConstants.SchemaDeactivatedAddParameter, false.ToString(), ParameterSystemTypeEnum.Boolean, "Can you add new Parameter entities to a Schema that used to be active?", true);
        Register(ParameterSystemConstants.SchemaDeactivatedAddProperty, false.ToString(), ParameterSystemTypeEnum.Boolean, "Can you add new Property entities to a Schema that used to be active?", true);

        Register(ParameterSystemConstants.SchemaDeactivatedCloneContext, false.ToString(), ParameterSystemTypeEnum.Boolean, "Can you clone a Context entity from a Schema that used to be active?", true);
        Register(ParameterSystemConstants.SchemaDeactivatedCloneElement, false.ToString(), ParameterSystemTypeEnum.Boolean, "Can you clone an Element entity from a Schema that used to be active?", true);
        Register(ParameterSystemConstants.SchemaDeactivatedCloneParameter, false.ToString(), ParameterSystemTypeEnum.Boolean, "Can you clone a Parameter entity from a Schema that used to be active?", true);
        Register(ParameterSystemConstants.SchemaDeactivatedCloneProperty, false.ToString(), ParameterSystemTypeEnum.Boolean, "Can you clone a Property entity from a Schema that used to be active?", true);

        Register(ParameterSystemConstants.SchemaDeactivatedEditEntity, false.ToString(), ParameterSystemTypeEnum.Boolean, "Can you edit a Schema entity that used to be active?", true);
        Register(ParameterSystemConstants.SchemaDeactivatedEditContext, false.ToString(), ParameterSystemTypeEnum.Boolean, "Can you edit a Context entity from a Schema that used to be active?", true);
        Register(ParameterSystemConstants.SchemaDeactivatedEditElement, false.ToString(), ParameterSystemTypeEnum.Boolean, "Can you edit an Element entity from a Schema that used to be active?", true);
        Register(ParameterSystemConstants.SchemaDeactivatedEditParameter, false.ToString(), ParameterSystemTypeEnum.Boolean, "Can you edit a Parameter entity from a Schema that used to be active?", true);
        Register(ParameterSystemConstants.SchemaDeactivatedEditProperty, false.ToString(), ParameterSystemTypeEnum.Boolean, "Can you edit a Property entity from a Schema that used to be active?", true);

        Register(ParameterSystemConstants.SchemaDeactivatedDeleteEntity, false.ToString(), ParameterSystemTypeEnum.Boolean, "Can you remove a Schema entity that used to be active?", true);
        Register(ParameterSystemConstants.SchemaDeactivatedDeleteContext, false.ToString(), ParameterSystemTypeEnum.Boolean, "Can you remove a Context entity from a Schema that used to be active?", true);
        Register(ParameterSystemConstants.SchemaDeactivatedDeleteElement, false.ToString(), ParameterSystemTypeEnum.Boolean, "Can you remove an Element entity from a Schema that used to be active?", true);
        Register(ParameterSystemConstants.SchemaDeactivatedDeleteParameter, false.ToString(), ParameterSystemTypeEnum.Boolean, "Can you remove a Parameter entity from a Schema that used to be active?", true);
        Register(ParameterSystemConstants.SchemaDeactivatedDeleteProperty, false.ToString(), ParameterSystemTypeEnum.Boolean, "Can you remove a Property entity from a Schema that used to be active?", true);


        Register(ParameterSystemConstants.SchemaInactiveAddContext, true.ToString(), ParameterSystemTypeEnum.Boolean, "Can you add new Context entities to a Schema that has not been activated yet?", true);
        Register(ParameterSystemConstants.SchemaInactiveAddContextHasElement, true.ToString(), ParameterSystemTypeEnum.Boolean, "Can you add new ContextHasElement entities to a Schema that has not been activated yet?", true);
        Register(ParameterSystemConstants.SchemaInactiveAddElement, true.ToString(), ParameterSystemTypeEnum.Boolean, "Can you add new Element entities to a Schema that has not been activated yet?", true);
        Register(ParameterSystemConstants.SchemaInactiveAddElementHasProperty, true.ToString(), ParameterSystemTypeEnum.Boolean, "Can you add new ElementHasProperty entities to a Schema that has not been activated yet?", true);
        Register(ParameterSystemConstants.SchemaInactiveAddParameter, true.ToString(), ParameterSystemTypeEnum.Boolean, "Can you add new Property entities to a Schema that has not been activated yet?", true);
        Register(ParameterSystemConstants.SchemaInactiveAddProperty, true.ToString(), ParameterSystemTypeEnum.Boolean, "Can you add new Parameter entities to a Schema that has not been activated yet?", true);

        Register(ParameterSystemConstants.SchemaInactiveCloneContext, false.ToString(), ParameterSystemTypeEnum.Boolean, "Can you clone a Context entity from a Schema that has not been activated yet?", true);
        Register(ParameterSystemConstants.SchemaInactiveCloneElement, false.ToString(), ParameterSystemTypeEnum.Boolean, "Can you clone an Element entity from a Schema that has not been activated yet?", true);
        Register(ParameterSystemConstants.SchemaInactiveCloneParameter, false.ToString(), ParameterSystemTypeEnum.Boolean, "Can you clone a Parameter entity from a Schema that has not been activated yet?", true);
        Register(ParameterSystemConstants.SchemaInactiveCloneProperty, false.ToString(), ParameterSystemTypeEnum.Boolean, "Can you clone a Property entity from a Schema that has not been activated yet?", true);

        Register(ParameterSystemConstants.SchemaInactiveEditEntity, true.ToString(), ParameterSystemTypeEnum.Boolean, "Can you edit a Schema entity that has not been activated yet?", true);
        Register(ParameterSystemConstants.SchemaInactiveEditContext, true.ToString(), ParameterSystemTypeEnum.Boolean, "Can you edit a Context entity from a Schema that has not been activated yet?", true);
        Register(ParameterSystemConstants.SchemaInactiveEditElement, true.ToString(), ParameterSystemTypeEnum.Boolean, "Can you edit a Element entity from a Schema that has not been activated yet?", true);
        Register(ParameterSystemConstants.SchemaInactiveEditParameter, true.ToString(), ParameterSystemTypeEnum.Boolean, "Can you edit a Property entity from a Schema that has not been activated yet?", true);
        Register(ParameterSystemConstants.SchemaInactiveEditProperty, true.ToString(), ParameterSystemTypeEnum.Boolean, "Can you edit a Parameter entity from a Schema that has not been activated yet?", true);

        Register(ParameterSystemConstants.SchemaInactiveDeleteEntity, true.ToString(), ParameterSystemTypeEnum.Boolean, "Can you remove a Schema entity that has not been activated yet?", true);
        Register(ParameterSystemConstants.SchemaInactiveDeleteContext, true.ToString(), ParameterSystemTypeEnum.Boolean, "Can you remove a Context entity from a Schema that has not been activated yet?", true);
        Register(ParameterSystemConstants.SchemaInactiveDeleteElement, true.ToString(), ParameterSystemTypeEnum.Boolean, "Can you remove an Element entity from a Schema that has not been activated yet?", true);
        Register(ParameterSystemConstants.SchemaInactiveDeleteParameter, true.ToString(), ParameterSystemTypeEnum.Boolean, "Can you remove a Property entity from a Schema that has not been activated yet?", true);
        Register(ParameterSystemConstants.SchemaInactiveDeleteProperty, true.ToString(), ParameterSystemTypeEnum.Boolean, "Can you remove a Parameter entity from a Schema that has not been activated yet?", true);
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
