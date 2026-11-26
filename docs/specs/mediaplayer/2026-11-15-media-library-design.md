# Media library and upload: design

Lines marked *(inferred)* are reasons read out of the code, so check them.

## Intent

A user's media files live on the server. They upload audio, edit its tags, delete it, and every device keeps the list of media files current and downloads the files it needs. The server never decodes or converts audio: it stores the file as uploaded, describes it, and serves it back. Uploading is accepted fast and finished in the background, so a slow check or a busy disk never holds a request open.

## Goals

- **Upload** one file per request as multipart form data with an optional `title` field (before the `file` field). The answer is `202` with a job ID; the result arrives later on the hub (`UploadSucceeded` or `UploadFailed`).
- **Only real audio in the allowed formats.** The extension must be in `Storage:AllowedExtensionList` (mp3, flac, m4a, ogg, opus, wav by default) and the first bytes must match that format. Anything else is a `415`.
- **Limits that protect the server and keep the library list small:** a maximum file size (`Storage:MaxSizeBytes`, 100 MB), a maximum number of media files per user (`MaxFilesPerUser`, 5000, which also bounds the list every device downloads), an optional byte quota per user (`MaxBytesPerUser`, 0 means none), a cap on waiting jobs per user (`Upload:MaxPendingPerUser`, 100), and one upload per user at a time.
- **A library list a device can diff cheaply:** `GET /Media/Library` returns every media file of the user with a strong ETag; a matching `If-None-Match` gets a `304` with no body.
- **Tag editing** with `PATCH /Media/{id}`: change only the fields sent; the stored file is never rewritten.
- **Delete** a media file with its stored file, and take it out of every playlist in the same commit.
- **Sync download** `GET /Media/{id}/File`: the raw stored file, strong ETag, range requests, so a device can resume a download against the same bytes only. The ETag is the `ContentHash`; the hash is not part of `ResponseMediaFile`, so a device sees the ETag only as an opaque value that it stores and sends back in `If-Range`.
- **Save as file** `GET /Media/{id}/Download`: a copy carrying the current tags, named after the title (details in `2026-11-15-sync-backend-design.md`).
- **Live changes:** after a change is saved the user's connected devices are told (`MediaUpdated`, `MediaDeleted`, `UploadSucceeded`, `UploadFailed`).

## Non-goals

- Converting, transcoding or decoding audio, extracting tags from the uploaded file on upload, cover art. *(inferred: the title defaults to the file name and the other tags start empty.)*
- Several files in one request, resumable uploads, upload progress events.
- Server-side search and sorting. `GET /Media` (paged, newest first) exists but the app does not use it; the library endpoint is the one devices use.
- Sharing media files between users, de-duplicating identical uploads across users.

## Anti-goals (a failure even if it technically works)

- A library entry without its file, or a stored file no entry points to, left behind by a crash, a failed commit or a cancelled request.
- A media file visible to another user: another user's media file is always a `404`, and the same for its file.
- A request body buffered whole in memory or a file larger than the limits reaching the disk (staging stops at the first byte over, and at the byte quota).
- A stored `ContentHash` or `SizeBytes` that does not describe the stored bytes.
- A tag edit that changes the stored file, so that every device downloads it again.
- A failed push to the hub failing an upload or an edit.

## Constraints

- Repository conventions (one class per file, `List` suffix, no magic strings, interfaces in `Interfaces`, and so on) and the command, query and notification shape in `2026-11-15-backend-cqrs-design.md`.
- A user can have at most 5000 media files, so the library response stays a single list (no paging in the app).
- Upload and download are rate limited per client address (`RateLimiting:Upload` 30 per 60 s, `RateLimiting:Download` 10 per 60 s). The sync download is not limited.

## Approaches considered

**Where the upload is finished.** (A) do everything in the request; (B) stage the file and a job row in the request, and let a background worker validate room, move the file into the library and create the entry. Chosen: **B** *(inferred)*. The request only checks the header, stages the bytes within the limits and saves a job; the worker is the authoritative check, so two racing uploads cannot exceed the library limits.

**Crash safety.** (A) a single step with cleanup on failure; (B) a job row that survives the crash plus a recovery pass at start-up. Chosen: **B** *(inferred)*. A job stays `Processing` if the process stops mid-way and is queued again after the restart; staged files with no job are deleted at start-up. Moving a file into the library is safe to repeat.

