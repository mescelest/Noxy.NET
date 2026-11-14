using Noxy.NET.Models;

namespace Noxy.NET.Test.Shared;

public class TableListTest
{
    [Fact]
    public void ReturnListWithWidth()
    {
        int width = 5;
        TableList<int> list = new(width);
        Assert.Equal(width, list.Width);
        Assert.Empty(list);
    }

    [Fact]
    public void ReturnListWithWidthAndContent()
    {
        int width = 5;
        List<int> init = [0, 1, 2, 3, 4];
        TableList<int> list = new(width, init);
        Assert.Equal(width, list.Width);
        Assert.Equal(init.Count, list.Count);
    }

    public static TableList<int> Generate(int width, int count)
    {
        return new(width, new int[count].Select((_, i) => i));
    }

    public class FindIndexShould
    {
        [Fact]
        public void ReturnIndexByColumnAndRow()
        {
            TableList<int> table = Generate(5, 25);
            foreach (int item in table)
            {
                int column = table.GetColumnByIndex(item);
                int row = table.GetRowByIndex(item);
                Assert.Equal(item, table.FindIndex(column, row));
            }
        }
    }

    public class GetRowByIndexTest
    {
        [Fact]
        public void ReturnRow()
        {
            TableList<int> table = Generate(5, 25);
            for (int row = 0; row < 5; row++)
            {
                for (int column = 0; column < 5; column++)
                {
                    int index = table.FindIndex(column, row);
                    Assert.Equal(row, table.GetRowByIndex(index));
                }
            }
        }
    }

    public class GetColumnByIndexTest
    {
        [Fact]
        public void ReturnColumn()
        {
            TableList<int> table = Generate(5, 25);
            for (int row = 0; row < 5; row++)
            {
                for (int column = 0; column < 5; column++)
                {
                    int index = table.FindIndex(column, row);
                    Assert.Equal(column, table.GetColumnByIndex(index));
                }
            }
        }
    }

    public class GetRowDistanceTest
    {
        [Fact]
        public void ReturnDistance()
        {
            TableList<int> table = Generate(5, 25);
            int indexA = table.FindIndex(2, 2);
            int rowA = table.GetRowByIndex(indexA);
            for (int row = 0; row < 5; row++)
            {
                for (int column = 0; column < 5; column++)
                {
                    int indexB = table.FindIndex(column, row);
                    int rowB = table.GetRowByIndex(indexB);

                    int expected = Math.Abs(rowA - rowB);
                    int result = table.GetRowDistance(indexA, indexB);
                    Assert.Equal(expected, result);
                }
            }
        }
    }

    public class GetColumnDistanceTest
    {
        [Fact]
        public void ReturnDistance()
        {
            TableList<int> table = Generate(5, 25);
            int indexA = table.FindIndex(2, 2);
            int columnA = table.GetColumnByIndex(indexA);
            for (int row = 0; row < 5; row++)
            {
                for (int column = 0; column < 5; column++)
                {
                    int indexB = table.FindIndex(column, row);
                    int columnB = table.GetColumnByIndex(indexB);

                    int expected = Math.Abs(columnA - columnB);
                    int result = table.GetColumnDistance(indexA, indexB);
                    Assert.Equal(expected, result);
                }
            }
        }
    }

    public class GetDiagonalDistanceTest
    {
        [Fact]
        public void ReturnDistance()
        {
            TableList<int> table = Generate(5, 25);
            int indexA = table.FindIndex(2, 2);
            int rowA = table.GetRowByIndex(indexA);
            int columnA = table.GetColumnByIndex(indexA);

            for (int row = 0; row < 5; row++)
            {
                for (int column = 0; column < 5; column++)
                {
                    int indexB = table.FindIndex(column, row);
                    int rowB = table.GetRowByIndex(indexB);
                    int columnB = table.GetColumnByIndex(indexB);

                    int expected = Math.Max(Math.Abs(columnA - columnB), Math.Abs(rowA - rowB));
                    int result = table.GetDiagonalDistance(indexA, indexB);
                    Assert.Equal(expected, result);
                }
            }
        }
    }

    public class IsRangeHorizontalTest
    {
        [Fact]
        public void ReturnIsInSameRow()
        {
            TableList<int> table = Generate(5, 25);
            int indexA = table.FindIndex(2, 2);
            int rowA = table.GetRowByIndex(indexA);

            for (int row = 0; row < 5; row++)
            {
                for (int column = 0; column < 5; column++)
                {
                    int indexB = table.FindIndex(column, row);
                    int rowB = table.GetRowByIndex(indexB);

                    bool expected = indexA == indexB || rowA == rowB;
                    bool result = table.IsRangeHorizontal(indexA, indexB);
                    Assert.Equal(expected, result);
                }
            }
        }
    }

    public class IsRangeVerticalTest
    {
        [Fact]
        public void ReturnIsInSameRow()
        {
            TableList<int> table = Generate(5, 25);
            int indexA = table.FindIndex(2, 2);
            int columnA = table.GetColumnByIndex(indexA);

            for (int row = 0; row < 5; row++)
            {
                for (int column = 0; column < 5; column++)
                {
                    int indexB = table.FindIndex(column, row);
                    int columnB = table.GetColumnByIndex(indexB);

                    bool expected = indexA == indexB || columnA == columnB;
                    bool result = table.IsRangeVertical(indexA, indexB);
                    Assert.Equal(expected, result);
                }
            }
        }
    }

