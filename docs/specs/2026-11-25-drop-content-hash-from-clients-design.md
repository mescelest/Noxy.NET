# Drop the content hash from the client contract

## Intent

A media file has an ID, and a device stores the file under it. The bytes behind an ID never change, so a device that holds a file with that ID has the file. The hash the server sends and the device checks is then only a guard against a damaged download. The user wants the hash to stay inside the server, for upload de-duplication, and out of the contract between server and client. It also removes the need to keep a local copy's bytes equal to a server hash, which the local file tags design ([2026-11-25-local-file-tags-design.md](2026-11-25-local-file-tags-design.md)) would otherwise have to work around.

## Goals

- `ResponseMediaFile` no longer has `ContentHash`.
- A device decides that a file is held by its ID and its state, not by comparing hashes.
- A finished download is checked against `SizeBytes` instead of a hash.
- The server keeps `ContentHash` on `EntityMediaFile` and uses it to refuse duplicate uploads, exactly as today.

## Non-goals

- No replace-the-audio feature. A stored file's bytes still never change; a future change of bytes would need a new ID or a version number added then.
- No audio-only hash.
- The server's de-duplication, the upload job's hash and the database column are not changed.

## Anti-goals

- A partial download that is committed as a complete file.
- A resume that mixes bytes of two different files.

## Constraints

- `GET /Media/{id}/File` uses the hash as its ETag so a device can resume against the same bytes. An ETag is opaque to the client, which only stores and returns it (`If-Range`), so the server may keep using the hash as the ETag with nothing leaking into `ResponseMediaFile`. (Your call; an ID-based ETag would do as well.)
- The server's response to a ranged request tells the device when it got the whole file instead of a part, and `DownloadService` already discards the held part then.
- A tag write on a device changes the local file's size after the download check, which is fine: the size is checked once, before the file is committed.

## Approaches

- **A. Hash stays server side only; the device checks the size (chosen).** Small change on the client: `DownloadService`, `FileReconciler`, `FileState`, `SyncService.Diff`. Cost: a silently corrupted download of the right length would be kept.
- **B. Keep the hash in the response as it is.** No change. Rejected, because the user wants the contract free of it and the tags design is simpler without it.
- **C. Drop the hash everywhere, including de-duplication.** Rejected: the server needs something to recognise a file uploaded twice.

## Design

- Remove `ContentHash` from `ResponseMediaFile` and from the mapping that builds it. Keep it on the entity, the table and the upload job.
- `DownloadService`: after the bytes are appended, compare the partial file's length with `media.SizeBytes`. A shorter file stays partial and is resumed or retried; a longer or otherwise wrong one is discarded and the ETag forgotten, the same as a hash mismatch is today. Commit sets the state to on device without a hash.
- `FileState` loses `VerifiedHash`. A file is held when its state is `OnDevice` and `IFileStore.GetPath` has it.
- `FileReconciler.IsVerified` loses the hash comparison and keeps the state check (rename it if the name no longer fits).
- `SyncService.Diff`: the "changed bytes" list goes, since no list entry can change its bytes. A library diff has added and removed files only. The wording in `ISyncService` is updated.
- `MediaDownload` keeps `ContentHash` for the ETag only.
- The device's local store of file states (and its schema) loses the hash column. The data can be deleted, so there is no migration.

## Left to the builder

- Whether an ID-based ETag is nicer than the hash as the ETag.
- Which tests change: the download and reconcile tests, the fakes and the frontend download flow integration test use `ContentHash` today.
- The exact rule for a download that is longer than `SizeBytes`.
