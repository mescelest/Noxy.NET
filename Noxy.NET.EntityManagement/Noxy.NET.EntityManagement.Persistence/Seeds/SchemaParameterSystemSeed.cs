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
        Register(ParameterSystemConstants.SchemaDeactivatedContextAdd, bool.FalseString, ParameterSystemTypeEnum.Boolean, "Can you add new Context entities to a Schema that used to be active?", true);
        Register(ParameterSystemConstants.SchemaDeactivatedContextHasElementAdd, bool.FalseString, ParameterSystemTypeEnum.Boolean, "Can you add new ContextHasElement entities to a Schema that used to be active?", true);
        Register(ParameterSystemConstants.SchemaDeactivatedElementAdd, bool.FalseString, ParameterSystemTypeEnum.Boolean, "Can you add new Element entities to a Schema that used to be active?", true);
        Register(ParameterSystemConstants.SchemaDeactivatedElementHasPropertyAdd, bool.FalseString, ParameterSystemTypeEnum.Boolean, "Can you add new ElementHasProperty entities to a Schema that used to be active?", true);
        Register(ParameterSystemConstants.SchemaDeactivatedParameterAdd, bool.FalseString, ParameterSystemTypeEnum.Boolean, "Can you add new Parameter entities to a Schema that used to be active?", true);
        Register(ParameterSystemConstants.SchemaDeactivatedPropertyAdd, bool.FalseString, ParameterSystemTypeEnum.Boolean, "Can you add new Property entities to a Schema that used to be active?", true);

        Register(ParameterSystemConstants.SchemaDeactivatedContextClone, bool.FalseString, ParameterSystemTypeEnum.Boolean, "Can you clone a Context entity from a Schema that used to be active?", true);
        Register(ParameterSystemConstants.SchemaDeactivatedElementClone, bool.FalseString, ParameterSystemTypeEnum.Boolean, "Can you clone an Element entity from a Schema that used to be active?", true);
        Register(ParameterSystemConstants.SchemaDeactivatedParameterClone, bool.FalseString, ParameterSystemTypeEnum.Boolean, "Can you clone a Parameter entity from a Schema that used to be active?", true);
        Register(ParameterSystemConstants.SchemaDeactivatedPropertyClone, bool.FalseString, ParameterSystemTypeEnum.Boolean, "Can you clone a Property entity from a Schema that used to be active?", true);

        Register(ParameterSystemConstants.SchemaDeactivatedEntityEdit, bool.FalseString, ParameterSystemTypeEnum.Boolean, "Can you edit a Schema entity that used to be active?", true);
        Register(ParameterSystemConstants.SchemaDeactivatedContextEdit, bool.FalseString, ParameterSystemTypeEnum.Boolean, "Can you edit a Context entity from a Schema that used to be active?", true);
        Register(ParameterSystemConstants.SchemaDeactivatedElementEdit, bool.FalseString, ParameterSystemTypeEnum.Boolean, "Can you edit an Element entity from a Schema that used to be active?", true);
        Register(ParameterSystemConstants.SchemaDeactivatedParameterEdit, bool.FalseString, ParameterSystemTypeEnum.Boolean, "Can you edit a Parameter entity from a Schema that used to be active?", true);
        Register(ParameterSystemConstants.SchemaDeactivatedPropertyEdit, bool.FalseString, ParameterSystemTypeEnum.Boolean, "Can you edit a Property entity from a Schema that used to be active?", true);

        Register(ParameterSystemConstants.SchemaDeactivatedEntityDelete, bool.FalseString, ParameterSystemTypeEnum.Boolean, "Can you remove a Schema entity that used to be active?", true);
        Register(ParameterSystemConstants.SchemaDeactivatedContextDelete, bool.FalseString, ParameterSystemTypeEnum.Boolean, "Can you remove a Context entity from a Schema that used to be active?", true);
        Register(ParameterSystemConstants.SchemaDeactivatedElementDelete, bool.FalseString, ParameterSystemTypeEnum.Boolean, "Can you remove an Element entity from a Schema that used to be active?", true);
        Register(ParameterSystemConstants.SchemaDeactivatedParameterDelete, bool.FalseString, ParameterSystemTypeEnum.Boolean, "Can you remove a Parameter entity from a Schema that used to be active?", true);
        Register(ParameterSystemConstants.SchemaDeactivatedPropertyDelete, bool.FalseString, ParameterSystemTypeEnum.Boolean, "Can you remove a Property entity from a Schema that used to be active?", true);


        Register(ParameterSystemConstants.SchemaInactiveContextAdd, bool.TrueString, ParameterSystemTypeEnum.Boolean, "Can you add new Context entities to a Schema that has not been activated yet?", true);
        Register(ParameterSystemConstants.SchemaInactiveContextHasElementAdd, bool.TrueString, ParameterSystemTypeEnum.Boolean, "Can you add new ContextHasElement entities to a Schema that has not been activated yet?", true);
        Register(ParameterSystemConstants.SchemaInactiveElementAdd, bool.TrueString, ParameterSystemTypeEnum.Boolean, "Can you add new Element entities to a Schema that has not been activated yet?", true);
        Register(ParameterSystemConstants.SchemaInactiveElementHasPropertyAdd, bool.TrueString, ParameterSystemTypeEnum.Boolean, "Can you add new ElementHasProperty entities to a Schema that has not been activated yet?", true);
        Register(ParameterSystemConstants.SchemaInactiveParameterAdd, bool.TrueString, ParameterSystemTypeEnum.Boolean, "Can you add new Property entities to a Schema that has not been activated yet?", true);
        Register(ParameterSystemConstants.SchemaInactivePropertyAdd, bool.TrueString, ParameterSystemTypeEnum.Boolean, "Can you add new Parameter entities to a Schema that has not been activated yet?", true);

        Register(ParameterSystemConstants.SchemaInactiveContextClone, bool.TrueString, ParameterSystemTypeEnum.Boolean, "Can you clone a Context entity from a Schema that has not been activated yet?", true);
        Register(ParameterSystemConstants.SchemaInactiveElementClone, bool.TrueString, ParameterSystemTypeEnum.Boolean, "Can you clone an Element entity from a Schema that has not been activated yet?", true);
        Register(ParameterSystemConstants.SchemaInactiveParameterClone, bool.TrueString, ParameterSystemTypeEnum.Boolean, "Can you clone a Parameter entity from a Schema that has not been activated yet?", true);
        Register(ParameterSystemConstants.SchemaInactivePropertyClone, bool.TrueString, ParameterSystemTypeEnum.Boolean, "Can you clone a Property entity from a Schema that has not been activated yet?", true);

        Register(ParameterSystemConstants.SchemaInactiveEntityEdit, bool.TrueString, ParameterSystemTypeEnum.Boolean, "Can you edit a Schema entity that has not been activated yet?", true);
        Register(ParameterSystemConstants.SchemaInactiveContextEdit, bool.TrueString, ParameterSystemTypeEnum.Boolean, "Can you edit a Context entity from a Schema that has not been activated yet?", true);
        Register(ParameterSystemConstants.SchemaInactiveElementEdit, bool.TrueString, ParameterSystemTypeEnum.Boolean, "Can you edit a Element entity from a Schema that has not been activated yet?", true);
        Register(ParameterSystemConstants.SchemaInactiveParameterEdit, bool.TrueString, ParameterSystemTypeEnum.Boolean, "Can you edit a Property entity from a Schema that has not been activated yet?", true);
        Register(ParameterSystemConstants.SchemaInactivePropertyEdit, bool.TrueString, ParameterSystemTypeEnum.Boolean, "Can you edit a Parameter entity from a Schema that has not been activated yet?", true);

        Register(ParameterSystemConstants.SchemaInactiveEntityDelete, bool.TrueString, ParameterSystemTypeEnum.Boolean, "Can you remove a Schema entity that has not been activated yet?", true);
        Register(ParameterSystemConstants.SchemaInactiveContextDelete, bool.TrueString, ParameterSystemTypeEnum.Boolean, "Can you remove a Context entity from a Schema that has not been activated yet?", true);
        Register(ParameterSystemConstants.SchemaInactiveElementDelete, bool.TrueString, ParameterSystemTypeEnum.Boolean, "Can you remove an Element entity from a Schema that has not been activated yet?", true);
        Register(ParameterSystemConstants.SchemaInactiveParameterDelete, bool.TrueString, ParameterSystemTypeEnum.Boolean, "Can you remove a Property entity from a Schema that has not been activated yet?", true);
        Register(ParameterSystemConstants.SchemaInactivePropertyDelete, bool.TrueString, ParameterSystemTypeEnum.Boolean, "Can you remove a Parameter entity from a Schema that has not been activated yet?", true);
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
            ID = $"{constant}-Value".ToDeterministicGuid(),
            SchemaIdentifier = constant,
            Value = value,
            TimeApproved = Now,
            TimeEffective = Now,
            TimeCreated = Now
        };
        Builder.Entity<TableDataParameterSystem>().HasData(entityValue);
    }
}
