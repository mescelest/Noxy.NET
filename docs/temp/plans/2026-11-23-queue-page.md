# Queue Page Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Replace the queue popup with a `/queue` page that shows the whole queue as one scrollable list, with multi-select and a context menu.

**Architecture:** The order, dividers and current index of the list are worked out by a pure model in `Presentation` (unit tested). A small `QueueActionService` in `Presentation` does the bulk remove and play-next through the existing `IPlaybackControlService`. In `App`, a `QueueList` component draws the virtualized rows and scrolls with a JS module, a `QueueContextMenu` builds the menu, and `Pages/Queue.razor` composes them under the shared `MediaFileHeader`. `QueuePanel` is deleted and the player-bar button navigates.

**Tech Stack:** .NET 10, Blazor Hybrid (MAUI), Fluxor, xUnit v3, scoped CSS, JS modules (`.razor.js`).

**Spec:** `docs/specs/2026-11-23-queue-page-design.md`

## Global Constraints

- Windows first; a popup or menu is at most 320px wide (`--size-column`); text that does not fit ends in an ellipsis.
- One class per file, named after it. Interfaces in an `Interfaces` folder; enums are named `...Enum` and live in `Enums`; lists are named with a `List` suffix; no magic strings (constants or `nameof`); DI parameters name the kind first (`serviceControl`, `serviceQueue`).
- LINQ in method syntax; no lambdas, ternaries or `??` inline in Razor attributes (named members); `is null` / `is not null`; extension blocks; no `yield`; no `!` operator; every enum `switch` ends in a default that throws `InvalidEnumArgumentException`.
- Colours, sizes and fonts only as CSS variables from `variables.css`; the current and selected look uses `--color-theme`. Texts only in `AppTexts.resx` (App) or `PresentationTexts.resx` (Presentation) with their `.Designer.cs` updated by hand.
- Code and tests say "media file"; user-facing text may say "song".
- Reuse before new: `MediaFileHeader`, `Button`, `ButtonList`, `ContextMenu`, `ContextMenuItem`, `InputControlCheckbox`, `LibraryMenuScope`, `ILibraryActionService`, `ConfirmActionDialog`, `PopoverSurface`/`PopoverRow` classes from `app.css`.
- Run only the tests of the area touched; the whole suite once at the end. Test command (from `Noxy.NET.Test/Noxy.NET.Test.UnitTest`): `dotnet build` then `bin/Debug/*/Noxy.NET.Test.UnitTest.exe -class "*QueueListModelTest"` (`dotnet test` does not work on this SDK).
- **Git:** the user reviews production code, so do not commit it. Stage new files by name with `git add`. Test files alone may be committed. Plan steps that say "Stage" mean exactly this.

## Review Focus

- The same media file queued twice: two rows with their own keys; ticking one does not tick the other (Task 1 test, Task 5 selection).
- The queue changes while rows are ticked (one advanced, removed or cleared): ticked IDs that are no longer in the queue are dropped (Task 5).
- Nothing is current (empty queue, or the queue ended): no "Currently playing" divider, `CurrentIndex` is -1, the empty box shows, scrolling does nothing (Tasks 1, 4, 5).
- 5,000 upcoming songs: one row per item plus dividers, built in one pass, and only the visible rows are drawn (Tasks 1, 4).
- Right-click or "⋯" on history or the current row: Remove and Play next are disabled with a reason, and the row cannot be dragged (Tasks 1, 2, 5).

---

### Task 1: Row model

**Files:**
- Create: `Noxy.NET.MediaPlayer.Presentation/Enums/QueueRowKindEnum.cs`, `Noxy.NET.MediaPlayer.Presentation/Models/QueueListRow.cs`, `Noxy.NET.MediaPlayer.Presentation/Models/QueueListModel.cs`, `Noxy.NET.MediaPlayer.Presentation/Extensions/QueueRowKindEnumExtension.cs`, `Noxy.NET.MediaPlayer.Presentation/Extensions/ResponseQueueExtension.cs`
- Test: `Noxy.NET.Test/Noxy.NET.Test.UnitTest/MediaPlayer/Presentation/QueueListModelTest.cs`

