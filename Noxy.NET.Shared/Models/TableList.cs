using Noxy.NET.Interfaces;

namespace Noxy.NET.Models;

public class TableList<TData> : List<TData>, ITableList<TData>
{
    public TableList(int width) : base(width)
    {
        Width = width;
    }

    public TableList(int width, IEnumerable<TData> collection) : base(collection)
    {
        Width = width;
    }

    public int MaxRow => GetRowByIndex(Count - 1);
    public int MaxColumn => Width - 1;

    public int Width { get; set; }

    public int FindIndex(int column, int row) => row * Width + column;

    public int GetRowByIndex(int index) => index / Width;
    public int GetColumnByIndex(int index) => index % Width;

    public int GetRowDistance(int indexA, int indexB) => Math.Abs(GetRowByIndex(indexB) - GetRowByIndex(indexA));
    public int GetColumnDistance(int a, int b) => Math.Abs(GetColumnByIndex(b) - GetColumnByIndex(a));
    public int GetDiagonalDistance(int a, int b) => Math.Max(GetColumnDistance(a, b), GetRowDistance(a, b));

    public bool IsRangeHorizontal(int a, int b) => a == b || GetRowByIndex(a) == GetRowByIndex(b);
    public bool IsRangeVertical(int a, int b) => a == b || GetColumnByIndex(a) == GetColumnByIndex(b);
    public bool IsRangeDiagonal(int a, int b) => a == b || GetColumnDistance(a, b) == GetRowDistance(a, b);

    public int GetIndexWithColumnOffset(int index, int offset)
    {
        int column = GetColumnByIndex(index) + offset;
        ArgumentOutOfRangeException.ThrowIfLessThan(column, 0);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(column, MaxColumn);

        int result = index + offset;
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(result, Count);

        return result;
    }

    public bool TryGetIndexWithColumnOffset(int index, int offset, out int result)
    {
        int column = GetColumnByIndex(index) + offset;
        if (column < 0 || MaxColumn < column)
        {
            result = -1;
            return false;
        }

        result = index + offset;

        if (Count > column) return true;
        result = -1;
        return false;
    }

    public int GetIndexWithRowOffset(int index, int offset)
    {
        int row = GetRowByIndex(index) + offset;
        ArgumentOutOfRangeException.ThrowIfLessThan(row, 0);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(row, MaxRow);

        int result = index + offset * Width;
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(result, Count);

        return result;
    }

    public bool TryGetIndexWithRowOffset(int index, int offset, out int result)
    {
        int row = GetRowByIndex(index) + offset;
        if (row < 0 || MaxRow < row)
        {
            result = -1;
            return false;
        }

        result = index + offset * Width;

        if (Count > row) return true;
        result = -1;
        return false;
    }

    public int GetIndexWithOffset(int index, int offsetColumn, int offsetRow)
    {
        int column = GetColumnByIndex(index) + offsetColumn;
        ArgumentOutOfRangeException.ThrowIfLessThan(column, 0);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(column, MaxColumn);

        int row = GetRowByIndex(index) + offsetRow;
        ArgumentOutOfRangeException.ThrowIfLessThan(row, 0);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(row, MaxRow);

        int result = index + offsetColumn + offsetRow * Width;
        ArgumentOutOfRangeException.ThrowIfLessThan(result, 0);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(result, Count);
        return result;
    }

    public bool TryGetIndexWithOffset(int index, int offsetColumn, int offsetRow, out int result)
    {
        int column = GetColumnByIndex(index) + offsetColumn;
        if (column < 0 || MaxColumn < column)
        {
            result = -1;
            return false;
        }

        int row = GetRowByIndex(index) + offsetRow;
        if (row < 0 || MaxRow < row)
        {
            result = -1;
            return false;
        }

        result = index + offsetColumn + offsetRow * Width;
        if (result < 0 || Count <= result)
        {
            result = -1;
            return false;
        }

        return true;
    }

    public IEnumerable<TData> TakeRow(int row)
    {
        List<TData> result = new();
        for (int column = 0; column <= MaxColumn; column++)
        {
            int index = column + row * Width;
            if (index >= Count) break;
            result.Add(this[index]);
        }

        return result;
    }

    public IEnumerable<TData> TakeColumn(int column)
    {
        List<TData> result = new();
        for (int row = 0; row <= MaxRow; row++)
        {
            int index = column + row * Width;
            result.Add(this[index]);
        }

        return result;
    }

    public List<IEnumerable<TData>> ToRowList()
    {
        List<IEnumerable<TData>> result = new();
        for (int i = 0; i < MaxRow + 1; i++)
        {
            result.Add(TakeRow(i));
        }

        return result;
    }

    public List<IEnumerable<TData>> ToColumnList()
    {
        List<IEnumerable<TData>> result = new();
        for (int i = 0; i < MaxColumn + 1; i++)
        {
            result.Add(TakeColumn(i));
        }

        return result;
    }

    public Dictionary<int, IEnumerable<TData>> ToRowDictionary()
    {
        Dictionary<int, IEnumerable<TData>> result = new();
        for (int i = 0; i < MaxRow + 1; i++)
        {
            result.Add(i, TakeRow(i));
        }

        return result;
    }

    public Dictionary<int, IEnumerable<TData>> ToColumnDictionary()
    {
        Dictionary<int, IEnumerable<TData>> result = new();
        for (int i = 0; i < MaxColumn + 1; i++)
        {
            result.Add(i, TakeColumn(i));
        }

        return result;
    }

    public TData? FindByOffset(int index, int offsetColumn, int offsetRow, Func<TData, bool> predicate, TData? defaultValue = default)
    {
        return FindByOffset(index, offsetColumn, offsetRow, (x, _) => predicate(x), defaultValue);
    }

    public TData? FindByOffset(int index, int offsetColumn, int offsetRow, Func<TData, int, bool> predicate, TData? defaultValue = default)
    {
        while (TryGetIndexWithOffset(index, offsetColumn, offsetRow, out int currentIndex))
        {
            TData currentElement = this[currentIndex];
            if (predicate(currentElement, currentIndex))
            {
                return currentElement;
            }
        }

        return defaultValue;
    }

    public int FindIndexByOffset(int index, int offsetColumn, int offsetRow, Func<TData, bool> predicate)
    {
        return FindIndexByOffset(index, offsetColumn, offsetRow, (x, _) => predicate(x));
    }

    public int FindIndexByOffset(int index, int offsetColumn, int offsetRow, Func<TData, int, bool> predicate)
    {
        while (TryGetIndexWithOffset(index, offsetColumn, offsetRow, out index))
        {
            TData currentElement = this[index];
            if (predicate(currentElement, index))
            {
                return index;
            }
        }

        return -1;
    }
}
