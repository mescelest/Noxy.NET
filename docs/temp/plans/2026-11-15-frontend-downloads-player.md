# Frontend Downloads and Player Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:executing-plans (the user wants this done natively, in the main session, not through subagents). Steps use checkbox (`- [ ]`) syntax for tracking. **Do not start before `2026-11-15-frontend-foundation.md` is done**: this plan builds on its `IMetadataStore`, `IApiClient`, `ISyncService` and `ISessionService`, on the SQLite store, and on the `Presentation`, `Storage` and `App` projects it creates.

**Goal:** Add everything between the metadata and the sound: a replaceable file store (one file per media file on disk), per-media-file file state, a download service that verifies every file against its `contentHash`, reconciliation when the library changes, opt-in bulk downloads, and a player that only ever plays from the device store with a queue and prefetch.

**Architecture:** The logic lives in `Noxy.NET.MediaPlayer.Presentation` behind interfaces, tested in the unit test project with fakes (file store, audio player, API client) and end to end in the integration project against the real API. The disk file store lives in `Storage` and is tested against a temporary folder, so it needs no manual check. Only the audio players, one per platform, live in `Noxy.NET.MediaPlayer.App` and are verified by hand. Fluxor state and every page are the screens plan, which waits for the user's visual design.

**Tech Stack:** .NET 10, .NET MAUI (Windows and Android), `System.Security.Cryptography` for SHA-256, xunit v3.

**Spec:** `docs/specs/2026-11-15-frontend-maui-design.md` (storage, hashing, playback) and `docs/specs/2026-11-15-frontend-sync-player-design.md` (file sync per media file, integrity, the player, the replaceable store, "Calls made while writing this" for hash, resume, playback and stale-hash decisions). Previous plan: `2026-11-15-frontend-foundation.md`.

## Global Constraints

- Work in `Presentation`, `Storage` and `App` (only for the audio players), plus the two test projects. Target `net10.0`. The backend is not changed. **No MAUI, Blazor or platform type in `Presentation` or `Storage`.**
- All conventions of the foundation plan apply: one class per file, no summary comment above a class, interfaces in `Interfaces/<same folder as the implementation>`, extensions in `Extensions`, `List` suffix, no `yield`, no magic strings, `is null`, no `!`, method-syntax LINQ, kind-first dependency names (`storeFile`, `serviceHash`, `playerAudio`, `clientApi`, `serviceDownload`), `time` keeps its name, no commits, `git add -N` new files by name, no machine-level commands, zero warnings, unit tests in `Noxy.NET.Test.UnitTest/MediaPlayer/Presentation/`, store and end-to-end tests in the integration project, fakes in `Fakes`, helpers in `Fixtures`, and a test folder contains only tests.
- A media file is "on this device" only when its bytes are in the file store **and** their SHA-256 equals the `contentHash` of the library entry they were downloaded for. A partial download is never written to the file store: it lives in a separate partial area and is only moved into the store after the hash matches.
- Only these start a download: opening a playlist, playing or queueing a media file that is missing, and an explicit download (one media file, a playlist, the whole library). Nothing starts because a list scrolled or a screen opened.
- The player plays only from the file store: no network call is made to play a media file that is on the device.
- Limits: a small number of parallel downloads (`Constants.MaxParallelDownloads` = 3), prefetch of the next `Constants.PrefetchCount` = 3 media files. Files are streamed to disk and hashed from the file, never held whole in memory.
- State a media file can be in: `NotDownloaded`, `Downloading`, `OnDevice`, `Failed`.

## Review Focus

