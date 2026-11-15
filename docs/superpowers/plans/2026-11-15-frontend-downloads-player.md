# Frontend Downloads and Player Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:executing-plans (the user wants this done natively, in the main session, not through subagents). Steps use checkbox (`- [ ]`) syntax for tracking. **Do not start before `2026-11-15-frontend-foundation.md` is done**: this plan builds on its `IMetadataStore`, `IApiClient`, `ISyncService` and `ISessionService`.

**Goal:** Add everything between the metadata and the sound: a replaceable file store (IndexedDB blobs in the browser), per-media-file file state, a download service that verifies every file against its `contentHash`, reconciliation when the library changes, opt-in bulk downloads, and a player that only ever plays from the device store with a queue and prefetch.

**Architecture:** All code lives in `Noxy.NET.MediaPlayer.Presentation` behind interfaces, tested in the unit test project with fakes (file store, hash service, audio player, API client) and end to end in the integration project against the real API. The browser pieces (IndexedDB blobs, `SubtleCrypto`, the `<audio>` element) are thin JS modules behind interfaces, verified by hand in a browser. Fluxor state and every page are the screens plan, which waits for the user's visual design.

**Tech Stack:** .NET 10, Blazor WebAssembly, JS modules in `Presentation/wwwroot`, xunit v3.

**Spec:** `docs/superpowers/specs/2026-11-15-frontend-sync-player-design.md` (file sync per media file, integrity, the player, the replaceable store, "Calls made while writing this" for hash, resume, playback and stale-hash decisions). Previous plan: `2026-11-15-frontend-foundation.md`.

## Global Constraints

- Work in `Noxy.NET.MediaPlayer.Presentation` (and `Frontend` only if wiring needs it), plus the two test projects. Target `net10.0`. The backend is not changed.
- All conventions of the foundation plan apply: one class per file, no summary comment above a class, interfaces in `Interfaces/<same folder as the implementation>`, extensions in `Extensions`, `List` suffix, no `yield`, no magic strings, `is null`, no `!`, method-syntax LINQ, kind-first dependency names (`storeFile`, `serviceHash`, `playerAudio`, `clientApi`, `serviceDownload`), `time` keeps its name, no commits, `git add -N` new files by name, no machine-level commands, zero warnings, unit tests in `Noxy.NET.Test.UnitTest/MediaPlayer/Presentation/`, end to end in the integration project, fakes in `Fakes`, helpers in `Fixtures`.
- A media file is "on this device" only when its bytes are in the file store **and** their SHA-256 equals the `contentHash` of the library entry they were downloaded for. A partial download is never written to the file store.
- Only these start a download: opening a playlist, playing or queueing a media file that is missing, and an explicit download (one media file, a playlist, the whole library). Nothing starts because a list scrolled or a screen opened.
- The player plays only from the file store: no network call is made to play a media file that is on the device.
- Limits from the design: `Storage:MaxSizeBytes` is 100 MB per file, so one file is buffered in memory for hashing; a small number of parallel downloads (`Constants.MaxParallelDownloads` = 3), prefetch of the next `Constants.PrefetchCount` = 3 media files.
- State a media file can be in: `NotDownloaded`, `Downloading`, `OnDevice`, `Failed`.

## Review Focus

- **Hash mismatch.** A file whose bytes do not match the `contentHash` must not become `OnDevice`, must not stay in the store, and must stop retrying after `Constants.MaxDownloadAttempts` (3) instead of looping (Task 5).
- **Library changes under a download or a play.** If a media file's `contentHash` changes or the media file is deleted while it downloads, the running download is cancelled and its state returns to `NotDownloaded`; a deletion while it plays lets the current play finish and then skips it (Tasks 6 and 9).
- **Cleared or partial storage.** If the browser dropped some or all blobs, start-up repair marks exactly those media files `NotDownloaded` and never reports a missing file as available; if storage is full, a bulk download stops and says why, and a single download fails with a clear state, not a crash (Tasks 2, 5, 6, 7).
- **Interrupted download.** Within a session a download resumes with `Range` and `If-Range` against the same `contentHash`; if the server answers `200` to a ranged request (the file changed), the partial bytes are thrown away and the download restarts from zero (Tasks 4 and 5).
- **Queue edge cases.** Next at the end of the queue, previous at the start, a queue entry that is not playable (failed download), the same media file queued twice, and an empty queue must each have defined behaviour and a test (Task 9).

