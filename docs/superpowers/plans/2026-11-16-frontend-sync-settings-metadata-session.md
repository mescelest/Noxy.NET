# Frontend Sync: Settings, Metadata Lists and the Synced Volume Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:executing-plans (the user wants this done natively, in the main session, not through subagents). Steps use checkbox (`- [ ]`) syntax for tracking. **Do not start before `2026-11-16-backend-session-removal-and-playlist-modes.md` is done** (the settings list includes `KeepQueuedItems` and the playlist request carries `Shuffle` and `Repeat`). The queue, the shuffle, the player rebuild and remote playback are **not** in this plan: they are in `2026-11-16-frontend-remote-playback.md`, which runs after it.

**Goal:** Keep the user's typed settings and the artist, album and genre lists on the device, in step with the server (ETag syncs, pushes and offline-safe writes), and make the player's volume follow the synced `Volume` setting. No screen and no queue work is built here.

**Architecture:** Everything lives in `Noxy.NET.MediaPlayer.Presentation` (services, models, contracts) and `Storage` (the SQLite implementation of two new store interfaces), tested in the unit project with fakes and end to end against the real API. Each synced subject gets its own service next to `SyncService`; the hub service forwards the new pushes. The existing `PlayerService` is only touched for the volume.

**Tech Stack:** .NET 10, `Microsoft.Data.Sqlite`, `System.Text.Json`, SignalR client, xunit v3.

**Spec:** `docs/superpowers/specs/2026-11-15-frontend-screens-design.md` ("Backend additions", "Everything the user changes syncs"), `2026-11-15-frontend-sync-player-design.md` and `2026-11-15-user-settings-design.md`. Previous plans (done): the frontend foundation and `2026-11-15-frontend-downloads-player.md`; and `2026-11-16-backend-session-removal-and-playlist-modes.md`.

## Global Constraints

