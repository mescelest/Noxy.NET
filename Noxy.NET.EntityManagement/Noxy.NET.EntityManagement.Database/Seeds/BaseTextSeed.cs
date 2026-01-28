using Microsoft.EntityFrameworkCore;
using Noxy.NET.EntityManagement.Database.Builders;
using Noxy.NET.EntityManagement.Persistence;
using Noxy.NET.EntityManagement.Persistence.Tables.Schemas;

namespace Noxy.NET.EntityManagement.Database.Seeds;

public class BaseTextSeed(DataContext context)
{
    public const string PropertyBooleanIsCompleted = "PropertyBooleanIsCompleted";
    public const string PropertyDateTimeCreatedAt = "PropertyDateTimeCreatedAt";
    public const string PropertyStringCaseNumber = "PropertyStringCaseNumber";

    public const string ElementCase = "ElementCase";
    public const string ElementJournal = "ElementJournal";

    public const string ContextCaseManagement = "ContextCaseManagement";

    public async Task Apply()
    {
        TableSchema tableSchema = await context.Schema.FirstAsync();
        SchemaSeedBuilder builder = new(context, tableSchema);
        DataSeedBuilder builderData = new(context);

        builder.AddDynamicValueTextParameter(PropertyBooleanIsCompleted, "[Property: Boolean] Is completed?");
        builder.AddDynamicValueTextParameter(PropertyDateTimeCreatedAt, "[Property: DateTime] Created at");
        builder.AddDynamicValueTextParameter(PropertyStringCaseNumber, "[Property: String] Case number");
        builderData.AddTextParameter(PropertyBooleanIsCompleted, "Is completed?");
        builderData.AddTextParameter(PropertyDateTimeCreatedAt, "Created at");
        builderData.AddTextParameter(PropertyStringCaseNumber, "Case number");

        builder.AddDynamicValueTextParameter(ElementCase, "[Element] Case");
        builderData.AddTextParameter(ElementCase, "Case");
        builder.AddDynamicValueTextParameter(ElementJournal, "[Element] Journal");
        builderData.AddTextParameter(ElementJournal, "Journal");

        builder.AddDynamicValueTextParameter(ContextCaseManagement, "[Context] Case management");
        builderData.AddTextParameter(ContextCaseManagement, "Case management");

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
