# File location and readable file names

## Intent

Some users want the media files this device keeps to be in their own music folder, where other players and the user can find them. Today the files sit in the app's data folder under names like `<guid>.mp3`, which are useless outside the app. The user picks the folder in Settings, and the files get readable names (`Artist - Title.mp3`). Changing the folder moves every held file and warns the user first that this takes a while.

## Goals

- A Settings control on the Device section shows the folder the media files are kept in and lets the user change it (Windows).
- Changing the folder asks for confirmation in a dialog that says all downloaded files will be moved to the new folder and that it might take a while. Then the files move, with visible progress.
- Held files are named `Artist - Title.<ext>` (just `Title` without an artist), made safe for Windows, with ` (2)`, ` (3)` added on a clash.
- The files of other apps in the same folder are never touched, listed, counted or deleted.
- A file that is renamed or deleted outside the app counts as missing and is downloaded again by the existing reconciler.

## Non-goals

- Android (scoped storage; the control is not shown there and the files stay in the app data folder).
- Migrating the existing `<guid>.ext` files (pre-production, no backfill): after this change they are not in the index, so they count as not downloaded and are fetched again under their new names. Deleting the old folder content is left to the user.
- Sub-folders (`Artist/Album/`), a name template setting, or finding a file the user renamed.
- Moving the `partial` folder or the album art cache: both stay in the app data folder.

## Anti-goals

- A file lost or half written because the app closed or failed during a move, or because the disk filled up.
- A move that deletes or overwrites a file that is not ours.
- A download committing into the folder while it moves, or the player holding a file the move needs to delete.
- Callers learning about names: they keep asking by media file ID.

## Constraints

- `IFileStore` is addressed by media file ID everywhere (`GetPath`, `Open`, `Delete`, `Commit`, `Rewrite`, the listings); that stays. Only `Commit` gains the name to give the file.
- The server holds the library: the files are a copy, so a lost file is recoverable, which is why an external rename is not worth defending against.
- `DiskFileStore` lives in the Storage project (no platform types). The folder picker is a platform thing and lives in the App project (`Platforms/Windows`), behind a contract in Presentation, like `IFolderOpener`.
- Repository conventions apply (one class per file, no magic strings, interfaces in `Interfaces`, enum names end in `Enum`, texts in `AppTexts` or `PresentationTexts`). Never commit.

## Approaches

- **A path index (chosen).** The store keeps a small file in the app data folder that maps media file ID to file name, together with the files folder. Names are free to be readable. A file the index does not list is not ours.
- **The ID in the file name** (`Artist - Title [3f9a2c1e].mp3`). No index, but every file carries a suffix, which defeats the point.
- **The ID in the file's tags.** Survives any rename but needs a scan of every file and a custom tag. Too heavy.

## Design

### The index and the folder (Storage)

- `DiskFileStore` keeps `fileindex.json` in the app data folder: the files folder path (the app data `files` folder by default) and the map of media file ID to file name. Writes go to a temporary file that replaces the old one, so the index is never half written. It is read once, and kept in memory behind a lock.
- `GetPath` and `Open` look the ID up in the index and check the file exists. `GetHeldIDList` and `GetEstimate` cover the indexed files that exist; an entry whose file is gone is dropped from the index when seen. A listing never enumerates the folder, so other files are invisible.
- `Commit(id, contentType, baseName)` moves the partial file into the folder under a free name built from `baseName` plus the extension of the content type, replaces an earlier file of the same ID, and records it. A name is free when no indexed file, and no file in the folder at all, has it (compared without regard to case): a file of another app is never overwritten.
- `Delete` removes the file and its entry. `Rewrite` works on the indexed path as it does now.
- `Rename(id, baseName)` gives a held file a new name by the same rule and updates the index; the same name is a no-op.
- `Folder` returns the current folder. `Move(newFolder, progress, token)` is described below.
- All of it runs under one lock, so a `Commit` waits while a `Move` runs.

### The name (Presentation)

- A small pure builder turns title and artist into a base name: `Artist - Title`, or `Title` with no artist; characters Windows refuses are replaced; trailing dots and spaces trimmed; reserved device names (`CON`, `NUL`, ...) avoided; the length capped (120 characters) so the path stays short enough. An empty result becomes the media file ID.
- `DownloadService` builds the name from the media file it commits (title and the first artist, from the metadata store) and passes it to `Commit`.
- Names follow the tags *(my call)*: `FileTagService.Update`, which already rewrites a file's tags from the library after an edit, also renames the file. With "write metadata to files" off the file name stays as downloaded, as its tags do.

### The move (Presentation and Storage)

- `IFileLocationService` (Presentation) offers `Folder`, `Task<bool> CanMoveTo(string folder)` and `Task Move(string folder, IProgress<FileMoveProgress> progress, CancellationToken token)`. It first releases the file the player holds (pauses it and lets go of the handle, loading it again at the same position afterwards without playing), then calls the store's `Move`. Downloads need nothing: their commit waits on the store's lock.
- The store's `Move`: refuse when the folder is not writable, is the current one, or already holds a file that would clash with one of ours; refuse when the files do not fit in the free space of the target drive; copy every indexed file to the new folder (progress after each file), then write the index with the new folder, then delete the old files. A failure or cancel before the index write removes the copies and leaves everything as it was; after it, the move is done and a leftover old file is only a stray. Moving in the same drive may use a rename for speed.
- The service raises `Changed` after a move so Settings redraws.

### Settings (App)

- A `FileLocationField` in `SectionDevice` shows the current folder, a change button that opens the Windows folder picker (`IFolderPicker`, contract in Presentation, WinRT `FolderPicker` implementation in `Platforms/Windows`; not registered on Android, where the control is not shown), and a reset-to-default action.
- Picking a folder opens a `ConfirmActionDialog` with the explanation text (all downloaded files are moved to the new folder; this might take a while; do not close the app) and the folder. On accept the dialog turns into a progress state (files done of total, a cancel button) and ends with a toast: success, or the reason (not writable, would overwrite, not enough space, cancelled).
- The stored and free figures in the Device section keep reading the store.

### Tests

- Storage (real temp folders): commit under a readable name, clash gets ` (2)` and ` (3)`, a foreign file with the wanted name is never overwritten, a foreign file is not listed, counted or deleted, an external delete drops the entry, a rename, the index survives a new instance, a replace of the same ID with another content type, the `Commit` blocked while a `Move` runs.
- Move: moves all files and the index, progress is reported, a target that clashes is refused with nothing moved, a cancel before the index write leaves the old folder intact and removes the copies, not enough space is refused, an old file that cannot be deleted does not fail the move.
- Name builder: invalid characters, reserved names, trailing dots, length cap, no artist, empty.
- Presentation: the location service releases and reloads the player around a move; `DownloadService` passes the built name; `FileTagService.Update` renames.
- By hand on Windows: the picker, the dialog text, a move with a long file list and a cancel, playing during a move, a file deleted and renamed in Explorer, another app's music in the target folder.

## Left to the builder

- How the player releases and reloads the current file (`IPlayerService`), and how `Move` shows progress in the dialog.
- The exact wording of the dialog and toasts (in `AppTexts`).
- Whether a move inside one drive uses a rename.
- Whether an old file that could not be deleted is retried at the next start.
