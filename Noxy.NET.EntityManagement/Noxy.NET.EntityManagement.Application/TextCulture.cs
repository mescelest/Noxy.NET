using Noxy.NET.EntityManagement.Definitions;

namespace Noxy.NET.EntityManagement.Application;

public static class TextCulture
{
    // The cultures to look in for a text, from the one that was asked for to the fallback: "da-DK", then "da", then the fallback ("en").
    // A culture that is missing or is not a language tag is not an error, because it comes from outside (a browser can send anything): the fallback is used.
    public static IReadOnlyList<string> GetChain(string? culture, string fallbackCulture)
    {
        List<string> chain = [];

        if (DefinitionCulture.IsValid(culture))
        {
            string current = culture;
            while (true)
            {
                Add(chain, current);

                int separator = current.LastIndexOf('-');
                if (separator < 0) break;

                current = current[..separator];
            }
        }

        Add(chain, fallbackCulture);
        return chain;
    }

    private static void Add(List<string> chain, string culture)
    {
        if (!chain.Contains(culture, StringComparer.OrdinalIgnoreCase))
        {
            chain.Add(culture);
        }
    }
}
