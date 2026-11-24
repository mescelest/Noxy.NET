# Album art and playlist colour

Status: approved in conversation 2026-11-24 (SkiaSharp, the places the picture shows, and a configurable thumbnail size); not yet built.

## Intent

Songs and playlists have no pictures except the placeholder tile. Audio files often carry an embedded cover. The user wants: a cover on each album, taken from the file the first time the album is made; a default picture when there is none; and a colour of their own choice for a playlist's tile. Editing artists, albums and genres (including their pictures) comes later, with the library organiser (`2026-11-15-library-organizer-design.md`).

## Goals

- An album made from an upload gets the embedded cover of that file as its picture.
- A song shows the picture of its album; a song with no album, or an album with no picture, shows a default.
- An album that has no picture takes one from a later upload of the same album that has one (never replacing a picture it has).
- A playlist's tile colour can be chosen; unchosen, it stays the colour made from its ID.
- Albums already in the library get their pictures from their files too, without an upload.

## Non-goals

- A picture for a playlist (the uploaded kind): a later round, on the same image storage.
- Editing or replacing an album's picture by hand: the organiser's job.
- Pictures for artists or genres, and a collage of covers for a playlist.
- Showing the full-size cover: only a small thumbnail is kept.

## Anti-goals

- A cover of several megabytes sent to every device and stored in the database.
- A picture that a device keeps showing after the album's picture changed.
- A broken-image icon where there is no picture.

## Approaches

**Where the picture is kept.** (A) a small file per album on the server's disk, next to the media files, with the album saying only when its picture last changed; (B) the bytes in the database. Chosen: **A**, the way the media files already are, so the database stays small and a backup of the folder holds the pictures.

**How a device gets it.** (A) the app asks the server for the picture with its sign-in token and keeps a copy on the device; (B) a link the web view loads by itself. Chosen: **A**: an image element cannot send the token, and a link anyone could open would need its own secret. The cost is a small cache on the device.

## Design

**Server**
- `IArtStorageService` (Application) with a disk implementation in the Storage project, built like `IFileStorageService`: save, open and delete the picture of an album by its ID. The playlist picture later uses the same service with another kind of owner.
- `IThumbnailService` (Application): turns the embedded picture bytes into a JPEG, at most `ConfigArt.ThumbnailMaxPixels` on its longest side (quality 80, a constant). SkiaSharp (MIT) does the resizing, which adds a package to the server (the user agreed).
- `ConfigArt` (Application/Configurations, section `Art`, validated at start like `ConfigPlaylist`): `ThumbnailMaxPixels`, an int from 32 to 2048, default 300, set in `appsettings.json`. A new size applies to pictures made after the change; the ones already kept are not made again.
- `MediaTagSet` carries the embedded picture (the front cover, else the first picture) from `MediaTagReadingService`.
- `EntityAlbum.TimeArtModified` (nullable): set when a picture is saved; null means none. `ResponseAlbum` carries it, so a device knows when its copy is old.
- When an album is created from an upload, or an upload names an album that has no picture, and the file has an embedded picture, the thumbnail is saved and `TimeArtModified` set; the existing `AlbumUpdated` push tells the devices. An album that has a picture is never touched.
- `MetadataBackfillService` runs the same step for albums with no picture, from the first of their media files that has one.
- `GET /Album/{id}/Art`: the JPEG, with an ETag; 404 when there is none; the user's own albums only.
- A migration adds the column.
- `EntityPlaylist.ArtHue` (int 0 to 359, nullable; null means the colour made from the ID). `ResponsePlaylist.ArtHue`. `PATCH /Playlist/{id}` takes `artHue` like its other optional fields (null clears it); the update does not need the playlist's modified time to change more than any other edit does.

**Client**
- `IArtService`: gives the picture of an album as a small `data:` URI, from a copy on the device when the album's `TimeArtModified` matches, otherwise fetched with the token and kept. A failed or missing picture gives null.
- `AlbumArt` (App component): shows the picture, or the default icon tile when there is none. It takes the album's ID and `TimeArtModified`.
- `PlaylistArt` uses `Playlist.ArtHue` when set, else the hue from the ID. The Edit playlist dialog gets a colour picker: a row of twelve swatches 30 degrees apart, and a "default" choice.
- Where the picture shows *(to confirm)*: a small square before the title in the Library, playlist and queue tables, and in the player bar. The sidebar tiles stay playlist colours.

## Left to the builder

- Choosing the cover when a file has several pictures; the exact cache file layout on the device; throttling many pictures asked for at once in a long list.
- The migration's name and its snapshot, which need the EF tool and so the API stopped.
- Tests: the thumbnail service (size, format, a broken image), the storage service, the album rule (create, fill an empty one, never replace), the backfill, the endpoint (404, 304, another user), the client cache (hit, stale, failure), and the hue edit (range, clear).

## Decided with the user

- SkiaSharp for the resizing; the picture shows before the title in the Library, playlist and queue tables and in the player bar; the thumbnail size is a setting.
