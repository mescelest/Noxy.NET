using Microsoft.EntityFrameworkCore;
using Noxy.NET.EntityManagement.Database.Builders;
using Noxy.NET.EntityManagement.Persistence;
using Noxy.NET.EntityManagement.Persistence.Tables.Schemas;

namespace Noxy.NET.EntityManagement.Database.Seeds;

public class BaseSchemaSeed(DataContext context)
{
    public const string IdentifierContextCase = "ContextCase";

    public const string IdentifierDynamicValueTextParameterLabelYes = "TextParameterLabelYes";
    public const string IdentifierDynamicValueTextParameterLabelNo = "TextParameterLabelNo";

    public const string IdentifierPropertyStringCaseNumber = "PropertyStringCaseNumber";
    public const string IdentifierPropertyBooleanCompleted = "PropertyBooleanCompleted";
    public const string IdentifierPropertyDateTimeCreated = "PropertyDateTimeCreated";

    public const string IdentifierElementCase = "ElementCase";
    public const string IdentifierElementJournal = "ElementJournal";

    public async Task Apply()
    {
        TableSchema tableSchema = await context.Schema.FirstAsync();
        SchemaSeedBuilder builder = new(context, tableSchema);
        DataSeedBuilder builderData = new(context);

        TableSchemaParameterText parameterTextPropertyBooleanIsCompleted = await context.SchemaParameterText.FirstAsync(x => x.SchemaIdentifier == BaseTextSeed.PropertyBooleanIsCompleted);
        TableSchemaParameterText parameterTextPropertyDateTimeCreatedAt = await context.SchemaParameterText.FirstAsync(x => x.SchemaIdentifier == BaseTextSeed.PropertyDateTimeCreatedAt);
        TableSchemaParameterText parameterTextPropertyStringCaseNumber = await context.SchemaParameterText.FirstAsync(x => x.SchemaIdentifier == BaseTextSeed.PropertyStringCaseNumber);

        TableSchemaParameterText parameterTextElementCase = await context.SchemaParameterText.FirstAsync(x => x.SchemaIdentifier == BaseTextSeed.ElementCase);
        TableSchemaParameterText parameterTextElementJournal = await context.SchemaParameterText.FirstAsync(x => x.SchemaIdentifier == BaseTextSeed.ElementJournal);

        TableSchemaParameterText parameterTextContextCaseManagement = await context.SchemaParameterText.FirstAsync(x => x.SchemaIdentifier == BaseTextSeed.ContextCaseManagement);

        // Add Text Parameters
        builder.AddDynamicValueTextParameter(IdentifierDynamicValueTextParameterLabelYes, "Label: Yes");
        builderData.AddTextParameter(IdentifierDynamicValueTextParameterLabelYes, "Yes");

        builder.AddDynamicValueTextParameter(IdentifierDynamicValueTextParameterLabelNo, "Label: No");
        builderData.AddTextParameter(IdentifierDynamicValueTextParameterLabelNo, "No");

        // Add Properties
        TableSchemaPropertyBoolean tablePropertyBooleanIsCompleted = builder.AddPropertyBoolean(IdentifierPropertyBooleanCompleted, "Is completed?", parameterTextPropertyBooleanIsCompleted);
        TableSchemaPropertyDateTime tablePropertyDateTimeCreatedAt = builder.AddPropertyDateTime(IdentifierPropertyDateTimeCreated, "Created at", parameterTextPropertyStringCaseNumber);
        TableSchemaPropertyString tablePropertyStringCaseNumber = builder.AddPropertyString(IdentifierPropertyStringCaseNumber, "Case number", parameterTextPropertyDateTimeCreatedAt);

        // Add case element
        TableSchemaElement tableElementCase = builder.AddElement(IdentifierElementCase, "Case", parameterTextElementCase);
        builder.Relate(tableElementCase, tablePropertyStringCaseNumber);
        builder.Relate(tableElementCase, tablePropertyBooleanIsCompleted);
        builder.Relate(tableElementCase, tablePropertyDateTimeCreatedAt);

        // Add journal element
        TableSchemaElement tableElementJournal = builder.AddElement(IdentifierElementJournal, "Journal", parameterTextElementJournal);

        // Add case context
        TableSchemaContext tableContextCase = builder.AddContext(IdentifierContextCase, "Case context", parameterTextContextCaseManagement);
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
