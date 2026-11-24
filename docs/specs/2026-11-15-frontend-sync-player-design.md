# Frontend: sync and player foundation: design

Status: platform changed on 2026-11-15 from Blazor WebAssembly to a MAUI Blazor Hybrid app for Windows and Android: read `2026-11-15-frontend-maui-design.md` first, it wins wherever the two differ (storage, hashing, playback, the project layout, Google sign-in, the server address). The sync, diff, push and player designs below stand. Statements about the browser in this document are kept as history where the MAUI design replaces them. The backend contracts below match the API as of 2026-11-15.

Built (frontend, 2026-11-17): the player now plays the current item of a queue (`IQueueService`) instead of its own list, with shuffle and repeat applied to the source part, a local copy of the queue on the device and the remote playback services; see the "Built (frontend)" paragraph of `2026-11-16-remote-playback-design.md`. `IPlayerService` no longer has `Play`, `PlayList`, `Enqueue` or `Remove`: it has `StartLibrary`, `StartPlaylist`, `PlayCurrent`, `JumpTo`, `Next`, `Previous`, `SetShuffle` and `SetRepeat`.

## Intent

The media player is a sync-and-play app, not a streaming service. A user's media files live on the server; every device keeps the full list of media files and playlists (metadata) up to date, and keeps the audio files it needs in local storage. The player only ever plays from local storage. A media file that is not on the device yet is downloaded first and then played. The first target is the Blazor WebAssembly app (`Noxy.NET.MediaPlayer.Frontend` with `Noxy.NET.MediaPlayer.Presentation`), with the browser as the file store, but the store must be replaceable so a native one can follow.

## Goals

- **Sign in and out** with email and password, keep the session across reloads, and refresh the access token without the user noticing.
- **Metadata always synced.** At start-up a device calls `GET /Media/Library` and `GET /Playlist` with the stored ETags, applies the changes, and keeps both lists current from the hub (`MediaUpdated`, `MediaDeleted`, `PlaylistUpdated`, `PlaylistDeleted`). The full library is browsable and searchable on the device, offline included. The ETag a device stores is only ever one that came with a full response it downloaded, never one inferred from a push, so a device that missed an event repairs itself at the next start-up or reconnect.
- **Manual refresh** of the library list and of a single playlist: each ignores the stored ETag (no `If-None-Match`), downloads in full, and replaces the local copy. It is the escape hatch for a device that looks wrong.
- **A library tab** showing the full library from the local store, searchable (title, artist, album, album artist, genre) and sortable, paginated by the client (infinite scroll later is a UI change only), with no server call per page or per search. The server still sends the whole list in one response; there is no server-side paging or search in the app.
- **File sync per media file, with a visible state** (not downloaded, downloading, on this device, failed). Files get onto a device in two ways: pressing play (or adding a media file to the queue) downloads the media file if it is missing and then plays it; and the user can start a download for a media file, for a playlist ("download all" on the playlist), or for the whole library ("download all" on the library tab). A bulk download is opt-in and one-time: it queues every media file that is missing at that moment, shows its progress, and stops when storage is low. Media files added later are not downloaded automatically; they come down when played or queued, or when the button is pressed again.
- **The player plays only from the device store**: play, pause, seek, next, previous, a queue, volume, and prefetching of the next few media files in the queue so the wait for a missing media file is rarely felt.
- **Playlists**: list, create, rename, delete, open (entries with their media file data), add and remove media files, reorder, and the duplicates flag, all through the existing `/Playlist` endpoints.
- **Integrity**: a downloaded file is verified against the media file's `contentHash` (SHA-256) before it counts as on the device.
- **A replaceable store.** Everything that touches local storage (metadata, ETags, files, free space) goes through interfaces, with the browser as the first implementation.

## Non-goals

