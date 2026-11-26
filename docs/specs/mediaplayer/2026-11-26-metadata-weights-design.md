# Weights for artists and genres

## Intent

A song can carry several genres (and artists), and today they show in the order the song happens to have them. The user wants a genre such as "Soundtrack" to stay on the song but come after "Pop" or "Rock" wherever the genres are listed, without reordering every song by hand. The user gives each genre and artist a weight, and a song's list is ordered by it. The same order is written into the file tags.

## Goals

- Each artist and each genre has a **weight**: a whole number from 0 to 1000, default 100, higher comes first. It is set on the Organise page, where the item is edited.
- Wherever a song's artists or genres are listed or joined (the player bar, tables, queue, edit dialog, the `Artist` and `Genre` text of a song, search text), they are in the weighted order. Items with the same weight keep the song's own order, so nothing changes until a weight is set.
- The order written into the file tags (by an upload's re-tagging on the server and by the local "write metadata to files" flow) is the weighted order.
- Changing a weight changes the songs that carry the item, and every device sees the new order without a full resync.

## Non-goals

- Album artists keep their order (an album is identified by its set of artists, and the order there is not a preference).
- Sorting by name, and a weight per song or per playlist.
- Rewriting tags in bulk when a weight changes: the files follow through the existing, opt-in tag update, as for a rename.
- A drag-and-drop ranking screen: the weight is a number field.

## Anti-goals

- A weight change that leaves a device showing the old order until the next full sync.
- The per-song order being lost: it stays in the data as the tie-break, and a per-song order set in the edit dialog still decides between items of the same weight.
- Two places that each work out the order and disagree.

## Constraints

- A song's artists and genres are stored as ordered link rows (`Position`) with the item loaded alongside (the existing accessors already require that). The order is worked out in one place on the server (`EntityMediaFileExtension`); responses carry the result, so devices and the tag writers follow it with no logic of their own.
- A weight change is a metadata change: the media files that carry the item get a new `TimeModified` in the same commit and are pushed, as a rename does.
- Repository conventions apply (one class per file, no magic strings, enums end in `Enum`, texts in resource files, never commit). The EF migration needs the API stopped.

## Approaches

- **A weight per item (chosen, the user's idea).** A number on the artist or genre, sorted descending with the song's own order as the tie-break.
- A "demote" switch per item: simpler, but only two levels.
- Sort by name: does not express "less important", so it does not solve the case.

## Design

- `EntityArtist.Weight` and `EntityGenre.Weight` (int, default 100, one named constant in the Domain project); `ResponseArtist.Weight`, `ResponseGenre.Weight`; table columns and a migration with default 0.
- `EntityMediaFileExtension` gets one ordered view of the artist links and of the genre links (`Weight` descending, then `Position`); `GetArtistNameList`, `GetGenreNameList`, `ToResponse` (the `ArtistIDList`, `GenreIDList` and the joined `Artist` and `Genre` text) and `ToTagValues` all use it.
- New commands `CommandArtistSetWeight` and `CommandGenreSetWeight` (user, item ID, weight), handled like the renames: set the weight and `TimeModified`, touch and push the files that carry the item, push the updated item. Endpoints next to the rename ones. A weight outside 0 to 1000 is refused.
- Client: the artist and genre lists carry `Weight` (the cached metadata stores it); the Organise edit dialog for an artist or genre gets a "weight" number field next to the name. Items are listed on the Organise page as they are today.
- The client's local tag writer already builds its tag values from the song's `ArtistIDList` and `GenreIDList`, which the server now sends in weighted order, so it needs no change beyond a test.

## Tests

- Domain: weighted order, tie-break by position, equal weights unchanged, a lower weight last, a weight of 0 below the default, the joined text and ID lists agree.
- Application: setting a weight touches and pushes the files that carry the item, rejects an out-of-range weight and another user's item; the tag values of an upload follow the weights.
- Client: an item dialog edit sends the weight; a song's tag values follow the pushed order.
- By hand: set "Soundtrack" low, check the player bar, the table and the edit dialog on a song with Soundtrack and Pop; write the tags and read them in another player.

## Left to the builder

- The exact dialog layout of the weight field, and whether the Organise list shows the weight.
- The migration name; the endpoint route names to match the rename ones.
- How the client's cached metadata stores the new column (a migration of its own cache if it keeps a schema).
