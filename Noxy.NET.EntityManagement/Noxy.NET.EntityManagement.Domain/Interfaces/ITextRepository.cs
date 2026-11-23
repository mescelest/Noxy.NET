using Noxy.NET.EntityManagement.Domain.Entities;

namespace Noxy.NET.EntityManagement.Domain.Interfaces;

// Stores the values of the text parameters, one for each text and culture. Like the rest of the data, nothing is changed in place: a change ends the row, which moves to the history, and a new row takes its place.
// Every call that writes does all of its work or none of it. The repository does not check that the text exists in the schema; that is for the layer above.
public interface ITextRepository
{
    Task<DataTextParameter?> FindText(string textIdentifier, string culture, CancellationToken cancellationToken = default);

    // Every text that has a value in the culture, by identifier. This is what an application loads to show itself in that language.
    Task<List<DataTextParameter>> GetTextList(string culture, CancellationToken cancellationToken = default);

    // Every culture a text has a value in, by culture. This is what is edited when a text is translated.
    Task<List<DataTextParameter>> GetTextListByIdentifier(string textIdentifier, CancellationToken cancellationToken = default);

    // The new row becomes the value of that text in that culture. The row it replaces, if there is one, moves to the history, ended by the user and at the time of the new row.
    Task<DataTextParameter> SetText(DataTextParameter text, CancellationToken cancellationToken = default);

    // Ends the value without a replacement. False when the text has no value in the culture.
    Task<bool> ClearText(string textIdentifier, string culture, Guid endedByUserID, CancellationToken cancellationToken = default);

    // The values the text has had in the culture, in the order they ended.
    Task<List<HistoryTextParameter>> GetTextHistoryList(string textIdentifier, string culture, CancellationToken cancellationToken = default);
}
