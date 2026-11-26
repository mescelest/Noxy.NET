# Local file tags

## Intent

The user shares the media files held on a device with other people, especially since the app can open the containing folder. The device's own copy of a media file should carry the current metadata, without making every device download the file again, and without the server's stored file or its `ContentHash` changing.

## Goals

- A device can write the current metadata of a media file into its own local copy.
- Writing tags never changes the server's `ContentHash`, never triggers a download and never changes what the server stores.
- The user chooses when it happens: by a menu action, by an "Update all files" action, by an account setting, or any of them.
- A device knows which of its files really differ from the metadata, and warns only for those.

## Non-goals

- The server's stored file is not rewritten. `GET /Media/{id}/File` and its ETag are unaffected. `GET /Media/{id}/Download` writes the tags into a temporary copy.
- No change to how `ContentHash` is computed. A hash over the audio only (TagLib's invariant range) is not used: it needs per-format parsing on every upload (ogg and opus have no stable byte range).
- No tag writing on files that are not on the device.

## Anti-goals

- A tag write that makes a device download a file, or that makes the file count as not held.
- A failed or locked write that leaves a half-written file, or marks the file up to date.
- A warning on a file whose tags already match the metadata.

## Constraints

- A device holds a file by its ID and state (the hash is not part of the client contract, see `2026-11-15-sync-backend-design.md`), so a local tag write has no hash to disturb. Nothing re-measures a held file after the download check.
- The tag writer and reader are in the Application project, which the Presentation project references (TagLibSharp comes with it). The device registers them in `AddPresentation`.
- A file that is being played can be locked on Windows.
- Account settings are declared in `SettingCatalog` and shown in `SettingList.razor`.

## Approaches

- **A. Each device writes tags into its own copy (chosen).** No hash change, no download. The local bytes differ from what the server stores, and a locked file has to be retried.
- **B. Write only when the user shares or opens the folder.** The same writer as A with no checking. Files in the folder stay stale until the user acts. It is the menu action of A on its own.
- **C. The server rewrites the stored file, with an audio-only hash.** Rejected: devices would still have to download to get the tags, and the hash parsing is risky.

## Design

### Writing

- The tag writer takes a value both sides have, `MediaTagValues`: title, artist names, album title, album artist names, genres, subtitle, comment, track, disc, year. The server's `HandlerMediaFileWithTags` builds it from the entity. The device builds it from the media file it holds: title, album, subtitle, comment, track, disc and year are plain values on `ResponseMediaFile`, and artists, album artists and genres are resolved from their IDs through the device's local lists, so a name is never cut out of joined text. A media file whose IDs the local lists do not have yet is skipped until they have synced. (The user's call: IDs, not text.)
- The device writes to a copy beside the held file and swaps it in only when the write succeeded (`IFileStore.Rewrite`), so a failure never leaves a half-written file.
- A write that fails, because the file is locked or the tagger cannot read it, leaves the file and its state as they were, is reported, and is retried the next time.

### Knowing what is out of date

- The device records, per file, the `TimeModified` of the media file the tags were last checked or written for (`FileState.TagTimeModified`).
- The warning means the file on disk really differs from the metadata. A file whose recorded time is missing or differs from the media file's `TimeModified` is **checked**: the device reads the file's tags (`IMediaTagReadingService`) and compares them with the tags it would write (title, artists, album, album artists, genres, subtitle, comment, track, disc, year; not the picture or duration). When they are equal the device records the current `TimeModified` and writes nothing. When they differ, or the tags cannot be read, the file is out of date. (The user's call: a downloaded file whose tags already match must not warn.)
- A file is out of date when it is held, its recorded time is not the current `TimeModified`, and a check at the current `TimeModified` found a difference. A file that has not been checked yet is not shown as out of date. The check runs at start, after every sync change and after every download state change, whatever the setting says. It is a read, never a write.
- A downloaded-again file has no recorded time and is checked again.
- The found differences are kept in memory only. A file whose check found a difference is not read again in the same session until its state or its media file's `TimeModified` changes.

### Triggers

- **Menu.** The context menu of media files (`LibraryContextMenu`, `QueueContextMenu`) has a submenu "Manage file":
  - **Update file metadata**: first. Writes the current metadata into the local copies of the selected media files that are held. One that is not held is skipped and reported in the result message. It works offline.
  - **Save file as**: second. Always offered. It saves the server's tagged copy (`GetTaggedFile`), so it works for media files that are not held too.
  - **Open containing folder**: third. Where the platform has a file manager (`CanOpenFolder`) and the media file is held.
  - The selected files, the whole-selection actions and the result messages follow the `ILibraryActionService` pattern.
- **Update all.** One action in the Library only (not in the queue or on a playlist's page), in the Library's Actions menu: "Update all files (N)" with the count of out-of-date files, hidden when there are none. The Actions button stays enabled while the count is above zero, even with nothing ticked, and then offers only this item. (The user's call: Library only.)
- **Setting.** An account boolean `UpdateFileMetadata` in `SettingCatalog`, default **off** (my call: a tag write can rewrite the whole file, which costs disk and battery). When on, the device checks its files and then writes the ones that differ, at start and whenever the library or a download changes.
- All of them use one service on the device, `IFileTagService`, so the setting only decides when it is called. Calls do not overlap, and a file problem never throws out of it.

### Showing it

- `FileStatusIcon` keeps the on-device icon for a held file that is out of date and adds a small warning badge in the warning colour (`--color-warning` and `--color-on-warning` in `variables.css`, no literal colours in the component CSS). The tooltip reads "On this device, metadata out of date".
- The out-of-date state is not a `FileSyncStateEnum` value. Many places test `== OnDevice`, and a new value would make an out-of-date file look like it is not held. The library state holds a set of the out-of-date IDs, and the icon takes a flag next to its state.
- The service owns the list of out-of-date IDs. The state bridge loads it into the library state with the file states, and whenever the service reports a change. It is also what "Update all files" counts. It does not read the library when nothing differs.
- The library table, the queue and a playlist all show the badge, since they pass the state to the same icon.

### Where it lives

- The writer, the reader, `MediaTagValues` and the matching rule are in Application, so the server and the device share them.
- `FileTagService` is in the Presentation project next to `DownloadService`. It uses `IFileStore.GetPath` for the held file and `IFileStore.Rewrite` to change it.
- The recorded time is kept with the file state on the device.

## Left to the builder

- The texts and icons of the submenu and its items, following the existing context menu.
- Whether the setting also needs a per-device override (not asked for).