## File Structure

New files (relative to `Noxy.NET.MediaPlayer.Presentation/`; tests to the test projects' `MediaPlayer/Presentation/`):
- `Interfaces/Services/`: `IDownloadService.cs`, `IHashService.cs`, `IPlayerService.cs`, `IFileReconciler.cs`, `IAudioPlayer.cs`
- `Interfaces/Stores/`: `IFileStore.cs`
- `Services/`: `DownloadService.cs`, `HashService.cs`, `PlayerService.cs`, `FileReconciler.cs`, `AudioPlayer.cs`
- `Stores/`: `IndexedDbFileStore.cs`; `wwwroot/fileStore.js`, `wwwroot/hash.js`, `wwwroot/audioPlayer.js`
- `Models/`: `FileState.cs`, `FileSyncState.cs` (enum), `StorageEstimate.cs`, `DownloadProgress.cs`, `LibraryDiff.cs`, `PlayerQueueEntry.cs`
- Tests: unit `Fakes/FakeFileStore.cs`, `Fakes/FakeHashService.cs`, `Fakes/FakeAudioPlayer.cs`, `Fakes/FakeDelay.cs`; test classes per class; integration `FrontendDownloadFlowTest.cs`

Modified: `IMetadataStore` and `FakeMetadataStore` (file state), `IApiClient` and `ApiClient` (`GetFile`), `SyncService` (raises `LibraryDiffApplied`), `ISyncService`, `Constants.cs`, `ServiceCollectionExtensions.cs` (registrations), the start-up service (repair), the design doc (status).

Contracts every task shares:
- `enum FileSyncState { NotDownloaded, Downloading, OnDevice, Failed }`; `record FileState(Guid MediaFileID, FileSyncState State, string? VerifiedHash, string? Error)`; `record StorageEstimate(long UsedBytes, long QuotaBytes)` with `long FreeBytes => QuotaBytes - UsedBytes`.
- `IMetadataStore` gains `Task<List<FileState>> GetFileStateList()`, `Task SaveFileState(FileState)`, `Task DeleteFileState(Guid mediaFileID)`; a missing state means `NotDownloaded`.
- `IFileStore`: `Task Save(Guid mediaFileID, byte[] content, string contentType)`, `Task<string?> GetPlayableUrl(Guid mediaFileID)` (an object URL, null when not held), `Task ReleasePlayableUrl(string url)`, `Task Delete(Guid mediaFileID)`, `Task<List<Guid>> GetHeldIDList()`, `Task<StorageEstimate> GetEstimate()`, `Task<bool> RequestPersistence()`.
- `IHashService`: `Task<string> Sha256Hex(byte[] content)` (lower-case hex, the format of `contentHash`).
- `IApiClient` gains `Task<ApiFileResponse> GetFile(Guid id, long? rangeStart, string? ifRange)`, where `record ApiFileResponse(HttpStatusCode Status, byte[] Content, string? ETag, long? TotalBytes)`; `Status` is `OK` or `PartialContent`.
- `IDownloadService`: `Task<bool> EnsureOnDevice(Guid mediaFileID)` (downloads if needed and returns true when the file is verified on the device), `Task Enqueue(List<Guid> mediaFileIDList)`, `Task Cancel(Guid mediaFileID)`, `Task QueueMissing(List<Guid> mediaFileIDList)` (bulk: only those not `OnDevice`, stops when storage is low), `DownloadProgress GetProgress()`, `event Action? Changed`.
- `ISyncService` gains `event Action<LibraryDiff>? LibraryDiffApplied`; `record LibraryDiff(List<ResponseMediaFile> AddedList, List<ResponseMediaFile> ChangedList, List<Guid> RemovedIDList)`, where `ChangedList` holds media files whose `ContentHash` differs from the previous list.
- `IPlayerService` and `IAudioPlayer` are defined in Tasks 8 and 9.

---

### Task 1: File state in the metadata store

**Files:**
- Create: `Models/FileSyncState.cs`, `Models/FileState.cs`; tests added to `Fixtures/MetadataStoreContract.cs`
- Modify: `IMetadataStore.cs`, `Fakes/FakeMetadataStore.cs`, `Stores/IndexedDbMetadataStore.cs`, `wwwroot/metadataStore.js`

**Interfaces:** Produces the `FileState` members above.

- [ ] **Step 1: Write the failing contract tests:** an empty store has no file states; `SaveFileState` inserts or replaces by media file ID and `GetFileStateList` returns it; `DeleteFileState` of an unknown ID is harmless; `DeleteMediaFile` also deletes that media file's state; `ReplaceMediaFileList` removes the state of media files no longer in the list.
- [ ] **Step 2:** Run `*FakeMetadataStoreTest`; expected: compile failure. Implement the fake, then the IndexedDB store and its JS (a new object store keyed by media file ID); run the unit tests; expected: pass. The IndexedDB side is verified in the browser as in the foundation plan.
- [ ] **Leave uncommitted.**

---

### Task 2: File store

**Files:**
- Create: `Interfaces/Stores/IFileStore.cs`, `Models/StorageEstimate.cs`, `Stores/IndexedDbFileStore.cs`, `wwwroot/fileStore.js`; tests `Fakes/FakeFileStore.cs`, `Fixtures/FileStoreContract.cs`, `FakeFileStoreTest.cs`

**Interfaces:** Produces `IFileStore` and `StorageEstimate` as above.

- [ ] **Step 1: Write the contract tests:** a saved file is held and its playable URL is non-null; an unknown ID has no URL; saving the same ID twice replaces; `Delete` removes it and is harmless for an unknown ID; `GetHeldIDList` lists exactly the held IDs; `GetEstimate` used bytes grow by the saved size and `FreeBytes` shrinks; `ReleasePlayableUrl` of an unknown URL is harmless; saving when the fake is configured as full throws a `StorageFullException` (new, in `Exceptions/`) and holds nothing partial.
- [ ] **Step 2:** Run `*FakeFileStoreTest`; expected: compile failure. Implement the fake and the exception; pass.
- [ ] **Step 3:** Implement the IndexedDB store and JS module: blobs keyed by media file ID, `URL.createObjectURL` for the playable URL, `navigator.storage.estimate()` and `navigator.storage.persist()`; a `QuotaExceededError` is rethrown as `StorageFullException`. Verify in the browser (save, play URL, delete, estimate, and a forced full-quota case if the browser allows it) and record the result in the report.
- [ ] **Leave uncommitted.**

---

### Task 3: Hash service

**Files:**
- Create: `Interfaces/Services/IHashService.cs`, `Services/HashService.cs`, `wwwroot/hash.js`; tests `Fakes/FakeHashService.cs`, `HashServiceContract` is not needed: the real service is verified in the browser

**Interfaces:** Produces `IHashService`.

- [ ] **Step 1:** Implement `HashService(IJSRuntime)` calling a JS function that runs `crypto.subtle.digest("SHA-256", bytes)` and returns lower-case hex. `FakeHashService` computes SHA-256 with `System.Security.Cryptography` (so tests use real hashes) and can be told to return a wrong value for a given media file.
- [ ] **Step 2:** Verification in the browser: hash the bytes of `abc` and expect `ba7816bf8f01cfea414140de5dae2223b00361a396177a9cb410ff61f20015ad`; hash a 50 MB array and record the time. Record both in the report.
- [ ] **Leave uncommitted.**

---

### Task 4: Ranged file download in the API client

**Files:**
- Modify: `Interfaces/Services/IApiClient.cs`, `Services/ApiClient.cs`; tests `ApiClientTest.cs`
- Create: `Models/ApiFileResponse.cs`

**Interfaces:** Produces `ApiClient.GetFile` as above.

- [ ] **Step 1: Write the failing tests:** `GetFile_WithNoRange_Sends…GET /Media/{id}/File` and returns `200`, the bytes and the ETag; `GetFile_WithARange_SendsRangeAndIfRange` (`Range: bytes=N-`, `If-Range` equal to the given ETag) and returns `206` with the bytes and the total size taken from `Content-Range`; `GetFile_WhenTheServerAnswers200ToARangedRequest_ReturnsOKWithTheWholeFile` (the file changed); the bearer token and the single refresh-and-retry apply as for the other calls; a 404 throws `ApiException`.
- [ ] **Step 2:** Run `*ApiClientTest`; expected: compile failure. Implement; pass.
- [ ] **Leave uncommitted.**

---

### Task 5: Download service

**Files:**
- Create: `Interfaces/Services/IDownloadService.cs`, `Services/DownloadService.cs`, `Models/DownloadProgress.cs`, `Fakes/FakeDelay.cs`; tests `DownloadServiceTest.cs`
- Modify: `Constants.cs` (`MaxParallelDownloads`, `MaxDownloadAttempts`, `RetryBaseDelaySeconds` = 2), `ServiceCollectionExtensions.cs`

**Interfaces:**
- Consumes: `IApiClient.GetFile`, `IFileStore`, `IHashService`, `IMetadataStore`, `TimeProvider`, and an `IDelay` seam (`Task Wait(TimeSpan, CancellationToken)`, real implementation uses `Task.Delay`; `FakeDelay` completes at once and records the delays) so backoff is testable. Produces `DownloadService(IApiClient clientApi, IFileStore storeFile, IHashService serviceHash, IMetadataStore storeMetadata, IDelay delay)` implementing `IDownloadService`.

- [ ] **Step 1: Write the failing tests** with fakes: `EnsureOnDevice_DownloadsVerifiesAndStores_ThenStateIsOnDeviceWithTheVerifiedHash`; `EnsureOnDevice_WhenAlreadyOnDeviceWithTheSameHash_DoesNotDownload`; `EnsureOnDevice_WhenOnDeviceWithADifferentHash_DownloadsAgain`; `EnsureOnDevice_WhenTheHashDoesNotMatch_StoresNothing_AndEndsFailedAfterMaxAttempts` (three requests, then `Failed` with an error text, nothing in the file store); `EnsureOnDevice_RetriesATransientFailure_WithGrowingDelays` (delays 2 s, 4 s recorded by `FakeDelay`); `EnsureOnDevice_AnInterruptedDownload_ResumesWithRangeAndIfRange`; `EnsureOnDevice_WhenARangedRequestReturns200_DiscardsThePartialBytes`; `EnsureOnDevice_ConcurrentCallsForTheSameMediaFile_DownloadOnce`; `AtMostThreeDownloadsRunAtOnce` (ten enqueued, the fake counts the maximum in flight); `Cancel_StopsARunningDownload_AndLeavesNotDownloaded_WithNothingStored`; `EnsureOnDevice_WhenStorageIsFull_EndsFailedWithAClearError_NotACrash`; `EnsureOnDevice_ForAnUnknownMediaFile_ReturnsFalse`; `StateChanges_RaiseChanged`; the state sequence is `Downloading` then `OnDevice` or `Failed`.
- [ ] **Step 2:** Run `*DownloadServiceTest`; expected: compile failure.
- [ ] **Step 3:** Implement with a bounded worker pool; the hash comes from the library entry in `IMetadataStore.GetMediaFileList()` read when the download starts and compared with `IHashService`; the file is saved only after the hash matches; a `StorageFullException` is not retried.
- [ ] **Step 4:** Run the unit project; expected: all pass.
- [ ] **Leave uncommitted.**

---

### Task 6: Reconciling files with the library, and start-up repair

**Files:**
- Create: `Interfaces/Services/IFileReconciler.cs`, `Services/FileReconciler.cs`, `Models/LibraryDiff.cs`; tests `FileReconcilerTest.cs`
- Modify: `Services/SyncService.cs`, `Interfaces/Services/ISyncService.cs` (raise `LibraryDiffApplied` after a library replace and after `ApplyMediaUpdated`, `ApplyMediaDeleted`, `ApplyUploadSucceeded`), `AppStartupService.cs` (call `Repair()` after restore), tests in `SyncServiceTest.cs` and `AppStartupServiceTest.cs`

**Interfaces:**
- Produces `FileReconciler(IFileStore storeFile, IMetadataStore storeMetadata, IDownloadService serviceDownload)` with `Task Apply(LibraryDiff diff)` and `Task Repair()`; it subscribes to `ISyncService.LibraryDiffApplied` when the app starts.

- [ ] **Step 1: Write the failing tests.** Sync: a library replace raises one `LibraryDiffApplied` listing added, changed (different `ContentHash`) and removed IDs; a metadata-only change raises none for that file; a push raises the matching small diff. Reconciler: a removed media file loses its file, its state and any running download; a changed hash cancels the running download and sets `NotDownloaded` (the held bytes are deleted, since they are another version); an added media file changes nothing; `Repair`: a state `OnDevice` whose bytes are missing becomes `NotDownloaded`; held bytes with no state or whose media file is gone are deleted; held bytes with a state `OnDevice` and a matching library hash are left alone; `Repair` on an empty store changes nothing.
- [ ] **Step 2:** Run the classes; expected: compile failure. Implement; pass.
- [ ] **Leave uncommitted.**

---

### Task 7: Bulk downloads

**Files:**
- Modify: `Services/DownloadService.cs`, `Interfaces/Services/IDownloadService.cs`; tests `DownloadServiceTest.cs`

- [ ] **Step 1: Write the failing tests:** `QueueMissing_QueuesOnlyMediaFilesNotOnDevice`; `QueueMissing_ShowsProgress` (`GetProgress()` reports total, finished and failed counts and the bytes done); `QueueMissing_WhenFreeSpaceIsLowerThanTheNextFile_StopsAndReportsWhy` (progress carries a `StoppedReason` of `StorageLow` and the remaining files stay `NotDownloaded`); `QueueMissing_CalledAgain_DoesNotQueueTheSameMediaFilesTwice`; `QueueMissing_ForAnEmptyList_DoesNothing`; media files added to the library later are not queued by a running bulk (only the list passed in is); `Enqueue_ForAPlaylist_StartsNothingUntilCalled` (no download starts from reading state).
- [ ] **Step 2:** Run `*DownloadServiceTest`; expected: compile failure. Implement: before each file, compare `FreeBytes` with its `sizeBytes`; request persistence once, on the first download after sign-in. Pass.
- [ ] **Leave uncommitted.**

---

### Task 8: Audio player wrapper

**Files:**
- Create: `Interfaces/Services/IAudioPlayer.cs`, `Services/AudioPlayer.cs`, `wwwroot/audioPlayer.js`; test `Fakes/FakeAudioPlayer.cs`

**Interfaces:**
- Produces `IAudioPlayer`: `Task Load(string url)`, `Task Play()`, `Task Pause()`, `Task Seek(TimeSpan position)`, `Task SetVolume(double volume)` (0 to 1), `TimeSpan Position`, `TimeSpan Duration`, `bool IsPlaying`, `event Action? Ended`, `event Action? Changed` (position, duration or state changed), `event Action<string>? Failed`. It is the only class that knows about the audio element. `FakeAudioPlayer` records calls and lets a test raise `Ended`, `Changed` and `Failed`.

- [ ] **Step 1:** Implement `AudioPlayer(IJSRuntime)` as a thin wrapper over one `<audio>` element created by the JS module (no markup needed), with `DotNetObjectReference` callbacks for the events.
- [ ] **Step 2:** Verify in the browser with a stored file's playable URL: play, pause, seek, volume, `Ended` fires at the end, `Failed` fires for a bad URL. Record the result in the report.
- [ ] **Leave uncommitted.**

---

### Task 9: Player service and queue

**Files:**
- Create: `Interfaces/Services/IPlayerService.cs`, `Services/PlayerService.cs`, `Models/PlayerQueueEntry.cs`; tests `PlayerServiceTest.cs`
- Modify: `Constants.cs` (`PrefetchCount`), `ServiceCollectionExtensions.cs`

**Interfaces:**
- Consumes: `IAudioPlayer`, `IDownloadService`, `IFileStore`, `IMetadataStore`, `ISyncService.LibraryDiffApplied`. Produces `IPlayerService`: `Task Play(Guid mediaFileID)` (clears the queue and plays that media file), `Task PlayList(List<Guid> mediaFileIDList, int startIndex)`, `Task Enqueue(Guid mediaFileID)`, `Task Remove(int index)`, `Task Pause()`, `Task Resume()`, `Task Seek(TimeSpan)`, `Task Next()`, `Task Previous()`, `Task SetVolume(double)`, `Task Clear()`, `PlayerQueueEntry? Current`, `List<PlayerQueueEntry> QueueList`, `bool IsPlaying`, `TimeSpan Position`, `TimeSpan Duration`, `double Volume`, `bool IsWaitingForDownload`, `event Action? Changed`; `record PlayerQueueEntry(Guid MediaFileID, FileSyncState State)`.

- [ ] **Step 1: Write the failing tests** (fakes for everything): `Play_WhenOnDevice_LoadsTheStoredUrl_AndPlays_WithNoDownload`; `Play_WhenMissing_DownloadsFirst_ThenPlays_AndIsWaitingForDownloadInBetween`; `Play_WhenTheDownloadFails_DoesNotPlay_AndSkipsToTheNextPlayableEntry`; `Ended_AdvancesToTheNextEntry`; `Ended_AtTheEndOfTheQueue_Stops`; `Next_AtTheEnd_DoesNothing`; `Previous_AtTheStart_RestartsTheCurrentMediaFile`; `Enqueue_ThenPlay_TheSameMediaFileTwice_PlaysItTwice`; `Remove_TheCurrentEntry_PlaysTheNextOne`; `Clear_StopsAndEmptiesTheQueue`; `Prefetch_RequestsTheNextThreeMissingMediaFiles_NotMore`; `Prefetch_NeverStartsDownloadsForEntriesBeyondTheWindow`; `SetVolume_IsClampedToZeroAndOne`; `ADeletedMediaFile_InTheQueue_IsSkipped_AfterTheCurrentOneFinishes`; `ADeletedCurrentMediaFile_FinishesPlaying_ThenIsSkipped`; `AChangedHash_ForTheCurrentMediaFile_FinishesPlaying_ThenIsReDownloadedOnNextPlay`; the object URL of the previous media file is released when the next one loads; an empty queue and `Resume()` with nothing loaded do nothing.
- [ ] **Step 2:** Run `*PlayerServiceTest`; expected: compile failure. Implement. The player asks `IMetadataStore` for the media file's data and `IFileStore.GetPlayableUrl`, never the network directly.
- [ ] **Step 3:** Run the unit project; expected: all pass.
- [ ] **Leave uncommitted.**

---

### Task 10: End to end, wiring and the acceptance run

**Files:**
- Create: integration test `FrontendDownloadFlowTest.cs`
- Modify: `ServiceCollectionExtensions.cs` (register every new service and store), `Frontend/Program.cs` if start-up order needs it, the design doc (status line)

- [ ] **Step 1: Write the failing test** (real API through `ApiFactory`; real `ApiClient`, `SessionService`, `SyncService`, `DownloadService`, `FileReconciler`, `PlayerService`; `FakeFileStore`, `FakeMetadataStore`, `FakeAudioPlayer`, real SHA-256 through `FakeHashService`): upload a media file and sync; `EnsureOnDevice` downloads it and its state is `OnDevice` with the server's `contentHash`; a second call does no request; play loads the stored URL and makes no request; edit the media file's tags through the API and the pushed update does not re-download it; deleting it through the API removes its file and state; a download interrupted by a dropped response resumes and ends with a verified file (a test handler cuts the first response); the player queue plays two media files in order and prefetches the third.
- [ ] **Step 2:** Run it; expected: compile failure, then implement the registrations until it passes.
- [ ] **Step 3:** Build the solution (0 warnings), run both test projects, and report counts before and after the whole plan.
- [ ] **Step 4:** Manual acceptance in a browser against the running API: sign in, let the library sync, play a media file (it downloads, verifies and plays), reload the page and play it again with the network off, delete it on another client and watch it disappear. Record what happened in the report.
- [ ] **Leave uncommitted.**
