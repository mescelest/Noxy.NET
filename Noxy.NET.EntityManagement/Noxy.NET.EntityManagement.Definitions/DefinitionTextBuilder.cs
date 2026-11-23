using Noxy.NET.EntityManagement.Definitions.Interfaces;
using Noxy.NET.EntityManagement.Definitions.Models;

namespace Noxy.NET.EntityManagement.Definitions;

public sealed class DefinitionTextBuilder : IDefinitionTextBuilder
{
    // The culture a text falls back to when it has nothing in the culture that is asked for.
    public const string DefaultFallbackCulture = "en";

    private readonly List<DefinitionText> _textList = [];

    // Runs the files of texts (the project's own, and a base system's if there is one) and builds them as one, against the definition.
    public static DefinitionTextRegistry Create(DefinitionRegistry definition, params IDefinitionTexts[] textsList)
    {
        return Create(definition, DefaultFallbackCulture, textsList);
    }

    public static DefinitionTextRegistry Create(DefinitionRegistry definition, string fallbackCulture, params IDefinitionTexts[] textsList)
    {
        DefinitionTextBuilder builder = new();
        foreach (IDefinitionTexts texts in textsList)
        {
            builder.Add(texts);
        }

        return builder.Build(definition, fallbackCulture);
    }

    public DefinitionTextBuilder Add(IDefinitionTexts texts)
    {
        ArgumentNullException.ThrowIfNull(texts);

        texts.Define(this);
        return this;
    }

    public IDefinitionTextBuilder AddText(string textIdentifier, string culture, string value)
    {
        _textList.Add(new(textIdentifier, culture, value));
        return this;
    }

    // Checks every text against the definition and returns the read-only registry, or throws with every problem found.
    // Every text parameter of the definition must have a default in the fallback culture, so there is always something to show.
    public DefinitionTextRegistry Build(DefinitionRegistry definition, string fallbackCulture = DefaultFallbackCulture)
    {
        ArgumentNullException.ThrowIfNull(definition);

        List<string> errorList = [];

        if (!DefinitionCulture.IsValid(fallbackCulture))
        {
            errorList.Add($"The fallback culture '{fallbackCulture}' is not valid.");
        }

        List<DefinitionText> textList = [];
        HashSet<(string TextIdentifier, string Culture)> seen = [];

        foreach (DefinitionText text in _textList)
        {
            string textIdentifier = text.TextIdentifier;
            string culture = text.Culture;
            string owner = $"Text '{textIdentifier}' ({culture})";

            if (!definition.TryGetTextParameter(textIdentifier, out _))
            {
                errorList.Add($"{owner}: the text parameter is not defined.");
            }

            if (!DefinitionCulture.IsValid(culture))
            {
                errorList.Add($"{owner}: the culture is not valid (a language tag such as 'en' or 'da-DK', at most {DefinitionCulture.MaxLength} characters).");
            }

            if (string.IsNullOrWhiteSpace(text.Value))
            {
                errorList.Add($"{owner}: the text is empty.");
            }

            if (!seen.Add((textIdentifier, culture.ToUpperInvariant())))
            {
                errorList.Add($"{owner}: defined more than once.");
                continue;
            }

            textList.Add(text);
        }

        if (DefinitionCulture.IsValid(fallbackCulture))
        {
            foreach (DefinitionTextParameter textParameter in definition.TextParameterList)
            {
                if (!seen.Contains((textParameter.Identifier, fallbackCulture.ToUpperInvariant())))
                {
                    errorList.Add($"Text parameter '{textParameter.Identifier}': there is no text in the fallback culture '{fallbackCulture}'.");
                }
            }
        }

        if (errorList.Count > 0)
        {
            throw new DefinitionException(errorList);
        }

        return new(fallbackCulture, textList);
    }
}
