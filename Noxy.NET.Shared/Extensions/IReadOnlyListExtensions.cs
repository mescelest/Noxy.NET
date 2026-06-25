namespace Noxy.NET.Extensions;

public static class IReadOnlyListExtensions
{
    extension<TValue>(IReadOnlyList<TValue> list)
    {
        public int FindIndex(Func<TValue, bool> iterator)
        {
            ArgumentNullException.ThrowIfNull(list);
            ArgumentNullException.ThrowIfNull(iterator);

            int count = list.Count;
            for (int i = 0; i < count; i++)
            {
                if (iterator(list[i])) return i;
            }

            return -1;
        }

        public int IndexOf(TValue value)
        {
            ArgumentNullException.ThrowIfNull(list);

            int count = list.Count;
            for (int i = 0; i < count; i++)
            {
                if (EqualityComparer<TValue>.Default.Equals(list[i], value)) return i;
            }

            return -1;
        }
    }
}
