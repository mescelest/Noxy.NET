using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Noxy.NET.EntityManagement.Domain.Entities;

namespace Noxy.NET.EntityManagement.Persistence.Repositories;

// What an element and a relation have in common: both own properties. The two repositories share how those are changed and ended.
internal static class OwnerData
{
    public static void ThrowIfNotOfOwner(Guid ownerID, IEnumerable<Guid> ownerOfList, IEnumerable<string> identifierList)
    {
        if (ownerOfList.Any(x => x != ownerID))
        {
            throw new ArgumentException("Every value must be of the owner.");
        }

        string? repeated = identifierList.GroupBy(x => x).FirstOrDefault(x => x.Count() > 1)?.Key;
        if (repeated is not null)
        {
            throw new ArgumentException($"The property '{repeated}' is there more than once.");
        }
    }

    // Sets and clears several properties of an owner in one transaction, together with the check that the owner exists, so it cannot end in between. False when it does not exist.
    public static async Task<bool> ChangePropertiesAsync(
        EntityManagementContext context,
        TimeProvider timeProvider,
        Guid ownerID,
        Func<CancellationToken, Task<bool>> ownerExists,
        IReadOnlyList<DataProperty> setList,
        IReadOnlyList<string> clearList,
        Guid endedByUserID,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(setList);
        ArgumentNullException.ThrowIfNull(clearList);

        List<string> identifierList = [.. setList.Select(x => x.PropertyIdentifier), .. clearList];
        ThrowIfNotOfOwner(ownerID, setList.Select(x => x.OwnerID), identifierList);

        await using IDbContextTransaction transaction = await context.Database.BeginTransactionAsync(cancellationToken);

        if (!await ownerExists(cancellationToken)) return false;

        List<DataProperty> currentList = await context.DataProperty.Where(x => x.OwnerID == ownerID && identifierList.Contains(x.PropertyIdentifier)).ToListAsync(cancellationToken);
        DateTime timeEnded = timeProvider.GetUtcNow().UtcDateTime;

        foreach (DataProperty property in setList)
        {
            DataProperty? current = currentList.SingleOrDefault(x => x.PropertyIdentifier == property.PropertyIdentifier);
            if (current is not null)
            {
                // A property has one type. A value of another type means the schema changed the type, which it must never do.
                if (current.GetType() != property.GetType())
                {
                    throw new InvalidOperationException($"The property {property.PropertyIdentifier} holds a {current.GetType().Name}, so it cannot be set to a {property.GetType().Name}.");
                }

                // The new row takes over at the moment it was made, so there is no gap and no overlap between the two.
                context.Add(current.ToHistory(property.CreatedByUserID, property.TimeCreated));
                context.Remove(current);
            }

            context.Add(property);
        }

        foreach (string identifier in clearList)
        {
            DataProperty? current = currentList.SingleOrDefault(x => x.PropertyIdentifier == identifier);
            if (current is null) continue;

            context.Add(current.ToHistory(endedByUserID, timeEnded));
            context.Remove(current);
        }

        await context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return true;
    }

    // Moves the properties of the owner, and the relations given with their properties, to the history. It only adds them to the context: the caller saves, so that it is all one step with whatever else ends.
    public static async Task EndOwnedAsync(EntityManagementContext context, IReadOnlyCollection<Guid> ownerIDList, IReadOnlyList<DataRelation> relationList, Guid endedByUserID, DateTime timeEnded, CancellationToken cancellationToken)
    {
        List<Guid> allOwnerIDList = [.. ownerIDList, .. relationList.Select(x => x.ID)];

        foreach (DataProperty property in await context.DataProperty.Where(x => allOwnerIDList.Contains(x.OwnerID)).ToListAsync(cancellationToken))
        {
            context.Add(property.ToHistory(endedByUserID, timeEnded));
            context.Remove(property);
        }

        foreach (DataRelation relation in relationList)
        {
            context.Add(relation.ToHistory(endedByUserID, timeEnded));
            context.Remove(relation);
        }
    }
}
