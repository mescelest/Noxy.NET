# File location and readable file names Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** The user picks the folder this device keeps its media files in (Windows), and held files get readable `Artist - Title.ext` names.

**Architecture:** `DiskFileStore` keeps an index file (`fileindex.json` in the app data folder) of media file ID to file name plus the files folder, so names can be readable and other files in the folder are never ours. A new `IFileLocationService` releases the player's file and moves the files with progress. Settings gets a field, a folder picker (App/Windows) and a confirm-then-progress dialog.

**Tech Stack:** .NET 10, MAUI Blazor (Windows), xUnit v3 (`dotnet run` in the test project, filter with `-class <full name>`), Fluxor, existing `ConfirmActionDialog`.

**Spec:** `docs/specs/mediaplayer/2026-11-26-file-location-design.md`

## Global Constraints

- Never commit (project memory): where a plan would commit, `git add -N` the new files instead.
- One class per file, no magic strings, interfaces in `Interfaces`, enum names end in `Enum`, texts in `AppTexts` (App) or `PresentationTexts` (Presentation), no platform types in Presentation or Storage.
- `IFileStore` stays addressed by media file ID; only `Commit` gains `string baseName`.
- Files are `Artist - Title.<ext>`, just `Title` without an artist; ` (2)`, ` (3)` on a clash (case-insensitive); name capped at 120 characters; an empty result becomes the media file ID.
- Only indexed files are ours: never list, count, overwrite or delete anything else in the folder.
- `partial` folder and album art stay in the app data folder. The control is Windows-only. No migration of old `<guid>.ext` files.

## Review Focus

- A foreign file already named `Artist - Title.mp3` in the folder: never overwritten, the new file gets ` (2)` (Task 2).
- Two media files with identical artist and title: both held, distinct names (Task 2).
- A title with `/ \ : * ? " < > |`, trailing dots, `CON`, or 300 characters: a valid, short name (Task 1).
- The app closed during a move: before the index write the old folder is intact, copies removed on the next failure/cancel path (Task 3).
- A held file open in the player during the move: the move still deletes it and the player loads it again at the same position, paused (Task 4).

---

### Task 1: The file name builder

**Files:**
- Create: `Noxy.NET.MediaPlayer/Noxy.NET.MediaPlayer.Presentation/Extensions/FileNameBuilder.cs` (static class, namespace `Noxy.NET.MediaPlayer.Presentation.Extensions`)
- Test: `Noxy.NET.Test/Noxy.NET.Test.UnitTest/MediaPlayer/Presentation/FileNameBuilderTest.cs`

**Interfaces:**
- Produces: `public static string FileNameBuilder.Build(Guid mediaFileID, string title, string? artist)` returning a base name with no extension.

- [ ] **Step 1: Write the failing tests.** `Build_WithArtist_IsArtistDashTitle` ("A", "T" → `A - T`); `Build_WithoutArtist_IsTheTitle` (null and whitespace artist → `T`); `Build_ReplacesInvalidCharacters` (`a/b:c*d?"e<f>g|h\i` has none of `Path.GetInvalidFileNameChars()` in its result); `Build_TrimsTrailingDotsAndSpaces` ("T. . " → `T`); `Build_AvoidsReservedDeviceNames` (title "CON", no artist → not `CON`, e.g. `CON_`); `Build_CapsTheLength` (300-char title → length ≤ 120); `Build_WithNothingUsable_IsTheMediaFileID` (title "...", no artist → the GUID string).
- [ ] **Step 2: Run:** `cd Noxy.NET.Test/Noxy.NET.Test.UnitTest && dotnet run -- -class Noxy.NET.Test.MediaPlayer.Presentation.FileNameBuilderTest`. Expected: build error (type missing).
- [ ] **Step 3: Implement `FileNameBuilder.Build`** per the signature; replace invalid characters with `_`, trim, cap at 120 then trim again, suffix `_` for reserved names (`CON PRN AUX NUL COM1-9 LPT1-9`, compared without extension and case).
- [ ] **Step 4: Run the same command.** Expected: all pass.
- [ ] **Step 5:** `git add -N` the two new files.

### Task 2: The index, readable commit and rename in `DiskFileStore`

