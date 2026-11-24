# UI guide: restyling the MediaPlayer screens

Everything below is wired and tested. The components in `Noxy.NET.MediaPlayer.App/Components` only read state and call services, so you can rewrite their markup and CSS freely.

## Styling

- All values live in `App/wwwroot/css/variables.css` (colours, gaps, fonts, sizes, glass, table columns) and `breakpoints.css` (the one place the narrow-window condition is written, plus `--layout-narrow`).
- Component CSS (`*.razor.css`) may only use `var(--…)`. `StyleRuleTest` fails the unit tests on a literal or an undefined variable, so a new variable goes into `variables.css` first.
- Theme: `theme="Light|Dark"` on the root element swaps `--color-primary` and `--color-complementary`.

## Components (all in `App/Components`)

| File | What it is | Reads / calls |
|---|---|---|
| `Layout/MainLayout` | shell: header, sidebar, centre, player bar; shows `Login` when signed out | `FeatureShellState`, `FeatureSessionState` |
| `Layout/Header`, `Sidebar` | hamburger, search, offline note, sign out; sections and playlist links | `FeatureLibraryState.Search`, `FeaturePlaylistListState` |
| `Layout/PlayerBar`, `QueuePanel`, `DevicePicker` | transport, seek, volume, modes, queue, devices | `FeaturePlayerState`, `IPlaybackControlService`, `IRemotePlaybackService`, `IQueueService` |
| `Pages/Login` | sign in, create account, Google, server address | `ISessionService`, `IServerAddressStorage`, `IAppStartupService` |
| `Pages/Home`, `PlaylistCard`, `ContinueCard` | recent playlists, Continue | `IHomeService`, `IPlaylistService`, `IPlaybackControlService` |
| `Pages/Library`, `ActionsMenu`, `ConfirmActionDialog`, `FileStatusIcon`, `GlassPanel` | library table and bulk actions | `FeatureLibraryState`, `ILibraryActionService` |
| `Pages/Playlist`, `NewPlaylistDialog` | playlist detail, reorder, modes | `IPlaylistService`, `IMetadataStore` |
| `Pages/Account`, `Settings`, `DeviceNameField` | account, theme, synced settings | `ISessionService`, `ISettingService`, `IDeviceService` |
| `Pages/Youtube`, `Bandcamp` | placeholders | none |

## State you can bind to (Fluxor, `Presentation/Features`)

`FeatureLibraryState` (media, visible list, search, sort, selection, file states), `FeaturePlaylistListState`, `FeaturePlayerState.Snapshot`, `FeatureSessionState` (signed in, offline, retrying, restored), `FeatureDownloadState` (bulk progress), `FeatureShellState` (sidebar). Inherit `FluxorComponent`, inject `IState<T>`, dispatch the `Action…` records. Reducers are unit tested.

## Services (`Presentation/Interfaces/Services`)

- Play and queue: `IPlaybackControlService` (every play command, queue edits, transfer, continue). Sends to the active device when this one is a controller.
- Bulk actions: `ILibraryActionService` (download, play next, add to queue, save as file, add to playlist, remove from device, delete from library, plus `GetDisabledReason`).
- Playlists: `IPlaylistService`. Home: `IHomeService`. Settings: `ISettingService`. Devices: `IDeviceService`, `IRemotePlaybackService`.
- Errors: `exception.ToActionMessage()` / `ToSignInMessage()` give text that is safe to show.

## Where to look when something is missing

- A new piece of screen state: add a `Feature…State` plus actions and a reducer, then dispatch it from `StateBridgeService`.
- A new bulk action: add it to `LibraryActionEnum`, `ILibraryActionService` and `ActionsMenu`'s item list (one entry).
- A new sign-in provider: one more entry in the provider list in `Login.razor`.

## Not built

Android, browse by genre/artist/album, cover art, the YouTube and Bandcamp integrations. `UI` libraries (`Noxy.NET.UI.*`) are shared with the entity management apps: changes to `Table`, `Collapsible` and the others affect those too.
