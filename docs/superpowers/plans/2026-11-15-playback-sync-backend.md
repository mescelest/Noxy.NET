# Last Played and Saved Playback Session Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:executing-plans (the user wants this done natively, in the main session, not through subagents). Steps use checkbox (`- [ ]`) syntax for tracking. It can run before or after `2026-11-15-metadata-entities.md` (they touch different files except the migration order and `ResponseMediaFile`-free code); run the two migrations one after the other, never in parallel.

**Goal:** Let devices share what the user was listening to: a `timeLastPlayed` on each playlist for the Home page's "Recently played", and one saved playback session per user (queue, current media file, position) for its "Continue" card, both synced through the existing list, ETag and hub-push pattern.

**Architecture:** Same shape as the earlier backend plans: entities in `Domain`, tables and repositories in `Persistence`, commands, queries, handlers and notifications in `Application`, requests with `ToCommand` in `Application/Requests`, controller actions and hub handlers in `API`. Last played is a stamp apart from `TimeModified` on purpose, so a play never changes a playlist's entries ETag or makes a device refetch them.

**Tech Stack:** .NET 10, ASP.NET Core, EF Core with SQLite, `Mediator` 3.0.2, xunit v3.

**Spec:** `docs/superpowers/specs/2026-11-15-frontend-screens-design.md`, "Backend additions" (approved 2026-11-15).

## Global Constraints

- Work only in `Noxy.NET.MediaPlayer` and `Noxy.NET.Test`. Target `net10.0`. No frontend work.
- All conventions of `2026-11-15-metadata-entities.md` apply (one class per file, kind-first names, `Entity` and `Table` prefixes, interfaces mirroring folders, no helper classes, handlers are singletons with one commit per use case and publish after it, `UserControllerBase`, request `ToCommand` methods, tests and the no-commit and no-machine-command rules, zero warnings). New names: `CommandPlaylistPlayed`, `CommandPlaybackSessionSave`, `CommandPlaybackSessionClear`, `QueryPlaybackSession`, `HandlerPlaybackSessionSave`, `NotificationPlaybackSessionUpdated`, `NotificationPlaybackSessionCleared`, `RequestSavePlaybackSession`, `ResponsePlaybackSession`, `EntityPlaybackSession`, `PlaybackSessionController`, hub events `PlaybackSessionUpdated` and `PlaybackSessionCleared`. The migration command is the project command given in that plan.
- Another user's playlist or media file is always a `404`; the user comes from the token.
- Existing playlist behaviour does not change: the playlist detail ETag, the entries, `TimeModified`, and every existing test stay as they are.

## Review Focus

- **A play must not move the entries ETag or `TimeModified`** (a play pushes only the summary with the new stamp), while the playlist **list** ETag must change (Tasks 1 and 2).
- **A session never holds another user's media file, and never keeps a deleted one:** deleting a media file removes it from the session's queue in the same commit, adjusts or clears the current index, and pushes the session (Task 4).
- **Two devices saving at once:** the last write wins, `GET` always returns the newest, and neither save is lost or half-applied (Task 4).
- **Malformed saves:** an index outside the queue, a negative position, an over-long queue and duplicate media files in a queue (allowed) each have a defined result (Task 3).
- **A burst of play calls** (a double click, two devices) does not write or push more than once per window (Task 2).

## File Structure

New files (relative to `Noxy.NET.MediaPlayer.<Layer>/`; tests relative to `Noxy.NET.Test.<UnitTest|IntegrationTest>/MediaPlayer/`):
- Domain: `Entities/EntityPlaybackSession.cs`, `Responses/Playback/ResponsePlaybackSession.cs`
- Persistence: `Tables/Playback/TablePlaybackSession.cs`, its type configuration, `Repositories/PlaybackRepository.cs`, one migration per task that changes the schema
- Application: `Interfaces/Repositories/IPlaybackRepository.cs`; `Commands/Playlist/CommandPlaylistPlayed.cs`; `Commands/Playback/…`, `Queries/Playback/QueryPlaybackSession.cs`, `Handlers/Playback/…`, `Handlers/Playlist/HandlerPlaylistPlayed.cs`, `Notifications/Playback/…`, `Requests/Playback/RequestSavePlaybackSession.cs`, `Configurations/ConfigPlayback.cs`
- API: `Controllers/PlaybackSessionController.cs`, notification handlers in `API/Handlers`; `MediaHubEvent` gains `PlaybackSessionUpdated` and `PlaybackSessionCleared`