**Interfaces:**
- Produces:
  - `enum QueueRowKindEnum { DividerCurrent, DividerUserQueue, DividerSource, History, Current, UserQueue, Source }`
  - `sealed record QueueListRow(QueueRowKindEnum Kind, QueueItem? Item, string? SourceName)`; `QueueItem` is null for the three divider kinds; `SourceName` is set only on `DividerSource`.
  - `sealed record QueueListModel(List<QueueListRow> RowList, int CurrentIndex)`; `CurrentIndex` is the index of the `Current` row, or -1.
  - `extension(QueueRowKindEnum kind)`: `bool IsItem` (everything but the dividers) and `bool IsUpcoming` (`UserQueue` or `Source`).
  - `extension(ResponseQueue queue)`: `QueueListModel ToListModel(string? sourceName)`: order is history rows, then (if `Current` is not null) `DividerCurrent` + `Current`, then (if the user queue is not empty) `DividerUserQueue` + its rows, then (if the source part is not empty) `DividerSource` (carrying `sourceName`) + its rows. History has no divider.

- [ ] **Step 1: Write the failing tests** in `QueueListModelTest.cs`. Build `ResponseQueue` values with `new QueueItem(Guid.NewGuid(), Guid.NewGuid())`. Tests, with assertions:
  - `ToListModel_OrdersHistoryCurrentUserQueueThenSource_WithTheThreeDividers`: 2 history, a current, 2 user, 2 source → kinds are `[History, History, DividerCurrent, Current, DividerUserQueue, UserQueue, UserQueue, DividerSource, Source, Source]` and `CurrentIndex == 3`.
  - `ToListModel_LeavesOutTheDividerOfAnEmptyPart`: no user queue → no `DividerUserQueue`; no source part → no `DividerSource`.
  - `ToListModel_WithNothingCurrent_HasNoCurrentDividerAndAnIndexOfMinusOne`.
  - `ToListModel_KeepsASongQueuedTwiceAsTwoRows`: two items with the same `MediaFileID` and different `ItemID` give two item rows.
  - `ToListModel_SourceDividerCarriesTheSourceName`: `"Road trip"` on `DividerSource.SourceName`, null on every other row.
  - `ToListModel_Of5000SourceItems_HasOneRowPerItemPlusTheDividers`: 5,000 source items, a current → `RowList.Count == 5000 + 3`.
  - `IsItemAndIsUpcoming_AreRightForEveryKind` (Theory over the enum): dividers are not items; only `UserQueue` and `Source` are upcoming.
- [ ] **Step 2: Run to verify they fail** (does not compile: types missing).
- [ ] **Step 3: Implement** the five files per the Interfaces block. `IsItem`/`IsUpcoming` switch over the enum with the throwing default.
- [ ] **Step 4: Run** `-class "*QueueListModelTest"`; expected: all pass.
- [ ] **Step 5: Stage** the new files and the test (`git add` each by name); commit only the test file.

### Task 2: Queue actions service

**Files:**
- Create: `Noxy.NET.MediaPlayer.Presentation/Interfaces/Services/IQueueActionService.cs`, `Noxy.NET.MediaPlayer.Presentation/Services/QueueActionService.cs`
- Modify: `Noxy.NET.MediaPlayer.Presentation/ServiceCollectionExtensions.cs` (register `services.AddSingleton<IQueueActionService, QueueActionService>();` next to `ILibraryActionService`)
- Test: `Noxy.NET.Test/Noxy.NET.Test.UnitTest/MediaPlayer/Presentation/QueueActionServiceTest.cs`

**Interfaces:**
- Consumes: `IPlaybackControlService.RemoveFromQueue(Guid)`, `.MoveInQueue(Guid itemID, Guid? afterItemID)`, `.PlayNext(Guid mediaFileID)`; `IQueueService.GetSnapshot()`; `BulkResult(int SucceededCount, List<Guid> FailedIDList, string? Message)`.
- Produces: `IQueueActionService` with
  - `Task<BulkResult> Remove(List<Guid> itemIDList)`: removes the ones that are upcoming (in the user queue or the source part of the current snapshot); history and the current item are skipped and are not failures. `FailedIDList` holds the item IDs whose call threw (the exception message of the first goes in `Message`).
  - `Task<BulkResult> PlayNext(List<Guid> itemIDList)`: the upcoming ones among them end up at the front of the user queue in the order they have in the queue now. A user-queue item is moved with `MoveInQueue(itemID, null)`; a source item is added with `PlayNext(mediaFileID)` and its original removed with `RemoveFromQueue`. Items are handled last to first so the order is kept.
  - `QueueActionService(IPlaybackControlService serviceControl, IQueueService serviceQueue)`.

