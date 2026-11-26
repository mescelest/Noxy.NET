# Add to playlist dialog and playlist graphic

## Intent

The "Add to playlist" dialog is a plain list of text buttons, copied into the library and queue menus, and it does not look like the rest of the app. Playlists also have no graphic.

## Design

- **`AddToPlaylistDialog`** (App, `Components/Dialogs`), one component replacing the two copies in `LibraryContextMenu` and `QueueContextMenu`. Parameters: `Open`/`OpenChanged`, and `OnChosen` (`EventCallback<Guid>`, the playlist ID). It closes on the normal close behaviour; there is no Cancel button.
- **Order and search:** every playlist (no cap, unlike Home), in the Home order (`OrderMostRecentFirst`: the later of last played and created). A search field of the same kind as the Library page (`InputControlText`, `Type=Search`, instant, with its X) filters by name, ignoring case. No playlists and no match have their own text.
- **Height:** the list scrolls inside the dialog, which is capped at `--size-dialog-list-height` (60% of the window height).
- **Choosing:** a click adds the songs and closes the dialog, as before.
- **`PlaylistArt`** (App, `Components`): a small rounded square with a music icon, the same for every use. It takes a playlist. Each playlist gets its own hue, worked out from its ID, so rows can be told apart; a real image later would change only this component. Shown in the sidebar before the name and in the dialog rows.

## Non-goals

Real images or uploads; a graphic on Home, the playlist page or the sub-menu items.

## Calls to check

- The per-playlist colour is an experiment the user wants to see before deciding ("let's try it").
