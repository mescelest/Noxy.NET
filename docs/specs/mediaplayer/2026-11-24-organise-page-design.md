# Organise page: sorting media files into artists, albums and genres

The frontend of the data layer in `2026-11-15-metadata-entities-design.md`, built on the ideas of `2026-11-15-library-organizer-design.md`. Lines marked *(my call)* are decisions made without asking.

## Intent

Artists, albums and genres are real items on the server (rename, delete, merge, assign to files), but the app can only read them. The user wants a place to look after them: see every file with its artist, album and genre, keep the lists of items tidy, and put the right item on the right files. It is a big enough tool that it gets a page of its own, below Library in the sidebar, and is not folded into Library.

## Goals

- **A new page "Organise"** in the sidebar, directly below Library.
- **A table like the Library's** with only: tick box, title, artist, album, genre. No year, added or download state.
- **A column on the right, like a sidebar,** with three tabs (Artist, Album, Genre; the same tab set the login uses). Each tab lists that kind's items with the number of files each has, and has:
  - a search field and a **+** button beside it, which creates an item of the open tab's kind without leaving the tab;
  - **edit** (rename; for an album also its artists) and **delete** on each row; delete asks for confirmation and takes the item off its files, never deleting a file.
- **Filtering by clicking an item.** Clicking an item's name filters the table to the files that have it; clicking it again clears it. One filter per kind at once (an artist, an album and a genre can be combined, and they narrow the table together); picking another item of the same kind replaces the first.
- **The filter is hard to miss,** shown in three places:
  1. a badge for each active filter beside the table's search field ("Artist: New Order", with an ✕ to remove it), plus "Clear all" when more than one;
  2. the chosen item in the column is highlighted (accent colour and a bar on its left edge);
  3. a dot on the tab of a kind that has a filter.
  The header count reads "12 of 340 files" while a filter or the search narrows the table. When nothing matches, the table says so and offers "Clear filters" instead of showing blank space.
- **Assign through the context menu,** on one file (right-click) or on the ticked files. Three entries come first: *Set artist…*, *Set album…* and *Set genre…*. Each opens a dialog for the target files: artists and genres are a searchable list of checkboxes (ticked when every file has the item, a dash when only some do), an album is a list of cards with its picture and a "[No album]" card. Nothing is sent until Save, and then only the difference. A plus button next to the search makes a new item and ticks it.
- **Every action in the Library context menu is also in this one** (play next, add to playlist, edit, delete from library), so an album can be queued right after organising it.
- **Merge.** Each row of the column has a "⋯" menu and a right-click menu: Edit, Merge into…, Merge others into this…, Delete. Merge uses the merge calls the server has, and asks for confirmation naming "Source → Target".
- **Changes show everywhere without reloading:** the three item lists and the files are already kept in step with the server and pushed by the hub; the page reads them like the rest of the app.

## Non-goals

