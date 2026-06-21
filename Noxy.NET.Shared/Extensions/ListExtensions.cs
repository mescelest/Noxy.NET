namespace Noxy.NET.Extensions;

public static class ListExtensions
{
    extension<T>(List<T> list)
    {
        public void SetItemByKey<TKey>(T item, Func<T, TKey> keySelector)
        {
            TKey targetKey = keySelector(item);
            int index = list.FindIndex(x => EqualityComparer<TKey>.Default.Equals(keySelector(x), targetKey));

            if (index != -1) list[index] = item;
            else list.Add(item);
        }
    }
}
