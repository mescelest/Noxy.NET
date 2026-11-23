using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace Noxy.NET.EntityManagement.Definitions;

// The name of a culture is a language tag such as "en", "da-DK" or "zh-Hans-CN". It is compared without regard to case, so "da-DK" and "da-dk" are one culture.
public static partial class DefinitionCulture
{
    // The same as the longest culture the database holds (a test keeps the two equal).
    public const int MaxLength = 35;

    public static bool IsValid([NotNullWhen(true)] string? culture)
    {
        return !string.IsNullOrEmpty(culture) && culture.Length <= MaxLength && Pattern().IsMatch(culture);
    }

    // The language is two or three letters (so "english" is caught as the typo it is), and each part after it is one to eight letters or digits.
    [GeneratedRegex("^[A-Za-z]{2,3}(?:-[A-Za-z0-9]{1,8})*$")]
    private static partial Regex Pattern();
}
