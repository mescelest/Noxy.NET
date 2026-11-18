# Handoff, 2026-11-17: plan 5 (frontend screens), all 17 tasks code-complete, awaiting the user manual checks, code review and commit

Start a new context and say: "Read `docs/superpowers/HANDOFF-2026-11-17-plan-5.md` and continue." Plan 4 (the live queue and remote playback) is built, reviewed and committed; its plan file `plans/2026-11-16-frontend-remote-playback.md` may be deleted once the user says so (ask).

## How to continue

1. `git status`: Tasks 1 to 5 of plan 5 may be uncommitted (new files are staged with `git add -N`). Build the solution and run both test exes: expect **1961 unit, 615 integration, 0 warnings**. Ask the user whether they committed Tasks 1 to 3; if not, carry on but keep that work separate.
2. Plan: `plans/2026-11-16-frontend-screens.md` (Tasks 1 to 17 and 14a). Spec: `specs/2026-11-15-frontend-screens-design.md`. Native `superpowers:executing-plans`, no subagents. Next is **Task 6** (the shell: layout, header, sidebar, routing), then Task 7 (sign-in), and so on.
3. A component task ends with a manual check list the user runs; the executor starts the app (`bin/Debug/net10.0-windows10.0.19041.0/win-x64/Noxy.NET.MediaPlayer.App.exe`) to confirm it launches and stays responsive (`Start-Process`, wait 12 s, `Responding`, close), and never claims a component works from the build alone.
4. After the plan: `/code-review high` (the user runs it), fix the real findings test-first, ask about committing. At the very end ask whether the finished plan files can be deleted.

## What Tasks 1 to 3 did

