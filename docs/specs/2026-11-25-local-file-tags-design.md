# Local file tags

## Intent

The user shares the media files held on a device with other people, especially now that the app can open the containing folder. A file on the device carries the tags it was uploaded with, not the metadata the user edited since. They want the device's own copy to carry the current metadata, without making every device download the file again.

## Goals

- A device can write the current metadata of a media file into its own local copy.
- Writing tags never changes the server's `ContentHash`, never triggers a download and never changes what the server stores.
- The user chooses when it happens: by a button, by an account setting, or both.
- A device knows which of its files are out of date, so the button can offer to update exactly those.

## Non-goals

- The server's stored file is not rewritten. `GET /Media/{id}/File` and its ETag stay as they are. `GET /Media/{id}/Download` keeps writing tags into a temporary copy.
- No change to how `ContentHash` is computed. A hash over the audio only (TagLib's invariant range) was considered and rejected for now: it needs per-format parsing on every upload and every download check (ogg and opus have no stable byte range), and a wrong range would fail every download of that format. It stays possible later, because the data can be deleted.
- No tag writing on files that are not on the device.

## Anti-goals

- A tag write that makes a device download a file, or that makes the file count as not held.
- A failed or locked write that leaves a half-written file, or marks the file up to date.

## Constraints (from the code today)

- This is built after [2026-11-25-drop-content-hash-from-clients-design.md](2026-11-25-drop-content-hash-from-clients-design.md), which removes the hash from the client contract. A device then holds a file by its ID and state, and a local tag write has no hash to disturb. Until that is done, `FileState.VerifiedHash` and `FileReconciler.IsVerified` would have to stay as they are, and this design must not change them.
- Nothing re-hashes or re-measures a held file after the download check, and this design keeps it that way.
- `MediaTagWritingService.Write(path, EntityMediaFile)` is server side and takes the server entity. TagLibSharp is referenced by the Application project, which the Presentation project already references.
- A file that is being played can be locked on Windows.
- Account settings are declared in `SettingCatalog` and shown in `SettingList.razor`.

## Approaches

- **A. Each device writes tags into its own copy (chosen).** No hash change, no download. The cost is that the local bytes differ from the server hash, and a locked file has to be retried.
- **B. Write only when the user shares or opens the folder.** The same writer as A, with no staleness tracking. Files in the folder stay stale until the user acts. It is the button of A on its own.
- **C. The server rewrites the stored file, with an audio-only hash.** Rejected: devices would still have to download to get the tags, and the hash parsing is risky.

## Design

### Writing

- The tag writer takes the metadata it needs from a value both sides have, instead of the server entity: title, artist names, album title, album artist names, genres, subtitle, comment, track, disc, year. The server's `HandlerMediaFileWithTags` and the device both use it. `ResponseMediaFile` already carries these as text.
- The device writes to a copy beside the held file and swaps it in only when the write succeeded, so a failure never leaves a half-written file. (Your call: how the swap is done per platform is for the builder.)
- A write that fails, because the file is locked or the tagger cannot read it, leaves the file and its state as they were, and is retried the next time.

### Knowing what is out of date

- The device records, per file, the `TimeModified` of the media file the tags were last written for. This is a new field next to the file state and not part of `VerifiedHash`.
- A file is out of date when it is held, and either no tags were ever written or the recorded time differs from the media file's current `TimeModified`.
- A file that was downloaded again (new bytes, new hash) has no recorded time, so it counts as out of date again.

### Triggers

- **Menu.** The context menu of songs (`LibraryContextMenu`, `QueueContextMenu`) gets a submenu "Manage file" in place of the single "Open containing folder" / "Save as file" item. The menu component already supports submenus (`ContextMenuItemKindEnum`, `ContextMenuNavigator`). Its items:
  - **Update file metadata**: first. Writes the current metadata into the local copies of the selected songs that are held on the device. A song that is not held is skipped and reported in the result message. The text is the user's wording; it says what happens without naming tags.
  - **Save file as**: second. Now always offered, also where there is a file manager (today it replaces "Open containing folder" and is shown only where there is none). It saves the server's tagged copy (`GetTaggedFile`), so it works for songs that are not held too.
  - **Open containing folder**: third. As today, where the platform has a file manager (`CanOpenFolder`) and the song is held.
  - The submenu is shown when it has at least one item. The selected songs, the whole-selection actions and the result messages follow the existing `ILibraryActionService` pattern.
- **Update all.** One action in the Library only (not in the queue or on a playlist's page), in the library's Actions menu. Its text is short, "Update all files" with the count of out-of-date files after it (my proposal, the user said the long wording was too long), and it is hidden when there are none. It writes the files that are held and out of date. (The user's call: Library only.)
- **Setting.** An account boolean in `SettingCatalog`, default **off** (my call: a tag write can rewrite the whole file, which costs disk and battery on every edit). When on, a device writes the tags of a held file when it learns the file changed (`MediaUpdated` push or library sync), and also catches up on the out-of-date files at start.
- Both use one service on the device, so the setting only decides when it is called.

### Showing it

- `FileStatusIcon` (the file download icon) keeps the on-device icon for a held file that is out of date and adds a small warning badge to it, in the warning colour (`--color-warning` and `--color-on-warning` in `variables.css`, no literal colours in the component CSS). The tooltip says so, for example "On device, metadata out of date". (The user's call: a small warning badge.)
- The out-of-date state is not a new `FileSyncStateEnum` value. Many places test `== OnDevice` (the context menu, the device section count), and a new value would make an out-of-date file look like it is not held. Instead the library state holds a set of the IDs that are out of date, and the icon gets a flag next to its state. (My call.)
- The set is recomputed when a file's state, its written time or the song's `TimeModified` changes, and is also what "Update all files" counts.
- Where the icon is used (the library table, the queue, a playlist) all show it, since they all pass the state to the same icon.

### Where it lives

- The writer and its model move to where the server and the device can both use them.
- The device service is in the Presentation project next to `DownloadService`. It uses `IFileStore.GetPath` for the held file.
- The record of the written time is kept with the file state on the device.

## Left to the builder

- The per-platform way to swap the written copy in (Windows and Android).
- Whether the setting also needs a per-device override (not asked for).
- The texts and icons of the submenu and its items, following the existing context menu, and where exactly "Update all files" sits in the Library's Actions menu.
- The tests: the writer on the shared model, the out-of-date rule, that a tag write leaves the file held and not downloaded again, and the retry after a locked file.
