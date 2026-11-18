# Frontend screens: shell, library, home, playlists and login: design

Status: the outline (shell, header, sidebar sections, pages, sign-in) is approved by the user as of 2026-11-16; the look (colours, gaps, fonts and so on) is explicitly **not decided**, see "Styling through variables". Written 2026-11-15 from the brainstorming conversation. It sits on top of `2026-11-15-frontend-sync-player-design.md` (sync, stores, downloads, player) and changes it in two places: Google sign-in is now in scope, and two backend additions are needed (see "Backend additions"). Lines marked *(my proposal)* are my suggestions, not decisions. The mockups the user reacted to are in `.superpowers/brainstorm/` (`shell-layout-v2.html`, `library-view-v2.html`, `login-screen-v2.html`).

Built (2026-11-17, Windows): the shell, sign-in and server address, the virtualized Library page and its actions (including Save as file), playlists, Home with Continue, the player bar, queue panel and device picker, Account and Settings with the theme. Not tried: Android and every on-screen manual check, which the user runs. Left out on purpose: the browse-by-genre/artist/album section, cover art, and the YouTube and Bandcamp integrations (empty pages).

Backend status (2026-11-16): the saved session is gone, per-playlist shuffle and repeat and the `KeepQueuedItems` setting exist, and the live session, device registry and remote commands are built on the hub (see `2026-11-16-remote-playback-design.md`).

## Intent

The app is a sync-and-play music player. This design is the part the user sees: one window with a header, a sidebar of sections, a centre that shows the current screen, and a player bar. It has to make the library easy to browse and act on in bulk, make playlists and "what I was just listening to" one click away, and leave room for integrations (a YouTube finder later) and for ways of organising the library that are not built yet.

## Goals

- **One shell for the whole app:** a header across the top, the sidebar and the centre side by side below it, and the player bar along the bottom. The centre is at least the full height of the space between header and player bar and scrolls inside itself when the content is longer.
- **Header:** a button that hides and shows the sidebar, the logo, a search bar, and room on the right for more (sync status, the user). It can be extended.
- **Sidebar in sections:**
  1. *Your music:* Home, Library, Account, Settings and other fixed places.
  2. *Integrations:* YouTube (to find and download files; only an empty page for now) and Bandcamp (a placeholder).
  3. *Playlists:* a "New playlist" button at the top of the section, then the user's playlists, most recently played first *(my proposal for the order)*.
  4. A section for browsing by genre, artist or album is **left for later** (see `2026-11-15-library-organizer-design.md`).
- **Home page:** a stack of sections that can grow. First section: the playlists played most recently, as cards. Second: "Continue", which resumes the listening session (see below).
- **Library page:** a frosted-glass panel pinned to the top of the centre while the list scrolls. Row 1: the title, file and selection counts, an **Actions** button (three dots, opens a menu, disabled until at least one row is ticked), the search box, and later a button that opens the organiser sidebar. Row 2: the column headers with a select-all tick box. Clicking a header sorts by it; clicking again reverses. Under the panel: one detailed row per file with a tick box, the on-device status icon (on this device, downloading, not downloaded), and the columns. A right-click menu per file comes later.
- **Actions menu items:** download to this device, save as file, add to playlist, remove from this device, delete from library (last, in red) *(the items are my proposal)*. The list is built from one place so items can be added.
- **Sign-in screen:** no shell while signed out. One card with the logo, then the sign-in providers as a list of buttons (Google first), a divider, then tabs for *Sign in* and *Create account* with email and password. A failed attempt shows the server's message in the form. More providers later are one more button in the list.
- **Forms use the existing WebForm components** (`WebForm`, `WebFormContext`, the input groups), which validate with the request classes' data annotations and place labels and errors. Styling is done on top of them for this app.
- **Everything the user changes syncs** across their devices: playlists and their order, media files, the last-played time of a playlist. The listening queue is not synced as a record: each device keeps its own local copy (see "Listening session and queue").

## Non-goals

- The browse-by-genre/artist/album sidebar and treating those as entities (its own design, later).
- Playing a media file straight from the server, cover art, sharing, and editing tags from the UI in the first version.
- The YouTube integration itself (finding and downloading). Only its place in the sidebar and an empty page now. Bandcamp is a placeholder.
- A per-song "continue playing": a song always continues through the playlist or queue it was started from.

## Anti-goals

- A screen that looks usable while offline but silently fails: actions that need the server are disabled with a reason.
- Losing the user's place: the Home page and the Continue section must never show a playlist or queue that no longer exists.
- Bulk actions that apply to rows the user cannot see (a filter hides some ticked rows) without saying how many will be affected.
- A glass panel that hides the list rows behind it while scrolling or makes the text unreadable on any theme.
- Sign-in that shows more than the server's message, or leaves the user unsure whether they are signed in.

## Constraints

