# Playlists: design

## Intent

A user can group their library media files into named, ordered playlists. Playlists live on the server and sync to every device the way the library does: a cheap start-up diff, plus a live push when something changes. A device lists the playlists first and loads a playlist's media files only when the user opens it.

## Goals

- Create, rename, delete and list playlists. A playlist has a name and an "allow duplicates" flag.
- Hold media files as ordered entries that reference library media files, with a stable order the user can change.
- A media file can appear more than once in a playlist unless the playlist turns that off.
- A device downloads only the playlist objects (no media files) to learn what exists and what changed, and fetches a playlist's entries on demand.
- Every change to a playlist or its entries reaches the user's other connected devices at once.
- Deleting a media file also removes it from every playlist and tells the devices.

## Non-goals

- Artwork for playlists (left out on purpose; it will be designed together with cover art from tags).
- Sharing playlists between users, collaboration, smart playlists, importing or exporting.
- Server-side sorting: the client can sort a playlist however it likes. With no sort applied it shows the stored order.
- A conflict check between two devices editing at once: last write wins.
- A per-playlist `If-Match` check.

## Anti-goals (a failure even if it technically works)

- Stored positions that are not exactly 0 to n−1 in the order shown, or that differ from what the client was told.
- An entry that points at a media file that no longer exists.
- A 304 (or an unchanged ETag) when a playlist or any of its entries changed.
- A playlist with the duplicates flag off that holds duplicates.
- A change request failing because a notification could not be sent.

## Constraints

- Follow the existing layering and conventions: API controller, a command or query and a handler per use case, repository on the unit of work, fakes in unit tests, EF in integration tests, `WebApplicationFactory` end to end. One class per file; no comment above a class; collections and list-returning methods use the `List` style (`EntryList`, `GetPlaylistList`); no `yield`; no magic strings (constants or `nameof`; hub events are members of `MediaHubEventEnum`); interfaces in an `Interfaces` folder; `is null`; no `Where(... == null)` in EF is the only exception.
- The `AddMediaFile` migration had not been applied anywhere, so the new tables were added by regenerating it (there is no separate playlist migration).
- The library cap is 5000 media files per user, so a playlist can never need to reference more media files than that.

## Approaches considered

**Reordering.** (A) move an entry to a position; (B) swap two entries; (C) replace the whole order. Chosen: **A**. It is what a drag and drop sends, it expresses a swap as two moves, and C can be added later without breaking anything.

**Position storage.** (A) plain integers renumbered by the server on every change; (B) gapped weights. Chosen: **A**: at the entry cap a change rewrites at most about a thousand rows in one transaction.

**What a device downloads.** (A) everything in one list; (B) a list of playlist objects plus a per-playlist fetch of the entries. Chosen: **B**, the user's call, so a device only loads entries for playlists it actually opens.

## Design

### Data

- **Playlist**: ID, owner, `Name` (1 to 255 characters, trimmed, not unique), `AllowDuplicates` (default true), `TimeCreated`, `TimeModified`.
- **PlaylistEntry**: ID, the playlist, the referenced media file, `Position`. Deleting a playlist deletes its entries. Deleting a media file deletes its entries (the delete-media handler does this explicitly so it can renumber and notify; the foreign key cascade is the safety net).
- **Invariant:** after every operation a playlist's entry positions are exactly 0 to n−1, in order, with no gaps and no repeats.
- **Limits** (`Playlist` config section, validated at start): `MaxPlaylistsPerUser` default 100, `MaxEntriesPerPlaylist` default 1000.

### Endpoints (all need a signed-in user; another user's playlist or media file is always a 404)

The paths are `/Playlist`, `/Playlist/{id}/EntryList`, `/Playlist/{id}/EntryList/Remove` and `/Playlist/{id}/Entry/{entryId}/Position`, and each entry in a response also carries its media file data (see `2026-11-15-sync-backend-design.md`).

- `GET /Playlists`: every playlist of the user, newest first, as playlist objects only: `id, name, allowDuplicates, entryCount, timeCreated, timeModified`. Strong ETag over each playlist's ID and modified time, `If-None-Match` gives a 304, gzip when asked (the same machinery as `GET /Media/Library`).
- `GET /Playlists/{id}`: the playlist object plus its entries, ordered by position, each as `id, mediaFileID, position` (now with the media file's data too). The ETag comes from the playlist's ID and modified time and, since entries carry media file data, from each entry's media file modified time; a matching `If-None-Match` gives a 304.
- `POST /Playlists` with `name` and optional `allowDuplicates`: 201 with the playlist object. At the cap: 409.
- `PATCH /Playlists/{id}` with any of `name`, `allowDuplicates` (left-out field unchanged, same presence handling and attribute validation as `PATCH /Media/{id}`): 200 with the playlist object. Turning `allowDuplicates` off while the playlist holds duplicate media files: 409 (the UI removes the duplicates first). A change that changes nothing does not bump the modified time or push.
- `DELETE /Playlists/{id}`: 204.
- `POST /Playlist/{id}/Played` (added later): sets the playlist's `TimeLastPlayed`, which the playlist object carries so the sidebar can list the most recently played first.
- `POST /Playlists/{id}/Entries` with `mediaFileIDList` (library media file IDs) and an optional `position` (default: the end): adds the media files in the order given, starting at that position. With duplicates off, media files already in the playlist, or repeated within the request, are skipped. Any unknown or other-user media file: 404 and nothing is added. Exceeding `MaxEntriesPerPlaylist`: 409. Returns 200 with the playlist and its entries, so the client sees the new positions.
- `POST /Playlists/{id}/Entries/Remove` with `entryIDList`: removes those entries and renumbers; an unknown entry: 404 and nothing is removed. Returns 200 with the playlist and its entries.
- `PUT /Playlists/{id}/Entries/{entryId}/Position` with `position` (0 to n−1, else 400): moves the entry, shifts the others and renumbers. Moving to its current position changes nothing. Returns 200 with the playlist and its entries.

### Behaviour

- Every change that alters a playlist or its entries sets the playlist's `TimeModified` (from the injectable `TimeProvider`) in the same commit as the change. This is what makes the ETags correct.
- **MediaFile delete.** In the same commit that removes the media file, remove its entries from every playlist, renumber those playlists, and bump their modified time. After the commit, push `PlaylistUpdated` for each affected playlist.
- **Push.** After the commit, send `PlaylistUpdated` (the playlist object, no entries) for any change to a playlist or its entries, and `PlaylistDeleted` (the ID) for a delete, to every connection of that user, through a notification published after the commit and handled by a SignalR hub handler (originally an `IPlaylistNotifier`; replaced by notifications in `2026-11-15-backend-cqrs-design.md`). A device that holds that playlist's entries compares the pushed modified time with its own and refetches `GET /Playlists/{id}` if it is newer. A failed notification is logged and never changes the response.
- `MediaHubEventEnum` gains `PlaylistUpdated` and `PlaylistDeleted`.

### Tests

Unit tests with the fakes (limits, ordering and renumbering, the duplicates flag, ownership, no-op changes, ETag changes, media file delete). Repository integration tests (atomic change of entries and positions, cascades). End-to-end tests for each endpoint, the 304s, gzip, push to every connection of the user and none to another user, and the 400, 404 and 409 cases.

## Left to the builder

How to renumber positions in SQLite (do not rely on a unique index across a multi-row update), the request and response class names, the repository method names, and the ETag hashing format (same as the library's).
