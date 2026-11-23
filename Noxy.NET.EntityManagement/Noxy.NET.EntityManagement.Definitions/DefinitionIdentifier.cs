using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace Noxy.NET.EntityManagement.Definitions;

// The identifier is what is stored with the data, so it is kept plain: lower case letters and digits, separated by single hyphens or dots ("person", "person-involved-in-case", "acme.weapon").
public static partial class DefinitionIdentifier
{
    public const int MaxLength = 100;

    public static bool IsValid([NotNullWhen(true)] string? identifier)
    {
        return !string.IsNullOrEmpty(identifier) && identifier.Length <= MaxLength && Pattern().IsMatch(identifier);
    }

    [GeneratedRegex("^[a-z0-9]+(?:[.-][a-z0-9]+)*$")]
    private static partial Regex Pattern();
}
