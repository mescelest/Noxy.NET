# Drop the content hash from the client contract Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** `ContentHash` leaves `ResponseMediaFile`; a device checks a finished download by `SizeBytes` and treats a file as held by its ID and state. The server keeps the hash for upload de-duplication and as the opaque ETag of `GET /Media/{id}/File`.

**Architecture:** The client stops reading the hash in four places (`DownloadService`, `FileReconciler`, `SyncService.Diff`, `FileState`), then the property is removed and the compiler finds what is left. Server entity, table, upload job and `MediaDownload.ContentHash` are untouched.

**Tech Stack:** C# / .NET 10, xUnit v3. Run unit tests from `Noxy.NET.Test/Noxy.NET.Test.UnitTest` with `dotnet run -- -class "*<ClassName>" -noLogo` (`dotnet test` is not set up for MTP); the integration project works the same way.

**Spec:** `docs/specs/2026-11-25-drop-content-hash-from-clients-design.md`

## Global Constraints

- The hash is never exposed in `ResponseMediaFile`; `EntityMediaFile.ContentHash`, `TableMediaFile`, `EntityMediaUploadJob` and upload de-duplication (`FindMediaFileByHash`) do not change.
- `GET /Media/{id}/File` keeps the stored hash as its ETag.
- A stored file's bytes never change; no replace-the-audio feature, no audio-only hash.
- Rules from the project memory: one class per file, `is null`, LINQ method syntax, no magic strings, enum switches end in a throwing default, DI names kind first, tests only in test folders (helpers in `Fixtures`/`Fakes`).
- Local data (the device's SQLite rows are JSON) and the server database can be deleted; no migration.
- Run only the tests of the area touched, the full unit suite and the integration tests once at the end (Task 4).

## Review Focus

- A download that ends cleanly but shorter than `SizeBytes`: must never be committed, and must resume (keeps its ETag) on the next attempt.
- A download longer than `SizeBytes`: discarded with the ETag forgotten, retried from zero.
- A ranged request answered `200`: the partial is discarded (exists today), and the fresh full body is then checked against `SizeBytes`.
- A metadata-only change to a held file (new `TimeModified`, same bytes): the file stays `OnDevice` and held after `FileReconciler.Repair` and after the sync diff.
- Upload of the same file twice is still refused, and `/File` still answers with the stored hash as its ETag.

---

### Task 1: Download check by size

**Files:**
- Modify: `Noxy.NET.MediaPlayer/Noxy.NET.MediaPlayer.Presentation/Services/DownloadService.cs` (`IsOnDevice`, `TryDownload`, constructor)
- Modify: `Noxy.NET.MediaPlayer/Noxy.NET.MediaPlayer.Presentation/Resources/PresentationTexts.resx` (+ `.Designer.cs`): `DownloadHashMismatch` becomes `DownloadSizeMismatch` with the wording "The downloaded file is not the size it should be." (rename the key, Designer property and its users)
- Test: `Noxy.NET.Test/Noxy.NET.Test.UnitTest/MediaPlayer/Presentation/DownloadServiceTest.cs`

**Interfaces:**
- Consumes: `ResponseMediaFile.SizeBytes`, `IFileStore.GetPartialLength(Guid)`, `IFileStore.DiscardPartial(Guid)`, `IFileStore.Commit(Guid, string)`.
- Produces: `FileState` rows written as `new(id, FileSyncStateEnum.OnDevice, null, null)` (the `VerifiedHash` slot is `null` until Task 3 removes it). `IsOnDevice(media)` is true for state `OnDevice` with the file held, with no hash comparison.

- [ ] **Step 1: Rewrite the tests that name the hash.** In `DownloadServiceTest` change these to the new rule, keeping their setup:
  - `EnsureOnDevice_DownloadsVerifiesAndStores_ThenStateIsOnDeviceWithTheVerifiedHash` becomes `..._ThenStateIsOnDevice`, asserting `new(media.ID, OnDevice, null, null)`.
  - `EnsureOnDevice_WhenAlreadyOnDeviceWithTheSameHash_DoesNotDownload` becomes `..._WhenAlreadyOnDevice_DoesNotDownload`.
  - `EnsureOnDevice_WhenOnDeviceWithADifferentHash_DownloadsAgain` is deleted (nothing can differ).
  - `EnsureOnDevice_WhenTheHashDoesNotMatch_...AfterMaxAttempts` becomes `..._WhenTheSizeDoesNotMatch_StoresNothing_DiscardsThePartial_AndEndsFailedAfterMaxAttempts` (served body one byte longer than `SizeBytes`); `..._WhenTheHashMatchesOnTheSecondTry_EndsOnDevice` becomes `..._WhenTheSizeMatchesOnTheSecondTry_EndsOnDevice`.
  - Test media in `DownloadServiceTest` is built with `SizeBytes = bytes.Length`.
- [ ] **Step 2: Add the two focused tests.**
  - `EnsureOnDevice_WhenTheBodyEndsShort_KeepsThePartialAndResumesWithIfRange_ThenCommits`: first response delivers fewer bytes than `SizeBytes` and ends cleanly, second (a `206`) delivers the rest. Assert the second request carries `Range` and `If-Range` (the first response's ETag), the file is committed once, and the final state is `OnDevice`.
  - `EnsureOnDevice_WhenARangedRequestReturns200_ChecksTheFreshBodyAgainstTheSize` extends the existing `...Returns200_DiscardsThePartialBytes` setup: after the discard the full body of exactly `SizeBytes` ends `OnDevice`.
- [ ] **Step 3: Run `*DownloadServiceTest`.** Expected: the changed and new tests FAIL (the code still hashes).
- [ ] **Step 4: Implement in `DownloadService`.**
  - `TryDownload`: after `AppendPartial`, read `long length = await storeFile.GetPartialLength(id)`. `length == media.SizeBytes` commits and sets `new(id, OnDevice, null, null)`. `length < media.SizeBytes` returns `false` without discarding and without clearing `eTag` (the next attempt resumes). `length > media.SizeBytes` discards the partial, sets `eTag.Value = null`, returns `false`. A `false` return keeps ending as the error `PresentationTexts.DownloadSizeMismatch`.
  - `IsOnDevice`: `state is { State: OnDevice } && await storeFile.GetPath(media.ID) is not null`.
  - Remove the `serviceHash` constructor parameter and field if nothing else in the class uses it; update the construction in `DownloadServiceTest` and wherever else `DownloadService` is constructed (compiler finds them). Leave `HashService` itself if something else uses it.
- [ ] **Step 5: Run `*DownloadServiceTest`.** Expected: all PASS.
- [ ] **Step 6: Commit** the test file and the production files together only if the user asks (project rule: production code stays uncommitted); stage new files by name.

### Task 2: Held by ID, no changed list

**Files:**
- Modify: `Presentation/Services/FileReconciler.cs` (`Apply`, `IsVerified`, comment in `Repair`)
- Modify: `Presentation/Models/LibraryDiff.cs`, `Presentation/Services/SyncService.cs` (`Diff`), `Presentation/Interfaces/Services/ISyncService.cs` (the comment on `LibraryDiffApplied`)
- Modify: whatever else reads `LibraryDiff.ChangedList` (search the solution; `QueueService.HandleLibraryDiff`, `AppStartupService.ApplyLibraryDiff` are the places that take a `LibraryDiff`)
- Test: `.../Presentation/FileReconcilerTest.cs`, `.../Presentation/SyncServiceTest.cs`, plus the tests of any other class changed

**Interfaces:**
- Consumes: `FileState` and `ResponseMediaFile` as they are.
- Produces: `public record LibraryDiff(List<ResponseMediaFile> AddedList, List<Guid> RemovedIDList)` with `IsEmpty => AddedList.Count == 0 && RemovedIDList.Count == 0`. `FileReconciler.Apply(LibraryDiff)` forgets only removed IDs. `Repair` keeps a held file whose media file is in the library and whose state is `OnDevice`.

- [ ] **Step 1: Update tests first.** In `FileReconcilerTest`: delete the tests about a changed hash and `Apply_ForAChangedMediaFile...`; rename any `...Verified...` test to the new rule; add `Repair_KeepsAHeldFileWhoseMediaFileWasModified` (media `TimeModified` newer than the state's time, same ID, state `OnDevice`, file held: after `Repair` the file is still held and the state row remains). In `SyncServiceTest` delete the three `ChangedList` assertions (lines near 691, 712-713, 767) and the tests that exist only to produce a changed entry; add `Diff_AMetadataOnlyChange_RaisesNoLibraryDiff` if no existing test already proves it.
- [ ] **Step 2: Run `*FileReconcilerTest` and `*SyncServiceTest`.** Expected: compile errors or FAIL.
- [ ] **Step 3: Implement.** Remove `ChangedList` from `LibraryDiff` and from `SyncService.Diff` (added and removed only). Remove the `ChangedList` loop from `FileReconciler.Apply`. `IsVerified` becomes `state.State == FileSyncStateEnum.OnDevice` (rename to `IsHeld`; update its comment and the one in `Repair`). Fix every compile error from the `LibraryDiff` change; keep behaviour of the other consumers for added and removed files. Update the `ISyncService` comment to "added or removed".
- [ ] **Step 4: Run `*FileReconcilerTest`, `*SyncServiceTest`, `*QueueServiceTest`, `*AppStartupServiceTest`** (those that exist). Expected: PASS.

### Task 3: Remove the properties

**Files:**
- Modify: `Presentation/Models/FileState.cs` to `public record FileState(Guid MediaFileID, FileSyncStateEnum State, string? Error);`
- Modify: `Domain/Responses/Media/ResponseMediaFile.cs` (remove `ContentHash`), `Domain/Extensions/EntityMediaFileExtension.cs` (`ToResponse` no longer sets it)
- Modify: every caller of `new FileState(...)` (drop the third argument) and every test, fake and fixture that sets or reads `ContentHash` or `VerifiedHash` on the client side: `FakeMetadataStore`, `MetadataStoreContract`, `FakeApiClient`, `MediaFileFactory`, `ApiSeeder`, the handler tests (`HandlerMediaListTest`, `HandlerMediaLibraryTest`, ...), `FrontendDownloadFlowTest`, `HubNotificationFlowTest`, `HandlerNotificationTest`, `MediaDownloadFlowTest` where it reads the response's hash
- Test: the compiler and the test classes above

**Interfaces:**
- Consumes: Tasks 1 and 2 (nothing reads the hash any more).
- Produces: `FileState(Guid MediaFileID, FileSyncStateEnum State, string? Error)`; `ResponseMediaFile` without `ContentHash`.

- [ ] **Step 1: Remove the two properties and build the solution.** Expected: compile errors in the files listed.
- [ ] **Step 2: Fix them.** Server tests that need the hash read `EntityMediaFile.ContentHash` or the ETag header, not the response. Tests that assert a response's hash are deleted or changed to assert `SizeBytes`. Do not add the hash back to any response.
- [ ] **Step 3: Add `Seed_AnOldFileStateRowWithAVerifiedHash_StillLoads`** to `SqliteMetadataStoreTest` (integration): write a `FileState` row whose JSON still has `"VerifiedHash"` straight into the table, then `GetFileStateList()` returns it. This pins that local data from before this change does not break the app.
- [ ] **Step 4: Build.** Expected: no warnings from the changed files and no errors.

### Task 4: Verify

- [ ] **Step 1:** Run the full unit suite (`dotnet run -- -noLogo` in the unit project). Expected: 0 failed.
- [ ] **Step 2:** Run the integration project the same way. Expected: 0 failed, in particular `MediaDownloadFlowTest` (ETag is still the stored hash), `MediaUploadFlowTest` (a duplicate upload is still refused) and `FrontendDownloadFlowTest` (end-to-end download by size).
- [ ] **Step 3:** Tell the user: the API server needs a rebuild and restart; the app's local database and the downloaded files can be deleted if anything looks stale; nothing has been committed.