**Files:**
- Modify: `Noxy.NET.MediaPlayer.Presentation/Interfaces/Stores/IFileStore.cs`, `Noxy.NET.MediaPlayer.Storage/Stores/DiskFileStore.cs`, `Noxy.NET.MediaPlayer.Storage/StorageConstants.cs` (`FileIndexFileName = "fileindex.json"`), `Noxy.NET.MediaPlayer.Storage/ServiceCollectionExtensions.cs` (unchanged signature)
- Create: `Noxy.NET.MediaPlayer.Storage/Stores/FileIndex.cs` (the persisted record: `string? Folder`, `Dictionary<Guid, string> NameMap`; load/save with a temp file that replaces the old one)
- Modify callers: `Noxy.NET.MediaPlayer.Presentation/Services/DownloadService.cs:354`, `Noxy.NET.Test/Noxy.NET.Test.UnitTest/MediaPlayer/Fakes/FakeFileStore.cs`
- Test: `Noxy.NET.Test/Noxy.NET.Test.IntegrationTest/MediaPlayer/Presentation/DiskFileStoreTest.cs` (update existing tests to the new `Commit` signature and add the ones below)

**Interfaces:**
- Consumes: `FileNameBuilder.Build` (Task 1).
- Produces: `IFileStore.Commit(Guid mediaFileID, string contentType, string baseName)`; `Task IFileStore.Rename(Guid mediaFileID, string baseName)` (InvalidOperationException when not held); `string IFileStore.Folder { get; }`. All other methods keep their signatures but resolve paths through the index.

- [ ] **Step 1: Write the failing tests** in `DiskFileStoreTest` (real temp folder): `Commit_UsesTheReadableName` (file at `<folder>/Artist - Title.mp3`, `GetPath` returns it); `Commit_OfAClash_AddsANumber` (two IDs, same base name → ` (2)`); `Commit_NeverOverwritesAForeignFile` (pre-create `Artist - Title.mp3` not in the index → new file is ` (2)`, foreign bytes unchanged); `ForeignFiles_AreNotListedCountedOrDeleted` (`GetHeldIDList`, `GetEstimate`, `Delete`); `AnExternalDelete_DropsTheEntry` (delete the file by hand → `GetPath` null, not in `GetHeldIDList`); `Rename_ChangesTheFileAndTheLookup` and `Rename_ToTheSameName_IsANoOp`; `TheIndex_SurvivesANewInstance`; `Commit_OfTheSameIDWithAnotherContentType_ReplacesTheFile`; `Rewrite_StillWorksOnAReadableName`.
- [ ] **Step 2: Run:** `cd Noxy.NET.Test/Noxy.NET.Test.IntegrationTest && dotnet run -- -class Noxy.NET.Test.MediaPlayer.Presentation.DiskFileStoreTest`. Expected: build errors, then failures.
- [ ] **Step 3: Implement.** `DiskFileStore` loads `FileIndex` once (index lives in `baseFolder`; the files folder is `index.Folder ?? <baseFolder>/files`), keeps it behind one `SemaphoreSlim`/lock shared by every mutating call, saves after each change. A name is free when no indexed entry and no file in the folder has it (`StringComparison.OrdinalIgnoreCase`); `Commit` and `Rename` pick `base`, `base (2)`, ... Listings read the index and drop entries whose file is gone. Update the interface summary comments. `DownloadService` builds `FileNameBuilder.Build(media.ID, media.Title, media.Artist)` and passes it; `FakeFileStore` follows the interface.
- [ ] **Step 4: Run the integration class, then the unit suite** (`cd ../Noxy.NET.Test.UnitTest && dotnet run`). Expected: all pass.
- [ ] **Step 5:** `git add -N` new files.

### Task 3: Moving the files (`DiskFileStore.Move`)

**Files:**
- Modify: `IFileStore.cs`, `DiskFileStore.cs`, `FakeFileStore.cs`
- Create: `Noxy.NET.MediaPlayer.Presentation/Models/FileMoveProgress.cs` (`record FileMoveProgress(int Done, int Total)`), `Noxy.NET.MediaPlayer.Presentation/Enums/FileMoveResultEnum.cs` (`Moved, SameFolder, NotWritable, WouldOverwrite, NotEnoughSpace, Cancelled`)
- Test: `DiskFileStoreTest.cs`

**Interfaces:**
- Consumes: the index from Task 2.
- Produces: `Task<FileMoveResultEnum> IFileStore.Move(string newFolder, IProgress<FileMoveProgress>? progress, CancellationToken token)`; `string IFileStore.DefaultFolder { get; }` (the app data `files` folder), so a reset is a `Move` to it.

