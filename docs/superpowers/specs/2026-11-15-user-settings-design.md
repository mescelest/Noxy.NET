# User settings: typed, one value per setting

Status: draft for the user's approval, written 2026-11-15 from the brainstorming conversation. Backend only. It replaces the "User settings" section of `2026-11-15-metadata-entities-design.md` and the settings record that was built from it (`EntityUserSettings`, a single row with a boolean column per setting). Lines marked *(my call)* are decisions I made without asking, so check them.

## Intent

Users need a few settings that follow them from device to device. Today there is one (merge on rename) and more are coming of different kinds, so adding a setting must not mean a migration and a new column each time. The user also wants the stored values strongly typed: no JSON blob, and no table where several nullable columns exist and only one is ever filled in, which makes finding the value a guessing game.

## Goals

- **Four settings to ship with:** `Shuffle` (enum: `Off`, `Random`, `SpreadArtists`), `Repeat` (enum: `No`, `Once`, `Forever`), `MergeOnRename` (boolean) and `KeepQueuedItems` (boolean, default false: keep the user queue when a different source starts, see `2026-11-16-remote-playback-design.md`). The user wants several ways to shuffle: completely random, or something smarter such as avoiding media from the same artist in a row. The server only stores which mode was chosen; the shuffling itself is done on the device, so a new mode is one new enum member with no migration. *(My call: the member names, and shipping the two modes the user named. A mode that adds media the user did not choose is deliberately not included.)* *(Volume was a fifth setting until 2026-11-16; it is per device now, see the remote playback design.)*
- **Strongly typed storage:** an abstract user-setting base (user, key, modified time) with a derived table per value type, each holding one non-null `Value`: boolean, integer, decimal, string and enum. The enum table stores the enum member's name, so reordering an enum later cannot change saved values.
- **Every setting declared once in code:** key, value type, default, and its limits (a range for a number, the member list for an enum). The server only accepts a value that matches the declaration, and code reads a setting as its real type, not as a key lookup.
- **Defaults are not stored.** A user with no saved row for a setting gets the declared default. The row is created the first time the setting is changed. Nothing is created when a user is created.
- **Synced like the rest:** `GET /Settings` has a strong ETag and `304`, and a change is pushed to every device of the user.
- **Merge on rename keeps working:** the rename handlers read `MergeOnRename` from these settings (server side, as built).

## Non-goals

- Shuffle and repeat **per playlist**: built. Two nullable columns on the playlist (`Shuffle`, `Repeat`), null meaning "use the global setting"; `PATCH /Playlist/{id}` sets them (a left-out field is untouched, an explicit `null` clears the mode, the same value changes nothing). A real change moves the playlist's modified time, so the list and detail ETags change and other devices download the entries again: accepted, mode changes are rare.
- The shuffle algorithms themselves (frontend work), and any mode that picks media for the user.
- Equalizer, crossfade, shortcuts and any structured value: left for later. The design leaves room (a new derived type) but builds none.
- Device-local settings (storage path, watched folders, theme): they never reach the server.
- API tokens for integrations: their own feature (a list the user creates and revokes, stored hashed).
- Resetting a setting to its default (deleting its row), and settings for other users: not needed now.

## Anti-goals

- A migration or a new column for each new setting.
- A saved value that does not match its declaration (wrong type, out of range, an enum name that does not exist).
- One device overwriting another's settings because they were saved together: each setting is saved on its own.
- Another user's settings reachable in any way: the user always comes from the token.
- User creation (sign-up, Google sign-in) gaining any dependency on settings.

## Constraints

- The sync design stays: strong ETags, `304`, a hub push per change, ownership from the token.
- Repository conventions apply (one class per file, `Entity` and `Table` prefixes, a command or query and a handler per use case, kind-first names, lists end in `List`, no magic strings: setting keys are defined once and shared).
- Nothing is deployed (the user confirmed it is all still in development), so the migrations that came with the metadata work (`AddMetadata`, `AddUploadJobTitleWasSent`) are regenerated as one migration that includes the new settings tables, instead of adding another on top. Earlier migrations stay as they are.

## Approaches

- **A. One table, one row per setting, nullable typed columns** (boolean, integer, decimal, string). One migration, no per-type tables, but only one of the columns is filled in per row, which the user does not want. **Not chosen.**
- **B. A derived table per value type under an abstract base, as in EntityManagement.** Strongly typed, nothing nullable, and EF reads all of a user's settings as one set through the base type. EntityManagement needs this because its schema is user-defined at runtime; here the declarations are in code, so what B adds is the typing. **Chosen by the user.**
- **C. A column per setting on one row** (what was built). Easy to read, but a migration for every setting. **Not chosen.**

## Design

### Declarations

A setting declaration names the key, the value type, the default and the limits. The five above are declared in one place; the server validates against it and returns it with the value. An unknown key is a `404`; a value of the wrong type, out of range, or an enum name that is not a member is a `400`.

### Data

- `EntityUserSetting` (abstract): ID, user, key, modified time. A unique index on (user, key).
- Derived: `EntityUserSettingBoolean`, `…Integer`, `…Decimal`, `…String`, `…Enum`, each with one `Value`. Only the types the five settings need are built now (boolean, integer, enum); decimal and string are added when a setting needs them *(my call, to avoid tables nothing uses)*.

### Endpoints

- `GET /Settings`: every declared setting with its current value, the default where nothing is saved. Strong ETag over the saved rows' IDs and modified times; `304` on a match.
- `PATCH /Settings/{key}`: body `{ "value": … }`. Changing nothing (the value equals the current one, including the default for a user with no row) saves and pushes nothing. The first change creates the row.
- Hub: `SettingUpdated` carries the one changed setting. The earlier `SettingsUpdated` event is removed.
- Responses carry key, type, value and modified time; a default has the epoch as its modified time.

### Replacing what was built

`EntityUserSettings`, its table, repository, request, handlers, controller, the `SettingsUpdated` event and their tests are replaced by the above, with no compatibility layer, since nothing is deployed. The rename handlers (artist, genre, album) keep reading merge-on-rename, now through the new store.

### Tests

Unit tests with fakes for each handler (defaults, first change creates, unchanged saves nothing, validation per type, unknown key, push, ownership), a test that every declaration is valid (its default is accepted by its own limits), repository integration tests (the unique key, ownership, each derived type round-trips), and end-to-end tests through HTTP and the hub, including a rename that merges because of the setting.

## Left to the builder

The class names of the declarations and how the catalog is exposed to the handlers, how EF maps the inheritance (table per type, as EntityManagement does, or table per concrete type), the exact response shape, how a first-change race on the unique (user, key) index is retried (the metadata work already has a retry for key conflicts), and the migration.

## Questions that could still change the design

None open.
