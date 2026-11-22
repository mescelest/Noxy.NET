# Context menu and popover: design

Status: agreed in conversation with the user on 2026-11-21 (approach chosen, behaviour decided where marked). Not built. Awaiting approval of this document before a plan is written.

## Intent

The app has three hand-made pop-ups: the Library **Actions** menu, the player bar's **device picker** and the **queue panel**. Each has its own backdrop element and fixed placement, none checks the window edges, and each closes in its own way. The user wants two reusable shared components so that anything that pops out of a click behaves the same way and stays inside the window:

- a **context menu**: a list of items with sub-menus, opened by right-clicking something (a Library row, a playlist in the sidebar) or by clicking a "⋯" button (one next to each playlist in the sidebar, with "Rename" and "Delete" among its options);
- a **popover**: a floating panel that takes any content, for the queue and the device picker.

The context menu is built on the popover.

## Goals

- **One look and one behaviour** for every pop-up: drawn above everything, kept inside the window, closed by clicking away or pressing Escape.
- **Edge-aware:** the panel flips to the other side of its anchor when there is no room, shifts to stay inside the window, and scrolls inside itself when it is taller than the window.
- **Two ways to open:** next to an element (a button) or at a point (the mouse position of a right-click).
- **Sub-menus** that work with a mouse, a keyboard and a finger (see "Touch and narrow windows").
- **Keyboard and screen reader support** for the menu: arrow keys, Enter and Space, Right and Left for sub-menus, Escape, Home and End; menu roles and expanded states; focus goes back to what opened it.
- **No backdrop element.** Clicking away closes the panel through the browser's own light dismiss.
- **The queue panel closes when you click away** (the user's decision). The device picker does too.
- **A "⋯" context button on each playlist in the sidebar** (the user's request, added to this design on 2026-11-21): it appears when the pointer is over the playlist, uses the wireframe `None` button style, and opens a menu of playlist actions (see "Where the app uses them").

## Non-goals

- Popovers that stay open until closed by hand (the browser's "manual" mode). Every pop-up here closes on click-away; add the mode when something needs it. *(my call)*
- Animation of opening and closing.
- Checkable or radio items, and menus wider than text and an optional icon.
- Replacing the `Dialog` component.
- "Add the whole playlist to the queue": the queue service adds one media file at a time today, so it needs its own change.
- The wording pass on user-facing text (the user will do one later).

## Anti-goals

Failures even if it technically works:

- A backdrop, or any element laid over the page to catch clicks.
- Behaviour that only works with hover, or only with a mouse.
- A panel that can open partly outside the window.
- Each consumer writing its own placement or closing logic again.
- Focus lost on the page after a menu closes.
- The trigger button reopening what the click-away just closed (clicking the queue button while the queue is open must close it, not close and reopen it).

## Constraints

- The app is a MAUI Blazor Hybrid app, Windows first, Android later. The web view is Chromium, so the browser's Popover API is available. Touch matters: hover cannot be the only way to do anything.
- The two components live in `Noxy.NET.UI.Shared` and are baseline components: defaults, `RenderFragment` hooks, `data-*` attributes and structural CSS only, with no opinion on colours, animation or icons beyond variables with fallbacks. The app styles them through its variables. Where the MediaPlayer needs to wire them to its services (the Library actions, the queue) it uses a thin wrapper in the MediaPlayer project, the same way as the upload button and drop zone.
- The shared library already shows the pattern for script modules (`GetComponentPath`, `Dialog`, `UploadButton`).
- Repository conventions apply: one class per file, no magic strings (script function names are constants), enums named `...Enum` in an `Enums` folder, no inline logic in markup (named methods and properties), `is null`, no `!`.
- Code says "media file"; text a user reads may say "song" (see the memory note). The menu examples below show user-facing wording.
- Dialogs already exist and stay as they are. The Library's "Add to playlist" chooser is a dialog today.

## Approaches considered

**A. Browser popover for everything, including placement.** The popover gives the top layer, light dismiss and nesting; CSS anchor positioning with fallbacks places it. Least code, but a right-click at the cursor and sub-menus still need script, and it depends on a newer Chromium feature (fine on a current Windows web view, a risk on older Android ones).

**B. Browser popover for the layer and closing, plus one small placement script. Chosen by the user.** The popover draws above everything, closes on click-away and Escape, and nests. A single script routine takes an anchor's rectangle or a point, measures the panel and flips and shifts it to fit. Context menus, sub-menus, the device picker and the queue use the same routine. One code path, and it works on any Chromium.

**C. All script.** Draw, place and close everything by hand. Most control, but it rebuilds what the browser does well (nesting, light dismiss, the top layer) for no gain.

## Design (approach B)

### The popover component

- A floating panel with a `ChildContent`. It is open or closed through a two-way `Open` parameter, like `Dialog`.
- It is placed next to an **anchor** (an element reference) or at a **point** (x and y in window coordinates). It also takes a preferred side and alignment (for example "below, aligned to the end"; the queue wants "above, aligned to the end").
- It uses the browser's popover in its click-away mode, so opening it closes other open popovers that are not its parent, and a click outside or Escape closes it. The component reports the close back through `OpenChanged`, as `Dialog` does.
- The placement script runs after the panel is shown: it measures the panel, then tries the preferred side, flips to the opposite side if that overflows, then shifts along the other axis to keep a margin from the window edge, and caps the height with internal scrolling if nothing fits. It runs again on window resize, or the panel closes on resize (the builder decides which; closing is the simpler one).
- The trigger must not undo a click-away. The builder picks how (the browser's own invoker attribute on the trigger, or ignoring a click-away that lands on the trigger); the requirement is the anti-goal above.
- Baseline structural CSS only: no backdrop, no animation, a `data-placement` attribute that says which side it ended up on (so a consumer can draw a caret).

### The context menu component

- Built on the popover. It is a vertical list with role `menu`.
- **Items are declared as child components**, so a consumer writes them in markup: an item (text, optional icon, disabled, a reason shown as a tooltip when disabled, a "destructive" flag exposed as a `data-*` attribute, and a click callback), a separator, and a sub-menu (text and its own items). A consumer can also use a `RenderFragment` for an item's content. *(my call; the builder may choose a data-driven list instead if the markup is clearer)*
- **Opening:** `OpenAt` with a point (called from an `@oncontextmenu` handler, which also prevents the browser's own menu), or anchored to a button. Opening from a keyboard (the menu key or Shift+F10) opens it at the focused element.
- **Closing:** choosing an item closes the whole chain of menus. Escape closes the current sub-menu first, then the menu.
- **Keyboard:** Up and Down move, Right or Enter opens a sub-menu, Left closes it, Home and End jump, Enter and Space choose, typing a letter jumps to the next item that starts with it *(my call)*. Focus returns to the trigger on close.
- **Accessibility:** `menu`, `menuitem`, `menuitem` with `aria-haspopup` and `aria-expanded` for sub-menu items, `aria-disabled` for disabled items, and a `title` carrying the reason.

### Touch and narrow windows (decided with the user)

- A sub-menu opens on click, tap, Enter or Right; hover also opens it with a mouse, but is never the only way.
- On a narrow window or a touch screen the sub-menu does not fly out to the side. It **replaces the menu's content in place**, with a "‹ Back" row at the top (drill-in).
- A long-press opens the context menu: the Android web view raises the same context menu event on a long-press. To be confirmed on a device.
- Items are at least the standard control height, so they are comfortable to tap.
- "Narrow" is decided by the same breakpoint the app already uses (`breakpoints.css`), read through the script, not by a second rule.

### Where the app uses them (this design's scope)

1. **Library row right-click → context menu.** The actions are the existing Actions menu items (download to this device, save as file, add to playlist, remove from this device, delete from library, with their disabled reasons and the confirmation for delete). The scope follows the user's decision:
   - Right-click on a row that is **not** part of a ticked group of two or more: the menu acts on that row only. Plain labels, for example "Delete song".
   - Right-click on a row that **is** part of a ticked group of two or more: the clicked row's actions come first, then a separator, then one entry, for example "26 selected songs", that opens a **sub-menu** with the same actions for the whole group, where delete reads "Delete 26 songs". *(sub-menu rather than a doubled flat list is my call; the user may revisit it in the later wording pass)*
   - "Add to playlist" can list the playlists as a sub-menu instead of opening a dialog; the builder may keep the dialog for the first version.
2. **The Actions button above the table** is rebuilt on the context menu, opened from the button, with the same items acting on the ticked rows. This replaces its own backdrop and fixed placement.
3. **The device picker** becomes a popover holding the device list, opened above the player bar.
4. **The queue panel** becomes a popover anchored above the player bar. It closes when you click away; its own close button can stay. This removes its fixed `position: absolute` placement.
5. The existing hand-made backdrops (`ActionsMenu`, `DevicePicker`) are deleted.

6. **The playlist "⋯" context button in the sidebar.**
   - **The button:** a small icon button (`more_horiz`) at the end of each playlist's row in the sidebar's playlist list, in the wireframe `None` button style. It is hidden until the pointer is over the row, and shows while the row or the button has keyboard focus. With no hover (touch, or a narrow window) it is always shown, since there is nothing to hover. The row link keeps its width, so showing the button does not move the name.
   - **Opening:** clicking the button opens the context menu next to it. A right-click on the row, or a long-press, opens the same menu at that point. The button's click must not also follow the row's link.
   - **The menu** (user-facing wording, my proposal, built from what the playlist services already do):
     - **Play:** starts the playlist.
     - **Download all:** downloads every song of the playlist to this device.
     - **Edit…:** opens an "Edit playlist" dialog with the name field and the "Allow the same song twice" option (a checkbox), in the style of the "New playlist" dialog, and saves both in one update. A dialog rather than separate menu items so that it can take more playlist settings later (the user's decision, 2026-11-21). The server refuses to turn the duplicates rule off while the playlist has duplicates; the refusal is shown inside the dialog as a form error, since it belongs to the form.
     - **Refresh:** syncs the playlist from the server again (moved here from the playlist page; see `2026-11-21-playlist-page-design.md`).
     - A separator.
     - **Delete playlist:** the destructive item, last. The first activation turns it into "Click again to delete" and keeps the menu open; the second deletes. Closing the menu or moving to another item puts it back. *(my call, in the spirit of the click-twice delete the user asked for on the server list; a confirmation dialog would also do)* If the deleted playlist is the one on screen, the app goes back to Home.
   - **Left out on purpose:** "Upload to playlist" (it needs a file picker, which a menu item cannot open reliably; it stays on the playlist page). "Add to queue" is a later change (see the non-goals).

## Testing

- The logic that decides what a right-click acts on (the clicked row, or the ticked group) lives in C# in the MediaPlayer project and is covered by unit tests.
- The item state (disabled reasons, which sub-menu is open, drill-in navigation, keyboard moves) should be written so it can be tested without a browser where that is practical.
- The placement script is checked by hand on a real window, including near each edge, at a small window size and on touch, because the unit test project has no browser. The builder may add a small pure function for the placement arithmetic if a test for it is cheap.

## Left to the builder

- Component and file names, and the exact parameters.
- How items register with their menu (child components that register themselves, or a list the consumer builds).
- The placement margin, the exact flip and shift rules, and whether a resize closes or repositions the panel.
- How the trigger avoids reopening what a click-away closed.
- The exact CSS, within the baseline rule (structure only, colours through variables with fallbacks).
- Whether "Add to playlist" is a sub-menu in the first version.
- How the playlist's "⋯" button is revealed on hover without hiding it from keyboard and screen readers (visually hidden is not the same as removed).
- The order of the work. Suggested: the placement script and the popover first, with the device picker as the first consumer; then the context menu with the Actions button; then the Library right-click; then the queue.
