# Metadata entities: artists, albums and genres, filled in on upload

Status: implemented (backend only), checked against the code on 2026-11-15; see the plan `docs/superpowers/plans/2026-11-15-metadata-entities.md` for the build and the rulings it records. It takes the data layer out of `2026-11-15-library-organizer-design.md` (which stays the description of the later panel and drag-and-drop UI). Where the build differs from the draft the text says so. Lines marked *(my call)* are decisions made without asking.

## Intent

A media file's artist, album, album artist and genre are free text today, and an upload fills in nothing but the title (taken from the file name), so every file needs hand-editing and the same artist ends up spelled several ways. The user wants artists, albums and genres to be real items in the library, and wants an upload to read what the file already says and attach it to the right items, finding the existing item or creating it. Uploading an album should create the album once and link all its songs to it. Where the automatic result is not right, the user must be able to fix it by hand: create items, rename them, merge them, and split an item such as "A & B" into "A" and "B".

## Goals

- **Three kinds of item, each owned by one user:** artists, albums and genres. (Free-form labels such as "chill" or "violin-heavy" that the user attaches themselves are left for later; see the non-goals.)
- **Links:** a file has several artists, several genres and at most one album; an album has several artists. Track number, disc number, year, comment, subtitle and duration stay plain values on the file.
- **Upload fills it in.** When the worker finishes an upload it reads the file's embedded tags and properties (title, artists, album, album artists, genres, track, disc, year, comment, duration), finds or creates the items and links them in the same commit that adds the file. This also fills in `duration`, which nothing sets today.
- **Title order:** the title the user sent wins, then the embedded title, then the file name without its extension.
- **Splitting credits, only where it is unambiguous.** A credit is split on the tag's own list of values (a format that stores several), on `;`, and on `feat.`, `ft.` and `featuring` (case-insensitive, with or without brackets). It is never split on `&`, `,` or `/`, because names such as "Earth, Wind & Fire", "AC/DC" and "Tyler, The Creator" contain them. Genres use the same rule.
- **Matching:** items match by a normalised name (trimmed, inner whitespace collapsed, case-folded, Unicode-normalised), so "Radiohead" and "radiohead " are one artist. The first spelling seen is the display name and can be edited. An album matches by its normalised title together with its set of album artists; tracks that carry no album-artist tag and the same title are one album.
- **Manual control:** create, rename and delete an item; merge one item into another; split an artist or genre into several; assign or remove items on many files at once; set an album's artists.
- **Existing library migrates:** the text already on files becomes items by the same rules, with no tag lost.
- **The response keeps the text and gains the IDs.** `artist`, `album`, `albumArtist` and `genre` stay as text derived from the item names (several are joined with "; "), so the app and its search keep working, and new fields carry the item IDs.
- **Sync keeps working:** every change that alters a file's response also changes that file's modified time, so the library ETag is right; each kind has its own list with an ETag and a push, like playlists.
- **A saved or downloaded file carries the item names as its tags**, written at download time as today.

## Non-goals

- The panel, folder view and drag-and-drop (the organiser design, later), and any frontend work.
- Online tag lookup, cover art, sub-genres or any hierarchy, sharing items between users.
- Rewriting the stored audio file: items live in the database.
- Turning other fields (year, comment, track number) into items.
- User-made free-form labels on files ("chill", "violin-heavy"), the same shape as genres but created only by the user. Left out for now and easy to add later.


## Anti-goals

- A tag lost or silently changed by the upload reader or the migration.
- Two items that differ only by case or spacing, including under concurrent uploads.
- A real name broken apart by the split rule ("Earth, Wind & Fire" must stay one artist).
- An item that points at a file or an album that no longer exists, or a file that points at a deleted item.
- A renamed, merged or split item that leaves a stale text name in a response or a stale library ETag.
- A tag edit that makes devices download the audio again.
- An upload that fails because its tags are unreadable: it falls back to the file name and no items.

## Constraints

- The sync design stays: whole lists with strong ETags and `304`, plus a hub push per change; ownership is always the signed-in user, and another user's item is a `404`.
- The library has at most 5000 files per user; a library query may join the link tables.
- Upload jobs are processed one at a time by the worker, so linking is done there; a unique index on the normalised key is the backstop against duplicates.
- Repository conventions apply (one class per file, `Entity` prefix on domain entities, a command or query and a handler per use case, services named `…Service`, no helper classes).

## Approaches considered

**Where the names live.** (1) The items are the source of truth and the text in responses is derived; (2) the text on the file stays authoritative and the items are an index parsed from it; (3) items only, responses carry just IDs. Chosen: **1**, the user's call. Option 2 keeps two sources of truth and would undo a hand-made split on the next edit. Option 3 breaks every client and its search until it joins names itself.

**How an edit through `PATCH /Media/{id}` works.** Text fields are still accepted and resolved to items with the same rules; new ID fields are accepted too. Sending both for the same field is a `400` *(my call)*.

## Design

### Data

- **EntityArtist, EntityGenre:** ID, owner, `Name`, `NameKey` (the normalised name, unique per user), created and modified times.
- **EntityAlbum:** ID, owner, `Title`, `TitleKey`, the set of its artists, and an `IdentityKey` (title key plus the sorted artist IDs) that is unique per user and recomputed when its artists change.
- **Links:** file to artist (ordered, so "A; B" keeps its order), file to genre, album to artist (ordered), and an optional album on the file.
- The existing text columns on the file metadata are kept until the backfill has run, then dropped in a later migration *(my call)*.

