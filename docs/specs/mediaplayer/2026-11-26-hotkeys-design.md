# Hotkeys and media keys

## Intent

The user wants to control the music without reaching for the mouse. With the window focused and nothing focused in it, **space** starts or pauses the music. The **media keys** of a keyboard (play/pause, next, previous) control the player from everywhere, also while the app is in the background and the user works in another window.

## Goals

- **Space** toggles play and pause when the window has the focus and nothing in it is focused.
- **Media keys** (play/pause, next, previous) control the player system-wide, also with the app in the background and with nothing loaded yet (right after start-up with a saved queue).
- Space, the media keys and the player bar's play/pause button all use one toggle, so they behave the same, including when this device controls another device.
- The Windows media overlay shows the current title and artist and the play state.
- All key bindings are declared in one list, so a hotkeys guide in Settings can show them later without searching through components.

## Non-goals

- The hotkeys guide in Settings. Only the list it will read is part of this design.
- Other hotkeys (seek, volume, shuffle) and stop as a media key.
- Android: headset and Bluetooth buttons need a media session and are their own work.
- Global hotkeys that take keys from other apps (`RegisterHotKey`).
- Changing what ctrl-click does.

## Anti-goals

- A space press that acts while the user types in a field, has a dialog or menu open, or has a button focused (that button handles space itself).
- Media keys that stop working because no file is loaded, or that start two things at once because two parts of the app listen for them.
- A key press that is handled and still scrolls the page, or one that is not handled and is swallowed.

## Constraints

- Playback goes through `IPlaybackControlService`, which already decides between playing here and sending the command to the active device when this device is a controller.
- The Windows player is the system `MediaPlayer`, created for each loaded file and released on stop. The app is unpackaged (`WindowsPackageType` None).
- The Presentation project has no platform types. The Windows pieces live in the App project under `Platforms/Windows`.
- Repository conventions apply (one class per file, no magic strings, interfaces in `Interfaces`, enum names end in `Enum`, texts in `AppTexts` or `PresentationTexts`).

## Approaches

- **A. The player's own media controls.** Least code, but it only exists while a file is loaded. At start-up, or after the file is released, the keys go to another app.
- **B. One media-control hook for the window (chosen).** The Windows system media transport controls (SMTC) are tied to the app's window, so the keys always reach the app and Windows shows the song in its overlay. It needs Windows-only code to get the window handle and to keep the state in sync.
- **C. Global hotkeys with `RegisterHotKey`.** Simple, but it takes the keys from every other app and shows no overlay. Rejected.

## Design

### The toggle

- `IPlaybackControlService` gets `TogglePlay()`: pause when the player that matters is playing, otherwise play. For a controller, the player that matters is the active device (its reported state); otherwise this device's own player. When nothing is playing and a saved queue can be continued (`CanContinue`), it continues that queue, as the Continue card does *(my call, so a media key right after start-up does something useful)*. It is what the player bar button calls instead of choosing between `Play` and `Pause` itself.

### The bindings

- A declared list of hotkeys (like `SettingCatalog`): the key as the user presses it, the action (`HotkeyActionEnum`: `PlayPause`, `Next`, `Previous`) and the scope (`HotkeyScopeEnum`: `Window`, `System`). The entries: Space, window, play/pause; the play/pause media key, system; the next media key, system; the previous media key, system. A handler in Presentation maps an action to the `IPlaybackControlService` call. The list holds no texts yet; the guide adds them.

### Space

- A small script in the page (a module like `CtrlKeyWatcher`, started from the main layout) listens for key presses on the window and calls the app for the action. It handles the key only when all of this holds: the key is space; no ctrl, alt, meta or shift; the press is not a key repeat; nothing is focused (the active element is the page body); no dialog or menu is open. Then it prevents the default action (the page scroll) and calls the app. Anything else is left alone.

### Media keys (Windows)

- A contract in Presentation, `IMediaKeyService`, raises `PlayPausePressed`, `NextPressed` and `PreviousPressed` and takes the state to show (playing or not, title, artist). The handler subscribes, maps to the bindings list and calls the control service. Where no implementation is registered (Android), nothing is raised.
- The Windows implementation (App, `Platforms/Windows`) uses the system media transport controls of the app window (the interop for a window handle, so it works unpackaged). It is enabled for the life of the app, whatever is loaded. It enables the play, pause, next and previous buttons, raises the three events when a button is pressed (these arrive off the UI thread), and updates the playback status and the music properties (title, artist) of the overlay.
- The `MediaPlayer` the Windows audio player creates for each file has its own command manager switched off, so only the window-level controls receive the keys.
- The overlay shows what the player bar shows (the same title, artist and play state, including what the active device reports on a controller).

### Tests

- Presentation: `TogglePlay` (local playing pauses, paused plays, a controller with the active device playing sends pause, with it paused sends play); the handler maps each media-key event and the space action to the right call; the bindings list has no duplicate key.
- By hand on Windows (the user runs them): space with nothing focused, in a text field, with a dialog open, with a button focused; the three media keys with the app focused, in the background, right after start-up with nothing loaded, and while controlling another device; the overlay text; that no other app also reacts to the key.

## Left to the builder

- How the window handle is obtained for the interop, and how the state reaches the Windows implementation so that the overlay and the player bar do not compute it twice.
- The exact shape of the script and how it asks whether a dialog or menu is open.
- Whether the overlay also shows the album picture.
- How Android satisfies the contract (no registration, or an implementation that raises nothing).
- A short test on a real Windows machine that the overlay and the keys behave for an unpackaged app.