- **Task 1:** `App/wwwroot/css/variables.css` (entity management names and values as placeholders, light and dark through `:root[theme=...]`, the app's own sizes, the glass tokens, what the UI libraries read), `breakpoints.css` (the only media query), `app.css`; the template `app.css` and bootstrap were removed; `index.html` links the three sheets. `StyleRuleTest` + `StyleFileFixture` scan every App `*.css` for colour, font, duration and size literals and check that `variables.css` defines every variable the used UI libraries (Shared, Shared.Fluxor, Elements, Table, Dialog, WebForm, Graphics) and the App read. The App references those seven UI projects and `Fluxor.Blazor.Web`; `Presentation` references `Fluxor` 6.11.0; `MauiProgram` calls `AddFluxor`.
- **Task 2:** 17 icons in `Noxy.NET.UI.Graphics/Components/Icons` (Play, Pause, Volume, VolumeMuted, Search, Menu, More, Queue, Home, Library, Account, Settings, Download, OnDevice, NotDownloaded, Trash, Music). Their look is a manual check for the user (a temporary page that renders them at the three sizes); not done yet.
- **Task 3:** `Presentation/Features/<Library|PlaylistList|Session|Shell|Download|Player>` (state records with `[FeatureState]` and init defaults, one action per file, reducers as static classes with `[ReducerMethod]`), `Models/PlayerSnapshot.cs`, `IStateBridgeService`/`StateBridgeService`, `Fakes/FakeDispatcher.cs`, reducer and bridge tests.

## Rulings and deviations from the plan (all because plan 5 was written before plan 4 changed things)

- `PlayerQueueEntry` no longer exists. `PlayerSnapshot` is `(QueueItem? Current, bool IsDisabled, bool IsPlaying, bool IsWaiting, TimeSpan Position, TimeSpan Duration, double Volume, ShuffleModeEnum Shuffle, RepeatModeEnum Repeat, Guid? PlaylistID, int QueueItemCount)`; there is no queue list in it. The queue panel reads `IQueueService.GetSnapshot()` when it opens (never `Queue.ToResponse()`, which is not safe across threads).
- **`StateBridgeService` is scoped, not a singleton**: Fluxor's `IDispatcher` is scoped and the Blazor WebView has its own scope, so a singleton would dispatch into a different store. It is started from `Routes.razor` (`OnInitialized`), implements `IDisposable` (unsubscribes), and takes an `ILogger`. `PresentationRegistrationTest` builds the container with `AddFluxor` and resolves inside a scope.
- The session state's `HasSession` is `IsSignedIn` and `IsRetrying` is false until Task 5 adds them.
- Library state stores the derived `VisibleList` (rebuilt in the reducers); the page's `Sort` is the existing `Filter` and `Sort` extensions.
- The queue holds at most 5000 items (`Constants.QueueMaxItemCount`, the library limit). The queue is a cache file (`queue.json`), the device ID, name and volume are `device.json`; SQLite only holds the library, playlists, metadata and download states. The queue is written only when playback stops and when the app leaves the foreground (`IPlaybackReporterService.SaveNow`, called from `App.xaml.cs` on `Stopped` and `Destroying`).
- Task 4 mentions `ILocalQueueStore.Clear()` for the owner wipe: it now clears `queue.json`; the volume and device files are the device's, not the account's (decide whether a wipe should reset them; I would keep the device ID and name and reset nothing else).

## Lessons for this environment

- **Multi-line shell commands with mixed quotes or apostrophes in comments can fail to parse and then nothing runs.** Use the Write tool for files and one-line `sed` or the Edit tool for small changes.
- **Never edit by line number with `sed` after another edit in the same session**: the numbers shift and the wrong line is replaced (it happened three times). Use the Edit tool with exact text.
- The user's IDE can change files while I work (`PlaybackReporterService` became a `switch` that way): read before re-editing and do not revert it.
- Tests that wait: wait for the condition, never for a flag that flips early (`Current` changes before the file loads); fakes with lists that tasks write to from several threads must lock.
- Full integration runs take about 40 s and the unit run about 5 s; run them from the test project output folders, build first.

## Tasks 4 and 5 (done after the first version of this note)

- **Task 4:** `ILocalDataOwnerService.EnsureOwner()` (reads the `sub` of the access token through the new `GetAccessTokenSubject()`, wipes library, playlists, ETags, file states, settings, artists/albums/genres, held and partial files, the player and its queue when the owner changes, and records the new owner; no stored owner means it just records; an unreadable token or signed out never wipes). Owner stored in `IMetadataStore` (`GetOwnerUserID`/`SetOwnerUserID`, SQLite settings table). `AppStartupService` calls it right after a successful restore, before any sync. The interactive sign-in path needs no extra call: the Login page (Task 7) calls `IAppStartupService.Start()`, which runs `EnsureOwner()` before any sync. A queue with no current item is never written (`SaveLocalCopy` clears the file instead).
- **Task 5:** `TryRestore` returns `RestoreResultEnum` (`SignedIn`, `NoSession`, `Rejected`, `Offline`); `ISessionService.HasSession` is true when signed in or when a stored session could not reach the server. `IAppStartupService` has `RetryNow()`, `IsRetrying` and `Changed`; on `Offline` start-up repairs files, loads the local queue and retries through `IDelay` after 5, 10, 20, 40, 60, 60 s, running the rest of start-up exactly once on success and stopping on `NoSession`/`Rejected`. `StateBridgeService` takes `IAppStartupService` and dispatches `HasSession` and `IsRetrying` (an offline session shows `IsOffline`).

## Tasks 6 to 17 (done 2026-11-17, uncommitted)

Unit 2044, integration 615, 0 warnings (both targets). The app launches and stays responsive; every on-screen behaviour is for the user to check with the Manual check lists of the plan. Rulings are in the ledger. Notable choices: the signed-out guard and the Login page live in `MainLayout`; `IsRestored`/`HasRestored` stop a sign-in flash at start-up; `IDownloadService.Remove` exists so the page refreshes after Remove from device; `LibraryActionService` also does Play next, Add to queue and Save as file (`IFileSaver`, `GetTaggedFile`); the theme uses the UI library `ThemeService` (localStorage, applied early from `index.html`); the playlist page has its own Remove selected; the temporary `/table-test` page is removed. Not tried: Android.
