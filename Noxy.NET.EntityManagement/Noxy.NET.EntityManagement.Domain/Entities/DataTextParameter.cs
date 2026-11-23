using Noxy.NET.EntityManagement.Domain.Abstractions;

namespace Noxy.NET.EntityManagement.Domain.Entities;

// What a text parameter says in one culture. The text parameter itself (its identifier and its kind) and its defaults are written in code; only the values people have edited are data, so a language can be added, and a text edited, without a deployment.
// The database only holds what a person has edited, so a text shows its default until somebody changes it, and a newer default in the code reaches everybody who has not edited it.
public sealed class DataTextParameter : BaseData
{
    // Long enough for any language tag in use (BCP 47, such as "en", "da-DK" or "zh-Hans-CN").
    public const int CultureMaxLength = 35;

    // The identifier of the text parameter in the schema.
    public required string TextIdentifier { get; init; }

    // The name of the culture, such as "en" or "da-DK". It is compared without regard to case.
    public required string Culture { get; init; }

    public required string Value { get; init; }

    public HistoryTextParameter ToHistory(Guid endedByUserID, DateTime timeEnded)
    {
        return new()
        {
            ID = ID,
            TimeCreated = TimeCreated,
            CreatedByUserID = CreatedByUserID,
            TextIdentifier = TextIdentifier,
            Culture = Culture,
            Value = Value,
            TimeEnded = timeEnded,
            EndedByUserID = endedByUserID
        };
    }
}

public sealed class HistoryTextParameter : BaseHistory
{
    public string TextIdentifier { get; init; } = string.Empty;
    public string Culture { get; init; } = string.Empty;
    public string Value { get; init; } = string.Empty;
}
