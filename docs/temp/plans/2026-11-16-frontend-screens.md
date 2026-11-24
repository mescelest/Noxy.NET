# Frontend Screens Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:executing-plans (the user wants this done natively, in the main session, not through subagents). Steps use checkbox (`- [ ]`) syntax for tracking. **Do not start before these are done, in this order: `2026-11-16-backend-session-removal-and-playlist-modes.md`, `2026-11-16-backend-remote-playback.md`, `2026-11-16-frontend-sync-settings-metadata-session.md`, `2026-11-16-frontend-remote-playback.md`.** Every playback action in the screens goes through `IPlaybackControlService` (`StartLibrary`, `StartPlaylist`, `Play`, `Pause`, `Seek`, `Next`, `Previous`, `SetVolume`, `SetShuffle`, `SetRepeat`, `PlayNext`, `AddToQueue`, `RemoveFromQueue`, `MoveInQueue`, `Transfer`, `CanContinue`, `Continue`); the player bar, the queue panel and the device picker read `IPlayerService` (local state), `IRemotePlaybackService` (`Role`, `DeviceList`, `Remote`, `GetPosition()`) and the `LiveQueue` of `IQueueService`; the Settings page reads `ISettingService` (including `KeepQueuedItems`) and `IDeviceService`.

**Goal:** Build everything the user sees in the MAUI app on Windows: sign-in, the shell (header, sidebar, player bar), Home, Library, playlists, queue, Account and Settings, all themed through CSS variables that are easy to change.

**Architecture:** Logic lives in `Noxy.NET.MediaPlayer.Presentation` (Fluxor state, reducers, view services, tested in the unit project with fakes). The App holds thin Razor components built on the shared `Noxy.NET.UI` libraries (layout, table, button, dialog, collapsible, tabs, WebForm, icons, theme toggle); two things are added to those libraries (a virtualized table and a sortable header cell, plus the missing icons). Every colour, size, gap, font and duration is a CSS custom property defined once in the App's `variables.css`, with the names of the entity management project's file; a test scans the style files so no literal creeps into a component.

**Tech Stack:** .NET 10, .NET MAUI Blazor Hybrid (Windows first), Blazor `Virtualize`, Fluxor 6.11.0 (`Fluxor` in `Presentation`, `Fluxor.Blazor.Web` in the App), the `Noxy.NET.UI` Razor libraries, xunit v3.

**Spec:** `docs/specs/2026-11-15-frontend-screens-design.md` (outline approved 2026-11-16; the look is explicitly open, see "Styling through variables") with `2026-11-15-frontend-maui-design.md` for the platform. Previous plans (done): the frontend foundation, `2026-11-15-frontend-downloads-player.md`, `2026-11-16-frontend-sync-settings-metadata-session.md`.

## Global Constraints

