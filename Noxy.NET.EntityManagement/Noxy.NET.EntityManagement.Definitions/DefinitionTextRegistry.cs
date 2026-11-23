using System.Diagnostics.CodeAnalysis;
using Noxy.NET.EntityManagement.Definitions.Models;

namespace Noxy.NET.EntityManagement.Definitions;

// The checked and read-only defaults of the texts, in every language the system ships. Every one is of a text parameter that exists, and every text parameter has one in the fallback culture.
public sealed class DefinitionTextRegistry
{
    private readonly Dictionary<(string TextIdentifier, string Culture), DefinitionText> _textMap;

    // The culture a text falls back to when it has nothing in the culture that is asked for.
    public string FallbackCulture { get; }

    public IReadOnlyList<DefinitionText> TextList { get; }

    // The cultures that have at least one text, in the order they were first used.
    public IReadOnlyList<string> CultureList { get; }

    internal DefinitionTextRegistry(string fallbackCulture, List<DefinitionText> textList)
    {
        FallbackCulture = fallbackCulture;
        TextList = textList.AsReadOnly();
        CultureList = textList.Select(x => x.Culture).Distinct(StringComparer.OrdinalIgnoreCase).ToList().AsReadOnly();

        _textMap = textList.ToDictionary(x => (x.TextIdentifier, x.Culture.ToUpperInvariant()));
    }

    // The default of a text in exactly that culture. Falling back to another culture is for whoever shows the text, because what a person has edited comes first.
    public bool TryGetText(string textIdentifier, string culture, [NotNullWhen(true)] out DefinitionText? text)
    {
        return _textMap.TryGetValue((textIdentifier, culture.ToUpperInvariant()), out text);
    }

    public IReadOnlyList<DefinitionText> GetTextList(string culture)
    {
        return TextList.Where(x => string.Equals(x.Culture, culture, StringComparison.OrdinalIgnoreCase)).ToList().AsReadOnly();
    }
}
