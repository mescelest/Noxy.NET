namespace Noxy.NET.UI.Extensions;

public static class EnumerableExtensions
{
    public static int FindIndex<TValue>(this IEnumerable<TValue> list, Func<TValue, bool> iterator)
    {
        ArgumentNullException.ThrowIfNull(list);
        ArgumentNullException.ThrowIfNull(iterator);

        TValue[] array = list.ToArray();
        for (int i = 0; i < array.Length; i++)
        {
            if (iterator(array[i]))
            {
                return i;
            }
        }

        return -1;
    }

    public static int IndexOf<TValue>(this IEnumerable<TValue> list, TValue value)
    {
        ArgumentNullException.ThrowIfNull(list);

        TValue[] array = list.ToArray();
        for (int i = 0; i < array.Length; i++)
        {
            if (EqualityComparer<TValue>.Default.Equals(array[i], value))
            {
                return i;
            }
        }

        return -1;
    }
}
