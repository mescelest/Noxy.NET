using Noxy.NET.EntityManagement.Domain.Entities;

namespace Noxy.NET.EntityManagement.Application.Interfaces;

// Gives the texts of the system to whoever shows them, in the language of the person who looks.
// A text is what a person has edited in the database, or else what the code says (the defaults). The database holds only what people have edited, so a text shows its default until somebody changes it.
// The culture is looked for first, then its language ("da-DK", then "da"), then the fallback culture, and in each of them an edit comes before the default.
public interface ITextService
{
    // One text. A culture that is missing or invalid is not an error: the fallback is used. A text that is not defined is (KeyNotFoundException), because it is a mistake of the code, not of the person who looks.
    Task<string> GetText(string textIdentifier, string? culture, CancellationToken cancellationToken = default);

    // Every text, by identifier, for an application to load at once.
    Task<IReadOnlyDictionary<string, string>> GetTextMap(string? culture, CancellationToken cancellationToken = default);

    // Edits a text in a culture, for any culture, also one that has no default. The previous edit moves to the history. A line of text cannot hold a line break. To go back to the default, use ResetText.
    Task<DataTextParameter> SetText(string textIdentifier, string culture, string value, Guid userID, CancellationToken cancellationToken = default);

    // Removes the edit of a text in a culture, so it shows its default again. False when there was no edit.
    Task<bool> ResetText(string textIdentifier, string culture, Guid userID, CancellationToken cancellationToken = default);
}