- Work in `Presentation` (features and view services), `App` (components, styles, platform pieces), `Noxy.NET.UI.Table` and `Noxy.NET.UI.Graphics` (the additions in Tasks 2 and 8), and the two test projects. Windows first; Android is deferred. The backend is not changed. **No MAUI, Blazor or browser type in `Presentation`** (the `Fluxor` core package is allowed there; `Fluxor.Blazor.Web` is not).
- All conventions of the earlier plans apply: one class per file, file named after it, no summary comment above a class, every enum named `...Enum` and kept in an `Enums` folder of its project (namespace mirroring the folder), interfaces in `Interfaces/<same folder as the implementation>`, extensions in `Extensions` (the class named after the type it extends), `List` suffix, no `yield`, no magic strings, `is null`, no `!`, method-syntax LINQ, "media" never "song", kind-first dependency names (`serviceLibraryAction`, `storeMetadata`), no helper classes, zero warnings, no commits, `git add -N` new files by name, no machine-level commands. Tests: unit in `Noxy.NET.Test.UnitTest/MediaPlayer/Presentation/`, fakes in `Fakes`, helpers in `Fixtures`, a test folder holds only tests, `TestContext.Current.CancellationToken` on every call that takes a token, build then run the exe.
- **Styling through variables (the spec's rule).** No component style contains a literal colour, `font-family`, duration or size: only `var(--…)`. Literals live in `wwwroot/css/variables.css` (names and groups as `Noxy.NET.EntityManagement.Administration/wwwroot/css/variables.css`: `--size-text-*`, `--size-gap-*`, `--size-border-*`, `--size-control-*`, `--size-icon-*`, `--animation-length-*`, `--font-*`, `--color-*`, `--color-primary`/`--color-complementary` with `color-mix` offsets) and the one media-query file `wwwroot/css/breakpoints.css`. Values in the mockups are placeholders; the defaults are the entity management project's.
- **No test project can reference the App or the UI libraries** (a MAUI project cannot be referenced from the test projects, and the UI libraries have no test project). So everything with logic is in `Presentation` and unit tested; a component task ends with a **Manual check** list that the user runs, and the executor starts the app to confirm it launches and stays responsive. An executor never claims a component works from the build alone.
- Offline: the sidebar, Home, Library and the queue read only from the local store and work offline; an action that needs the server is disabled with its reason shown, never silently failing.
- Layout, from the spec: a header (sidebar toggle, logo, search bar, room on the right), a sidebar in sections (Your music: Home, Library, Account, Settings; Integrations: YouTube, Bandcamp; Playlists: "New playlist" first, then the playlists most recently played first), a centre at least the full height between header and player bar that scrolls inside itself, and the player bar along the bottom. Home: up to 6 "Recently played" playlist cards, and one "Continue" card only when `IPlaybackControlService.CanContinue` (this device has a local copy of a queue, no device is active and nothing is playing). Library: a glass panel pinned above the scrolling list (row 1: title, file and selection counts, the Actions button disabled until a row is ticked, the search box; row 2: column headers with a select-all tick, click to sort and click again to reverse), one row per media file with a tick box, the on-device status icon (on this device, downloading, not downloaded) and the columns. **Double-clicking a row plays the Library from there:** `IPlaybackControlService.StartLibrary(mediaFileID, sort, descending, search)` with the page's current sort and search, so the queue is the media files the list shows, in that order (shuffled by the effective shuffle mode, repeated by the effective repeat mode), and new media files join it where the order puts them. Ticked rows are only for bulk actions: local to the page, never synced and never a queue. Actions menu items, in this order: Download to this device, Save as file, Add to playlist, Remove from this device, Delete from library (last, in the error colour); there is no Play item. **The player starts disabled** (decided 2026-11-16): on a first start-up, after a deleted source, after an account switch and whenever there is no current item (no queue), the player bar says "Nothing playing" with its controls disabled and Home shows no Continue card and no error; the app opens on Home and never plays by itself, so the way back is a click on the latest playlist's card, a double-click in the Library, or Continue when this device has a local copy of a queue. The player bar's shuffle button cycles `Off`, `Random`, `SpreadArtists` and its repeat button `No`, `Once`, `Forever`, each showing the effective mode and writing the playing playlist's own mode when a playlist plays and the global setting otherwise. Sign-in: no shell while signed out; one card with the logo, the provider buttons (Google first), a divider, then tabs Sign in and Create account with email and password; a failed attempt shows only the server's message.

## Review Focus

- **Hidden selected rows:** a filter can hide ticked rows; the count always shows the real number selected, select-all ticks only the rows that match the search, and a bulk action says how many rows it will touch, including the hidden ones (Tasks 3, 9, 10).
- **Dead references and the disabled player:** Home never shows a playlist that is gone or a Continue card when there is no local copy; an open playlist deleted on another device shows a "gone" state instead of an error and the playing queue finishes its current media file and then the player disables; the player queue and the playlist page drop a deleted media file; the disabled player shows no stale title or position (Tasks 12, 13, 14).
- **A different account on the same device:** signing in as another user never shows or plays the previous user's library; the local copy is wiped when the account changes (Task 4).
- **Offline:** an app started with no network shows the shell and the local library, says it is offline, retries by itself and on demand, and a refused server action says why (Task 5, and every action task).
- **Sign-in messages:** a failed sign-in shows the server's own message and nothing else (no stack, no Google error text); a cancelled Google sign-in changes nothing on screen (Task 7).

## File Structure

New files (relative to the project named):
- `Presentation/Features/<Name>/` for `Shell`, `Session`, `Library`, `PlaylistList`, `Player`, `Download`: one file per type, so `Features/Library/FeatureLibraryState.cs`, `Features/Library/ActionLibrarySearchChanged.cs`, `Features/Library/FeatureLibraryReducers.cs`, and so on; `Presentation/Services/`: `StateBridgeService.cs`, `LocalDataOwnerService.cs`, `LibraryActionService.cs`, `PlaylistService.cs`, `HomeService.cs`; `Presentation/Models/`: `BulkResult.cs`, `HomeModel.cs`, `PlayerSnapshot.cs`; `Presentation/Enums/`: `RestoreResultEnum.cs`, `LibraryActionEnum.cs`; `Presentation/Extensions/ResponsePlaylistListExtension.cs`
- `App/wwwroot/css/`: `variables.css`, `breakpoints.css`, `app.css`; `App/Components/Layout/`: `MainLayout.razor`, `Header.razor`, `Sidebar.razor`, `PlayerBar.razor`, `QueuePanel.razor`; `App/Components/Pages/`: `Login.razor`, `Home.razor`, `Library.razor`, `Playlist.razor`, `Account.razor`, `Settings.razor`, `Youtube.razor`, `Bandcamp.razor`; `App/Components/`: `GlassPanel.razor`, `ActionsMenu.razor`, `FileStatusIcon.razor`, `PlaylistCard.razor`, `ContinueCard.razor`, `NewPlaylistDialog.razor`; `App/Stores/PreferencesDevicePreferenceStorage.cs`, `App/Services/ThemeService.cs`
- `Noxy.NET.UI.Table`: `Enums/SortDirectionEnum.cs`, `Components/TableSortHeaderCell.razor`; `Noxy.NET.UI.Graphics/Components/Icons/`: the missing icons of Task 2
- Tests: unit `Fakes/FakeDispatcher.cs`, `Fakes/FakeDevicePreferenceStorage.cs`, `Fakes/FakeFileSaver.cs`, `Fixtures/StyleFileFixture.cs`, `StyleRuleTest.cs`, reducer tests per feature, `StateBridgeServiceTest.cs`, `LocalDataOwnerServiceTest.cs`, `LibraryActionServiceTest.cs`, `PlaylistServiceTest.cs`, `HomeServiceTest.cs`, `ResponsePlaylistListExtensionTest.cs`

Modified: `App.csproj` (references to the UI projects and Fluxor), `wwwroot/index.html`, `MauiProgram.cs`, `Routes.razor`, `_Imports.razor`, `ISessionService`/`SessionService`, `IAppStartupService`/`AppStartupService`, `IApiClient`/`ApiClient`/`FakeApiClient`, `IMetadataStore` (owner), `Table.razor`, both `ServiceCollectionExtensions.cs`, the design docs; removed at the end: the temporary diagnostic `Home.razor`.

Contracts every task shares:
- Fluxor naming as the entity management project: state `FeatureXState`, actions `ActionXVerb`, reducers class `FeatureXReducers` with `[ReducerMethod]` methods; reducers are pure and return a new state record.
- `IDevicePreferenceStorage` (`Presentation/Interfaces/Stores`): `Task<string?> Get(string key)`, `Task Set(string key, string value)`, `Task Remove(string key)`; the App implements it over MAUI `Preferences`; the keys are constants in `Presentation/Constants.cs` (`SidebarOpenKey`, `ThemeKey`).

---

### Task 1: Styling foundation, the UI references and Fluxor

**Files:**
- Create: `App/wwwroot/css/variables.css`, `App/wwwroot/css/breakpoints.css`, `App/wwwroot/css/app.css`; tests `Fixtures/StyleFileFixture.cs`, `StyleRuleTest.cs`
- Modify: `App/Noxy.NET.MediaPlayer.App.csproj` (project references to `Noxy.NET.UI.Shared`, `.Shared.Fluxor`, `.Elements`, `.Table`, `.Dialog`, `.WebForm`, `.Graphics`; package `Fluxor.Blazor.Web` 6.11.0), `Presentation.csproj` (package `Fluxor` 6.11.0), `App/wwwroot/index.html`, `App/Components/_Imports.razor`, `App/MauiProgram.cs` (`AddFluxor` over the `Presentation` assembly and the App), `App/Components/Routes.razor` (`<Fluxor.Blazor.Web.StoreInitializer/>`)

**Interfaces:** Produces the stylesheet set below. `variables.css` holds the groups of the entity management file under the same names (text, gap, border, control, icon and page sizes, animation lengths, `--font-family-primary` with the label/body/button shorthands, the semantic colours, `--color-primary` and `--color-complementary` with their `color-mix` offsets) plus the app's own tokens: `--size-layout-sidebar`, `--size-layout-header`, `--size-layout-player-bar`, `--size-library-row`, `--size-card-width`, `--size-card-height`, `--size-queue-panel`, `--glass-blur`, `--glass-opacity`, `--glass-border-color`, and light and dark values switched by `:root[theme="Light"]` and `:root[theme="Dark"]`. `breakpoints.css` is the only place a literal media query condition may appear. `index.html` links `variables.css`, `breakpoints.css` and `app.css` and drops the template's bootstrap.

- [ ] **Step 1: Write the failing test** `StyleRuleTest` with `StyleFileFixture` (finds the repository root by walking up from the test's base directory to `Noxy.NET.sln`, then lists every `*.css` under `Noxy.NET.MediaPlayer.App` except `variables.css`, `breakpoints.css`, anything under `bin` or `obj`, and `wwwroot/lib`): `NoComponentStyle_HoldsAColourLiteral` (no `#rgb`, `#rrggbb`, `#rrggbbaa`, `rgb(`, `rgba(`, `hsl(`, `hsla(` and no named colour other than `transparent`, `currentColor`, `inherit`), `NoComponentStyle_SetsAFontFamilyLiteral` (every `font-family` value is `var(...)`), `NoComponentStyle_HoldsADurationLiteral` (no `\d+m?s` in `transition`, `animation` or their duration properties unless inside `var(...)`), `NoComponentStyle_HoldsASizeLiteral` (no `\d+(px|rem|em)` except `0`, `1px` borders written as `var(--size-border-width-sm)` instead, so none at all); a unit test of the scanner itself on a string with a violation and on a clean string, so the rule is proven to bite. `variables.css` must define every `--size-gap-*`, `--size-text-*` and `--color-*` name the shared UI libraries use: `VariablesCss_DefinesEveryVariableTheUiLibrariesRead` scans the `Noxy.NET.UI` `*.razor.css` files for `var(--name` and asserts each `--name` is defined in `variables.css`.
- [ ] **Step 2:** Run `*StyleRuleTest`; expected: failure (no `variables.css` yet). Create the three stylesheets so the tests pass; the reset and base rules in `app.css` use variables only.
- [ ] **Step 3:** Add the references and `AddFluxor`; build `App` for `net10.0-windows10.0.19041.0`; expected: 0 warnings. If a UI library cannot be referenced from MAUI (a browser-only package), stop and report which one and why instead of working around it.
- [ ] **Step 4:** Start the app; expected: it launches and stays responsive with the unstyled placeholder page. Run both test projects; expected: all pass.
- [ ] **Leave uncommitted.**

---

### Task 2: Icons

**Files:**
- Create in `Noxy.NET.UI.Graphics/Components/Icons/` (same pattern as `IconPlus.razor`, drawn in the 100 by 100 view box with `currentColor`): `IconPlay`, `IconPause`, `IconVolume`, `IconVolumeMuted`, `IconSearch`, `IconMenu` (the hamburger), `IconMore` (three dots), `IconQueue`, `IconHome`, `IconLibrary`, `IconAccount`, `IconSettings`, `IconDownload`, `IconOnDevice`, `IconNotDownloaded`, `IconTrash`, `IconMusic` (the logo mark placeholder)

- [ ] **Step 1:** Add each icon as a component inheriting `IconComponent`; reuse `IconBackwards`, `IconForward`, `IconFastBackwards`, `IconFastForward`, `IconChevron*`, `IconPlus`, `IconTick` and `IconSaltire` where one fits (previous and next in the player bar use the existing fast icons unless the user prefers another look).
- [ ] **Step 2:** Build the solution for both targets; expected: 0 warnings. Manual check: render all icons once on a temporary page (removed afterwards) and confirm each reads at the three icon sizes and takes its colour from the surrounding text colour.
- [ ] **Leave uncommitted.**

---

### Task 3: Fluxor features and the state bridge

**Files:**
- Create: the six features of File Structure (state record, one file per action, reducers), `Models/PlayerSnapshot.cs`, `Interfaces/Services/IStateBridgeService.cs`, `Services/StateBridgeService.cs`, `Fakes/FakeDispatcher.cs`; tests per reducer class and `StateBridgeServiceTest.cs`

**Interfaces:**
- Consumes: `ISyncService`, `ISessionService`, `IDownloadService`, `IPlayerService`, `IMetadataStore` (`GetMediaFileList`, `GetPlaylistList`, `GetFileStateList`), Fluxor's `IDispatcher`, the existing `Filter` and `Sort` extensions.
- Produces: `FeatureLibraryState` (`List<ResponseMediaFile> MediaList`, `Dictionary<Guid, FileSyncStateEnum> StateMap`, `string Search`, `LibrarySortEnum Sort` (default `Title`), `bool Descending`, `HashSet<Guid> SelectedSet`, and the derived `List<ResponseMediaFile> VisibleList`, `int SelectedCount`, `int HiddenSelectedCount`, `bool AllVisibleSelected`); actions `ActionLibraryMediaLoaded(List<ResponseMediaFile>)`, `ActionLibraryStateLoaded(List<FileState>)`, `ActionLibrarySearchChanged(string)`, `ActionLibrarySortChanged(LibrarySortEnum)`, `ActionLibrarySelectionToggled(Guid)`, `ActionLibraryVisibleSelected()`, `ActionLibrarySelectionCleared()`; `FeaturePlaylistListState(List<ResponsePlaylist> PlaylistList)` with `ActionPlaylistListLoaded`; `FeatureSessionState(bool IsSignedIn, bool HasSession, bool IsOffline, bool IsRetrying)` with `ActionSessionChanged`; `FeatureShellState(bool SidebarOpen)` with `ActionShellSidebarSet(bool)`; `FeatureDownloadState(DownloadProgress Progress)` with `ActionDownloadChanged`; `FeaturePlayerState(PlayerSnapshot Snapshot)` with `ActionPlayerChanged`; `record PlayerSnapshot(PlayerQueueEntry? Current, List<PlayerQueueEntry> QueueList, bool IsDisabled, bool IsPlaying, bool IsWaitingForDownload, TimeSpan Position, TimeSpan Duration, double Volume, ShuffleModeEnum Shuffle, RepeatModeEnum Repeat)`; `StateBridgeService(IDispatcher dispatcher, ISyncService serviceSync, ISessionService serviceSession, IDownloadService serviceDownload, IPlayerService servicePlayer, IMetadataStore storeMetadata)` implementing `IStateBridgeService { void Start(); Task Refresh(); }`.

- [ ] **Step 1: Write the failing tests.** Library reducers: `SearchChanged_FiltersTheVisibleList`; `SortChanged_WithANewSort_SortsAscending_AndWithTheSameSortReversesIt`; `SelectionToggled_AddsAndRemoves_AndKeepsTheSelectionWhenTheSortChanges`; `VisibleSelected_SelectsOnlyTheRowsThatMatchTheSearch` (Review Focus); `HiddenSelectedCount_CountsSelectedRowsTheSearchHides` and `SelectedCount_IsTheRealCount`; `AllVisibleSelected_IsTrueOnlyWhenEveryVisibleRowIsTicked_AndFalseForAnEmptyList`; `VisibleSelected_WhenEveryVisibleRowIsTicked_UnticksThemButNotTheHiddenOnes` (select-all toggles); `MediaLoaded_DropsSelectedIDsThatLeftTheLibrary`; `MediaLoaded_KeepsTheSearchAndSort`. Other reducers: each action replaces its state. Bridge: `Start_DispatchesTheCurrentSessionLibraryPlaylistDownloadAndPlayerState`; `SyncChanged_ReloadsTheMediaAndPlaylistLists_NotTheFileStates`; `DownloadChanged_ReloadsTheFileStatesAndTheProgress_NotTheMediaList` (a download tick never rereads the library); `PlayerChanged_DispatchesAFreshSnapshot`; `SessionChanged_DispatchesTheSessionState`; `ABurstOfChanges_RefreshesAtMostOnceWhileOneRefreshIsRunning_ThenOnceMore`; `Start_CalledTwice_SubscribesOnce`.
- [ ] **Step 2:** Run the new classes; expected: compile failure. Implement; pass. Register `IStateBridgeService` in `AddPresentation`, start it from `MauiProgram` after the start-up service.
- [ ] **Leave uncommitted.**

---

### Task 4: A different account wipes the previous user's local copy

**Files:**
- Create: `Interfaces/Services/ILocalDataOwnerService.cs`, `Services/LocalDataOwnerService.cs`, `Extensions/StringExtensions` addition (`GetAccessTokenSubject() : Guid?`); tests `LocalDataOwnerServiceTest.cs`
- Modify: `IMetadataStore`, `FakeMetadataStore`, `SqliteMetadataStore` (`Task<Guid?> GetOwnerUserID()`, `Task SetOwnerUserID(Guid? id)`, in the key-value table; contract tests), `AppStartupService` and the sign-in paths (after a successful sign-in or restore, call `EnsureOwner`)

**Interfaces:**
- Consumes: `ISessionService.GetAccessToken()`, `IMetadataStore`, `IFileStore`.
- Produces: `LocalDataOwnerService(ISessionService serviceSession, IMetadataStore storeMetadata, IFileStore storeFile, IDownloadService serviceDownload, IPlayerService servicePlayer)` implementing `ILocalDataOwnerService { Task EnsureOwner(); }`: reads the signed-in user's ID from the access token's `sub` claim; with no owner stored it records it; with the same owner it does nothing; with a different owner it stops the player (`Clear`), cancels downloads, deletes every held and partial file, and replaces every list, ETag, file state and the local queue copy (`ILocalQueueStore.Clear()`) with empty ones before recording the new owner, so nothing of the previous account is ever shown or played.

- [ ] **Step 1: Write the failing tests** (Review Focus): `EnsureOwner_WithNoStoredOwner_RecordsTheUser_AndKeepsTheData`; `EnsureOwner_ForTheSameUser_ChangesNothing`; `EnsureOwner_ForADifferentUser_WipesLibraryPlaylistsETagsFileStatesAndSession_AndRecordsTheNewOwner`; `…_DeletesHeldAndPartialFiles`; `…_StopsThePlayerAndCancelsDownloads`; `…_WhenSignedOut_DoesNothing`; a token with no readable `sub` does nothing and never wipes (a failure to read must not destroy data); a contract test for the owner on both stores; `GetAccessTokenSubject` tests for a valid token, garbage, and a payload with no `sub`.
- [ ] **Step 2:** Run the new classes; expected: compile failure. Implement; pass. Call `EnsureOwner` first thing after `TryRestore` succeeds and after an interactive sign-in succeeds (before any sync).
- [ ] **Leave uncommitted.**

---

### Task 5: Offline start-up: signed in without a network, and retry

**Files:**
- Create: `Enums/RestoreResultEnum.cs` (`SignedIn`, `NoSession`, `Rejected`, `Offline`)
- Modify: `Interfaces/Services/ISessionService.cs`, `Services/SessionService.cs` (`TryRestore` now returns `Task<RestoreResultEnum>`; `bool HasSession` is true when signed in or when a refresh token is stored and the server could not be reached), `FakeSessionService`, `Interfaces/Services/IAppStartupService.cs`, `Services/AppStartupService.cs`, `Constants.cs` (`StartupRetryBaseSeconds` = 5, `StartupRetryMaxSeconds` = 60), the existing session, start-up and integration tests

**Interfaces:**
- Produces: `IAppStartupService { Task Start(); Task RetryNow(); bool IsRetrying { get; } event Action? Changed; }`. `Start` on `Offline`: the repair still runs, the local data stays available, `HasSession` is true, and a retry loop through `IDelay` waits 5 s, 10 s, 20 s, up to 60 s between tries, calls `TryRestore` again, and on `SignedIn` runs the rest of start-up (`EnsureOwner`, the syncs, the hub, the reporter) exactly once and stops; `NoSession` and `Rejected` end the loop with no retry; `RetryNow` tries at once; `IsRetrying` is true while the loop waits; `Changed` is raised when it changes.

- [ ] **Step 1: Write the failing tests** (Review Focus): `TryRestore_WhenTheNetworkIsDown_ReturnsOffline_KeepsTheStoredToken_AndHasSessionIsTrue`; `TryRestore_WithNoStoredToken_ReturnsNoSession_AndHasSessionIsFalse`; `TryRestore_WhenRejected_ReturnsRejected_ClearsTheToken_AndHasSessionIsFalse`; `Start_WhenOffline_RetriesWithGrowingDelays_AndCapsAt60Seconds` (the fake delay records `[5, 10, 20, 40, 60, 60]`); `Start_WhenTheRetrySucceeds_RunsTheRestOfStartUpOnce_AndStops`; `Start_WhenTheRetryIsRejected_StopsRetrying`; `RetryNow_TriesAtOnce`; `IsRetrying_IsTrueWhileWaiting_AndChangedIsRaised`; `Start_WhenOffline_StillRepairsTheFiles`. Update the existing tests for the new return type.
- [ ] **Step 2:** Run the unit and integration projects; expected: compile failure, then implement until all pass.
- [ ] **Leave uncommitted.**

---

### Task 6: The shell: layout, header, sidebar and routing

**Files:**
- Create: `App/Components/Layout/MainLayout.razor`, `Header.razor`, `Sidebar.razor` (and their `.razor.css`), `App/Components/Pages/Youtube.razor`, `Bandcamp.razor`, `App/Stores/PreferencesDevicePreferenceStorage.cs`, `Presentation/Interfaces/Stores/IDevicePreferenceStorage.cs`, `Fakes/FakeDevicePreferenceStorage.cs`, `Presentation/Features/` additions for the sidebar persistence (an effect that stores `SidebarOpen` and a bridge call that reads it on start), tests for that effect
- Modify: `App/Components/Routes.razor` (a guard: no shell and the Login page while `HasSession` is false; the shell otherwise), `Constants.cs` (`SidebarOpenKey`)

**Interfaces:** Consumes the features of Task 3 and the sidebar section layout from the Global Constraints. Produces: `MainLayout` on `SidebarLeftLayout` with the header on top and a slot for the player bar (Task 14) at the bottom; the sidebar sections as `Collapsible` per section with links; the sidebar toggle button in the header (`IconMenu`) flipping `FeatureShellState.SidebarOpen`; on a narrow window (the breakpoint in `breakpoints.css`) the sidebar starts hidden and opens over the centre; the choice is remembered per device through `IDevicePreferenceStorage`; the header search box dispatches `ActionLibrarySearchChanged` and navigates to `/library`; room on the right shows the offline state (Task 5) and a Sign out button. The Youtube and Bandcamp pages are an empty page with a short "coming later" text in a `MessageBox`.

- [ ] **Step 1: Write the failing tests** for the logic only: `SidebarSet_IsStored_AndRestoredOnStart`; `AMissingOrUnreadableStoredValue_MeansOpen`; `SidebarSet_WhenStoringFails_DoesNotThrow`.
- [ ] **Step 2:** Run the new class; expected: compile failure. Implement the logic, then the components. Build for Windows (0 warnings); start the app.
- [ ] **Manual check (user):** signed out: only the login page (Task 7) shows, no shell; signed in: the shell shows header, sidebar and centre with the right sections; the hamburger hides and shows the sidebar and the choice survives a restart; shrinking the window makes the sidebar start hidden and open over the centre; the centre fills the height between header and the bottom edge and scrolls inside itself; the header search sends you to the Library; with the API stopped before start-up the shell still opens and says offline.
- [ ] **Leave uncommitted.**

---

### Task 7: Sign-in and the server address

**Files:**
- Create: `App/Components/Pages/Login.razor` (+ `.razor.css`)
- Modify: `Routes.razor`

**Interfaces:** Consumes `ISessionService` (`SignIn`, `SignUp`, `SignInWithGoogle`), `IServerAddressStorage`, `IAppStartupService.Start`, `ILocalDataOwnerService`. Produces the screen of the spec: one card with the logo, the provider list (a Google button first; each provider is one entry of a list so another is one more entry), a divider, `TabSet` with the tabs Sign in and Create account (email and password, `WebForm` with the request classes' data annotations); a server address field above the card on a first run or when none is saved, saved through `IServerAddressStorage` (normalised, a bad address shows its message); after a successful sign-in the page calls `Start` and the router moves to Home. An `ApiException` shows its `Title` (the server's message) in the form's message box; any other failure shows one fixed sentence; a cancelled Google sign-in shows nothing.

- [ ] **Step 1:** Add the logic that decides what the form shows to `Presentation` if any is non-trivial (an extension `ExceptionExtension.ToSignInMessage(): string` returning the `ApiException` title or the fixed sentence, never an exception message) with tests: `ApiException_ShowsItsTitle`, `AnyOtherException_ShowsTheFixedSentence_AndNoExceptionText`, `AnOfflineFailure_SaysTheServerCannotBeReached`.
- [ ] **Step 2:** Run it; expected: compile failure. Implement it and the page. Build for Windows (0 warnings); start the app.
- [ ] **Manual check (user):** a first run asks for the server address and refuses a malformed one with a message; wrong email or password shows only the server's message; creating an account signs in and opens Home; **Sign in with Google** opens the browser, signs in, and cancelling the browser leaves the page as it was; Sign out returns to this page and shows nothing of the previous library after signing in as another account.
- [ ] **Leave uncommitted.**

---

### Task 8: A virtualized table and a sortable header cell (UI library)

**Files:**
- Create: `Noxy.NET.UI.Table/Enums/SortDirectionEnum.cs` (`None`, `Ascending`, `Descending`), `Noxy.NET.UI.Table/Components/TableSortHeaderCell.razor` (+ `.razor.css` using variables only)
- Modify: `Noxy.NET.UI.Table/Components/Table.razor`

**Interfaces:** `Table<T>` gains `[Parameter] public bool Virtualize { get; set; }` and `[Parameter] public float ItemSize { get; set; } = 48` (an estimate; Blazor measures the real height): when `Virtualize` is true the rows are rendered through `Virtualize<T>` over `List` with the same `TableRow` fragment and `KeySelector`, and the header stays outside the scrolling items; when false nothing changes for existing users. `TableSortHeaderCell`: `[Parameter] RenderFragment? ChildContent`, `[Parameter] SortDirectionEnum Direction`, `[Parameter] EventCallback OnClick`; it renders a button-like cell with the title and a chevron (`IconChevronUp`, `IconChevronDown`, none for `None`) and `aria-sort`.

- [ ] **Step 1:** Implement both; no unit test project exists for the UI libraries, so write a temporary diagnostic page in the App that shows `Table` with 5000 rows virtualized and not (removed afterwards).
- [ ] **Step 2:** Build the solution for both targets (0 warnings); the existing apps that use `Table` (entity management) must still build.
- [ ] **Manual check (user):** with 5000 rows the virtualized table scrolls smoothly and the page stays responsive while a non-virtualized one is visibly slower; clicking a header cell cycles its indicator; the header stays visible; no existing table page of the entity management apps changed.
- [ ] **Leave uncommitted.**

---

### Task 9: The Library page

**Files:**
- Create: `App/Components/Pages/Library.razor`, `App/Components/GlassPanel.razor`, `FileStatusIcon.razor` (+ styles)

**Interfaces:** Consumes `FeatureLibraryState` and its actions (Task 3), `Table<T>` with `Virtualize` and `TableSortHeaderCell` (Task 8), the icons (Task 2), `ActionsMenu` (Task 10, a placeholder button until then). Produces the page of the Global Constraints: `GlassPanel` pinned to the top of the centre with `backdrop-filter: blur(var(--glass-blur))` and its colours from variables; row 1: the title, "N files · M selected" (the real selected count and, when some are hidden, "K hidden by the search"), the Actions button, the search box (bound to `Search`, so the header search and this box are one value); row 2: the select-all tick (ticked when `AllVisibleSelected`, toggling `ActionLibraryVisibleSelected`) and the sortable column headers (Title, Artist, Album, Year, Added) with the direction indicator on the active one; one row per media file with a tick box, `FileStatusIcon` (on this device, downloading, not downloaded, failed, from `StateMap`), and the columns; an empty library shows a `MessageBox` with "Upload media on the server" or the offline reason; a double-click on a row calls `IPlaybackControlService.StartLibrary(mediaFileID, Sort, Descending, Search)` from `FeatureLibraryState` (the played list is therefore exactly what the page shows); nothing here triggers a download except playing.

- [ ] **Step 1:** Implement the page and components. Build for Windows (0 warnings); start the app. Run both test projects.
- [ ] **Manual check (user):** with a few thousand media files (add them through the API; the diagnostic Home page keeps its upload button until Task 13) scrolling is smooth; typing in the search box filters at once and the count follows; clicking a header sorts, again reverses; select-all ticks only the matching rows; ticking rows then searching keeps the real selected count and says how many are hidden; the glass panel stays pinned and the text stays readable on both themes; the status icons change as a download runs; double-clicking a row plays it and Next continues through the list as sorted and filtered (search "a", sort by Year descending, then double-click a row and check the following ones follow that order); with shuffle on the same double-click plays the clicked media file first and then a shuffled order; uploading a media file on the server while it plays adds it to the queue only where the order puts it; changing a colour or a gap in `variables.css` changes the page with no other edit.
- [ ] **Leave uncommitted.**

---

### Task 10: Library actions

**Files:**
- Create: `Interfaces/Services/ILibraryActionService.cs`, `Services/LibraryActionService.cs`, `Models/BulkResult.cs`, `Enums/LibraryActionEnum.cs`; tests `LibraryActionServiceTest.cs`
- Modify: `IApiClient`, `ApiClient`, `FakeApiClient` (`Task DeleteMedia(Guid id)`: `DELETE /Media/{id}`, `204`; test in `ApiClientTest`)

**Interfaces:**
- Consumes: `IDownloadService`, `IFileStore`, `IMetadataStore`, `IPlayerService`, `IApiClient` (`DeleteMedia`, `AddPlaylistEntryList`), `ISyncService.IsOffline`.
- Produces: `record BulkResult(int SucceededCount, List<Guid> FailedIDList, string? Message)`; `LibraryActionService(IDownloadService serviceDownload, IFileStore storeFile, IMetadataStore storeMetadata, IPlayerService servicePlayer, IApiClient clientApi, ISyncService serviceSync)` implementing `ILibraryActionService { Task<BulkResult> Download(List<Guid> mediaFileIDList); Task<BulkResult> RemoveFromDevice(List<Guid> mediaFileIDList); Task<BulkResult> DeleteFromLibrary(List<Guid> mediaFileIDList); Task<BulkResult> AddToPlaylist(Guid playlistID, List<Guid> mediaFileIDList); string? GetDisabledReason(LibraryActionEnum action); }` with `Enums/LibraryActionEnum.cs` (`Download`, `SaveAsFile`, `AddToPlaylist`, `RemoveFromDevice`, `DeleteFromLibrary`). `Download` uses `QueueMissing` (so the low-space stop applies); `RemoveFromDevice` takes the media files out of the player queue first (the playing one stops and releases its file), cancels any download, then deletes the file and the file state, and never touches the library; `DeleteFromLibrary` calls `DeleteMedia` one by one and reports the failures (the push removes the rows); `GetDisabledReason` returns the offline sentence for `DeleteFromLibrary`, `AddToPlaylist` and `SaveAsFile` when `IsOffline`, and null otherwise.

- [ ] **Step 1: Write the failing tests:** `Download_QueuesOnlyTheMissingOnes`; `RemoveFromDevice_DeletesTheFileAndState_ButKeepsTheLibraryEntry`; `RemoveFromDevice_OfThePlayingMediaFile_StopsThePlayerFirst_SoTheFileIsReleased` (Review Focus; the fake player and file store record the order); `RemoveFromDevice_CancelsARunningDownload`; `DeleteFromLibrary_DeletesEachOnTheServer_AndReportsTheOnesThatFailed`; `DeleteFromLibrary_WhenOffline_DeletesNothing_AndSaysWhy`; `AddToPlaylist_SendsOneRequestWithAllTheMediaFiles`; `AddToPlaylist_ForAFullPlaylist_ReportsTheServersMessage`; `GetDisabledReason_IsNullOnlineAndTheOfflineSentenceOffline`; an empty ID list changes nothing and reports success with zero.
- [ ] **Step 2:** Run the new classes; expected: compile failure. Implement; pass.
- [ ] **Leave uncommitted.**

---

### Task 11: The Actions menu and its confirmations

**Files:**
- Create: `App/Components/ActionsMenu.razor` (+ style), `App/Components/ConfirmActionDialog.razor`
- Modify: `Library.razor`

**Interfaces:** Consumes `ILibraryActionService`, `BooleanDialog` and `ModalDialog` (Noxy.NET.UI.Dialog), `FeatureLibraryState`. Produces the three-dots menu (`IconMore`), disabled until `SelectedCount > 0`, with the five items in the Global Constraints' order, built from one list so an item is one more entry; an item that has a `GetDisabledReason` is disabled and shows it; "Add to playlist" opens a picker of the user's playlists; "Remove from this device" and "Delete from library" ask first with the count it will touch and, when `HiddenSelectedCount > 0`, say that some of those rows are hidden by the search ("Delete 5 media files from your library? 2 of them are hidden by the current search."); "Delete from library" is last and uses the error colour variable; a `BulkResult` with failures shows how many failed and the first message in a `MessageBox`. "Save as file" is present and handled in Task 16.

- [ ] **Step 1:** Implement the menu and dialogs. Build for Windows (0 warnings); start the app.
- [ ] **Manual check (user):** the button is disabled with nothing ticked; Download starts the downloads and the status icons follow; Remove from this device on a downloaded file deletes it from the data folder and the icon changes, also while that file is playing; Delete from library asks first, names hidden rows, removes the rows and the file; Add to playlist adds and the playlist page shows them; offline the server actions are disabled with the reason.
- [ ] **Leave uncommitted.**

---

### Task 12: Playlists: sidebar section, New playlist and the playlist page

**Files:**
- Create: `Interfaces/Services/IPlaylistService.cs`, `Services/PlaylistService.cs`, `Extensions/ResponsePlaylistListExtension.cs`; tests `PlaylistServiceTest.cs`, `ResponsePlaylistListExtensionTest.cs`; `App/Components/Pages/Playlist.razor`, `NewPlaylistDialog.razor`, `PlaylistCard.razor` (used again in Task 13)
- Modify: `Sidebar.razor` (the Playlists section: a New playlist button, then the list)

**Interfaces:**
- Consumes: `IApiClient` (`CreatePlaylist`, `UpdatePlaylist`, `DeletePlaylist`, `RemovePlaylistEntryList`, `MovePlaylistEntry`), `ISyncService` (`SyncPlaylistList`, `SyncPlaylistDetail`), `IDownloadService`, `IMetadataStore`, `IPlayerService`.
- Produces: `extension(List<ResponsePlaylist> playlistList) { public List<ResponsePlaylist> OrderMostRecentlyPlayedFirst() }` (played playlists by `TimeLastPlayed` newest first, then never-played ones by `TimeModified` newest first, stable, a new list); `PlaylistService(...)` implementing `IPlaylistService { Task<ResponsePlaylist> Create(string name); Task Open(Guid id); Task Refresh(Guid id); Task DownloadAll(Guid id); Task Play(Guid id, Guid? startEntryID = null); Task Rename(Guid id, string name); Task SetAllowDuplicates(Guid id, bool allow); Task SetShuffle(Guid id, ShuffleModeEnum? mode); Task SetRepeat(Guid id, RepeatModeEnum? mode); Task RemoveEntryList(Guid id, List<Guid> entryIDList); Task MoveEntry(Guid id, Guid entryID, int newPosition); Task Delete(Guid id); }` (`SetShuffle` and `SetRepeat` write the playlist's own mode, null meaning "follow the global setting"). `Open` syncs the playlist's detail and then enqueues the downloads of its media files that are missing (the sync design: opening a playlist downloads its media files; nothing else does); `Play` calls `IPlaybackControlService.StartPlaylist(playlistID)`; every write sends the request, saves the answer (detail and summary) into the store at once, and throws when offline or refused so the page shows the reason.

- [ ] **Step 1: Write the failing tests:** ordering (`PlayedFirstNewestPlayedFirst`, `NeverPlayedAfterThemByModifiedTime`, `ATiedTimeKeepsTheInputOrder`, `DoesNotChangeTheInputList`); `Create_PostsTheName_SavesTheSummary_AndReturnsIt`; `Open_SyncsTheDetail_ThenEnqueuesOnlyTheMissingMediaFiles`; `Open_WhenOffline_StillReturns_AndEnqueuesWhatTheStoredDetailListsMissing`; `OpeningTheList_ByReadingIt_StartsNoDownload` (the page's data read is a store read); `DownloadAll_QueuesEveryEntryNotOnTheDevice`; `Play_CallsStartPlaylist_WithThePlaylistID`; `Play_FromAnEntry_PassesTheEntryID`; `SetShuffle_And_SetRepeat_SendOnlyThatModeAsAPlaylistUpdate_AndNullClearsIt`; `Rename_And_SetAllowDuplicates_SendAnUpdateWithOnlyThatField`; `RemoveEntryList_SendsTheEntryIDs_AndSavesTheNewDetail`; `MoveEntry_SendsThePosition_AndSavesTheNewDetail`; `Delete_DeletesOnTheServer_AndRemovesTheLocalPlaylistAndDetail`; every write when offline throws and changes nothing locally; a playlist deleted elsewhere: `Open_When404_RemovesTheLocalPlaylist_AndSaysItIsGone` (Review Focus).
- [ ] **Step 2:** Run the new classes; expected: compile failure. Implement the services, then the sidebar section, the dialog and the page (the entries as `SortableTable`/`SortableList` with drag-to-reorder calling `MoveEntry`, a tick box per entry, a double-click on an entry calling `Play(id, entryID)`, the same Actions menu, a play button, "Download all", "Refresh", the duplicates setting, and two selects for this playlist's shuffle and repeat mode, each with a "Follow my setting" choice that sends null). Build for Windows (0 warnings); start the app.
- [ ] **Manual check (user):** New playlist asks for a name, opens the new playlist and it appears in the sidebar; the order of the sidebar is most recently played first; opening a playlist downloads its files and nothing else does; drag reorders and survives a restart; removing entries, Refresh and the duplicates switch work; double-clicking an entry plays from it through the playlist; a playlist's own shuffle and repeat survive a restart and show on a second device while a playlist with "Follow my setting" follows the Settings page; removing an entry from the playlist you are listening to takes it out of the queue without interrupting the one playing; deleting a playlist on another device shows "this playlist was deleted" on the open page and removes it from the sidebar, and if it was playing the current media file finishes and the player then shows "Nothing playing".
- [ ] **Leave uncommitted.**

---

### Task 13: Home

**Files:**
- Create: `Interfaces/Services/IHomeService.cs`, `Services/HomeService.cs`, `Models/HomeModel.cs`; tests `HomeServiceTest.cs`; `App/Components/Pages/Home.razor` (replacing the diagnostic), `ContinueCard.razor` (+ styles)

**Interfaces:**
- Consumes: `IMetadataStore.GetPlaylistList`, `IPlaybackControlService.CanContinue` and `Continue`, `ILocalQueueStore.Get` (the saved queue's current media file for the title), `ResponsePlaylistListExtension`, `IPlaylistService.Play`.
- Produces: `record HomeModel(List<ResponsePlaylist> RecentList, ContinueInfo? Continue, bool HasPlaylists)` and `record ContinueInfo(string? Title, string? Artist, double PositionSeconds)` (from the local copy and the media file in the library); `HomeService(IMetadataStore storeMetadata, IPlaybackControlService serviceControl, ILocalQueueStore storeLocalQueue)` implementing `IHomeService { Task<HomeModel> Get(); }` with `RecentList` the playlists that have a `TimeLastPlayed`, newest first, at most 6 (`Constants.HomeRecentCount`); when none was ever played, the first 6 of the sidebar order; `Continue` only when `CanContinue` is true.

- [ ] **Step 1: Write the failing tests:** `Get_ReturnsAtMostSixPlayedPlaylists_NewestFirst`; `Get_WhenNothingWasPlayed_ListsThePlaylistsInTheSidebarOrder`; `Get_WithNoPlaylists_HasPlaylistsIsFalse`; `Get_IncludesTheContinueInfo_AndTheTitleOfTheCurrentMediaFile`; `Get_WhenCannotContinue_HasNoContinue` (a live session, a playing device or no local copy gives none, so the page is just the playlists); `Get_NeverIncludesAPlaylistThatIsNotInTheLocalList` (Review Focus: a deleted playlist cannot appear); `Get_ReadsOnlyTheLocalStore` (no `IApiClient`).
- [ ] **Step 2:** Run it; expected: compile failure. Implement; pass. Build `Home.razor`: "Recently played" cards (`PlaylistCard`: a click calls `IPlaylistService.Play`, a menu opens it; the first card is the latest played and the page shows nothing else when the player is disabled, so playing the latest playlist is one click), "Continue" as one card that calls `IPlaybackControlService.Continue`, an empty-state prompt to create a playlist. The page never starts playback by itself. Remove the diagnostic page's content. Build for Windows (0 warnings); start the app.
- [ ] **Manual check (user):** after playing a playlist it moves to the front of the cards; after a session ends (about 10 minutes idle) the Continue card shows on a device that saw it and clicking it resumes the queue at the saved media file and position; the card is absent while any device plays or a session is live; deleting a playlist on another device removes its card; with none played the cards list the playlists; with none at all a prompt to create one shows.
- [ ] **Leave uncommitted.**

---

### Task 14: The player bar and the queue

**Files:**
- Create: `App/Components/Layout/PlayerBar.razor`, `QueuePanel.razor` (+ styles)
- Modify: `MainLayout.razor` (the bottom slot)

**Interfaces:** Consumes `FeaturePlayerState.Snapshot`, `IPlaybackControlService` for every command (it sends the command to the active device when this device is a controller, otherwise plays locally) and `IRemotePlaybackService` for what the active device is playing (title, position by `GetPosition()`, playing or paused, volume, shuffle, repeat), and the bar shows that remote state when this device is a controller, `ISyncService`/session state for the offline marker, the icons. Produces: the bar of the spec: previous, play/pause, next, the current title and artist (read from the media file in the library list), a seek bar showing `Position` of `Duration` that sends `Seek` on release, a volume control that calls `SetVolume` on release (Task 10 of the sync plan writes the setting), a shuffle button cycling `Off`, `Random`, `SpreadArtists` and a repeat button cycling `No`, `Once`, `Forever` (each showing the effective mode, calling `SetShuffle` and `SetRepeat`, and showing the refusal's reason in a `MessageBox` when the write throws; a mode that is the playing playlist's own is marked as such), the state ("Waiting for a download", "Offline"), and a click on the title opens `QueuePanel` (the current entry, then the user queue ("Next in queue") and the source part ("Next from <source>") as one virtualized list (history is behind a "Previously played" fold), since a Library queue can hold thousands, with the current marked, their file state icon, a remove button per entry calling `RemoveFromQueue` and a click on any entry (history, user queue or source part) calling `JumpTo(itemID, keepSkipped: true)` (the queue panel always keeps skipped songs and has no toggle; the flag stays in the API and the control service so other callers can choose, decided by the user), a drag handle on upcoming entries calling `MoveInQueue` (both synced to every device through the live session), "Clear"); a failed entry shows the failed icon; nothing plays from the server. **When `IsDisabled` the bar shows "Nothing playing" and no title, position or queue, every control is disabled and the queue panel is not offered**, and nothing about it looks like an error.

- [ ] **Step 1:** Implement the bar and panel. Build for Windows (0 warnings); start the app.
- [ ] **Manual check (user):** play a downloaded media file: it plays, pause and resume work, the seek bar tracks and seeks, the volume changes the sound and survives a restart; Next and Previous move through the queue; a missing media file shows "Waiting for a download" then plays; the queue panel shows the entries and their states, removing the current one plays the next, a media file deleted on the server leaves the queue after the current one ends; shuffle changes the order of what comes next without replaying what already played, `SpreadArtists` keeps one artist's media files apart, Repeat Once plays the whole queue exactly one more time and Forever loops; the same shuffled queue continues on a second device in the same order; a freshly started app, a deleted playing playlist (after its current media file ends) and a signed-out and in-again account all show "Nothing playing" with disabled controls; with the API stopped a downloaded media file still plays.
- [ ] **Leave uncommitted.**

---

### Task 14a: The device picker and the queue actions

**Files:**
- Create: `App/Components/Layout/DevicePicker.razor`, `DeviceNameField.razor` (+ styles); modify `PlayerBar.razor`, `QueuePanel.razor`, `Library.razor`/the playlist page rows' menu (the Actions menu of Task 11 gains items), `Settings.razor` (the device name)

**Interfaces:** Consumes `IRemotePlaybackService` (`DeviceList`, `Role`, `ActiveDeviceID`, `IsOnline`, `Changed`), `IPlaybackControlService` (`Transfer`, `RemoveFromQueue`, `PlayNext`, `AddToQueue`, `TransferFailed`), `IDeviceService` (`GetName`, `SetName`). Produces: a device button in the player bar showing the active device's name (or "This device") that opens `DevicePicker`, a list of the online devices with the active one marked and this one first; choosing another calls `Transfer` and shows "Moving playback to X…", a `MessageBox` with the reason on a `TransferFailed` push or a thrown error; **offline** the button is disabled with the reason "Offline"; the player bar when this device is a controller shows the active device's state (its own seek bar by `GetPosition()`), with a small "Playing on X" label; "Nothing active" with a "Play here" action when no device is active and a session exists (the Continue flow); in the Actions menu and the queue rows the items **Play next** and **Add to queue** (any media file, from the Library or any playlist) next to the existing ones, the queue panel's per-entry remove, and the device name editable on the Settings page (`SetName`, the refusal's reason shown in a `MessageBox`). Reordering is by drag in the queue panel only (`MoveInQueue`). Colours and sizes from `variables.css`.

- [ ] **Step 1:** Implement. Build for Windows (0 warnings); start the app.
- [ ] **Manual check (user, two devices on the account):** the second device appears in the picker within a moment of starting; pressing play on the phone while the laptop plays controls the laptop; a transfer moves the sound and the position; play next and add to queue from a different playlist change the laptop's queue; removing a queued item removes it on both; seek and volume dragged on the phone move the laptop once on release; closing the laptop makes the phone show "Nothing active" after the grace period and Play here plays on the phone; turning the phone's network off disables the picker with "Offline" but local playback still works; reconnecting the phone while the laptop plays makes the phone pause and show "Playing on Laptop".
- [ ] **Leave uncommitted.**

---

### Task 15: Account, Settings and the theme

**Files:**
- Create: `App/Components/Pages/Account.razor`, `Settings.razor` (+ styles), `App/Services/ThemeService.cs` (implements the UI library's `IThemeService<ThemeEnum>` over `IDevicePreferenceStorage`; read the entity management app's implementation first and follow it), `App/Enums/ThemeEnum.cs` (`Light`, `Dark`)
- Modify: `MauiProgram.cs` (register the theme service), `Constants.cs` (`ThemeKey`)

**Interfaces:** Consumes `ISessionService`, `ISettingService`, `IFileStore.GetEstimate`, `ISyncService`, the existing `ThemeToggle`. Produces: **Account**: the signed-in email (read from the access token's `email` claim through an `GetAccessTokenEmail()` extension with tests), Sign out, and the storage used on this device (the held files' total and the free space, from `GetEstimate`). **Settings**: the `ThemeToggle`, "Refresh library" (a forced `SyncLibrary(true)` and playlist sync), what is stored on this device (counts of media files on device and bytes), and the synced user settings listed from `ISettingService.GetSettingList()` each with an editor for its kind (boolean switch, integer input within the declared range, enum select) writing through `Update`, disabled with the offline reason when offline; a failed write shows the server's message and the value reverts. A theme choice sets the `theme` attribute on the root so the variable blocks of Task 1 apply.

- [ ] **Step 1:** Add `GetAccessTokenEmail` with tests (`ReadsTheEmailClaim`, `ReturnsNullForGarbageOrNoClaim`); implement the theme service and both pages. Build for Windows (0 warnings); start the app. Run both test projects.
- [ ] **Manual check (user):** Account shows your email and the storage figures; Sign out returns to the login page; the theme switch changes every colour at once and survives a restart; a synced setting changed here appears on a second device (and the volume setting moves the player's volume); offline the settings are read-only with the reason; Refresh library syncs.
- [ ] **Leave uncommitted.**

---

### Task 16: Save as file

**Files:**
- Create: `Interfaces/Services/IFileSaver.cs` (`Task<bool> Save(string suggestedName, Stream content)`), `App/Services/FileSaver.cs` (the MAUI file picker for saving), `Fakes/FakeFileSaver.cs`; tests added to `LibraryActionServiceTest.cs` and `ApiClientTest.cs`
- Modify: `IApiClient`/`ApiClient`/`FakeApiClient` (`Task<ApiFileResponse> GetTaggedFile(Guid id)`: `GET /Media/{id}/Download`, streamed like `GetFile`), `ILibraryActionService`/`LibraryActionService` (`Task<BulkResult> SaveAsFile(List<Guid> mediaFileIDList)`), `ActionsMenu.razor`

**Interfaces:** `SaveAsFile` asks `IFileSaver` once per media file (or once for a folder when more than one, if the platform offers it) and writes the tagged copy the server builds, streamed to the chosen place; a cancelled save is not an error; offline it is disabled with the reason.

- [ ] **Step 1: Write the failing tests:** `GetTaggedFile_GetsTheDownloadPath_AndStreamsTheBody`; `SaveAsFile_StreamsEachMediaFileToTheSaver_UsingItsTitleAsTheSuggestedName`; `SaveAsFile_WhenTheUserCancels_IsNotAFailure`; `SaveAsFile_WhenOffline_SavesNothing_AndSaysWhy`.
- [ ] **Step 2:** Run the new tests; expected: compile failure. Implement; build for Windows (0 warnings).
- [ ] **Manual check (user):** Save as file on one media file opens a save dialog and the saved file plays elsewhere with its tags; cancelling the dialog shows nothing wrong.
- [ ] **Leave uncommitted.**

---

### Task 17: Clean-up and the whole-plan acceptance

**Files:**
- Modify: the design docs (status line)
- Delete: any temporary diagnostic page left from Tasks 8 and 9

- [ ] **Step 1:** Build the solution for both targets (0 warnings); run both test projects and report the counts before and after the whole plan.
- [ ] **Step 2:** Run the **Manual check** lists of Tasks 6, 7, 9, 11, 12, 13, 14 and 15 once more end to end with the user in one sitting, in this order: sign out and in as two different accounts (nothing of the first shows for the second), upload media through the API, browse and sort 1000 or more, select with a search active, download, play, queue, playlists, Continue across two devices, offline start-up, change a colour and a gap in `variables.css` and see the whole app follow.
- [ ] **Step 3:** Record the result, and anything that could not be tried (Android), in the report; update the status line of the maui design doc: the screens are built on Windows.
- [ ] **Leave uncommitted.**
