# Organise Page Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task, **natively** (the user chose this: the implementer does every task in one session, then one fresh reviewer checks the whole branch; no subagent per task). Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** A new Organise page below Library: a library-style table (tick, title, artist, album, genre) with a right-hand column of Artist/Album/Genre tabs for managing items, filtering by clicking an item, and assigning through the context menu.

**Architecture:** The server and the client's read side are done. This plan adds the client's write calls and a service on top of them, a Fluxor state for the three item lists and one for the page's choices (tab, filters, selection, search, sort), makes `MediaFileTable` choose its columns, and builds the page, the column, the dialogs and the context menu in the App project. The page computes its visible list from the Library state's `MediaList` plus its own filters, so files are not loaded twice.

**Tech Stack:** .NET 10, MAUI Blazor Hybrid, Fluxor, xUnit v3 (run as an executable, see Global Constraints), `Noxy.NET.UI.Shared` components.

**Spec:** `docs/specs/2026-11-24-organise-page-design.md`

## Global Constraints

- Work in `Noxy.NET.MediaPlayer/*` (`Presentation` = logic, `App` = Razor) and tests in `Noxy.NET.Test/Noxy.NET.Test.UnitTest/MediaPlayer/Presentation` (namespace `Noxy.NET.Test.MediaPlayer.Presentation`); fakes in `.../MediaPlayer/Fakes`, builders in `.../MediaPlayer/Fixtures`. A test folder holds only tests.
- `dotnet test` does not work here. Build and run the executable, only for the area touched: `cd Noxy.NET.Test/Noxy.NET.Test.UnitTest && dotnet build -v q` then `./bin/Debug/net10.0/Noxy.NET.Test.UnitTest.exe -class "<full class name>"`. The whole suite runs once at the end (Task 9).
- **Git:** test files may be committed alone. Production code is **not committed**: stage each new file by name with `git add <path>` so it shows in the diff, and leave the commit to the user. Never `git add -A`.
- One class per file, file named after its class; no summary or doc comment at the top of a file or class; interfaces in an `Interfaces` folder (namespace mirrors the folder); enums are named `...Enum` in an `Enums` folder; every `switch` on an enum ends in a `default` that throws `InvalidEnumArgumentException`; domain entities carry the `Entity` prefix; lists are named with a `List` suffix (`ArtistList`); injected collaborators are named kind first (`serviceMetadataAction`, `clientApi`, `storeDevicePreference`); extensions go in an `Extensions` folder; no helper classes; no `yield`; use `is null` / `is not null`; avoid `!` (throw with a message, or comment why it is safe); LINQ method syntax; no magic strings (constants or `nameof`, defined once); never compare doubles with `==`.
- In code, JSON, tests, comments and docs say "media file"; only user-facing text may say "file" or "song". Every user-facing text goes in `AppTexts.resx` plus `AppTexts.Designer.cs` (edited by hand; keep the trailing space inside `<data name="X" xml:space=...>` when editing the resx; `HardCodedTextRuleTest` fails on a literal text in markup).
- **Pages carry no CSS of their own** (no `Pages/*.razor.css`, no page rules in `app.css`): the page gets the existing `.Page` rules, and anything more comes from components that carry their own style (so the table-beside-column arrangement is styled by a component, for example the sidebar component or a small styled layout component; if that cannot be done cleanly, stop and ask the user).
- Razor: no inline logic in attributes (named methods or properties, not lambdas, ternaries or `??`; a lambda only to carry a loop value); a page holds its components and its logic (no `...View` wrapper); use shared components from `Noxy.NET.UI.Shared` first; colours, gaps, sizes and fonts only as CSS variables from `variables.css`; a `::deep` is always followed by `>` and a child (`.Parent ::deep > .child`); components stay baseline (no animation, bold, icons unless asked).
- The server limits one assign or remove request to **1000** media file IDs (`RequestAddMediaFileList.MaxMediaFileCount`) and an item name to 255 characters. The library holds at most 5000 media files. The user's call: the Organise page **never holds more than 1000 ticked media files**; when a tick or "select all" would go past that, it ticks only what fits and shows a warning toast ("Only the first {0} media files were selected. Please narrow your search."). The service does not chunk.
- Spec values to keep exactly: route `/organise`; sidebar entry directly below Library; columns tick, title, artist, album, genre; tabs Artist, Album, Genre; last-assigned list holds **five** items per kind, newest first, kept on the device (not synced); one filter per kind, kinds combine; the filter clears on leaving the page; the header count reads "N of M files" while a filter or the search narrows the table.

