# Frontend Sync of Settings, Metadata Lists and the Playback Session Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:executing-plans (the user wants this done natively, in the main session, not through subagents). Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Keep the user's typed settings, the artist, album and genre lists and the saved listening session on the device, in step with the server and the user's other devices, and make the player save its session, offer "Continue", report a playlist as played, and follow the synced volume.

**Architecture:** Everything lives in `Noxy.NET.MediaPlayer.Presentation` (services, models, contracts) and `Storage` (the SQLite implementation of three new store interfaces), tested in the unit project with fakes and end to end against the real API. Each subject gets its own service next to `SyncService` (which keeps the library and the playlists), with the same ETag, offline and push rules; the hub service forwards the new pushes to them. No screen is built here: the screens plan reads these services.

**Tech Stack:** .NET 10, `Microsoft.Data.Sqlite`, `System.Text.Json`, SignalR client, xunit v3.

**Spec:** `docs/superpowers/specs/2026-11-15-frontend-screens-design.md` (sections "Saved listening session", "Backend additions", "Everything the user changes syncs") and `docs/superpowers/specs/2026-11-15-frontend-sync-player-design.md` (metadata sync, the replaceable store, the backend contracts). Previous plans (done and committed): the frontend foundation, and `2026-11-15-frontend-downloads-player.md` (its `IPlayerService`, `IDownloadService` and file store are used here).

## Global Constraints

