using Microsoft.EntityFrameworkCore;
using Noxy.NET.EntityManagement.Domain.Constants;
using Noxy.NET.EntityManagement.Persistence.Abstractions;
using Noxy.NET.EntityManagement.Persistence.Tables.Schemas;

namespace Noxy.NET.EntityManagement.Persistence.Seeds;

public class TextSeed(ModelBuilder builder, TableSchema refSchema) : BaseSeed(builder, refSchema)
{
    public void Apply()
    {
        HasSchemaParameterText("019c2571-7b67-75bb-a029-42c44fbfd1fb", TextConstants.Title);
        HasDataParameterText("019c2571-7b67-75bb-a029-45530805ee07", TextConstants.Title, "Noxy.NET");

        HasSchemaParameterText("019789fc-3929-75a9-99e9-11f9d8b81e8a", TextConstants.DefaultEmptyValue);
        HasSchemaParameterText("019c2584-67dc-749e-971e-580816b3b821", TextConstants.DefaultEmptyList);

        HasDataParameterText("019789fc-3929-75a9-99e9-142d13f18fb0", TextConstants.DefaultEmptyValue, "-");
        HasDataParameterText("019c2584-67dc-749e-971e-5e6410aeeacd", TextConstants.DefaultEmptyList, "Nothing here yet...");

        HasSchemaParameterText("019764ca-25c4-7785-bd02-daa168ae477d", TextConstants.ButtonActivate);
        HasSchemaParameterText("019c29f9-5de4-760e-90b6-c0b6d174ed8f", TextConstants.ButtonSearch);
        HasSchemaParameterText("019c29f9-5de4-760e-90b6-c5ba80aa9a14", TextConstants.ButtonFilter);
        HasSchemaParameterText("019c29f9-5de4-760e-90b6-d26d9226e044", TextConstants.ButtonReset);
        HasSchemaParameterText("019764ca-25c4-7785-bd02-de67a804e592", TextConstants.ButtonCreate);
        HasSchemaParameterText("019764ca-25c4-7785-bd02-e26550fa3aa9", TextConstants.ButtonUpdate);
        HasSchemaParameterText("019764ca-25c4-7785-bd02-e5902e766443", TextConstants.ButtonSubmit);
        HasSchemaParameterText("01977fb8-8d9e-7179-b098-d8800c456351", TextConstants.ButtonSignIn);
        HasSchemaParameterText("01977fb8-8d9e-7179-b098-dde7fae42dc4", TextConstants.ButtonSignUp);
        HasSchemaParameterText("019c2571-7b67-75bb-a029-3ad9550d778d", TextConstants.ButtonSignOut);

        HasDataParameterText("019764ca-25c4-7785-bd02-ebdc5a27fb39", TextConstants.ButtonActivate, "Activate");
        HasDataParameterText("019c29f9-5de4-760e-90b6-caaaa39d8d0d", TextConstants.ButtonSearch, "Search");
        HasDataParameterText("019c29f9-5de4-760e-90b6-cd1d017360d4", TextConstants.ButtonFilter, "Filter");
        HasDataParameterText("019c29f9-5de4-760e-90b6-d5266cbcd3fd", TextConstants.ButtonReset, "Reset");
        HasDataParameterText("019764ca-25c4-7785-bd02-efa276b57b62", TextConstants.ButtonCreate, "Create");
        HasDataParameterText("019764ca-25c4-7785-bd02-f1e439a3bb07", TextConstants.ButtonUpdate, "Update");
        HasDataParameterText("019764ca-25c4-7785-bd02-f7c5260cb82d", TextConstants.ButtonSubmit, "Submit");
        HasDataParameterText("01977fb8-8d9e-7179-b098-d37940c4d817", TextConstants.ButtonSignIn, "Sign in");
        HasDataParameterText("01977fb8-8d9e-7179-b098-d431e095ba95", TextConstants.ButtonSignUp, "Sign up");
        HasDataParameterText("019c2571-7b67-75bb-a029-3eea76c0f0c3", TextConstants.ButtonSignOut, "Sign out");

        HasSchemaParameterText("01978309-3029-74e9-931c-3cf1322948fd", TextConstants.LinkNavigationSchema);
        HasSchemaParameterText("019c3efa-a140-73be-900f-2e766665bc18", TextConstants.LinkNavigationParameter);

        HasDataParameterText("01978309-3029-74e9-931c-436de21f95b0", TextConstants.LinkNavigationSchema, "Schemas");
        HasDataParameterText("019c3efa-a140-73be-900f-33d0023c5698", TextConstants.LinkNavigationParameter, "Parameters");

        HasSchemaParameterText("019c254b-1b85-7dca-abe2-d2a11538b7ed", TextConstants.HeaderSchema);
        HasSchemaParameterText("019c254b-1b85-71dc-8f4a-4295bd279c0a", TextConstants.HeaderContext);
        HasSchemaParameterText("019c254b-1b85-772d-aaf0-dff4ea453c6c", TextConstants.HeaderElement);
        HasSchemaParameterText("019c254b-1b85-7ec8-9720-24a5bcaf1888", TextConstants.HeaderParameter);
        HasSchemaParameterText("019c2960-0f40-756c-b67a-982d29ade43c", TextConstants.HeaderParameterStyle);
        HasSchemaParameterText("019c2960-0f40-756c-b67a-9fecaa359970", TextConstants.HeaderParameterSystem);
        HasSchemaParameterText("019c2960-0f40-756c-b67a-a2ee6d674e65", TextConstants.HeaderParameterText);
        HasSchemaParameterText("019c254b-1b85-7b4f-bd18-355a9af4b607", TextConstants.HeaderProperty);
        HasSchemaParameterText("019c2960-9149-73cf-a2a9-8655ebbe904f", TextConstants.HeaderPropertyBoolean);
        HasSchemaParameterText("019c2960-9149-73cf-a2a9-89219fb3f605", TextConstants.HeaderPropertyCollection);
        HasSchemaParameterText("019c2960-9149-73cf-a2a9-8f6cfdeda288", TextConstants.HeaderPropertyDateTime);
        HasSchemaParameterText("019c2960-9149-73cf-a2a9-90eae211c597", TextConstants.HeaderPropertyDecimal);
        HasSchemaParameterText("019c2960-9149-73cf-a2a9-97f682a1a142", TextConstants.HeaderPropertyImage);
        HasSchemaParameterText("019c2960-9149-73cf-a2a9-9a52f7cd3179", TextConstants.HeaderPropertyInteger);
        HasSchemaParameterText("019c2960-9149-73cf-a2a9-9fce8dad7570", TextConstants.HeaderPropertyString);
        HasSchemaParameterText("019c2960-9149-73cf-a2a9-a2f08d495c1c", TextConstants.HeaderPropertyTable);

        HasDataParameterText("019c254a-c196-7c51-a3c1-82f826185ea8", TextConstants.HeaderSchema, "Schemas");
        HasDataParameterText("019c254a-c196-7595-b493-5ccf1ea9faad", TextConstants.HeaderContext, "Contexts");
        HasDataParameterText("019c254a-c196-7968-8109-da3f68bd324f", TextConstants.HeaderElement, "Elements");
        HasDataParameterText("019c254a-c196-7e7d-98bb-36fd8384fb8f", TextConstants.HeaderParameter, "Parameters");
        HasDataParameterText("019c2570-ace5-7641-8c58-2e1be9103503", TextConstants.HeaderParameterStyle, "Style Parameters");
        HasDataParameterText("019c2570-ace5-7641-8c58-33c7d3d23c9c", TextConstants.HeaderParameterSystem, "System Parameters");
        HasDataParameterText("019c2570-ace5-7641-8c58-36370d2a0092", TextConstants.HeaderParameterText, "Text Parameters");
        HasDataParameterText("019c254a-c196-734d-aeaa-3f93015db3bc", TextConstants.HeaderProperty, "Properties");
        HasDataParameterText("019c2571-2306-7122-ba5f-910cbf856f22", TextConstants.HeaderPropertyBoolean, "Boolean Properties");
        HasDataParameterText("019c2571-2306-7122-ba5f-953e8fa4c021", TextConstants.HeaderPropertyCollection, "Collection Properties");
        HasDataParameterText("019c2571-2306-7122-ba5f-9b4244c8d98f", TextConstants.HeaderPropertyDateTime, "DateTime Properties");
        HasDataParameterText("019c2571-2306-7122-ba5f-9ecf6b7c21d9", TextConstants.HeaderPropertyDecimal, "Decimal Properties");
        HasDataParameterText("019c2571-2306-7122-ba5f-a1272b07a512", TextConstants.HeaderPropertyImage, "Image Properties");
        HasDataParameterText("019c2571-2306-7122-ba5f-a4fc019da2c8", TextConstants.HeaderPropertyInteger, "Integer Properties");
        HasDataParameterText("019c2571-2306-7122-ba5f-aa68f923e6cf", TextConstants.HeaderPropertyString, "String Properties");
        HasDataParameterText("019c2571-7b67-75bb-a029-35dcce9e1f9f", TextConstants.HeaderPropertyTable, "Table Properties");

        HasSchemaParameterText("019c3efa-a140-73be-900f-360a6ccd354c", TextConstants.LabelValue);
        HasSchemaParameterText("019c3efa-a140-73be-900f-3bf25ff45c30", TextConstants.LabelTimeApproved);
        HasSchemaParameterText("019c3efa-a140-73be-900f-3ecaa29e0b39", TextConstants.LabelTimeCreated);
        HasSchemaParameterText("019c3efa-a140-73be-900f-40a664ac2558", TextConstants.LabelTimeEffectiveFrom);
        HasSchemaParameterText("019c3efa-a140-73be-900f-45045dba457e", TextConstants.LabelTimeEffectiveTo);

        HasDataParameterText("019c3efa-a140-73be-900f-4bed02a7227c", TextConstants.LabelValue, "Value");
        HasDataParameterText("019c3efa-a140-73be-900f-4e2b26906c90", TextConstants.LabelTimeApproved, "Approved at");
        HasDataParameterText("019c47d2-99d6-76d6-bffc-a46b61cfe957", TextConstants.LabelTimeCreated, "Created at");
        HasDataParameterText("019c3efd-764d-740a-baae-c0240683a292", TextConstants.LabelTimeEffectiveFrom, "Effective from");
        HasDataParameterText("019c3efd-764d-740a-baae-c7503349db66", TextConstants.LabelTimeEffectiveTo, "Effective to");

        HasSchemaParameterText("019c2964-c66f-77de-83ad-b8caf05e9319", TextConstants.LabelFormEmail);
        HasSchemaParameterText("019c2964-c66f-77de-83ad-bfe74128c9df", TextConstants.LabelFormPassword);
        HasSchemaParameterText("019c2964-c66f-77de-83ad-c219ed783910", TextConstants.LabelFormConfirmPassword);
        HasSchemaParameterText("019c297a-384a-716a-92bd-67c96ecb2b83", TextConstants.LabelFormSearch);
        HasSchemaParameterText("019789de-e449-71aa-ab1d-0992c66a9dae", TextConstants.LabelFormSchemaIdentifier);
        HasSchemaParameterText("019c4c5d-b1c8-722e-ac29-e0f469968754", TextConstants.LabelFormValue);
        HasSchemaParameterText("019789de-e449-71aa-ab1d-12e8b546b239", TextConstants.LabelFormName);
        HasSchemaParameterText("019789de-e449-71aa-ab1d-17209b94ded6", TextConstants.LabelFormNote);
        HasSchemaParameterText("019789de-e449-71aa-ab1d-192c4a3c0eb6", TextConstants.LabelFormTitle);
        HasSchemaParameterText("019789de-e449-71aa-ab1d-1c1707f61f89", TextConstants.LabelFormDescription);
        HasSchemaParameterText("019789de-e449-71aa-ab1d-205e04219122", TextConstants.LabelFormOrder);
        HasSchemaParameterText("019c297a-384a-716a-92bd-54bc539e59e1", TextConstants.LabelFormIsSystemDefined);
        HasSchemaParameterText("01978a17-7901-7131-8b49-005b042a1608", TextConstants.LabelFormIsApprovalRequired);
        HasSchemaParameterText("019c4c5d-b1c8-722e-ac2a-010981c08758", TextConstants.LabelFormDateEffective);
        HasSchemaParameterText("019c3819-d223-700d-a78d-40606a57c869", TextConstants.LabelFormPageNumber);
        HasSchemaParameterText("019c3819-d224-705d-91a0-96e586add967", TextConstants.LabelFormPageSize);
        HasSchemaParameterText("01979d14-54ad-72f9-b5d8-a5aa1089fe1b", TextConstants.LabelFormParameterTextType);
        HasSchemaParameterText("019c47d2-99d6-76d6-bffc-a94bdaa55937", TextConstants.LabelFormPropertyTypeList);
        HasSchemaParameterText("01978a1f-f0a2-731f-b17d-14f803055489", TextConstants.LabelFormIsValueList);
        HasSchemaParameterText("019799a3-a72f-725a-b4f6-b7144f565f3f", TextConstants.LabelFormBoolean);
        HasSchemaParameterText("019799a3-a72f-725a-b4f6-b8b9e65fa2e9", TextConstants.LabelFormDateTime);
        HasSchemaParameterText("019799a3-a72f-725a-b4f6-be98a390cf1a", TextConstants.LabelFormDecimal);
        HasSchemaParameterText("019799a3-a72f-725a-b4f6-c0efbd9b5dbb", TextConstants.LabelFormInteger);
        HasSchemaParameterText("019799a3-a72f-725a-b4f6-c449cc81422f", TextConstants.LabelFormString);
        HasSchemaParameterText("019799a3-a72f-725a-b4f6-cd308b80c164", TextConstants.LabelFormParameterStyle);
        HasSchemaParameterText("019799a3-a72f-725a-b4f6-d3344af0a274", TextConstants.LabelFormParameterSystem);
        HasSchemaParameterText("019799a3-a72f-725a-b4f6-d6b06a4f6d1b", TextConstants.LabelFormParameterText);

        HasDataParameterText("019c2964-c66f-77de-83ad-c60e1292abfa", TextConstants.LabelFormEmail, "Email");
        HasDataParameterText("019c2964-c66f-77de-83ad-c8e93394b591", TextConstants.LabelFormPassword, "Password");
        HasDataParameterText("019c2964-c66f-77de-83ad-cf27649f093d", TextConstants.LabelFormConfirmPassword, "Confirm password");
        HasDataParameterText("019c297a-384a-716a-92bd-6ad2110dfd27", TextConstants.LabelFormSearch, "Search");
        HasDataParameterText("019789f9-2601-72ac-ad27-ea4b8f4855d6", TextConstants.LabelFormSchemaIdentifier, "Schema identifier");
        HasDataParameterText("019c4c5d-b1c8-722e-ac29-e4745ec5528e", TextConstants.LabelFormValue, "Value");
        HasDataParameterText("019789f9-2601-72ac-ad27-f1f7ad078b01", TextConstants.LabelFormName, "Name");
        HasDataParameterText("019789f9-2601-72ac-ad27-f7c76f0002d4", TextConstants.LabelFormNote, "Note");
        HasDataParameterText("019789f9-2601-72ac-ad27-f9ef87027fec", TextConstants.LabelFormTitle, "Title");
        HasDataParameterText("019789f9-2601-72ac-ad27-fceadc8f7eda", TextConstants.LabelFormDescription, "Description");
        HasDataParameterText("019789f9-2601-72ac-ad28-0204b7c6d497", TextConstants.LabelFormOrder, "Order");
        HasDataParameterText("019c297a-384a-716a-92bd-5b2db391ac89", TextConstants.LabelFormIsSystemDefined, "Is system defined?");
        HasDataParameterText("01978a17-7901-7131-8b49-1200bad01c81", TextConstants.LabelFormIsApprovalRequired, "Is approval required?");
        HasDataParameterText("019c4c5d-b1c8-722e-ac29-fee711de1d54", TextConstants.LabelFormDateEffective, "Effective date");
        HasDataParameterText("019c3819-d224-705d-91a0-98a1e7acf79a", TextConstants.LabelFormPageNumber, "Page");
        HasDataParameterText("019c3819-d224-705d-91a0-9f7cd0157d26", TextConstants.LabelFormPageSize, "Rows");
        HasDataParameterText("01978a1f-f0a2-731f-b17d-2174a6ecd4fe", TextConstants.LabelFormParameterTextType, "Text parameter type");
        HasDataParameterText("01979d14-54ad-72f9-b5d8-ae413650b40c", TextConstants.LabelFormPropertyTypeList, "Choose property type");
        HasDataParameterText("019799a4-1a2b-7368-9b33-6a3f0bf60dae", TextConstants.LabelFormIsValueList, "Is value list?");
        HasDataParameterText("019799a4-1a2b-7368-9b33-6c45805d80a3", TextConstants.LabelFormBoolean, "Boolean");
        HasDataParameterText("019799a4-1a2b-7368-9b33-72817ce487a1", TextConstants.LabelFormDateTime, "DateTime");
        HasDataParameterText("019799a4-1a2b-7368-9b33-77fdd478928f", TextConstants.LabelFormDecimal, "Decimal");
        HasDataParameterText("019799a4-1a2b-7368-9b33-781625aca070", TextConstants.LabelFormInteger, "Integer");
        HasDataParameterText("019799a4-1a2b-7368-9b33-7c0e241b9622", TextConstants.LabelFormString, "String");
        HasDataParameterText("019799a4-1a2b-7368-9b33-86038825b246", TextConstants.LabelFormParameterStyle, "Style parameter");
        HasDataParameterText("019799a4-1a2b-7368-9b33-8bd3f0f5e948", TextConstants.LabelFormParameterSystem, "System parameter");
        HasDataParameterText("019799a4-1a2b-7368-9b33-8c2e6b4a0b6d", TextConstants.LabelFormParameterText, "Text parameter");

        HasSchemaParameterText("019c2964-c66f-77de-83ad-d2e4058c6134", TextConstants.HelpFormEmail);
        HasSchemaParameterText("019c2964-c66f-77de-83ad-d586d9686df2", TextConstants.HelpFormPassword);
        HasSchemaParameterText("019c2964-c66f-77de-83ad-d89e8c04683e", TextConstants.HelpFormConfirmPassword);
        HasSchemaParameterText("019c29f9-5de4-760e-90b6-d8e7a6c0239a", TextConstants.HelpFormSearch);
        HasSchemaParameterText("019789fc-3929-75a9-99e8-f7a6abf0c139", TextConstants.HelpFormSchemaIdentifier);
        HasSchemaParameterText("019c4c5d-b1c8-722e-ac29-e8fc5683a10d", TextConstants.HelpFormValue);
        HasSchemaParameterText("019789fc-3929-75a9-99e8-f819edf63934", TextConstants.HelpFormName);
        HasSchemaParameterText("019789fc-3929-75a9-99e8-ffd616e87b30", TextConstants.HelpFormNote);
        HasSchemaParameterText("019789fc-3929-75a9-99e9-0229499f0ddd", TextConstants.HelpFormTitle);
        HasSchemaParameterText("019789fc-3929-75a9-99e9-046632ba51b7", TextConstants.HelpFormDescription);
        HasSchemaParameterText("019789fc-3929-75a9-99e9-0a5f8ca00604", TextConstants.HelpFormOrder);
        HasSchemaParameterText("019c297a-384a-716a-92bd-5cefb96ac3a0", TextConstants.HelpFormIsSystemDefined);
        HasSchemaParameterText("01978a17-b7b1-772f-a129-543b087e1606", TextConstants.HelpFormIsApprovalRequired);
        HasSchemaParameterText("019c4c5d-b1c8-722e-ac29-f7ab4a6300bd", TextConstants.HelpFormDateEffective);
        HasSchemaParameterText("019c3819-d224-705d-91a0-a1fc39ba13d5", TextConstants.HelpFormPageNumber);
        HasSchemaParameterText("019c3819-d224-705d-91a0-a5d6e5fdfc6d", TextConstants.HelpFormPageSize);
        HasSchemaParameterText("01979d14-54ad-72f9-b5d8-b33557c5a806", TextConstants.HelpFormParameterTextType);
        HasSchemaParameterText("01979d14-54ad-72f9-b5d8-c3a68639a28a", TextConstants.HelpFormParameterTypeList);
        HasSchemaParameterText("01979d14-54ad-72f9-b5d8-c4050ea1f0fd", TextConstants.HelpFormPropertyTypeList);
        HasSchemaParameterText("01978a20-9692-72ff-be7d-a9bb3d99565a", TextConstants.HelpFormIsValueList);
        HasSchemaParameterText("019799a6-8dc0-75af-8866-97a26b3415dc", TextConstants.HelpFormBoolean);
        HasSchemaParameterText("019799a6-8dc0-75af-8866-986c4570a7bd", TextConstants.HelpFormDateTime);
        HasSchemaParameterText("019799a6-8dc0-75af-8866-9f89357ebb7f", TextConstants.HelpFormDecimal);
        HasSchemaParameterText("019799a6-8dc0-75af-8866-a390807971d2", TextConstants.HelpFormInteger);
        HasSchemaParameterText("019799a6-8dc0-75af-8866-a44ca62f2e18", TextConstants.HelpFormString);
        HasSchemaParameterText("019799a6-8dc0-75af-8866-ad7a01af4eac", TextConstants.HelpFormParameterStyle);
        HasSchemaParameterText("019799a6-8dc0-75af-8866-b0be19e7dcdd", TextConstants.HelpFormParameterSystem);
        HasSchemaParameterText("019799a6-8dc0-75af-8866-b4f08d0df491", TextConstants.HelpFormParameterText);

        HasDataParameterText("019c2964-c66f-77de-83ad-ddee52bf7792", TextConstants.HelpFormEmail, "The email address you want to associate with the account.");
        HasDataParameterText("019c2964-c66f-77de-83ad-e28d48839f57", TextConstants.HelpFormPassword, "A password consisting of at least 12 characters.");
        HasDataParameterText("019c2964-c66f-77de-83ad-e416d3c0445a", TextConstants.HelpFormConfirmPassword, "Repeat your password to confirm it.");
        HasDataParameterText("019c297a-384a-716a-92bd-6e1c4715908c", TextConstants.HelpFormSearch, "Enter the search term you wish to use");
        HasDataParameterText("019789fc-18dc-73ee-94b6-1564b2f72eb7", TextConstants.HelpFormSchemaIdentifier, "The unique, humanly readable identifier for this entity type.");
        HasDataParameterText("019c4c5d-b1c8-722e-ac29-f25552e22b82", TextConstants.HelpFormValue, "The value to be used with this entity.");
        HasDataParameterText("019789fc-18dc-73ee-94b6-1c60e96f26a9", TextConstants.HelpFormName, "The internal name of this entity type. Should only be visible in the configuration.");
        HasDataParameterText("019789fc-18dc-73ee-94b6-21b290f8401d", TextConstants.HelpFormNote, "A short note detailing how this entity type is used. Should only be visible in the configuration.");
        HasDataParameterText("019789fc-18dc-73ee-94b6-26ead5f18d4d", TextConstants.HelpFormTitle, "The title used when displaying an entity of this type.");
        HasDataParameterText("019789fc-18dc-73ee-94b6-28dc0edf9b69", TextConstants.HelpFormDescription, "The description used when displaying an entity of this type.");
        HasDataParameterText("019789fc-18dc-73ee-94b6-2ee0b27366dc", TextConstants.HelpFormOrder, "The order in which this entity type is sorted.");
        HasDataParameterText("019c297a-384a-716a-92bd-627c7c93c3c9", TextConstants.HelpFormIsSystemDefined, "Determines if the entity is system defined and therefore cannot be changed.");
        HasDataParameterText("01978a17-b7b1-772f-a129-66dba634b0b5", TextConstants.HelpFormIsApprovalRequired, "Determines if another user must approve a text parameter value before it becomes active.");
        HasDataParameterText("019c4c5d-b1c8-722e-ac29-f9cc8fe007f7", TextConstants.HelpFormDateEffective, "The date from which this entity should become active.");
        HasDataParameterText("019c3819-d224-705d-91a0-a9fe4d149531", TextConstants.HelpFormPageNumber, "The current page being shown");
        HasDataParameterText("019c3819-d224-705d-91a0-af95528a7316", TextConstants.HelpFormPageSize, "The number of rows shown on each page");
        HasDataParameterText("01978a20-9692-72ff-be7d-b59a17facafb", TextConstants.HelpFormParameterTextType, "The type of text parameter.");
        HasDataParameterText("01979d14-54ad-72f9-b5d8-b830dc26f9f4", TextConstants.HelpFormParameterTypeList, "");
        HasDataParameterText("01979d14-54ad-72f9-b5d8-bf20682e54af", TextConstants.HelpFormPropertyTypeList, "");
        HasDataParameterText("019799a6-6b05-7029-b9a7-2d147f778de8", TextConstants.HelpFormBoolean, "Represents a boolean value.");
        HasDataParameterText("019799a6-6b05-7029-b9a7-300f0306a227", TextConstants.HelpFormDateTime, "Represents a datetime value.");
        HasDataParameterText("019799a6-6b05-7029-b9a7-35f76c49841a", TextConstants.HelpFormDecimal, "Represents a decimal value.");
        HasDataParameterText("019799a6-6b05-7029-b9a7-3a924229a88a", TextConstants.HelpFormInteger, "Represents a integer value.");
        HasDataParameterText("019799a6-6b05-7029-b9a7-3ca405bc7246", TextConstants.HelpFormString, "Represents a string value.");
        HasDataParameterText("019799a6-6b05-7029-b9a7-473fbbc23ec3", TextConstants.HelpFormParameterStyle, "Represents a dynamic style parameter value.");
        HasDataParameterText("019799a6-6b05-7029-b9a7-48183a3903a4", TextConstants.HelpFormParameterSystem, "Represents a dynamic system parameter value.");
        HasDataParameterText("019799a6-6b05-7029-b9a7-4c3fb79cf9d1", TextConstants.HelpFormParameterText, "Represents a dynamic text parameter value.");

        HasSchemaParameterText("019c27aa-cee4-758d-a50d-333d37fb8c53", TextConstants.ValueFormCheckboxYes);

        HasDataParameterText("019c27aa-cee4-758d-a50d-3543259e9a3f", TextConstants.ValueFormCheckboxYes, "Yes");

        HasSchemaParameterText("019c2571-7b67-75bb-a029-49faffc52b0f", TextConstants.HeaderFormSignIn);
        HasSchemaParameterText("019c2571-7b67-75bb-a029-4fcd838e27ad", TextConstants.HeaderFormSignUp);

        HasDataParameterText("019c2584-67dc-749e-971e-554b0930363a", TextConstants.HeaderFormSignIn, "...Or sign in");
        HasDataParameterText("019c2584-67dc-749e-971e-536af5f07c05", TextConstants.HeaderFormSignUp, "Create an account");

        HasSchemaParameterText("019c240c-791c-7579-8011-fd306ef3be7d", TextConstants.HeaderFormCreateEntitySchema);
        HasSchemaParameterText("019c240c-791c-7d7e-accd-c533af9a13f2", TextConstants.HeaderFormCreateEntitySchemaContext);
        HasSchemaParameterText("019c240c-791c-73eb-be79-a8f9fd6195bf", TextConstants.HeaderFormCreateEntitySchemaElement);
        HasSchemaParameterText("019c240c-791c-70b5-866b-f755d1c45382", TextConstants.HeaderFormCreateEntitySchemaParameterStyle);
        HasSchemaParameterText("019c240c-791c-79a9-ad25-bd0e7f197bfe", TextConstants.HeaderFormCreateEntitySchemaParameterSystem);
        HasSchemaParameterText("019c240c-791c-7b41-ad27-573d62c60f54", TextConstants.HeaderFormCreateEntitySchemaParameterText);
        HasSchemaParameterText("019c240c-791c-728d-a32f-32740d710848", TextConstants.HeaderFormCreateEntitySchemaPropertyBoolean);
        HasSchemaParameterText("019c240c-791c-77c6-9d30-a90e02a17d97", TextConstants.HeaderFormCreateEntitySchemaPropertyCollection);
        HasSchemaParameterText("019c240c-791c-780b-8287-8db5087a9579", TextConstants.HeaderFormCreateEntitySchemaPropertyDateTime);
        HasSchemaParameterText("019c240c-791c-7a15-946f-d1cf59ddac12", TextConstants.HeaderFormCreateEntitySchemaPropertyDecimal);
        HasSchemaParameterText("019c240c-791c-7a50-8798-918e8726d667", TextConstants.HeaderFormCreateEntitySchemaPropertyImage);
        HasSchemaParameterText("019c240c-791c-7afa-a013-344f1600999c", TextConstants.HeaderFormCreateEntitySchemaPropertyInteger);
        HasSchemaParameterText("019c240c-791c-7ffb-8f29-38fb70a51934", TextConstants.HeaderFormCreateEntitySchemaPropertyString);
        HasSchemaParameterText("019c240c-791c-77f9-97ee-ad37630c7fdf", TextConstants.HeaderFormCreateEntitySchemaPropertyTable);

        HasDataParameterText("019c240d-19d8-7f6e-a91d-db2e3c9e6e5e", TextConstants.HeaderFormCreateEntitySchema, "Create Schema");
        HasDataParameterText("019c240d-19d8-71b0-b48d-639f8765db5a", TextConstants.HeaderFormCreateEntitySchemaContext, "Create Schema Context");
        HasDataParameterText("019c240d-19d8-754f-9c95-ca256884cd81", TextConstants.HeaderFormCreateEntitySchemaElement, "Create Schema Element");
        HasDataParameterText("019c240d-19d8-77db-a925-13311a12f816", TextConstants.HeaderFormCreateEntitySchemaParameterStyle, "Create Schema Style Parameter");
        HasDataParameterText("019c240d-19d8-761c-804b-b56d211c5c43", TextConstants.HeaderFormCreateEntitySchemaParameterSystem, "Create Schema System Parameter");
        HasDataParameterText("019c240d-19d8-79bf-af26-543a2ae1fcdf", TextConstants.HeaderFormCreateEntitySchemaParameterText, "Create Schema Text Parameter");
        HasDataParameterText("019c240d-19d8-75f8-99cb-285ba1b2ecd8", TextConstants.HeaderFormCreateEntitySchemaPropertyBoolean, "Create Schema Boolean Property");
        HasDataParameterText("019c240d-19d8-7cb3-b84c-981fd2862f0e", TextConstants.HeaderFormCreateEntitySchemaPropertyCollection, "Create Schema Collection Property");
        HasDataParameterText("019c240d-19d8-736f-87d7-71972f4848b0", TextConstants.HeaderFormCreateEntitySchemaPropertyDateTime, "Create Schema DateTime Property");
        HasDataParameterText("019c240d-19d8-727f-9d4a-b6a92f70bfd9", TextConstants.HeaderFormCreateEntitySchemaPropertyDecimal, "Create Schema Decimal Property");
        HasDataParameterText("019c240d-19d8-7789-b384-b32c673d1f98", TextConstants.HeaderFormCreateEntitySchemaPropertyImage, "Create Schema Image Property");
        HasDataParameterText("019c240d-19d8-7b18-8700-7933cc2e5ac7", TextConstants.HeaderFormCreateEntitySchemaPropertyInteger, "Create Schema Integer Property");
        HasDataParameterText("019c240d-19d8-7f86-b88c-db22de407519", TextConstants.HeaderFormCreateEntitySchemaPropertyString, "Create Schema String Property");
        HasDataParameterText("019c240d-19d8-7b93-9f90-de94403d000a", TextConstants.HeaderFormCreateEntitySchemaPropertyTable, "Create Schema Table Property");

        HasSchemaParameterText("019c240d-6e22-77a8-99d9-76c997bc7eff", TextConstants.HeaderFormUpdateEntitySchema);
        HasSchemaParameterText("019c240d-6e22-70f5-bb78-f1017feca103", TextConstants.HeaderFormUpdateEntitySchemaContext);
        HasSchemaParameterText("019c240d-6e22-7bf6-9778-7d42751cc301", TextConstants.HeaderFormUpdateEntitySchemaElement);
        HasSchemaParameterText("019c240d-6e22-78af-affd-196b637a0dfc", TextConstants.HeaderFormUpdateEntitySchemaParameterStyle);
        HasSchemaParameterText("019c240d-6e22-7f13-91f7-40b557df76e5", TextConstants.HeaderFormUpdateEntitySchemaParameterSystem);
        HasSchemaParameterText("019c240d-6e22-7fed-b8e1-87cca3ee930d", TextConstants.HeaderFormUpdateEntitySchemaParameterText);
        HasSchemaParameterText("019c240d-6e22-7913-9fae-b135cf903053", TextConstants.HeaderFormUpdateEntitySchemaPropertyBoolean);
        HasSchemaParameterText("019c240d-6e22-7942-adec-6a8773092b8b", TextConstants.HeaderFormUpdateEntitySchemaPropertyCollection);
        HasSchemaParameterText("019c240d-6e22-71fa-a972-a870f1167975", TextConstants.HeaderFormUpdateEntitySchemaPropertyDateTime);
        HasSchemaParameterText("019c240d-6e22-7df2-9ad3-1f4337f1f415", TextConstants.HeaderFormUpdateEntitySchemaPropertyDecimal);
        HasSchemaParameterText("019c240d-6e22-7e3a-8edf-09814b4b8a57", TextConstants.HeaderFormUpdateEntitySchemaPropertyImage);
        HasSchemaParameterText("019c240d-6e22-7532-a326-85012b6af7be", TextConstants.HeaderFormUpdateEntitySchemaPropertyInteger);
        HasSchemaParameterText("019c240d-6e22-7b1e-b540-fe8a2aac155f", TextConstants.HeaderFormUpdateEntitySchemaPropertyString);
        HasSchemaParameterText("019c240d-6e22-77d3-a36c-bbff9662a8e1", TextConstants.HeaderFormUpdateEntitySchemaPropertyTable);

        HasDataParameterText("019c240d-8b86-7c30-8564-1242f5f9abaa", TextConstants.HeaderFormUpdateEntitySchema, "Update Schema");
        HasDataParameterText("019c240d-8b86-7303-b114-c733077eb990", TextConstants.HeaderFormUpdateEntitySchemaContext, "Update Schema Context");
        HasDataParameterText("019c240d-8b86-7a12-bcd4-5316fd6894b0", TextConstants.HeaderFormUpdateEntitySchemaElement, "Update Schema Element");
        HasDataParameterText("019c240d-8b86-7b6f-8892-d4ddd7793234", TextConstants.HeaderFormUpdateEntitySchemaParameterStyle, "Update Schema Style Parameter");
        HasDataParameterText("019c240d-8b86-79f5-9787-1aeb9cde12b1", TextConstants.HeaderFormUpdateEntitySchemaParameterSystem, "Update Schema System Parameter");
        HasDataParameterText("019c240d-8b86-7eb2-bf9e-fa45f0051326", TextConstants.HeaderFormUpdateEntitySchemaParameterText, "Update Schema Text Parameter");
        HasDataParameterText("019c240d-8b86-764f-86ad-1e8c2203acee", TextConstants.HeaderFormUpdateEntitySchemaPropertyBoolean, "Update Schema Boolean Property");
        HasDataParameterText("019c240d-8b86-794e-a648-3da4fd31dc9b", TextConstants.HeaderFormUpdateEntitySchemaPropertyCollection, "Update Schema Collection Property");
        HasDataParameterText("019c240d-8b86-7b8a-9c71-0d9ae20a2a50", TextConstants.HeaderFormUpdateEntitySchemaPropertyDateTime, "Update Schema DateTime Property");
        HasDataParameterText("019c240d-8b86-7b31-98f5-60c0c15ecb49", TextConstants.HeaderFormUpdateEntitySchemaPropertyDecimal, "Update Schema Decimal Property");
        HasDataParameterText("019c240d-8b86-7d81-bfbc-9f8384ab8a2d", TextConstants.HeaderFormUpdateEntitySchemaPropertyImage, "Update Schema Image Property");
        HasDataParameterText("019c240d-8b86-79e9-af55-66544574de3a", TextConstants.HeaderFormUpdateEntitySchemaPropertyInteger, "Update Schema Integer Property");
        HasDataParameterText("019c240d-8b86-7197-954e-c80cb43b9192", TextConstants.HeaderFormUpdateEntitySchemaPropertyString, "Update Schema String Property");
        HasDataParameterText("019c240d-8b86-78a6-942c-7210fa5d6003", TextConstants.HeaderFormUpdateEntitySchemaPropertyTable, "Update Schema Table Property");
    }
}