## Review Focus

1. **"Select all" or a tick past 1000 ticked media files**: only what fits is ticked, the rest is left unticked, and the user is told with a warning toast, never a failed request (Task 3 tests, Task 5 toast).
2. **The filtered item is deleted** here or on another device: the filter clears instead of leaving an empty table nobody can explain (Task 3 test).
3. **Ticked media files a filter hides** stay ticked and are still what an action works on; "select all" only ticks what is shown, and the count says how many are hidden (Task 4 test).
4. **A media file with no artist, album or genre**: blank cells, never a crash, and it matches no filter on that kind (Task 4 test).
5. **A remembered "last assigned" item that no longer exists** is dropped when the list is read, and the same item is never listed twice (Task 2 test).

---

### Task 0: Shared `EmptyState` and `CardGrid`, and the page CSS files they replace

The user's call: pages carry no CSS, and these two pieces are generic, so they go in `Noxy.NET.UI.Shared`. Done first because Task 4 and the Organise page build on the result.

**Files:**
- Create: `Noxy.NET.UI/Noxy.NET.UI.Shared/Components/Containers/EmptyState.razor` (+ `.razor.css`), `.../Containers/CardGrid.razor` (+ `.razor.css`)
- Modify: `App/Components/MediaFileTable.razor` and `.razor.css` (use `EmptyState`, drop its own `.empty` rules), `App/Pages/Queue.razor` (use `EmptyState`, drop the page's own box), `App/Components/MediaFileHeader.razor` (+ `.css`) or `App/Components/PageHeader.razor` (the Clear button's place, see below), `App/Pages/Settings.razor`, `App/Components/Sections/SectionAccount.razor.css` and `SectionDevice.razor.css` (the card look)
- Delete: `App/Pages/Queue.razor.css`, `App/Pages/Settings.razor.css`. **Keep** `NotFound.razor.css` (the user: it is a default file and is fine).
- Modify: `Noxy.NET.UI/Noxy.NET.UI.Shared/README.md` (one line each for the two components)

