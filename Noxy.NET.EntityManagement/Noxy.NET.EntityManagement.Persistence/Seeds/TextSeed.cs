using Microsoft.EntityFrameworkCore;
using Noxy.NET.EntityManagement.Domain.Constants;
using Noxy.NET.EntityManagement.Persistence.Abstractions;
using Noxy.NET.EntityManagement.Persistence.Tables.Schemas;

namespace Noxy.NET.EntityManagement.Persistence.Seeds;

public class TextSeed(ModelBuilder builder, TableSchema refSchema) : BaseSeed(builder, refSchema)
{
    public void Apply()
    {
        #region Baseline

        RegisterText(TextConstants.Title, "Noxy.NET");

        #endregion Baseline

        #region DefaultEmpty

        RegisterText(TextConstants.DefaultEmptyValue, "-");
        RegisterText(TextConstants.DefaultEmptyList, "Nothing here yet...");

        #endregion DefaultEmpty

        #region Button

        RegisterText(TextConstants.ButtonActivate, "Activate");
        RegisterText(TextConstants.ButtonSearch, "Search");
        RegisterText(TextConstants.ButtonFilter, "Filter");
        RegisterText(TextConstants.ButtonReset, "Reset");
        RegisterText(TextConstants.ButtonCreate, "Create");
        RegisterText(TextConstants.ButtonCancel, "Cancel");
        RegisterText(TextConstants.ButtonUpdate, "Update");
        RegisterText(TextConstants.ButtonSubmit, "Submit");
        RegisterText(TextConstants.ButtonSignIn, "Sign in");
        RegisterText(TextConstants.ButtonSignUp, "Sign up");
        RegisterText(TextConstants.ButtonSignOut, "Sign out");

        #endregion Button

        #region Link

        RegisterText(TextConstants.LinkNavigationSchema, "Schemas");
        RegisterText(TextConstants.LinkNavigationParameter, "Parameters");

        #endregion Link

        #region Header

        RegisterText(TextConstants.HeaderSchema, "Schemas");
        RegisterText(TextConstants.HeaderContext, "Contexts");
        RegisterText(TextConstants.HeaderElement, "Elements");
        RegisterText(TextConstants.HeaderParameter, "Parameters");
        RegisterText(TextConstants.HeaderParameterStyle, "Style Parameters");
        RegisterText(TextConstants.HeaderParameterSystem, "System Parameters");
        RegisterText(TextConstants.HeaderParameterText, "Text Parameters");
        RegisterText(TextConstants.HeaderProperty, "Properties");
        RegisterText(TextConstants.HeaderPropertyBoolean, "Boolean Properties");
        RegisterText(TextConstants.HeaderPropertyCollection, "Collection Properties");
        RegisterText(TextConstants.HeaderPropertyDateTime, "DateTime Properties");
        RegisterText(TextConstants.HeaderPropertyDecimal, "Decimal Properties");
        RegisterText(TextConstants.HeaderPropertyImage, "Image Properties");
        RegisterText(TextConstants.HeaderPropertyInteger, "Integer Properties");
        RegisterText(TextConstants.HeaderPropertyString, "String Properties");
        RegisterText(TextConstants.HeaderPropertyTable, "Table Properties");

        #endregion Header

        #region Header:Form

        RegisterText(TextConstants.HeaderFormSignIn, "...Or sign in");
        RegisterText(TextConstants.HeaderFormSignUp, "Create an account");
        RegisterText(TextConstants.HeaderFormCreateEntitySchema, "Create Schema");
        RegisterText(TextConstants.HeaderFormCreateEntitySchemaContext, "Create Schema Context");
        RegisterText(TextConstants.HeaderFormCreateEntitySchemaElement, "Create Schema Element");
        RegisterText(TextConstants.HeaderFormCreateEntitySchemaParameterStyle, "Create Schema Style Parameter");
        RegisterText(TextConstants.HeaderFormCreateEntitySchemaParameterSystem, "Create Schema System Parameter");
        RegisterText(TextConstants.HeaderFormCreateEntitySchemaParameterText, "Create Schema Text Parameter");
        RegisterText(TextConstants.HeaderFormCreateEntitySchemaPropertyBoolean, "Create Schema Boolean Property");
        RegisterText(TextConstants.HeaderFormCreateEntitySchemaPropertyCollection, "Create Schema Collection Property");
        RegisterText(TextConstants.HeaderFormCreateEntitySchemaPropertyDateTime, "Create Schema DateTime Property");
        RegisterText(TextConstants.HeaderFormCreateEntitySchemaPropertyDecimal, "Create Schema Decimal Property");
        RegisterText(TextConstants.HeaderFormCreateEntitySchemaPropertyImage, "Create Schema Image Property");
        RegisterText(TextConstants.HeaderFormCreateEntitySchemaPropertyInteger, "Create Schema Integer Property");
        RegisterText(TextConstants.HeaderFormCreateEntitySchemaPropertyString, "Create Schema String Property");
        RegisterText(TextConstants.HeaderFormCreateEntitySchemaPropertyTable, "Create Schema Table Property");
        RegisterText(TextConstants.HeaderFormUpdateEntitySchema, "Update Schema");
        RegisterText(TextConstants.HeaderFormUpdateEntitySchemaContext, "Update Schema Context");
        RegisterText(TextConstants.HeaderFormUpdateEntitySchemaElement, "Update Schema Element");
        RegisterText(TextConstants.HeaderFormUpdateEntitySchemaParameterStyle, "Update Schema Style Parameter");
        RegisterText(TextConstants.HeaderFormUpdateEntitySchemaParameterSystem, "Update Schema System Parameter");
        RegisterText(TextConstants.HeaderFormUpdateEntitySchemaParameterText, "Update Schema Text Parameter");
        RegisterText(TextConstants.HeaderFormUpdateEntitySchemaPropertyBoolean, "Update Schema Boolean Property");
        RegisterText(TextConstants.HeaderFormUpdateEntitySchemaPropertyCollection, "Update Schema Collection Property");
        RegisterText(TextConstants.HeaderFormUpdateEntitySchemaPropertyDateTime, "Update Schema DateTime Property");
        RegisterText(TextConstants.HeaderFormUpdateEntitySchemaPropertyDecimal, "Update Schema Decimal Property");
        RegisterText(TextConstants.HeaderFormUpdateEntitySchemaPropertyImage, "Update Schema Image Property");
        RegisterText(TextConstants.HeaderFormUpdateEntitySchemaPropertyInteger, "Update Schema Integer Property");
        RegisterText(TextConstants.HeaderFormUpdateEntitySchemaPropertyString, "Update Schema String Property");
        RegisterText(TextConstants.HeaderFormUpdateEntitySchemaPropertyTable, "Update Schema Table Property");

        #endregion

        #region Label

        RegisterText(TextConstants.LabelValue, "Value");
        RegisterText(TextConstants.LabelParameterStyle, "Style parameter");
        RegisterText(TextConstants.LabelParameterSystem, "System parameter");
        RegisterText(TextConstants.LabelParameterText, "Text parameter");
        RegisterText(TextConstants.LabelCulture, "Culture");
        RegisterText(TextConstants.LabelTimeApproved, "Approved at");
        RegisterText(TextConstants.LabelTimeCreated, "Created at");
        RegisterText(TextConstants.LabelTimeEffective, "Effective at");
        RegisterText(TextConstants.LabelTimeEffectiveFrom, "Effective from");
        RegisterText(TextConstants.LabelTimeEffectiveTo, "Effective to");

        #endregion Label

        #region Label:Form

        RegisterText(TextConstants.LabelFormEmail, "Email");
        RegisterText(TextConstants.LabelFormPassword, "Password");
        RegisterText(TextConstants.LabelFormConfirmPassword, "Confirm password");
        RegisterText(TextConstants.LabelFormSearch, "Search");
        RegisterText(TextConstants.LabelFormSchemaIdentifier, "Schema identifier");
        RegisterText(TextConstants.LabelFormValue, "Value");
        RegisterText(TextConstants.LabelFormCulture, "Culture");
        RegisterText(TextConstants.LabelFormName, "Name");
        RegisterText(TextConstants.LabelFormNote, "Note");
        RegisterText(TextConstants.LabelFormTitle, "Title");
        RegisterText(TextConstants.LabelFormDescription, "Description");
        RegisterText(TextConstants.LabelFormOrder, "Order");
        RegisterText(TextConstants.LabelFormIsSystemDefined, "Is system defined?");
        RegisterText(TextConstants.LabelFormIsApprovalRequired, "Is approval required?");
        RegisterText(TextConstants.LabelFormParameterType, "Parameter type");
        RegisterText(TextConstants.LabelFormDateEffective, "Effective date");
        RegisterText(TextConstants.LabelFormPageNumber, "Page");
        RegisterText(TextConstants.LabelFormPageSize, "Rows");
        RegisterText(TextConstants.LabelFormParameterTextType, "Text parameter type");
        RegisterText(TextConstants.LabelFormBoolean, "Boolean");
        RegisterText(TextConstants.LabelFormDateTime, "DateTime");
        RegisterText(TextConstants.LabelFormDecimal, "Decimal");
        RegisterText(TextConstants.LabelFormInteger, "Integer");
        RegisterText(TextConstants.LabelFormString, "String");
        RegisterText(TextConstants.LabelFormParameterStyle, "Style parameter");
        RegisterText(TextConstants.LabelFormParameterSystem, "System parameter");
        RegisterText(TextConstants.LabelFormParameterText, "Text parameter");

        #endregion Label:Form

        #region Help:Form

        RegisterText(TextConstants.HelpFormEmail, "The email address you want to associate with the account.");
        RegisterText(TextConstants.HelpFormPassword, "A password consisting of at least 12 characters.");
        RegisterText(TextConstants.HelpFormConfirmPassword, "Repeat your password to confirm it.");
        RegisterText(TextConstants.HelpFormSearch, "Enter the search term you wish to use");
        RegisterText(TextConstants.HelpFormSchemaIdentifier, "The unique, humanly readable identifier for this entity type.");
        RegisterText(TextConstants.HelpFormValue, "The value to be used with this entity.");
        RegisterText(TextConstants.HelpFormCulture, "The culture this entity is used together with.");
        RegisterText(TextConstants.HelpFormName, "The internal name of this entity type. Should only be visible in the configuration.");
        RegisterText(TextConstants.HelpFormNote, "A short note detailing how this entity type is used. Should only be visible in the configuration.");
        RegisterText(TextConstants.HelpFormTitle, "The title used when displaying an entity of this type.");
        RegisterText(TextConstants.HelpFormDescription, "The description used when displaying an entity of this type.");
        RegisterText(TextConstants.HelpFormOrder, "The order in which this entity type is sorted.");
        RegisterText(TextConstants.HelpFormIsSystemDefined, "Determines if the entity is system defined and therefore cannot be changed.");
        RegisterText(TextConstants.HelpFormIsApprovalRequired, "Determines if another user must approve a text parameter value before it becomes active.");
        RegisterText(TextConstants.HelpFormParameterType, "The specific type of parameter to be used.");
        RegisterText(TextConstants.HelpFormDateEffective, "The date from which this entity should become active.");
        RegisterText(TextConstants.HelpFormPageNumber, "The current page being shown");
        RegisterText(TextConstants.HelpFormPageSize, "The number of rows shown on each page");
        RegisterText(TextConstants.HelpFormParameterTextType, "The type of text parameter.");
        RegisterText(TextConstants.HelpFormBoolean, "Represents a boolean value.");
        RegisterText(TextConstants.HelpFormDateTime, "Represents a datetime value.");
        RegisterText(TextConstants.HelpFormDecimal, "Represents a decimal value.");
        RegisterText(TextConstants.HelpFormInteger, "Represents a integer value.");
        RegisterText(TextConstants.HelpFormString, "Represents a string value.");
        RegisterText(TextConstants.HelpFormParameterStyle, "Represents a dynamic style parameter value.");
        RegisterText(TextConstants.HelpFormParameterSystem, "Represents a dynamic system parameter value.");
        RegisterText(TextConstants.HelpFormParameterText, "Represents a dynamic text parameter value.");

        #endregion Help:Form

        #region Value

        RegisterText(TextConstants.ValueOptionYes, "Yes");
        RegisterText(TextConstants.ValueOptionNo, "No");
        RegisterText(TextConstants.ValueIsSystemDefinedNeutral, "Either");
        RegisterText(TextConstants.ValueIsSystemDefinedYes, "System defined");
        RegisterText(TextConstants.ValueIsSystemDefinedNo, "User defined");
        RegisterText(TextConstants.ValueIsApprovalRequiredNeutral, "Either");
        RegisterText(TextConstants.ValueIsApprovalRequiredYes, "With approval required");
        RegisterText(TextConstants.ValueIsApprovalRequiredNo, "Without approval required");

        #endregion
    }
}
