# Noxy.NET.UI.Common

## Table

### Structure

```razor
<Table RowCount="List.Count">
    <TableHeader>
        <TableCell>Name</TableCell>
        <TableHeaderCellSort Direction="..." OnClick="...">Title</TableHeaderCellSort>
    </TableHeader>
    <TableBody Loading="IsLoading">
        @foreach (var (index, row) in List.Index())
        {
            <TableRow @key="row.ID" Index="index">...</TableRow>
        }
    </TableBody>
</Table>
```

`TableBodyVirtualize` replaces `TableBody` when a list is long enough that only the rows in view should be drawn: it takes `List`, `ItemSize` and a row fragment receiving `(Row, Index)`. The table scrolls, and the header stays at the top.

The columns come from `--table-columns` (or the grid the table is placed in); the header, the body and the rows are subgrids of it.

### Accessibility

The components render the roles of the table, its header row, the body (`rowgroup`) and the rows, with `aria-rowcount` (from `RowCount`) and `aria-rowindex` (from `Index`), so a virtualized table reports its full size. `TableCell` gets its role from where it
sits: a column header inside `TableHeader`, a cell inside `TableRow`. Set `Kind="TableCellKindEnum.RowHeader"` on the cell that names the row. `TableHeaderCellSort` is a column header.

`SortableTable` rows can be reordered with the keyboard: focus a row and press Alt with the up or down arrow.