- Streaming: no playing while downloading, no `MediaSource`, no service worker tricks, no access token in an audio or file URL. (Revisit later behind the same interfaces.) The SignalR hub is the one exception: its connection sends the access token as the `access_token` query parameter, which the API accepts for the hub only.
- Several tabs of one browser coordinating with each other: each tab runs its own sync, hub connection and downloads. Writes are keyed by media file ID and idempotent, so the worst case is a duplicate download, not corruption.
- Cover art, sharing playlists, smart playlists, import or export.
- Google sign-in (sign-up and sign-in with email and password only for now).
- Server-side search or sorting.
- Automatic eviction of files from the device: files leave only when the user removes them or the media file is deleted.
- Editing media file tags from the UI (the endpoint exists; the screen can come later).
- Uploading media files from the UI: built later, see the Library and Playlist pages in `2026-11-15-frontend-screens-design.md`.

## Anti-goals

- Playback that depends on the network when the media file is on the device.
- A media file marked "on this device" whose bytes are missing, partial, or do not match its hash.
- A screen that says a media file is available offline when it is not.
- Losing the library or the files because the browser cleared its storage without the app noticing: the app must detect an empty or partial store and recover by syncing again, never show stale "on this device" states.
- A download storm from scrolling a long list: only playing, queueing or an explicit force download start a download.
- A change made on another device not appearing on this one while the app is open.

## Constraints

- Follow the repository conventions: one class per file, no summary comment above a class, `List` suffix for lists, no `yield`, no magic strings (constants or `nameof`; hub event names are the members of `MediaHubEventEnum`), interfaces in an `Interfaces` folder, `is null`, no `!` (handle the null or comment why it is safe), extension blocks and primary constructors.
- The app is hosted on a different origin from the API, so reading `ETag` (and `Content-Range`) from responses requires the API's CORS policy to expose them. This is a backend prerequisite, listed below.
- The solution already uses Fluxor for state (`Noxy.NET.UI.Shared.Fluxor`) and has shared UI libraries under `Noxy.NET.UI`; build on them where they fit.
- Storage: no browser storage any more. The platform is a MAUI app (see `2026-11-15-frontend-maui-design.md`): metadata in SQLite, files on disk, the refresh token in `SecureStorage`.
- Library caps from the backend: 5000 media files per user, 100 playlists, 1000 entries per playlist.
- `Noxy.NET.MediaPlayer.Presentation` references `Application`, which now carries server-only dependencies (TagLibSharp, Google.Apis.Auth). The WebAssembly bundle must not grow because of that; if it does, the shared contracts (requests and responses) are what belongs in a lighter shared place.

## Approaches considered

**How a media file reaches the player.** (A) download it to the device store and play from the stored copy; (B) stream it, with the audio element reading the server directly; (C) stream and save at the same time through a service worker or `MediaSource`. Chosen: **A**, the user's call. B needs credentials in the URL or a service worker, and turns the app into a streaming player; C is a lot of machinery for a wait of a few seconds on a fast connection. The player gets its audio through the store interface, so B or C can be added later without touching the rest.

**What a device holds.** (A) everything, always; (B) only what the user has opened, played or asked for; (C) metadata for everything plus files on demand, with opt-in bulk downloads ("download all" for a playlist or the whole library). Chosen: **C**, the user's call: the full library is browsable and playlists can be built offline, the user can always see what is not downloaded, and anyone who wants everything on the device presses "download all".

**Where files live.** Superseded by `2026-11-15-frontend-maui-design.md`: metadata in one SQLite file and files as one file per media file in the app's data folder (not a cache folder). The first design chose IndexedDB blobs for the browser; a browser cannot hold a library of this size reliably, which is why the platform changed. The store still reports free space.

**Where state lives.** (A) a Fluxor store fed by services; (B) plain services with events. Chosen: **A**, my call, to match the shared UI libraries; the services own the work (sync, downloads, playback) and the store only reflects it for the components.

## Design

### Layers (names are guidance; the builder decides the exact ones)

