using Noxy.NET.EntityManagement.Application.Interfaces;
using Noxy.NET.EntityManagement.Definitions;
using Noxy.NET.EntityManagement.Definitions.Enums;
using Noxy.NET.EntityManagement.Definitions.Models;
using Noxy.NET.EntityManagement.Domain.Abstractions;
using Noxy.NET.EntityManagement.Domain.Entities;
using Noxy.NET.EntityManagement.Domain.Interfaces;

namespace Noxy.NET.EntityManagement.Application.Services;

public sealed class TextService(DefinitionRegistry definition, DefinitionTextRegistry defaults, ITextRepository repository, TimeProvider timeProvider) : ITextService
{
    public async Task<string> GetText(string textIdentifier, string? culture, CancellationToken cancellationToken = default)
    {
        definition.GetTextParameter(textIdentifier);

        IReadOnlyList<string> chain = TextCulture.GetChain(culture, defaults.FallbackCulture);
        List<DataTextParameter> editList = await repository.GetTextListByIdentifier(textIdentifier, cancellationToken);

        foreach (string candidate in chain)
        {
            DataTextParameter? edit = editList.FirstOrDefault(x => string.Equals(x.Culture, candidate, StringComparison.OrdinalIgnoreCase));
            if (edit is not null) return edit.Value;

            if (defaults.TryGetText(textIdentifier, candidate, out DefinitionText? text)) return text.Value;
        }

        // The fallback culture has a default for every text parameter: that was checked when the defaults were built.
        throw new InvalidOperationException($"The text '{textIdentifier}' has no text in any of: {string.Join(", ", chain)}.");
    }

    public async Task<IReadOnlyDictionary<string, string>> GetTextMap(string? culture, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<string> chain = TextCulture.GetChain(culture, defaults.FallbackCulture);

        // One query for each culture of the chain, not one for each text.
        List<Dictionary<string, string>> editMapList = [];
        foreach (string candidate in chain)
        {
            editMapList.Add((await repository.GetTextList(candidate, cancellationToken)).ToDictionary(x => x.TextIdentifier, x => x.Value));
        }

        Dictionary<string, string> result = new(definition.TextParameterList.Count);
        foreach (DefinitionTextParameter textParameter in definition.TextParameterList)
        {
            for (int i = 0; i < chain.Count; i++)
            {
                if (editMapList[i].TryGetValue(textParameter.Identifier, out string? edit))
                {
                    result[textParameter.Identifier] = edit;
                    break;
                }

                if (defaults.TryGetText(textParameter.Identifier, chain[i], out DefinitionText? text))
                {
                    result[textParameter.Identifier] = text.Value;
                    break;
                }
            }
        }

        return result;
    }

    public async Task<DataTextParameter> SetText(string textIdentifier, string culture, string value, Guid userID, CancellationToken cancellationToken = default)
    {
        DefinitionTextParameter textParameter = definition.GetTextParameter(textIdentifier);

        if (!DefinitionCulture.IsValid(culture))
        {
            throw new ArgumentException($"'{culture}' is not a language tag such as 'en' or 'da-DK'.", nameof(culture));
        }

        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("A text cannot be empty. To go back to the default, reset the text.", nameof(value));
        }

        if (value.Length > BaseData.TextMaxLength)
        {
            throw new ArgumentException($"A text can be at most {BaseData.TextMaxLength} characters.", nameof(value));
        }

        if (textParameter.Kind == TextKindEnum.Line && value.AsSpan().ContainsAny('\r', '\n'))
        {
            throw new ArgumentException($"The text '{textIdentifier}' is a line of text and cannot hold a line break.", nameof(value));
        }

        // The same text again is not a change, so it is not a row in the history.
        DataTextParameter? current = await repository.FindText(textIdentifier, culture, cancellationToken);
        if (current is not null && current.Value == value) return current;

        return await repository.SetText(new() { TextIdentifier = textIdentifier, Culture = culture, Value = value, CreatedByUserID = userID, TimeCreated = timeProvider.GetUtcNow().UtcDateTime }, cancellationToken);
    }

    public Task<bool> ResetText(string textIdentifier, string culture, Guid userID, CancellationToken cancellationToken = default)
    {
        definition.GetTextParameter(textIdentifier);

        return repository.ClearText(textIdentifier, culture, userID, cancellationToken);
    }
}
