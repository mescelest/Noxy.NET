using Microsoft.EntityFrameworkCore;
using Noxy.NET.CaseManagement.Database.Builders;
using Noxy.NET.CaseManagement.Domain.Constants;
using Noxy.NET.CaseManagement.Domain.Enums;
using Noxy.NET.CaseManagement.Persistence;
using Noxy.NET.CaseManagement.Persistence.Tables.Schemas;

namespace Noxy.NET.CaseManagement.Database.Seeds;

public class BaseSchemaSeed(DataContext context)
{
    public const string IdentifierContextCase = "ContextCase";

    public const string IdentifierDynamicValueCodeCreateCase = "CodeCreateCase";
    public const string IdentifierDynamicValueCodeUpdateCase = "CodeUpdateCase";
    public const string IdentifierDynamicValueCodeCurrentDateAsHTMLValue = "CodeCurrentDateAsHTMLValue";
    public const string IdentifierDynamicValueCodeCurrentDateNextYearAsHTMLValue = "CodeCurrentDateNextYearAsHTMLValue";

    public const string IdentifierDynamicValueTextParameterLabelYes = "TextParameterLabelYes";
    public const string IdentifierDynamicValueTextParameterLabelNo = "TextParameterLabelNo";
    
    public const string IdentifierAttributeIntegerMinLength = "AttributeIntegerMinLength";
    public const string IdentifierAttributeIntegerMaxLength = "AttributeIntegerMaxLength";

    public const string IdentifierAttributeStringHTMLAutoComplete = "AttributeStringHTMLAutoComplete";

    public const string IdentifierAttributeDynamicValueTextParameterLabelSingular = "AttributeDynamicValueTextParameterLabelSingular";
    public const string IdentifierAttributeDynamicValueTextParameterLabelMultiple = "AttributeDynamicValueTextParameterLabelMultiple";

    public const string IdentifierActionInputCaseNumber = "ActionInputCaseNumber";
    public const string IdentifierActionInputIsCompleted = "ActionInputIsCompleted";
    public const string IdentifierActionInputCreatedAt = "ActionInputCreatedAt";

    public const string IdentifierPropertyStringCaseNumber = "PropertyStringCaseNumber";
    public const string IdentifierPropertyBooleanCompleted = "PropertyBooleanCompleted";
    public const string IdentifierPropertyDateTimeCreated = "PropertyDateTimeCreated";

    public const string IdentifierElementCase = "ElementCase";
    public const string IdentifierElementJournal = "ElementJournal";

    public const string IdentifierActionCreateCase = "ActionCreateCase";
    public const string IdentifierActionUpdateCase = "ActionUpdateCase";

    public const string IdentifierActionCreateCaseActionStepAddData = "ActionCreateCaseActionStepAddData";

