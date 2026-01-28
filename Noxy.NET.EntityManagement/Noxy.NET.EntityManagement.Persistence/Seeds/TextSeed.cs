using Microsoft.EntityFrameworkCore;
using Noxy.NET.EntityManagement.Domain.Constants;
using Noxy.NET.EntityManagement.Persistence.Abstractions;
using Noxy.NET.EntityManagement.Persistence.Tables.Schemas;

namespace Noxy.NET.EntityManagement.Persistence.Seeds;

public class TextSeed(ModelBuilder builder, TableSchema refSchema) : BaseSeed(builder, refSchema)
{
    public void Apply()
    {
        HasSchemaDynamicValueTextParameter("019789fc-3929-75a9-99e9-11f9d8b81e8a", TextConstants.DefaultEmptyValue);

        HasTableDataTextParameter("019789fc-3929-75a9-99e9-142d13f18fb0", TextConstants.DefaultEmptyValue, "-");

        HasSchemaDynamicValueTextParameter("019764ca-25c4-7785-bd02-daa168ae477d", TextConstants.ButtonActivate);
        HasSchemaDynamicValueTextParameter("019764ca-25c4-7785-bd02-de67a804e592", TextConstants.ButtonCreate);
        HasSchemaDynamicValueTextParameter("019764ca-25c4-7785-bd02-e26550fa3aa9", TextConstants.ButtonUpdate);
        HasSchemaDynamicValueTextParameter("019764ca-25c4-7785-bd02-e5902e766443", TextConstants.ButtonSubmit);
        HasSchemaDynamicValueTextParameter("01977fb8-8d9e-7179-b098-d8800c456351", TextConstants.ButtonSignIn);
        HasSchemaDynamicValueTextParameter("01977fb8-8d9e-7179-b098-dde7fae42dc4", TextConstants.ButtonSignUp);

        HasTableDataTextParameter("019764ca-25c4-7785-bd02-ebdc5a27fb39", TextConstants.ButtonActivate, "Activate");
        HasTableDataTextParameter("019764ca-25c4-7785-bd02-efa276b57b62", TextConstants.ButtonCreate, "Create");
        HasTableDataTextParameter("019764ca-25c4-7785-bd02-f1e439a3bb07", TextConstants.ButtonUpdate, "Update");
        HasTableDataTextParameter("019764ca-25c4-7785-bd02-f7c5260cb82d", TextConstants.ButtonSubmit, "Submit");
        HasTableDataTextParameter("01977fb8-8d9e-7179-b098-d37940c4d817", TextConstants.ButtonSignIn, "Sign in");
        HasTableDataTextParameter("01977fb8-8d9e-7179-b098-d431e095ba95", TextConstants.ButtonSignUp, "Sign up");

        HasSchemaDynamicValueTextParameter("01978309-3029-74e9-931c-3cf1322948fd", TextConstants.LinkNavigationSchema);

        HasTableDataTextParameter("01978309-3029-74e9-931c-436de21f95b0", TextConstants.LinkNavigationSchema, "Schemas");

        HasSchemaDynamicValueTextParameter("019789de-e449-71aa-ab1d-0992c66a9dae", TextConstants.LabelFormSchemaIdentifier);
        HasSchemaDynamicValueTextParameter("019789de-e449-71aa-ab1d-12e8b546b239", TextConstants.LabelFormName);
        HasSchemaDynamicValueTextParameter("019789de-e449-71aa-ab1d-17209b94ded6", TextConstants.LabelFormNote);
        HasSchemaDynamicValueTextParameter("019789de-e449-71aa-ab1d-192c4a3c0eb6", TextConstants.LabelFormTitle);
        HasSchemaDynamicValueTextParameter("019789de-e449-71aa-ab1d-1c1707f61f89", TextConstants.LabelFormDescription);
        HasSchemaDynamicValueTextParameter("019789de-e449-71aa-ab1d-205e04219122", TextConstants.LabelFormOrder);
        HasSchemaDynamicValueTextParameter("019789de-e449-71aa-ab1d-0e7af6463e30", TextConstants.LabelFormInputID);
        HasSchemaDynamicValueTextParameter("01978a17-7901-7131-8b48-f882aa64e37a", TextConstants.LabelFormCodeSnippet);
        HasSchemaDynamicValueTextParameter("01978a17-7901-7131-8b48-ff6a41005bb3", TextConstants.LabelFormDefaultValue);
        HasSchemaDynamicValueTextParameter("01978a17-7901-7131-8b49-005b042a1608", TextConstants.LabelFormIsApprovalRequired);
        HasSchemaDynamicValueTextParameter("01978a17-7901-7131-8b49-07a43e807dfd", TextConstants.LabelFormIsAsynchronous);
        HasSchemaDynamicValueTextParameter("01978a1f-f0a2-731f-b17d-0991eba49899", TextConstants.LabelFormIsRepeatable);
        HasSchemaDynamicValueTextParameter("01978a1f-f0a2-731f-b17d-0e2bbf4c0700", TextConstants.LabelFormAttributeType);
        HasSchemaDynamicValueTextParameter("01978a1f-f0a2-731f-b17d-12579bcc7198", TextConstants.LabelFormTextParameterType);
        HasSchemaDynamicValueTextParameter("01979d14-54ad-72f9-b5d8-a291a3ca06d4", TextConstants.LabelFormDynamicValueTypeList);
        HasSchemaDynamicValueTextParameter("01979d14-54ad-72f9-b5d8-a5aa1089fe1b", TextConstants.LabelFormPropertyTypeList);
        HasSchemaDynamicValueTextParameter("01978a1f-f0a2-731f-b17d-14f803055489", TextConstants.LabelFormIsValueList);
        HasSchemaDynamicValueTextParameter("019799a3-a72f-725a-b4f6-b7144f565f3f", TextConstants.LabelFormBoolean);
        HasSchemaDynamicValueTextParameter("019799a3-a72f-725a-b4f6-b8b9e65fa2e9", TextConstants.LabelFormDateTime);
        HasSchemaDynamicValueTextParameter("019799a3-a72f-725a-b4f6-be98a390cf1a", TextConstants.LabelFormDecimal);
        HasSchemaDynamicValueTextParameter("019799a3-a72f-725a-b4f6-c0efbd9b5dbb", TextConstants.LabelFormInteger);
        HasSchemaDynamicValueTextParameter("019799a3-a72f-725a-b4f6-c449cc81422f", TextConstants.LabelFormString);
        HasSchemaDynamicValueTextParameter("019799a3-a72f-725a-b4f6-c9599b28ac9c", TextConstants.LabelFormDynamicValueCode);
        HasSchemaDynamicValueTextParameter("019799a3-a72f-725a-b4f6-cd308b80c164", TextConstants.LabelFormDynamicValueStyleParameter);
        HasSchemaDynamicValueTextParameter("019799a3-a72f-725a-b4f6-d3344af0a274", TextConstants.LabelFormDynamicValueSystemParameter);
        HasSchemaDynamicValueTextParameter("019799a3-a72f-725a-b4f6-d6b06a4f6d1b", TextConstants.LabelFormDynamicValueTextParameter);

        HasTableDataTextParameter("019789f9-2601-72ac-ad27-ea4b8f4855d6", TextConstants.LabelFormSchemaIdentifier, "Schema identifier");
        HasTableDataTextParameter("019789f9-2601-72ac-ad27-f1f7ad078b01", TextConstants.LabelFormName, "Name");
        HasTableDataTextParameter("019789f9-2601-72ac-ad27-f7c76f0002d4", TextConstants.LabelFormNote, "Note");
        HasTableDataTextParameter("019789f9-2601-72ac-ad27-f9ef87027fec", TextConstants.LabelFormTitle, "Title");
        HasTableDataTextParameter("019789f9-2601-72ac-ad27-fceadc8f7eda", TextConstants.LabelFormDescription, "Description");
        HasTableDataTextParameter("019789f9-2601-72ac-ad28-0204b7c6d497", TextConstants.LabelFormOrder, "Order");
        HasTableDataTextParameter("019789f9-2601-72ac-ad27-ed762c6df40e", TextConstants.LabelFormInputID, "Input type");
        HasTableDataTextParameter("01978a17-7901-7131-8b49-0a2832bb83bd", TextConstants.LabelFormCodeSnippet, "Value");
        HasTableDataTextParameter("01978a17-7901-7131-8b49-0cb8c0a70a25", TextConstants.LabelFormDefaultValue, "Default value");
        HasTableDataTextParameter("01978a17-7901-7131-8b49-1200bad01c81", TextConstants.LabelFormIsApprovalRequired, "Is approval required?");
        HasTableDataTextParameter("01978a17-7901-7131-8b49-179f268688f2", TextConstants.LabelFormIsAsynchronous, "Is asynchronous?");
        HasTableDataTextParameter("01978a1f-f0a2-731f-b17d-1981b375a5db", TextConstants.LabelFormIsRepeatable, "Is repeatable?");
        HasTableDataTextParameter("01978a1f-f0a2-731f-b17d-1e57e7540e08", TextConstants.LabelFormAttributeType, "Attribute type");
        HasTableDataTextParameter("01978a1f-f0a2-731f-b17d-2174a6ecd4fe", TextConstants.LabelFormTextParameterType, "Text parameter type");
        HasTableDataTextParameter("01979d14-54ad-72f9-b5d8-aa11a0e1ce60", TextConstants.LabelFormDynamicValueTypeList, "Choose dynamic value type");
        HasTableDataTextParameter("01979d14-54ad-72f9-b5d8-ae413650b40c", TextConstants.LabelFormPropertyTypeList, "Choose property type");
        HasTableDataTextParameter("019799a4-1a2b-7368-9b33-6a3f0bf60dae", TextConstants.LabelFormIsValueList, "Is value list?");
        HasTableDataTextParameter("019799a4-1a2b-7368-9b33-6c45805d80a3", TextConstants.LabelFormBoolean, "Boolean");
        HasTableDataTextParameter("019799a4-1a2b-7368-9b33-72817ce487a1", TextConstants.LabelFormDateTime, "DateTime");
        HasTableDataTextParameter("019799a4-1a2b-7368-9b33-77fdd478928f", TextConstants.LabelFormDecimal, "Decimal");
        HasTableDataTextParameter("019799a4-1a2b-7368-9b33-781625aca070", TextConstants.LabelFormInteger, "Integer");
        HasTableDataTextParameter("019799a4-1a2b-7368-9b33-7c0e241b9622", TextConstants.LabelFormString, "String");
        HasTableDataTextParameter("019799a4-1a2b-7368-9b33-809a8b75e571", TextConstants.LabelFormDynamicValueCode, "Code");
        HasTableDataTextParameter("019799a4-1a2b-7368-9b33-86038825b246", TextConstants.LabelFormDynamicValueStyleParameter, "Style parameter");
        HasTableDataTextParameter("019799a4-1a2b-7368-9b33-8bd3f0f5e948", TextConstants.LabelFormDynamicValueSystemParameter, "System parameter");
        HasTableDataTextParameter("019799a4-1a2b-7368-9b33-8c2e6b4a0b6d", TextConstants.LabelFormDynamicValueTextParameter, "Text parameter");

        HasSchemaDynamicValueTextParameter("019789fc-3929-75a9-99e8-f7a6abf0c139", TextConstants.HelpFormSchemaIdentifier);
        HasSchemaDynamicValueTextParameter("019789fc-3929-75a9-99e8-f819edf63934", TextConstants.HelpFormName);
        HasSchemaDynamicValueTextParameter("019789fc-3929-75a9-99e8-ffd616e87b30", TextConstants.HelpFormNote);
        HasSchemaDynamicValueTextParameter("019789fc-3929-75a9-99e9-0229499f0ddd", TextConstants.HelpFormTitle);
        HasSchemaDynamicValueTextParameter("019789fc-3929-75a9-99e9-046632ba51b7", TextConstants.HelpFormDescription);
        HasSchemaDynamicValueTextParameter("019789fc-3929-75a9-99e9-0a5f8ca00604", TextConstants.HelpFormOrder);
        HasSchemaDynamicValueTextParameter("019789fc-3929-75a9-99e9-0c9241c61162", TextConstants.HelpFormInputID);
        HasSchemaDynamicValueTextParameter("01978a17-b7b1-772f-a129-4ce642008489", TextConstants.HelpFormCodeSnippet);
        HasSchemaDynamicValueTextParameter("01978a17-b7b1-772f-a129-52ee6b9269cd", TextConstants.HelpFormDefaultValue);
        HasSchemaDynamicValueTextParameter("01978a17-b7b1-772f-a129-543b087e1606", TextConstants.HelpFormIsApprovalRequired);
        HasSchemaDynamicValueTextParameter("01978a17-b7b1-772f-a129-5bbae750bee1", TextConstants.HelpFormIsAsynchronous);
        HasSchemaDynamicValueTextParameter("01978a20-9692-72ff-be7d-9cb3d3d46359", TextConstants.HelpFormIsRepeatable);
        HasSchemaDynamicValueTextParameter("01978a20-9692-72ff-be7d-a3b72d338572", TextConstants.HelpFormAttributeType);
        HasSchemaDynamicValueTextParameter("01979d14-54ad-72f9-b5d8-b33557c5a806", TextConstants.HelpFormTextParameterType);
        HasSchemaDynamicValueTextParameter("01979d14-54ad-72f9-b5d8-c3a68639a28a", TextConstants.HelpFormDynamicValueTypeList);
        HasSchemaDynamicValueTextParameter("01979d14-54ad-72f9-b5d8-c4050ea1f0fd", TextConstants.HelpFormPropertyTypeList);
        HasSchemaDynamicValueTextParameter("01978a20-9692-72ff-be7d-a9bb3d99565a", TextConstants.HelpFormIsValueList);
        HasSchemaDynamicValueTextParameter("019799a6-8dc0-75af-8866-97a26b3415dc", TextConstants.HelpFormBoolean);
        HasSchemaDynamicValueTextParameter("019799a6-8dc0-75af-8866-986c4570a7bd", TextConstants.HelpFormDateTime);
        HasSchemaDynamicValueTextParameter("019799a6-8dc0-75af-8866-9f89357ebb7f", TextConstants.HelpFormDecimal);
        HasSchemaDynamicValueTextParameter("019799a6-8dc0-75af-8866-a390807971d2", TextConstants.HelpFormInteger);
        HasSchemaDynamicValueTextParameter("019799a6-8dc0-75af-8866-a44ca62f2e18", TextConstants.HelpFormString);
        HasSchemaDynamicValueTextParameter("019799a6-8dc0-75af-8866-a93e37c915a8", TextConstants.HelpFormDynamicValueCode);
        HasSchemaDynamicValueTextParameter("019799a6-8dc0-75af-8866-ad7a01af4eac", TextConstants.HelpFormDynamicValueStyleParameter);
        HasSchemaDynamicValueTextParameter("019799a6-8dc0-75af-8866-b0be19e7dcdd", TextConstants.HelpFormDynamicValueSystemParameter);
        HasSchemaDynamicValueTextParameter("019799a6-8dc0-75af-8866-b4f08d0df491", TextConstants.HelpFormDynamicValueTextParameter);

        HasTableDataTextParameter("019789fc-18dc-73ee-94b6-1564b2f72eb7", TextConstants.HelpFormSchemaIdentifier, "The unique, humanly readable identifier for this entity type.");
        HasTableDataTextParameter("019789fc-18dc-73ee-94b6-18a6e3314ccd", TextConstants.HelpFormInputID, "The type of input this entity is related with.");
        HasTableDataTextParameter("019789fc-18dc-73ee-94b6-1c60e96f26a9", TextConstants.HelpFormName, "The internal name of this entity type. Should only be visible in the configuration.");
        HasTableDataTextParameter("019789fc-18dc-73ee-94b6-21b290f8401d", TextConstants.HelpFormNote, "A short note detailing how this entity type is used. Should only be visible in the configuration.");
        HasTableDataTextParameter("019789fc-18dc-73ee-94b6-26ead5f18d4d", TextConstants.HelpFormTitle, "The title used when displaying an entity of this type.");
        HasTableDataTextParameter("019789fc-18dc-73ee-94b6-28dc0edf9b69", TextConstants.HelpFormDescription, "The description used when displaying an entity of this type.");
        HasTableDataTextParameter("019789fc-18dc-73ee-94b6-2ee0b27366dc", TextConstants.HelpFormOrder, "The order in which this entity type is sorted.");
        HasTableDataTextParameter("01978a17-b7b1-772f-a129-5fe41a2f1676", TextConstants.HelpFormCodeSnippet, "The code snippet to be run. Must return a value.");
        HasTableDataTextParameter("01978a17-b7b1-772f-a129-6253fc96820a", TextConstants.HelpFormDefaultValue, "The default value that will be used for this entity type.");
        HasTableDataTextParameter("01978a17-b7b1-772f-a129-66dba634b0b5", TextConstants.HelpFormIsApprovalRequired, "Determines if another user must approve a text parameter value before it becomes active.");
        HasTableDataTextParameter("01978a17-b7b1-772f-a129-68c0836a0759", TextConstants.HelpFormIsAsynchronous, "Determines if the method will be run as asynchronous code.");
        HasTableDataTextParameter("01978a20-9692-72ff-be7d-ac500344bf4b", TextConstants.HelpFormIsRepeatable, "Determines if this step can be completed with multiple results.");
        HasTableDataTextParameter("01978a20-9692-72ff-be7d-b08279f62f4a", TextConstants.HelpFormAttributeType, "The type of attribute.");
        HasTableDataTextParameter("01978a20-9692-72ff-be7d-b59a17facafb", TextConstants.HelpFormTextParameterType, "The type of text parameter.");
        HasTableDataTextParameter("01979d14-54ad-72f9-b5d8-b830dc26f9f4", TextConstants.HelpFormDynamicValueTypeList, "");
        HasTableDataTextParameter("01979d14-54ad-72f9-b5d8-bf20682e54af", TextConstants.HelpFormPropertyTypeList, "");
        HasTableDataTextParameter("01978a20-9692-72ff-be7d-ba24274b04c3", TextConstants.HelpFormIsValueList, "Determines if this input attribute can be configured with a list of values.");
        HasTableDataTextParameter("019799a6-6b05-7029-b9a7-2d147f778de8", TextConstants.HelpFormBoolean, "Represents a boolean value.");
        HasTableDataTextParameter("019799a6-6b05-7029-b9a7-300f0306a227", TextConstants.HelpFormDateTime, "Represents a datetime value.");
        HasTableDataTextParameter("019799a6-6b05-7029-b9a7-35f76c49841a", TextConstants.HelpFormDecimal, "Represents a decimal value.");
        HasTableDataTextParameter("019799a6-6b05-7029-b9a7-3a924229a88a", TextConstants.HelpFormInteger, "Represents a integer value.");
        HasTableDataTextParameter("019799a6-6b05-7029-b9a7-3ca405bc7246", TextConstants.HelpFormString, "Represents a string value.");
        HasTableDataTextParameter("019799a6-6b05-7029-b9a7-40860609aed2", TextConstants.HelpFormDynamicValueCode, "Represents a dynamic code value.");
        HasTableDataTextParameter("019799a6-6b05-7029-b9a7-473fbbc23ec3", TextConstants.HelpFormDynamicValueStyleParameter, "Represents a dynamic style parameter value.");
        HasTableDataTextParameter("019799a6-6b05-7029-b9a7-48183a3903a4", TextConstants.HelpFormDynamicValueSystemParameter, "Represents a dynamic system parameter value.");
        HasTableDataTextParameter("019799a6-6b05-7029-b9a7-4c3fb79cf9d1", TextConstants.HelpFormDynamicValueTextParameter, "Represents a dynamic text parameter value.");
    }
}