- **Hash mismatch.** A file whose bytes do not match the `contentHash` must not become `OnDevice`, must not stay in the store, and must stop retrying after `Constants.MaxDownloadAttempts` (3) instead of looping (Task 5).
- **Library changes under a download or a play.** If a media file's `contentHash` changes or the media file is deleted while it downloads, the running download is cancelled, its partial file is discarded and its state returns to `NotDownloaded`; a deletion while it plays lets the current play finish and then skips it (Tasks 6 and 9).
- **Cleared app data, a deleted file or a full disk.** If the user or the OS removed some or all files, start-up repair marks exactly those media files `NotDownloaded`, deletes leftover partial files and never reports a missing file as available; if the disk is full, a bulk download stops and says why, and a single download fails with a clear state, not a crash (Tasks 2, 5, 6, 7).
- **Interrupted download.** Within a session a download resumes from its partial file with `Range` and `If-Range` against the same `contentHash`; if the server answers `200` to a ranged request (the file changed), the partial bytes are thrown away and the download restarts from zero. A partial file left by an earlier run is discarded at start-up, never resumed (Tasks 4, 5, 6).
- **Queue edge cases.** Next at the end of the queue, previous at the start, a queue entry that is not playable (failed download), the same media file queued twice, and an empty queue must each have defined behaviour and a test (Task 9).
- **A file the player cannot decode.** The audio player raises `Failed`; the queue moves on and the entry is marked failed without deleting a verified file (Tasks 8 and 9).

## File Structure

