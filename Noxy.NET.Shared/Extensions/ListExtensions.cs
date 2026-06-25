namespace Noxy.NET.Extensions;

public static class ListExtensions
{
    extension<T>(List<T> list)
    {
        public List<T> SetItemByKey<TKey>(T item, Func<T, TKey> selector)
        {
            TKey key = selector(item);
            int index = list.FindIndex(x => EqualityComparer<TKey>.Default.Equals(selector(x), key));

            if (index != -1) list[index] = item;
            else list.Add(item);

            return list;
        }
    }
}