- [ ] **Step 1: Write the failing tests** using `FakePlaybackControlService` (its `CallList` strings) and `FakeQueueService`. First read `FakeQueueService` and `FakePlaybackControlService` to see how a queue is set and how a call is written down, and use that. Tests:
  - `Remove_RemovesUpcomingItems_AndSkipsHistoryAndTheCurrentOne`: calls recorded only for the upcoming IDs; `FailedIDList` empty; `SucceededCount` is the number removed.
  - `Remove_ACallThatThrows_IsAFailureAndTheRestStillRun`.
  - `PlayNext_MovesUserQueueItemsToTheFrontLastFirst`: user queue `[a, b, c]`, ask for `[a, c]` → `MoveInQueue(c, null)` then `MoveInQueue(a, null)` (so `a, c, b`).
  - `PlayNext_OfASourceItem_AddsTheFileAndRemovesTheOriginal`: one `PlayNext(mediaFileID)` then one `RemoveFromQueue(itemID)`.
  - `PlayNext_SkipsHistoryAndTheCurrentOne`.
- [ ] **Step 2: Run** `-class "*QueueActionServiceTest"`; expected: fail (types missing).
- [ ] **Step 3: Implement** the interface and service. Classify IDs against `serviceQueue.GetSnapshot()`; catch `Exception` per item (not `OperationCanceledException`) and collect it.
- [ ] **Step 4: Run** the class; expected: pass.
- [ ] **Step 5: Stage** new files and the modified registration; commit only the test.

### Task 3: Texts

**Files:**
- Modify: `Noxy.NET.MediaPlayer.App/Resources/AppTexts.resx`, `AppTexts.Designer.cs`

**Interfaces:**
- Produces `AppTexts` properties (each with a `<comment>` in the resx saying where it shows): `QueueTitle` ("Queue"), `QueueClear` ("Clear"), `QueueBackToPlaying` ("Back to playing"), `QueueDividerCurrent` ("Currently playing"), `QueueDividerUserQueue` ("Next in queue"), `QueueDividerSource` ("Queued from {0}"), `QueueSourceLibrary` ("Library"), `QueueEmptyTitle` ("The queue is empty"), `QueueEmptyDescription` ("Play a song from the Library or a playlist to fill it."), `QueuePlayNow` ("Play now"), `QueuePlayNext` ("Play next"), `QueueRemove` ("Remove from queue"), `QueueNothingRemovable` ("Only songs that have not played yet can be removed."), `QueueColumnTitle`/`QueueColumnArtist`/`QueueColumnAlbum`/`QueueColumnDuration` ("Title", "Artist", "Album", "Length").

- [ ] **Step 1:** Add the entries to the resx and the matching properties to the designer file in the same style as the neighbours (alphabetical order, `ResourceManager.GetString`).
- [ ] **Step 2:** Build the App (`dotnet build -f net10.0-windows10.0.19041.0` in `Noxy.NET.MediaPlayer.App`, with `-p:OutDir=<a temp folder>/` if the app is running); expected: build succeeded.
- [ ] **Step 3: Stage** the two files.

### Task 4: The list component

**Files:**
- Create: `Noxy.NET.MediaPlayer.App/Components/QueueList.razor`, `QueueList.razor.css`, `QueueList.razor.js`
- Modify: none

**Interfaces:**
- Consumes: `QueueListModel`/`QueueListRow` (Task 1), `AppTexts` (Task 3), `FileStatusIcon`, `InputControlCheckbox`, `Button`.
- Produces: `QueueList` with parameters
  - `QueueListModel Model` (required), `IReadOnlySet<Guid> SelectedSet`, `Func<Guid, ResponseMediaFile?> MediaOf`, `Func<QueueItem, FileSyncStateEnum> StateOf`;
  - callbacks: `EventCallback OnToggleAll`, `EventCallback<Guid> OnToggleRow`, `EventCallback<Guid> OnJump` (double-click), `EventCallback<(MouseEventArgs Args, QueueListRow Row)> OnContextMenu`, `EventCallback<(Guid DragKey, Guid TargetKey)> OnMove` (the target is the row it was dropped on, as the old popup did: the page calls `MoveInQueue(dragKey, targetKey)`).
  - `bool AllSelected`.
  - The list scrolls itself; the page does not need a handle.