- [ ] **Step 1: Write the failing tests:** `Move_MovesEveryFileAndTheIndex` (new folder holds all files, old folder none of ours, `Folder` is the new one, a new instance agrees); `Move_ReportsProgress` (Done 1..Total); `Move_ToTheSameFolder_ReportsSameFolder`; `Move_WhenAFileWouldBeOverwritten_IsRefusedAndNothingMoves` (target has a file with the name of a held file); `Move_ToANotWritableFolder_IsRefused` (a path that cannot be created); `Move_Cancelled_BeforeTheIndexWrite_LeavesTheOldFolderAndRemovesCopies`; `Move_WhenAnOldFileCannotBeDeleted_StillSucceeds` (hold an old file open with `FileShare.None`); `Commit_WaitsWhileAMoveRuns` (gate the move with a progress callback that blocks, start a `Commit`, assert it is not complete, release, assert it completes); a free-space refusal is tested through an internal seam: `DiskFileStore` takes an optional `Func<string, long>` free-space reader in its constructor (default `DriveInfo`) and the test returns 0.
- [ ] **Step 2: Run** the class. Expected: failures.
- [ ] **Step 3: Implement `Move`** under the store's lock: refuse in the order SameFolder, NotWritable (create the folder and a probe file), WouldOverwrite (a held name exists in the target), NotEnoughSpace (sum of indexed lengths > free space of the target drive); copy each file, report progress, check the token between files; on cancel or failure delete the copies and return/throw without touching the index; then save the index with the new folder, then delete the old files, ignoring `IOException` on delete.
- [ ] **Step 4: Run** the class and the unit suite. Expected: pass.
- [ ] **Step 5:** `git add -N` new files.

### Task 4: `IFileLocationService` and releasing the player's file

**Files:**
- Create: `Presentation/Interfaces/Services/IFileLocationService.cs`, `Presentation/Services/FileLocationService.cs`, `Noxy.NET.Test.UnitTest/MediaPlayer/Fakes/FakeFileLocationService.cs`
- Modify: `Presentation/Interfaces/Services/IPlayerService.cs`, `Presentation/Services/PlayerService.cs`, `Fakes/FakePlayerService.cs`, `Presentation/ServiceCollectionExtensions.cs`
- Test: `Noxy.NET.Test.UnitTest/MediaPlayer/Presentation/FileLocationServiceTest.cs`

**Interfaces:**
- Consumes: `IFileStore.Folder/DefaultFolder/Move` (Task 3).
- Produces: `event Action? Changed`; `string Folder { get; }`; `bool IsDefault { get; }`; `Task<FileMoveResultEnum> Move(string folder, IProgress<FileMoveProgress>? progress, CancellationToken token)`; `Task<FileMoveResultEnum> Reset(IProgress<FileMoveProgress>? progress, CancellationToken token)` (moves to `DefaultFolder`). On `IPlayerService`: `Task Unload()` (pauses and lets go of the file, remembering the position; no-op when nothing is loaded) and `Task Reload()` (loads the same item at the remembered position, paused; no-op after a `Clear` or when nothing was unloaded).

- [ ] **Step 1: Write the failing tests** with the fakes: `Move_UnloadsThePlayer_MovesTheFiles_ThenReloads` (call order recorded: `Unload`, `Move`, `Reload`); `Move_ThatIsRefused_StillReloadsThePlayer`; `Move_ThatThrows_StillReloadsThePlayer_AndRethrows`; `Move_RaisesChangedOnlyWhenMoved`; `Reset_MovesToTheDefaultFolder`. In `PlayerServiceTest` (existing file): `Unload_ThenReload_RestoresThePositionPaused` using the fake audio player (`Load`, `Seek` called; `Play` not).
- [ ] **Step 2: Run** `FileLocationServiceTest` and the `PlayerServiceTest` class. Expected: build errors/failures.
- [ ] **Step 3: Implement** both in `Presentation`; `FileLocationService` uses `try/finally` around the move so `Reload` always runs; register `IFileLocationService` as a singleton.
- [ ] **Step 4: Run** the two classes, then the whole unit suite. Expected: pass.
- [ ] **Step 5:** `git add -N` new files.

### Task 5: Names follow the tag write

**Files:**
- Modify: `Presentation/Services/FileTagService.cs` (in `Update`, after the successful `Rewrite`), `Fakes/FakeFileStore.cs` (records `Rename` calls)
- Test: the existing `FileTagServiceTest.cs`

**Interfaces:**
- Consumes: `IFileStore.Rename` (Task 2), `FileNameBuilder.Build` (Task 1).