**Interfaces:**
- Produces `EmptyState` (inherits `BaseComponentElement`): `[Parameter] RenderFragment? Graphic` (the picture above the title; the caller passes a `MaterialIcon`, the component has no icon of its own), `[Parameter] string? Title`, `[Parameter] string? Description`, `[Parameter] RenderFragment? ChildContent` (what can be done about it). Renders the title as a heading and the description as a paragraph only when set. It fills what is left of its container (flex, centred) and takes its look from variables with fallbacks, as `Section.razor.css` does: the dashed border `var(--size-border-width-md)` / `var(--color-offset-primary-lg)`, the muted text colour, the radius, the gap and padding.
- Produces `CardGrid` (inherits `BaseComponentElement`): `[Parameter] RenderFragment? ChildContent`; a grid that makes as many columns as fit (`repeat(auto-fit, minmax(min(100%, var(--size-column)), 1fr))`), cards as high as their content, gap and padding from variables, scrolls when too tall.
- The Clear button on the Queue page keeps its right-hand place without a page rule: give the header a slot for it (`MediaFileHeader` / `PageHeader`: a named `RenderFragment? EndContent` rendered after the counts with `margin-inline-start: auto` and the control height, in the header's own stylesheet), and the Queue page passes the button there.
- The card look (border, radius, padding, the line under the card's header) moves into the stylesheets of `SectionAccount` and `SectionDevice` (they own their `Section`), selecting the `Section`'s `.head` through `::deep > .head`.

- [ ] **Step 1:** Read `Noxy.NET.UI.Shared/README.md`, `Section.razor(.css)` and `MessageBox` for how much look a Shared component carries (the user's rule: shared UI is defaults plus `RenderFragment` hooks, `data-*` and structural CSS; no animation, bold or icons of its own). If the dashed border and muted colours do not fit that rule, stop and ask the user before going on.
- [ ] **Step 2:** Create the two components and their stylesheets; update the README.
- [ ] **Step 3:** Switch `MediaFileTable`, `Queue.razor` and `Settings.razor` to them; move the Clear button and the card look as above; delete the two page CSS files.
- [ ] **Step 4:** Build the App (`net10.0-windows10.0.19041.0`, app closed). Run it and compare before and after, reporting what you saw: an empty Library, an empty playlist, an empty Queue (same box as before, same fill), a queue with items (Clear button at the right of the header), and Settings at a wide and a narrow window (cards in a grid, borders and header lines as before). Package version of `Noxy.NET.UI.Shared` is not bumped unless the user asks.
- [ ] **Step 5:** Stage by name. No unit tests: the repository has no component tests, and nothing here has logic.

### Task 1: The client's write calls for artists, albums and genres

**Files:**
- Modify: `Noxy.NET.MediaPlayer.Presentation/Interfaces/Services/IApiClient.cs`, `Noxy.NET.MediaPlayer.Presentation/Services/ApiClient.cs`
- Modify: `Noxy.NET.Test.UnitTest/MediaPlayer/Fakes/FakeApiClient.cs`
- Test: `Noxy.NET.Test.UnitTest/MediaPlayer/Presentation/ApiClientTest.cs` (extend)

**Interfaces:**
- Produces, on `IApiClient`, for each kind K in `Artist`, `Album`, `Genre` (request types are the existing ones in `Noxy.NET.MediaPlayer.Application.Requests.Metadata`; the response is the kind's `Response` type):
  - `Task<ResponseK> CreateK(RequestCreateK request)` → `POST /K`
  - `Task<ResponseK> UpdateK(Guid id, RequestUpdateK request)` → `PATCH /K/{id}`
  - `Task DeleteK(Guid id)` → `DELETE /K/{id}` (204)
  - `Task<ResponseK> AddKMediaFileList(Guid id, RequestAddMediaFileList request)` → `POST /K/{id}/MediaFileList`
  - `Task<ResponseK> RemoveKMediaFileList(Guid id, RequestRemoveMediaFileList request)` → `POST /K/{id}/MediaFileList/Remove`
- `FakeApiClient` gets, per method, a `...Handler` func, a `...RequestList` of what was sent, and the call, in the style of `UpdatePlaylist` there.

- [ ] **Step 1: Write the failing tests** in `ApiClientTest`: for `CreateArtist`, `UpdateAlbum`, `DeleteGenre` and `AddArtistMediaFileList` / `RemoveArtistMediaFileList` assert the HTTP method, the path (`/Artist`, `/Album/{id}`, `/Genre/{id}`, `/Artist/{id}/MediaFileList`, `/Artist/{id}/MediaFileList/Remove`), the bearer header and that the JSON body carries the request (`Name`, or `MediaFileIDList`), following the existing playlist tests in that file.
- [ ] **Step 2:** Build and run `-class "Noxy.NET.Test.MediaPlayer.Presentation.ApiClientTest"`. Expected: compile error (members missing).
- [ ] **Step 3:** Add the 15 members to `IApiClient` (one comment line each where the answer is not obvious) and implement them in `ApiClient` with the existing `Write<T>` and `Send` helpers and the `ArtistPath`, `AlbumPath`, `GenrePath` constants already there; a `MediaFileListSegment` constant for `MediaFileList` and a `RemoveSegment` constant for `Remove`. Implement the fake.
- [ ] **Step 4:** Run the class again. Expected: PASS. Also build `Noxy.NET.MediaPlayer.App` for `net10.0-windows10.0.19041.0` is not needed yet.
- [ ] **Step 5:** `git add` the new/changed test and production files by name; commit the test files only.

### Task 2: `IMetadataActionService` and the last-assigned list

**Files:**
- Create: `Presentation/Enums/MetadataKindEnum.cs` (`Artist`, `Album`, `Genre`), `Presentation/Interfaces/Services/IMetadataActionService.cs`, `Presentation/Services/MetadataActionService.cs`, `Presentation/Interfaces/Services/IMetadataRecentService.cs`, `Presentation/Services/MetadataRecentService.cs`
- Modify: `Presentation/PresentationConstants.cs` (add `MetadataRecentCount = 5` and `OrganiseSelectionLimit = 1000`), `Presentation/ServiceCollectionExtensions.cs` (register both as the other services are), `Presentation/Interfaces/Services/IMetadataSyncService.cs` is not changed
- Test: `Noxy.NET.Test.UnitTest/MediaPlayer/Presentation/MetadataActionServiceTest.cs`, `MetadataRecentServiceTest.cs`; a fake `FakeDevicePreferenceStorage` in `Fakes` if none exists

**Interfaces:**
- Consumes: Task 1's `IApiClient` members; `IDevicePreferenceStorage` (`Get`, `Set`, `Remove`).
- Produces:
  - `Task<Guid> IMetadataActionService.Create(MetadataKindEnum kind, string name, List<Guid>? albumArtistIDList)` returns the new item's ID (`albumArtistIDList` is used for an album only).
  - `Task Rename(MetadataKindEnum kind, Guid id, string name, List<Guid>? albumArtistIDList)` (for an album, the title and, when not null, the artists).
  - `Task Delete(MetadataKindEnum kind, Guid id)`.
  - `Task Assign(MetadataKindEnum kind, Guid itemID, List<Guid> mediaFileIDList)`: adds an artist or genre, **sets** an album; one request; on success records the item in the recent list. A list of more than `PresentationConstants.OrganiseSelectionLimit` (1000) throws `ArgumentException` with a message (the page never sends one).
  - `Task Remove(MetadataKindEnum kind, Guid itemID, List<Guid> mediaFileIDList)`: the same limit; does not record.
  - `Task<List<Guid>> IMetadataRecentService.Get(MetadataKindEnum kind, IReadOnlySet<Guid> existingIDSet)`: newest first, only IDs in the set, at most `MetadataRecentCount`; `Task Record(MetadataKindEnum kind, Guid itemID)`: moves it to the front without duplicates and keeps five. Stored as JSON in `IDevicePreferenceStorage`, one key per kind built from one constant prefix.
  - Failures are not caught here: `ApiException` and `HttpRequestException` propagate, as in `MediaEditService`; the page shows them.

- [ ] **Step 1: Write the failing tests.** `MetadataActionServiceTest`: `Assign_Artist_CallsAddArtistMediaFileList_WithTheList`; `Assign_Album_CallsAddAlbumMediaFileList` (the server replaces); `Assign_MoreThanTheLimit_Throws_AndSendsNothing`; `Assign_Exactly1000_SendsOneRequest`; `Remove_Genre_CallsRemoveGenreMediaFileList_AndDoesNotRecord`; `Create_Album_SendsTitleAndArtistIDList_AndReturnsTheNewID`; `Assign_WhenTheServerRefuses_ThrowsAndRecordsNothing`. `MetadataRecentServiceTest`: `Record_PutsTheItemFirst_AndKeepsFive`; `Record_TheSameItemTwice_ListsItOnce`; `Get_DropsIDsThatNoLongerExist` (Review Focus 5); `Get_KindsDoNotShareAList`.
- [ ] **Step 2:** Build and run both classes. Expected: compile errors.
- [ ] **Step 3:** Implement. Chunking is `Chunk(1000)` over the list. The kind switch has the throwing default.
- [ ] **Step 4:** Run both classes. Expected: PASS.
- [ ] **Step 5:** Stage by name; commit tests only.

### Task 3: The item lists in Fluxor, and the Organise state

**Files:**
- Create (all under `Presentation/Features/`): `MetadataList/FeatureMetadataListState.cs` (`ArtistList`, `AlbumList`, `GenreList`), `MetadataList/ActionMetadataListLoaded.cs` (the three lists), `MetadataList/FeatureMetadataListReducers.cs`; `Organise/FeatureOrganiseState.cs`, `Organise/FeatureOrganiseReducers.cs` and one action class per action named below
- Modify: `Presentation/Services/StateBridgeService.cs` (inject `IMetadataListStore storeMetadataList` and `IMetadataSyncService serviceMetadataSync`; `LoadLists` also dispatches `ActionMetadataListLoaded`; subscribe to `serviceMetadataSync.Changed` like `serviceSync.Changed`, unsubscribe in `Dispose`)
- Test: `FeatureMetadataListReducersTest.cs`, `FeatureOrganiseReducersTest.cs`, and the existing `StateBridgeService` test file if one exists (else add `StateBridgeServiceMetadataTest.cs` with a fake store and a recording dispatcher)

**Interfaces:**
- Produces `FeatureOrganiseState` (a record): `MetadataKindEnum Tab = Artist`, `string Search = ""`, `LibrarySortEnum Sort = Title`, `bool Descending`, `HashSet<Guid> SelectedSet`, `Guid? ArtistFilterID`, `Guid? AlbumFilterID`, `Guid? GenreFilterID`, and `bool HasFilter`, `Guid? FilterOf(MetadataKindEnum kind)`, `int SelectionRoom` (the limit minus the ticked count, never below 0).
- Actions (each a record in its own file) and their reducers: `ActionOrganiseTabChanged(MetadataKindEnum Tab)`, `ActionOrganiseSearchChanged(string Search)`, `ActionOrganiseSortChanged(LibrarySortEnum Sort)` (same sort again reverses, as in Library), `ActionOrganiseFilterToggled(MetadataKindEnum Kind, Guid ItemID)` (same item clears; another item of the kind replaces), `ActionOrganiseFilterCleared(MetadataKindEnum? Kind)` (null clears all), `ActionOrganiseSelectionToggled(Guid MediaFileID)` (unticking always works; ticking is ignored when `SelectedSet.Count` is already `PresentationConstants.OrganiseSelectionLimit`), `ActionOrganiseVisibleSelected(List<Guid> VisibleIDList, bool IsAllSelected)` (unticks all of the list when `IsAllSelected`, else ticks the list in order only until the limit is reached; hidden ticks are left alone), `ActionOrganiseSelectionCleared`, `ActionOrganiseReset` (back to a new state; sent when the page is left).
- Cross-feature reducers (in `FeatureOrganiseReducers`): on `ActionMetadataListLoaded`, a filter whose item is not in the loaded list is cleared; on `ActionLibraryMediaLoaded`, ticked IDs no longer in the library are dropped.

- [ ] **Step 1: Write the failing tests** (names are the behaviours): `FilterToggled_SameItemTwice_Clears`; `FilterToggled_AnotherItemOfTheKind_Replaces`; `FilterToggled_DifferentKinds_Combine`; `FilterCleared_Null_ClearsAll`; `MetadataListLoaded_ClearsAFilterWhoseItemIsGone` (Review Focus 2); `LibraryMediaLoaded_DropsTickedIDsNotInTheLibrary`; `VisibleSelected_TicksTheShown_AndLeavesHiddenTicksAlone` and `..._WhenAllShownAreTicked_UntickThemOnly` (Review Focus 3); `VisibleSelected_With1500Shown_TicksOnlyTheFirst1000InOrder`; `VisibleSelected_WithSomeAlreadyTicked_TicksOnlyWhatFits`; `SelectionToggled_At1000_IgnoresATick_ButAllowsAnUntick`; `SelectionRoom_NeverBelowZero`; `Reset_ReturnsTheStartState`; `SortChanged_SameSortReverses_NewSortStartsAscending`; `MetadataListLoaded_ReplacesTheThreeLists`; the bridge test: `a change raised by the metadata sync service dispatches ActionMetadataListLoaded with the stored lists`.
- [ ] **Step 2:** Build and run the new classes. Expected: compile errors.
- [ ] **Step 3:** Implement the state, actions, reducers and the bridge change. Fluxor allows several reducers for one action across features.
- [ ] **Step 4:** Run the new classes and the existing `FeatureLibraryReducersTest`. Expected: PASS.
- [ ] **Step 5:** Stage by name; commit tests only.

### Task 4: Visible-list logic, genre sort and the table's columns

**Files:**
- Modify: `Presentation/Extensions/ResponseMediaFileListExtension.cs` (add `FilterByMetadata(Guid? artistID, Guid? albumID, Guid? genreID)`; a `Genre` case in the sort), `Presentation/Enums/LibrarySortEnum.cs` (add `Genre`)
- Create: `App/Enums/MediaFileTableColumnEnum.cs` (`Select`, `State`, `Title`, `Artist`, `Album`, `Genre`, `Year`, `Added`)
- Modify: `App/Components/MediaFileTable.razor` and `MediaFileTable.razor.css` (Task 0 already moved its empty box to `EmptyState`)
- Test: `ResponseMediaFileListExtensionTest.cs` (extend)

**Interfaces:**
- Produces: `List<ResponseMediaFile> FilterByMetadata(Guid? artistID, Guid? albumID, Guid? genreID)`: a null kind is not filtered; a file with no item of a filtered kind is dropped. `MediaFileTable` gets `[Parameter] public List<MediaFileTableColumnEnum>? ColumnList`; null means exactly today's columns (Select, State, Title, Artist, Album, Year, Added) so Library, Queue and Playlist are unchanged. The genre cell shows the response's `Genre` text.

- [ ] **Step 1: Write the failing tests:** `FilterByMetadata_AllNull_ReturnsEveryFile`; `..._Artist_KeepsFilesWithThatArtistAmongSeveral`; `..._Album_And_Genre_Combine`; `..._AFileWithNoArtist_MatchesNoArtistFilter_AndIsKeptWhenNotFiltering` (Review Focus 4); `Sort_ByGenre_OrdersByTheGenreText_BlankLast`.
- [ ] **Step 2:** Build and run `ResponseMediaFileListExtensionTest`. Expected: compile errors.
- [ ] **Step 3:** Implement the extension and the sort. Make `MediaFileTable` render the header cells and body cells from `ColumnList` through one small method per column kind (a `switch` with the throwing default), replacing the hard-wired cells; the grid template in the stylesheet must follow the chosen columns (a CSS variable for the column count or per-column classes; the builder reads the stylesheet first). Genre header is sortable with `LibrarySortEnum.Genre`.
- [ ] **Step 4:** Run the extension tests (PASS). Build the App for `net10.0-windows10.0.19041.0`, run the app (skill `run`) and confirm Library, a playlist and the Queue look exactly as before. Close the app before rebuilding (it locks its `.exe`).
- [ ] **Step 5:** Stage by name; commit tests only.

### Task 5: The page, the sidebar entry and the filter badges

**Files:**
- Create: `App/Pages/Organise.razor` (no `.razor.css`: pages carry no CSS of their own), `App/Components/OrganiseFilterBadgeList.razor` (+ `.css`)
- Modify: `App/Components/Scaffolding/SidebarElement.razor` (a `NavLink href="organise"` directly after Library), `App/Resources/AppTexts.resx` and `AppTexts.Designer.cs`

**Interfaces:**
- Consumes: Task 3 state and actions; Task 4 `FilterByMetadata`, `MediaFileTableColumnEnum`; `FeatureLibraryState.MediaList`.
- Produces: `@page "/organise"`. The page holds `PageHeader`-based `MediaFileHeader` (name "Organise", counts text "N of M files" while the search or a filter narrows, else the normal count), the `MediaFileTable` with `ColumnList = [Select, Title, Artist, Album, Genre]`, its search box and `OrganiseFilterBadgeList` in `ToolContent`, and an empty state with a "Clear filters" button when filters leave nothing. The visible list is `Library.MediaList.Filter(search).FilterByMetadata(...).Sort(...)`, rebuilt only when its inputs change (the reference check Library's `RowList` uses). The page dispatches `ActionOrganiseReset` in `Dispose`. **The limit warning:** before dispatching a tick or "select all" the page checks `SelectionRoom` against how many would be added; when not all fit it dispatches anyway (the reducer ticks what fits) and shows `ServiceToast` a warning with the text above (`{0}` = the limit). A test cannot cover the page, so the check is a small method on the state or an extension (`int ToTickCount`, tested in Task 3 style) that the page calls. `OrganiseFilterBadgeList`: parameters `ArtistName`, `AlbumName`, `GenreName` (string?) and `EventCallback<MetadataKindEnum> OnClear`, `EventCallback OnClearAll`; one badge per active filter with an ✕ and a "Clear all" when more than one.
- New texts: sidebar entry, page title, "N of M files", filter badge labels ("Artist: {0}", "Album: {0}", "Genre: {0}"), badge remove label, "Clear all", empty-filter title and "Clear filters", the column title Genre.

- [ ] **Step 1:** Write the texts and the sidebar entry; build (resx is valid XML or the build fails with MSB3103).
- [ ] **Step 2:** Build the page and badge list. No unit test (no component test infrastructure in the repo); verification is Step 3.
- [ ] **Step 3:** Run the app. Expected: the sidebar shows Organise below Library; the page shows the five columns, the search works, ticking and "select all" work, leaving and returning shows no filter. Describe what you saw; do not claim it works without running it.
- [ ] **Step 4:** Stage by name.

### Task 6: The right-hand column

**Files:**
- Create: `App/Components/OrganiseSidebar.razor` (+ `.css`), `App/Components/MetadataItemList.razor` (+ `.css`), `App/Components/Dialogs/MetadataItemDialog.razor` (+ `.css`)
- Modify: `Organise.razor` (place the column to the right of the table; below it in a narrow window, following how `Queue` handles `NarrowWatcher`), `AppTexts.resx` and `AppTexts.Designer.cs`

**Interfaces:**
- Consumes: Task 2 `IMetadataActionService`; Task 3 state and actions; `TabSet`, `TabPage`, `ConfirmActionDialog`, `InputControlText`.
- Produces:
  - `MetadataItemList`: `[Parameter] MetadataKindEnum Kind`; reads the kind's list from `FeatureMetadataListState`; its own search (`InputControlText` `Type=Search`, ignoring case) and a `+` button on the same row; rows show the name (album: title), the file count, ✎ and 🗑 icon buttons; clicking the name dispatches `ActionOrganiseFilterToggled`; the filtered row is highlighted (accent colour and a left bar, via a `data-` attribute and CSS variables); no items and no match have their own text. Each row is one element carrying the item ID in a `data-` attribute so it can become a drop target later.
  - `OrganiseSidebar`: `TabSet` with three tabs, a dot on a tab whose kind has a filter, hosts three `MetadataItemList`s and the dialogs.
  - `MetadataItemDialog`: `[Parameter] bool Open`, `EventCallback<bool> OpenChanged`, `MetadataKindEnum Kind`, `Guid? ItemID` (null creates), `EventCallback OnSaved`; a name field (trimmed, required, at most 255); for an album also an artist picker built from the artist list; saves through `IMetadataActionService.Create` / `Rename`; a thrown `ApiException` (for example the 409 that names the other item) is shown in the dialog with its message, and the dialog stays open.
  - Delete: `ConfirmActionDialog` naming the item and the number of files it touches, then `IMetadataActionService.Delete`; if the item was the filter, the Task 3 reducer already clears it.

- [ ] **Step 1:** Write the texts (tab titles, search placeholders, add and edit and delete labels, dialog titles, delete confirmation with `{0}` name and `{1}` count, empty and no-match texts, the 409 fallback).
- [ ] **Step 2:** Build the three components and the dialog; wire them into `Organise.razor`.
- [ ] **Step 3:** Run the app against a server with data. Verify and report what you saw: create an artist (and one with a name that exists), rename, delete (files stay, the tag leaves them), click to filter and click again to clear, the three filter indicators (badge, highlighted row, tab dot), an album's artist picker, and the narrow-window layout.
- [ ] **Step 4:** Stage by name.

### Task 7: The context menu and the picker

**Files:**
- Create: `App/Components/OrganiseContextMenu.razor` (+ `.css`), `App/Components/Dialogs/MetadataPickerDialog.razor` (+ `.css`)
- Modify: `Organise.razor` (open the menu on right-click of a row and from the header's actions button, as `Library.razor` does), `AppTexts.resx` and `AppTexts.Designer.cs`

**Interfaces:**
- Consumes: `LibraryContextMenu` (read it first: it keeps `ItemList`, `OpenAtAsync(MouseEventArgs, Guid)` and the `ContextMenuItem` helpers), Task 2 services, Task 3 state.
- Produces: `OrganiseContextMenu` with the parameters of `LibraryContextMenu` (`SelectedSet`, `SelectedKeyList`, `ShowTrigger`). It contains the Library actions (the trial from the spec: build it by composing or extending `LibraryContextMenu` so nothing in Library changes; if the Library menu cannot be extended without changing its behaviour, extract the shared item building into a method both use) plus three submenus built like `AddToPlaylistItem`:
  - **Artist** and **Genre**: first entry "Choose…" opens `MetadataPickerDialog`; separator; the recent items (`IMetadataRecentService.Get`) each calling `Assign`; then "Remove" with a submenu or picker for removing.
  - **Album**: "Choose…" (a "Set" label), recent albums, and "Remove" (no picker: it clears the album of the target files by removing each album they have; the builder picks the simplest correct way from the files' `AlbumID`).
  - The target is the right-clicked row or, from the header button, the ticked rows (`SelectedIDList` of the Organise selection), as in Library.
  - `MetadataPickerDialog`: like `AddToPlaylistDialog` (search field, list in name order with counts, `OnChosen` with the ID) plus a first "New…" row that opens `MetadataItemDialog` and, once saved, chooses the new item. Parameters `bool Open`, `EventCallback<bool> OpenChanged`, `MetadataKindEnum Kind`, `EventCallback<Guid> OnChosen`.
  - After a successful assign or remove a toast says what happened and how many media files; a failure shows the exception text (`ServiceToast`, as the Library menu does).

- [ ] **Step 1:** Write the texts.
- [ ] **Step 2:** Build the menu and the picker; wire into the page.
- [ ] **Step 3:** Run the app. Verify and report: right-click one file and assign an artist; tick several and add a genre; set an album; remove; the recent list shows the last five and drops a deleted item; "select all" with more than 1000 media files showing ticks exactly 1000 and shows the warning toast (use a temporary large library or a lowered limit constant, then restore it); the Library actions in the menu work (play next, add to playlist); Library's own menu is unchanged.
- [ ] **Step 4:** Stage by name.

### Task 8: Spec and handover notes

**Files:**
- Modify: `docs/specs/2026-11-15-library-organizer-design.md` (its Status line: the panel and drag idea became a page, link `2026-11-24-organise-page-design.md`), `docs/specs/2026-11-15-frontend-screens-design.md` (the "left for later" line points to the new spec)
- Modify: `docs/specs/2026-11-24-organise-page-design.md` (set Status to "implemented", with a short "How it ended up" list of anything that differs from this design)

- [ ] **Step 1:** Make the three edits above.
- [ ] **Step 2:** Stage by name.

### Task 9: Whole-suite check

- [ ] **Step 1:** `cd Noxy.NET.Test/Noxy.NET.Test.UnitTest && dotnet build -v q && ./bin/Debug/net10.0/Noxy.NET.Test.UnitTest.exe`. Expected: all pass (the known flaky `PlaybackControlServiceTest.Seek_ADragOfManySteps_...` may fail once under load; rerun it alone).
- [ ] **Step 2:** Build the App: `dotnet build Noxy.NET.MediaPlayer/Noxy.NET.MediaPlayer.App -f net10.0-windows10.0.19041.0` with the app closed. Expected: 0 errors.
- [ ] **Step 3:** Report to the user: what was built, the three things seen running in Tasks 5-7, any deviation from the spec, and that nothing is committed except the test files. Ask whether this plan file can be deleted now that the work is done.
