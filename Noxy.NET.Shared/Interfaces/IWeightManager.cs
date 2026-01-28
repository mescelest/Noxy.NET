namespace Noxy.NET.Interfaces;

public interface IWeightManager<TItem> where TItem : IWeightedItem
{
    IEnumerable<TItem> Order(IEnumerable<TItem> list, bool apply = false);
    void Swap(IEnumerable<TItem> list, int source, int target);
    void Move(IEnumerable<TItem> list, int source, int target);
}
