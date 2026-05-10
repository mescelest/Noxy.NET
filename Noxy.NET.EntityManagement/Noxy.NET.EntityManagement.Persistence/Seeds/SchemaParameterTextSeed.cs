using Microsoft.EntityFrameworkCore;
using Noxy.NET.EntityManagement.Domain.Constants;
using Noxy.NET.EntityManagement.Domain.Enums;
using Noxy.NET.EntityManagement.Persistence.Abstractions;
using Noxy.NET.EntityManagement.Persistence.Tables.Data;
using Noxy.NET.EntityManagement.Persistence.Tables.Schemas;
using Noxy.NET.Extensions;

namespace Noxy.NET.EntityManagement.Persistence.Seeds;

public class SchemaParameterTextSeed(ModelBuilder builder, TableSchema refSchema) : BaseSeed(builder, refSchema)
{
    public void Apply()
    {
        #region Baseline

        Register(ParameterTextConstants.Title, "Noxy.NET");

        #endregion Baseline

        #region DefaultEmpty

        Register(ParameterTextConstants.DefaultEmptyValue, "-");
        Register(ParameterTextConstants.DefaultEmptyList, "Nothing here yet...");

        #endregion DefaultEmpty

        #region Button

        Register(ParameterTextConstants.ButtonActivate, "Activate");
        Register(ParameterTextConstants.ButtonSearch, "Search");
        Register(ParameterTextConstants.ButtonFilter, "Filter");
        Register(ParameterTextConstants.ButtonReset, "Reset");
        Register(ParameterTextConstants.ButtonCreate, "Create");
        Register(ParameterTextConstants.ButtonCancel, "Cancel");
        Register(ParameterTextConstants.ButtonUpdate, "Update");
        Register(ParameterTextConstants.ButtonSubmit, "Submit");
        Register(ParameterTextConstants.ButtonSignIn, "Sign in");
        Register(ParameterTextConstants.ButtonSignUp, "Sign up");
        Register(ParameterTextConstants.ButtonSignOut, "Sign out");

        #endregion Button

        #region Link

        Register(ParameterTextConstants.LinkNavigationSchema, "Schemas");
        Register(ParameterTextConstants.LinkNavigationParameter, "Parameters");

        #endregion Link

        #region Header

        Register(ParameterTextConstants.HeaderSchema, "Schemas");
        Register(ParameterTextConstants.HeaderContext, "Contexts");
        Register(ParameterTextConstants.HeaderElement, "Elements");
        Register(ParameterTextConstants.HeaderParameter, "Parameters");
        Register(ParameterTextConstants.HeaderParameterStyle, "Style Parameters");
        Register(ParameterTextConstants.HeaderParameterSystem, "System Parameters");
        Register(ParameterTextConstants.HeaderParameterText, "Text Parameters");
        Register(ParameterTextConstants.HeaderProperty, "Properties");
        Register(ParameterTextConstants.HeaderPropertyBoolean, "Boolean Properties");
        Register(ParameterTextConstants.HeaderPropertyCollection, "Collection Properties");
        Register(ParameterTextConstants.HeaderPropertyDateTime, "DateTime Properties");
        Register(ParameterTextConstants.HeaderPropertyDecimal, "Decimal Properties");
        Register(ParameterTextConstants.HeaderPropertyImage, "Image Properties");
        Register(ParameterTextConstants.HeaderPropertyInteger, "Integer Properties");
        Register(ParameterTextConstants.HeaderPropertyString, "String Properties");
        Register(ParameterTextConstants.HeaderPropertyTable, "Table Properties");

        #endregion Header

        #region Header:Form

        Register(ParameterTextConstants.HeaderFormSignIn, "...Or sign in");
        Register(ParameterTextConstants.HeaderFormSignUp, "Create an account");
        Register(ParameterTextConstants.HeaderFormCreateSchema, "Create Schema");
        Register(ParameterTextConstants.HeaderFormCreateSchemaContext, "Create Schema Context");
        Register(ParameterTextConstants.HeaderFormCreateSchemaElement, "Create Schema Element");
        Register(ParameterTextConstants.HeaderFormCreateSchemaParameterStyle, "Create Schema Style Parameter");
        Register(ParameterTextConstants.HeaderFormCreateSchemaParameterSystem, "Create Schema System Parameter");
        Register(ParameterTextConstants.HeaderFormCreateSchemaParameterText, "Create Schema Text Parameter");
        Register(ParameterTextConstants.HeaderFormCreateSchemaPropertyBoolean, "Create Schema Boolean Property");
        Register(ParameterTextConstants.HeaderFormCreateSchemaPropertyCollection, "Create Schema Collection Property");
        Register(ParameterTextConstants.HeaderFormCreateSchemaPropertyDateTime, "Create Schema DateTime Property");
        Register(ParameterTextConstants.HeaderFormCreateSchemaPropertyDecimal, "Create Schema Decimal Property");
        Register(ParameterTextConstants.HeaderFormCreateSchemaPropertyImage, "Create Schema Image Property");
        Register(ParameterTextConstants.HeaderFormCreateSchemaPropertyInteger, "Create Schema Integer Property");
        Register(ParameterTextConstants.HeaderFormCreateSchemaPropertyString, "Create Schema String Property");
        Register(ParameterTextConstants.HeaderFormCreateSchemaPropertyTable, "Create Schema Table Property");
        Register(ParameterTextConstants.HeaderFormUpdateSchema, "Update Schema");
        Register(ParameterTextConstants.HeaderFormUpdateSchemaContext, "Update Schema Context");
        Register(ParameterTextConstants.HeaderFormUpdateSchemaElement, "Update Schema Element");
        Register(ParameterTextConstants.HeaderFormUpdateSchemaParameterStyle, "Update Schema Style Parameter");
        Register(ParameterTextConstants.HeaderFormUpdateSchemaParameterSystem, "Update Schema System Parameter");
        Register(ParameterTextConstants.HeaderFormUpdateSchemaParameterText, "Update Schema Text Parameter");
        Register(ParameterTextConstants.HeaderFormUpdateSchemaPropertyBoolean, "Update Schema Boolean Property");
        Register(ParameterTextConstants.HeaderFormUpdateSchemaPropertyCollection, "Update Schema Collection Property");
        Register(ParameterTextConstants.HeaderFormUpdateSchemaPropertyDateTime, "Update Schema DateTime Property");
        Register(ParameterTextConstants.HeaderFormUpdateSchemaPropertyDecimal, "Update Schema Decimal Property");
        Register(ParameterTextConstants.HeaderFormUpdateSchemaPropertyImage, "Update Schema Image Property");
        Register(ParameterTextConstants.HeaderFormUpdateSchemaPropertyInteger, "Update Schema Integer Property");
        Register(ParameterTextConstants.HeaderFormUpdateSchemaPropertyString, "Update Schema String Property");
        Register(ParameterTextConstants.HeaderFormUpdateSchemaPropertyTable, "Update Schema Table Property");

        #endregion

        #region Label

        Register(ParameterTextConstants.LabelID, "ID");
        Register(ParameterTextConstants.LabelValue, "Value");
        Register(ParameterTextConstants.LabelParameterStyle, "Style parameter");
        Register(ParameterTextConstants.LabelParameterSystem, "System parameter");
        Register(ParameterTextConstants.LabelParameterText, "Text parameter");
        Register(ParameterTextConstants.LabelParameterTextType, "Text parameter type");
        Register(ParameterTextConstants.LabelPropertyBoolean, "Boolean property");
        Register(ParameterTextConstants.LabelPropertyDateTime, "DateTime property");
        Register(ParameterTextConstants.LabelPropertyDecimal, "Decimal property");
        Register(ParameterTextConstants.LabelPropertyImage, "Image property");
        Register(ParameterTextConstants.LabelPropertyInteger, "Integer property");
        Register(ParameterTextConstants.LabelPropertyString, "String property");
        Register(ParameterTextConstants.LabelPropertyDateTimeType, "DateTime property type");
        Register(ParameterTextConstants.LabelCulture, "Culture");
        Register(ParameterTextConstants.LabelSchemaIdentifier, "Schema Identifier");
        Register(ParameterTextConstants.LabelName, "Name");
        Register(ParameterTextConstants.LabelNote, "Note");
        Register(ParameterTextConstants.LabelWeight, "Weight");
        Register(ParameterTextConstants.LabelTitle, "Title");
        Register(ParameterTextConstants.LabelDescription, "Description");
        Register(ParameterTextConstants.LabelIsApprovalRequired, "Is approval required?");
        Register(ParameterTextConstants.LabelIsSystemDefined, "Is system defined?");
        Register(ParameterTextConstants.LabelTimeApproved, "Approved at");
        Register(ParameterTextConstants.LabelTimeActivated, "Activated at");
        Register(ParameterTextConstants.LabelTimeCreated, "Created at");
        Register(ParameterTextConstants.LabelTimeEffective, "Effective at");
        Register(ParameterTextConstants.LabelTimeEffectiveFrom, "Effective from");
        Register(ParameterTextConstants.LabelTimeEffectiveTo, "Effective to");

        #endregion Label

        #region Label:Form

        Register(ParameterTextConstants.LabelFormEmail, "Email");
        Register(ParameterTextConstants.LabelFormPassword, "Password");
        Register(ParameterTextConstants.LabelFormConfirmPassword, "Confirm password");
        Register(ParameterTextConstants.LabelFormSearch, "Search");
        Register(ParameterTextConstants.LabelFormSchemaIdentifier, "Schema identifier");
        Register(ParameterTextConstants.LabelFormValue, "Value");
        Register(ParameterTextConstants.LabelFormCulture, "Culture");
        Register(ParameterTextConstants.LabelFormName, "Name");
        Register(ParameterTextConstants.LabelFormNote, "Note");
        Register(ParameterTextConstants.LabelFormTitle, "Title");
        Register(ParameterTextConstants.LabelFormDescription, "Description");
        Register(ParameterTextConstants.LabelFormWeight, "Weight");
        Register(ParameterTextConstants.LabelFormDateTimeType, "DateTime type");
        Register(ParameterTextConstants.LabelFormIsSystemDefined, "Is system defined?");
        Register(ParameterTextConstants.LabelFormIsApprovalRequired, "Is approval required?");
        Register(ParameterTextConstants.LabelFormIsActivated, "Is activated?");
        Register(ParameterTextConstants.LabelFormParameterType, "Parameter type");
        Register(ParameterTextConstants.LabelFormPropertyType, "Property type");
        Register(ParameterTextConstants.LabelFormDateEffective, "Effective date");
        Register(ParameterTextConstants.LabelFormPageSize, "Rows");
        Register(ParameterTextConstants.LabelFormPageNumber, "Page");
        Register(ParameterTextConstants.LabelFormSortColumn, "Sorting column");
        Register(ParameterTextConstants.LabelFormSortDirection, "Sorting direction");
        Register(ParameterTextConstants.LabelFormParameterTextType, "Text parameter type");
        Register(ParameterTextConstants.LabelFormParameterSystemType, "System parameter type");
        Register(ParameterTextConstants.LabelFormBoolean, "Boolean");
        Register(ParameterTextConstants.LabelFormDateTime, "DateTime");
        Register(ParameterTextConstants.LabelFormDecimal, "Decimal");
        Register(ParameterTextConstants.LabelFormInteger, "Integer");
        Register(ParameterTextConstants.LabelFormString, "String");
        Register(ParameterTextConstants.LabelFormParameterStyle, "Style parameter");
        Register(ParameterTextConstants.LabelFormParameterSystem, "System parameter");
        Register(ParameterTextConstants.LabelFormParameterText, "Text parameter");

        #endregion Label:Form

        #region Help:Form

        Register(ParameterTextConstants.HelpFormEmail, "The email address you want to associate with the account.");
        Register(ParameterTextConstants.HelpFormPassword, "A password consisting of at least 12 characters.");
        Register(ParameterTextConstants.HelpFormConfirmPassword, "Repeat your password to confirm it.");
        Register(ParameterTextConstants.HelpFormSearch, "Enter the search term you wish to use");
        Register(ParameterTextConstants.HelpFormSchemaIdentifier, "The unique, humanly readable identifier for this entity type.");
        Register(ParameterTextConstants.HelpFormValue, "The value to be used with this entity.");
        Register(ParameterTextConstants.HelpFormCulture, "The culture this entity is used together with.");
        Register(ParameterTextConstants.HelpFormName, "The internal name of this entity type. Should only be visible in the configuration.");
        Register(ParameterTextConstants.HelpFormNote, "A short note detailing how this entity type is used. Should only be visible in the configuration.");
        Register(ParameterTextConstants.HelpFormTitle, "The title used when displaying an entity of this type.");
        Register(ParameterTextConstants.HelpFormDescription, "The description used when displaying an entity of this type.");
        Register(ParameterTextConstants.HelpFormWeight, "The sorting weight of this entity type when it is being ordered.");
        Register(ParameterTextConstants.HelpFormDateTimeType, "The type of DateTime property.");
        Register(ParameterTextConstants.HelpFormIsSystemDefined, "Determines if the entity is system defined and therefore cannot be changed.");
        Register(ParameterTextConstants.HelpFormIsApprovalRequired, "Determines if another user must approve a text parameter value before it becomes active.");
        Register(ParameterTextConstants.HelpFormIsActivated, "Determines if the entity is active or has been previously activated.");
        Register(ParameterTextConstants.HelpFormParameterType, "The specific type of parameter to be used.");
        Register(ParameterTextConstants.HelpFormPropertyType, "The type of property.");
        Register(ParameterTextConstants.HelpFormDateEffective, "The date from which this entity should become active.");
        Register(ParameterTextConstants.HelpFormPageNumber, "The current page being shown");
        Register(ParameterTextConstants.HelpFormSortColumn, "The column which the page is sorted by");
        Register(ParameterTextConstants.HelpFormSortDirection, "The direction which the page is sorted by");
        Register(ParameterTextConstants.HelpFormPageSize, "The number of rows shown on each page");
        Register(ParameterTextConstants.HelpFormParameterTextType, "The type of text parameter.");
        Register(ParameterTextConstants.HelpFormParameterSystemType, "The type of system parameter.");
        Register(ParameterTextConstants.HelpFormBoolean, "Represents a boolean value.");
        Register(ParameterTextConstants.HelpFormDateTime, "Represents a datetime value.");
        Register(ParameterTextConstants.HelpFormDecimal, "Represents a decimal value.");
        Register(ParameterTextConstants.HelpFormInteger, "Represents a integer value.");
        Register(ParameterTextConstants.HelpFormString, "Represents a string value.");
        Register(ParameterTextConstants.HelpFormParameterStyle, "Represents a dynamic style parameter value.");
        Register(ParameterTextConstants.HelpFormParameterSystem, "Represents a dynamic system parameter value.");
        Register(ParameterTextConstants.HelpFormParameterText, "Represents a dynamic text parameter value.");

        #endregion Help:Form

        #region Value

        Register(ParameterTextConstants.ValueYes, "Yes");
        Register(ParameterTextConstants.ValueNo, "No");
        Register(ParameterTextConstants.ValueAscending, "Ascending");
        Register(ParameterTextConstants.ValueDescending, "Descending");
        Register(ParameterTextConstants.ValueParameterTextTypeLine, "Line");
        Register(ParameterTextConstants.ValueParameterTextTypeText, "Text");
        Register(ParameterTextConstants.ValueParameterTextTypeRichText, "Rich text");
        Register(ParameterTextConstants.ValuePropertyDateTimeTypeTime, "Time");
        Register(ParameterTextConstants.ValuePropertyDateTimeTypeDate, "Date");
        Register(ParameterTextConstants.ValuePropertyDateTimeTypeDateTime, "Date and time");
        Register(ParameterTextConstants.ValueIsSystemDefinedNeutral, "Either");
        Register(ParameterTextConstants.ValueIsSystemDefinedYes, "System defined");
        Register(ParameterTextConstants.ValueIsSystemDefinedNo, "User defined");
        Register(ParameterTextConstants.ValueIsActivatedNeutral, "Either");
        Register(ParameterTextConstants.ValueIsActivatedYes, "Has been activated");
        Register(ParameterTextConstants.ValueIsActivatedNo, "Never activated");
        Register(ParameterTextConstants.ValueIsApprovalRequiredNeutral, "Either");
        Register(ParameterTextConstants.ValueIsApprovalRequiredYes, "With approval required");
        Register(ParameterTextConstants.ValueIsApprovalRequiredNo, "Without approval required");

        #endregion
    }

    protected void Register(string constant, string value, string culture = "en", ParameterTextTypeEnum type = ParameterTextTypeEnum.Line, string note = "", bool isApprovalRequired = false)
    {
        TableSchemaParameterText tableParameterText = new()
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
        Builder.Entity<TableSchemaParameterText>().HasData(tableParameterText);

        TableDataParameterText tableParameterValue = new()
        {
            ID = $"{constant}:Value".ToDeterministicGuid(),
            SchemaIdentifier = constant,
            Culture = culture,
            Value = value,
            TimeApproved = Now,
            TimeEffective = Now,
            TimeCreated = Now
        };
        Builder.Entity<TableDataParameterText>().HasData(tableParameterValue);
    }
}
