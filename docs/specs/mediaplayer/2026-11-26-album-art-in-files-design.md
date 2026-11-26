# Album art in the local files

## Intent

The local "write metadata to files" flow writes the text tags of a song but never its picture, so a file in the user's music folder has the cover it was uploaded with, or none, whatever the app shows for the album. The user wants the album's picture written into the file as its cover, replacing the one that is there, so the files match the app.

## Goals

- When a file's tags are written on a device, the album's picture (the thumbnail the app keeps) is embedded as the front cover, replacing every picture the file had.
- A file whose cover is not the album's picture counts as out of date, like one whose text tags differ, so the warning, the "update outdated files" action and the automatic write all see it.
- A changed album picture (set by hand, filled in by a later upload, or inherited by a moved song) flags every file of that album again.

## Non-goals

- The server's own tag writing (an upload keeps the file's cover).
- A larger picture than the thumbnail (`Art:ThumbnailMaxPixels`, default 300).
- Removing a cover from a file whose album has no picture; an album without a picture writes nothing and counts as matching.
- Several pictures per file, and non-cover picture types.

## Anti-goals

- A file left half written when the cover is large: it goes through the same copy-and-swap as the other tags (`IFileStore.Rewrite`).
- The warning or the automatic write flapping: once a file is written with the cover it must compare equal to it.
- Rewriting files for an album picture the device does not have yet: no picture, no comparison.

## Constraints

- The comparison is byte for byte: the cover TagLib reads back from a file written with the thumbnail is the thumbnail.
- The device decides "out of date" from the media file's `TimeModified` (recorded when the tags were last written or found equal). An album picture change does not change the media file today, so the server must move the `TimeModified` of the album's files when the picture changes; nothing on the device needs a second recorded time.
- Repository conventions apply (one class per file, no magic strings, never commit).

## Approaches

- **Part of the out-of-date check (chosen, the user's pick).** The cover is compared like the other tags, so files match the app and a changed picture is noticed. The cost is that most files, which were uploaded with their own larger cover, show as out of date until written once.
- Only written during a text-tag write, not part of the check: quiet, but a new album picture never reaches the files by itself. Not chosen.

## Design

- `IMediaTagWritingService.Write(string filePath, MediaTagValues values, byte[]? picture = null)`: a picture, when given, replaces all pictures of the file (a JPEG front cover); `null` leaves the pictures alone. The server's calls pass none.
- `MediaTagSetExtension.Matches(MediaTagSet tags, MediaTagValues values, byte[]? picture)`: the existing text comparison, and, when a picture is given, the file's cover must equal it byte for byte. Without a picture the cover is not compared.
- `IArtService.GetAlbumArt(Guid albumID)` returns the album's picture bytes (or null: no album, no picture, or not available now), from the same copy and cache as the data address.
- `FileTagService`: the check and the update look up the album picture of each media file (`ResponseMediaFile.AlbumID`) and pass it to the comparison and the writer. A picture that cannot be had now leaves the file unchecked this time (as a locked file does), so a network failure never flags files.
- Server: when an album's picture is stored (`IAlbumArtService.Apply`, `Replace`, `Inherit`), the media files of the album get a new `TimeModified` in the same unit of work and are pushed (`NotificationMediaUpdated`), so every device sees the change and checks its files again.

## Tests

- Writer: a picture replaces existing pictures, none given leaves them, reading back gives the same bytes.
- Matches: equal cover matches, different cover does not, no picture given ignores the cover, a file with no cover and a picture given does not match.
- FileTagService: a file with another cover is out of date; after `Update` the writer got the album picture and the file compares equal; an album with no picture is never flagged; a picture that cannot be had leaves the file unchecked.
- AlbumArtService: storing, replacing or inheriting a picture touches and pushes the album's files.
- By hand: update outdated files and see the covers in another player; replace an album picture in the app and watch its files get flagged.

## Left to the builder

- How `FileTagService` fetches pictures for many files without asking twice for one album.
- Whether the pushed files of a large album are batched.
