# Playlist page like the Library: design

Status: short design for a small change, agreed in conversation with the user on 2026-11-21 (the decisions below marked as theirs); the calls marked *(my call)* are mine. Built, as `Pages/Playlist.razor` on `MediaFileTable` (there is no `PlaylistView` component; the page holds its own components). Depended on the context menu and popover (`2026-11-21-context-menu-and-popover-design.md`) for the Actions menu.

## Intent

The playlist page should look and behave like the Library: the same table, with a search box, an Actions menu for the ticked rows, an upload button, status icons and sortable column headers. Today it is a glass header full of plain buttons above a simple list with a drag handle, a tick box and three text columns, and it carries its own button styles.

## Goals

- **Same look as the Library:** a header row with select-all, the download status icon, Title, Artist, Album, Year and Added columns, the Library's row layout, spacing and checkbox, and the same search box and button row above the table.
- **Sortable headers** (the user's decision): clicking a column sorts the rows by it, with the same quiet arrow as the Library.
- **Sorting is view-only** (the user's decision): it never changes the playlist.
- **A sorted view cannot be reordered** (the user's decision): while a sort is active the drag handles are disabled (shown dimmed, with a tooltip saying to clear the sort first) and a **Clear sort** button appears next to the search box. Clearing it returns to the playlist's own order and turns dragging back on.
- **An Actions menu** for the ticked rows: the Library's actions (download to this device, play next, add to queue, save as file, add to playlist, remove from this device) with **Remove from this playlist** in the place of "Delete from library". Built on the context menu component, and a right-click on a row opens the same menu, with the same single-row and group rules as the Library.
- **Dragging still reorders** the playlist when no sort is active, as today.

## Non-goals

- Sorting that changes the playlist's real order.
- A saved sort: the sort is forgotten when you leave the page *(my call)*.
- Search on the playlist page beyond the Library's: it filters the visible rows by the same fields as the Library *(my call, since "look like the library" includes the search box)*.

## Anti-goals

- Dragging that silently reorders the wrong rows while a sort or a search is active. With a search active, dragging is disabled the same way as with a sort *(my call: the visible rows are then not the playlist)*.
- Two copies of the table markup and styles that drift apart.

## Approaches considered

**A. Rebuild `PlaylistView` by copying the Library's markup and styles.** Quick, but two copies of the same table.

**B. Extract one shared table component in the app (`MediaFileTable`) that both views use.** *Chosen (my call).* The Library and the playlist differ only in a few places, passed as parameters: where the rows come from, where the selection is kept (the Library's Fluxor state, the playlist's local set), whether rows can be dragged, and what a double-click plays. One place for the columns, the checkbox, the status icon and the sort arrows.

**C. Make the playlist a mode of `LibraryView`.** Rejected: the Library is bound to its Fluxor state and playback source, and a playlist is a different source with its own order.

## Design

- **`MediaFileTable`** (app, `Components/`): renders the header (select-all, status, the five sortable columns through `SortHeaderCell`) and the rows. Parameters: the rows to show, the current sort and a callback for a header click, the selection and its callbacks, `IsReorderable` and a drop callback (shown as a grip column only when the host supports reordering), and a row double-click callback. The grip column is present in the playlist and absent in the Library; the columns line up either way.
- **`PlaylistView`** keeps its own state: the detail, a local `_selectedSet`, a local sort (`LibrarySortEnum` and direction, reused from the Library) and a search string, all view-only. Rows are the detail's entries ordered by position, then filtered and sorted for display. `IsReorderable` is true only with no sort and no search. A **Clear sort** button shows in the button row while a sort is active and clears the sort only.
- **The header area:** the playlist name and counts above the table, a **Play** button, the upload button (as today), and the playlist's "⋯" button, which opens the same `PlaylistContextMenu` as the sidebar (Play, Download all, Edit…, **Refresh**, Delete playlist). "Refresh" and "Download all" and "Delete playlist" move out of the page into that menu *(my call; Refresh joins the menu that was going to leave it out)*.
- **Playback** always follows the playlist's own order, whatever the view shows: Play, and double-clicking a row, start the playlist's queue from that entry *(my call: the queue source is the playlist, not the view; this differs from the Library, which plays exactly what the page shows)*.
- **The Actions menu** is the Library's menu parameterised by where the ticked ids come from and by one different last item. "Remove from this playlist" calls the playlist service's existing remove-entries call. The row right-click rules (single row, or the ticked group with a sub-menu) are shared with the Library through the same scope type.
- Wording uses "song"/"songs" in user-facing text and "media file" in code.

## Testing

- The decision of what the view shows (sorting and filtering the entries) and when dragging is allowed (no sort and no search) is plain C# and unit tested.
- The visual match with the Library and the drag behaviour are checked by hand in the app.

## Left to the builder

- The parameter names and exact shape of `MediaFileTable`, and how the Library's virtualization and the playlist's rows share it.
- Whether the grip column's dimmed state is a CSS rule or a `data-` attribute on the row.
- Where exactly the Clear sort button sits in the button row.
- The order of the work: extract `MediaFileTable` from `LibraryView` first (the Library must look and work exactly as before), then rebuild `PlaylistView` on it, then the Actions menu once the context menu exists.