New files (relative to the project named; tests to the test projects' `MediaPlayer/Presentation/`):
- `Presentation/Interfaces/Services/`: `IDownloadService.cs`, `IHashService.cs`, `IPlayerService.cs`, `IFileReconciler.cs`, `IAudioPlayer.cs`
- `Presentation/Interfaces/Stores/`: `IFileStore.cs`
- `Presentation/Services/`: `DownloadService.cs`, `HashService.cs`, `PlayerService.cs`, `FileReconciler.cs`
- `Presentation/Models/`: `FileState.cs`, `FileSyncStateEnum.cs` (enum), `StorageEstimate.cs`, `DownloadProgress.cs`, `LibraryDiff.cs`, `PlayerQueueEntry.cs`, `ApiFileResponse.cs`; `Exceptions/StorageFullException.cs`; `Extensions/ContentTypeExtension.cs` (file extension for a content type)
- `Storage/Stores/`: `DiskFileStore.cs`
- `App/Services/`: `AudioPlayer.cs` with the per-platform parts under `App/Platforms/Windows` and `App/Platforms/Android`
- Tests: unit `Fakes/FakeFileStore.cs`, `Fakes/FakeAudioPlayer.cs`, `Fakes/FakeDelay.cs`, `Fixtures/FileStoreContract.cs`; test classes per class; integration `DiskFileStoreTest.cs`, `FrontendDownloadFlowTest.cs`

Modified: `IMetadataStore`, `FakeMetadataStore` and `SqliteMetadataStore` (file state), `IApiClient` and `ApiClient` (`GetFile`), `SyncService` (raises `LibraryDiffApplied`), `ISyncService`, `Constants.cs`, `ServiceCollectionExtensions.cs` (both), `App/MauiProgram.cs` (registrations), the start-up service (repair), the design docs (status).

Contracts every task shares:
- `enum FileSyncStateEnum { NotDownloaded, Downloading, OnDevice, Failed }`; `record FileState(Guid MediaFileID, FileSyncStateEnum State, string? VerifiedHash, string? Error)`; `record StorageEstimate(long UsedBytes, long FreeBytes)` (the bytes the store holds and the free space on the volume the store is on).
- `IMetadataStore` gains `Task<List<FileState>> GetFileStateList()`, `Task SaveFileState(FileState)`, `Task DeleteFileState(Guid mediaFileID)`; a missing state means `NotDownloaded`.
- `IFileStore`: the held files: `Task<string?> GetPath(Guid mediaFileID)` (the full path of a held file, null when not held), `Task<Stream?> Open(Guid mediaFileID)`, `Task Delete(Guid mediaFileID)`, `Task<List<Guid>> GetHeldIDList()`, `Task<StorageEstimate> GetEstimate()`; and the partial area: `Task<long> GetPartialLength(Guid mediaFileID)` (0 when none), `Task AppendPartial(Guid mediaFileID, Stream content)`, `Task<Stream> OpenPartial(Guid mediaFileID)`, `Task DiscardPartial(Guid mediaFileID)`, `Task<List<Guid>> GetPartialIDList()`, and `Task Commit(Guid mediaFileID, string contentType)` (moves the partial file into the store, replacing a held file, and removes the partial). A disk that is full throws `StorageFullException` and leaves the held files untouched.
- `IHashService`: `Task<string> Sha256Hex(Stream content)` (lower-case hex, the format of `contentHash`; reads the stream from its current position to the end).
- `IApiClient` gains `Task<ApiFileResponse> GetFile(Guid id, long? rangeStart, string? ifRange)`, where `record ApiFileResponse(HttpStatusCode Status, Stream Content, string? ETag, long? TotalBytes) : IDisposable`; `Status` is `OK` or `PartialContent`, and the caller disposes the response.
- `IDownloadService`: `Task<bool> EnsureOnDevice(Guid mediaFileID)` (downloads if needed and returns true when the file is verified on the device), `Task Enqueue(List<Guid> mediaFileIDList)`, `Task Cancel(Guid mediaFileID)`, `Task QueueMissing(List<Guid> mediaFileIDList)` (bulk: only those not `OnDevice`, stops when the disk is low), `DownloadProgress GetProgress()`, `event Action? Changed`.
- `ISyncService` gains `event Action<LibraryDiff>? LibraryDiffApplied`; `record LibraryDiff(List<ResponseMediaFile> AddedList, List<ResponseMediaFile> ChangedList, List<Guid> RemovedIDList)`, where `ChangedList` holds media files whose `ContentHash` differs from the previous list.
- `IPlayerService` and `IAudioPlayer` are defined in Tasks 8 and 9.

---

### Task 1: File state in the metadata store

**Files:**
- Create: `Models/FileSyncStateEnum.cs`, `Models/FileState.cs`; tests added to `Fixtures/MetadataStoreContract.cs`
- Modify: `IMetadataStore.cs`, `Fakes/FakeMetadataStore.cs`, `Storage/Stores/SqliteMetadataStore.cs`

**Interfaces:** Produces the `FileState` members above.

- [ ] **Step 1: Write the failing contract tests:** an empty store has no file states; `SaveFileState` inserts or replaces by media file ID and `GetFileStateList` returns it (every field, including a null hash and a null error); `DeleteFileState` of an unknown ID is harmless; `DeleteMediaFile` also deletes that media file's state; `ReplaceMediaFileList` removes the state of media files no longer in the list.
- [ ] **Step 2:** Run `*FakeMetadataStoreTest` and `*SqliteMetadataStoreTest`; expected: compile failure. Implement the fake, then the SQLite store (a new table keyed by media file ID, written in the same transaction as the media file changes that remove it); run both; expected: pass.
- [ ] **Leave uncommitted.**

---

### Task 2: File store

**Files:**
- Create: `Interfaces/Stores/IFileStore.cs`, `Models/StorageEstimate.cs`, `Exceptions/StorageFullException.cs`, `Extensions/ContentTypeExtension.cs`, `Storage/Stores/DiskFileStore.cs`; tests `Fakes/FakeFileStore.cs`, `Fixtures/FileStoreContract.cs`, `FakeFileStoreTest.cs` (unit), `DiskFileStoreTest.cs` (integration, links the contract)

**Interfaces:** Produces `IFileStore`, `StorageEstimate` and `DiskFileStore(string baseFolder)`: held files at `{base}/files/{mediaFileID}{extension}` and partial files at `{base}/partial/{mediaFileID}`. The extension comes from `ContentTypeExtension` (`audio/mpeg` to `.mp3`, `audio/mp4` and `audio/x-m4a` to `.m4a`, `audio/flac` to `.flac`, `audio/ogg` to `.ogg`, `audio/wav` and `audio/x-wav` to `.wav`, anything else to `.bin`), because the platform players decide a decoder partly by extension.

- [ ] **Step 1: Write the contract tests:** a committed file is held, `GetPath` is non-null and the file at that path has exactly the committed bytes; an unknown ID has no path and no stream; appending twice builds one partial file whose length is the sum and `GetPartialLength` reports it; `OpenPartial` reads what was appended; a partial file is not in `GetHeldIDList` and has no path; `Commit` moves the partial into the store and `GetPartialLength` is 0 afterwards; committing the same ID twice replaces the file; `Delete` removes it and is harmless for an unknown ID; `DiscardPartial` of an unknown ID is harmless; `GetHeldIDList` lists exactly the held IDs and `GetPartialIDList` exactly the partial ones; `GetEstimate` used bytes grow by the committed size; `Commit` with no partial file throws `InvalidOperationException`; the fake configured as full throws `StorageFullException` from `AppendPartial` and holds nothing partial or held beyond what it had.
- [ ] **Step 2:** Run `*FakeFileStoreTest`; expected: compile failure. Implement the fake and the exception; pass.
- [ ] **Step 3:** Write `DiskFileStoreTest` (inherits the contract, one temporary folder per test, deleted in `Dispose`) with extra tests: the extension follows the content type and falls back to `.bin`; `Commit` is atomic (the held file is never half written: replace by writing to a temporary name and moving it); a file name in the folder that is not a GUID is ignored by the lists; a missing folder is created; a path outside the base folder can never be produced (IDs are GUIDs, so assert the built path starts with the base folder); `GetEstimate` free bytes are the volume's available free space. Implement `DiskFileStore`; a disk-full `IOException` (HResult for `ERROR_DISK_FULL` or `ERROR_HANDLE_DISK_FULL` on Windows, `ENOSPC` on Android) becomes `StorageFullException`.
- [ ] **Step 4:** Run both projects; expected: all pass.
- [ ] **Leave uncommitted.**

---

### Task 3: Hash service

**Files:**
- Create: `Interfaces/Services/IHashService.cs`, `Services/HashService.cs`; tests `HashServiceTest.cs`

**Interfaces:** Produces `IHashService` and `HashService`.

- [ ] **Step 1: Write the failing tests:** the bytes of `abc` hash to `ba7816bf8f01cfea414140de5dae2223b00361a396177a9cb410ff61f20015ad`; an empty stream hashes to `e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855`; the output is lower-case hex of 64 characters; a 50 MB stream of a repeating pattern hashes to the same value as `SHA256.HashData` on the same bytes, without the service holding the whole stream (feed it a stream that throws if more than one buffer is read ahead of the consumer, or simply assert the service accepts a non-seekable stream); a stream already read partway hashes from its current position; a cancelled token throws `OperationCanceledException`.
- [ ] **Step 2:** Run `*HashServiceTest`; expected: compile failure. Implement with `SHA256.HashDataAsync(stream, cancellationToken)`; pass.
- [ ] **Leave uncommitted.**

---

### Task 4: Streaming and ranged file download in the API client

**Files:**
- Modify: `Interfaces/Services/IApiClient.cs`, `Services/ApiClient.cs`; tests `ApiClientTest.cs`
- Create: `Models/ApiFileResponse.cs`

**Interfaces:** Produces `ApiClient.GetFile` as above.

- [ ] **Step 1: Write the failing tests:** `GetFile_WithNoRange_Sends…GET /Media/{id}/File` and returns `200`, a readable stream of the bytes and the ETag; the request uses `HttpCompletionOption.ResponseHeadersRead` (the body is not buffered: assert with a handler whose content throws if it is read to the end before the caller reads); `GetFile_WithARange_SendsRangeAndIfRange` (`Range: bytes=N-`, `If-Range` equal to the given ETag) and returns `206` with the stream and the total size taken from `Content-Range`; `GetFile_WhenTheServerAnswers200ToARangedRequest_ReturnsOKWithTheWholeFile` (the file changed); the bearer token, the saved server address and the single refresh-and-retry apply as for the other calls; a 404 throws `ApiException`; disposing the response disposes the stream and the underlying response.
- [ ] **Step 2:** Run `*ApiClientTest`; expected: compile failure. Implement; pass.
- [ ] **Leave uncommitted.**

---

### Task 5: Download service

**Files:**
- Create: `Interfaces/Services/IDownloadService.cs`, `Services/DownloadService.cs`, `Models/DownloadProgress.cs`, `Fakes/FakeDelay.cs`; tests `DownloadServiceTest.cs`
- Modify: `Constants.cs` (`MaxParallelDownloads`, `MaxDownloadAttempts`, `RetryBaseDelaySeconds` = 2), both `ServiceCollectionExtensions.cs`

**Interfaces:**
- Consumes: `IApiClient.GetFile`, `IFileStore`, `IHashService`, `IMetadataStore`, `TimeProvider`, and an `IDelay` seam (`Task Wait(TimeSpan, CancellationToken)`, real implementation uses `Task.Delay`; `FakeDelay` completes at once and records the delays) so backoff is testable. Produces `DownloadService(IApiClient clientApi, IFileStore storeFile, IHashService serviceHash, IMetadataStore storeMetadata, IDelay delay)` implementing `IDownloadService`.

- [ ] **Step 1: Write the failing tests** with fakes: `EnsureOnDevice_DownloadsVerifiesAndStores_ThenStateIsOnDeviceWithTheVerifiedHash`; `EnsureOnDevice_WhenAlreadyOnDeviceWithTheSameHash_DoesNotDownload`; `EnsureOnDevice_WhenOnDeviceWithADifferentHash_DownloadsAgain`; `EnsureOnDevice_WhenTheHashDoesNotMatch_StoresNothing_DiscardsThePartial_AndEndsFailedAfterMaxAttempts` (three requests, then `Failed` with an error text, nothing held, no partial left); `EnsureOnDevice_RetriesATransientFailure_WithGrowingDelays` (delays 2 s, 4 s recorded by `FakeDelay`); `EnsureOnDevice_AnInterruptedDownload_ResumesFromThePartialWithRangeAndIfRange`; `EnsureOnDevice_WhenARangedRequestReturns200_DiscardsThePartialBytes`; `EnsureOnDevice_ConcurrentCallsForTheSameMediaFile_DownloadOnce`; `AtMostThreeDownloadsRunAtOnce` (ten enqueued, the fake counts the maximum in flight); `Cancel_StopsARunningDownload_DiscardsThePartial_AndLeavesNotDownloaded`; `EnsureOnDevice_WhenTheDiskIsFull_EndsFailedWithAClearError_AndDiscardsThePartial_NotACrash`; `EnsureOnDevice_ForAnUnknownMediaFile_ReturnsFalse`; `StateChanges_RaiseChanged`; the state sequence is `Downloading` then `OnDevice` or `Failed`; the verified file is committed with the library entry's content type.
- [ ] **Step 2:** Run `*DownloadServiceTest`; expected: compile failure.
- [ ] **Step 3:** Implement with a bounded worker pool; the hash comes from the library entry in `IMetadataStore.GetMediaFileList()` read when the download starts; the response stream is appended to the partial file in chunks, the partial is hashed with `IHashService` from `OpenPartial`, and only a match is committed; a `StorageFullException` is not retried.
- [ ] **Step 4:** Run the unit project; expected: all pass.
- [ ] **Leave uncommitted.**

---

### Task 6: Reconciling files with the library, and start-up repair

**Files:**
- Create: `Interfaces/Services/IFileReconciler.cs`, `Services/FileReconciler.cs`, `Models/LibraryDiff.cs`; tests `FileReconcilerTest.cs`
- Modify: `Services/SyncService.cs`, `Interfaces/Services/ISyncService.cs` (raise `LibraryDiffApplied` after a library replace and after `ApplyMediaUpdated`, `ApplyMediaDeleted`, `ApplyUploadSucceeded`), `AppStartupService.cs` (call `Repair()` after restore), tests in `SyncServiceTest.cs` and `AppStartupServiceTest.cs`

**Interfaces:**
- Produces `FileReconciler(IFileStore storeFile, IMetadataStore storeMetadata, IDownloadService serviceDownload)` with `Task Apply(LibraryDiff diff)` and `Task Repair()`; it subscribes to `ISyncService.LibraryDiffApplied` when the app starts.

- [ ] **Step 1: Write the failing tests.** Sync: a library replace raises one `LibraryDiffApplied` listing added, changed (different `ContentHash`) and removed IDs; a metadata-only change raises none for that file; a push raises the matching small diff. Reconciler: a removed media file loses its file, its partial, its state and any running download; a changed hash cancels the running download and sets `NotDownloaded` (the held bytes are deleted, since they are another version); an added media file changes nothing; `Repair`: a state `OnDevice` whose file is missing becomes `NotDownloaded`; held files with no state or whose media file is gone are deleted; held files with a state `OnDevice` and a matching library hash are left alone; every partial file is deleted (none is resumed across a restart) and a state `Downloading` left by an earlier run becomes `NotDownloaded`; `Repair` on an empty store changes nothing.
- [ ] **Step 2:** Run the classes; expected: compile failure. Implement; pass.
- [ ] **Leave uncommitted.**

---

### Task 7: Bulk downloads

**Files:**
- Modify: `Services/DownloadService.cs`, `Interfaces/Services/IDownloadService.cs`; tests `DownloadServiceTest.cs`

- [ ] **Step 1: Write the failing tests:** `QueueMissing_QueuesOnlyMediaFilesNotOnDevice`; `QueueMissing_ShowsProgress` (`GetProgress()` reports total, finished and failed counts and the bytes done); `QueueMissing_WhenFreeSpaceIsLowerThanTheNextFile_StopsAndReportsWhy` (progress carries a `StoppedReason` of `StorageLow` and the remaining files stay `NotDownloaded`); `QueueMissing_CalledAgain_DoesNotQueueTheSameMediaFilesTwice`; `QueueMissing_ForAnEmptyList_DoesNothing`; media files added to the library later are not queued by a running bulk (only the list passed in is); `Enqueue_ForAPlaylist_StartsNothingUntilCalled` (no download starts from reading state).
- [ ] **Step 2:** Run `*DownloadServiceTest`; expected: compile failure. Implement: before each file, compare `GetEstimate().FreeBytes` with its `sizeBytes` plus a small reserve (`Constants.StorageReserveBytes` = 50 MB). Pass.
- [ ] **Leave uncommitted.**

---

### Task 8: Audio player

**Files:**
- Create: `Presentation/Interfaces/Services/IAudioPlayer.cs`; test `Fakes/FakeAudioPlayer.cs`; `App/Services/AudioPlayer.cs` with `App/Platforms/Windows/…` and `App/Platforms/Android/…`

**Interfaces:**
- Produces `IAudioPlayer`: `Task Load(string path)` (a file path from `IFileStore.GetPath`), `Task Play()`, `Task Pause()`, `Task Seek(TimeSpan position)`, `Task SetVolume(double volume)` (0 to 1), `Task Stop()` (releases the file so it can be deleted), `TimeSpan Position`, `TimeSpan Duration`, `bool IsPlaying`, `event Action? Ended`, `event Action? Changed` (position, duration or state changed), `event Action<string>? Failed`. It is the only contract that knows about a sound device. `FakeAudioPlayer` records calls and lets a test raise `Ended`, `Changed` and `Failed`.

- [ ] **Step 1:** Implement `FakeAudioPlayer` with the unit tests that need it (it is exercised by the player service tests in Task 9).
- [ ] **Step 2:** Implement the App's `AudioPlayer` per platform behind the interface: Windows with `Windows.Media.Playback.MediaPlayer`, Android with `Android.Media.MediaPlayer` (or the player library the builder prefers), registered in `MauiProgram.cs`. Position and `Changed` come from a timer, not on every sample, at most once a second while playing. `Stop()` releases the file handle.
- [ ] **Step 3:** Verify by hand on Windows (and Android when its SDK is installed) with a downloaded file from `DiskFileStore`: play, pause, seek, volume, `Ended` fires at the end, `Failed` fires for a corrupt file, and a file can be deleted right after `Stop()`. Record the result in the report.
- [ ] **Leave uncommitted.**

---

### Task 9: Player service and queue

**Files:**
- Create: `Interfaces/Services/IPlayerService.cs`, `Services/PlayerService.cs`, `Models/PlayerQueueEntry.cs`; tests `PlayerServiceTest.cs`
- Modify: `Constants.cs` (`PrefetchCount`), both `ServiceCollectionExtensions.cs`

**Interfaces:**
- Consumes: `IAudioPlayer`, `IDownloadService`, `IFileStore`, `IMetadataStore`, `ISyncService.LibraryDiffApplied`. Produces `IPlayerService`: `Task Play(Guid mediaFileID)` (clears the queue and plays that media file), `Task PlayList(List<Guid> mediaFileIDList, int startIndex)`, `Task Enqueue(Guid mediaFileID)`, `Task Remove(int index)`, `Task Pause()`, `Task Resume()`, `Task Seek(TimeSpan)`, `Task Next()`, `Task Previous()`, `Task SetVolume(double)`, `Task Clear()`, `PlayerQueueEntry? Current`, `List<PlayerQueueEntry> QueueList`, `bool IsPlaying`, `TimeSpan Position`, `TimeSpan Duration`, `double Volume`, `bool IsWaitingForDownload`, `event Action? Changed`; `record PlayerQueueEntry(Guid MediaFileID, FileSyncStateEnum State)`.

- [ ] **Step 1: Write the failing tests** (fakes for everything): `Play_WhenOnDevice_LoadsTheStoredPath_AndPlays_WithNoDownload`; `Play_WhenMissing_DownloadsFirst_ThenPlays_AndIsWaitingForDownloadInBetween`; `Play_WhenTheDownloadFails_DoesNotPlay_AndSkipsToTheNextPlayableEntry`; `Failed_FromTheAudioPlayer_MarksTheEntryFailed_AndMovesOn_WithoutDeletingTheVerifiedFile`; `Ended_AdvancesToTheNextEntry`; `Ended_AtTheEndOfTheQueue_Stops`; `Next_AtTheEnd_DoesNothing`; `Previous_AtTheStart_RestartsTheCurrentMediaFile`; `Enqueue_ThenPlay_TheSameMediaFileTwice_PlaysItTwice`; `Remove_TheCurrentEntry_PlaysTheNextOne`; `Clear_StopsAndEmptiesTheQueue`; `Prefetch_RequestsTheNextThreeMissingMediaFiles_NotMore`; `Prefetch_NeverStartsDownloadsForEntriesBeyondTheWindow`; `SetVolume_IsClampedToZeroAndOne`; `ADeletedMediaFile_InTheQueue_IsSkipped_AfterTheCurrentOneFinishes`; `ADeletedCurrentMediaFile_FinishesPlaying_ThenIsSkipped`; `AChangedHash_ForTheCurrentMediaFile_FinishesPlaying_ThenIsReDownloadedOnNextPlay`; the audio player is stopped before the next file loads, so the previous file is never held open; an empty queue and `Resume()` with nothing loaded do nothing.
- [ ] **Step 2:** Run `*PlayerServiceTest`; expected: compile failure. Implement. The player asks `IMetadataStore` for the media file's data and `IFileStore.GetPath`, never the network directly.
- [ ] **Step 3:** Run the unit project; expected: all pass.
- [ ] **Leave uncommitted.**

---

### Task 10: End to end, wiring and the acceptance run

**Files:**
- Create: integration test `FrontendDownloadFlowTest.cs`
- Modify: both `ServiceCollectionExtensions.cs` (register every new service and store), `App/MauiProgram.cs` (the data folder for `DiskFileStore` is `FileSystem.AppDataDirectory`, not a cache folder), the design docs (status line)

- [ ] **Step 1: Write the failing test** (real API through `ApiFactory`; real `ApiClient`, `SessionService`, `SyncService`, `DownloadService`, `FileReconciler`, `PlayerService`, `HashService`, `SqliteMetadataStore` and `DiskFileStore` in temporary folders; `FakeAudioPlayer`): upload a media file and sync; `EnsureOnDevice` downloads it and its state is `OnDevice` with the server's `contentHash` and the file on disk is byte-for-byte the upload; a second call does no request; play loads the stored path and makes no request; edit the media file's tags through the API and the pushed update does not re-download it; deleting it through the API removes its file and state; a download interrupted by a dropped response resumes from the partial file and ends with a verified file (a test handler cuts the first response); deleting the app data folder's files and running `Repair` marks the media file `NotDownloaded` and a new `EnsureOnDevice` downloads it again; the player queue plays two media files in order and prefetches the third.
- [ ] **Step 2:** Run it; expected: compile failure, then implement the registrations until it passes.
- [ ] **Step 3:** Build the solution for both platform targets (0 warnings), run both test projects, and report counts before and after the whole plan.
- [ ] **Step 4:** Manual acceptance on Windows (and Android when its SDK is installed) against the running API, using a temporary diagnostic that runs the flow since the screens come later: sign in, let the library sync, play a media file (it downloads, verifies and plays), restart the app and play it again with the network off, delete it on another client and watch the file disappear from the app data folder. Remove the diagnostic afterwards and record what happened in the report.
- [ ] **Leave uncommitted.**
