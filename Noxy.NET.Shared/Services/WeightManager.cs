using Noxy.NET.Interfaces;

namespace Noxy.NET.Services;

public class WeightManager<TItem> : IWeightManager<TItem> where TItem : class, IWeightedItem
{
    public IEnumerable<TItem> Order(IEnumerable<TItem> list, bool apply = false)
    {
        List<TItem> result = list.OrderBy(x => x.Weight).ToList();
        if (!apply) return result;

        for (int index = 0; index < result.Count; index++)
        {
            result.ElementAt(index).Weight = index;
        }

        return result;
    }

    public void Swap(IEnumerable<TItem> list, int source, int target)
    {
        foreach (TItem entry in list)
        {
            if (entry.Weight == target)
            {
                entry.Weight = source;
            }
            else if (entry.Weight == source)
            {
                entry.Weight = target;
            }
        }
    }

    public void Move(IEnumerable<TItem> list, int source, int target)
    {
        foreach (TItem entry in list)
        {
            if (entry.Weight == target)
            {
                entry.Weight = source;
            }
        }
    }
}