- [ ] **Step 1: Markup.** A header row (select-all checkbox, then the four column names from Task 3), then `<Virtualize Items="Model.RowList" ItemSize="...">` whose rows are either a divider (text from the kind: `QueueDividerCurrent`, `QueueDividerUserQueue`, `string.Format(AppTexts.QueueDividerSource, row.SourceName ?? AppTexts.QueueSourceLibrary)`) or an item row: checkbox, `FileStatusIcon`, title, artist, album, duration (`m:ss`). Every row and divider has the same height, `var(--size-library-row)`; the `Virtualize` `ItemSize` is that height in pixels as an estimate (48). Named members for everything computed (`TitleOf(row)`, `IsSelected(row)`, `IsDraggable(row)`: `Kind.IsUpcoming`).
- [ ] **Step 2: Drag, jump, right-click** as `MediaFileTable` does for its rows (`draggable`, `@ondragstart`, `@ondragover:preventDefault`, `@ondrop`, `@ondragend` clearing the marks, `@ondblclick`, `@oncontextmenu:preventDefault`); text not selectable (`user-select: none` on rows); the current row is bold in `--color-theme`; a dragged-over row shows a top border in `--color-theme`.
- [ ] **Step 3: `QueueList.razor.js`** exports `ScrollToIndex(container, index, rowsAbove)` (measures the height of a rendered row, sets `scrollTop` to `max(0, (index - rowsAbove) * height)`), `IsIndexVisible(container, index)` (measured the same way) and `Watch(container, dotNetReference, methodName)` / `Unwatch(container)` which call back when the user scrolls (not when `ScrollToIndex` did: set a flag around the programmatic scroll). Follow `NarrowWatcher.razor.js` and its import and dispose pattern.
- [ ] **Step 4: Behaviour.** After the first render and whenever `Model.CurrentIndex` changes to another item, call `ScrollToIndex(..., Model.CurrentIndex, RowsAboveCurrent)` unless the user has scrolled since (a private flag, cleared when they press "Back to playing" or the current item changes while the current row is visible). `RowsAboveCurrent` is a constant of 2 (a divider and one history row). A "Back to playing" outlined `Button` shows when the user has scrolled and `CurrentIndex >= 0`, and scrolls back and clears the flag. When the model changes for another reason (an item removed, a drag), the scroll position is not touched.
- [ ] **Step 5: CSS** (`QueueList.razor.css`): the same column grid and paddings as `MediaFileTable.razor.css` (copy the values, do not couple), dividers as small uppercase `--color-muted` labels using the `PopoverLabel`-like values from `app.css`, no striping on dividers.
- [ ] **Step 6: Build** the App; expected: build succeeded. Visual check is in Task 7.
- [ ] **Step 7: Stage** the three files.

### Task 5: The context menu

**Files:**
- Create: `Noxy.NET.MediaPlayer.App/Components/QueueContextMenu.razor`, `QueueContextMenu.razor.css`

**Interfaces:**
- Consumes: `ContextMenu`, `ContextMenuItem.Item/Separator/Submenu`, `LibraryMenuScope.Of(clickedID, selectedSet, selectedIDList)`, `IQueueActionService` (Task 2), `ILibraryActionService`, `IPlaybackControlService.JumpTo`, `IToastService`, `AppTexts`, the "Add to playlist" dialog flow already in `LibraryContextMenu` (copy its `Dialog`/`playlist-choice` markup and handler; do not change `LibraryContextMenu`).
- Produces: `QueueContextMenu` with parameters `IReadOnlySet<Guid> SelectedSet`, `List<Guid> SelectedKeyList` (item IDs in list order), `Func<Guid, QueueListRow?> RowOf`, `bool ShowTrigger`; and `Task OpenAtAsync(MouseEventArgs args, Guid itemID)`. The trigger is the same ghost icon `Button` (class `trigger`, 1.875rem) as in `LibraryContextMenu`, popover-linked to the menu.
- Menu items for a scope (one row, or the group): *Play now* (one row only, `JumpTo(itemID, true)`), *Play next* and *Remove from queue* (disabled with `AppTexts.QueueNothingRemovable` when none of the rows is upcoming; they act on the upcoming ones), a separator, then Download, Save as file, Add to playlist, Remove from device on the media file IDs of the rows (via `ILibraryActionService`, disabled reasons from `GetDisabledReason`). A failure result is shown as `LibraryContextMenu` does (`ServiceToast.ShowWarning` with the bulk text).

- [ ] **Step 1:** Implement, building the item list in a named method `BuildItemList(List<Guid> itemIDList)`; a right-click inside a selection of two or more acts on the selection (`LibraryMenuScope.Of`).
- [ ] **Step 2: Build** the App; expected: build succeeded.
- [ ] **Step 3: Stage** the two files.