- [ ] **Step 1: Write the failing tests:** `Update_RenamesTheFileToTheNewName` (media with a changed title → `Rename(id, "Artist - NewTitle")` recorded after the rewrite); `Update_WhenTheRenameFails_DoesNotFailTheTagWrite` (rename throws `IOException` → counted as succeeded, the file state still saved).
- [ ] **Step 2: Run** the `FileTagServiceTest` class. Expected: failures.
- [ ] **Step 3: Implement:** after `Rewrite` and before `SaveFileState`, call `Rename`; catch `IOException` and log the exception type only.
- [ ] **Step 4: Run** the class and the suite. Expected: pass.
- [ ] **Step 5:** `git add -N`.

### Task 6: The folder picker (Windows) and registration

**Files:**
- Create: `Presentation/Interfaces/Services/IFolderPicker.cs` (`bool IsSupported { get; }`, `Task<string?> Pick()` returning null when cancelled), `Noxy.NET.MediaPlayer.App/Services/FolderPicker.cs` (follow `FolderOpener.cs`: `#if WINDOWS` WinRT `Windows.Storage.Pickers.FolderPicker` initialised with the window handle through `WinRT.Interop.InitializeWithWindow`, `#else` unsupported), `Fakes/FakeFolderPicker.cs`
- Modify: `Noxy.NET.MediaPlayer.App/MauiProgram.cs` (register `IFolderPicker` next to `IFolderOpener`)

**Interfaces:**
- Produces: `IFolderPicker` as above.

- [ ] **Step 1:** Add the contract and the fake (no logic to test). 
- [ ] **Step 2: Implement `FolderPicker`**; the window handle comes from `Application.Current?.Windows[0].Handler?.PlatformView as MauiWinUIWindow`; return `folder.Path`.
- [ ] **Step 3: Build the app.** Run: `dotnet build Noxy.NET.MediaPlayer/Noxy.NET.MediaPlayer.App/Noxy.NET.MediaPlayer.App.csproj -f net10.0-windows10.0.19041.0`. Expected: Build succeeded (close a running debug session first, or the copy step fails on locked files).
- [ ] **Step 4:** `git add -N`.

### Task 7: Settings: the field, the confirm dialog and progress

**Files:**
- Create: `Noxy.NET.MediaPlayer.App/Components/Settings/FileLocationField.razor` (+ `.razor.css`), `Noxy.NET.MediaPlayer.App/Components/Dialogs/FileMoveDialog.razor` (+ `.razor.css`)
- Modify: `Noxy.NET.MediaPlayer.App/Components/Sections/SectionDevice.razor` (the field goes under the free-space figures; the section re-reads the figures on `IFileLocationService.Changed`), `Noxy.NET.MediaPlayer.App/Resources/AppTexts.resx` (+ designer) for the texts, `docs/UI-GUIDE.md` only if a pattern is introduced
- Test: none for markup; the behaviour is covered by Tasks 3 and 4.

**Interfaces:**
- Consumes: `IFileLocationService`, `IFolderPicker`, `FileMoveProgress`, `FileMoveResultEnum`, `IToastService.ShowError/ShowSuccess` (use the names the toast service has).

- [ ] **Step 1: `FileLocationField`:** shows the folder (`IFileLocationService.Folder`), a Change button (`IFolderPicker.Pick`, hidden when `IsSupported` is false), and a Reset button disabled while `IsDefault`. A picked folder opens `FileMoveDialog`.
- [ ] **Step 2: `FileMoveDialog`:** first state is a `ConfirmActionDialog` with the title, a detail text saying that all downloaded files will be moved to the new folder, that it might take a while and not to close the app, the folder path, and an accept button. On accept the dialog shows `Done of Total files` with a cancel button (cancels the token); it ends with a toast: success, or the text for the `FileMoveResultEnum` (not writable, would overwrite a file, not enough space, cancelled). Texts go in `AppTexts`; use "media files" wording consistent with the section.
- [ ] **Step 3: Build** (command in Task 6). Expected: Build succeeded.
- [ ] **Step 4: Run the whole unit and integration suites.** Expected: all pass.
- [ ] **Step 5:** `git add -N` new files.

### Task 8: Verification

- [ ] **Step 1:** Run both test projects in full (`dotnet run` in each). Expected: 0 failed.
- [ ] **Step 2:** Hand the user the by-hand list from the spec's Tests section to run on Windows (picker, dialog text, a move with many files and a cancel, playing during a move, a file renamed/deleted in Explorer, another app's music in the target folder).
