# Organise page: sorting media files into artists, albums and genres

Status: implemented 2026-11-25 (not yet checked by running the app; see "How it ended up"). Design written 2026-11-24 from the conversation with the user. It is the frontend of `2026-11-15-library-organizer-design.md` (whose panel and drag-and-drop idea it replaces with a page) on top of the backend in `2026-11-15-metadata-entities-design.md`, which is built. Lines marked *(my call)* are decisions made without asking.

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
- **Assign through the context menu,** on one file (right-click) or on the ticked files. New items in the menu:
  - **Artist** and **Genre** (a file can have several): *Add* and *Remove*.
  - **Album** (a file has at most one): *Set* and *Remove*.
  - Each of the three is a submenu: the first entry opens a picker dialog (search field, a "New…" option), then a separator, then the last few the user assigned *(my call: five per kind)*. This is the shape of "Add to playlist" in the Library menu. "Recent" here means last assigned to, because items have no played date.
- **Every action in the Library's context menu is also in this one** (play next, add to playlist, edit, delete from library), *tried as an experiment at the user's request*: the user's gut feeling is that it does not belong on a page about something different, but wants to queue an album right after organising it. If it does not feel right, the Library actions are removed from this menu.
- **Changes show everywhere without reloading:** the three item lists and the files are already kept in step with the server and pushed by the hub; the page reads them like the rest of the app.

## Non-goals

- Drag-and-drop from the table onto the column (the user's intended next step; nothing here may make it harder, see "Later").
- Merge and split of items (the server has them; the rename that collides with another item follows the existing `MergeOnRename` setting and, when it is off, shows the server's message that names the other item).
- Tag lookup online, nested genres, sharing items between users, editing the stored audio file.
- Changing the Library, Queue or Playlist pages.

## Anti-goals (a failure even if it technically works)

- A filter that is on without the user noticing.
- An assignment or delete that changes files the user did not mean (the action's target is always either the right-clicked row or the ticked rows, as in Library, and a delete says how many files it touches).
- A table that is slower than Library with 5000 files.
- A change to the shared table that alters how Library, Queue or Playlist look.

## Constraints

- MAUI Blazor Hybrid, `Noxy.NET.UI.Shared` components first, styling only through the shared CSS variables; the page follows `docs/UI-GUIDE.md` and the repository's conventions (one class per file, `Entity`/`Response` naming, handlers, no inline logic in markup, texts in `AppTexts`).
- The file response already has `ArtistIDList`, `AlbumID` and `GenreIDList` next to the text names; the client already syncs `ResponseArtist`, `ResponseAlbum` and `ResponseGenre` lists (`MetadataSyncService`, with `MediaFileCount`). The client has only the **read** calls today.
- The current-device "last assigned" lists are not synced between devices.

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
- **Dialogs:** create and edit an item (a name; an album also picks its artists), confirm delete (names the item and the number of files), and the picker for the context menu. The picker is a dialog like `AddToPlaylistDialog` (a search field, the items in name order, "New…" which creates and assigns in one step).
- **Context menu:** a menu for this page that builds on `LibraryContextMenu`: the Library items plus Artist, Album and Genre. The builder decides whether it is the same component with a parameter or a small component that composes it; the Library page itself must not change behaviour.
- **Last assigned:** a short list per kind kept on the device (settings or preferences storage; the builder decides), newest first, at most five, with items that no longer exist dropped when read.
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
Play next
Add to playlist ▸
Artist ▸   Choose artist…  /  ─────  /  last assigned…  /  Remove ▸
Album  ▸   Choose album…   /  ─────  /  last assigned…  /  Remove
Genre  ▸   Choose genre…   /  ─────  /  last assigned…  /  Remove ▸
Edit…
Delete from library
```

In a narrow window the column goes below the table *(my call, to be checked against how the Queue panel behaves in a narrow window)*.

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
- Merge and split in the column; a way to see files with no artist, album or genre.

## Left to the builder

Names of the new classes and files, the exact shape of the state, how the column choice is expressed on the table, the preferences storage for last-assigned, texts, the icons, and the exact narrow-window layout.

## How it ended up

- The write calls, `MetadataActionService` and the Fluxor state (`FeatureMetadataListState`, `FeatureOrganiseState`) follow the design, with unit tests.
- **The context menu does not have submenus.** `LibraryContextMenu` has a new optional `ExtraItemFactory` parameter; `OrganiseContextMenu` fills it with three entries, "Set artist…", "Set album…" and "Set genre…", which come first, then a line, then the media file actions. The Library page is unchanged.
- **The "last assigned" lists were dropped.** Each entry opens `MetadataSetDialog` for the target media files: artists and genres are a searchable list of checkboxes (ticked when every media file has the item, a dash when only some do), an album is a list of cards with its picture and a "[No album]" card. Nothing is sent until Save, and then only the difference (`MetadataSetModel`, sent through `MetadataActionService.Apply`). A plus button next to the search makes a new item and ticks it.
- `MetadataItemDialog` (create and edit) shows the album's picture and a "Change picture" button; for that the server got `PUT /Album/{id}/Art`. The artists of an album use the same `MetadataCheckList` as the Set dialog.
- `MetadataItemDialog.OnSaved` carries the ID of the saved item (`EventCallback<Guid>`).
- The create, edit and delete dialogs live in `MetadataItemList`, one set per tab, not in `OrganiseSidebar`.
- The tabs have no event for a change, so `ActionOrganiseTabChanged` exists and is tested but the sidebar does not dispatch it; the tabs do not show that a filter is set (the highlighted row and the badge do).
- `MediaFileTable` takes `ColumnList`; a table with its own columns sets its grid from `--table-track-*` variables, the default columns keep `--table-columns-library`.
- The page and the table share the media file list of the Library state; nothing is loaded twice.
- The shared `Dialog` got a `Height` option (`DialogHeightEnum.Fixed`, used by the dialogs with a searched list, so they do not move while the list gets shorter) and a `Medium` size.
- Not done: drag and drop onto an item, and running the app to check the layout, the narrow window and the toasts.
