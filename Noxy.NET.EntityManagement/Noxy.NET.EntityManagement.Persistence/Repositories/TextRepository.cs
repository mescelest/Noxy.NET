using Microsoft.EntityFrameworkCore;
using Noxy.NET.EntityManagement.Domain.Entities;
using Noxy.NET.EntityManagement.Domain.Interfaces;

namespace Noxy.NET.EntityManagement.Persistence.Repositories;

// Each write is one SaveChanges, which is one transaction: the row that ends and the row that replaces it are moved and added together, or not at all.
public sealed class TextRepository(EntityManagementContext context, TimeProvider timeProvider) : ITextRepository
{
    public Task<DataTextParameter?> FindText(string textIdentifier, string culture, CancellationToken cancellationToken = default)
    {
        ThrowIfBlank(textIdentifier, culture);

        return context.DataTextParameter.AsNoTracking().SingleOrDefaultAsync(x => x.TextIdentifier == textIdentifier && x.Culture == culture, cancellationToken);
    }

    public Task<List<DataTextParameter>> GetTextList(string culture, CancellationToken cancellationToken = default)
    {
        ThrowIfBlank(culture);

        return context.DataTextParameter.AsNoTracking()
            .Where(x => x.Culture == culture)
            .OrderBy(x => x.TextIdentifier)
            .ToListAsync(cancellationToken);
    }

    public Task<List<DataTextParameter>> GetTextListByIdentifier(string textIdentifier, CancellationToken cancellationToken = default)
    {
        ThrowIfBlank(textIdentifier);

        return context.DataTextParameter.AsNoTracking()
            .Where(x => x.TextIdentifier == textIdentifier)
            .OrderBy(x => x.Culture)
            .ToListAsync(cancellationToken);
    }

    public async Task<DataTextParameter> SetText(DataTextParameter text, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(text);
        ThrowIfBlank(text.TextIdentifier, text.Culture);

        DataTextParameter? current = await context.DataTextParameter.SingleOrDefaultAsync(x => x.TextIdentifier == text.TextIdentifier && x.Culture == text.Culture, cancellationToken);
        if (current is not null)
        {
            // The new row takes over at the moment it was made, so there is no gap and no overlap between the two.
            context.Add(current.ToHistory(text.CreatedByUserID, text.TimeCreated));
            context.Remove(current);
        }

        context.Add(text);
        await context.SaveChangesAsync(cancellationToken);

        return text;
    }

    public async Task<bool> ClearText(string textIdentifier, string culture, Guid endedByUserID, CancellationToken cancellationToken = default)
    {
        ThrowIfBlank(textIdentifier, culture);

        DataTextParameter? current = await context.DataTextParameter.SingleOrDefaultAsync(x => x.TextIdentifier == textIdentifier && x.Culture == culture, cancellationToken);
        if (current is null) return false;

        context.Add(current.ToHistory(endedByUserID, timeProvider.GetUtcNow().UtcDateTime));
        context.Remove(current);

        await context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public Task<List<HistoryTextParameter>> GetTextHistoryList(string textIdentifier, string culture, CancellationToken cancellationToken = default)
    {
        ThrowIfBlank(textIdentifier, culture);

        return context.HistoryTextParameter.AsNoTracking()
            .Where(x => x.TextIdentifier == textIdentifier && x.Culture == culture)
            .OrderBy(x => x.TimeEnded).ThenBy(x => x.TimeCreated)
            .ToListAsync(cancellationToken);
    }

    private static void ThrowIfBlank(params string[] valueList)
    {
        foreach (string value in valueList)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(value);
        }
    }
}
