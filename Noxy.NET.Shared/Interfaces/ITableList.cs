using System.Collections;

namespace Noxy.NET.Interfaces;

public interface ITableListBase
{
    int Width { get; set; }

    int FindIndex(int column, int row);
    int GetRowByIndex(int index);
    int GetColumnByIndex(int index);

    bool IsRangeHorizontal(int indexA, int indexB);
    bool IsRangeVertical(int indexA, int indexB);
    bool IsRangeDiagonal(int indexA, int indexB);

    int GetRowDistance(int indexA, int indexB);
    int GetColumnDistance(int indexA, int indexB);
    int GetDiagonalDistance(int indexA, int indexB);

    int GetIndexWithColumnOffset(int index, int offset);
    bool TryGetIndexWithColumnOffset(int index, int offset, out int result);

    int GetIndexWithRowOffset(int index, int offset);
    bool TryGetIndexWithRowOffset(int index, int offset, out int result);

    int GetIndexWithOffset(int index, int offsetColumn, int offsetRow);
    bool TryGetIndexWithOffset(int index, int offsetColumn, int offsetRow, out int result);
}

public interface ITableList : IList, ITableListBase;

public interface ITableList<TData> : IList<TData>, ITableListBase
{
    public IEnumerable<TData> TakeRow(int row);
    public IEnumerable<TData> TakeColumn(int column);

    public List<IEnumerable<TData>> ToRowList();
    public List<IEnumerable<TData>> ToColumnList();

    public Dictionary<int, IEnumerable<TData>> ToRowDictionary();
    public Dictionary<int, IEnumerable<TData>> ToColumnDictionary();

    public TData? FindByOffset(int index, int offsetColumn, int offsetRow, Func<TData, bool> predicate, TData? defaultValue = default);
    public TData? FindByOffset(int index, int offsetColumn, int offsetRow, Func<TData, int, bool> predicate, TData? defaultValue = default);

    public int FindIndexByOffset(int index, int offsetColumn, int offsetRow, Func<TData, bool> predicate);
    public int FindIndexByOffset(int index, int offsetColumn, int offsetRow, Func<TData, int, bool> predicate);
}
