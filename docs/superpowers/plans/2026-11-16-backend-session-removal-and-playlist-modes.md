# Backend: Remove the Saved Session, Add `KeepQueuedItems` and Per-Playlist Modes Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:executing-plans (the user wants this done natively, in the main session, not through subagents). Steps use checkbox (`- [ ]`) syntax for tracking. **This plan runs first** of the five plans after the redesign (order at the end of the Global Constraints).

**Goal:** Take the saved listening session off the server (the live queue replaces it, in memory, in the remote playback plan), add the synced setting `KeepQueuedItems`, and give a playlist its own optional shuffle and repeat mode that falls back to the user's global setting.

**Architecture:** The same layers as the rest of the backend. Removing the session is a deletion across Domain, Application, Persistence, API and the tests, followed by a squashed migration; the setting is one declaration in the catalog; the per-playlist modes are two nullable columns on the playlist that `PATCH /Playlist/{id}` sets.

**Tech Stack:** .NET 10, ASP.NET Core, EF Core with SQLite, Mediator, xunit v3.

**Spec:** `docs/superpowers/specs/2026-11-16-remote-playback-design.md` ("Local copy on each device", "The `KeepQueuedItems` setting"), `2026-11-15-user-settings-design.md` (the global `Shuffle` and `Repeat`, and the "per playlist" line that Task 4 builds) and `2026-11-15-frontend-screens-design.md`.

## Global Constraints

- Work in the backend projects (`Domain`, `Application`, `Persistence`, `API`) and the two test projects. No frontend change here. Nothing is deployed, so the saved session is **deleted, not migrated**, and the migrations are squashed again: at the end of a model-changing task delete every file in `Persistence/Migrations` and run `dotnet ef migrations add Initial --project Noxy.NET.MediaPlayer/Noxy.NET.MediaPlayer.Persistence --startup-project Noxy.NET.MediaPlayer/Noxy.NET.MediaPlayer.API` (a project command, allowed). The user recreates the dev database afterwards (delete the `.sqlite` file in `Data`, then `dotnet ef database update` with the same two arguments): say so in the report, never do it yourself.
- Conventions of the earlier plans: one class per file named after it, no summary comment above a class, every enum named `...Enum` in an `Enums` folder, interfaces in `Interfaces`, extensions in `Extensions`, `List` suffix, no `yield`, no magic strings, `is null`, no `!`, method-syntax LINQ, "media" never "song", no helper classes, zero warnings, no commits, `git add -N` new files by name (`git rm --cached` for deleted tracked files). Tests: unit in `Noxy.NET.Test.UnitTest/MediaPlayer/`, integration in `Noxy.NET.Test.IntegrationTest/MediaPlayer/`, `TestContext.Current.CancellationToken` on every call that takes a token, build then run the exe. Any new `Optional<T>` request property needs `[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]`.
- **What stays of `Playback`:** `ConfigPlayback.PlayedWindowSeconds` (the playlist "played" window) and its section; `MaxQueueLength` goes.
- **Order of the five plans:** this one, then `2026-11-16-backend-remote-playback.md`, then `2026-11-16-frontend-sync-settings-metadata-session.md`, then `2026-11-16-frontend-remote-playback.md`, then `2026-11-16-frontend-screens.md`.

## Review Focus

- **Nothing left pointing at the deleted session:** no controller route, hub event, notification, repository member, table, config key or test mentions it, and deleting a media file no longer touches any session (Task 2).
- **A setting that does not exist yet for an old client, and a bad value:** `PATCH /Settings/KeepQueuedItems` with a non-boolean is a `400`, the default is `false`, and `GET /Settings` lists it (Task 3).
- **A playlist mode that does not change anything, or is cleared:** setting the mode it already has changes nothing and pushes nothing, setting it to null clears it, a name that is not a mode is a `400`, and neither the entries nor the duplicate rule are touched (Task 4).
- **A left-out field versus an explicit null** in the playlist update: a left-out `shuffle` leaves the mode alone, an explicit `null` clears it (Task 4).

## File Structure