Modified: `EntityPlaylist`/`TablePlaylist` (`TimeLastPlayed`), `ResponsePlaylist` and the playlist extension, `PlaylistStamp`/`PlaylistSummary` and the playlist repository, `HandlerPlaylistList`, `PlaylistController`, `HandlerMediaDelete`, `IUnitOfWork`/`UnitOfWork`/fakes, `appsettings.json`.

Contracts every task shares:
- `EntityPlaylist.TimeLastPlayed` is a nullable UTC time; `ResponsePlaylist.TimeLastPlayed` carries it; the detail response may show an older value (its ETag ignores the stamp) and clients take the stamp from the list.
- `POST /Playlist/{id}/Played`: sets `TimeLastPlayed` to now, answers `204`. A call within `ConfigPlayback.PlayedWindowSeconds` (default 10) of the stored stamp changes nothing.
- `EntityPlaybackSession(ID, UserID, QueueMediaFileIDList, CurrentIndex (int?), PositionSeconds (double), PlaylistID (Guid?), TimeModified)`, one per user.
- `ResponsePlaybackSession` has the same fields without `UserID`. `RequestSavePlaybackSession(QueueMediaFileIDList, CurrentIndex, PositionSeconds, PlaylistID)`.
- `ConfigPlayback` (section `Playback`): `MaxQueueLength` (default 1000, `[Range(1, 10000)]`), `PlayedWindowSeconds` (default 10).

---

### Task 1: Last played on the playlist

**Files:** modify `EntityPlaylist`, `TablePlaylist` (+ migration `AddPlaylistLastPlayed`), mappers, `ResponsePlaylist`, `PlaylistExtension`, `PlaylistStamp`, the repository summary and stamp queries; tests in `PlaylistRepositoryTest.cs`, `EntityPlaylistExtensionTest.cs`, `HandlerPlaylistListTest.cs`

- [ ] **Step 1: Write the failing tests.** The repository stores and returns the stamp (null by default); `ToResponse` carries it as UTC; the list ETag differs after the stamp changes with `TimeModified` unchanged, and is the same when nothing changed; the **detail** ETag is unchanged by the stamp; a playlist that was never played has a stable ETag.
- [ ] **Step 2:** Run the classes; expected: compile failure. Implement. The list handler adds the stamp to the ETag input (two entries per playlist: the modified time and the last-played time, using `DateTime.MinValue` for none); the detail handler is untouched.
- [ ] **Step 3:** Run both projects; expected: all pass.
- [ ] **Leave uncommitted.**

---

### Task 2: The Played endpoint

**Files:** create `CommandPlaylistPlayed`, `HandlerPlaylistPlayed`, `Configurations/ConfigPlayback.cs` (and its registration and `appsettings.json` section); modify `PlaylistController`; tests `HandlerPlaylistPlayedTest.cs`, additions to `PlaylistFlowTest.cs`

- [ ] **Step 1: Write the failing tests.** Handler: sets the stamp to `time` and does **not** change `TimeModified`; commits once and publishes `NotificationPlaylistUpdated` with the summary (carrying the stamp) after the commit, with the token; a second call inside the window neither commits nor publishes; after the window it updates again; another user's or an unknown playlist is `PlaylistNotFoundException`. End to end: `POST /Playlist/{id}/Played` is `204`; the list ETag changes and the detail ETag does not; a connected hub client receives `PlaylistUpdated` with the stamp and the same `timeModified`; `404` for another user's playlist and for a malformed ID; `401` without a token.
- [ ] **Step 2:** Run the classes; expected: compile failure. Implement.
- [ ] **Step 3:** Run both projects; expected: all pass.
- [ ] **Leave uncommitted.**