- **API client**: typed calls for every endpoint the app uses (sign-in, sign-up, refresh, library, playlists, file download), attaching the bearer token and handling a 401 by refreshing once and retrying. Sends `If-None-Match` and reads `ETag`.
- **Session service**: signs in and out, keeps the tokens (refresh token in storage, access token in memory), signs in again silently on load, and refreshes before expiry.
- **`IMetadataStore`** and **`IFileStore`** (the replaceable seam, in an `Interfaces` folder): metadata (media files, playlists, playlist entries, the two list ETags, per-media file file state) and files (a held file by media file ID with its path, a partial area for downloads in progress, delete, list which media file IDs are held, free space). The implementations are a SQLite store and a disk store in `Storage`.
- **Sync service**: start-up diff of library and playlists (send the stored ETag; on 304 do nothing; on 200 replace the local list and remove local media files and files that are gone); hub connection that applies each pushed change as it arrives; a reconnect always re-runs the diff because pushes sent while offline are lost.
- **Download service**: a queue with a small number of parallel downloads, resume with `Range` and `If-Range` after an interruption, hash verification before a file is marked on the device, retry with backoff, and state per media file. Callers: play or queue of a missing media file, and force download.
- **Player service**: the queue, the current media file, position, volume, and prefetching of the next few media files; plays through an `IAudioPlayer` (native, one per platform) given the file path from `IFileStore`; waits on the download service when the media file is missing; and is the only part that knows about the audio player.
- **Search**: a pure function over the local library list.

### Screens (what exists, not how it looks; the look is designed separately with the user)

Sign in and sign up; library tab (all media files from the local store, searchable, sortable, paginated, each with its file state and an action to download, play or add to a playlist; a "download all" button and a "refresh library" action); playlists list; a playlist (its media files in order, reorder, remove, add, the duplicates setting, a "download all" button and a "refresh playlist" action); now playing (current media file, queue, controls); a small storage and sync status (free space, what is downloading).

### Behaviour

- **Start-up:** read local state first so the library shows instantly, even offline; then sign in silently; then run the diff and connect the hub. A failed diff because the device is offline leaves local state as it is and shows an offline indicator.
- **File state** is stored per media file with the `contentHash` the file was verified against. If the library's `contentHash` for a media file differs from the stored one, the file is stale and the state returns to not downloaded.
- **A media file deleted on the server** removes its metadata and its file; a media file in the current queue that is playing finishes its current play first (it is held outside the regenerated queue), then the queue goes on without it, and Previous from the next media file goes to the one before the deleted one.
- **Playlist entries** carry the media file data, so a playlist can be shown and its files queued without another call.
- **Storage low or cleared:** a bulk download stops and says why; on start-up the app compares the held file IDs with the file states and repairs any that disagree.
- **Offline:** playing a media file on the device, browsing, searching and viewing playlists all work. Actions that need the server (create or edit a playlist, download) are disabled with a reason, not silently queued, in this first version.

### Backend contracts (authoritative; read them from the code)