### Task 6: The page

**Files:**
- Create: `Noxy.NET.MediaPlayer.App/Pages/Queue.razor`, `Queue.razor.css` (only if the page needs rules of its own)

**Interfaces:**
- Consumes: Tasks 1, 4, 5; `IQueueService` (`GetSnapshot()`, `Changed`), `IPlayerService` (`Clear`, `StateOf`), `IPlaybackControlService` (`JumpTo`, `MoveInQueue`), `IState<FeatureLibraryState>` for the media lookup (as `QueuePanel` did), `IState<FeaturePlaylistListState>` for the playlist's name, `MediaFileHeader`, `MessageBox`.
- Produces: the route `@page "/queue"`, derived from `BaseComponentPageFluxor` like `Playlist.razor`, with `@implements IAsyncDisposable`.

- [ ] **Step 1:** The page builds `Model` with `ServiceQueue.GetSnapshot().ToListModel(SourceName)` on init and on every `Changed` (through `InvokeAsync`), where `SourceName` is the playlist's name for a playlist source, otherwise null. The selected set is a `HashSet<Guid>` of item IDs; after every rebuild it is intersected with the IDs in the model. Select-all toggles every item row. Unsubscribe in `DisposeAsync`.
- [ ] **Step 2:** Markup as `Playlist.razor`: `<div class="@CssClass @FullscreenPageClass">`, `MediaFileHeader Name="@AppTexts.QueueTitle" EntryCount="..." SelectedCount="_selectedSet.Count"` holding the `QueueContextMenu` (`ShowTrigger` when something is selected), then an outlined `Button` "Clear" in a right-aligned `ButtonList` above the list, then `QueueList`. `EntryCount` is the number of item rows. With no item rows show the empty box (title and description from Task 3) instead of the list.
- [ ] **Step 3:** Handlers: `OnJump` → `ServiceControl.JumpTo(id, true)`; `OnMove` → `ServiceControl.MoveInQueue(dragKey, targetKey)`; `OnContextMenu` → `_menu.OpenAtAsync`; each wrapped in a `Run` that shows `exception.ToActionMessage()` as an error toast, as `Playlist.razor` does.
- [ ] **Step 4: Build** the App; expected: build succeeded.
- [ ] **Step 5: Stage** the new files.

### Task 7: Remove the popup, link the button, check by hand

**Files:**
- Delete: `Noxy.NET.MediaPlayer.App/Components/QueuePanel.razor`, `QueuePanel.razor.css`
- Modify: `Noxy.NET.MediaPlayer.App/Components/Scaffolding/FooterElement.razor` (remove `<QueuePanel/>`), `Noxy.NET.MediaPlayer.App/Components/MediaControls/MediaOutputControls.razor` (the queue button becomes navigation to `/queue`; keep its classes, ghost look and the `queue-icon`), `Noxy.NET.MediaPlayer.App/wwwroot/css/app.css` (the `PopoverSurface` rules stay; remove nothing the device list uses)

**Interfaces:**
- Consumes: `NavigationManager`; the route from Task 6.

- [ ] **Step 1:** Replace the `popovertarget` on the queue `Button` with `@onclick="ClickQueueHandler"` that calls `NavigationManager.NavigateTo("queue")`; add `[Inject]`/`@inject NavigationManager Navigation` as the neighbouring components do. The route text is a constant on the `Queue` page (`public const string Route = "queue";`) and the button uses it; `@page "/queue"` stays a literal because Razor requires one.
- [ ] **Step 2:** Delete `QueuePanel` files with `git rm`; grep for `QueuePanel` and `queue-panel`; expected: no matches outside `bin`/`obj`.
- [ ] **Step 3:** Build the App; expected: build succeeded. Run the whole unit suite (`.../Noxy.NET.Test.UnitTest.exe`); expected: all pass.
- [ ] **Step 4: By hand** (the user runs the app): open the queue from the player bar with songs queued from a playlist and some added by hand; check the dividers, that the current song is near the top and follows a song change, that scrolling away shows "Back to playing", that clicking and dragging rows never closes or jumps the page, that multi-select and the menu work (single and several rows), that Clear empties it and shows the empty box, that a 5,000-song library queue scrolls smoothly, and that the sidebar and playlist pages are unchanged.
- [ ] **Step 5: Stage** the modified files.
