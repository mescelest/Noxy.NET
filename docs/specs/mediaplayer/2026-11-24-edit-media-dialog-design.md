# Edit media file dialog

## Intent

The server already edits a media file's tags (`PATCH /media/{id}`, `RequestUpdateMedia`), and the frontend sync spec left the screen for later. This adds the screen: a dialog to correct the tags of one song, opened from the context menu of a row in the Library and on a playlist's page.

## Goals

- Fix a typo or a missing tag on one song without leaving the app.
- Look and behave like the Edit playlist dialog.

## Non-goals

- Editing several songs at once. Edit is left out of the menu for a ticked group.
- Choosing existing artists, genres or albums from a list. They are typed as text.
- Editing offline.
- Rewriting the stored file. The server only changes its database, as the sync spec says.

## Approaches

One sensible way: copy the pattern of Edit playlist (a form component with its form model, inside the shared Dialog), because the project already does it that way and the shared WebForm comes with validation, a loading state and form errors. A hand-built form would only repeat that.

## Design

- **Menu:** an "Edit…" item (icon `edit`) in `LibraryContextMenu`, shown only for the menu of a right-click on a row, not in the ticked group's menu or its sub-menu, and not in the Actions button's menu. Library and playlist pages share the component, so both get it. Disabled with the offline reason when the device is offline.
- **Dialog:** the shared `Dialog` at its default size, titled "Edit song", closed on save.
- **Form:** `FormEditMedia` in `App/Components/Forms`, a shared `WebForm` over `FormModelEditMedia` (in Presentation, next to `FormModelEditPlaylist`). Shared input groups only: `InputGroupText` for title, artist, album, album artist, genre and subtitle; `InputGroupInteger` for year, track and disc; `InputGroupTextArea` for the comment; the shared `Button` to save.
- **Rules:** the form model repeats the server's limits (title required, 255; artist, album, album artist, subtitle 256; comment 2048; genre 128; year, track, disc 0 to 9999).
- **Artists, album artists and genres** are one text field each. The server splits the text (`;` and "feat."/"ft."/"featuring"), so the field carries a hint: separate several with `;`. The field starts out holding the song's current text.
- **Saving:** only the fields that changed are sent, as the server takes each as optional. The answer updates the local copy, and the existing push updates other devices. A refusal (`ApiException`, `HttpRequestException`) is shown as a form error.
- **Service:** an `IMediaService` method (or the nearest existing service) sends the update and stores the answer; the form calls it as Edit playlist calls `ServicePlaylist.Edit`.
- **Texts:** all in `AppTexts` (and `PresentationTexts` where they belong), media in code and song in user-facing text.

## Left to the builder

- Where the update method lives and how the local metadata store is refreshed from the answer.
- Exact field order and how year, track and disc sit (one row if the shared components allow it).
- Tests: the service, the form model rules, and the menu (Edit present for a row, absent for the group).