- Requests are the classes in `Noxy.NET.MediaPlayer.Application` (`Requests/…`) and responses the classes in `Noxy.NET.MediaPlayer.Domain` (`Responses/…`), both shared with the app. Sign-up, sign-in and refresh live in `AuthController` (`POST /Auth/SignUp`, `/Auth/SignIn`, `/Auth/Refresh`) and return `ResponseAuthentication` (`AccessToken`, `RefreshToken`); the access token lives 15 minutes and the refresh token 30 days by default (`ConfigJwt`), and the refresh request is `RequestRefresh { RefreshToken }`. Errors come back as problem details with the status code (400, 401, 409, 429 with `Retry-After`); see `ApiExceptionHandler`.
- `GET /Media/Library` and `GET /Playlist` return the whole list on a 200 with the ETag in the `ETag` header, and a 304 with no body when `If-None-Match` matches. There are no deltas: the client diffs against what it holds.
- `GET /Media/{id}/File` is the sync download: strong ETag equal to the quoted `contentHash`, range requests, and `If-Range`. `GET /Media/{id}/Download` is a tagged copy for saving to disk, not for sync.
- `GET /Playlist/{id}` returns the playlist with its entries, each carrying the media file's full response. Its ETag also changes when a contained media file's metadata changes.
- The hub is at `/hubs/media`. Events are the members of `MediaHubEventEnum`: `MediaUpdated` (the full media file response), `MediaDeleted` (the media file ID), `PlaylistUpdated` (the playlist response without entries), `PlaylistDeleted` (the playlist ID), plus `UploadSucceeded` and `UploadFailed`. A device holding a playlist's entries refetches it when the pushed `timeModified` is newer than its own.
- There is no saved playback session on the server (the earlier `/PlaybackSession` endpoints and their two hub events are gone). The live queue, the device registry and the commands are in `2026-11-16-remote-playback-design.md`; each device keeps a local copy of its last queue.

### Backend prerequisites

- The CORS policy exposes `ETag`, `Content-Range`, `Accept-Ranges` and `Content-Disposition` (`WithExposedHeaders`), with a test. This is the first task of the plan. **Done (2026-11-15):** `AddApiCors` exposes the four headers, pinned by `CorsPolicyTest` (an allowed origin can read each one; a disallowed origin gets no CORS headers). Without it a browser on another origin cannot read `ETag` and the start-up diff would never get a 304.
- Nothing else is required. `GET /Media` (paged) is not used by the app and can be removed later.

### Starting point

The first version of this design started from a blank Blazor WebAssembly shell. The platform is now a MAUI Blazor Hybrid app (see `2026-11-15-frontend-maui-design.md`): `Frontend` is replaced by `App`, `Presentation` becomes a plain class library and a new `Storage` holds the disk-only store implementations. There are no auth pages, no Fluxor setup and no screens yet, so the plans include the scaffolding. `Noxy.NET.UI` has shared components and Fluxor helpers to reuse where they fit.

### Calls made while writing this (mine, to be checked)

- **Hash verification:** the SHA-256 of the file on disk, computed from a stream, so the file is never held whole in memory. (Platform changed to MAUI, see `2026-11-15-frontend-maui-design.md`.)
- **Resume:** within a session, the partial bytes stay in memory and the download continues with `Range` and `If-Range`; across a reload, a partial download simply starts again. A partial file is never stored as a media file on the device.
- **Playback:** the native audio player of each platform plays the file at the path the file store reports, and releases it before the file can be deleted. (Platform changed to MAUI, see `2026-11-15-frontend-maui-design.md`.)
- **Refresh token storage:** `localStorage` through Blazored.LocalStorage, because the API returns the refresh token in the response body (there is no cookie flow). The access token stays in memory only. A cookie flow would be a backend change for later.
- **A media file whose `contentHash` changes while it plays or downloads:** the current play finishes, the running download is cancelled, and the state returns to not downloaded.
- **`Presentation` referencing `Application`:** measure the published bundle size first; move the request and response contracts to a lighter shared project only if the server-only packages end up in the bundle. This is a separate task, not part of the first plan.

### Tests

Unit tests with fakes for the stores and the API client: the diff (304, changes, deletions, stale files), the download queue (resume, hash mismatch, retry, limits), the player queue and prefetch, search, session refresh. The browser store through interop is covered by a small set of in-browser tests or a documented manual check. An end-to-end run against the real API for sign-in, sync and a download is the acceptance test.

## Left to the builder

Folder and class names inside `Presentation`, `Storage` and `App`, Fluxor action and reducer naming, the SQLite schema, the number of parallel downloads and prefetched media files, the retry policy, how a bulk download is scheduled, and the native audio players. The look of every screen is a separate design with the user.
