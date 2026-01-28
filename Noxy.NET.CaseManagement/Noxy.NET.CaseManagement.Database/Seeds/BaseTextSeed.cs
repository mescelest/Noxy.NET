using Microsoft.EntityFrameworkCore;
using Noxy.NET.CaseManagement.Database.Builders;
using Noxy.NET.CaseManagement.Persistence;
using Noxy.NET.CaseManagement.Persistence.Tables.Schemas;

namespace Noxy.NET.CaseManagement.Database.Seeds;

public class BaseTextSeed(DataContext context)
{
    public const string ActionPanelHeader = "ActionPanelHeader";

    public const string ActionCreateCase = "ActionCreateCase";
    public const string ActionUpdateCase = "ActionUpdateCase";

    public const string ActionStepAddCaseData = "ActionStepAddCaseData";

    public const string ActionInputCaseNumber = "ActionInputCaseNumber";
    public const string ActionInputIsCompleted = "ActionInputIsCompleted";
    public const string ActionInputCreatedAt = "ActionInputCreatedAt";

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

        builder.AddDynamicValueTextParameter(ActionCreateCase, "[Action] Create case");
        builderData.AddTextParameter(ActionCreateCase, "Create case");
        builder.AddDynamicValueTextParameter(ActionUpdateCase, "[Action] Update case");
        builderData.AddTextParameter(ActionUpdateCase, "Update case");

        builder.AddDynamicValueTextParameter(ActionStepAddCaseData, "[Action Step] Add case data");
        builderData.AddTextParameter(ActionStepAddCaseData, "Add case data");

        builder.AddDynamicValueTextParameter(ActionInputCaseNumber, "[Action Input] Case number");
        builder.AddDynamicValueTextParameter(ActionInputIsCompleted, "[Action Input] Is completed?");
        builder.AddDynamicValueTextParameter(ActionInputCreatedAt, "[Action Input] Created at");
        builderData.AddTextParameter(ActionInputCaseNumber, "Case number");
        builderData.AddTextParameter(ActionInputIsCompleted, "Is completed?");
        builderData.AddTextParameter(ActionInputCreatedAt, "Created at");

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

        builder.AddDynamicValueTextParameter(ActionPanelHeader, "ActionPanel header");
        builderData.AddTextParameter(ActionPanelHeader, "Actions");

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