- Deleted (`git rm --cached`, then removed): `API/Controllers/PlaybackSessionController.cs`; `API/Handlers/Playback/HandlerNotificationPlaybackSessionCleared.cs` and `...Updated.cs`; `Application/Commands/Playback/CommandPlaybackSessionClear.cs` and `...Save.cs`; `Application/Handlers/Playback/HandlerPlaybackSession.cs`, `...Clear.cs`, `...Save.cs`; `Application/Interfaces/Repositories/IPlaybackRepository.cs`; `Application/Models/Playback/PlaybackSessionView.cs`; `Application/Notifications/Playback/*`; `Application/Queries/Playback/QueryPlaybackSession.cs`; `Application/Requests/Playback/RequestSavePlaybackSession.cs`; `Domain/Entities/EntityPlaybackSession.cs`; `Domain/Exceptions/InvalidPlaybackSessionException.cs`; `Domain/Extensions/EntityPlaybackSessionExtension.cs`; `Domain/Responses/Playback/ResponsePlaybackSession.cs`; `Persistence/Repositories/PlaybackRepository.cs`; `Persistence/Tables/Playback/TablePlaybackSession.cs`; `Persistence/TypeConfigurations/PlaybackSessionTypeConfiguration.cs`; tests `Playback/EntityPlaybackSessionExtensionTest.cs`, `HandlerPlaybackSessionClearTest.cs`, `HandlerPlaybackSessionSaveTest.cs`, `HandlerPlaybackSessionTest.cs`, `RequestSavePlaybackSessionTest.cs`, `Api/PlaybackSessionFlowTest.cs`, `Playback/PlaybackRepositoryTest.cs`, `Fakes/FakePlaybackRepository.cs`
- Modified: `Domain/Enums/MediaHubEventEnum.cs` (remove `PlaybackSessionUpdated`, `PlaybackSessionCleared`), `API/ApiExceptionHandler.cs`, `API/appsettings.json`, `Application/Configurations/ConfigPlayback.cs`, `Application/Handlers/Media/HandlerMediaDelete.cs`, `Persistence/DataContext.cs`, `Persistence/UnitOfWork.cs`, both mapper interfaces and both mappers, `Persistence/Migrations/` (regenerated), `Application/Interfaces/IUnitOfWork.cs`, tests `Fakes/FakeStore.cs`, `Fixtures/LibraryHubListener.cs`, `Fixtures/MediaApiClient.cs`, `Handlers/HandlerNotificationTest.cs`, `Media/HandlerMediaDeleteTest.cs`, `Playlists/ConfigPlaybackTest.cs`
- Moved: `Noxy.NET.MediaPlayer.Presentation/Enums/LibrarySortEnum.cs` to `Domain/Enums/LibrarySortEnum.cs` (Task 1)
- Task 3: `Domain/Declarations/SettingKey.cs`, `SettingCatalog.cs`, `Test/.../Metadata/SettingCatalogTest.cs`
- Task 4: `Domain/Entities/EntityPlaylist.cs`, `Domain/Extensions/EntityPlaylistExtension.cs`, `Domain/Responses/Playlist/ResponsePlaylist.cs` and `ResponsePlaylistDetail.cs`, `Application/Requests/Playlist/RequestUpdatePlaylist.cs`, `Application/Commands/Playlist/CommandPlaylistUpdate.cs`, `Application/Handlers/Playlist/HandlerPlaylistUpdate.cs`, `Persistence/Tables/Playlist/TablePlaylist.cs`, `Persistence/TypeConfigurations/PlaylistTypeConfiguration.cs`, the mappers if they copy field by field, tests in the existing playlist handler, request and response tests, `Api/PlaylistModeFlowTest.cs` (new)

---

### Task 1: Move `LibrarySortEnum` to the Domain

**Files:**
- Move: `Noxy.NET.MediaPlayer.Presentation/Enums/LibrarySortEnum.cs` to `Noxy.NET.MediaPlayer.Domain/Enums/LibrarySortEnum.cs` (members `Title`, `Artist`, `Album`, `Year`, `TimeCreated`; namespace `Noxy.NET.MediaPlayer.Domain.Enums`)

**Interfaces:** Produces the enum in `Domain` (the remote playback messages name it). `Presentation` already references `Domain`, so the frontend's `ResponseMediaFileListExtension`, its tests and `Features` keep working with a `using` change only.

- [ ] **Step 1:** Make the move; fix the `using` of every file that names `LibrarySortEnum` (the Presentation extension and its test, `Home.razor` if it names it). `git rm --cached` the old path and `git add -N` the new one.
- [ ] **Step 2:** Build the solution for both targets (0 warnings) and run both test projects; expected: all pass, counts unchanged.
- [ ] **Leave uncommitted.**

---

### Task 2: Remove the saved listening session

**Files:** the deleted and modified lists of the File Structure.

**Interfaces:** Produces a backend with no `PlaybackSession` anywhere: the `Playback` repository and `IUnitOfWork.Playback` are gone; `HandlerMediaDelete` no longer reads or edits a session and publishes no session notification; `MediaHubEventEnum` has no session members; `ConfigPlayback` has only `PlayedWindowSeconds`; the regenerated `Initial` migration has no `PlaybackSession` table.