**Order of removal on delete.** The database row goes first, then the file. A failure in between leaves an orphaned file (harmless, and logged), never an entry that points at nothing.

**ETag of the library.** Computed from each media file's ID and modified time. The header version is computed from lightweight stamps first, so a `304` does not load the whole list; the returned version is computed from the list that is sent, so the ETag and the body can never disagree.

## Design

### Data

- **MediaFile**: ID, owner, stored file name (`FilePath`, unique, with the extension), `ContentType`, `SizeBytes`, `ContentHash` (SHA-256 of the stored bytes), `TimeModified`, and one **MediaFileMetadata** (title, artist, album, album artist, subtitle, comment, genre, track number, disc number, year, duration).
- **MediaUploadJob**: ID, owner, staged file name, title, content type, size, hash, and a status (pending or processing). A job exists from the accepted request until its result is final.
- Stored files are named by a generated unique name, never by anything the client sent. The client's file name is only used for the extension and the default title.

### Upload

1. The handler takes the user's upload lease (one at a time per user; a second is `429`). The lease is released on every exit.
2. It reads the first bytes, validates extension and header (`415` if either fails), and checks the advisory limits: library full (`409`), too many waiting jobs (`429`).
3. It stages the stream within the remaining quota and the size limit (`413` over the file limit, `409` over the quota), then saves the job. If saving the job fails the staged file is deleted.
4. It wakes the worker and answers `202` with the job ID. The title is the sent `title` or, if left out or blank, the file name without its extension.
5. The worker claims the next pending job, re-checks the limits, moves the staged file into the library, creates the media file and removes the job in one commit, and publishes `NotificationUploadSucceeded` (with the media file) after the commit. A rejected or failed job is removed and its files deleted, and `NotificationUploadFailed` carries a reason (`LibraryFull`, `FileMissing`, `Unknown`). If the job cannot even be removed it stays queued and nothing is reported yet.
6. The worker also wakes by itself every `Upload:PollSeconds` (30) in case a wake-up was missed.

### Endpoints (all need a signed-in user)

- `POST /Media`: upload as above.
- `GET /Media`: paged list, `skip` and `take`, newest first.
- `GET /Media/Library`: the whole list; `ETag` and `Cache-Control: private, no-cache`; `304` on a match.
- `PATCH /Media/{id}`: any of `title`, `artist`, `album`, `albumArtist`, `subtitle`, `comment`, `genre`, `trackNumber`, `discNumber`, `year`. A field left out is untouched; a text field sent as `null` or blank clears the tag (a tag never holds an empty string); a number sent as `null` clears it; the title cannot be blank. Text values are trimmed. An edit that changes nothing does not bump `TimeModified`, so it cannot change the library ETag or push anything.
- `DELETE /Media/{id}`: `204`.
- `GET /Media/{id}/File` and `GET /Media/{id}/Download`: as in the goals. A file missing from storage is a `404`.

### Behaviour

- Every change that alters a media file sets its `TimeModified` from the injectable `TimeProvider` in the same commit as the change. That is what keeps the library ETag correct.
- **Delete:** in one commit the media file's entries leave every playlist, those playlists are renumbered and their modified time bumped, and the row is removed. The stored file is then deleted (a failure is logged, not thrown). After that the handler publishes `NotificationMediaDeleted`, then one `NotificationPlaylistUpdated` per affected playlist.
- **Push:** notifications are handled in the API project and sent to every connection of that user over the hub. A failed push is logged and never changes a response (see `2026-11-15-backend-cqrs-design.md`).
- **Cleanup:** an expired refresh-token cleanup runs in the background (see `2026-11-15-authentication-design.md`); it does not touch media.

### Tests

Unit tests with the fakes for each handler (limits, header validation, one upload at a time, lease release, staging and cleanup on failure, tag edit semantics, no-op edits, delete cascade and notification order), for the upload processor (claim, recovery, failure reasons), and for the validator. Integration tests against SQLite and end to end through HTTP and the real hub (upload to result, library `304`, edit, delete with playlists, ranges and `If-Range`, rate limits, `413`, `415`, ownership).

## Left to the builder

The staging layout on disk, how the worker is scheduled, and the repository method names.