- MAUI Blazor Hybrid (Windows and Android), see `2026-11-15-frontend-maui-design.md`. The screens are built on the shared `Noxy.NET.UI` libraries (the left-sidebar layout, table, button, dialog, collapsible, icons, the theme toggle, WebForm and the input groups) with Fluxor holding state that reflects the services in `Presentation` (see the foundation and downloads plans). New components (the glass panel, player bar, header) are added in `Presentation` or the UI libraries as fits.
- The library has at most 5000 files, all held locally, so sort, search and selection are done on the device with no server call per interaction.
- The sidebar's playlist list and the home page read only from the local store, so they work offline.
- Repository conventions apply (one class per file, interfaces in `Interfaces`, no helper classes, and so on).

## Approaches considered

**How the screens are built.** (A) on the user's own UI libraries with Fluxor, adding the glass panel and player bar as new components; (B) all-new components in the media player project; (C) an outside component library. Chosen: **A**, the user's call: they already have the layout, table, dialogs and form components and want to style them, not adopt a second look and feel.

**Where "last played" is stored.** (A) a `timeLastPlayed` on the playlist, set by a dedicated endpoint and pushed like other playlist changes; (B) a separate play-history record; (C) on the device only. Chosen: **A**, the user's call, for simplicity; B is the natural next step if "recently played songs" is ever wanted; C was ruled out because the user wants it synced.

