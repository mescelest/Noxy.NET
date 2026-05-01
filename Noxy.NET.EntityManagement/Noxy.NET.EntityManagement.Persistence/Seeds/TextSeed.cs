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
        RegisterText(TextConstants.HeaderFormCreateSchema, "Create Schema");
        RegisterText(TextConstants.HeaderFormCreateSchemaContext, "Create Schema Context");
        RegisterText(TextConstants.HeaderFormCreateSchemaElement, "Create Schema Element");
        RegisterText(TextConstants.HeaderFormCreateSchemaParameterStyle, "Create Schema Style Parameter");
        RegisterText(TextConstants.HeaderFormCreateSchemaParameterSystem, "Create Schema System Parameter");
        RegisterText(TextConstants.HeaderFormCreateSchemaParameterText, "Create Schema Text Parameter");
        RegisterText(TextConstants.HeaderFormCreateSchemaPropertyBoolean, "Create Schema Boolean Property");
        RegisterText(TextConstants.HeaderFormCreateSchemaPropertyCollection, "Create Schema Collection Property");
        RegisterText(TextConstants.HeaderFormCreateSchemaPropertyDateTime, "Create Schema DateTime Property");
        RegisterText(TextConstants.HeaderFormCreateSchemaPropertyDecimal, "Create Schema Decimal Property");
        RegisterText(TextConstants.HeaderFormCreateSchemaPropertyImage, "Create Schema Image Property");
        RegisterText(TextConstants.HeaderFormCreateSchemaPropertyInteger, "Create Schema Integer Property");
        RegisterText(TextConstants.HeaderFormCreateSchemaPropertyString, "Create Schema String Property");
        RegisterText(TextConstants.HeaderFormCreateSchemaPropertyTable, "Create Schema Table Property");
        RegisterText(TextConstants.HeaderFormUpdateSchema, "Update Schema");
        RegisterText(TextConstants.HeaderFormUpdateSchemaContext, "Update Schema Context");
        RegisterText(TextConstants.HeaderFormUpdateSchemaElement, "Update Schema Element");
        RegisterText(TextConstants.HeaderFormUpdateSchemaParameterStyle, "Update Schema Style Parameter");
        RegisterText(TextConstants.HeaderFormUpdateSchemaParameterSystem, "Update Schema System Parameter");
        RegisterText(TextConstants.HeaderFormUpdateSchemaParameterText, "Update Schema Text Parameter");
        RegisterText(TextConstants.HeaderFormUpdateSchemaPropertyBoolean, "Update Schema Boolean Property");
        RegisterText(TextConstants.HeaderFormUpdateSchemaPropertyCollection, "Update Schema Collection Property");
        RegisterText(TextConstants.HeaderFormUpdateSchemaPropertyDateTime, "Update Schema DateTime Property");
        RegisterText(TextConstants.HeaderFormUpdateSchemaPropertyDecimal, "Update Schema Decimal Property");
        RegisterText(TextConstants.HeaderFormUpdateSchemaPropertyImage, "Update Schema Image Property");
        RegisterText(TextConstants.HeaderFormUpdateSchemaPropertyInteger, "Update Schema Integer Property");
        RegisterText(TextConstants.HeaderFormUpdateSchemaPropertyString, "Update Schema String Property");
        RegisterText(TextConstants.HeaderFormUpdateSchemaPropertyTable, "Update Schema Table Property");

        #endregion

        #region Label

        RegisterText(TextConstants.LabelValue, "Value");
        RegisterText(TextConstants.LabelParameterStyle, "Style parameter");
        RegisterText(TextConstants.LabelParameterSystem, "System parameter");
        RegisterText(TextConstants.LabelParameterText, "Text parameter");
        RegisterText(TextConstants.LabelParameterTextType, "Text parameter type");
        RegisterText(TextConstants.LabelCulture, "Culture");
        RegisterText(TextConstants.LabelSchemaIdentifier, "Schema Identifier");
        RegisterText(TextConstants.LabelName, "Name");
        RegisterText(TextConstants.LabelNote, "Note");
        RegisterText(TextConstants.LabelWeight, "Weight");
        RegisterText(TextConstants.LabelTitle, "Title");
        RegisterText(TextConstants.LabelDescription, "Description");
        RegisterText(TextConstants.LabelIsApprovalRequired, "Is approval required?");
        RegisterText(TextConstants.LabelIsSystemDefined, "Is system defined?");
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
        RegisterText(TextConstants.LabelFormWeight, "Weight");
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
        RegisterText(TextConstants.HelpFormWeight, "The sorting weight of this entity type when it is being ordered.");
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
        RegisterText(TextConstants.ValueParameterTextTypeLine, "Line");
        RegisterText(TextConstants.ValueParameterTextTypeText, "Text");
        RegisterText(TextConstants.ValueParameterTextTypeRichText, "Rich text");
        RegisterText(TextConstants.ValueIsSystemDefinedNeutral, "Either");
        RegisterText(TextConstants.ValueIsSystemDefinedYes, "System defined");
        RegisterText(TextConstants.ValueIsSystemDefinedNo, "User defined");
        RegisterText(TextConstants.ValueIsApprovalRequiredNeutral, "Either");
        RegisterText(TextConstants.ValueIsApprovalRequiredYes, "With approval required");
        RegisterText(TextConstants.ValueIsApprovalRequiredNo, "Without approval required");

        #endregion
    }
}