- [ ] **Step 1: Write the failing tests first** where a test still has value: `HandlerMediaDeleteTest.Delete_DoesNotTouchAnyRepositoryOtherThanTheMediaAndPlaylistOnes` (the fake unit of work no longer has a `Playback` member, so the old session assertions are deleted and this one pins that deleting a media file reads and writes nothing else); `ConfigPlaybackTest.PlayedWindowSeconds_KeepsItsRangeAndDefault` and `ConfigPlaybackTest.MaxQueueLength_IsGone` (reflection: the type has no such property); `MediaHubEventEnum` has exactly the members that remain (a test lists them, so a stray session event fails it).
- [ ] **Step 2:** Delete the files and edit the modified ones (`ApiExceptionHandler` loses the `InvalidPlaybackSessionException` mapping; `appsettings.json` loses `MaxQueueLength`); regenerate the migration as the Global Constraints say; build both targets (0 warnings); run both test projects; expected: all pass, and the counts fall by exactly the deleted tests.
- [ ] **Step 3:** `grep -ri "PlaybackSession" --include=*.cs` over the backend and the test projects (excluding `bin`, `obj` and the frontend) returns nothing; record that. Update the screens design's "Backend additions" and the sync-player design's backend contracts: the session endpoints are gone, replaced by the live session of the remote playback design.
- [ ] **Leave uncommitted.** Say in the report that the dev database must be recreated.

---

### Task 3: The `KeepQueuedItems` setting

**Files:** `Domain/Declarations/SettingKey.cs`, `Domain/Declarations/SettingCatalog.cs`; tests `SettingCatalogTest.cs` and the settings flow tests that list the catalog.

**Interfaces:** Produces `SettingKey.KeepQueuedItems` (the string `"KeepQueuedItems"`, exact case like the others) and `SettingCatalog.KeepQueuedItems = new SettingDeclarationBoolean(SettingKey.KeepQueuedItems, false)`, added to `DeclarationList`.

- [ ] **Step 1: Write the failing tests:** `Catalog_ContainsKeepQueuedItems_DefaultFalse`; `Find_IsCaseExact_ForKeepQueuedItems`; the settings flow: `GET /Settings` lists `KeepQueuedItems` with `false` by default; `PATCH /Settings/KeepQueuedItems` with `{"value":true}` stores and pushes `SettingUpdated`; with `{"value":"yes"}` or `{"value":1}` is a `400`; a second `PATCH` with the same value pushes nothing.
- [ ] **Step 2:** Run the new tests; expected failures. Implement; both test projects pass.
- [ ] **Step 3:** In the user-settings design add the setting to the list of settings (boolean, default false, "keep the user queue when a different source starts").
- [ ] **Leave uncommitted.**

---

### Task 4: Per-playlist shuffle and repeat (separable: drop this task if the user decides global-only)

**Files:** the Task 4 list of the File Structure.

**Interfaces:**
- Produces: `EntityPlaylist.Shuffle` (`ShuffleModeEnum?`) and `EntityPlaylist.Repeat` (`RepeatModeEnum?`), null meaning "use the user's global setting"; `ResponsePlaylist.Shuffle` and `Repeat` (nullable, serialised by name); `RequestUpdatePlaylist.Shuffle` and `Repeat` as `Optional<ShuffleModeEnum?>` and `Optional<RepeatModeEnum?>` with `[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]`, so a left-out field is not touched, a value sets it and an explicit `null` clears it; `CommandPlaylistUpdate` carries both; `HandlerPlaylistUpdate` applies them with the same change detection as the name: a mode equal to the stored one is not a change, and a real change goes through the existing `Save` so `TimeModified` moves (the playlist-list ETag changes and `PlaylistUpdated` is pushed; the detail ETag also moves, which makes other devices download the entries again: accepted, mode changes are rare, and noted in the spec).

- [ ] **Step 1: Write the failing tests** (Review Focus): `Update_SetsTheShuffleMode_AndBumpsTheModifiedTime_AndPublishes`; `Update_SetsTheRepeatMode`; `Update_WithTheModeItAlreadyHas_ChangesNothing_AndPublishesNothing`; `Update_WithExplicitNull_ClearsTheMode`; `Update_WithTheFieldLeftOut_DoesNotTouchTheMode` (the request serialises a left-out `Optional` as nothing: `{"name":"x"}` carries no `shuffle`); `Update_OfTheModeDoesNotChangeTheEntriesOrTheDuplicateRule`; `ANameThatIsNotAMode_Is400`; `ToResponse_CarriesTheModes_AndNullByDefault` for the summary and the detail; flow: a `PATCH` with `{"shuffle":"Random"}` answers the playlist with the mode, arrives as a `PlaylistUpdated` push and shows in `GET /Playlist` and `GET /Playlist/{id}`, a `PATCH` with `{"shuffle":null}` clears it, a second `PATCH` with the same value pushes nothing.
- [ ] **Step 2:** Run the new tests; expected: compile failure. Implement; regenerate the migration; run both test projects; expected: all pass.
- [ ] **Step 3:** In the user-settings design change the non-goal "per playlist" line to say it is built, with null meaning the global setting; in the screens design note where the player bar's shuffle and repeat buttons write (the playing playlist's own mode, otherwise the global setting). Report that the dev database must be recreated.
- [ ] **Leave uncommitted.**
