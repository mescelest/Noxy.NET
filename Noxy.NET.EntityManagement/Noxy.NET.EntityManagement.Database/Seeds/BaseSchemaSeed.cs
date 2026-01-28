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

        TableSchemaDynamicValueTextParameter textPropertyBooleanIsCompleted = await context.SchemaDynamicValueTextParameter.FirstAsync(x => x.SchemaIdentifier == BaseTextSeed.PropertyBooleanIsCompleted);
        TableSchemaDynamicValueTextParameter textPropertyDateTimeCreatedAt = await context.SchemaDynamicValueTextParameter.FirstAsync(x => x.SchemaIdentifier == BaseTextSeed.PropertyDateTimeCreatedAt);
        TableSchemaDynamicValueTextParameter textPropertyStringCaseNumber = await context.SchemaDynamicValueTextParameter.FirstAsync(x => x.SchemaIdentifier == BaseTextSeed.PropertyStringCaseNumber);

        TableSchemaDynamicValueTextParameter textElementCase = await context.SchemaDynamicValueTextParameter.FirstAsync(x => x.SchemaIdentifier == BaseTextSeed.ElementCase);
        TableSchemaDynamicValueTextParameter textElementJournal = await context.SchemaDynamicValueTextParameter.FirstAsync(x => x.SchemaIdentifier == BaseTextSeed.ElementJournal);

        TableSchemaDynamicValueTextParameter textContextCaseManagement = await context.SchemaDynamicValueTextParameter.FirstAsync(x => x.SchemaIdentifier == BaseTextSeed.ContextCaseManagement);

        // Add Text Parameters
        builder.AddDynamicValueTextParameter(IdentifierDynamicValueTextParameterLabelYes, "Label: Yes");
        builderData.AddTextParameter(IdentifierDynamicValueTextParameterLabelYes, "Yes");

        builder.AddDynamicValueTextParameter(IdentifierDynamicValueTextParameterLabelNo, "Label: No");
        builderData.AddTextParameter(IdentifierDynamicValueTextParameterLabelNo, "No");

        // Add Properties
        TableSchemaPropertyString tablePropertyStringCaseNumber = builder.AddPropertyString(IdentifierPropertyStringCaseNumber, "Case number", textPropertyStringCaseNumber);
        TableSchemaPropertyBoolean tablePropertyBooleanIsCompleted = builder.AddPropertyBoolean(IdentifierPropertyBooleanCompleted, "Is completed?", textPropertyBooleanIsCompleted);
        TableSchemaPropertyDateTime tablePropertyDateTimeCreatedAt = builder.AddPropertyDateTime(IdentifierPropertyDateTimeCreated, "Created at", textPropertyDateTimeCreatedAt);

        // Add case element
        TableSchemaElement tableElementCase = builder.AddElement(IdentifierElementCase, "Case", textElementCase);
        builder.Relate(tableElementCase, tablePropertyStringCaseNumber);
        builder.Relate(tableElementCase, tablePropertyBooleanIsCompleted);
        builder.Relate(tableElementCase, tablePropertyDateTimeCreatedAt);

        // Add journal element
        TableSchemaElement tableElementJournal = builder.AddElement(IdentifierElementJournal, "Journal", textElementJournal);

        // Add case context
        TableSchemaContext tableContextCase = builder.AddContext(IdentifierContextCase, "Case context", textContextCaseManagement);
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
