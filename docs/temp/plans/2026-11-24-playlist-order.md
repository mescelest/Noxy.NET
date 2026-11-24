# Playlist Order Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Playlists have a saved position the user sets by dragging them in the sidebar; a new playlist is always on top.

**Architecture:** `EntityPlaylist.Position` (whole numbers, 0 on top) behind a `PUT /Playlist/{id}/Position` endpoint like the entry move. Create shifts the others down; delete closes the gap. A new hub event `PlaylistOrderChanged` tells the other devices to refresh the list. The sidebar sorts by `Position` and drags with the shared `SortableList`.

**Tech Stack:** .NET 10, EF Core migration, Mediator handlers, SignalR hub, MAUI Blazor Hybrid, Fluxor, xUnit v3.

**Spec:** `docs/specs/2026-11-24-playlist-order-design.md`

## Global Constraints

- Positions are whole and per user: 0, 1, 2 with no gaps, 0 on top. A move does not change `TimeModified`.
- A new playlist gets position 0 and every other playlist of the user moves down by one in the same save.
- "Media" in code, "song" in user-facing text. One class per file; enums end in `Enum` in an `Enums` folder; no magic strings (event names through `nameof(MediaHubEventEnum.…)`); DI names kind first; `is null`, no `!`; no inline logic in Razor attributes.
- Run only relevant tests: `cd Noxy.NET.Test/Noxy.NET.Test.UnitTest && dotnet build && ./bin/Debug/net10.0/Noxy.NET.Test.UnitTest.exe -class "<ns.Class>"`. Integration tests run the same way from `Noxy.NET.Test.IntegrationTest`. `dotnet test` does not work. Build the App with `--no-dependencies` and read only ` error CS` / ` error RZ` lines.
- Do not commit production code: stage new files by name and leave the commit to the user. Test files alone may be committed.

## Review Focus

