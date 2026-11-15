# Sync backend: design

Status: implemented (backend). Checked against the code on 2026-11-15: the renamed routes, the file ETag, the tagged download and its `download` rate limit (10 per 60 seconds) are all in place. The media file side of this is described in `2026-11-15-media-library-design.md`.

## Intent

Devices sync media files from the server to local storage and play them from there. Opening a playlist gives a device everything it needs to show and sync it. The synced file never changes after upload, so a tag edit never makes a device download a media file again; the new tags reach other devices through the library and the hub. When a user saves a media file as a file, they get a copy carrying the current tags.

## Goals

- Rename the playlist routes so no list endpoint ends in an "s": `Playlists` becomes `Playlist`, and `Entries` becomes `EntryList`.
- A playlist entry carries the media file's metadata (title, artist, album, album artist, subtitle, comment, genre, track, disc, year, duration, size, content hash, content type, media file modified time), so `GET /Playlist/{id}` alone is enough to display and sync a playlist.
- The stored file is never rewritten after upload. `ContentHash` and `SizeBytes` describe it for good, and a metadata edit changes only the database and `TimeModified`.
- `GET /Media/{id}/File` is the raw stored file, used for sync. It returns a strong ETag taken from `ContentHash` and honours `If-Range`, so a resumed download cannot join two versions of a file.
- A new `GET /Media/{id}/Download` returns a copy of the file with the media file's current metadata written into its tags, in its original format, named after the title with the stored extension.
- MediaFile deletes are already pushed on the hub (`MediaDeleted`, plus `PlaylistUpdated` for each affected playlist). Keep that and cover it with a test if it is not.

## Non-goals

- Compression of files for transfer (audio is already compressed).
- Cover art and artwork extraction.
- The frontend, the browser store and the sync engine (a separate design).
- Server-side search or sorting.

## Anti-goals

- A stale playlist: a 304 on `GET /Playlist/{id}` after a media file's metadata changed.
- A `ContentHash` that does not match the stored bytes.
- A saved file whose tags disagree with the database at the time of the download.
- A tag edit that makes devices download audio again.
- A resumed download that mixes bytes of an old and a new version of a file.

## Constraints

- Existing conventions: one class per file, no summary comment above a class, `List` suffix for lists, no `yield`, no magic strings, interfaces in `Interfaces`, `is null`.
- Writing tags to a copy goes through one interface (in `Interfaces`) so tests can fake it. Library choice: TagLibSharp, as the choice with no extra moving parts. This is my call.

## Approaches considered

**Where tags are written.** (A) tag a temporary copy at download time, leaving the stored file untouched; (B) rewrite the stored file when metadata is edited. Chosen: **A**, the user's call after weighing B: B would change the hash and size of the stored file on every edit, so every device would download the media file again although the library list and the `MediaUpdated` push already carry the new tags. The cost of A is a temporary copy per download.

**Playlist ETag with metadata.** (A) include each entry's media file modified time in the `GET /Playlist/{id}` ETag; (B) bump the playlist's modified time and push when a contained media file changes. Chosen: **A**, no extra writes or pushes. The `GET /Playlist` ETag is unchanged because it carries no media file data.

## Design

### Routes

| Before | After |
|---|---|
| `GET /Playlists` | `GET /Playlist` (the list) |
| `GET /Playlists/{id}` | `GET /Playlist/{id}` |
| `POST /Playlists` | `POST /Playlist` |
| `PATCH` and `DELETE /Playlists/{id}` | `PATCH` and `DELETE /Playlist/{id}` |
| `POST /Playlists/{id}/Entries` | `POST /Playlist/{id}/EntryList` |
| `POST /Playlists/{id}/Entries/Remove` | `POST /Playlist/{id}/EntryList/Remove` |
| `PUT /Playlists/{id}/Entries/{entryID}/Position` | `PUT /Playlist/{id}/Entry/{entryID}/Position` |

Request and response bodies keep their shape, except that each entry gains the metadata fields above. The route constants stay in the controller; tests and any client constants move with them.

### Metadata edit

`PATCH /Media/{id}` is unchanged: it changes the database and `TimeModified` and pushes `MediaUpdated`. The stored file is not touched.

### File endpoints

`GET /Media/{id}/File` sets the `ETag` header to the quoted `ContentHash` and keeps range processing, so `If-Range` and `If-None-Match` follow from that.

`GET /Media/{id}/Download` copies the stored file to a temporary file, writes the current metadata into its tags (title, artist, album, album artist, subtitle, comment, genre, track, disc, year), streams it with the title as the file name and the stored extension, and deletes the copy when the response ends. If the format cannot hold some tag, that tag is skipped and the download still succeeds. If the file cannot be parsed for tagging, the untagged copy is served and a warning is logged. It has its own rate-limit policy, `download`, per client address like `auth` and `upload`, with a low default of 10 requests per 60 seconds, configurable in `RateLimiting:Download`, because every request makes a copy. `GET /Media/{id}/File` (sync) has no policy. The download does not support range requests (unlike the sync file), because every request makes its own copy and the pieces of a resumed download could come from differently tagged copies. A file missing from storage is a 404 for both.

### Tests

Unit tests with fakes for the tag writer (the download writes the current metadata into a copy; the stored file and its hash are untouched; a tag the format cannot hold is skipped; the copy is removed afterwards), the playlist-detail ETag changing when a contained media file's metadata changes, and entries carrying metadata. End-to-end tests for the renamed routes, the file ETag and `If-Range`, the tagged download (read the tags back from the response), and the hub push on a media file delete.

## Left to the builder

How to stream the temporary copy and clean it up reliably, how tags map for each supported format, the tag writer's name and members, and the response class names for entries carrying metadata.