    public class IsRangeDiagonal
    {
        [Fact]
        public void ReturnIsInSameRow()
        {
            TableList<int> table = Generate(5, 25);
            int indexA = table.FindIndex(2, 2);
            int rowA = table.GetRowByIndex(indexA);
            int columnA = table.GetColumnByIndex(indexA);

            for (int row = 0; row < 5; row++)
            {
                for (int column = 0; column < 5; column++)
                {
                    int indexB = table.FindIndex(column, row);
                    int rowB = table.GetRowByIndex(indexB);
                    int columnB = table.GetColumnByIndex(indexB);

                    bool expected = indexA == indexB || Math.Abs(columnA - columnB) == Math.Abs(rowA - rowB);
                    bool result = table.IsRangeDiagonal(indexA, indexB);
                    Assert.Equal(expected, result);
                }
            }
        }
    }

    public class GetIndexWithColumnOffsetTest
    {
        [Fact]
        public void ReturnNextIndex()
        {
            TableList<int> table = Generate(5, 25);
            int indexA = table.FindIndex(2, 2);

            for (int column = -2; column < 2; column++)
            {
                int expected = table.FindIndex(column + 2, table.GetRowByIndex(indexA));
                int result = table.GetIndexWithColumnOffset(indexA, column);
                Assert.Equal(expected, result);
            }
        }
    }

    public class TryGetIndexWithColumnOffsetTest
    {
        [Fact]
        public void ReturnTrueAndNextIndex()
        {
            TableList<int> table = Generate(5, 25);
            int indexA = table.FindIndex(2, 2);

            for (int column = -2; column < 2; column++)
            {
                int expected = table.FindIndex(column + 2, table.GetRowByIndex(indexA));
                Assert.True(table.TryGetIndexWithColumnOffset(indexA, column, out int result));
                Assert.Equal(expected, result);
            }
        }
    }

    public class GetIndexWithRowOffsetTest
    {
        [Fact]
        public void ReturnNextIndex()
        {
            TableList<int> table = Generate(5, 25);
            int indexA = table.FindIndex(2, 2);

            for (int row = -2; row < 2; row++)
            {
                int expected = table.FindIndex(table.GetColumnByIndex(indexA), row + 2);
                int result = table.GetIndexWithRowOffset(indexA, row);
                Assert.Equal(expected, result);
            }
        }
    }


    public class TryGetIndexWithRowOffsetTest
    {
        [Fact]
        public void ReturnTrueAndNextIndex()
        {
            TableList<int> table = Generate(5, 25);
            int indexA = table.FindIndex(2, 2);

            for (int row = -2; row < 2; row++)
            {
                int expected = table.FindIndex(table.GetColumnByIndex(indexA), row + 2);
                Assert.True(table.TryGetIndexWithRowOffset(indexA, row, out int result));
                Assert.Equal(expected, result);
            }
        }
    }


    public class GetIndexWithOffsetTest
    {
        [Fact]
        public void ReturnNextIndex()
        {
            TableList<int> table = Generate(5, 25);
            int indexA = table.FindIndex(2, 2);

            for (int row = -2; row < 2; row++)
            {
                for (int column = -2; column < 2; column++)
                {
                    int expected = table.FindIndex(column + 2, row + 2);
                    int result = table.GetIndexWithOffset(indexA, column, row);
                    Assert.Equal(expected, result);
                }
            }
        }
    }


    public class TryGetIndexWithOffsetTest
    {
        [Fact]
        public void ReturnTrueAndNextIndex()
        {
            TableList<int> table = Generate(5, 25);
            int indexA = table.FindIndex(2, 2);

            for (int row = -2; row < 2; row++)
            {
                for (int column = -2; column < 2; column++)
                {
                    int expected = table.FindIndex(column + 2, row + 2);
                    Assert.True(table.TryGetIndexWithOffset(indexA, column, row, out int result));
                    Assert.Equal(expected, result);
                }
            }
        }
    }


    public class TakeRowTest
    {
        [Fact]
        public void ReturnElementListfromRow()
        {
            TableList<int> table = Generate(5, 25);

            for (int row = 0; row < table.MaxRow; row++)
            {
                IEnumerable<int> result = table.TakeRow(row).ToList();
                for (int column = 0; column < table.MaxColumn; column++)
                {
                    int expected = table.FindIndex(column, row);
                    Assert.Equal(expected, result.ElementAt(column));
                }
            }
        }
    }


    public class TakeColumnTest
    {
        [Fact]
        public void ReturnElementListfromColumn()
        {
            TableList<int> table = Generate(5, 25);

            for (int column = 0; column < table.MaxColumn; column++)
            {
                IEnumerable<int> result = table.TakeColumn(column).ToList();
                for (int row = 0; row < table.MaxRow; row++)
                {
                    int expected = table.FindIndex(column, row);
                    Assert.Equal(expected, result.ElementAt(row));
                }
            }
        }
    }
}
