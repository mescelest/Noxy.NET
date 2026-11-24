# Queue page instead of the queue popup: design

## Intent

The queue is a small popup opened from the player bar. It is hard to navigate and gives no overview, and while it was being tried it closed on every click inside it. The user wants a dedicated page for the queue, laid out like the playlist and Library pages, where the whole queue is one scrollable list: what was played above the current song, what is coming below it. The popup goes away completely.

## Goals

- A page at `/queue` that shows the whole queue as one continuous list, in play order: the played history (the last 50 items the app keeps), the current song, the songs the user queued, and the songs that come from the source (the playlist or the library list).
- The current song is highlighted and sits near the top of the list, with the previous song or two just visible above it. The page scrolls there when it opens and follows when the song changes.
- Thin divider rows name the parts: "Currently playing", "Next in queue", "Queued from X" (X is the playlist's name, or "Library" for a library source). Played songs sit above "Currently playing" under no divider of their own, so scrolling up reads as "what played".
- The same header as the Library and playlist pages (`MediaFileHeader`): the title "Queue", the count line, "N selected" and the actions button when rows are selected, and an outlined "Clear" button on the right of the tool row.
- Rows can be selected (a checkbox on each row, one in the header row for all), and a context menu offers actions that fit a queue. The actions differ for one row and for several.
- Rows are draggable by the whole row to reorder, as on the playlist page. Double-click jumps to the song. Text in rows is not selectable.
- The player-bar queue button goes to the page. The popup panel (`QueuePanel`) and its styles are deleted.

## Non-goals

- Sorting or searching the queue: its order is the point.
- A "playing now" block in the page header: the player bar already shows it.
- A link to the page in the sidebar (the player-bar button is the way in). Easy to add later.
- Changing how the queue works underneath (`QueueService`, the live session, the server).
- Selecting across the history and the upcoming parts with different actions per kind beyond what is listed under "Context menu".

## Anti-goals

- A page that closes, jumps or loses the scroll position when the user clicks on a row or when the queue changes under them.
- Touching the playlist or Library pages, or the shared library table, to make this fit. Those were just tuned.
- A list that gets slow with a queue of 5,000 songs.

## Constraints

- Blazor Hybrid (MAUI) front end, Windows first. The page and the list live in `Noxy.NET.MediaPlayer.App`; the logic that is not about drawing belongs in `Noxy.NET.MediaPlayer.Presentation`.
- The queue's data and actions already exist and stay as they are: `IQueueService.GetSnapshot()` (history, current, user queue, source part, source, with `Changed`), and `IPlaybackControlService` (`JumpTo`, `RemoveFromQueue`, `MoveInQueue`, `PlayNext`, `AddToQueue`) which also works when this device is only a controller. `IPlayerService.Clear()` empties the queue. `ILibraryActionService` offers the library actions on media file IDs.
- The queue's rows are queue items (`ItemID`, `MediaFileID`), not library files: the same file can be in the queue twice, so a row is keyed by `ItemID`.
- The project's conventions apply: one class per file named after it, interfaces in `Interfaces`, enums named `...Enum` in `Enums`, no magic strings (constants shared), DI names with the kind first, method syntax for LINQ, no inline logic in Razor attributes, colours and sizes only as CSS variables, user-facing text in the resource files, shared pieces reused before anything new (`MediaFileHeader`, `Button`, `ContextMenu`, `InputControlCheckbox`, the table look). New shared pieces stay baseline.
- Popups and menus are at most 320px wide (already done in `app.css`); this page has none of its own besides the context menu.

## Approaches considered

1. **Reuse the library table (`MediaFileTable`).** The look comes for free. But the table is built for sorting, searching and selecting; dividers do not fit its row model, so the shared table would need a "divider row"; and it has no way to scroll to a row. Rejected: it changes a component two other pages depend on.
2. **A dedicated queue list component (chosen).** A `/queue` page with its own virtualized list: fixed-height rows and divider rows of the same height, the same header component and the same column look as the playlist page. With fixed heights the position of the current song is its row number times the row height, which stays exact and fast with 5,000 rows. Costs some repeated column styling.
3. **Extract a shared table base first.** One clean base for both. Rejected for now: the biggest change and a risk to pages that work.

The user picked 2, and asked for multi-select with its own context options.

## Design

**Page.** `Pages/Queue.razor` at `/queue`, built like `Playlist.razor`: a `MediaFileHeader` (name "Queue", entry count, selected count) holding the selected-rows actions button, and below it the list. A `Clear` button (outlined) in the tool row calls `IPlayerService.Clear()` through the same error toast handling the other pages use. An empty queue shows the same kind of empty box the Library shows ("The queue is empty", with a line on how to fill it).

**List.** A `QueueList` component (own file, own CSS) draws a header row (select-all checkbox and the column names: title, artist, album, duration; not sortable) and a virtualized body. The body's rows are built from the snapshot in a Presentation model (a list of `QueueListRow`, each a divider or an item, with its kind: history, current, user queue, source) so the order, the dividers and the index of the current song are worked out and tested outside Razor. Row height is one constant shared by the model's consumers and the CSS variable the list reads (the existing `--size-library-row`), so the offset of the current song is `index * height`.

**Scrolling.** On first render and whenever the current song changes, the list scrolls so the current row is near the top with the previous one or two rows above it, unless the user has scrolled since. A small outlined "Back to playing" button shows when the current row is out of view and scrolls back on click. Scrolling is done with a small JS module of the list (the `.razor.js` convention used by `Popover` and `ContextMenu`). When the queue changes (a song removed, a drag, a new source) the scroll position is kept.

**Rows.** Each row: checkbox, drag by the whole row (upcoming rows only; the played and the current row are not draggable), the file state icon, title, artist, album, duration. Double-click on any row jumps there (`JumpTo`, keeping the skipped ones in history as the popup does). The current row is highlighted in the theme colour.

**Selection.** A set of selected item IDs, owned by the page like the playlist page owns its set. Select-all in the header row selects every row. The set drops IDs that are no longer in the queue.

**Context menu.** Right-click on a row, and the header's "⋯" button for the selected rows, open a `ContextMenu`. Right-click on a row inside a selection acts on the selection, as in the Library. Items:

- *Play now* (one row only): jump to it.
- *Play next*: put the rows at the front of the user queue, in their order in the list.
- *Remove from queue*: for upcoming rows (history and the current row are skipped; if none of the chosen rows can be removed the item is disabled with a reason).
- The library actions: Download, Save as file, Add to playlist, Remove from device (through `ILibraryActionService`, on the media file IDs of the rows).

Removing several rows is several `RemoveFromQueue` calls in a row; a failure for one is reported like the Library does with a bulk result toast.

**Player bar.** The queue button becomes a link to `/queue` (an `<a>`-based or `NavigationManager` button, one of the existing patterns). `QueuePanel.razor`, its CSS, `FooterElement`'s use of it and the `PopoverID` constant are removed. The `PopoverSurface` rows and labels stay: the device list uses them.

**Texts and style.** New texts in `AppTexts.resx` and its designer file, with the count helper for singular and plural. Colours and sizes from `variables.css` only; the selected and current colours use `--color-theme`.

## Testing

- Unit tests for the row model: order and dividers for a queue with history, a user queue and a source; no divider for an empty part; "Queued from" naming for a playlist and for the library; the index of the current song; a repeated media file keeps separate rows.
- Unit tests for the selection rules and for which context items are enabled for one row, for several rows, and for history and current rows.
- The scroll behaviour and the drag are checked by hand (there are no component tests in this project).

## Left to the builder

- How *Play next* works on rows that are already in the queue. `PlayNext(mediaFileID)` adds a new item, and `MoveInQueue` is meant for reordering; the builder checks what `Move` does across the user queue and the source part. If it cannot move a source row to the front, *Play next* is `PlayNext` for the file followed by removing the original upcoming item, so the song is not queued twice. Dragging a row across the divider between "Next in queue" and "Queued from X" uses whatever `MoveInQueue` already does for that today (the popup's drop behaviour is kept).
- The exact wording of the texts, the empty-box text, and the order of the context items.
- How many previous rows stay visible above the current one (one or two).
- Whether the list reuses the library table's CSS classes or copies the values; copy if reusing would couple the pages.
- Small details of the JS module (names, how "the user scrolled" is told apart from "the list scrolled itself").
