namespace Noxy.NET.Extensions;

public static class EnumerableExtensions
{
    extension<TValue>(IEnumerable<TValue> list)
    {
        public int FindIndex(Func<TValue, bool> iterator)
        {
            ArgumentNullException.ThrowIfNull(list);
            ArgumentNullException.ThrowIfNull(iterator);

            int index = 0;
            foreach (TValue item in list)
            {
                if (iterator(item)) return index;
                index++;
            }

            return -1;
        }

        public int IndexOf(TValue value)
        {
            ArgumentNullException.ThrowIfNull(list);

            int index = 0;
            foreach (TValue item in list)
            {
                if (EqualityComparer<TValue>.Default.Equals(item, value)) return index;
                index++;
            }

            return -1;
        }
    }
}
