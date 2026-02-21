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

        HasSchemaParameterText("019c2571-7b67-75bb-a029-42c44fbfd1fb", TextConstants.Title);
        HasDataParameterText("019c2571-7b67-75bb-a029-45530805ee07", TextConstants.Title, "Noxy.NET");

        #endregion Baseline

        #region DefaultEmpty

        HasSchemaParameterText("019789fc-3929-75a9-99e9-11f9d8b81e8a", TextConstants.DefaultEmptyValue);
        HasDataParameterText("019789fc-3929-75a9-99e9-142d13f18fb0", TextConstants.DefaultEmptyValue, "-");

        HasSchemaParameterText("019c2584-67dc-749e-971e-580816b3b821", TextConstants.DefaultEmptyList);
        HasDataParameterText("019c2584-67dc-749e-971e-5e6410aeeacd", TextConstants.DefaultEmptyList, "Nothing here yet...");

        #endregion DefaultEmpty

        #region Button

        HasSchemaParameterText("019764ca-25c4-7785-bd02-daa168ae477d", TextConstants.ButtonActivate);
        HasDataParameterText("019764ca-25c4-7785-bd02-ebdc5a27fb39", TextConstants.ButtonActivate, "Activate");

        HasSchemaParameterText("019c29f9-5de4-760e-90b6-c0b6d174ed8f", TextConstants.ButtonSearch);
        HasDataParameterText("019c29f9-5de4-760e-90b6-caaaa39d8d0d", TextConstants.ButtonSearch, "Search");

        HasSchemaParameterText("019c29f9-5de4-760e-90b6-c5ba80aa9a14", TextConstants.ButtonFilter);
        HasDataParameterText("019c29f9-5de4-760e-90b6-cd1d017360d4", TextConstants.ButtonFilter, "Filter");

        HasSchemaParameterText("019c29f9-5de4-760e-90b6-d26d9226e044", TextConstants.ButtonReset);
        HasDataParameterText("019c29f9-5de4-760e-90b6-d5266cbcd3fd", TextConstants.ButtonReset, "Reset");

        HasSchemaParameterText("019764ca-25c4-7785-bd02-de67a804e592", TextConstants.ButtonCreate);
        HasDataParameterText("019764ca-25c4-7785-bd02-efa276b57b62", TextConstants.ButtonCreate, "Create");

        HasSchemaParameterText("019764ca-25c4-7785-bd02-e26550fa3aa9", TextConstants.ButtonUpdate);
        HasDataParameterText("019764ca-25c4-7785-bd02-f1e439a3bb07", TextConstants.ButtonUpdate, "Update");

        HasSchemaParameterText("019764ca-25c4-7785-bd02-e5902e766443", TextConstants.ButtonSubmit);
        HasDataParameterText("019764ca-25c4-7785-bd02-f7c5260cb82d", TextConstants.ButtonSubmit, "Submit");

        HasSchemaParameterText("01977fb8-8d9e-7179-b098-d8800c456351", TextConstants.ButtonSignIn);
        HasDataParameterText("01977fb8-8d9e-7179-b098-d37940c4d817", TextConstants.ButtonSignIn, "Sign in");

        HasSchemaParameterText("01977fb8-8d9e-7179-b098-dde7fae42dc4", TextConstants.ButtonSignUp);
        HasDataParameterText("01977fb8-8d9e-7179-b098-d431e095ba95", TextConstants.ButtonSignUp, "Sign up");

        HasSchemaParameterText("019c2571-7b67-75bb-a029-3ad9550d778d", TextConstants.ButtonSignOut);
        HasDataParameterText("019c2571-7b67-75bb-a029-3eea76c0f0c3", TextConstants.ButtonSignOut, "Sign out");

        #endregion Button

        #region Link

        HasSchemaParameterText("01978309-3029-74e9-931c-3cf1322948fd", TextConstants.LinkNavigationSchema);
        HasDataParameterText("01978309-3029-74e9-931c-436de21f95b0", TextConstants.LinkNavigationSchema, "Schemas");

        HasSchemaParameterText("019c3efa-a140-73be-900f-2e766665bc18", TextConstants.LinkNavigationParameter);
        HasDataParameterText("019c3efa-a140-73be-900f-33d0023c5698", TextConstants.LinkNavigationParameter, "Parameters");

        #endregion Link

        #region Header

        HasSchemaParameterText("019c254b-1b85-7dca-abe2-d2a11538b7ed", TextConstants.HeaderSchema);
        HasDataParameterText("019c254a-c196-7c51-a3c1-82f826185ea8", TextConstants.HeaderSchema, "Schemas");

        HasSchemaParameterText("019c254b-1b85-71dc-8f4a-4295bd279c0a", TextConstants.HeaderContext);
        HasDataParameterText("019c254a-c196-7595-b493-5ccf1ea9faad", TextConstants.HeaderContext, "Contexts");

        HasSchemaParameterText("019c254b-1b85-772d-aaf0-dff4ea453c6c", TextConstants.HeaderElement);
        HasDataParameterText("019c254a-c196-7968-8109-da3f68bd324f", TextConstants.HeaderElement, "Elements");

        HasSchemaParameterText("019c254b-1b85-7ec8-9720-24a5bcaf1888", TextConstants.HeaderParameter);
        HasDataParameterText("019c254a-c196-7e7d-98bb-36fd8384fb8f", TextConstants.HeaderParameter, "Parameters");

        HasSchemaParameterText("019c2960-0f40-756c-b67a-982d29ade43c", TextConstants.HeaderParameterStyle);
        HasDataParameterText("019c2570-ace5-7641-8c58-2e1be9103503", TextConstants.HeaderParameterStyle, "Style Parameters");

        HasSchemaParameterText("019c2960-0f40-756c-b67a-9fecaa359970", TextConstants.HeaderParameterSystem);
        HasDataParameterText("019c2570-ace5-7641-8c58-33c7d3d23c9c", TextConstants.HeaderParameterSystem, "System Parameters");

        HasSchemaParameterText("019c2960-0f40-756c-b67a-a2ee6d674e65", TextConstants.HeaderParameterText);
        HasDataParameterText("019c2570-ace5-7641-8c58-36370d2a0092", TextConstants.HeaderParameterText, "Text Parameters");

        HasSchemaParameterText("019c254b-1b85-7b4f-bd18-355a9af4b607", TextConstants.HeaderProperty);
        HasDataParameterText("019c254a-c196-734d-aeaa-3f93015db3bc", TextConstants.HeaderProperty, "Properties");

        HasSchemaParameterText("019c2960-9149-73cf-a2a9-8655ebbe904f", TextConstants.HeaderPropertyBoolean);
        HasDataParameterText("019c2571-2306-7122-ba5f-910cbf856f22", TextConstants.HeaderPropertyBoolean, "Boolean Properties");

        HasSchemaParameterText("019c2960-9149-73cf-a2a9-89219fb3f605", TextConstants.HeaderPropertyCollection);
        HasDataParameterText("019c2571-2306-7122-ba5f-953e8fa4c021", TextConstants.HeaderPropertyCollection, "Collection Properties");

        HasSchemaParameterText("019c2960-9149-73cf-a2a9-8f6cfdeda288", TextConstants.HeaderPropertyDateTime);
        HasDataParameterText("019c2571-2306-7122-ba5f-9b4244c8d98f", TextConstants.HeaderPropertyDateTime, "DateTime Properties");

        HasSchemaParameterText("019c2960-9149-73cf-a2a9-90eae211c597", TextConstants.HeaderPropertyDecimal);
        HasDataParameterText("019c2571-2306-7122-ba5f-9ecf6b7c21d9", TextConstants.HeaderPropertyDecimal, "Decimal Properties");

        HasSchemaParameterText("019c2960-9149-73cf-a2a9-97f682a1a142", TextConstants.HeaderPropertyImage);
        HasDataParameterText("019c2571-2306-7122-ba5f-a1272b07a512", TextConstants.HeaderPropertyImage, "Image Properties");

        HasSchemaParameterText("019c2960-9149-73cf-a2a9-9a52f7cd3179", TextConstants.HeaderPropertyInteger);
        HasDataParameterText("019c2571-2306-7122-ba5f-a4fc019da2c8", TextConstants.HeaderPropertyInteger, "Integer Properties");

        HasSchemaParameterText("019c2960-9149-73cf-a2a9-9fce8dad7570", TextConstants.HeaderPropertyString);
        HasDataParameterText("019c2571-2306-7122-ba5f-aa68f923e6cf", TextConstants.HeaderPropertyString, "String Properties");

        HasSchemaParameterText("019c2960-9149-73cf-a2a9-a2f08d495c1c", TextConstants.HeaderPropertyTable);
        HasDataParameterText("019c2571-7b67-75bb-a029-35dcce9e1f9f", TextConstants.HeaderPropertyTable, "Table Properties");

        #endregion Header

        #region Header:Form

        HasSchemaParameterText("019c2571-7b67-75bb-a029-49faffc52b0f", TextConstants.HeaderFormSignIn);
        HasDataParameterText("019c2584-67dc-749e-971e-554b0930363a", TextConstants.HeaderFormSignIn, "...Or sign in");

        HasSchemaParameterText("019c2571-7b67-75bb-a029-4fcd838e27ad", TextConstants.HeaderFormSignUp);
        HasDataParameterText("019c2584-67dc-749e-971e-536af5f07c05", TextConstants.HeaderFormSignUp, "Create an account");

        HasSchemaParameterText("019c240c-791c-7579-8011-fd306ef3be7d", TextConstants.HeaderFormCreateEntitySchema);
        HasDataParameterText("019c240d-19d8-7f6e-a91d-db2e3c9e6e5e", TextConstants.HeaderFormCreateEntitySchema, "Create Schema");

        HasSchemaParameterText("019c240c-791c-7d7e-accd-c533af9a13f2", TextConstants.HeaderFormCreateEntitySchemaContext);
        HasDataParameterText("019c240d-19d8-71b0-b48d-639f8765db5a", TextConstants.HeaderFormCreateEntitySchemaContext, "Create Schema Context");

        HasSchemaParameterText("019c240c-791c-73eb-be79-a8f9fd6195bf", TextConstants.HeaderFormCreateEntitySchemaElement);
        HasDataParameterText("019c240d-19d8-754f-9c95-ca256884cd81", TextConstants.HeaderFormCreateEntitySchemaElement, "Create Schema Element");

        HasSchemaParameterText("019c240c-791c-70b5-866b-f755d1c45382", TextConstants.HeaderFormCreateEntitySchemaParameterStyle);
        HasDataParameterText("019c240d-19d8-77db-a925-13311a12f816", TextConstants.HeaderFormCreateEntitySchemaParameterStyle, "Create Schema Style Parameter");

        HasSchemaParameterText("019c240c-791c-79a9-ad25-bd0e7f197bfe", TextConstants.HeaderFormCreateEntitySchemaParameterSystem);
        HasDataParameterText("019c240d-19d8-761c-804b-b56d211c5c43", TextConstants.HeaderFormCreateEntitySchemaParameterSystem, "Create Schema System Parameter");

        HasSchemaParameterText("019c240c-791c-7b41-ad27-573d62c60f54", TextConstants.HeaderFormCreateEntitySchemaParameterText);
        HasDataParameterText("019c240d-19d8-79bf-af26-543a2ae1fcdf", TextConstants.HeaderFormCreateEntitySchemaParameterText, "Create Schema Text Parameter");

        HasSchemaParameterText("019c240c-791c-728d-a32f-32740d710848", TextConstants.HeaderFormCreateEntitySchemaPropertyBoolean);
        HasDataParameterText("019c240d-19d8-75f8-99cb-285ba1b2ecd8", TextConstants.HeaderFormCreateEntitySchemaPropertyBoolean, "Create Schema Boolean Property");

        HasSchemaParameterText("019c240c-791c-77c6-9d30-a90e02a17d97", TextConstants.HeaderFormCreateEntitySchemaPropertyCollection);
        HasDataParameterText("019c240d-19d8-7cb3-b84c-981fd2862f0e", TextConstants.HeaderFormCreateEntitySchemaPropertyCollection, "Create Schema Collection Property");

        HasSchemaParameterText("019c240c-791c-780b-8287-8db5087a9579", TextConstants.HeaderFormCreateEntitySchemaPropertyDateTime);
        HasDataParameterText("019c240d-19d8-736f-87d7-71972f4848b0", TextConstants.HeaderFormCreateEntitySchemaPropertyDateTime, "Create Schema DateTime Property");

        HasSchemaParameterText("019c240c-791c-7a15-946f-d1cf59ddac12", TextConstants.HeaderFormCreateEntitySchemaPropertyDecimal);
        HasDataParameterText("019c240d-19d8-727f-9d4a-b6a92f70bfd9", TextConstants.HeaderFormCreateEntitySchemaPropertyDecimal, "Create Schema Decimal Property");

        HasSchemaParameterText("019c240c-791c-7a50-8798-918e8726d667", TextConstants.HeaderFormCreateEntitySchemaPropertyImage);
        HasDataParameterText("019c240d-19d8-7789-b384-b32c673d1f98", TextConstants.HeaderFormCreateEntitySchemaPropertyImage, "Create Schema Image Property");

        HasSchemaParameterText("019c240c-791c-7afa-a013-344f1600999c", TextConstants.HeaderFormCreateEntitySchemaPropertyInteger);
        HasDataParameterText("019c240d-19d8-7b18-8700-7933cc2e5ac7", TextConstants.HeaderFormCreateEntitySchemaPropertyInteger, "Create Schema Integer Property");

        HasSchemaParameterText("019c240c-791c-7ffb-8f29-38fb70a51934", TextConstants.HeaderFormCreateEntitySchemaPropertyString);
        HasDataParameterText("019c240d-19d8-7f86-b88c-db22de407519", TextConstants.HeaderFormCreateEntitySchemaPropertyString, "Create Schema String Property");

        HasSchemaParameterText("019c240c-791c-77f9-97ee-ad37630c7fdf", TextConstants.HeaderFormCreateEntitySchemaPropertyTable);
        HasDataParameterText("019c240d-19d8-7b93-9f90-de94403d000a", TextConstants.HeaderFormCreateEntitySchemaPropertyTable, "Create Schema Table Property");

        HasSchemaParameterText("019c240d-6e22-77a8-99d9-76c997bc7eff", TextConstants.HeaderFormUpdateEntitySchema);
        HasDataParameterText("019c240d-8b86-7c30-8564-1242f5f9abaa", TextConstants.HeaderFormUpdateEntitySchema, "Update Schema");

        HasSchemaParameterText("019c240d-6e22-70f5-bb78-f1017feca103", TextConstants.HeaderFormUpdateEntitySchemaContext);
        HasDataParameterText("019c240d-8b86-7303-b114-c733077eb990", TextConstants.HeaderFormUpdateEntitySchemaContext, "Update Schema Context");

        HasSchemaParameterText("019c240d-6e22-7bf6-9778-7d42751cc301", TextConstants.HeaderFormUpdateEntitySchemaElement);
        HasDataParameterText("019c240d-8b86-7a12-bcd4-5316fd6894b0", TextConstants.HeaderFormUpdateEntitySchemaElement, "Update Schema Element");

        HasSchemaParameterText("019c240d-6e22-78af-affd-196b637a0dfc", TextConstants.HeaderFormUpdateEntitySchemaParameterStyle);
        HasDataParameterText("019c240d-8b86-7b6f-8892-d4ddd7793234", TextConstants.HeaderFormUpdateEntitySchemaParameterStyle, "Update Schema Style Parameter");

        HasSchemaParameterText("019c240d-6e22-7f13-91f7-40b557df76e5", TextConstants.HeaderFormUpdateEntitySchemaParameterSystem);
        HasDataParameterText("019c240d-8b86-79f5-9787-1aeb9cde12b1", TextConstants.HeaderFormUpdateEntitySchemaParameterSystem, "Update Schema System Parameter");

        HasSchemaParameterText("019c240d-6e22-7fed-b8e1-87cca3ee930d", TextConstants.HeaderFormUpdateEntitySchemaParameterText);
        HasDataParameterText("019c240d-8b86-7eb2-bf9e-fa45f0051326", TextConstants.HeaderFormUpdateEntitySchemaParameterText, "Update Schema Text Parameter");

        HasSchemaParameterText("019c240d-6e22-7913-9fae-b135cf903053", TextConstants.HeaderFormUpdateEntitySchemaPropertyBoolean);
        HasDataParameterText("019c240d-8b86-764f-86ad-1e8c2203acee", TextConstants.HeaderFormUpdateEntitySchemaPropertyBoolean, "Update Schema Boolean Property");

        HasSchemaParameterText("019c240d-6e22-7942-adec-6a8773092b8b", TextConstants.HeaderFormUpdateEntitySchemaPropertyCollection);
        HasDataParameterText("019c240d-8b86-794e-a648-3da4fd31dc9b", TextConstants.HeaderFormUpdateEntitySchemaPropertyCollection, "Update Schema Collection Property");

        HasSchemaParameterText("019c240d-6e22-71fa-a972-a870f1167975", TextConstants.HeaderFormUpdateEntitySchemaPropertyDateTime);
        HasDataParameterText("019c240d-8b86-7b8a-9c71-0d9ae20a2a50", TextConstants.HeaderFormUpdateEntitySchemaPropertyDateTime, "Update Schema DateTime Property");

        HasSchemaParameterText("019c240d-6e22-7df2-9ad3-1f4337f1f415", TextConstants.HeaderFormUpdateEntitySchemaPropertyDecimal);
        HasDataParameterText("019c240d-8b86-7b31-98f5-60c0c15ecb49", TextConstants.HeaderFormUpdateEntitySchemaPropertyDecimal, "Update Schema Decimal Property");

        HasSchemaParameterText("019c240d-6e22-7e3a-8edf-09814b4b8a57", TextConstants.HeaderFormUpdateEntitySchemaPropertyImage);
        HasDataParameterText("019c240d-8b86-7d81-bfbc-9f8384ab8a2d", TextConstants.HeaderFormUpdateEntitySchemaPropertyImage, "Update Schema Image Property");

        HasSchemaParameterText("019c240d-6e22-7532-a326-85012b6af7be", TextConstants.HeaderFormUpdateEntitySchemaPropertyInteger);
        HasDataParameterText("019c240d-8b86-79e9-af55-66544574de3a", TextConstants.HeaderFormUpdateEntitySchemaPropertyInteger, "Update Schema Integer Property");

        HasSchemaParameterText("019c240d-6e22-7b1e-b540-fe8a2aac155f", TextConstants.HeaderFormUpdateEntitySchemaPropertyString);
        HasDataParameterText("019c240d-8b86-7197-954e-c80cb43b9192", TextConstants.HeaderFormUpdateEntitySchemaPropertyString, "Update Schema String Property");

        HasSchemaParameterText("019c240d-6e22-77d3-a36c-bbff9662a8e1", TextConstants.HeaderFormUpdateEntitySchemaPropertyTable);
        HasDataParameterText("019c240d-8b86-78a6-942c-7210fa5d6003", TextConstants.HeaderFormUpdateEntitySchemaPropertyTable, "Update Schema Table Property");

        #endregion

        #region Label

        HasSchemaParameterText("019c3efa-a140-73be-900f-360a6ccd354c", TextConstants.LabelValue);
        HasDataParameterText("019c3efa-a140-73be-900f-4bed02a7227c", TextConstants.LabelValue, "Value");

        HasSchemaParameterText("019c6162-6735-703a-be4d-44e4e3999607", TextConstants.LabelParameterStyle);
        HasDataParameterText("019c6162-6735-703a-be4d-53373d5df7f0", TextConstants.LabelParameterStyle, "Style parameter");

        HasSchemaParameterText("019c6162-6735-703a-be4d-4951669119ef", TextConstants.LabelParameterSystem);
        HasDataParameterText("019c6162-6735-703a-be4d-568ee7a09e9c", TextConstants.LabelParameterSystem, "System parameter");

        HasSchemaParameterText("019c6162-6735-703a-be4d-4d4849732133", TextConstants.LabelParameterText);
        HasDataParameterText("019c6162-6735-703a-be4d-586369c5b173", TextConstants.LabelParameterText, "Text parameter");

        HasSchemaParameterText("019c5c4f-2f15-74a8-a98f-6ec5e9fe6e27", TextConstants.LabelCulture);
        HasDataParameterText("019c5c4f-2f15-74a8-a98f-70001c79500c", TextConstants.LabelCulture, "Culture");

        HasSchemaParameterText("019c3efa-a140-73be-900f-3bf25ff45c30", TextConstants.LabelTimeApproved);
        HasDataParameterText("019c3efa-a140-73be-900f-4e2b26906c90", TextConstants.LabelTimeApproved, "Approved at");

        HasSchemaParameterText("019c3efa-a140-73be-900f-3ecaa29e0b39", TextConstants.LabelTimeCreated);
        HasDataParameterText("019c47d2-99d6-76d6-bffc-a46b61cfe957", TextConstants.LabelTimeCreated, "Created at");

        HasSchemaParameterText("019c535a-46a5-7391-afea-fd216ed0931f", TextConstants.LabelTimeEffective);
        HasDataParameterText("019c535d-0142-766d-9f9d-eb464887d764", TextConstants.LabelTimeEffective, "Effective at");

        HasSchemaParameterText("019c3efa-a140-73be-900f-40a664ac2558", TextConstants.LabelTimeEffectiveFrom);
        HasDataParameterText("019c3efd-764d-740a-baae-c0240683a292", TextConstants.LabelTimeEffectiveFrom, "Effective from");

        HasSchemaParameterText("019c3efa-a140-73be-900f-45045dba457e", TextConstants.LabelTimeEffectiveTo);
        HasDataParameterText("019c3efd-764d-740a-baae-c7503349db66", TextConstants.LabelTimeEffectiveTo, "Effective to");

        #endregion Label

        #region Label:Form

        HasSchemaParameterText("019c2964-c66f-77de-83ad-b8caf05e9319", TextConstants.LabelFormEmail);
        HasDataParameterText("019c2964-c66f-77de-83ad-c60e1292abfa", TextConstants.LabelFormEmail, "Email");

        HasSchemaParameterText("019c2964-c66f-77de-83ad-bfe74128c9df", TextConstants.LabelFormPassword);
        HasDataParameterText("019c2964-c66f-77de-83ad-c8e93394b591", TextConstants.LabelFormPassword, "Password");

        HasSchemaParameterText("019c2964-c66f-77de-83ad-c219ed783910", TextConstants.LabelFormConfirmPassword);
        HasDataParameterText("019c2964-c66f-77de-83ad-cf27649f093d", TextConstants.LabelFormConfirmPassword, "Confirm password");

        HasSchemaParameterText("019c297a-384a-716a-92bd-67c96ecb2b83", TextConstants.LabelFormSearch);
        HasDataParameterText("019c297a-384a-716a-92bd-6ad2110dfd27", TextConstants.LabelFormSearch, "Search");

        HasSchemaParameterText("019789de-e449-71aa-ab1d-0992c66a9dae", TextConstants.LabelFormSchemaIdentifier);
        HasDataParameterText("019789f9-2601-72ac-ad27-ea4b8f4855d6", TextConstants.LabelFormSchemaIdentifier, "Schema identifier");

        HasSchemaParameterText("019c4c5d-b1c8-722e-ac29-e0f469968754", TextConstants.LabelFormValue);
        HasDataParameterText("019c4c5d-b1c8-722e-ac29-e4745ec5528e", TextConstants.LabelFormValue, "Value");

        HasSchemaParameterText("019c5c4f-2f15-74a8-a98f-7832658bff84", TextConstants.LabelFormCulture);
        HasDataParameterText("019c5c4f-2f15-74a8-a98f-77d2f6daeaf6", TextConstants.LabelFormCulture, "Culture");

        HasSchemaParameterText("019789de-e449-71aa-ab1d-12e8b546b239", TextConstants.LabelFormName);
        HasDataParameterText("019789f9-2601-72ac-ad27-f1f7ad078b01", TextConstants.LabelFormName, "Name");

        HasSchemaParameterText("019789de-e449-71aa-ab1d-17209b94ded6", TextConstants.LabelFormNote);
        HasDataParameterText("019789f9-2601-72ac-ad27-f7c76f0002d4", TextConstants.LabelFormNote, "Note");

        HasSchemaParameterText("019789de-e449-71aa-ab1d-192c4a3c0eb6", TextConstants.LabelFormTitle);
        HasDataParameterText("019789f9-2601-72ac-ad27-f9ef87027fec", TextConstants.LabelFormTitle, "Title");

        HasSchemaParameterText("019789de-e449-71aa-ab1d-1c1707f61f89", TextConstants.LabelFormDescription);
        HasDataParameterText("019789f9-2601-72ac-ad27-fceadc8f7eda", TextConstants.LabelFormDescription, "Description");

        HasSchemaParameterText("019789de-e449-71aa-ab1d-205e04219122", TextConstants.LabelFormOrder);
        HasDataParameterText("019789f9-2601-72ac-ad28-0204b7c6d497", TextConstants.LabelFormOrder, "Order");

        HasSchemaParameterText("019c297a-384a-716a-92bd-54bc539e59e1", TextConstants.LabelFormIsSystemDefined);
        HasDataParameterText("019c297a-384a-716a-92bd-5b2db391ac89", TextConstants.LabelFormIsSystemDefined, "Is system defined?");

        HasSchemaParameterText("01978a17-7901-7131-8b49-005b042a1608", TextConstants.LabelFormIsApprovalRequired);
        HasDataParameterText("01978a17-7901-7131-8b49-1200bad01c81", TextConstants.LabelFormIsApprovalRequired, "Is approval required?");

        HasSchemaParameterText("019c6569-7c03-7147-ba6b-441952343d05", TextConstants.LabelFormParameterType);
        HasDataParameterText("019c6569-7c03-7147-ba6b-4960b1761d54", TextConstants.LabelFormParameterType, "Parameter type");

        HasSchemaParameterText("019c4c5d-b1c8-722e-ac2a-010981c08758", TextConstants.LabelFormDateEffective);
        HasDataParameterText("019c4c5d-b1c8-722e-ac29-fee711de1d54", TextConstants.LabelFormDateEffective, "Effective date");

        HasSchemaParameterText("019c3819-d223-700d-a78d-40606a57c869", TextConstants.LabelFormPageNumber);
        HasDataParameterText("019c3819-d224-705d-91a0-98a1e7acf79a", TextConstants.LabelFormPageNumber, "Page");

        HasSchemaParameterText("019c3819-d224-705d-91a0-96e586add967", TextConstants.LabelFormPageSize);
        HasDataParameterText("019c3819-d224-705d-91a0-9f7cd0157d26", TextConstants.LabelFormPageSize, "Rows");

        HasSchemaParameterText("01979d14-54ad-72f9-b5d8-a5aa1089fe1b", TextConstants.LabelFormParameterTextType);
        HasDataParameterText("01978a1f-f0a2-731f-b17d-2174a6ecd4fe", TextConstants.LabelFormParameterTextType, "Text parameter type");

        HasSchemaParameterText("019799a3-a72f-725a-b4f6-b7144f565f3f", TextConstants.LabelFormBoolean);
        HasDataParameterText("019799a4-1a2b-7368-9b33-6c45805d80a3", TextConstants.LabelFormBoolean, "Boolean");

        HasSchemaParameterText("019799a3-a72f-725a-b4f6-b8b9e65fa2e9", TextConstants.LabelFormDateTime);
        HasDataParameterText("019799a4-1a2b-7368-9b33-72817ce487a1", TextConstants.LabelFormDateTime, "DateTime");

        HasSchemaParameterText("019799a3-a72f-725a-b4f6-be98a390cf1a", TextConstants.LabelFormDecimal);
        HasDataParameterText("019799a4-1a2b-7368-9b33-77fdd478928f", TextConstants.LabelFormDecimal, "Decimal");

        HasSchemaParameterText("019799a3-a72f-725a-b4f6-c0efbd9b5dbb", TextConstants.LabelFormInteger);
        HasDataParameterText("019799a4-1a2b-7368-9b33-781625aca070", TextConstants.LabelFormInteger, "Integer");

        HasSchemaParameterText("019799a3-a72f-725a-b4f6-c449cc81422f", TextConstants.LabelFormString);
        HasDataParameterText("019799a4-1a2b-7368-9b33-7c0e241b9622", TextConstants.LabelFormString, "String");

        HasSchemaParameterText("019799a3-a72f-725a-b4f6-cd308b80c164", TextConstants.LabelFormParameterStyle);
        HasDataParameterText("019799a4-1a2b-7368-9b33-86038825b246", TextConstants.LabelFormParameterStyle, "Style parameter");

        HasSchemaParameterText("019799a3-a72f-725a-b4f6-d3344af0a274", TextConstants.LabelFormParameterSystem);
        HasDataParameterText("019799a4-1a2b-7368-9b33-8bd3f0f5e948", TextConstants.LabelFormParameterSystem, "System parameter");

        HasSchemaParameterText("019799a3-a72f-725a-b4f6-d6b06a4f6d1b", TextConstants.LabelFormParameterText);
        HasDataParameterText("019799a4-1a2b-7368-9b33-8c2e6b4a0b6d", TextConstants.LabelFormParameterText, "Text parameter");

        #endregion Label:Form

        #region Help:Form

        HasSchemaParameterText("019c2964-c66f-77de-83ad-d2e4058c6134", TextConstants.HelpFormEmail);
        HasDataParameterText("019c2964-c66f-77de-83ad-ddee52bf7792", TextConstants.HelpFormEmail, "The email address you want to associate with the account.");

        HasSchemaParameterText("019c2964-c66f-77de-83ad-d586d9686df2", TextConstants.HelpFormPassword);
        HasDataParameterText("019c2964-c66f-77de-83ad-e28d48839f57", TextConstants.HelpFormPassword, "A password consisting of at least 12 characters.");

        HasSchemaParameterText("019c2964-c66f-77de-83ad-d89e8c04683e", TextConstants.HelpFormConfirmPassword);
        HasDataParameterText("019c2964-c66f-77de-83ad-e416d3c0445a", TextConstants.HelpFormConfirmPassword, "Repeat your password to confirm it.");

        HasSchemaParameterText("019c29f9-5de4-760e-90b6-d8e7a6c0239a", TextConstants.HelpFormSearch);
        HasDataParameterText("019c297a-384a-716a-92bd-6e1c4715908c", TextConstants.HelpFormSearch, "Enter the search term you wish to use");

        HasSchemaParameterText("019789fc-3929-75a9-99e8-f7a6abf0c139", TextConstants.HelpFormSchemaIdentifier);
        HasDataParameterText("019789fc-18dc-73ee-94b6-1564b2f72eb7", TextConstants.HelpFormSchemaIdentifier, "The unique, humanly readable identifier for this entity type.");

        HasSchemaParameterText("019c4c5d-b1c8-722e-ac29-e8fc5683a10d", TextConstants.HelpFormValue);
        HasDataParameterText("019c4c5d-b1c8-722e-ac29-f25552e22b82", TextConstants.HelpFormValue, "The value to be used with this entity.");

        HasSchemaParameterText("019c5c4f-2f15-74a8-a98f-7fde1fc8f8ac", TextConstants.HelpFormCulture);
        HasDataParameterText("019c5c4f-2f15-74a8-a98f-8095ce1d47e4", TextConstants.HelpFormCulture, "The culture this entity is used together with.");

        HasSchemaParameterText("019789fc-3929-75a9-99e8-f819edf63934", TextConstants.HelpFormName);
        HasDataParameterText("019789fc-18dc-73ee-94b6-1c60e96f26a9", TextConstants.HelpFormName, "The internal name of this entity type. Should only be visible in the configuration.");

        HasSchemaParameterText("019789fc-3929-75a9-99e8-ffd616e87b30", TextConstants.HelpFormNote);
        HasDataParameterText("019789fc-18dc-73ee-94b6-21b290f8401d", TextConstants.HelpFormNote, "A short note detailing how this entity type is used. Should only be visible in the configuration.");

        HasSchemaParameterText("019789fc-3929-75a9-99e9-0229499f0ddd", TextConstants.HelpFormTitle);
        HasDataParameterText("019789fc-18dc-73ee-94b6-26ead5f18d4d", TextConstants.HelpFormTitle, "The title used when displaying an entity of this type.");

        HasSchemaParameterText("019789fc-3929-75a9-99e9-046632ba51b7", TextConstants.HelpFormDescription);
        HasDataParameterText("019789fc-18dc-73ee-94b6-28dc0edf9b69", TextConstants.HelpFormDescription, "The description used when displaying an entity of this type.");

        HasSchemaParameterText("019789fc-3929-75a9-99e9-0a5f8ca00604", TextConstants.HelpFormOrder);
        HasDataParameterText("019789fc-18dc-73ee-94b6-2ee0b27366dc", TextConstants.HelpFormOrder, "The order in which this entity type is sorted.");

        HasSchemaParameterText("019c297a-384a-716a-92bd-5cefb96ac3a0", TextConstants.HelpFormIsSystemDefined);
        HasDataParameterText("019c297a-384a-716a-92bd-627c7c93c3c9", TextConstants.HelpFormIsSystemDefined, "Determines if the entity is system defined and therefore cannot be changed.");

        HasSchemaParameterText("01978a17-b7b1-772f-a129-543b087e1606", TextConstants.HelpFormIsApprovalRequired);
        HasDataParameterText("01978a17-b7b1-772f-a129-66dba634b0b5", TextConstants.HelpFormIsApprovalRequired, "Determines if another user must approve a text parameter value before it becomes active.");

        HasSchemaParameterText("019c656d-2bcd-758e-9de1-138050bc8afe", TextConstants.HelpFormParameterType);
        HasDataParameterText("01979d14-54ad-72f9-b5d8-b830dc26f9f4", TextConstants.HelpFormParameterType, "The specific type of parameter to be used.");

        HasSchemaParameterText("019c4c5d-b1c8-722e-ac29-f7ab4a6300bd", TextConstants.HelpFormDateEffective);
        HasDataParameterText("019c4c5d-b1c8-722e-ac29-f9cc8fe007f7", TextConstants.HelpFormDateEffective, "The date from which this entity should become active.");

        HasSchemaParameterText("019c3819-d224-705d-91a0-a1fc39ba13d5", TextConstants.HelpFormPageNumber);
        HasDataParameterText("019c3819-d224-705d-91a0-a9fe4d149531", TextConstants.HelpFormPageNumber, "The current page being shown");

        HasSchemaParameterText("019c3819-d224-705d-91a0-a5d6e5fdfc6d", TextConstants.HelpFormPageSize);
        HasDataParameterText("019c3819-d224-705d-91a0-af95528a7316", TextConstants.HelpFormPageSize, "The number of rows shown on each page");

        HasSchemaParameterText("01979d14-54ad-72f9-b5d8-b33557c5a806", TextConstants.HelpFormParameterTextType);
        HasDataParameterText("01978a20-9692-72ff-be7d-b59a17facafb", TextConstants.HelpFormParameterTextType, "The type of text parameter.");

        HasSchemaParameterText("019799a6-8dc0-75af-8866-97a26b3415dc", TextConstants.HelpFormBoolean);
        HasDataParameterText("019799a6-6b05-7029-b9a7-2d147f778de8", TextConstants.HelpFormBoolean, "Represents a boolean value.");

        HasSchemaParameterText("019799a6-8dc0-75af-8866-986c4570a7bd", TextConstants.HelpFormDateTime);
        HasDataParameterText("019799a6-6b05-7029-b9a7-300f0306a227", TextConstants.HelpFormDateTime, "Represents a datetime value.");

        HasSchemaParameterText("019799a6-8dc0-75af-8866-9f89357ebb7f", TextConstants.HelpFormDecimal);
        HasDataParameterText("019799a6-6b05-7029-b9a7-35f76c49841a", TextConstants.HelpFormDecimal, "Represents a decimal value.");

        HasSchemaParameterText("019799a6-8dc0-75af-8866-a390807971d2", TextConstants.HelpFormInteger);
        HasDataParameterText("019799a6-6b05-7029-b9a7-3a924229a88a", TextConstants.HelpFormInteger, "Represents a integer value.");

        HasSchemaParameterText("019799a6-8dc0-75af-8866-a44ca62f2e18", TextConstants.HelpFormString);
        HasDataParameterText("019799a6-6b05-7029-b9a7-3ca405bc7246", TextConstants.HelpFormString, "Represents a string value.");

        HasSchemaParameterText("019799a6-8dc0-75af-8866-ad7a01af4eac", TextConstants.HelpFormParameterStyle);
        HasDataParameterText("019799a6-6b05-7029-b9a7-473fbbc23ec3", TextConstants.HelpFormParameterStyle, "Represents a dynamic style parameter value.");

        HasSchemaParameterText("019799a6-8dc0-75af-8866-b0be19e7dcdd", TextConstants.HelpFormParameterSystem);
        HasDataParameterText("019799a6-6b05-7029-b9a7-48183a3903a4", TextConstants.HelpFormParameterSystem, "Represents a dynamic system parameter value.");

        HasSchemaParameterText("019799a6-8dc0-75af-8866-b4f08d0df491", TextConstants.HelpFormParameterText);
        HasDataParameterText("019799a6-6b05-7029-b9a7-4c3fb79cf9d1", TextConstants.HelpFormParameterText, "Represents a dynamic text parameter value.");

        #endregion Help:Form

        #region Value

        HasSchemaParameterText("019c27aa-cee4-758d-a50d-333d37fb8c53", TextConstants.ValueOptionYes);
        HasDataParameterText("019c27aa-cee4-758d-a50d-3543259e9a3f", TextConstants.ValueOptionYes, "Yes");

        HasSchemaParameterText("019c6567-5dfb-718a-a78d-e461307e3c4a", TextConstants.ValueOptionNo);
        HasDataParameterText("019c6569-7c03-7147-ba6b-298df560d2fa", TextConstants.ValueOptionNo, "No");

        HasSchemaParameterText("019c6567-5dfb-718a-a78d-eab80f1ddd37", TextConstants.ValueIsSystemDefinedNeutral);
        HasDataParameterText("019c6569-7c03-7147-ba6b-2c9e53c8740e", TextConstants.ValueIsSystemDefinedNeutral, "Either");

        HasSchemaParameterText("019c6567-5dfb-718a-a78d-ec459138c4da", TextConstants.ValueIsSystemDefinedYes);
        HasDataParameterText("019c6569-7c03-7147-ba6b-32b69e752274", TextConstants.ValueIsSystemDefinedYes, "System defined");

        HasSchemaParameterText("019c6567-5dfb-718a-a78d-f0ac058dd105", TextConstants.ValueIsSystemDefinedNo);
        HasDataParameterText("019c6569-7c03-7147-ba6b-3534a4a64629", TextConstants.ValueIsSystemDefinedNo, "User defined");

        HasSchemaParameterText("019c6567-5dfb-718a-a78d-f5d5ccd3967b", TextConstants.ValueIsApprovalRequiredNeutral);
        HasDataParameterText("019c6569-7c03-7147-ba6b-3a959e7c8175", TextConstants.ValueIsApprovalRequiredNeutral, "Either");

        HasSchemaParameterText("019c6567-5dfb-718a-a78d-f9edcef3f870", TextConstants.ValueIsApprovalRequiredYes);
        HasDataParameterText("019c6569-7c03-7147-ba6b-3f6148ddde49", TextConstants.ValueIsApprovalRequiredYes, "With approval required");

        HasSchemaParameterText("019c6567-5dfb-718a-a78d-fd0894414d18", TextConstants.ValueIsApprovalRequiredNo);
        HasDataParameterText("019c6569-7c03-7147-ba6b-407b06241d47", TextConstants.ValueIsApprovalRequiredNo, "Without approval required");

        #endregion
    }
}
