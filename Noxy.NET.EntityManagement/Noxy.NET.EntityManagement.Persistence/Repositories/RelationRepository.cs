using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Noxy.NET.EntityManagement.Domain.Entities;
using Noxy.NET.EntityManagement.Domain.Enums;
using Noxy.NET.EntityManagement.Domain.Interfaces;
using Noxy.NET.EntityManagement.Domain.Models;

namespace Noxy.NET.EntityManagement.Persistence.Repositories;

// Each write is one transaction: the checks and the changes are made together, so what was checked is still true when it is stored.
public sealed class RelationRepository(EntityManagementContext context, TimeProvider timeProvider) : IRelationRepository
{
    public async Task<CreateRelationResult> CreateRelation(DataRelation relation, IReadOnlyList<DataProperty> attributeList, int? maxLinksOfFrom, int? maxLinksOfTo, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(relation);
        ArgumentNullException.ThrowIfNull(attributeList);
        ArgumentOutOfRangeException.ThrowIfLessThan(maxLinksOfFrom ?? 1, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(maxLinksOfTo ?? 1, 1);
        OwnerData.ThrowIfNotOfOwner(relation.ID, attributeList.Select(x => x.OwnerID), attributeList.Select(x => x.PropertyIdentifier));

        await using IDbContextTransaction transaction = await context.Database.BeginTransactionAsync(cancellationToken);

        if (!await context.DataElement.AnyAsync(x => x.ID == relation.FromElementID, cancellationToken)) return new(RelationCreateStatusEnum.FromElementNotFound, null);
        if (!await context.DataElement.AnyAsync(x => x.ID == relation.ToElementID, cancellationToken)) return new(RelationCreateStatusEnum.ToElementNotFound, null);

        if (await context.DataRelation.AnyAsync(x => x.RelationIdentifier == relation.RelationIdentifier && x.FromElementID == relation.FromElementID && x.ToElementID == relation.ToElementID, cancellationToken))
        {
            return new(RelationCreateStatusEnum.AlreadyExists, null);
        }

        // The count includes the relation that is being made, so a limit of one means there must be none yet.
        if (maxLinksOfFrom.HasValue && await context.DataRelation.CountAsync(x => x.RelationIdentifier == relation.RelationIdentifier && x.FromElementID == relation.FromElementID, cancellationToken) + 1 > maxLinksOfFrom.Value)
        {
            return new(RelationCreateStatusEnum.FromLimitReached, null);
        }

        if (maxLinksOfTo.HasValue && await context.DataRelation.CountAsync(x => x.RelationIdentifier == relation.RelationIdentifier && x.ToElementID == relation.ToElementID, cancellationToken) + 1 > maxLinksOfTo.Value)
        {
            return new(RelationCreateStatusEnum.ToLimitReached, null);
        }

        context.DataRelation.Add(relation);
        foreach (DataProperty attribute in attributeList)
        {
            context.Add(attribute);
        }

        await context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return new(RelationCreateStatusEnum.Created, relation);
    }

    public Task<DataRelation?> FindRelation(Guid id, CancellationToken cancellationToken = default)
    {
        return context.DataRelation.AsNoTracking().SingleOrDefaultAsync(x => x.ID == id, cancellationToken);
    }

    public Task<List<DataRelation>> GetRelationList(Guid elementID, string? relationIdentifier = null, CancellationToken cancellationToken = default)
    {
        return context.DataRelation.AsNoTracking()
            .Where(x => (x.FromElementID == elementID || x.ToElementID == elementID) && (relationIdentifier == null || x.RelationIdentifier == relationIdentifier))
            .OrderBy(x => x.TimeCreated).ThenBy(x => x.ID)
            .ToListAsync(cancellationToken);
    }

    public Task<bool> ChangeRelationAttributes(Guid relationID, IReadOnlyList<DataProperty> setList, IReadOnlyList<string> clearList, Guid endedByUserID, CancellationToken cancellationToken = default)
    {
        return OwnerData.ChangePropertiesAsync(context, timeProvider, relationID, ct => context.DataRelation.AnyAsync(x => x.ID == relationID, ct), setList, clearList, endedByUserID, cancellationToken);
    }

    public async Task<bool> EndRelation(Guid id, Guid endedByUserID, CancellationToken cancellationToken = default)
    {
        DataRelation? relation = await context.DataRelation.SingleOrDefaultAsync(x => x.ID == id, cancellationToken);
        if (relation is null) return false;

        await OwnerData.EndOwnedAsync(context, [], [relation], endedByUserID, timeProvider.GetUtcNow().UtcDateTime, cancellationToken);

        await context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public Task<List<HistoryRelation>> GetRelationHistoryList(Guid elementID, CancellationToken cancellationToken = default)
    {
        return context.HistoryRelation.AsNoTracking()
            .Where(x => x.FromElementID == elementID || x.ToElementID == elementID)
            .OrderBy(x => x.TimeEnded).ThenBy(x => x.TimeCreated)
            .ToListAsync(cancellationToken);
    }
}