### Upload

- The worker reads the file through a new `MediaTagReadingService` (the counterpart of the writing service). Unreadable tags are logged and ignored.
- The credit-splitting rule lives in a service of its own so upload, `PATCH` and the migration use one implementation.
- The job remembers whether the user sent a title, so the order above can be applied in the worker.
- Find-or-create and linking happen in the same commit that adds the file and removes the job.

### Endpoints (each kind follows the playlist pattern)

- `GET /Artist`, `/Album`, `/Genre`: the user's items with their file counts; strong ETag; `304`.
- `POST` to create, `PATCH /{id}` to rename (and for an album, to set its artists), `DELETE /{id}` (clears that link from its files; files are never deleted). A rename that collides with another item depends on the user's `autoMergeOnRename` setting (see "User settings"): off (the default), it is a `409` that names the other item so a client can offer a merge; on, the renamed item is merged into the existing one and the existing one keeps its name. The user chose this.
- `POST /{kind}/{id}/MediaFileList` and `.../Remove`: assign or clear the item on a list of files. For an album, assigning replaces the file's album.
- `POST /{kind}/{id}/Merge` (into another item) and, for artists and genres, `POST /{kind}/{id}/Split` with the names to split into (existing items are reused, new ones created).
- `ResponseMediaFile` gains `artistIDList`, `albumID`, `albumArtistIDList`, `genreIDList` next to the existing text fields. `PATCH /Media/{id}` accepts the same ID fields.
- Hub: one pair of events per kind, an updated one with the item and a deleted one with the ID; files changed by an assignment, rename, merge, split or delete are pushed with the existing `MediaUpdated`.

### User settings (a placeholder with one setting)

The user expects more settings over time but has not thought them through, so this adds only the minimum and keeps it extensible:

- A per-user settings record (`EntityUserSettings`, one per user) holding typed values. The first is `autoMergeOnRename`, a boolean, default `false`. `GET /Settings` returns the defaults when the user has no row, and the row is created by the first `PATCH` that changes a setting (this replaces "created on first read"): a `PATCH` that leaves the settings as they are, including one that sends the default to a user with no row, saves and pushes nothing.
- `GET /Settings` (strong ETag, `304`) and `PATCH /Settings` with the usual left-out-field-is-untouched semantics; a push `SettingsUpdated` so every device sees a change. Another user's settings are never reachable: the user comes from the token.
- A new setting later is a new field, a new line in the PATCH request and a new test; no new endpoint.
- The frontend gives it a place on the Settings page; the rest of that page is separate.

### Migration

A schema migration adds the tables and links. An idempotent, resumable start-up step then reads each file's existing text, applies the same split and matching rules per user, creates and links the items, and marks the file done. Nothing is dropped until every file is done. A test proves the step can run twice with the same result.

### Tests

Unit tests with the fakes for the splitting service (each separator, no split on `&`, `,` or `/`, names kept intact, duplicates inside one credit), the key normalisation, upload linking (album created once for a whole album, the second track reuses artists and album, concurrent uploads of the same artist make one), title order, unreadable tags, every manual operation (create, rename collision with the setting on and off, merge, split, assign, delete that clears the links), the settings record (defaults, PATCH, ETag, push, ownership), and ETag changes after each. Repository integration tests for the unique keys and cascades, the migration step, and end-to-end tests through HTTP and the hub, including `PATCH` with text and with IDs.

## Left to the builder

Table and class shapes, the exact normalisation (including which Unicode form), the tag reading mechanics and which TagLib properties map to which fields, how multi-value artists are written back into a downloaded file, batch sizes for the backfill, and the response class shapes.

## Questions that could still change the design

None open. Decided by the user: a rename that collides is controlled by the synced `autoMergeOnRename` setting (default off); deleting an artist, album or genre clears the link from its files and keeps the files; labels are left for later; credits are split only where it is unambiguous.

## How it ended up

Calls made while building, beyond the design above (the plan's ledger lists them with their cost if wrong):

- **ETag of the item lists** covers each item's file count as well as its ID and modified time, because an upload or a delete changes a count without touching any item row.
- **A unique-key conflict is detected at commit** (`MetadataKeyConflictException`), and the whole use case runs again in a fresh unit of work, which finds the winner's item. Find-by-key also sees an item added earlier in the same unit of work, so one batch never adds the same item twice.
- **A name that is taken is a `409`** whose problem details carry the existing item in `existingID`. Merging into itself and a split with fewer than two different names are `400`.
- **Album identity follows its artists.** Deleting, merging or splitting an artist replaces it in every album artist list, recomputes the identity key, and merges two albums that become the same.
- **IDs and text for the same field, or `albumID` with album text, are a `400`** on `PATCH /Media/{id}`; a file belongs to the album with a given title and artist set, so changing the album artists moves it to the matching album.
- **ID3v2.2 and 2.3 have no list of values**, and TagLib splits their text frames on "/". The reader puts the pieces back together so "AC/DC" stays one artist; the writer writes ID3v2.4 so several artists are separate values.
- **Legacy text** stays in the old columns until the start-up backfill has linked it and cleared it; a response shows the legacy text only for a kind the file has no items of. Album artist text with no album keeps its legacy text.