- Work in `Presentation`, `Storage` and the two test projects (and `App/MauiProgram.cs` for registrations only). Target `net10.0`. The backend is not changed. **No MAUI, Blazor or platform type in `Presentation` or `Storage`.** Windows first; nothing here is platform specific.
- All conventions of the earlier plans apply: one class per file, file named after it, no summary comment above a class, every enum named `...Enum` and kept in an `Enums` folder of its project (namespace mirroring the folder; the backend's `MediaHubEventEnum`, `RepeatModeEnum`, `ShuffleModeEnum`, `SettingValueTypeEnum` and the frontend's `FileSyncStateEnum`, `LibrarySortEnum`, `DownloadStopReasonEnum` already follow it), interfaces in `Interfaces/<same folder as the implementation>` with a mirroring namespace, extensions in `Extensions` (the class named after the type it extends), `List` suffix for lists, no `yield`, no magic strings (constants, `nameof`, `SettingKey`, `nameof(MediaHubEventEnum.X)`), `is null`, no `!`, method-syntax LINQ, "media" never "song", kind-first dependency names (`serviceSetting`, `storeSetting`, `clientApi`), no helper classes, zero warnings, no commits, `git add -N` new files by name, no machine-level commands. Tests: unit in `Noxy.NET.Test.UnitTest/MediaPlayer/Presentation/`, store and end-to-end in the integration project, fakes in `Fakes`, helpers in `Fixtures`, a test folder holds only tests, `TestContext.Current.CancellationToken` on every call that takes a token, build then run the exe (`dotnet test` finds nothing).
- ETag rules (as `SyncService`): a stored ETag is only one that arrived with a full `200` the device downloaded; a push never writes an ETag; no `If-None-Match` is sent when the local list is empty (cleared app data) or when `force` is set; a failed network call (`HttpRequestException`, `ServerAddressMissingException`, `TaskCanceledException`) leaves the local data and the ETag alone.
- Settings (the backend's `SettingCatalog`): `Shuffle` (enum `ShuffleModeEnum`, default `Off`), `Repeat` (enum `RepeatModeEnum`: `No`, `Once`, `Forever`; default `No`), `Volume` (integer 0 to 100, default 100), `MergeOnRename` (boolean, default false). `ResponseSetting.Value` is a bool, an int or an enum member name; on the client it arrives as a `JsonElement`. `GET /Settings` returns every declared setting with its default where nothing is saved. A write is `PATCH /Settings/{key}` with `{ "value": ... }`.
- The saved session (the backend's rules): one per user, last write wins, `GET /PlaybackSession` answers `200` with an ETag, `304`, or `204` when there is none; `PUT` saves it (queue of media file IDs at most 1000 long, a current index inside the queue or null, a non-negative position in seconds, an optional playlist ID); `DELETE` clears it. The client saves when playback starts, pauses or changes media file, and every 30 seconds while playing (`Constants.SessionSaveSeconds` = 30), never on every progress tick. A device offers "Continue" only for a session newer than the one it last played, and never takes over a device that is playing. A session's playlist is a hint: the client checks the playlist still exists before it shows or resumes it.
- The access token and refresh token never appear in a log or an exception message; a log line names the subject, never a value.

## Review Focus

- **A settings write while offline or refused:** the server is the authority, so the write throws, the local copy is unchanged (no phantom value shown), and a later sync still converges (Task 3).
- **Out-of-order or repeated pushes:** an older `timeModified` never overwrites a newer local setting, metadata item or session; a delete for an unknown ID is harmless; a cleared-session push removes the local session and its ETag (Tasks 3, 4, 5).
- **A session that points at things that are gone:** media files deleted since the save are dropped from the offer (the index follows the media file that was current), a deleted playlist is dropped from the offer, and an offer whose queue ends up empty is not offered at all (Task 5).
- **Two devices:** a device's own save never offers "Continue" on that device, an older session is not offered, and a device that is playing or waiting for a download is never taken over (Tasks 5, 6).
- **Save traffic:** position ticks alone never save, a pause-resume-pause burst coalesces to one save in flight plus at most one pending, a failed save (offline) is dropped and retried by the next trigger, and an emptied queue clears the server session (Task 6).

## File Structure

New files (relative to the project named; tests relative to the test projects' `MediaPlayer/Presentation/`):
- `Presentation/Interfaces/Stores/`: `ISettingStore.cs`, `IMetadataListStore.cs`, `IPlaybackSessionStore.cs`
- `Presentation/Interfaces/Services/`: `ISettingService.cs`, `IMetadataSyncService.cs`, `IPlaybackSessionService.cs`, `IPlaybackSessionRecorderService.cs`
- `Presentation/Services/`: `SettingService.cs`, `MetadataSyncService.cs`, `PlaybackSessionService.cs`, `PlaybackSessionRecorderService.cs`
- `Presentation/Models/`: `ContinueOffer.cs`
- `Presentation/Extensions/`: `ResponseSettingExtension.cs`, `ExceptionExtension.cs` (is this an offline failure)
- Tests: unit `Fakes/FakeSettingService.cs`, `Fakes/FakeMetadataSyncService.cs`, `Fakes/FakePlaybackSessionService.cs`, `Fakes/FakePlayerService.cs`, `Fixtures/SettingStoreContract.cs`, `Fixtures/MetadataListStoreContract.cs`, `Fixtures/PlaybackSessionStoreContract.cs`, test classes per class; integration `FrontendMetadataSyncFlowTest.cs`

Modified: `FakeMetadataStore` and `SqliteMetadataStore` (they implement the three new store interfaces too), `IApiClient` and `ApiClient`, `ApiResponse`, `FakeApiClient`, `SyncService` (uses `ExceptionExtension`), `IPlayerService` and `PlayerService`, `MediaHubService` and `IMediaHubService` tests, `AppStartupService`, `Constants.cs`, both `ServiceCollectionExtensions.cs`, `FrontendClient` (integration fixture), the design docs (status line).

Contracts every task shares:
- `ISettingStore`: `Task<List<ResponseSetting>> GetSettingList()`, `Task ReplaceSettingList(List<ResponseSetting>)` (one atomic write), `Task SaveSetting(ResponseSetting)` (insert or replace by `Key`, an existing one keeps its place), `Task<string?> GetSettingListETag()`, `Task SetSettingListETag(string?)`.
- `IMetadataListStore`, for each kind `Artist`, `Album`, `Genre` (`ResponseArtist`, `ResponseAlbum`, `ResponseGenre`, each with a `Guid ID` and a `TimeModified`): `Task<List<ResponseX>> GetXList()`, `Task ReplaceXList(List<ResponseX>)`, `Task SaveX(ResponseX)`, `Task DeleteX(Guid id)` (an unknown ID is harmless), `Task<string?> GetXListETag()`, `Task SetXListETag(string?)`.
- `IPlaybackSessionStore`: `Task<ResponsePlaybackSession?> GetPlaybackSession()`, `Task SavePlaybackSession(ResponsePlaybackSession)`, `Task ClearPlaybackSession()`, `Task<string?> GetPlaybackSessionETag()`, `Task SetPlaybackSessionETag(string?)`, and the device's own marker: `Task<DateTime?> GetLastPlayedSessionTime()`, `Task SetLastPlayedSessionTime(DateTime? time)` (the `timeModified` of the session this device last played or saved).
- `IApiClient` gains `Task<ApiResponse<List<ResponseSetting>>> GetSettingList(string? eTag)`, `Task<ResponseSetting> UpdateSetting(string key, JsonElement value)`, `Task<ApiResponse<List<ResponseArtist>>> GetArtistList(string? eTag)` (and `GetAlbumList`, `GetGenreList`), `Task<ApiResponse<ResponsePlaybackSession>> GetPlaybackSession(string? eTag)` (status `OK`, `NotModified` or `NoContent`), `Task<ResponsePlaybackSession> SavePlaybackSession(RequestSavePlaybackSession request)`, `Task ClearPlaybackSession()`, `Task MarkPlaylistPlayed(Guid id)`. `ApiResponse<T>` gains `bool IsNoContent => Status == HttpStatusCode.NoContent`.
- `ISettingService`, `IMetadataSyncService`, `IPlaybackSessionService`, `IPlaybackSessionRecorderService`: defined in Tasks 3 to 6.

---

### Task 1: The stores

**Files:**
- Create: `Interfaces/Stores/ISettingStore.cs`, `Interfaces/Stores/IMetadataListStore.cs`, `Interfaces/Stores/IPlaybackSessionStore.cs`; tests `Fixtures/SettingStoreContract.cs`, `Fixtures/MetadataListStoreContract.cs`, `Fixtures/PlaybackSessionStoreContract.cs`
- Modify: `Fakes/FakeMetadataStore.cs` and `Storage/Stores/SqliteMetadataStore.cs` (each also implements the three new interfaces); `FakeMetadataStoreTest.cs` and `SqliteMetadataStoreTest.cs` (also run the three new contracts); `Storage/ServiceCollectionExtensions.cs` (`AddStorage` registers the one `SqliteMetadataStore` instance for `IMetadataStore`, `ISettingStore`, `IMetadataListStore` and `IPlaybackSessionStore`); the integration project (link the three contracts)

**Interfaces:** Produces the three store interfaces above. The SQLite store keeps its one file: a settings table keyed by the setting's `Key` (text), one table per metadata kind keyed by `ID` in the same JSON shape as the existing tables, a single-row table for the session, and the new ETags and the device's last-played marker in the existing `Setting` key-value table.

- [ ] **Step 1: Write the contract tests** (each contract is an abstract class with `CreateStore()` and `[Fact]` methods, like `MetadataStoreContract`). Settings: an empty store has no settings and no ETag; `ReplaceSettingList` replaces rather than merges and keeps the given order; `SaveSetting` inserts a new key at the end and replaces an existing key in place; a boolean, an integer and an enum value each come back equal (the `Value` as a `JsonElement` of the same kind); the ETag round-trips and `null` clears it. Metadata lists, run for artists, albums and genres: replace, save (insert at the end, replace in place), delete (an unknown ID is harmless), every field of a saved item comes back equal (an album with two artist IDs, a count of 0), the three kinds are independent of each other, and each kind's ETag round-trips independently. Session: empty store has none; `SavePlaybackSession` then `Get` returns every field (a null current index and a null playlist ID included); saving again replaces it; `ClearPlaybackSession` removes it and is harmless when there is none; the ETag round-trips and clears with `null`; the last-played marker round-trips as a UTC `DateTime` and clears with `null`; clearing the session does not clear the marker.
- [ ] **Step 2:** Run `*FakeMetadataStoreTest` and `*SqliteMetadataStoreTest`; expected: compile failure. Implement the fake, then the SQLite store (every `Replace…List` one transaction); run both; expected: all pass.
- [ ] **Leave uncommitted.**

---

### Task 2: API client calls

**Files:**
- Modify: `Interfaces/Services/IApiClient.cs`, `Services/ApiClient.cs`, `Models/ApiResponse.cs`, `Fakes/FakeApiClient.cs`; tests `ApiClientTest.cs`

**Interfaces:**
- Consumes: the private `Get<T>`, `Write<T>` and `Send` of `ApiClient`.
- Produces: the `IApiClient` members listed above. `Get<T>` returns `new(HttpStatusCode.NoContent, default, null)` for a `204` instead of failing to read a body. Paths: `/Settings`, `PATCH /Settings/{key}` with body `{"value": <the JsonElement>}`, `/Artist`, `/Album`, `/Genre`, `/PlaybackSession` (`GET`, `PUT`, `DELETE`), `POST /Playlist/{id}/Played` (`204`). `FakeApiClient` gets a scripted handler per new call, recording the ETag or request it was sent, like the existing ones.

- [ ] **Step 1: Write the failing tests** (fake handler, as the existing ones): each read sends the bearer token and `If-None-Match`, returns the list and the ETag on `200`, and `NotModified` with no value on `304`; `GetPlaybackSession_On204_ReturnsNoContentWithNoValue`; `UpdateSetting_PatchesTheKeyWithTheValueAsJson` for a boolean, an integer and a string (the body is exactly `{"value":true}`, `{"value":40}`, `{"value":"Forever"}`) and returns the setting; `SavePlaybackSession_PutsTheRequestBody` (camel-case JSON with `queueMediaFileIDList`, `currentIndex`, `positionSeconds`, `playlistID`); `ClearPlaybackSession_DeletesAndAcceptsA204`; `MarkPlaylistPlayed_PostsToThePlayedPathAndAcceptsA204`; a `404` or `409` problem response on a write throws `ApiException` with its status and title; the bearer, address and single refresh-and-retry rules apply to every new call.
- [ ] **Step 2:** Run `*ApiClientTest`; expected: compile failure. Implement; run the unit project; expected: all pass.
- [ ] **Leave uncommitted.**

---

### Task 3: Setting service

**Files:**
- Create: `Interfaces/Services/ISettingService.cs`, `Services/SettingService.cs`, `Extensions/ResponseSettingExtension.cs`, `Extensions/ExceptionExtension.cs`; tests `SettingServiceTest.cs`, `ResponseSettingExtensionTest.cs`, `ExceptionExtensionTest.cs`, `Fakes/FakeSettingService.cs`
- Modify: `Services/SyncService.cs` (use `ExceptionExtension` instead of its own offline filter)

**Interfaces:**
- Consumes: `IApiClient` (Task 2), `ISettingStore` (Task 1).
- Produces: `extension(Exception exception) { public bool IsOfflineFailure }` (true for `HttpRequestException`, `ServerAddressMissingException`, `TaskCanceledException`); `extension(ResponseSetting setting) { public bool GetBoolean(); public int GetInteger(); public string GetEnumName(); }` (read the `JsonElement`, throw `InvalidOperationException` with the key when the value is not that kind); `SettingService(IApiClient clientApi, ISettingStore storeSetting)` implementing `ISettingService { event Action? Changed; Task<List<ResponseSetting>> GetSettingList(); Task<int> GetVolume(); Task SyncSettingList(bool force = false); Task Update(string key, JsonElement value); Task ApplySettingUpdated(ResponseSetting setting); }`. `GetVolume` returns the stored `Volume` (0 to 100) or `SettingCatalog.Volume`'s default when none is stored yet.

- [ ] **Step 1: Write the failing tests** (`FakeApiClient`, `FakeSettingStore` is the `FakeMetadataStore`): `SyncSettingList_FirstRun_SendsNoETag_StoresTheListAndTheETag`; `…_On304_ChangesNothing`; `…_On200_ReplacesTheLocalList`; `…_WhenTheLocalListIsEmptyButAnETagIsStored_SendsNoETag`; `…_Force_SendsNoETag`; `…_WhenOffline_LeavesListAndETagAlone`; `Changed_IsRaisedAfterEachStoreWrite`; `Update_SendsTheValue_SavesTheServersAnswer_AndRaisesChanged`; `Update_WhenOffline_Throws_AndTheLocalValueIsUnchanged` (Review Focus); `Update_WhenTheServerRefuses_ThrowsApiException_AndTheLocalValueIsUnchanged`; `ApplySettingUpdated_SavesIt_AndKeepsTheStoredETag`; `ApplySettingUpdated_WithAnOlderTimeModified_IsIgnored`; `ApplySettingUpdated_WithTheSameTimeModified_IsIgnored`; `GetVolume_ReturnsTheStoredValue_AndTheDefaultWhenNoneIsStored`. The two extension test classes pin `GetBoolean/GetInteger/GetEnumName` for the three value kinds and the throw for a wrong kind, and `IsOfflineFailure` for each type and for an `ApiException` (false).
- [ ] **Step 2:** Run the new classes; expected: compile failure. Implement; run the unit project (the existing `SyncServiceTest` must still pass); expected: all pass.
- [ ] **Leave uncommitted.**

---

### Task 4: Metadata list sync

**Files:**
- Create: `Interfaces/Services/IMetadataSyncService.cs`, `Services/MetadataSyncService.cs`, `Fakes/FakeMetadataSyncService.cs`; tests `MetadataSyncServiceTest.cs`

**Interfaces:**
- Consumes: `IApiClient`, `IMetadataListStore`, `ExceptionExtension`.
- Produces: `MetadataSyncService(IApiClient clientApi, IMetadataListStore storeMetadataList)` implementing `IMetadataSyncService { event Action? Changed; Task SyncArtistList(bool force = false); Task SyncAlbumList(bool force = false); Task SyncGenreList(bool force = false); Task ApplyArtistUpdated(ResponseArtist artist); Task ApplyArtistDeleted(Guid id); Task ApplyAlbumUpdated(ResponseAlbum album); Task ApplyAlbumDeleted(Guid id); Task ApplyGenreUpdated(ResponseGenre genre); Task ApplyGenreDeleted(Guid id); }`.

- [ ] **Step 1: Write the failing tests**, written once per kind with a theory or three small methods: `Sync…List` has the same eleven behaviours as the setting service's sync tests (first run, 304, 200 replaces, empty list with stored ETag, force, offline, changed raised); `Apply…Updated_SavesIt_AndKeepsTheStoredETag`; `Apply…Updated_WithAnOlderOrEqualTimeModified_IsIgnored`; `Apply…Deleted_RemovesIt_AndAnUnknownIDIsHarmless`; the three kinds never touch each other's lists or ETags.
- [ ] **Step 2:** Run `*MetadataSyncServiceTest`; expected: compile failure. Implement; pass.
- [ ] **Leave uncommitted.**

---

### Task 5: Playback session service and "Continue"

**Files:**
- Create: `Interfaces/Services/IPlaybackSessionService.cs`, `Services/PlaybackSessionService.cs`, `Models/ContinueOffer.cs`, `Fakes/FakePlaybackSessionService.cs`, `Fakes/FakePlayerService.cs`; tests `PlaybackSessionServiceTest.cs`
- Modify: `IPlayerService` and `PlayerService` (Task 7 adds the members this task consumes; this task starts with the interface members only, see Interfaces)

**Interfaces:**
- Consumes: `IApiClient`, `IPlaybackSessionStore`, `IMetadataStore` (`GetMediaFile`, `GetPlaylistList`), `IPlayerService` with `bool IsPlaying`, `bool IsWaitingForDownload` and `Task PlayList(List<Guid> mediaFileIDList, int startIndex, Guid? playlistID = null, TimeSpan? startPosition = null)` (the last is added to the interface in Task 7; here `FakePlayerService` implements it first).
- Produces: `record ContinueOffer(List<Guid> MediaFileIDList, int Index, TimeSpan Position, Guid? PlaylistID, DateTime TimeModified)`; `PlaybackSessionService(IApiClient clientApi, IPlaybackSessionStore storePlaybackSession, IMetadataStore storeMetadata, IPlayerService servicePlayer)` implementing `IPlaybackSessionService { event Action? Changed; Task<ResponsePlaybackSession?> GetSession(); Task SyncPlaybackSession(bool force = false); Task ApplyPlaybackSessionUpdated(ResponsePlaybackSession session); Task ApplyPlaybackSessionCleared(); Task<ContinueOffer?> GetContinueOffer(); Task<bool> Continue(); }`.

- [ ] **Step 1: Write the failing tests:** sync: `SyncPlaybackSession_On200_StoresTheSessionAndTheETag`; `…_On304_ChangesNothing`; `…_On204_RemovesTheLocalSessionAndItsETag`; the empty-local-with-ETag, force and offline rules as for the other lists (the local list being "empty" means no stored session); `Changed` after each write. Push: `ApplyPlaybackSessionUpdated_SavesIt_AndKeepsTheETag`; `…_WithAnOlderOrEqualTimeModified_IsIgnored`; `ApplyPlaybackSessionCleared_RemovesTheSessionAndTheETag_AndIsHarmlessWhenThereIsNone`. Offer (Review Focus): `GetContinueOffer_ForANewerSession_ReturnsTheQueueIndexPositionAndPlaylist`; `…_WhenTheSessionIsNotNewerThanTheLastPlayedMarker_IsNull`; `…_WithNoLastPlayedMarker_OffersAnySession`; `…_WhenNoSessionIsStored_IsNull`; `…_DropsMediaFilesThatLeftTheLibrary_AndTheIndexFollowsTheMediaFileThatWasCurrent`; `…_WhenTheCurrentMediaFileLeftTheLibrary_TheIndexPointsAtTheNextOne_WithPositionZero`; `…_DropsAPlaylistThatNoLongerExists`; `…_WhenEveryMediaFileIsGone_IsNull`; `…_WhenTheCurrentIndexIsNull_StartsAtTheFirstEntryAtZero`; `…_WhenThePlayerIsPlaying_IsNull`; `…_WhenThePlayerIsWaitingForADownload_IsNull`. `Continue_PlaysTheOffer_AndRecordsItsTimeAsTheLastPlayedMarker` (the fake player gets `PlayList(ids, index, playlistID, position)` and the marker equals the offer's `TimeModified`); `Continue_WithNoOffer_ReturnsFalse_AndPlaysNothing`.
- [ ] **Step 2:** Run `*PlaybackSessionServiceTest`; expected: compile failure. Implement; pass.
- [ ] **Leave uncommitted.**

---

### Task 6: Recorder, saving the session from the player

**Files:**
- Create: `Interfaces/Services/IPlaybackSessionRecorderService.cs`, `Services/PlaybackSessionRecorderService.cs`; tests `PlaybackSessionRecorderServiceTest.cs`
- Modify: `Constants.cs` (`SessionSaveSeconds` = 30), `Interfaces/Services/IPlayerService.cs` and `Services/PlayerService.cs` (add `int? CurrentIndex` and `Guid? PlaylistID`, see Task 7 for how the playlist is set; here `CurrentIndex` is the index of the current entry in `QueueList`, null when nothing is current), `Fakes/FakePlayerService.cs`

**Interfaces:**
- Consumes: `IPlayerService` (`QueueList`, `Current`, `CurrentIndex`, `PlaylistID`, `IsPlaying`, `Position`, `Changed`), `IApiClient.SavePlaybackSession` and `ClearPlaybackSession`, `IPlaybackSessionStore`, `IDelay`.
- Produces: `PlaybackSessionRecorderService(IPlayerService servicePlayer, IApiClient clientApi, IPlaybackSessionStore storePlaybackSession, IDelay delay, ILogger<PlaybackSessionRecorderService> logger)` implementing `IPlaybackSessionRecorderService { void Start(); Task SaveNow(); }`. `Start` subscribes to `servicePlayer.Changed` once (a second call does nothing). A change in the player's signature (`IsPlaying`, `CurrentIndex`, the queue's ID list, `PlaylistID`) triggers a save; a change of `Position` alone does not. While `IsPlaying`, a loop waits `Constants.SessionSaveSeconds` through `IDelay` and saves again. A save sends the queue IDs, the current index, the position in seconds and the playlist ID; an empty queue calls `ClearPlaybackSession` instead. A save in flight with another trigger arriving runs once more after it, never in parallel. A save that succeeds stores the answer with `SavePlaybackSession` and sets the last-played marker to its `TimeModified`, so this device is never offered its own session. A failed save is logged (type and subject only) and dropped.

- [ ] **Step 1: Write the failing tests** (`FakePlayerService` with raisable `Changed` and settable state, `FakeApiClient`, `FakeDelay`, the fake store): `ChangingTheCurrentMediaFile_Saves_WithTheQueueIndexPositionAndPlaylist`; `StartingToPlay_Saves`; `Pausing_Saves`; `APositionOnlyChange_DoesNotSave` (Review Focus); `WhilePlaying_SavesAgainAfterEachThirtySecondWait_AndStopsWhenPaused` (the fake delay records `[30 s, 30 s]` for two loops); `AnEmptiedQueue_ClearsTheServerSession_InsteadOfSavingAnEmptyOne`; `ABurstOfChanges_DuringASave_RunsOneMoreSaveAfterIt_NeverTwoAtOnce`; `AFailedSave_IsLoggedAndDropped_AndTheNextTriggerSavesAgain`; `ASuccessfulSave_StoresTheSessionAndTheMarker`; `Start_CalledTwice_SubscribesOnce`; `NoLogLineHoldsATokenOrAQueue`.
- [ ] **Step 2:** Run `*PlaybackSessionRecorderServiceTest`; expected: compile failure. Implement; pass.
- [ ] **Leave uncommitted.**

---

### Task 7: The player reports a playlist as played and can start from a position

**Files:**
- Modify: `Interfaces/Services/IPlayerService.cs`, `Services/PlayerService.cs`, `Fakes/FakePlayerService.cs`; tests `PlayerServiceTest.cs`

**Interfaces:**
- Consumes: `IApiClient.MarkPlaylistPlayed` (Task 2).
- Produces: `PlayerService` takes one more constructor argument, `IApiClient clientApi`. `IPlayerService` gains `Guid? PlaylistID { get; }` and `PlayList(List<Guid> mediaFileIDList, int startIndex, Guid? playlistID = null, TimeSpan? startPosition = null)` (the two optional parameters replace nothing: the two-argument call still works). `Play`, `Enqueue`, `Clear` and a `PlayList` with no playlist ID leave `PlaylistID` null.

- [ ] **Step 1: Write the failing tests** (add to `PlayerServiceTest`, with a `FakeApiClient` that records `MarkPlaylistPlayed`): `PlayList_FromAPlaylist_ReportsItPlayedOnceThePlaylistStartsPlaying`; `PlayList_WithNoPlaylistID_ReportsNothing`; `ReportingPlayed_WhenOffline_IsIgnored_AndPlaybackContinues`; `AFollowingEntryStarting_DoesNotReportAgain`; `PlayList_WithAStartPosition_SeeksBeforeTheFirstEntryPlays_AndOnlyForThatEntry` (the audio fake records `Seek` before `Play`, and the next entry gets no seek); `PlaylistID_IsSetByPlayList_AndClearedByPlayAndClear`; `CurrentIndex_FollowsTheCurrentEntry_AndIsNullWhenNothingIsCurrent`.
- [ ] **Step 2:** Run `*PlayerServiceTest`; expected: compile failure. Implement: the report is fire-and-forget through the existing `Fire` helper, a failure is logged at warning without detail; the start position is consumed by the first successful `Load`. Run the unit project; expected: all pass.
- [ ] **Leave uncommitted.**

---

### Task 8: The synced volume

**Files:**
- Modify: `Services/PlayerService.cs` (one more constructor argument, `ISettingService serviceSetting`), `Fakes/FakeSettingService.cs`; tests `PlayerServiceTest.cs`

**Interfaces:**
- Consumes: `ISettingService.GetVolume()`, `Update(SettingKey.Volume, value)`, `Changed` (Task 3).
- Produces: the player's volume follows the `Volume` setting (0 to 100 on the server, 0 to 1 in the player): the stored value is applied the first time the player starts a media file and whenever `ISettingService.Changed` reports a different value; `SetVolume` applies it at once and then writes `Volume` (rounded to a whole number) through `Update`; a write that fails (offline) is logged and does not undo the local volume. A volume applied because of a setting change is never written back.

- [ ] **Step 1: Write the failing tests:** `TheStoredVolume_IsAppliedWhenTheFirstMediaFileStarts`; `SetVolume_AppliesAtOnce_AndWritesTheSettingRounded` (0.456 becomes 46); `SetVolume_WhenTheWriteFails_KeepsTheLocalVolume_AndLogsIt`; `ASettingChangeFromAnotherDevice_ChangesThePlayerVolume_AndIsNotWrittenBack`; `AnUnchangedSetting_DoesNotTouchThePlayer`.
- [ ] **Step 2:** Run `*PlayerServiceTest`; expected: compile failure. Implement; run the unit project; expected: all pass.
- [ ] **Leave uncommitted.**

---

### Task 9: Hub, start-up, registrations and the end-to-end test

**Files:**
- Create: integration test `FrontendMetadataSyncFlowTest.cs`
- Modify: `Services/MediaHubService.cs` (three more constructor arguments: `ISettingService`, `IMetadataSyncService`, `IPlaybackSessionService`), `Services/AppStartupService.cs` (and the recorder), both `ServiceCollectionExtensions.cs`, `Fixtures/FrontendClient.cs` (build and expose the new services and a recorder), `MediaHubServiceTest.cs`, `AppStartupServiceTest.cs`, the maui design doc (status line)

**Interfaces:**
- Consumes: every service of Tasks 3 to 6.
- Produces: `MediaHubService.HandledEventNameList` grows to these names, in this order: the five existing ones, then `SettingUpdated`, `ArtistUpdated`, `ArtistDeleted`, `AlbumUpdated`, `AlbumDeleted`, `GenreUpdated`, `GenreDeleted`, `PlaybackSessionUpdated`, `PlaybackSessionCleared` (all `nameof(MediaHubEventEnum.X)`), each forwarded to the matching `Apply…`; the clear push carries no payload. On `Reconnected` the hub runs the library and playlist syncs as before and then the settings, the three lists and the session syncs. `AppStartupService.Start` syncs, after the library and the playlists and each in its own try so one failure does not skip the rest: settings, artists, albums, genres, the session; then starts the hub and calls `servicePlaybackSessionRecorder.Start()`.

- [ ] **Step 1: Write the failing tests.** Unit: `MediaHubServiceTest.TheHandledEvents_AreExactlyThoseThisPlanSyncs` (the list above); `AppStartupServiceTest`: `Start_SyncsTheNewSubjectsAfterTheLibraryAndPlaylists_ThenConnectsTheHub`, `Start_WhenOneNewSyncFails_StillRunsTheRest`, `Start_StartsTheRecorder`. Integration `FrontendMetadataSyncFlowTest` (real API through `ApiFactory`, the `FrontendClient` fixture with the new services): a setting changed through the API arrives as a push and updates the store with no ETag change, and `GetVolume` follows it; a settings write through `SettingService.Update` is accepted by the server and read back by a second client; an artist created, renamed and deleted through the API arrives as pushes and the store follows; a session saved through the API (`PUT /PlaybackSession`) arrives as `PlaybackSessionUpdated` and `GetContinueOffer` offers it on a second client; clearing it (`DELETE`) arrives as `PlaybackSessionCleared` and the local session is gone; the sync after dropping and restoring the connection brings all of them level; a cleared store with the ETags gone is refilled by the next sync; the recorder, with the fake audio player playing two media files, makes the server's session hold the queue, the index and the playlist (read through the API), and the same client is not offered its own session while a second client is; `Continue` on the second client calls `PlayList` with the saved index and position.
- [ ] **Step 2:** Run the new tests; expected: compile failure. Implement the hub forwarding, the start-up steps and the registrations (`AddPresentation` registers the four new services as singletons; `AddStorage` already registers the store interfaces); build the solution for both targets (0 warnings), run both test projects, report counts before and after the plan.
- [ ] **Step 3:** Add one status line to the maui design doc: settings, metadata lists and the playback session are synced on the device, the player saves its session and offers "Continue", and what is left is the screens.
- [ ] **Leave uncommitted.**
