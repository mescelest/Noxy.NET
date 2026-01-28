using Microsoft.EntityFrameworkCore;
using Noxy.NET.CaseManagement.Domain.Constants;
using Noxy.NET.CaseManagement.Persistence.Abstractions;
using Noxy.NET.CaseManagement.Persistence.Tables.Schemas;

namespace Noxy.NET.CaseManagement.Persistence.Seeds;

public class SchemaSeed(ModelBuilder builder, TableSchema refSchema) : BaseSeed(builder, refSchema)
{
    public void Apply()
    {
        HasTableSchemaInput("01974e8c-ecb8-75ab-9070-f0fed39b9c54", SchemaActionInputConstants.SchemaIdentifierCheckbox, "Checkbox", note: "The base checkbox input type.");
        HasTableSchemaInput("01974e8c-ecb8-75ab-9070-f625218aed76", SchemaActionInputConstants.SchemaIdentifierDate, "Date input", note: "The base date input type.");
        HasTableSchemaInput("019765f7-2847-757d-8d6f-e48dbec5bbeb", SchemaActionInputConstants.SchemaIdentifierDecimal, "Decimal input", note: "The base decimal input type.");
        HasTableSchemaInput("019765f7-2847-757d-8d6f-ea72d6453f86", SchemaActionInputConstants.SchemaIdentifierInteger, "Integer input", note: "The base integer input type.");
        HasTableSchemaInput("019765f7-2847-757d-8d6f-ec49fff67113", SchemaActionInputConstants.SchemaIdentifierPassword, "Password input", note: "The base password input type.");
        HasTableSchemaInput("019765f7-2847-757d-8d6f-ec49fff67fa1", SchemaActionInputConstants.SchemaIdentifierSingleChoice, "Single choice input", note: "The base single choice input type.");
        HasTableSchemaInput("01974e8c-ecb8-75ab-9070-f8a855e17dda", SchemaActionInputConstants.SchemaIdentifierText, "Text input", note: "The base text input type.");
    }
}