    public async Task Apply()
    {
        TableSchema tableSchema = await context.Schema.FirstAsync();
        SchemaSeedBuilder builder = new(context, tableSchema);
        DataSeedBuilder builderData = new(context);

        TableSchemaDynamicValueTextParameter textPropertyBooleanIsCompleted = await context.SchemaDynamicValueTextParameter.FirstAsync(x => x.SchemaIdentifier == BaseTextSeed.PropertyBooleanIsCompleted);
        TableSchemaDynamicValueTextParameter textPropertyDateTimeCreatedAt = await context.SchemaDynamicValueTextParameter.FirstAsync(x => x.SchemaIdentifier == BaseTextSeed.PropertyDateTimeCreatedAt);
        TableSchemaDynamicValueTextParameter textPropertyStringCaseNumber = await context.SchemaDynamicValueTextParameter.FirstAsync(x => x.SchemaIdentifier == BaseTextSeed.PropertyStringCaseNumber);
        
        TableSchemaDynamicValueTextParameter textActionCreateCase = await context.SchemaDynamicValueTextParameter.FirstAsync(x => x.SchemaIdentifier == BaseTextSeed.ActionCreateCase);
        TableSchemaDynamicValueTextParameter textActionUpdateCase = await context.SchemaDynamicValueTextParameter.FirstAsync(x => x.SchemaIdentifier == BaseTextSeed.ActionUpdateCase);
        
        TableSchemaDynamicValueTextParameter textActionStepEnterCaseData = await context.SchemaDynamicValueTextParameter.FirstAsync(x => x.SchemaIdentifier == BaseTextSeed.ActionStepAddCaseData);
        
        TableSchemaDynamicValueTextParameter textActionInputCaseNumber = await context.SchemaDynamicValueTextParameter.FirstAsync(x => x.SchemaIdentifier == BaseTextSeed.ActionInputCaseNumber);
        TableSchemaDynamicValueTextParameter textActionInputCreatedAt = await context.SchemaDynamicValueTextParameter.FirstAsync(x => x.SchemaIdentifier == BaseTextSeed.ActionInputCreatedAt);
        TableSchemaDynamicValueTextParameter textActionInputIsCompleted = await context.SchemaDynamicValueTextParameter.FirstAsync(x => x.SchemaIdentifier == BaseTextSeed.ActionInputIsCompleted);
        
        TableSchemaDynamicValueTextParameter textElementCase = await context.SchemaDynamicValueTextParameter.FirstAsync(x => x.SchemaIdentifier == BaseTextSeed.ElementCase);
        TableSchemaDynamicValueTextParameter textElementJournal = await context.SchemaDynamicValueTextParameter.FirstAsync(x => x.SchemaIdentifier == BaseTextSeed.ElementJournal);

        TableSchemaDynamicValueTextParameter textContextCaseManagement = await context.SchemaDynamicValueTextParameter.FirstAsync(x => x.SchemaIdentifier == BaseTextSeed.ContextCaseManagement);
        
        // Add attributes
        TableSchemaAttribute tableSchemaAttributeDynamicValueTextParameterLabelSingular = builder.AddAttribute(IdentifierAttributeDynamicValueTextParameterLabelSingular, "Label (singular, dynamic)", type: AttributeTypeEnum.DynamicValue);
        TableSchemaAttribute tableSchemaAttributeDynamicValueTextParameterLabelMultiple = builder.AddAttribute(IdentifierAttributeDynamicValueTextParameterLabelMultiple, "Label (multiple, dynamic)", type: AttributeTypeEnum.DynamicValue, isList: true);

        TableSchemaAttribute tableSchemaAttributeIntegerDynamicMinDate = builder.AddAttribute(SchemaActionInputConstants.SchemaIdentifierAttributeDynamicValueMinDate, "Min date (dynamic)", type: AttributeTypeEnum.DynamicValue);
        TableSchemaAttribute tableSchemaAttributeIntegerDynamicMaxDate = builder.AddAttribute(SchemaActionInputConstants.SchemaIdentifierAttributeDynamicValueMaxDate, "Max date (dynamic)", type: AttributeTypeEnum.DynamicValue);

        TableSchemaAttribute tableSchemaAttributeIntegerMinLength = builder.AddAttribute(IdentifierAttributeIntegerMinLength, "Min length (static)", type: AttributeTypeEnum.Integer);
        TableSchemaAttribute tableSchemaAttributeIntegerMaxLength = builder.AddAttribute(IdentifierAttributeIntegerMaxLength, "Max length (static)", type: AttributeTypeEnum.Integer);

        TableSchemaAttribute tableSchemaAttributeStringHTMLAutoComplete = builder.AddAttribute(IdentifierAttributeStringHTMLAutoComplete, "HTML AutoComplete (static)", type: AttributeTypeEnum.String);

        // Add Inputs
        TableSchemaInput tableSchemaInputCheckbox = await context.SchemaInput.FirstAsync(x => x.SchemaIdentifier == "ActionInputCheckbox");
        builder.Relate(tableSchemaInputCheckbox, tableSchemaAttributeDynamicValueTextParameterLabelSingular);

        TableSchemaInput tableSchemaInputSingleChoice = await context.SchemaInput.FirstAsync(x => x.SchemaIdentifier == "ActionInputSingleChoice");
        builder.Relate(tableSchemaInputSingleChoice, tableSchemaAttributeDynamicValueTextParameterLabelMultiple);

        TableSchemaInput tableSchemaInputDate = await context.SchemaInput.FirstAsync(x => x.SchemaIdentifier == "ActionInputDate");
        builder.Relate(tableSchemaInputDate, tableSchemaAttributeIntegerDynamicMinDate);
        builder.Relate(tableSchemaInputDate, tableSchemaAttributeIntegerDynamicMaxDate);

        TableSchemaInput tableSchemaInputText = await context.SchemaInput.FirstAsync(x => x.SchemaIdentifier == "ActionInputText");
        builder.Relate(tableSchemaInputText, tableSchemaAttributeIntegerMinLength);
        builder.Relate(tableSchemaInputText, tableSchemaAttributeIntegerMaxLength);
        builder.Relate(tableSchemaInputText, tableSchemaAttributeStringHTMLAutoComplete);

        // Add Code snippets

        const string codeDynamicValueCodeCurrentDateAsHTMLValue =
            """
            return DateTime.UtcNow.ToString("yyyy-MM-dd");
            """;
        TableSchemaDynamicValueCode tableDynamicValueCodeCurrentDateAsHTMLValue = builder.AddDynamicValueCode(IdentifierDynamicValueCodeCurrentDateAsHTMLValue, "Current date as HTML value", codeDynamicValueCodeCurrentDateAsHTMLValue);

        const string codeDynamicValueCodeCurrentDateNextYearAsHTMLValue =
            """
            return DateTime.UtcNow.AddYears(1).ToString("yyyy-MM-dd");
            """;
        TableSchemaDynamicValueCode tableDynamicValueCodeCurrentDateNextYearAsHTMLValue =
            builder.AddDynamicValueCode(IdentifierDynamicValueCodeCurrentDateNextYearAsHTMLValue, "Current date next year as HTML value", codeDynamicValueCodeCurrentDateNextYearAsHTMLValue);

        const string codeCreateCase =
            $$"""
              Dictionary<string, object?> properties = new()
              {
                  { "{{IdentifierPropertyStringCaseNumber}}", data.{{IdentifierActionInputCaseNumber}} },
                  { "{{IdentifierPropertyBooleanCompleted}}", data.{{IdentifierActionInputIsCompleted}} ?? false },
                  { "{{IdentifierPropertyDateTimeCreated}}", data.{{IdentifierActionInputCreatedAt}} ?? DateTime.UtcNow },
              };
              return await API.CreateElement("{{IdentifierElementCase}}", properties);
              """;
        TableSchemaDynamicValueCode tableDynamicValueCodeCreateCase = builder.AddDynamicValueCode(IdentifierDynamicValueCodeCreateCase, "Create case logic", codeCreateCase, isAsynchronous: true);

        const string codeUpdateCase =
            $$"""
              Dictionary<string, object?> properties = new()
              {
                  { "{{IdentifierPropertyStringCaseNumber}}", data.{{IdentifierActionInputCaseNumber}} },
                  { "{{IdentifierPropertyBooleanCompleted}}", data.{{IdentifierActionInputIsCompleted}} ?? false },
                  { "{{IdentifierPropertyDateTimeCreated}}", data.{{IdentifierActionInputCreatedAt}} ?? DateTime.UtcNow },
              };
              return await API.UpdateElement(data.ID, properties);
              """;
        TableSchemaDynamicValueCode tableDynamicValueCodeUpdateCase = builder.AddDynamicValueCode(IdentifierDynamicValueCodeUpdateCase, "Create case logic", codeUpdateCase, isAsynchronous: true);
        
        // Add Text Parameters
        TableSchemaDynamicValueTextParameter tableDynamicValueTextParameterLabelYes = builder.AddDynamicValueTextParameter(IdentifierDynamicValueTextParameterLabelYes, "Label: Yes");
        builderData.AddTextParameter(IdentifierDynamicValueTextParameterLabelYes, "Yes");

        TableSchemaDynamicValueTextParameter tableDynamicValueTextParameterLabelNo = builder.AddDynamicValueTextParameter(IdentifierDynamicValueTextParameterLabelNo, "Label: No");
        builderData.AddTextParameter(IdentifierDynamicValueTextParameterLabelNo, "No");

        // Add Properties
        TableSchemaPropertyString tablePropertyStringCaseNumber = builder.AddPropertyString(IdentifierPropertyStringCaseNumber, "Case number", textPropertyStringCaseNumber);
        TableSchemaPropertyBoolean tablePropertyBooleanIsCompleted = builder.AddPropertyBoolean(IdentifierPropertyBooleanCompleted, "Is completed?", textPropertyBooleanIsCompleted);
        TableSchemaPropertyDateTime tablePropertyDateTimeCreatedAt = builder.AddPropertyDateTime(IdentifierPropertyDateTimeCreated, "Created at", textPropertyDateTimeCreatedAt);

        // Add CaseNumber ActionInput
        TableSchemaActionInput tableActionInputCaseNumber = builder.AddActionInput(IdentifierActionInputCaseNumber, tableSchemaInputText, "Case number", textActionInputCaseNumber);
        builder.Relate(tableActionInputCaseNumber, tableSchemaAttributeIntegerMinLength, 8);
        builder.Relate(tableActionInputCaseNumber, tableSchemaAttributeIntegerMaxLength, 12);
        
        // Add IsCompleted ActionInput
        TableSchemaActionInput tableActionInputIsCompleted = builder.AddActionInput(IdentifierActionInputIsCompleted, tableSchemaInputSingleChoice, "Is completed?", textActionInputIsCompleted);
        builder.Relate(tableActionInputIsCompleted, tableSchemaAttributeDynamicValueTextParameterLabelMultiple, tableDynamicValueTextParameterLabelYes);
        builder.Relate(tableActionInputIsCompleted, tableSchemaAttributeDynamicValueTextParameterLabelMultiple, tableDynamicValueTextParameterLabelNo);

        // Add CreatedAt ActionInput
        TableSchemaActionInput tableActionInputCreatedAt = builder.AddActionInput(IdentifierActionInputCreatedAt, tableSchemaInputDate, "Created at", textActionInputCreatedAt);
        builder.Relate(tableActionInputCreatedAt, tableSchemaAttributeIntegerDynamicMinDate, tableDynamicValueCodeCurrentDateAsHTMLValue);
        builder.Relate(tableActionInputCreatedAt, tableSchemaAttributeIntegerDynamicMaxDate, tableDynamicValueCodeCurrentDateNextYearAsHTMLValue);

        // Add EnterCaseData ActionStep
        TableSchemaActionStep tableActionStepEnterCaseData = builder.AddActionStep(IdentifierActionCreateCaseActionStepAddData, "Enter case data", textActionStepEnterCaseData);
        builder.Relate(tableActionStepEnterCaseData, tableActionInputIsCompleted);
        builder.Relate(tableActionStepEnterCaseData, tableActionInputCaseNumber);
        builder.Relate(tableActionStepEnterCaseData, tableActionInputCreatedAt);

        // Add CreateCase Action
        TableSchemaAction tableActionCreateCase = builder.AddAction(IdentifierActionCreateCase,"Create case", textActionCreateCase, "Context specific Action for creating a Case Element");
        builder.Relate(tableActionCreateCase, tableDynamicValueCodeCreateCase);
        builder.Relate(tableActionCreateCase, tableActionStepEnterCaseData);

        // Add UpdateCase Action
        TableSchemaAction tableActionUpdateCase = builder.AddAction(IdentifierActionUpdateCase,"Update case", textActionUpdateCase, "Case Element specific Action for updating a Case Element");
        builder.Relate(tableActionUpdateCase, tableDynamicValueCodeUpdateCase);
        builder.Relate(tableActionUpdateCase, tableActionStepEnterCaseData);
        
        // Add case element
        TableSchemaElement tableElementCase = builder.AddElement(IdentifierElementCase, "Case", textElementCase);
        builder.Relate(tableElementCase, tablePropertyStringCaseNumber);
        builder.Relate(tableElementCase, tablePropertyBooleanIsCompleted);
        builder.Relate(tableElementCase, tablePropertyDateTimeCreatedAt);

        // Add journal element
        TableSchemaElement tableElementJournal = builder.AddElement(IdentifierElementJournal, "Journal", textElementJournal);

        // Add case context
        TableSchemaContext tableContextCase = builder.AddContext(IdentifierContextCase, "Case context", textContextCaseManagement);
        builder.Relate(tableContextCase, tableActionCreateCase);
        builder.Relate(tableContextCase, tableElementCase);
        builder.Relate(tableContextCase, tableElementJournal);

        try
        {
            await context.SaveChangesAsync();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }
}
