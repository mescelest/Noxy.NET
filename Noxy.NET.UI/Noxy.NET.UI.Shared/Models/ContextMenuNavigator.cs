using Noxy.NET.UI.Enums;

namespace Noxy.NET.UI.Models;

public class ContextMenuNavigator(List<ContextMenuItem> rootList, bool drillIn)
{
    private readonly List<int> _openPath = [];
    private List<ContextMenuItem> _rootList = rootList;

    // The index, in the list the focus is in, of the focused item.
    public int? FocusIndex { get; private set; }

    // The index of the opened sub-menu item at each level, from the root down.
    public List<int> OpenPath => [.. _openPath];

    // The list at every open level, the root first and the one the focus is in last.
    public List<List<ContextMenuItem>> LevelList
    {
        get
        {
            List<List<ContextMenuItem>> list = [_rootList];
            foreach (int index in _openPath)
            {
                list.Add(list[^1][index].SubItemList ?? throw new InvalidOperationException("An opened sub-menu item has no items."));
            }

            return list;
        }
    }

    private List<ContextMenuItem> ActiveList => LevelList[^1];

    // With drill-in a sub-menu replaces the list; otherwise it flies out beside it and the root stays.
    public List<ContextMenuItem> VisibleList => drillIn ? ActiveList : _rootList;

    public bool CanGoBack => drillIn && _openPath.Count > 0;

    public void MoveNext()
    {
        List<ContextMenuItem> list = ActiveList;
        FocusIndex = Find(list, FocusIndex is int current ? current + 1 : 0, 1, _ => true) ?? FocusIndex;
    }

    public void MovePrevious()
    {
        List<ContextMenuItem> list = ActiveList;
        FocusIndex = Find(list, FocusIndex is int current ? current - 1 : list.Count - 1, -1, _ => true) ?? FocusIndex;
    }

    public void MoveFirst()
    {
        FocusIndex = Find(ActiveList, 0, 1, _ => true) ?? FocusIndex;
    }

    public void MoveLast()
    {
        List<ContextMenuItem> list = ActiveList;
        FocusIndex = Find(list, list.Count - 1, -1, _ => true) ?? FocusIndex;
    }

    public void MoveToLetter(char letter)
    {
        List<ContextMenuItem> list = ActiveList;
        string wanted = char.ToUpperInvariant(letter).ToString();
        FocusIndex = Find(list, FocusIndex is int current ? current + 1 : 0, 1, x => x.Text.StartsWith(wanted, StringComparison.CurrentCultureIgnoreCase)) ?? FocusIndex;
    }

    public void OpenSubmenu()
    {
        if (FocusIndex is not int index) return;

        ContextMenuItem item = ActiveList[index];
        if (item.Kind != ContextMenuItemKindEnum.Submenu || !item.IsEnabled || item.SubItemList is not List<ContextMenuItem> subItemList) return;

        _openPath.Add(index);
        FocusIndex = Find(subItemList, 0, 1, _ => true);
    }

    // Closes one level and puts the focus back on the sub-menu item that opened it. False when nothing was open.
    public bool Close()
    {
        if (_openPath.Count == 0) return false;

        FocusIndex = _openPath[^1];
        _openPath.RemoveAt(_openPath.Count - 1);
        return true;
    }

    // Closes the levels below depth and puts the focus back on the sub-menu item that opened the first of them.
    public void CloseTo(int depth)
    {
        if (depth < 0 || depth >= _openPath.Count) return;

        FocusIndex = _openPath[depth];
        _openPath.RemoveRange(depth, _openPath.Count - depth);
    }

    // The mouse moved onto an item: focus it, and close the levels that were open below its own.
    public void FocusAt(int depth, int index)
    {
        List<List<ContextMenuItem>> levelList = LevelList;
        if (depth < 0 || depth >= levelList.Count) return;

        List<ContextMenuItem> list = levelList[depth];
        if (index < 0 || index >= list.Count || !list[index].IsEnabled) return;

        _openPath.RemoveRange(depth, _openPath.Count - depth);
        FocusIndex = index;
    }

    // The items were rebuilt while the menu is open: keep the open levels and the focus that still make sense.
    public void Replace(List<ContextMenuItem> newRootList)
    {
        _rootList = newRootList;

        List<ContextMenuItem> list = _rootList;
        for (int depth = 0; depth < _openPath.Count; depth++)
        {
            int index = _openPath[depth];
            bool isOpenable = index < list.Count && list[index].Kind == ContextMenuItemKindEnum.Submenu && list[index].IsEnabled && list[index].SubItemList is not null;
            if (!isOpenable)
            {
                _openPath.RemoveRange(depth, _openPath.Count - depth);
                FocusIndex = null;
                return;
            }

            list = list[index].SubItemList ?? throw new InvalidOperationException("An opened sub-menu item has no items.");
        }

        if (FocusIndex is int focus && (focus >= list.Count || !list[focus].IsEnabled))
        {
            FocusIndex = null;
        }
    }

    public ContextMenuItem? Choose()
    {
        if (FocusIndex is not int index) return null;

        ContextMenuItem item = ActiveList[index];
        return item.Kind == ContextMenuItemKindEnum.Item && item.IsEnabled ? item : null;
    }

    // Walks the list from start in the given direction, wrapping once round, and returns the first enabled match.
    private static int? Find(List<ContextMenuItem> list, int start, int step, Func<ContextMenuItem, bool> isMatch)
    {
        int count = list.Count;
        for (int offset = 0; offset < count; offset++)
        {
            int index = ((start + offset * step) % count + count) % count;
            if (list[index].IsEnabled && isMatch(list[index])) return index;
        }

        return null;
    }
}
