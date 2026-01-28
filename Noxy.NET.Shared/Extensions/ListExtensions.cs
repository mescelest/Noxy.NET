namespace Noxy.NET.Extensions;

public static class ListExtensions
{
    public static List<T> MoveItem<T>(this List<T> list, int oldIndex, int newIndex)
    {
        ArgumentNullException.ThrowIfNull(list);
        ArgumentOutOfRangeException.ThrowIfLessThan(oldIndex, 0, nameof(oldIndex));
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(oldIndex, list.Count, nameof(oldIndex));
        ArgumentOutOfRangeException.ThrowIfLessThan(newIndex, 0, nameof(newIndex));
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(newIndex, list.Count, nameof(newIndex));
        if (oldIndex == newIndex) return list;

        // TODO: Does not work.
        T item = list[oldIndex];
        int step = oldIndex < newIndex ? 1 : -1;
        for (int i = oldIndex; i != newIndex; i += step)
        {
            list[i] = list[i + step];
        }

        list[newIndex] = item;
        return list;
    }
}
