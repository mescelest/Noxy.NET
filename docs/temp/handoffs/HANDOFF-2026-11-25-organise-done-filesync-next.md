# Handoff 2026-11-25: Organise page done, file sync next

## Next topic: file sync

The user wants to look at file sync again: getting edited metadata (artist, album, genre, title and so on) onto the media file itself. They said "we've been through it before". Ask what they have in mind before building.

What exists today (`docs/specs/2026-11-15-sync-backend-design.md`, "Where tags are written"): the server writes the current metadata into the tags of a **temporary copy at download time** (`GET /Media/{id}/Download`, `HandlerMediaFileWithTags`, `IMediaTagWritingService`), and never rewrites the stored file. That was the user's call after weighing "rewrite the stored file when metadata is edited" (their reasons are cut off in the spec text, so ask). Options to raise: an explicit "write metadata to the file" action, rewriting the stored file on edit, or something per device. The hash and size of the stored file, upload de-duplication and the downloaded copy on devices are what such a change touches.

## State

All of this session's work is **uncommitted**. New files were staged by name (`git add`); production code is for the user to review and commit. No integration tests were run, only the unit suite (3100 tests, all passing; it had a flaky race in the fakes `FakeDelay` and `FakeMediaHubService`, fixed with snapshots under their lock).

Nothing in the UI has been seen running. The user was trying the new things out at the end of the session.

## Needs the user's attention

- **Rebuild and restart the API server.** It needs `PUT /Album/{id}/Art` (album picture) and the change that reports an album a media file left (counts).
- The picture picker uses the platform dialog (`IPicturePicker`, `PicturePicker`) because the web view's file chooser crashed the app (WebView2, `EmbeddedBrowserWebView.dll`, from inside a modal dialog).
- Things not looked at yet: the left sidebar's "⋯" probably has the same faint hover as the one fixed in `MetadataItemList`; `QueueList` duplicates the row click and ctrl handling of `MediaFileTable`.

## What was built (Organise plan, now deleted; the spec has "How it ended up")

- `/organise` page with a table (`MediaFileTable` with `ColumnList`), a right-hand column (Artist/Album/Genre lists), filter badges, selection limit of 1000.
- Context menu entries "Set artist…", "Set album…", "Set genre…" (`MetadataSetDialog`, `MetadataSetModel`, tri-state checkboxes, Save sends only the difference through `MetadataActionService.Apply`).
- `MetadataItemDialog` creates and edits; the album variant has a picture (staged, sent with Save) and an artist checkbox list (`MetadataCheckList`).
- Rows have a "⋯" menu and a right-click menu: Edit, Merge into…, Merge others into this…, Delete. Merge uses the server's existing merge endpoints (`MetadataMergeDialog`, `MetadataMergeOthersDialog`, confirmations with "Source → Target").
- `RowSelectEnum` (`CheckboxOnly`, `Ctrl`, `Click`) on `MediaFileTable` and `QueueList`; `CtrlKeyWatcher` sets `data-ctrl` on the page while ctrl is held. A double-click with ctrl held does not play.
- Shared UI: `EmptyState`, `CardGrid`, `PageContent`, `Dialog` got `Height` (`DialogHeightEnum.Fixed`) and a `Medium` size, `InputControlCheckbox` got `IsMixed`, `TabPage` refreshes its `TabSet` when its title changes.
- Item counts: the app takes a pushed item with an **equal** modified time (counts change without it); `HandlerAlbumMediaFileListAdd` reports the album a file left.
- `IMetadataRecentService` (last assigned) was removed; `Assign` and `Remove` on the action service are gone, everything goes through `Apply`.
- `Components/` was sorted into folders (`ContextMenus`, `MediaFiles`, `Playlists`, `Organise`, `Upload`, `Settings`, `Layouts`, `Dialogs`, plus the old ones); each is a namespace imported in `_Imports.razor`. `NarrowWatcher.razor.js` is listed by path in the csproj.

## Rules learned in this session (also in memory)

- Blazor `::deep` needs an anchor element in the component's **own markup**; a child component's root does not carry the parent's scope attribute, so wrap it in a plain element with a class.
- A string parameter needs `@` to take a value (`PreviewUri="@_previewUri"`), or the component gets the literal text.
- No size literals in component CSS (the style test rejects `10px`); use the variables.
- Every variable the UI libraries read must be defined in the app's `variables.css` (also tested).