---

### Task 3: The playback session, storage and save

**Files:** create the entity, table, configuration, migration `AddPlaybackSession`, `IPlaybackRepository`, `PlaybackRepository`, `CommandPlaybackSessionSave`, `CommandPlaybackSessionClear`, `QueryPlaybackSession`, their handlers, the notifications and `HandlerNotification…` classes, `RequestSavePlaybackSession` with `ToCommand`, `ResponsePlaybackSession`, `PlaybackSessionController`, fakes; modify `IUnitOfWork`/`UnitOfWork`/`FakeUnitOfWork`; tests per handler and a repository test

**Interfaces:** `IPlaybackRepository`: `FindSession(userID)`, `AddSession`, `UpdateSession`, `RemoveSession`, `GetSessionListByMediaFileID(userID, mediaFileID)`. `IUnitOfWork.Playback`. Endpoints: `GET /PlaybackSession` (`200` with the session and a strong ETag, `304` on a match, `204` when there is none), `PUT /PlaybackSession` (`200` with the saved session), `DELETE /PlaybackSession` (`204`).

- [ ] **Step 1: Write the failing tests.** Save creates the session on the first call and replaces it afterwards; every media file in the queue must exist and belong to the user (otherwise `MediaNotFoundException` and nothing is saved); the queue may repeat a media file; a queue longer than `MaxQueueLength` is a `400`; an index outside `0..length-1` (or any index for an empty queue) is a `400`, `null` is allowed; a negative or non-finite position is a `400`; a `PlaylistID` that is not the user's is a `404`; an identical save neither commits nor publishes; a changed save sets `TimeModified` from `time`, commits once, then publishes `NotificationPlaybackSessionUpdated` with the token; `GET` returns the ETag, `304` and `204` as above and never another user's session; `DELETE` removes it, publishes `NotificationPlaybackSessionCleared`, and is a `204` even when there is none (no publish then); the hub names are `nameof(MediaHubEvent.PlaybackSessionUpdated)` and `PlaybackSessionCleared`; a failed push is logged and never fails the request.
- [ ] **Step 2:** Run the classes; expected: compile failure. Implement.
- [ ] **Step 3:** Run both projects; expected: all pass.
- [ ] **Leave uncommitted.**

---

### Task 4: Deleting media, two devices, and the acceptance run

**Files:** modify `HandlerMediaDelete`, `HandlerMediaDeleteTest.cs`; create `PlaybackSessionFlowTest.cs` (integration); docs

- [ ] **Step 1: Write the failing tests.** Deleting a media file removes it from the user's session queue in the same commit as the file and the playlist entries, keeps the order of the rest, moves `CurrentIndex` back by one when the removed file was before it, sets it to `null` when the current file itself was removed (and the position to 0), clears the session when the queue becomes empty, and publishes the session after the media-deleted and playlist notifications; another user's session is untouched. Two saves from two devices in a row: the second wins and a `GET` returns it. End to end through HTTP and the hub: save, `GET`, `304` with the ETag, a hub client receives `PlaybackSessionUpdated`, delete a media file in the queue and receive the updated session, `DELETE` and receive `PlaybackSessionCleared`, `401` without a token, and the rate-limit behaviour is unchanged.
- [ ] **Step 2:** Run the classes; expected: failing tests. Implement.
- [ ] **Step 3:** Update the screens design doc if anything differs from the contracts above, and note in the metadata plan's report that both migrations ran in order. Build the whole solution (0 warnings), run both test projects, and report the counts before and after the plan.
- [ ] **Leave uncommitted.**
