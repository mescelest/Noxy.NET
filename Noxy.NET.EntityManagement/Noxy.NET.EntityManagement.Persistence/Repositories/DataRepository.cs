using Microsoft.EntityFrameworkCore;
using Noxy.NET.EntityManagement.Domain.Entities;
using Noxy.NET.EntityManagement.Domain.Interfaces;

namespace Noxy.NET.EntityManagement.Persistence.Repositories;

// Each write is one SaveChanges, which is one transaction: the row that ends and the row that replaces it are moved and added together, or not at all.
public sealed class DataRepository(EntityManagementContext context, TimeProvider timeProvider) : IDataRepository
{
    public async Task<DataElement> CreateElement(DataElement element, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(element);

        context.DataElement.Add(element);
        await context.SaveChangesAsync(cancellationToken);

        return element;
    }

    public async Task<DataElement> CreateElement(DataElement element, IReadOnlyList<DataProperty> propertyList, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(element);
        ArgumentNullException.ThrowIfNull(propertyList);
        OwnerData.ThrowIfNotOfOwner(element.ID, propertyList.Select(x => x.OwnerID), propertyList.Select(x => x.PropertyIdentifier));

        context.DataElement.Add(element);
        foreach (DataProperty property in propertyList)
        {
            context.Add(property);
        }

        await context.SaveChangesAsync(cancellationToken);

        return element;
    }

    public Task<DataElement?> FindElement(Guid id, CancellationToken cancellationToken = default)
    {
        return context.DataElement.AsNoTracking().SingleOrDefaultAsync(x => x.ID == id, cancellationToken);
    }

    public Task<List<DataElement>> GetElementList(string schemaIdentifier, int skip, int take, CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(skip);
        ArgumentOutOfRangeException.ThrowIfNegative(take);

        return context.DataElement.AsNoTracking()
            .Where(x => x.SchemaIdentifier == schemaIdentifier)
            .OrderBy(x => x.TimeCreated).ThenBy(x => x.ID)
            .Skip(skip).Take(take)
            .ToListAsync(cancellationToken);
    }

    public Task<List<DataElement>> GetElementList(IReadOnlyCollection<Guid> idList, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(idList);

        return context.DataElement.AsNoTracking()
            .Where(x => idList.Contains(x.ID))
            .OrderBy(x => x.TimeCreated).ThenBy(x => x.ID)
            .ToListAsync(cancellationToken);
    }

    public Task<int> GetElementCount(string schemaIdentifier, CancellationToken cancellationToken = default)
    {
        return context.DataElement.CountAsync(x => x.SchemaIdentifier == schemaIdentifier, cancellationToken);
    }

    public async Task<bool> EndElement(Guid id, Guid endedByUserID, CancellationToken cancellationToken = default)
    {
        DataElement? element = await context.DataElement.SingleOrDefaultAsync(x => x.ID == id, cancellationToken);
        if (element is null) return false;

        DateTime timeEnded = timeProvider.GetUtcNow().UtcDateTime;

        // The element, its values, and the relations it is part of (either way) with their values: one step, so no relation is left pointing at an element that has ended.
        List<DataRelation> relationList = await context.DataRelation.Where(x => x.FromElementID == id || x.ToElementID == id).ToListAsync(cancellationToken);
        await OwnerData.EndOwnedAsync(context, [id], relationList, endedByUserID, timeEnded, cancellationToken);

        context.Add(element.ToHistory(endedByUserID, timeEnded));
        context.Remove(element);

        await context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public Task<List<DataProperty>> GetPropertyList(Guid ownerID, CancellationToken cancellationToken = default)
    {
        return context.DataProperty.AsNoTracking()
            .Where(x => x.OwnerID == ownerID)
            .OrderBy(x => x.PropertyIdentifier)
            .ToListAsync(cancellationToken);
    }

    public Task<List<DataProperty>> GetPropertyList(IReadOnlyCollection<Guid> ownerIDList, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(ownerIDList);

        return context.DataProperty.AsNoTracking()
            .Where(x => ownerIDList.Contains(x.OwnerID))
            .OrderBy(x => x.OwnerID).ThenBy(x => x.PropertyIdentifier)
            .ToListAsync(cancellationToken);
    }

    public Task<bool> ChangeElementProperties(Guid elementID, IReadOnlyList<DataProperty> setList, IReadOnlyList<string> clearList, Guid endedByUserID, CancellationToken cancellationToken = default)
    {
        // The check that the element exists and the changes are one transaction, so the element cannot end in between.
        return OwnerData.ChangePropertiesAsync(context, timeProvider, elementID, ct => context.DataElement.AnyAsync(x => x.ID == elementID, ct), setList, clearList, endedByUserID, cancellationToken);
    }

    public async Task<TProperty> SetProperty<TProperty>(TProperty property, CancellationToken cancellationToken = default) where TProperty : DataProperty
    {
        ArgumentNullException.ThrowIfNull(property);

        TProperty? current = await FindCurrent<TProperty>(property.OwnerID, property.PropertyIdentifier, cancellationToken);
        if (current is not null)
        {
            // The new row takes over at the moment it was made, so there is no gap and no overlap between the two.
            context.Add(current.ToHistory(property.CreatedByUserID, property.TimeCreated));
            context.Remove(current);
        }

        context.Add(property);
        await context.SaveChangesAsync(cancellationToken);

        return property;
    }

    public async Task<bool> ClearProperty<TProperty>(Guid ownerID, string propertyIdentifier, Guid endedByUserID, CancellationToken cancellationToken = default) where TProperty : DataProperty
    {
        TProperty? current = await FindCurrent<TProperty>(ownerID, propertyIdentifier, cancellationToken);
        if (current is null) return false;

        context.Add(current.ToHistory(endedByUserID, timeProvider.GetUtcNow().UtcDateTime));
        context.Remove(current);

        await context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public Task<HistoryElement?> FindElementHistory(Guid id, CancellationToken cancellationToken = default)
    {
        return context.HistoryElement.AsNoTracking().SingleOrDefaultAsync(x => x.ID == id, cancellationToken);
    }

    public Task<List<HistoryProperty>> GetPropertyHistoryList(Guid ownerID, CancellationToken cancellationToken = default)
    {
        return context.HistoryProperty.AsNoTracking()
            .Where(x => x.OwnerID == ownerID)
            .OrderBy(x => x.TimeEnded).ThenBy(x => x.TimeCreated)
            .ToListAsync(cancellationToken);
    }

    private Task<TProperty?> FindCurrent<TProperty>(Guid ownerID, string propertyIdentifier, CancellationToken cancellationToken) where TProperty : DataProperty
    {
        return context.Set<TProperty>().SingleOrDefaultAsync(x => x.OwnerID == ownerID && x.PropertyIdentifier == propertyIdentifier, cancellationToken);
    }
}