- Work in `Presentation`, `Storage` and the two test projects (and `App/MauiProgram.cs` and the temporary diagnostic page for registrations and the replaced player calls only). Target `net10.0`. The backend is not changed here. **No MAUI, Blazor or platform type in `Presentation` or `Storage`.** Windows first.
- Conventions of the earlier plans: one class per file named after it, no summary comment above a class, every enum named `...Enum` in an `Enums` folder of its project (namespace mirroring the folder), interfaces in `Interfaces/<same folder as the implementation>`, extensions in `Extensions` (the class named after the type it extends), `List` suffix, no `yield`, no magic strings (constants, `nameof`, `SettingKey`, `nameof(MediaHubEventEnum.X)`), `is null`, no `!`, method-syntax LINQ, "media" never "song", kind-first dependency names (`serviceSetting`, `storeSetting`, `clientApi`), no helper classes, zero warnings, no commits, `git add -N` new files by name, no machine-level commands. Tests: unit in `Noxy.NET.Test.UnitTest/MediaPlayer/Presentation/`, store and end-to-end in the integration project, fakes in `Fakes`, helpers in `Fixtures`, a test folder holds only tests, `TestContext.Current.CancellationToken` on every call that takes a token, build then run the exe (`dotnet test` finds nothing).
- ETag rules (as `SyncService`): a stored ETag is only one that arrived with a full `200` the device downloaded; a push never writes an ETag; no `If-None-Match` is sent when the local list is empty (cleared app data) or when `force` is set; a failed network call (`HttpRequestException`, `ServerAddressMissingException`, `TaskCanceledException`) leaves the local data and the ETag alone.
- Settings (the backend's `SettingCatalog`): `Shuffle` (`ShuffleModeEnum`: `Off`, `Random`, `SpreadArtists`; default `Off`), `Repeat` (`RepeatModeEnum`: `No`, `Once`, `Forever`; default `No`), `Volume` (integer 0 to 100, default 100), `MergeOnRename` (boolean, default false), `KeepQueuedItems` (boolean, default false). `ResponseSetting.Value` arrives as a `JsonElement`. A write is `PATCH /Settings/{key}` with `{ "value": ... }`.
- The access token and refresh token never appear in a log or an exception message; a log line names the subject, never a value.

## Review Focus

- **A settings write while offline or refused:** the server is the authority, the write throws, the local value is unchanged (no phantom setting is shown), and a later sync converges (Task 3).

## File Structure

New files (relative to the project named; tests relative to the test projects' `MediaPlayer/Presentation/`):
- `Presentation/Interfaces/Stores/`: `ISettingStore.cs`, `IMetadataListStore.cs`
- `Presentation/Interfaces/Services/`: `ISettingService.cs`, `IMetadataSyncService.cs`
- `Presentation/Services/`: `SettingService.cs`, `MetadataSyncService.cs`
- `Presentation/Extensions/`: `ResponseSettingExtension.cs`, `ExceptionExtension.cs` (is this an offline failure)
- Tests: unit `Fakes/FakeSettingService.cs`, `Fakes/FakeMetadataSyncService.cs`, `Fixtures/SettingStoreContract.cs`, `Fixtures/MetadataListStoreContract.cs`, test classes per class; integration `FrontendMetadataSyncFlowTest.cs`

Modified: `FakeMetadataStore` and `SqliteMetadataStore` (they implement the two new store interfaces too), `IApiClient`, `ApiClient`, `ApiResponse`, `FakeApiClient`, `SyncService` (uses `ExceptionExtension`), `PlayerService` (volume only, Task 10) and its tests, `MediaHubService`, `AppStartupService`, both `ServiceCollectionExtensions.cs`, `FrontendClient`, the design docs.

Contracts every task shares:
- `ISettingStore`: `Task<List<ResponseSetting>> GetSettingList()`, `Task ReplaceSettingList(List<ResponseSetting>)` (one atomic write), `Task SaveSetting(ResponseSetting)` (insert or replace by `Key`, an existing key keeps its place), `Task<string?> GetSettingListETag()`, `Task SetSettingListETag(string?)`.
- `IMetadataListStore`, for each kind `Artist`, `Album`, `Genre` (each response has a `Guid ID` and a `TimeModified`): `Task<List<ResponseX>> GetXList()`, `Task ReplaceXList(List<ResponseX>)`, `Task SaveX(ResponseX)`, `Task DeleteX(Guid id)` (an unknown ID is harmless), `Task<string?> GetXListETag()`, `Task SetXListETag(string?)`.
- `IApiClient` gains `Task<ApiResponse<List<ResponseSetting>>> GetSettingList(string? eTag)`, `Task<ResponseSetting> UpdateSetting(string key, JsonElement value)`, `GetArtistList/GetAlbumList/GetGenreList(string? eTag)`, `Task MarkPlaylistPlayed(Guid id)`. `ApiResponse<T>` gains `bool IsNoContent => Status == HttpStatusCode.NoContent`. `UpdatePlaylist` already takes the request class, which carries `Shuffle` and `Repeat` after the backend plan.

---

### Task 1: The stores

**Files:**
- Create: `Interfaces/Stores/ISettingStore.cs`, `IMetadataListStore.cs`; tests `Fixtures/SettingStoreContract.cs`, `Fixtures/MetadataListStoreContract.cs`
- Modify: `Fakes/FakeMetadataStore.cs` and `Storage/Stores/SqliteMetadataStore.cs` (each also implements the two new interfaces); `FakeMetadataStoreTest.cs` and `SqliteMetadataStoreTest.cs` (also run the two new contracts); `Storage/ServiceCollectionExtensions.cs` (`AddStorage` registers the one `SqliteMetadataStore` instance for `IMetadataStore`, `ISettingStore` and `IMetadataListStore`); the integration project (link the two contracts)

**Interfaces:** Produces the two store interfaces above. The SQLite store keeps its one file: a settings table keyed by the setting's `Key` (text), one table per metadata kind keyed by `ID` in the existing JSON shape, and the new ETags in the existing `Setting` key-value table.

- [ ] **Step 1: Write the contract tests** (abstract classes with `CreateStore()` and `[Fact]` methods, like `MetadataStoreContract`). Settings: empty store has none and no ETag; `ReplaceSettingList` replaces rather than merges and keeps the order; `SaveSetting` inserts a new key at the end and replaces an existing key in place; a boolean, an integer and an enum value each come back equal (a `JsonElement` of the same kind); the ETag round-trips and `null` clears it. Metadata lists, for artists, albums and genres: replace, save (insert at the end, replace in place), delete (unknown ID harmless), every field comes back equal (an album with two artist IDs, a count of 0), the kinds are independent, each kind's ETag round-trips independently.
- [ ] **Step 2:** Run `*FakeMetadataStoreTest` and `*SqliteMetadataStoreTest`; expected: compile failure. Implement the fake, then the SQLite store (every `Replace…List` one transaction); run both; expected: all pass.
- [ ] **Leave uncommitted.**

---

### Task 2: API client calls

**Files:**
- Modify: `Interfaces/Services/IApiClient.cs`, `Services/ApiClient.cs`, `Models/ApiResponse.cs`, `Fakes/FakeApiClient.cs`; tests `ApiClientTest.cs`

**Interfaces:**
- Consumes: the private `Get<T>`, `Write<T>` and `Send` of `ApiClient`.
- Produces: the `IApiClient` members listed above. `Get<T>` returns `new(HttpStatusCode.NoContent, default, null)` for a `204` instead of failing to read a body. Paths: `GET /Settings`, `PATCH /Settings/{key}` with body `{"value": <the JsonElement>}`, `GET /Artist`, `/Album`, `/Genre`, `POST /Playlist/{id}/Played` (`204`). `FakeApiClient` gets a scripted handler per new call that records what it was sent, like the existing ones.

- [ ] **Step 1: Write the failing tests** (fake handler, as the existing ones): each read sends the bearer token and `If-None-Match`, returns the list and ETag on `200` and `NotModified` with no value on `304`; `UpdateSetting_PatchesTheKeyWithTheValueAsJson` for a boolean, an integer and a string (the body is exactly `{"value":true}`, `{"value":40}`, `{"value":"Forever"}`) and returns the setting; `MarkPlaylistPlayed_PostsToThePlayedPathAndAcceptsA204`; `UpdatePlaylist_WithAShuffleAndRepeatMode_SendsThemByName_AndWithNullSendsNull_AndWithNeitherSendsNeither`; a `404` or `409` problem on a write throws `ApiException` with its status and title; the bearer, address and single refresh-and-retry rules apply to every new call.
- [ ] **Step 2:** Run `*ApiClientTest`; expected: compile failure. Implement; run the unit project; expected: all pass.
- [ ] **Leave uncommitted.**

---

### Task 3: Setting service

**Files:**
- Create: `Interfaces/Services/ISettingService.cs`, `Services/SettingService.cs`, `Extensions/ResponseSettingExtension.cs`, `Extensions/ExceptionExtension.cs`; tests `SettingServiceTest.cs`, `ResponseSettingExtensionTest.cs`, `ExceptionExtensionTest.cs`, `Fakes/FakeSettingService.cs`
- Modify: `Services/SyncService.cs` (use `ExceptionExtension` instead of its own offline filter)

**Interfaces:**
- Consumes: `IApiClient` (Task 2), `ISettingStore` (Task 1).
- Produces: `extension(Exception exception) { public bool IsOfflineFailure }` (true for `HttpRequestException`, `ServerAddressMissingException`, `TaskCanceledException`); `extension(ResponseSetting setting) { public bool GetBoolean(); public int GetInteger(); public string GetEnumName(); }` (read the `JsonElement`, throw `InvalidOperationException` with the key when the value is not that kind); `SettingService(IApiClient clientApi, ISettingStore storeSetting)` implementing `ISettingService { event Action? Changed; Task<List<ResponseSetting>> GetSettingList(); Task<int> GetVolume(); Task<ShuffleModeEnum> GetShuffle(); Task<RepeatModeEnum> GetRepeat(); Task SyncSettingList(bool force = false); Task Update(string key, JsonElement value); Task ApplySettingUpdated(ResponseSetting setting); }`. Each getter returns the stored value or the declaration's default in `SettingCatalog` when none is stored yet.

- [ ] **Step 1: Write the failing tests** (`FakeApiClient`, the `FakeMetadataStore`): `SyncSettingList_FirstRun_SendsNoETag_StoresTheListAndTheETag`; `…_On304_ChangesNothing`; `…_On200_ReplacesTheLocalList`; `…_WhenTheLocalListIsEmptyButAnETagIsStored_SendsNoETag`; `…_Force_SendsNoETag`; `…_WhenOffline_LeavesListAndETagAlone`; `Changed_IsRaisedAfterEachStoreWrite`; `Update_SendsTheValue_SavesTheServersAnswer_AndRaisesChanged`; `Update_WhenOffline_Throws_AndTheLocalValueIsUnchanged` (Review Focus); `Update_WhenTheServerRefuses_ThrowsApiException_AndTheLocalValueIsUnchanged`; `ApplySettingUpdated_SavesIt_AndKeepsTheStoredETag`; `…_WithAnOlderOrEqualTimeModified_IsIgnored`; `GetVolume_GetShuffle_GetRepeat_ReturnTheStoredValueOrTheDefault`. The extension tests pin the three value kinds, the throw for a wrong kind, and `IsOfflineFailure` for each type and for an `ApiException` (false).
- [ ] **Step 2:** Run the new classes; expected: compile failure. Implement; run the unit project (the existing `SyncServiceTest` must still pass); expected: all pass.
- [ ] **Leave uncommitted.**

---

### Task 4: Metadata list sync

**Files:**
- Create: `Interfaces/Services/IMetadataSyncService.cs`, `Services/MetadataSyncService.cs`, `Fakes/FakeMetadataSyncService.cs`; tests `MetadataSyncServiceTest.cs`

**Interfaces:**
- Consumes: `IApiClient`, `IMetadataListStore`, `ExceptionExtension`.
- Produces: `MetadataSyncService(IApiClient clientApi, IMetadataListStore storeMetadataList)` implementing `IMetadataSyncService { event Action? Changed; Task SyncArtistList(bool force = false); Task SyncAlbumList(bool force = false); Task SyncGenreList(bool force = false); Task ApplyArtistUpdated(ResponseArtist artist); Task ApplyArtistDeleted(Guid id); Task ApplyAlbumUpdated(ResponseAlbum album); Task ApplyAlbumDeleted(Guid id); Task ApplyGenreUpdated(ResponseGenre genre); Task ApplyGenreDeleted(Guid id); }`.

- [ ] **Step 1: Write the failing tests**, once per kind (a theory or three small methods): `Sync…List` has the same behaviours as the setting service's sync tests (first run, 304, 200 replaces, empty list with stored ETag, force, offline, changed raised); `Apply…Updated_SavesIt_AndKeepsTheStoredETag`; `Apply…Updated_WithAnOlderOrEqualTimeModified_IsIgnored`; `Apply…Deleted_RemovesIt_AndAnUnknownIDIsHarmless`; the three kinds never touch each other's lists or ETags.
- [ ] **Step 2:** Run `*MetadataSyncServiceTest`; expected: compile failure. Implement; pass.
- [ ] **Leave uncommitted.**

---

### Task 10: The synced volume (reversed on 2026-11-16: the volume is per device and shared by the live session, see `HANDOFF-2026-11-17.md`)

**Files:**
- Modify: `Services/PlayerService.cs`, `Fakes/FakeSettingService.cs`; tests `PlayerServiceTest.cs`

**Interfaces:**
- Consumes: `ISettingService.GetVolume()`, `Update(SettingKey.Volume, value)`, `Changed`.
- Produces: the player's volume follows the `Volume` setting (0 to 100 on the server, 0 to 1 in the player): the stored value is applied the first time a media file starts and whenever `ISettingService.Changed` reports a different value; `SetVolume` applies at once and then writes `Volume` (rounded to a whole number) through `Update`; a write that fails (offline) is logged and does not undo the local volume. A volume applied because of a setting change is never written back.

- [ ] **Step 1: Write the failing tests:** `TheStoredVolume_IsAppliedWhenTheFirstMediaFileStarts`; `SetVolume_AppliesAtOnce_AndWritesTheSettingRounded` (0.456 becomes 46); `SetVolume_WhenTheWriteFails_KeepsTheLocalVolume_AndLogsIt`; `ASettingChangeFromAnotherDevice_ChangesThePlayerVolume_AndIsNotWrittenBack`; `AnUnchangedSetting_DoesNotTouchThePlayer`.
- [ ] **Step 2:** Run `*PlayerServiceTest`; expected: compile failure. Implement; run the unit project; expected: all pass.
- [ ] **Leave uncommitted.**

---

### Task 11: Hub, start-up, registrations and the end-to-end tests

**Files:**
- Create: integration test `FrontendMetadataSyncFlowTest.cs`
- Modify: `Services/MediaHubService.cs` (two more constructor arguments: `ISettingService`, `IMetadataSyncService`), `Services/AppStartupService.cs`, both `ServiceCollectionExtensions.cs`, `Fixtures/FrontendClient.cs` (build and expose the new services), `MediaHubServiceTest.cs`, `AppStartupServiceTest.cs`, the maui design doc (status line)

**Interfaces:**
- Consumes: every service of Tasks 3, 4 and 10.
- Produces: `MediaHubService.HandledEventNameList` grows to these names, in this order: the five existing ones, then `SettingUpdated`, `ArtistUpdated`, `ArtistDeleted`, `AlbumUpdated`, `AlbumDeleted`, `GenreUpdated`, `GenreDeleted` (all `nameof(MediaHubEventEnum.X)`), each forwarded to the matching `Apply…`. On `Reconnected` the hub runs the library and playlist syncs as before and then the settings and the three lists. `AppStartupService.Start` syncs, after the library and the playlists and each in its own try so one failure does not skip the rest: settings, artists, albums, genres; then starts the hub. Start-up never plays.

- [ ] **Step 1: Write the failing tests.** Unit: `MediaHubServiceTest.TheHandledEvents_AreExactlyThoseThisPlanSyncs` (the list above); `AppStartupServiceTest`: `Start_SyncsTheNewSubjectsAfterTheLibraryAndPlaylists_ThenConnectsTheHub`, `Start_WhenOneNewSyncFails_StillRunsTheRest`, `Start_NeverStartsPlayback`. Integration `FrontendMetadataSyncFlowTest` (real API through `ApiFactory`, the `FrontendClient` fixture): a setting changed through the API arrives as a push with no ETag change and `GetVolume` follows it; `KeepQueuedItems` written through `SettingService.Update` is accepted and read back by a second client; an artist created, renamed and deleted through the API arrives and the store follows; the sync after dropping and restoring the connection brings all of them level; a cleared store with the ETags gone is refilled by the next sync.
- [ ] **Step 2:** Run the new tests; expected: compile failure. Implement the hub forwarding, the start-up steps and the registrations (`AddPresentation` registers the new services as singletons; `AddStorage` already registers the store interfaces); build the solution for both targets (0 warnings), run both test projects, report counts before and after the plan.
- [ ] **Step 3:** Add one status line to the maui design doc: settings and the metadata lists are synced on the device and the player volume follows the synced setting; what is left is the live queue and remote playback, then the screens.
- [ ] **Leave uncommitted.**
