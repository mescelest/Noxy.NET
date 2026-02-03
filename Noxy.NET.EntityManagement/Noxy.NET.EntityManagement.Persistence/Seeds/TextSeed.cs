using Microsoft.EntityFrameworkCore;
using Noxy.NET.EntityManagement.Domain.Constants;
using Noxy.NET.EntityManagement.Persistence.Abstractions;
using Noxy.NET.EntityManagement.Persistence.Tables.Schemas;

namespace Noxy.NET.EntityManagement.Persistence.Seeds;

public class TextSeed(ModelBuilder builder, TableSchema refSchema) : BaseSeed(builder, refSchema)
{
    public void Apply()
    {
        HasSchemaParameterText("019789fc-3929-75a9-99e9-11f9d8b81e8a", TextConstants.DefaultEmptyValue);

        HasDataParameterText("019789fc-3929-75a9-99e9-142d13f18fb0", TextConstants.DefaultEmptyValue, "-");

        HasSchemaParameterText("019764ca-25c4-7785-bd02-daa168ae477d", TextConstants.ButtonActivate);
        HasSchemaParameterText("019764ca-25c4-7785-bd02-de67a804e592", TextConstants.ButtonCreate);
        HasSchemaParameterText("019764ca-25c4-7785-bd02-e26550fa3aa9", TextConstants.ButtonUpdate);
        HasSchemaParameterText("019764ca-25c4-7785-bd02-e5902e766443", TextConstants.ButtonSubmit);
        HasSchemaParameterText("01977fb8-8d9e-7179-b098-d8800c456351", TextConstants.ButtonSignIn);
        HasSchemaParameterText("01977fb8-8d9e-7179-b098-dde7fae42dc4", TextConstants.ButtonSignUp);

        HasDataParameterText("019764ca-25c4-7785-bd02-ebdc5a27fb39", TextConstants.ButtonActivate, "Activate");
        HasDataParameterText("019764ca-25c4-7785-bd02-efa276b57b62", TextConstants.ButtonCreate, "Create");
        HasDataParameterText("019764ca-25c4-7785-bd02-f1e439a3bb07", TextConstants.ButtonUpdate, "Update");
        HasDataParameterText("019764ca-25c4-7785-bd02-f7c5260cb82d", TextConstants.ButtonSubmit, "Submit");
        HasDataParameterText("01977fb8-8d9e-7179-b098-d37940c4d817", TextConstants.ButtonSignIn, "Sign in");
        HasDataParameterText("01977fb8-8d9e-7179-b098-d431e095ba95", TextConstants.ButtonSignUp, "Sign up");

        HasSchemaParameterText("01978309-3029-74e9-931c-3cf1322948fd", TextConstants.LinkNavigationSchema);

        HasDataParameterText("01978309-3029-74e9-931c-436de21f95b0", TextConstants.LinkNavigationSchema, "Schemas");

        HasSchemaParameterText("019c254b-1b85-7dca-abe2-d2a11538b7ed", TextConstants.HeaderSchema);
        HasSchemaParameterText("019c254b-1b85-71dc-8f4a-4295bd279c0a", TextConstants.HeaderContext);
        HasSchemaParameterText("019c254b-1b85-772d-aaf0-dff4ea453c6c", TextConstants.HeaderElement);
        HasSchemaParameterText("019c254b-1b85-7ec8-9720-24a5bcaf1888", TextConstants.HeaderParameter);
        HasSchemaParameterText("019c254b-1b85-7b4f-bd18-355a9af4b607", TextConstants.HeaderProperty);

        HasDataParameterText("019c254a-c196-7c51-a3c1-82f826185ea8", TextConstants.HeaderSchema, "Schemas");
        HasDataParameterText("019c254a-c196-7595-b493-5ccf1ea9faad", TextConstants.HeaderContext, "Contexts");
        HasDataParameterText("019c254a-c196-7968-8109-da3f68bd324f", TextConstants.HeaderElement, "Elements");
        HasDataParameterText("019c254a-c196-7e7d-98bb-36fd8384fb8f", TextConstants.HeaderParameter, "Parameters");
        HasDataParameterText("019c254a-c196-734d-aeaa-3f93015db3bc", TextConstants.HeaderProperty, "Properties");

        HasSchemaParameterText("019789de-e449-71aa-ab1d-0992c66a9dae", TextConstants.LabelFormSchemaIdentifier);
        HasSchemaParameterText("019789de-e449-71aa-ab1d-12e8b546b239", TextConstants.LabelFormName);
        HasSchemaParameterText("019789de-e449-71aa-ab1d-17209b94ded6", TextConstants.LabelFormNote);
        HasSchemaParameterText("019789de-e449-71aa-ab1d-192c4a3c0eb6", TextConstants.LabelFormTitle);
        HasSchemaParameterText("019789de-e449-71aa-ab1d-1c1707f61f89", TextConstants.LabelFormDescription);
        HasSchemaParameterText("019789de-e449-71aa-ab1d-205e04219122", TextConstants.LabelFormOrder);
        HasSchemaParameterText("01978a17-7901-7131-8b49-005b042a1608", TextConstants.LabelFormIsApprovalRequired);
        HasSchemaParameterText("01979d14-54ad-72f9-b5d8-a5aa1089fe1b", TextConstants.LabelFormPropertyTypeList);
        HasSchemaParameterText("01978a1f-f0a2-731f-b17d-14f803055489", TextConstants.LabelFormIsValueList);
        HasSchemaParameterText("019799a3-a72f-725a-b4f6-b7144f565f3f", TextConstants.LabelFormBoolean);
        HasSchemaParameterText("019799a3-a72f-725a-b4f6-b8b9e65fa2e9", TextConstants.LabelFormDateTime);
        HasSchemaParameterText("019799a3-a72f-725a-b4f6-be98a390cf1a", TextConstants.LabelFormDecimal);
        HasSchemaParameterText("019799a3-a72f-725a-b4f6-c0efbd9b5dbb", TextConstants.LabelFormInteger);
        HasSchemaParameterText("019799a3-a72f-725a-b4f6-c449cc81422f", TextConstants.LabelFormString);
        HasSchemaParameterText("019799a3-a72f-725a-b4f6-cd308b80c164", TextConstants.LabelFormParameterStyle);
        HasSchemaParameterText("019799a3-a72f-725a-b4f6-d3344af0a274", TextConstants.LabelFormParameterSystem);
        HasSchemaParameterText("019799a3-a72f-725a-b4f6-d6b06a4f6d1b", TextConstants.LabelFormParameterText);

        HasDataParameterText("019789f9-2601-72ac-ad27-ea4b8f4855d6", TextConstants.LabelFormSchemaIdentifier, "Schema identifier");
        HasDataParameterText("019789f9-2601-72ac-ad27-f1f7ad078b01", TextConstants.LabelFormName, "Name");
        HasDataParameterText("019789f9-2601-72ac-ad27-f7c76f0002d4", TextConstants.LabelFormNote, "Note");
        HasDataParameterText("019789f9-2601-72ac-ad27-f9ef87027fec", TextConstants.LabelFormTitle, "Title");
        HasDataParameterText("019789f9-2601-72ac-ad27-fceadc8f7eda", TextConstants.LabelFormDescription, "Description");
        HasDataParameterText("019789f9-2601-72ac-ad28-0204b7c6d497", TextConstants.LabelFormOrder, "Order");
        HasDataParameterText("01978a17-7901-7131-8b49-1200bad01c81", TextConstants.LabelFormIsApprovalRequired, "Is approval required?");
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

        HasSchemaParameterText("019789fc-3929-75a9-99e8-f7a6abf0c139", TextConstants.HelpFormSchemaIdentifier);
        HasSchemaParameterText("019789fc-3929-75a9-99e8-f819edf63934", TextConstants.HelpFormName);
        HasSchemaParameterText("019789fc-3929-75a9-99e8-ffd616e87b30", TextConstants.HelpFormNote);
        HasSchemaParameterText("019789fc-3929-75a9-99e9-0229499f0ddd", TextConstants.HelpFormTitle);
        HasSchemaParameterText("019789fc-3929-75a9-99e9-046632ba51b7", TextConstants.HelpFormDescription);
        HasSchemaParameterText("019789fc-3929-75a9-99e9-0a5f8ca00604", TextConstants.HelpFormOrder);
        HasSchemaParameterText("01978a17-b7b1-772f-a129-543b087e1606", TextConstants.HelpFormIsApprovalRequired);
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

        HasDataParameterText("019789fc-18dc-73ee-94b6-1564b2f72eb7", TextConstants.HelpFormSchemaIdentifier, "The unique, humanly readable identifier for this entity type.");
        HasDataParameterText("019789fc-18dc-73ee-94b6-1c60e96f26a9", TextConstants.HelpFormName, "The internal name of this entity type. Should only be visible in the configuration.");
        HasDataParameterText("019789fc-18dc-73ee-94b6-21b290f8401d", TextConstants.HelpFormNote, "A short note detailing how this entity type is used. Should only be visible in the configuration.");
        HasDataParameterText("019789fc-18dc-73ee-94b6-26ead5f18d4d", TextConstants.HelpFormTitle, "The title used when displaying an entity of this type.");
        HasDataParameterText("019789fc-18dc-73ee-94b6-28dc0edf9b69", TextConstants.HelpFormDescription, "The description used when displaying an entity of this type.");
        HasDataParameterText("019789fc-18dc-73ee-94b6-2ee0b27366dc", TextConstants.HelpFormOrder, "The order in which this entity type is sorted.");
        HasDataParameterText("01978a17-b7b1-772f-a129-66dba634b0b5", TextConstants.HelpFormIsApprovalRequired, "Determines if another user must approve a text parameter value before it becomes active.");
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
