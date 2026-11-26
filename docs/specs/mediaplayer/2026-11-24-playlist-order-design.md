# Playlist order: drag the sidebar playlists into your own order

## Intent

The sidebar lists the playlists in an order of the user's own, set by dragging, saved on the server so every device shows the same one, and working like the entries of a playlist.

## Goals

- Drag a playlist in the sidebar to a new place; the order is the same after a restart and on every device.
- A new playlist always lands on top and pushes the others down by one.
- Moving, creating and deleting keep the positions whole: 0, 1, 2 and so on, with no gaps.

## Non-goals

- Playlists dragged in the Add to playlist dialog, on Home or anywhere but the sidebar. The dialog keeps its recently-played order.
- Folders, groups or pinning.
- Sorting the sidebar by name or date from a menu.

## Anti-goals

- A new playlist or a delete that scrambles the order the user made.
- A drag that works on one device and shows the old order on the next.
- A move that changes a playlist's modified time (it is not an edit of the playlist).

## Approaches

One sensible way, the user's choice: do what the entries of a playlist do. Each playlist has a whole-number position, one endpoint moves a playlist to a position and renumbers the rest, and the server tells the other devices. A gap-based weight (positions with room between them, so a move writes one row) was considered and dropped: with a few dozen playlists a renumber is cheap, and a gap-based number needs rebalancing rules the entries do not need.

## Design

- **Data:** `EntityPlaylist` gets `Position` (int, per user, 0 is the top). `ResponsePlaylist` carries it. The migration has the column.
- **Create:** the new playlist gets position 0 and every other playlist of the user moves down by one, in the same save.
- **Delete:** the playlists below it move up by one, so positions stay whole.
- **Move:** `PUT /Playlist/{id}/Position` with `{ "position": n }`, like the entry move. The playlist goes to `n` (clamped to the list) and the others shift to make room. The answer is the user's whole playlist list in its new order. A move does not touch `TimeModified`.
- **Sync:** one new hub event, `PlaylistOrderChanged`, sent to the user's other devices after a move, create or delete that changes positions. A device that gets it refreshes the playlist list (`SyncPlaylistList(force: true)`). The list's ETag must change when positions change.
- **Sidebar:** sorted by `Position`; the playlist rows are a shared `SortableList` and a drop calls `IPlaylistService.Move(id, newPosition)`. The new order shows at once and is replaced by the server's answer; a refusal puts the old order back and shows a toast.
- **Offline:** dragging is disabled while offline (the same reason text as the other writes).
- **Order elsewhere:** Home and the Add to playlist dialog keep the orders they have (`TakeRecentlyPlayed`, `OrderMostRecentFirst`). `OrderNewestCreatedFirst` is no longer used by the sidebar and goes.

## Left to the builder

- The migration's exact SQL and naming; how the position of the whole list is renumbered in one save.
- Whether the answer of a move is the list or just the positions that changed, as long as the client ends up with the whole order.
- How the drag handle looks in the sidebar and that a click on a row still navigates.
- Tests: handlers (create, delete, move, clamp, no `TimeModified` change), the migration's order, the client's optimistic move and its rollback, and the hub event refreshing the list.