**Shuffle and repeat buttons.** A playlist has its own optional shuffle and repeat mode (null means the user's global setting). The player bar's buttons write to the playing playlist's own mode through `PATCH /Playlist/{id}`; when the source is not a playlist (the library, a search, an album) they write the global `Shuffle` and `Repeat` settings.

**Listening session and queue.** Not a saved record: a *live session* held in server memory while someone is listening (the queue as a real list, the active device and the playback state), torn down after about 10 minutes of inactivity; every device keeps a local copy of the last queue it saw to resume from. Decided 2026-11-16 in `2026-11-16-remote-playback-design.md`, which replaces the earlier "synced saved session".

## Design

### Shell

```
┌────────────────────────────────────────────────────────────────┐
│ ☰  ♪ Logo     [ Search your library …… ]            (room)     │  header
├───────────────┬────────────────────────────────────────────────┤
│ Your music    │                                                │
│  Home         │                                                │
│  Library      │          the current screen                    │
│  Account      │          (at least full height,                │
│  Settings     │           scrolls inside)                      │
│ Integrations  │                                                │
│  YouTube      │                                                │
│  Bandcamp     │                                                │
│ Playlists     │                                                │
│  [+ New]      │                                                │
│  Road trip    │                                                │
│  Focus  …     │                                                │
├───────────────┴────────────────────────────────────────────────┤
│ ⏮ ⏯ ⏭   Title – Artist   ━━━●━━━━   🔊                        │  player bar
└────────────────────────────────────────────────────────────────┘
```

- Hiding the sidebar remembers the choice per device *(my proposal)*. On a narrow window the sidebar starts hidden and opens over the centre *(my proposal)*.
- The header search searches the library; from another page it opens the Library with that text in its search box *(my proposal)*.
- The player bar shows the current media file, play, pause, previous, next, a seek bar, volume, and its state (waiting for a download, offline). Clicking the title opens the queue *(my proposal)*.

### Pages

- **Home:** "Recently played" (up to 6 playlist cards, newest first; a card plays the playlist on click and opens it from a menu) and "Continue" (one card with this device's local copy of the queue's current media file and position, shown only when no device is active and nothing is playing). Nothing played yet: the section lists the user's playlists, and with none, a prompt to create one.
- **Library:** as in the goals. The select-all tick covers the rows that match the current search *(my proposal)*; "3 selected" always shows the real count.
- **Playlist:** the name, a play button, "download all", "refresh", the duplicates setting, and the entries as a list with drag-to-reorder (the shared sortable list), a tick box per entry and the same Actions menu *(my proposal)*. Opening it downloads its media files, as the sync design says.
- **New playlist:** the button at the top of the Playlists section opens a small dialog with a name field (WebForm), then opens the new playlist.
- **Account** *(my proposal)*: the signed-in email, sign out, and storage used on this device.
- **Settings** *(my proposal)*: the theme toggle, "refresh library", "request persistent storage", what is stored on this device, and the user settings that sync with the server (the first one is "merge instead of refusing when a rename matches an existing name"; more will come, so the page lists settings in one place and is easy to extend).
- **YouTube / Bandcamp:** an empty page with a short "coming later" text.
- **Login:** as in the goals. The Google button loads Google's script and sends the ID token to the existing endpoint.

### Styling through variables

The user approved the outline on 2026-11-16 but is not sure about the colours, gaps and fonts, and wants them easy to change. So **no look is decided here**, and every visual value is a CSS custom property:

- **One file.** The App's `wwwroot/css/variables.css` defines every value once, in `:root`. Its names and groups follow `Noxy.NET.EntityManagement.Administration/wwwroot/css/variables.css`, which the shared `Noxy.NET.UI` components already read (for example `SortableTable` uses `--size-gap-*` and `--color-offset-*`): text sizes (`--size-text-*`), gaps (`--size-gap-*`), border widths and radii (`--size-border-*`), control and icon sizes (`--size-control-*`, `--size-icon-*`), page and column widths, animation lengths (`--animation-length-*`), fonts (`--font-family-primary` and one shorthand per role: label, body, button), the semantic colours (`--color-muted`, `-highlight`, `-info`, `-success`, `-warning`, `-notice`, `-error`, `-disabled`) and the light/dark pair `--color-primary` and `--color-complementary` with their `color-mix` offsets. Using the same names is what lets the shared components look right with no extra work.
- **App tokens in the same style.** What this app adds is also a variable, in the same file: the sidebar width, header height and player-bar height, the glass panel (blur, opacity, border colour), the library row height, the card sizes of the Home page, the queue panel width, and so on.
- **The rule.** Component CSS (`*.razor.css` and any page style) contains no literal colour, font family, duration or size: only `var(...)`. A literal belongs in `variables.css` and nowhere else (0 and `100%` aside). A test scans the App's style files for colour literals, `font-family` values and durations outside `variables.css`, so the rule cannot erode.
- **Changing the look** means editing that one file (or adding a theme block under `:root[theme="..."]`, as the `ThemeToggle` already switches). The defaults are the entity management project's values until the user chooses: they are placeholders, as are the mockups' colours.

### Backend additions (needed before the Home page is complete)

- **Last played.** `EntityPlaylist` gains a nullable `TimeLastPlayed`; `ResponsePlaylist` carries it; `POST /Playlist/{id}/Played` sets it to now and returns `204`. It is **not** the modified time: a play must not change the playlist's detail ETag or make devices refetch its entries. The playlist list ETag includes it, and the change is pushed as `PlaylistUpdated`. Another user's playlist is a `404`. The client calls it when playback starts from a playlist. *Built:* a second call within `Playback:PlayedWindowSeconds` (default 10) of the stored time changes and pushes nothing, so a double click or two devices count as one play; the pushed `PlaylistUpdated` carries the new stamp with the unchanged `timeModified`.
- **Live session instead of a saved session.** The earlier `GET`/`PUT`/`DELETE /PlaybackSession` endpoints, the ETag and the `PlaybackSessionUpdated` and `PlaybackSessionCleared` pushes are **removed** (nothing was deployed). The live queue, the device registry, queue edits and commands go through the hub as described in `2026-11-16-remote-playback-design.md`; the "Continue" card resumes the device's own local copy of the queue.
- **Sign-in with Google** needs only the existing endpoint plus the client IDs in the API's `Google:ClientIdList` setting (one per app: Android, Windows), which the user must create in Google Cloud Console.

### Changes to the earlier frontend design

Google sign-in moves from non-goal to goal; the metadata sync gains the last-played time (the playback session was dropped again: the live session of the remote playback design replaces it); the screens follow this design. The foundation and downloads plans cover everything below the screens; a backend plan for the two additions and a screens plan follow once this design is approved.

### Existing components to reuse (from `Noxy.NET.UI`)

The user pointed out the library has buttons and more besides the form components. Mapping of the screens to what exists (component names as in the library):

| Screen piece | Use |
|---|---|
| Shell: sidebar and centre | `SidebarLeftLayout` (and `SidebarLeftFrame`); the header and player bar are new components around it |
| Sidebar sections (Your music, Integrations, Playlists) | `Collapsible` per section |
| Buttons: New playlist, Sign in, Download all, Refresh | `Button`, grouped with `ButtonList` where they sit together |
| Actions menu (three dots) | built from `Button` plus a small menu component; new if nothing in the library fits |
| Login tabs (Sign in / Create account) | `TabSet` and `TabPage` |
| Login and every form | `WebForm`, `WebFormContext`, and the input groups (`InputGroupText`, `InputGroupPassword`, ...); the form's built-in message box shows the server's error |
| New playlist, confirm delete, save as file | `ModalDialog`, `BooleanDialog` and `DialogViewer` |
| Loading and sync-in-progress states | `Loader` |
| Offline, failed download, empty states | `MessageBox` |
| Library list | `Table` / `SortableTable` if they can do what the library needs (below), otherwise extend them |
| Playlist entries, drag to reorder | `SortableList` |
| Theme switch (Settings, header) | `ThemeToggle` |
| Icons (play, chevrons, tick, plus, saltire) | the `Noxy.NET.UI.Graphics` icons; icons that do not exist (the on-device status icons, three dots) are added there |

**To check before building the library page:** whether `SortableTable` supports clickable sortable headers with a direction indicator, a tick box column with select-all, a sticky header inside a custom panel, and 5000 rows without paging (row virtualisation). `Pagination` suggests it was built for paged data. If it cannot do these, the table is extended in the library rather than replaced, so other apps benefit.

## Left to the builder

The exact look of the glass panel and the themes, icons, spacing and animation, how Fluxor actions and reducers are named, how the drag and drop and the context menu are built, the card layout on the Home page, and how the sidebar behaves between the wide and narrow window.

## Questions that could still change the design

- Is the order of the playlists in the sidebar "recently played first", or alphabetical, or the user's own order?
- What should the Account and Settings pages hold beyond what I proposed?
- Should the Continue card appear on a device that is currently playing something else?
