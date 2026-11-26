# Library organiser: genres, artists and albums as things you can sort files into

The Organise page (`2026-11-24-organise-page-design.md`) is the UI for this; drag and drop is not built. The data layer (the items, upload linking and the endpoints) is in `2026-11-15-metadata-entities-design.md`, which also answers the questions about several artists per file, splitting credits and manual editing. Everything marked *(my proposal)* is a suggestion, not a decision. The questions at the end are the user to answer before this can be planned.

## Intent

Today a media file's artist, album, album artist and genre are free text. The same artist can exist as "Radiohead", "radiohead" and "Radiohead " on different files, renaming one means editing every file, and there is no way to see "all the genres I have" as a list. The user wants genres, artists and albums to be real things in the library: each one listed by name, with the files that belong to it, and organised by dragging library files into them. It should feel like sorting files into folders.

## Goals

- **Genres, artists and albums exist as items of their own,** owned by the user, each with a name and the files assigned to it.
- **A panel on the library page** opens from a button right of the search bar. It has a section for each of the three kinds. Choosing a kind shows a folder-like view of the ones that already exist.
- **Drag to assign:** the user ticks library rows (or drags one), drops them onto a folder, and that genre, artist or album is added to those files.
- **Existing text tags carry over.** On first use, the distinct values already typed into files become items, with no tag lost and no file changed in a way the user did not ask for.
- **Everything syncs like playlists do:** each list has an ETag and a hub push, and every device sees an assignment or a rename without a reload.
- **A saved or downloaded file still carries the right tags,** written from the item names at download time, as the "Save file as" copy does.
- *(my proposal)* Renaming an item changes it on all its files at once, and two items can be merged.

## Non-goals

- Automatic tag lookup from online databases, cover art, and nested genres (sub-genres).
- Sharing items between users.
- Any change to the stored audio file: assignments live in the database, as tag edits do today.
- Building this before the library, playlists, downloads and player work.

## Anti-goals (a failure even if it technically works)

- A tag lost or silently changed by the move from text to items.
- Two items that differ only by case or spacing (the very problem this is meant to remove), unless the user chose to keep them apart.
- A drop that replaces a value the user wanted to keep, or adds one they did not expect.
- A tag change that makes devices download the audio again (the stored file stays untouched).
- A library, search or sort that gets slower or stops working offline because values now live in separate lists.

## Constraints

- The sync design stays: whole lists with a strong ETag and `304`, plus a hub push per change; no deltas. The library has at most 5000 files per user.
- The media file response carries `artist`, `album`, `albumArtist`, `genre` as text today, and the app searches and sorts on them. Existing clients must keep working, so the text stays in the response, derived from the item names.
- Repository conventions apply (one class per file, `Entity` prefix on domain entities, handlers per use case, and so on).

## Approaches considered

**1. Items with links to files, names also kept in the file response** *(my proposal)*. New tables for artists, albums and genres, files link to them, and the media file response keeps the text names and gains the item IDs. The panel and the drag use the IDs; search and sort keep using the text. Trade-off: a real data change and a migration, but nothing the app already does has to change.

**2. Keep text, add only a view.** The panel groups files by the distinct text values; a drop rewrites the text on the files. Trade-off: no migration at all, but no empty folder can exist, renaming is still an edit per file, and spelling differences stay separate folders. It does not give the user the "things that exist" they described.

**3. Full model with album belonging to an album artist and tracks ordered inside albums.** Most correct for a music library. Trade-off: the biggest change, and it asks questions about compilations and "feat." artists that the user has not answered.

Recommended: 1, with 3 left open as a later step. The user has not chosen.

## Design sketch *(my proposal, for discussion)*

### Data

- **Artist, Album, Genre**: ID, owner, `Name` (an album also has its album artist), a normalised name that is unique per user. A file has at most one artist and one album and any number of genres.
- **Migration:** for each user, the distinct trimmed, case-folded values of the existing text tags become items; files are linked; the original spelling of the first file seen is kept as the name.

### Endpoints (all follow the playlist pattern)

- `GET /Artist`, `GET /Album`, `GET /Genre`: the user's items with file counts, strong ETag, `304`.
- `POST`, `PATCH /{id}` (rename), `DELETE /{id}` for each kind. Deleting an item clears that tag from its files (it never deletes files).
- `POST /Genre/{id}/MediaFileList` and `.../Remove`, and the same for artists and albums: assign or clear a list of files in one call.
- `POST /{Kind}/{id}/Merge` with the item to merge into.
- Hub events for each list change, like `PlaylistUpdated`; media files changed by an assignment are pushed with `MediaUpdated`.

### The panel (what the user sees)

```
Library  [Actions •••]  [ Search …………… ]  [⋮ Organise]      <- button opens the panel →
                                         ┌ Organise ─────────┐
  ☐ Title      Artist     Album          │ Genres Artists Albums │
  ☑ Blue Monday  …                       │ [+ New genre]          │
  ☑ Teardrop     …        ──drag──▶      │ 📁 Electronic (212)    │
                                         │ 📁 Trip hop (48)       │
                                         │ 📁 Metal (97)          │
                                         └───────────────────────┘
```

Clicking a folder filters the library to its files (*my proposal*). Dragging ticked rows onto a folder shows the number of files being moved and highlights the folder it will land in.

## Left to the builder

The migration's exact normalisation rules, the drag-and-drop mechanism in Blazor, and the response class shapes.

## Questions for the user (needed before this can be planned)

- **One or several artists per file?** A genre is naturally several per file; an artist is usually one, but "feat." and collaborations may need several.
- **When you drop onto an artist or album that a file already has one of,** should it replace (my assumption for one-value tags) or ask?
- **What should deleting a folder do?** I assume the files stay and just lose that tag.
- **Two items with the same name after a rename:** merge them automatically, or refuse and tell the user?
- **Albums:** do they belong to an album artist (the same album title by two artists is two albums), or are they just names?
- **Should clicking a folder filter the library,** or only open a separate view of that folder?