- The migration gives the existing playlists positions in today's order (newest created first), per user, so the sidebar looks the same right after it.
- Moving to the position a playlist already has changes nothing: no save, no event, same ETag.
- A position out of range (negative, or past the end) is refused, as for entries (`InvalidPlaylistPositionException`).
- Another user's playlist cannot be moved or shifted (`PlaylistNotFoundException`), and one user's create never changes another user's positions.
- Creating when the user has no playlists, and deleting the last or the first playlist, leave positions whole.
- Two devices: after a move on one, the other refreshes the list and shows the same order (the list's ETag changes with positions).

---

### Task 1: Position in the data

**Files:**
- Modify: `Noxy.NET.MediaPlayer.Domain/Entities/EntityPlaylist.cs`, `Domain/Responses/Playlist/ResponsePlaylist.cs`, `Application/Models/Playlist/PlaylistStamp.cs`, the `ToResponse` mapping for playlists, the playlist repository (`GetPlaylistSummaryList`, `GetPlaylistStampList`, and its interface comment "Newest first")
- Create: an EF migration (`Persistence/Migrations`) and update `DataContextModelSnapshot.cs`
- Modify: `Application/Handlers/Playlist/HandlerPlaylistList.cs` (ETag stamps)
- Test: the existing playlist repository integration test class and `HandlerPlaylistList` unit tests (find them with `grep -rl PlaylistRepository`/`HandlerPlaylistList` under `Noxy.NET.Test`)

**Interfaces:**
- Produces: `EntityPlaylist.Position` (`int`, default 0), `ResponsePlaylist.Position` (`int`, `{ get; init; }`, not `required`, so existing constructions still compile), `PlaylistStamp(Guid ID, DateTime TimeModified, DateTime? TimeLastPlayed = null, int Position = 0)`. `GetPlaylistSummaryList` returns the playlists ordered by `Position` ascending.

- [ ] **Step 1: Write the failing tests:** repository `GetPlaylistSummaryList_OrdersByPosition`; list handler `Handle_ETagChanges_WhenOnlyAPositionChanges`; migration test (integration) `Migration_GivesExistingPlaylistsPositionsNewestCreatedFirst_PerUser` if the integration project already tests migrations, otherwise a repository test that a database built from the migration has the positions.
- [ ] **Step 2: Run them;** expect failures (missing `Position`).
- [ ] **Step 3: Implement** the property, response mapping, stamp field (added to the list ETag stamps beside `TimeModified` and `TimeLastPlayed`), repository ordering, and the migration: add the column, then set each user's positions with a window function ordered by `TimeCreated` descending.
- [ ] **Step 4: Run** the same classes; expect pass.
- [ ] **Step 5:** Stage by name.

### Task 2: Create, delete and move on the server

**Files:**
- Modify: `Application/Handlers/Playlist/HandlerPlaylistCreate.cs`, `HandlerPlaylistDelete.cs`, `Application/Interfaces/Repositories/IPlaylistRepository.cs` and its implementation and `Fakes/FakePlaylistRepository` (a method to load all of a user's playlists without entries ordered by position, if none fits), `API/Controllers/PlaylistController.cs`
- Create: `Application/Commands/Playlist/CommandPlaylistMove.cs`, `Application/Requests/Playlist/RequestMovePlaylist.cs`, `Application/Handlers/Playlist/HandlerPlaylistMove.cs`
- Test: the existing playlist handler test classes (find with `grep -rl HandlerPlaylistCreate Noxy.NET.Test`)

**Interfaces:**
- Consumes: `EntityPlaylist.Position`, `GetPlaylistSummaryList` ordered by position (Task 1).
- Produces: `CommandPlaylistMove(Guid UserID, Guid ID, int Position) : ICommand<List<ResponsePlaylist>>`; `RequestMovePlaylist { int Position }` with `ToCommand(Guid userID, Guid id)`; endpoint `[HttpPut($"{IdRoute}/Position")] Task<ActionResult<List<ResponsePlaylist>>> Move(Guid id, RequestMovePlaylist request, CancellationToken cancellationToken)`; `HandlerPlaylistMove`.

- [ ] **Step 1: Write the failing tests:** `Create_PutsTheNewPlaylistOnTop_AndMovesTheOthersDownByOne`; `Create_DoesNotChangeAnotherUsersPositions`; `Create_WithNoPlaylists_GivesPositionZero`; `Delete_ClosesTheGap_SoPositionsStayWhole` (first, middle, last); `Move_PutsThePlaylistAtThePosition_AndRenumbersTheOthers` (up and down); `Move_ToItsOwnPosition_ChangesNothing` (no save, no event); `Move_OutOfRange_Throws_InvalidPlaylistPositionException` (-1 and count); `Move_OfAnotherUsersPlaylist_Throws_PlaylistNotFoundException`; `Move_DoesNotChangeTimeModified`; and the move's answer is the whole list in the new order.
- [ ] **Step 2: Run them;** expect failures.
- [ ] **Step 3: Implement.** Create sets the new playlist's `Position = 0` and increments the positions of the user's existing playlists in the same commit. Delete decrements the positions after the removed one. Move loads the user's playlists ordered by position, removes and inserts the playlist, renumbers, saves only the rows whose position changed, and returns the list through `ToResponse`. All three keep the same save pattern as `HandlerPlaylistEntryMove`.
- [ ] **Step 4: Run** the playlist handler classes; expect pass.
- [ ] **Step 5:** Stage by name.

### Task 3: The hub event

**Files:**
- Modify: `Domain/Enums/MediaHubEventEnum.cs` (add `PlaylistOrderChanged` after `PlaylistDeleted`), `Application/Handlers/Playlist/HandlerPlaylistCreate.cs` and `HandlerPlaylistMove.cs` (publish it)
- Create: `Application/Notifications/Playlist/NotificationPlaylistOrderChanged.cs` (`(Guid UserID)`), `API/Handlers/Playlist/HandlerNotificationPlaylistOrderChanged.cs` (copy of `HandlerNotificationPlaylistDeleted`, sending `nameof(MediaHubEventEnum.PlaylistOrderChanged)` with no argument)
- Test: the handler tests of Task 2 and the existing API notification handler tests (find with `grep -rl HandlerNotificationPlaylistDeleted Noxy.NET.Test`)

**Interfaces:**
- Produces: `NotificationPlaylistOrderChanged(Guid UserID)`; hub event `PlaylistOrderChanged` with no payload.

- [ ] **Step 1: Write the failing tests:** `Create_PublishesPlaylistOrderChanged_WhenOthersMoved` and not for the first playlist of a user; `Move_PublishesPlaylistOrderChanged_OnlyWhenSomethingMoved`; `NotificationHandler_SendsPlaylistOrderChanged_ToTheUsersClients`.
- [ ] **Step 2: Run;** expect failures.
- [ ] **Step 3: Implement** the enum value, notification, API handler and the two publishes.
- [ ] **Step 4: Run;** expect pass.
- [ ] **Step 5:** Stage by name.

### Task 4: The client calls and sync

**Files:**
- Modify: `Presentation/Interfaces/Services/IApiClient.cs`, `Services/ApiClient.cs`, `Interfaces/Services/IPlaylistService.cs`, `Services/PlaylistService.cs`, `Interfaces/Services/ISyncService.cs` and `Services/SyncService.cs`, `Services/MediaHubService.cs`, `Extensions/ResponsePlaylistListExtension.cs`
- Modify: fakes `FakeApiClient`, `FakeSyncService`, `FakePlaylistService` (if present)
- Test: `ApiClientTest`, `PlaylistServiceTest`, `SyncServiceTest`, `MediaHubServiceTest` and `ResponsePlaylistListExtensionTest` in `Noxy.NET.Test.UnitTest/MediaPlayer/Presentation`

**Interfaces:**
- Consumes: `PUT /Playlist/{id}/Position` and `PlaylistOrderChanged` (Tasks 2 and 3).
- Produces: `Task<List<ResponsePlaylist>> MovePlaylist(Guid id, RequestMovePlaylist request)` on `IApiClient`; `Task Move(Guid id, int newPosition)` on `IPlaylistService`; `Task ApplyPlaylistOrder(List<ResponsePlaylist> playlistList)` on `ISyncService` (replaces the stored list and raises `Changed`); `OrderByPosition()` replacing `OrderNewestCreatedFirst()` in the list extension (ascending `Position`, ties keep input order).

- [ ] **Step 1: Write the failing tests:** `MovePlaylist_PutsToThePosition_AndReturnsTheList` (method `Put`, URI `…/Playlist/{id}/Position`, body `{"position":2}`); `Move_SendsTheMove_AndAppliesTheAnswerAsTheOrder`; `Move_WhenTheServerRefuses_Throws_AndAppliesNothing`; `ApplyPlaylistOrder_ReplacesTheList_AndRaisesChanged`; `HubEvent_PlaylistOrderChanged_RefreshesThePlaylistListWithForce` and the event name is in `HandledEventNameList`; `OrderByPosition_SortsAscending_AndKeepsTheInputOrderOnATie`.
- [ ] **Step 2: Run;** expect failures.
- [ ] **Step 3: Implement** each member; `ApplyPlaylistOrder` uses `storeMetadata.ReplacePlaylistList` and clears the list ETag so the next sync refetches; the hub handler calls `serviceSync.SyncPlaylistList(true)`.
- [ ] **Step 4: Run** the classes above; expect pass.
- [ ] **Step 5:** Stage by name.

### Task 5: The sidebar

**Files:**
- Modify: `App/Components/Scaffolding/SidebarElement.razor` and `.razor.css`, `Presentation/Extensions/ResponsePlaylistListExtension.cs` (remove `OrderNewestCreatedFirst` and its test), `docs/specs/2026-11-15-frontend-screens-design.md` (the sidebar order line), the playlist order spec status

**Interfaces:**
- Consumes: `OrderByPosition()`, `IPlaylistService.Move(Guid id, int newPosition)`, `ISyncService.IsOffline` (to disable the drag, with the reason text used by other writes).

- [ ] **Step 1: Sidebar order:** `PlaylistList` is `StatePlaylistList.Value.PlaylistList.OrderByPosition()`; the playlist rows are items of a shared `SortableList<ResponsePlaylist>` with `OnUpdate` calling `Move(list[IndexOld].ID, IndexNew)`; the whole row is the drag handle except the "⋯" button and the link text click, which still navigate and open the menu. While offline the list is not draggable.
- [ ] **Step 2: Optimistic order:** on a drop the sidebar shows the new order at once from a local copy and replaces it with the stored list when `Move` returns; on an exception it restores the old order and shows a toast through `IToastService` with `exception.ToActionMessage()`.
- [ ] **Step 3: Remove** `OrderNewestCreatedFirst` and its test; update the two spec lines.
- [ ] **Step 4: Build** the App with `--no-dependencies`; expect no ` error CS` / ` error RZ`.
- [ ] **Step 5: Run the app and check by hand** (no component harness): drag a playlist and see it stay after a restart; create a playlist and see it on top with the others pushed down; with two devices, move on one and see the other follow; go offline and see dragging disabled; click a row and see it still opens; right-click and see its menu.
- [ ] **Step 6:** Run the whole unit suite once (`.exe` with no filter) and the integration playlist tests; expect all pass. Stage by name.

---

## Self-review notes

- Spec coverage: data and migration (Task 1), create/delete/move rules and the answer (Task 2), hub event (Task 3), client move, optimistic order and rollback, offline (Tasks 4 and 5), other orders left alone (Task 5 touches only the sidebar), no `TimeModified` change (Tasks 1, 2).
- Names checked across tasks: `Position`, `CommandPlaylistMove`, `RequestMovePlaylist`, `MovePlaylist`, `Move`, `ApplyPlaylistOrder`, `OrderByPosition`, `PlaylistOrderChanged`.
- Ruling: `ResponsePlaylist.Position` is not `required`, so the many existing test constructions of `ResponsePlaylist` keep compiling.
