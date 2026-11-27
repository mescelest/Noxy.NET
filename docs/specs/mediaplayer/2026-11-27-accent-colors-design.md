# Accent colors, and a Home page for no playlists

## Intent

The app is mostly one color: the highlight (blue). The user wants a few alternatives to pick from, and wants the app to stay mostly that one color. Red and orange stay the serious-button colors. The green on add/save buttons goes, so the rest of the app follows the highlight. The Home page is a dead end for a new user with no playlists, so it should offer the button to make one.

## Goals

- The user picks an accent in Settings. It replaces the highlight everywhere the highlight shows (buttons, focus ring, ticked boxes, seek bar, volume, the chosen theme button).
- Blue stays the default.
- Every green (success) button becomes a highlight button.
- Home with no playlists shows an empty state with a "New playlist" button in place of an info message.

## Non-goals

- No syncing of the accent across devices.
- No tinting of surfaces, text or the page.
- No change to warning or error buttons.
- No accents in the red/orange range (they would make Save and Delete look the same).

## Approaches

1. **A second root attribute that overrides only `--color-highlight` and `--color-on-highlight`** (chosen). The shades (hover, active, subtle, border) and `--color-theme` are already worked out from the highlight, so everything follows. No component needs rewriting.
2. An extra `--color-accent` layer with highlight pointing at it. Indirection for no gain.
3. A full palette per accent. Overkill for an app that should stay one color.

## Design

**Accent.** `accent="Purple"` (and so on) on the root element, next to `theme`. Each accent sets `--color-highlight` and `--color-on-highlight` as `light-dark(a, b)` pairs. No attribute means blue. Set: Blue (default), Purple, Magenta, Green, Teal. Exact values are the builder's, with the same bar as the status colors in `variables.css`: 4.5:1 between fill and "on" color, 3:1 between fill and page, in both themes.

**Storage and picker.** Per device in local storage, applied before first paint like the theme. The Settings picker is a row of swatches beside the theme picker, behaving like it. The theme code in `Noxy.NET.UI.Shared` (`ThemeToggle.razor.js`, `ThemeService`) hardcodes the `theme` attribute and key; the builder decides whether to generalize it or add a small app-side service next to it.

**Success to highlight.**
- Buttons: `MetadataItemDialog`, `MetadataSetDialog` (two), `ServerManageDialog`, `FormEditMedia`, `FormEditPlaylist`, `FormNewPlaylist`, `MetadataItemList`, `SidebarElement` all change from `StatusEnum.Success` to `StatusEnum.Highlight`.
- CSS uses of `--color-success`: `FileStatusIcon`, `HeaderElement`, `DevicePicker`, `app.css` (outline). Check each one: if it means "ok/connected", it is a real success and may stay green; if it is decoration, it moves to highlight.

**Home, no playlists.** The "Recently played" heading and the info message go. In their place: a centered empty state with an icon, a short line (the existing `HomeNoPlaylists` text may be reworded) and a "New playlist" highlight button that opens `NewPlaylistDialog` (the sidebar uses the same dialog with `@bind-Open`). The `Continue` section is unchanged.

```
 Home
 ──────────────────────────
 [ Continue card, if any ]

        ♫
   No playlists yet
   [ + New playlist ]
```

## Calls I made (not yet agreed)

- Per-device storage, not synced.
- Swatch row as the picker.
- Accent set: Blue, Purple, Magenta, Green, Teal (no red or orange).
- All success buttons become highlight, Save and Create included.
- Heading dropped in the empty state.

## Left to the builder

- Exact color values.
- Whether the shared theme code is generalized.
- Wording of the new texts (`AppTexts` resources).
- That the Home list updates once a playlist is created (it reloads on `ServiceSync.Changed`; confirm the create path raises it).