- Drag-and-drop from the table onto the column (the user's intended next step; nothing here may make it harder, see "Later").
- Split of items (the server has it; the rename that collides with another item follows the existing `MergeOnRename` setting and, when it is off, shows the server message that names the other item).
- Tag lookup online, nested genres, sharing items between users, editing the stored audio file.
- Changing the Library, Queue or Playlist pages.

## Anti-goals (a failure even if it technically works)

- A filter that is on without the user noticing.
- An assignment or delete that changes files the user did not mean (the action's target is always either the right-clicked row or the ticked rows, as in Library, and a delete says how many files it touches).
- A table that is slower than Library with 5000 files.
- A change to the shared table that alters how Library, Queue or Playlist look.

## Constraints

- MAUI Blazor Hybrid, `Noxy.NET.UI.Shared` components first, styling only through the shared CSS variables; the page follows `docs/UI-GUIDE.md` and the repository's conventions (one class per file, `Entity`/`Response` naming, handlers, no inline logic in markup, texts in `AppTexts`).
- The file response already has `ArtistIDList`, `AlbumID` and `GenreIDList` next to the text names; the client already syncs `ResponseArtist`, `ResponseAlbum` and `ResponseGenre` lists (`MetadataSyncService`, with `MediaFileCount`).

## Approaches considered

**A. Make the shared table configurable, add a new page with its own state.** `MediaFileTable` gets a way to choose its columns (the other pages keep today's); a new `Organise` page, its own Fluxor state, its own right-hand column component and a context menu for the metadata. Chosen by the user. The trade-off is touching a shared component, kept small and covered by the existing pages' behaviour staying the same.

**B. A copy of the table for the Organise page.** Touches nothing shared, but sort headers, tick boxes, row height and the later drag logic would exist twice. Rejected.

**C. An organise mode on the Library page.** Rejected by the user: too big to fit there.

## Design

### Pieces

- **Sidebar:** a link "Organise" below Library (route `/organise`), with an icon and a text in `AppTexts`.
- **`MediaFileTable` columns:** a parameter that says which columns show. Not set, the table is what it is now. Organise asks for tick box, title, artist, album and genre. A genre column is new to the table (a file can have several: shown joined, as the server's `Genre` text is).
- **State (`FeatureOrganiseState` and its actions and reducers):** the open tab, the search text of the table, the filter (an artist ID, an album ID and a genre ID, each possibly none), the selection, and the sort. The files and item lists come from the existing Library and metadata state and are not loaded again. The selection and filter are kept only while the page is open: leaving the page clears the filter *(my call, so the user never returns to a short table without knowing why)*. Deleting the item that is the current filter clears that filter.
- **The visible list:** the files that match the table's search, then the filters (a file is shown when it has the chosen artist, has the chosen album and has the chosen genre; kinds with no filter do not count). "Select all" ticks only what is shown, and ticked files that a filter hides stay ticked, like the Library search today; the count says how many are hidden.
- **Right column (`OrganiseSidebar` and one list component per tab, or one list component given the kind; the builder decides):** `TabSet` with the three kinds, a search field and the + button, the list with counts, the highlight on the filtered item, edit and delete icons.
- **Dialogs:** create and edit an item (a name; an album also has a picture and picks its artists), confirm delete (names the item and the number of files), the merge dialogs, and the Set dialog for the context menu.
- **Context menu:** `OrganiseContextMenu` builds on `LibraryContextMenu` through its `ExtraItemFactory` parameter: the three Set entries first, then a line, then the Library items. The Library page itself does not change behaviour.
- **Client calls and service:** the API client gets the write calls the server already has (create, rename or update, delete, assign a list of files, remove a list of files, for each of the three kinds); a service on top of them (for example `IMetadataActionService`) is what the page and menu use, raises a toast on failure like the other services, and does not apply changes itself: the hub events and the sync already update the lists and files.

### What the user sees

```
Organise                                              12 of 340 files
[🔍 Search…………] [Artist: New Order ✕] [Genre: Synth ✕]  Clear all   │ [Artist•][Album][Genre•]
 ☐ Title        Artist      Album       Genre                         │ [🔍 Search……] [+]
 ☑ Blue Monday  New Order   Power…      Synth                         │ ▌New Order (12)   ✎ 🗑
 ☑ Temptation   New Order   Substance   Synth                         │  Massive Attack (48)
                                                                      │  Radiohead (63)
```

Right-click on a file or the ticked rows:

```
Set artist…
Set album…
Set genre…
─────
Play next
Add to playlist ▸
Edit…
Delete from library
```

In a narrow window the column goes below the table *(my call, to be checked against how the Queue page behaves in a narrow window)*.

### Edge cases

- **At most 1000 media files can be ticked** (the server takes at most 1000 in one assign or remove request, and the library can hold 5000). When a tick or "select all" would go past that, only the first 1000 are ticked and a warning toast says so and asks the user to narrow the search ("Only the first 1000 media files were selected. Please narrow your search."). The user's call; the alternative of sending several requests was set aside.
- No items of a kind: the tab says so and points to +.
- No files: the page shows the same empty state as Library.
- A delete or rename coming from another device while a filter or a dialog is open: the list updates; a filter on a deleted item clears; an open edit dialog for a deleted item closes with a toast *(my call)*.
- Creating an item with a name that already exists (same after normalising): the server's answer is shown; the user may then pick the existing one.
- Assigning an album that replaces another: it replaces silently, as the server does; "Set" says so in its label.
- Removing an artist or genre from files that do not have it, or an album that is not theirs: allowed and ignored by the server.

## Later (not built now, must not be made harder)

- **Drag a file (or the ticked files) onto an item in the column** to assign it. The column's rows must therefore be real drop targets later (one element per item, a stable item ID on it); the assignment goes through the same service call as the menu.
- Split in the column; a way to see files with no artist, album or genre.

## Left to the builder

Names of the new classes and files, the exact shape of the state, how the column choice is expressed on the table, the preferences storage for last-assigned, texts, the icons, and the exact narrow-window layout.

## Implementation notes

- **Extra items.** `LibraryContextMenu` has an optional `ExtraItemFactory` parameter; `OrganiseContextMenu` fills it with three entries, "Set artist…", "Set album…" and "Set genre…", which come first, then a line, then the media file actions. The Library page is unchanged.
- **Set dialogs.** Each entry opens `MetadataSetDialog` for the target media files: artists and genres are a searchable list of checkboxes (ticked when every media file has the item, a dash when only some do), an album is a list of cards with its picture and a "[No album]" card. Nothing is sent until Save, and then only the difference (`MetadataSetModel`, sent through `MetadataActionService.Apply`). A plus button next to the search makes a new item and ticks it.
- `MetadataItemDialog` (create and edit) shows the album's picture and a "Change picture" button; for that the server got `PUT /Album/{id}/Art`. The artists of an album use the same `MetadataCheckList` as the Set dialog.
- `MetadataItemDialog.OnSaved` carries the ID of the saved item (`EventCallback<Guid>`).
- The create, edit and delete dialogs live in `MetadataItemList`, one set per tab, not in `OrganiseSidebar`.
- The tabs have no event for a change, so `ActionOrganiseTabChanged` exists and is tested but the sidebar does not dispatch it; the tabs do not show that a filter is set (the highlighted row and the badge do).
- `MediaFileTable` takes `ColumnList`; a table with its own columns sets its grid from `--table-track-*` variables, the default columns keep `--table-columns-library`.
- The page and the table share the media file list of the Library state; nothing is loaded twice.
- The shared `Dialog` got a `Height` option (`DialogHeightEnum.Fixed`, used by the dialogs with a searched list, so they do not move while the list gets shorter) and a `Medium` size.
- Not done: drag and drop onto an item, and running the app to check the layout, the narrow window and the toasts.
