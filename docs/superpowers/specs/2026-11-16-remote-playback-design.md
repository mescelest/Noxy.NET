# Remote Playback and the Live Queue Design

Status: draft for approval, 2026-11-16 (second version; it replaces the first, which stored a seeded recipe on the server). Approach chosen by the user: the server relay on the existing hub, with the queue held as a real list in a live session in server memory.

Built (frontend, 2026-11-17, plan `2026-11-16-frontend-remote-playback.md`): the device identity and the local queue store (SQLite, one row), `QueueService` (the device's copy of the queue, shared `LiveQueue`), `QueueSourceService` and `ShuffleService` (building a source), the player on the queue, `RemotePlaybackService` (devices, remote state, role), `PlaybackReporterService` (reports, claims, local copy position), `RemoteCommandHandlerService` (commands, transfer) and `PlaybackControlService` (what every screen calls). Where the build differs from this doc: a device that registers claims again when it plays (also when registering says it is the active one, because its queue may have moved on while it was away); a snapshot that merely lacks the active device's current item is stale and keeps the device's pointer (only a snapshot with no current item means "removed, skip"); a controller adopts the volume of each state push while the active device never does; a device that loses the active role while playing stops; `Transfer` to the device's own ID is a no-op except for a controller (that is "play here"); `CanContinue` also holds for a device that is still the session's active device after a restart.

Built (backend, 2026-11-16): the live session, device registry, queue edits, commands and transfer are implemented on `MediaHub` (plan `2026-11-16-backend-remote-playback.md`). Hub methods (client to server, `MediaHubMethodEnum`): `RegisterDevice`, `ClaimActive`, `ReportState`, `EditQueue`, `RequestSnapshot`, `SendCommand`, `Transfer`. Events (server to client, `MediaHubEventEnum`): `DeviceListChanged`, `PlayerStateChanged`, `QueueChanged`, `QueueSnapshot`, `SessionEnded`, `TransferFailed`, `PlayerCommandReceived`, `PlayerTransferRequested`, `PlayerStartRequested` (state and queue as two arguments). Messages are camelCase JSON, enums by name. `Transfer` returns true once the transfer is under way and finishes in the background; its failure is pushed as `TransferFailed`.

## Intent

The user wants one device to do the playing and every other signed-in device to see what it plays and control it, the way Spotify does: a laptop wired to a speaker, controlled from a phone. The queue is a real list that any device can edit (add, remove, play next, add to queue, reorder) and that follows the music from device to device while the user is listening. Nothing about it is stored on the server for good: a **live session** lives in server memory while there is listening, is torn down after a period of inactivity, and each device keeps its own local copy of the queue. Playback itself must never depend on the server: every device can always play its own downloaded media files alone. The server only coordinates.

## Goals

- At most one **active device** per account at a time: the device that makes the sound.
- Every other connected device shows what the active device plays (the queue, the current item, playing or paused, position, volume, shuffle, repeat) and can send it commands and queue edits.
- The queue is an explicit list, edited by item: **remove, play next, add to queue, reorder, replace by a new source**, from any connected device.
- The user can change which device is active (a device picker); the queue and position carry over.
- Updates are **event driven**: a change is sent when it happens (play, pause, a different item, a seek, a volume change, a queue edit, a shuffle or repeat change); controllers show a moving seek bar by calculation.
- A live session exists only while someone is listening; when it ends, every device continues from its own local copy.

## Non-goals

- A live position stream, or any audio streaming between devices.
- Direct device to device control on the local network, waking a sleeping device.
- Several devices playing at once; controlling another account's devices.
- Keeping a queue on the server after the live session ends, or resuming it on a device that never saw it.

## Anti-goals (a failure even if it technically works)

- Playback that needs the server, or stops when the connection drops.
- Two devices making sound at once for the same account.
- A device shown as playing after it has died; a session torn down while something is playing.
- A command or edit that silently does nothing: the controller must see the new state or that the session or device is gone.

## Constraints

- Builds on the existing SignalR hub (`MediaHub`, currently push-only) and the frontend player. Conventions of the earlier plans apply (no magic strings for hub names, enums named `...Enum`, "media" never "song").
- The player plays only from the device's local store and waits on a download when a media file is missing: a device that becomes active without the files downloads them first.
- The server never reads the media library to build a queue: the **active device** builds it from its own synced library and playlists and sends it.

## Approaches considered

1. **Server relay on the existing hub, live queue in server memory (chosen).** The server holds one live session per account (the queue list, the active device and the playback state), applies queue edits in the order they arrive and pushes the result to every device. Edits name items by ID, so two devices editing at once cannot clash.
2. **The first version of this design: a small seeded recipe saved on the server and regenerated on every device.** Rejected by the user as over-complex: it needed deterministic shuffle versioning, dead-session rules and separate edit lists, and could not support reordering.
3. **Direct device to device on the local network.** Needs discovery and its own security, cannot work over mobile data. Rejected.

## The design

### Devices and the registry

- Each installation has a stable `DeviceID` (a GUID in its own storage) and a name the user sets once (default: the machine name).
- On connecting to the hub a device registers (`DeviceID`, name). The server keeps, in memory and per account, the online devices and at most one **live session**. A device that reconnects replaces its own earlier connection. The device list is pushed to the account's devices whenever it changes.

### The live session (server memory only)

- **Contents:** the **queue** (below), the `ActiveDeviceID`, the source the queue came from (the Library with its sort, direction and search, or a playlist ID), the playback state (`IsPlaying`, `PositionSeconds`, a time stamp, `Volume`, the effective shuffle and repeat, the current item's title and artist), a `QueueRevision` that the server increases on every queue change and a `StateRevision` that the active device increases on every report.
- **The queue is an explicit list of items** `(ItemID, MediaFileID)` laid out as: **history** (items already played, newest last, at most `HistoryLimit`, so Previous works), the **current item**, the **user queue** (items the user added), the **source part** (the rest of the source, in its play order). The user queue plays before the source part resumes.
- **Edits (server applies them in arrival order, by `ItemID`):** `PlayNext(mediaFileID)` puts the item at the front of the user queue; `AddToQueue(mediaFileID)` at the end of the user queue; `Remove(itemID)` (removing the playing item means "skip": the active device moves on at once and the item never enters history); `Move(itemID, afterItemID)` (reordering inside the upcoming part; moving a source item into the user queue makes it a user-queue item); `ReplaceSource(items, source, currentItemID)` (a new source: see below); `InsertSourceItem(item, afterItemID)` and `RemoveSourceItem(itemID)` (the source changed underneath, see below); `Advance(item)` and `Previous(item)` (one step back in the normal queue: the song left goes to the front of the source part, so Next still plays the user queue first; the active device reports the item it is now playing; the server accepts it even if a controller removed it a moment earlier, so there is no error and the result does not depend on arrival order; naming the item also allows a jump: the songs skipped over in the same part are dropped by default and never enter history; with `KeepSkipped` true (a click in the queue panel, which always passes `true`) they move to history in list order after the song left, and a jump back with `KeepSkipped` puts the songs between at the front of the source part; the other part is untouched; a `JumpTo(item, keepSkipped)` command carries this from a controller). An edit that names an item that is gone is a no-op, not an error.
- **The server only keeps lists and applies these edits.** It never builds a queue: when a new source starts, the device that starts it (or, when a controller starts it, the active device on its behalf) builds the list from its local library and sends `ReplaceSource`.
- **Size:** a queue can hold thousands of items. Pushes after the first are **deltas** (the edit and the new `QueueRevision`); a device that misses a revision, or joins, asks for a full snapshot.
- **Torn down** (see Teardown): the session disappears from memory; nothing about it is stored on the server.

### Starting, shuffle, repeat and the source part

- **Starting a source** (pressing play on a playlist or on a Library row): the queue is replaced: the source part is the whole source in its play order, starting from the item pressed. With shuffle on, the source part is the source in a shuffled order (the item pressed first, then the shuffled rest; the shuffle is random, made by the building device, and needs no seed because only that device builds it). History is cleared.
- **Turning shuffle on or off** regenerates only the source part, from the current item: shuffled when on, in the source's normal order starting after the current item when off. The user queue is not touched.
- **Repeat** applies to the source part: when it runs out, `No` stops, `Once` appends the source once more (regenerated, so a new shuffle), `Forever` appends it each time it ends. The user queue is played once and consumed.
- **The source changes underneath** (a media file uploaded or deleted, a playlist edited): the active device, which holds the synced library, sends `InsertSourceItem` where the source's order puts the new item (after the current item for a sorted Library; at a random place after the current item when shuffled) or `RemoveSourceItem`. A deleted item that is playing finishes first, then is dropped (so Previous goes to the item before it). A deleted playlist lets the current item finish and ends the source part.
- **Effective shuffle and repeat** are the playing playlist's own when set, else the user's settings (as in the settings design).

### The `KeepQueuedItems` setting

A new synced user setting, `KeepQueuedItems` (boolean, default false). When a **different source starts**: off: the user queue is cleared with everything else; on: the user queue is kept, in its order, straight after the new current item, ahead of the new source part. Shuffle toggles and source-changes-underneath never touch the user queue, whatever the setting says.

### Who plays, and starting playback

- **The active device keeps playing** until the user changes device with the picker, or until it goes offline. Nothing else stops it.
- **Pressing play on a device** while a live session has an online active device: the command (start this source, edits, pause, seek and the rest) goes to the active device; the sound stays where it is. The picker is how the sound moves.
- **No live session** (or no online active device): the device you pressed on starts a new live session with the server and becomes the active device. "Continue" is this: the device resumes its own local copy of the queue as a new session.
- **Device picker / transfer:** the server asks the active device for a final state (pause and report), then tells the target to start from that queue and position; the target becomes active when it starts. If the old device does not answer within a timeout, the last reported state is used, advanced by the elapsed time when it was playing. A failed transfer leaves the old device active and tells the controllers. The target downloads missing files first.
- **Controllers** can send: play, pause, seek, next, previous, volume, shuffle, repeat, every queue edit above, start a source, and transfer.

### Offline and reconnecting

- Playback never needs the server. An offline device plays alone from its local copy of the queue and keeps advancing it locally; its picker and remote controls are disabled with the reason "offline".
- When the **active device goes offline**, after a short grace period the session is marked not playing (controllers show "device offline"), and the idle countdown starts. If it reconnects in time and nobody else took over, it is the active device again and reports its state; its queue, advanced while offline, replaces the session's copy.
- **Reconnecting while playing locally** (the user's rule): if another online device is the active device and is playing, that one wins: the reconnecting device pauses and becomes a controller, showing the winner's queue. Otherwise the reconnecting device becomes (or stays) the active device, and its local queue becomes the session's (a new session when the old one was torn down).
- A device that starts playing while nothing is active and it is connected opens a session and becomes active.

### Teardown

- A live session is torn down after `IdleTeardownMinutes` (10) of inactivity. **Inactivity** means: nothing playing (paused, stopped, or the active device offline past its grace period) and no command, queue edit or report for that long. The countdown never runs while the active device is online and playing, and any command, edit or report resets it.
- On teardown every connected device is told (`SessionEnded`); each keeps its **local copy** of the queue (the last it saw), the current item and the position, to resume from. The server keeps nothing.
- A server restart ends every live session the same way; devices keep their local copies and open new sessions when they next play.

### Local copy on each device

Every device stores, as it receives them, the queue, the current item, the position, the source and the modes, so it can resume alone and so a session can be rebuilt from it. This replaces the saved session record on the server and its endpoints (`GET`, `PUT`, `DELETE /PlaybackSession`, the ETag and the `PlaybackSessionUpdated` and `PlaybackSessionCleared` pushes); nothing was deployed, so they are removed, not migrated.

### Volume

Each device keeps its own volume locally (it survives a restart), but while a session is live the volume is **shared by the session**, and the session's value is the one that counts. The active device reports its volume in every state report, and the server pushes it to every device. Rules (decided by the user, 2026-11-16): a device that connects or reconnects must never change the sound of something already playing, so it sends no volume of its own unless it becomes the active device; it adopts the session's volume instead (from the state the server sends on register, and from every later state push). A volume change on a controller is sent to the server and forwarded to the active device, which applies it and reports it; the report reaches every connected device, and each one applies it to its own player and stores it locally, so all devices follow. A device that becomes active through a transfer plays at the session's volume (the start request carries the state). With no live session a device just uses its own stored volume. It is not a synced account setting.

## Left to the builder

- The hub method, event and edit names (constants shared by server and client, no magic strings) and the message shapes; delta versus snapshot details and the resync on a missed revision.
- `HistoryLimit` (suggested 50), the grace period for a dropped active device (suggested 10 seconds), the transfer timeout, the deferral of seek and volume on the controller (suggested 300 ms), `IdleTeardownMinutes` = 10, and a cap on queue length for safety.
- Where the registry and live sessions live in the server (a singleton service tested without a hub) and how the idle timer uses `TimeProvider`.
- The controller user interface (device picker, queue panel with reorder, "playing on X", "device offline", "Play here"), in the screens plan.

## Test notes

Unit tests with fakes for the queue edits (each edit, an edit naming a gone item, concurrent edits by `ItemID`), shuffle regeneration, the user queue with and without `KeepQueuedItems`, source changes underneath, the reconnect rule, teardown (the clock advanced with `FakeTimeProvider`: never while playing, reset by an edit, active-device-offline starts it) and transfer; an integration flow with two real hub clients on one account and one on another (an edit on one reaches the other, a command reaches the active device only, transfer, reconnect, teardown and each device keeping its local copy).
