# Metadata Entities and User Settings Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:executing-plans (the user wants this done natively, in the main session, not through subagents). Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Make artists, albums and genres real per-user items, filled in from the embedded tags when a file is uploaded, editable by hand (create, rename, merge, split, delete, assign), exposed through the same list, ETag and push pattern as playlists, plus a small synced user-settings record whose first setting decides what a colliding rename does.

**Architecture:** The same shape as the earlier backend plans: entities in `Domain`, tables, mappers and repositories in `Persistence`, commands, queries, handlers, notifications and services in `Application`, requests (with `ToCommand` methods) in `Application/Requests`, controllers and hub handlers in `API`. A media file's text fields in responses become derived from its links, and every change that alters a file's response also changes that file's modified time. Shared logic exists once (key normalisation, credit splitting, find-or-create, the change base for each kind).

**Tech Stack:** .NET 10, ASP.NET Core, EF Core with SQLite, `Mediator` 3.0.2, TagLibSharp (already referenced), xunit v3.

**Spec:** `docs/superpowers/specs/2026-11-15-metadata-entities-design.md` (approved 2026-11-15). Follow-up plans: the last-played time and saved session, the screens, and updates to the two frontend plans.

## Global Constraints

- Work only in `Noxy.NET.MediaPlayer` and `Noxy.NET.Test`. Target `net10.0`. No frontend work.
- Conventions: one class per file, file named after it; no summary comment above a class or record; interfaces in an `Interfaces` folder that mirrors the implementation's folder; extensions in an `Extensions` folder; no helper classes (logic lives on the owning type, in a service named `…Service`, or in an extension); lists end in `List`; no `yield`; no magic strings; `is null`; no `!`; method-syntax LINQ; media, never "song". Domain entities carry the `Entity` prefix, tables the `Table` prefix. Injected dependencies are named kind first (`serviceNameKey`, `factoryUnitOfWork`, `configPlaylist`); an `IPublisher` is `publisher`; `IMediator` is `mediator`; `time` keeps its name.
- Names follow the kind-first rule: `CommandArtistCreate`, `QueryArtistList`, `HandlerArtistCreate`, `NotificationArtistUpdated`, `RequestCreateArtist` (in `Application/Requests/Metadata`), `ResponseArtist` (in `Domain/Responses/Metadata`), controllers `ArtistController`, `AlbumController`, `GenreController`, `SettingsController` (all derive from `UserControllerBase`). Folders per kind and area with namespaces mirroring them (`Commands/Metadata`, `Queries/Metadata`, `Handlers/Metadata`, `Notifications/Metadata`, `Models/Metadata`). Watch for the namespace-versus-type clash a new `Metadata` namespace can cause; rename the type, never alias around it.
- Commands and queries are immutable records carrying the `UserID` from the token; the controller keeps all HTTP (ETag, 304, status codes). Handlers are singletons with no mutable state, keep one commit per use case, publish only after the commit, pass the cancellation token to the publish, and throw domain exceptions that `ApiExceptionHandler` maps (new ones are added there: a name collision is `409`, not found is `404`).
- Another user's item or file is always a `404`, and two users may have items with the same name. A request class never carries a user.
- Tests: xunit v3. Build the test project, then run its exe, for example `Noxy.NET.Test/Noxy.NET.Test.UnitTest/bin/Debug/net10.0/Noxy.NET.Test.UnitTest.exe -class "*CreditSplittingServiceTest"`; `dotnet test` finds nothing. Pass `TestContext.Current.CancellationToken` to every call that accepts a token. Zero warnings. A test folder contains only tests; helpers go in `Fakes` or `Fixtures`. One unit test class per handler or service; mapper-style tests for each request's `ToCommand`; end-to-end tests in the integration project. The existing tests keep passing in every task unless a task says one is changed.
- Do not commit. The user reviews the diff in the IDE; `git add -N` files you create, by name. Do not run machine-level commands: when a step needs `dotnet ef` and it is not installed, give the user the install command; the migration command itself (`dotnet ef migrations add <Name> --project Noxy.NET.MediaPlayer/Noxy.NET.MediaPlayer.Persistence --startup-project Noxy.NET.MediaPlayer/Noxy.NET.MediaPlayer.API`) is a project command you may run. Write files with the editor tool when they contain quotes, apostrophes, backslashes or `$`. If a file seems to have lost an edit, re-read it before editing.

## Review Focus

- **Real names containing `&`, `,` or `/` stay one artist** ("Earth, Wind & Fire", "AC/DC", "Simon & Garfunkel", "Tyler, The Creator"), while `;`, `feat.`, `ft.`, `featuring` and a tag's own value list split (Task 2, with the same tests reused by `PATCH` in Task 6 and the backfill in Task 10).
- **Two uploads, or an upload and an edit, naming the same new artist must produce one artist** even when both run at once: a unique index on the per-user key is the backstop and the loser must re-find, not fail (Tasks 1 and 4).
- **A rename, merge, split, delete or assignment never leaves a stale response:** each changes the modified time of every file whose response changed, so the library ETag and the pushed `MediaUpdated` are right (Tasks 7 to 9).
- **The backfill loses no tag and can run twice:** empty, whitespace-only, separator-only and very long text, and a file already linked, all behave (Task 10).
- **Unreadable or empty tags never fail an upload:** the file still becomes a library entry with its title from the file name (Tasks 3 and 5).

## File Structure

New files (relative to `Noxy.NET.MediaPlayer.<Layer>/`; tests relative to `Noxy.NET.Test.<UnitTest|IntegrationTest>/MediaPlayer/`):
- Domain: `Entities/EntityArtist.cs`, `EntityGenre.cs`, `EntityAlbum.cs`, `EntityMediaFileArtist.cs`, `EntityMediaFileGenre.cs`, `EntityAlbumArtist.cs`, `EntityUserSettings.cs`; `Responses/Metadata/ResponseArtist.cs`, `ResponseGenre.cs`, `ResponseAlbum.cs`, `ResponseSettings.cs`; exceptions `NameAlreadyInUseException.cs`, `MetadataItemNotFoundException.cs`
- Persistence: the matching `Tables/Metadata/Table*.cs`, type configurations, repository `Repositories/MetadataRepository.cs`, `SettingsRepository.cs`, one migration
- Application: `Interfaces/Services/INameKeyService.cs`, `ICreditSplittingService.cs`, `IMediaTagReadingService.cs`, `IMetadataResolutionService.cs`; `Services/NameKeyService.cs`, `CreditSplittingService.cs`, `MediaTagReadingService.cs`, `MetadataResolutionService.cs`; `Interfaces/Repositories/IMetadataRepository.cs`, `ISettingsRepository.cs`; `Models/Metadata/MediaTagSet.cs`, `ResolvedMetadata.cs`; commands, queries, handlers, notifications and requests per kind and for settings; `BackgroundServices/MetadataBackfillService.cs`
- API: `Controllers/ArtistController.cs`, `AlbumController.cs`, `GenreController.cs`, `SettingsController.cs`; `Handlers/HandlerNotification*Updated|Deleted.cs` per kind, and `MediaHubEvent` gains `ArtistUpdated`, `ArtistDeleted`, `AlbumUpdated`, `AlbumDeleted`, `GenreUpdated`, `GenreDeleted`, `SettingsUpdated`

Modified: `EntityMediaFile` (an optional `AlbumID` and link lists), `EntityMediaUploadJob` (remembers whether the title was sent), `UploadProcessor`, `ResponseMediaFile` and its `ToResponse`, `RequestUpdateMedia`, `HandlerMediaUpdate`, `HandlerMediaDelete` (links go with the file), `MediaTagWritingService`, `IUnitOfWork` and `UnitOfWork` (`Metadata`, `Settings`), the mappers, the fakes, `ApiExceptionHandler`, `Application/ServiceCollectionExtensions.cs`.

Contracts every task shares:
- **Items:** `EntityArtist` and `EntityGenre`: `ID`, `UserID`, `Name`, `NameKey`, `TimeCreated`, `TimeModified`. `EntityAlbum`: the same with `Title`, `TitleKey`, plus `IdentityKey` and an ordered `ArtistLinkList` of `EntityAlbumArtist(AlbumID, ArtistID, Position)`. Links: `EntityMediaFileArtist(MediaFileID, ArtistID, Position)`, `EntityMediaFileGenre(MediaFileID, GenreID, Position)`. `EntityMediaFile` gains `Guid? AlbumID`, `ArtistLinkList`, `GenreLinkList`. A unique index on `(UserID, NameKey)` for artists and genres, and on `(UserID, IdentityKey)` for albums.
- **Keys:** `INameKeyService.GetKey(string name) → string` (trim, collapse inner whitespace, Unicode form KC, lower-case invariant; an all-blank name has no key and is never stored). `EntityAlbum.IdentityKey` is the title key, a pipe, then the artist IDs sorted ascending and joined with commas (empty when it has no artists).
- **Splitting:** `ICreditSplittingService.Split(string? credit) → List<string>` and `Split(IEnumerable<string> valueList) → List<string>`: splits on `;` and on whole-word `feat.`, `ft.`, `featuring` (case-insensitive, also inside brackets, which are removed), trims, drops empties, removes repeats by key while keeping the first spelling and the order.
- **Tags:** `record MediaTagSet(string? Title, List<string> ArtistList, string? Album, List<string> AlbumArtistList, List<string> GenreList, int? TrackNumber, int? DiscNumber, int? Year, string? Comment, string? Subtitle, TimeSpan? Duration)`; `IMediaTagReadingService.Read(string path) → MediaTagSet` (never throws for an unreadable file: returns an empty set).
- **Resolution:** `IMetadataResolutionService.Resolve(IUnitOfWork uow, Guid userID, MediaTagSet tags, CancellationToken) → ResolvedMetadata` where `ResolvedMetadata(List<EntityArtist> ArtistList, EntityAlbum? Album, List<EntityGenre> GenreList)`; it finds each item by key (and the album by identity key) or adds a new one to the unit of work, and re-finds after a unique-key conflict.
- **Responses:** `ResponseArtist(ID, Name, MediaFileCount, TimeModified)`, `ResponseGenre` the same, `ResponseAlbum(ID, Title, ArtistIDList, MediaFileCount, TimeModified)`, `ResponseSettings(AutoMergeOnRename, TimeModified)`. `ResponseMediaFile` keeps `artist`, `album`, `albumArtist`, `genre` (text, derived, several joined with `"; "`) and gains `ArtistIDList`, `AlbumID`, `AlbumArtistIDList`, `GenreIDList`.
- **Settings:** `EntityUserSettings(ID, UserID, AutoMergeOnRename, TimeModified)`, one per user; `GET /Settings` returns the defaults (`AutoMergeOnRename` false) when no row exists and the row is created by the first `PATCH`.

---

### Task 1: Entities, tables, repositories and the migration

**Files:** create the Domain entities and exceptions, the Persistence tables, type configurations, `MetadataRepository`, `SettingsRepository`, the interfaces, the mapper cases, the fakes (`FakeMetadataRepository`, `FakeSettingsRepository`, `FakeStore` additions) and the migration; modify `IUnitOfWork`, `UnitOfWork`, `FakeUnitOfWork`, `DataContext`, `EntityMediaFile`, `TableMediaFile`, `Application/ServiceCollectionExtensions.cs`/`Persistence/ServiceCollectionExtensions.cs`; tests `Metadata/MetadataRepositoryTest.cs`, `Metadata/SettingsRepositoryTest.cs` in the integration project.

**Interfaces:** Produces the entities, `IMetadataRepository` and `ISettingsRepository`. `IMetadataRepository` has, per kind (artist, album, genre), members mirroring `IPlaylistRepository`: `Find…(id, userID)`, `Find…ByKey(userID, key)`, `Get…SummaryList(userID)` (each item with its file count), `Get…StampList(userID)` (ID and modified time, for the ETag), `Add…`, `Update…`, `Remove…`, `GetMediaFileListBy…(userID, id)` (files with their links loaded), and for albums `FindAlbumByIdentityKey(userID, key)`; all scoped to the owner. `ISettingsRepository`: `FindSettings(userID)`, `AddSettings`, `UpdateSettings`. `IUnitOfWork` gains `IMetadataRepository Metadata` and `ISettingsRepository Settings`.

- [ ] **Step 1: Write the failing repository tests** (SQLite through `DatabaseFixture`): add and find each kind; the key is unique per user but the same key under two users is allowed; a second artist with the same key under one user violates the unique index; links store their order; removing an artist removes its links but not the files; removing a file removes its links; `GetArtistSummaryList` counts files per artist; another user's item is not found; settings add and find.
- [ ] **Step 2:** Run the integration project; expected: compile failure.
- [ ] **Step 3:** Implement the entities, tables (with the unique indexes and cascades), type configurations, mapper cases, repositories and the unit of work members. Generate the migration `AddMetadata`; read it, and check it does not touch existing columns.
- [ ] **Step 4:** Run both projects; expected: all pass.
- [ ] **Leave uncommitted.**

---

### Task 2: Key normalisation and credit splitting

**Files:** create `INameKeyService`, `NameKeyService`, `ICreditSplittingService`, `CreditSplittingService`; tests `NameKeyServiceTest.cs`, `CreditSplittingServiceTest.cs`. Register both as singletons.

- [ ] **Step 1: Write the failing tests.** Key: `"Radiohead"`, `"radiohead "` and `"  RADIOHEAD"` share a key; inner whitespace collapses (`"Massive   Attack"` equals `"Massive Attack"`); composed and decomposed `"Beyoncé"` share a key; full-width letters match their normal form; `"AC/DC"` and `"ACDC"` do not; blank or whitespace-only has no key. Splitting: `"A; B"` gives A and B; `"A feat. B"`, `"A (feat. B)"`, `"A ft. B"`, `"A FEATURING B"` give A and B; **`"Earth, Wind & Fire"`, `"AC/DC"`, `"Simon & Garfunkel"`, `"Tyler, The Creator"`, `"Crosby, Stills, Nash & Young"` each give one value unchanged**; a word that merely contains the marker (`"Feathers"`, `"Left Banke"`, `"Soft Machine"`) is not split; repeats by key are removed keeping the first spelling and the order; empty, null, `";"` and `" ; "` give an empty list; the list overload flattens and dedupes across values.
- [ ] **Step 2:** Run the two classes; expected: compile failure. Implement; run; expected: pass.
- [ ] **Leave uncommitted.**

---

### Task 3: Reading tags from a file

**Files:** create `IMediaTagReadingService`, `MediaTagReadingService`, `Models/Metadata/MediaTagSet.cs`; tests `MediaTagReadingServiceTest.cs` (integration project, like the writing service's test) and `Fakes/FakeMediaTagReadingService.cs`. Register as a singleton.

- [ ] **Step 1: Write the failing tests** against small real files built in the test with TagLib (write tags with the existing writing service or TagLib directly, then read): every field round-trips for an mp3 and a flac, including several artists and several genres as a tag's own value list; the duration is read from the file's properties; a file with no tags returns an empty set; a corrupt or non-audio file returns an empty set and does not throw; a very long value is returned as is (limits are applied later).
- [ ] **Step 2:** Run `*MediaTagReadingServiceTest`; expected: compile failure. Implement with TagLib: `Tag.Title`, `Performers`, `Album`, `AlbumArtists`, `Genres`, `Track`, `Disc`, `Year`, `Comment`, `Subtitle`, `Properties.Duration`, mapping 0 to null for the numbers; the result of an exception is an empty set (log a warning, never the file contents).
- [ ] **Leave uncommitted.**

---

### Task 4: Find-or-create resolution

**Files:** create `IMetadataResolutionService`, `MetadataResolutionService`, `Models/Metadata/ResolvedMetadata.cs`; tests `MetadataResolutionServiceTest.cs`. Register as a singleton.

- [ ] **Step 1: Write the failing tests** (fakes, plus one integration test for the conflict): new names create artists, genres and an album and nothing is saved by the service itself (the caller commits); a second call with the same names in different spelling and case reuses the existing items; an album is found by title key plus its album artists, so `"Greatest Hits"` by two different artists are two albums and the same title with no album artist is one album whatever the tracks' artists; an album with no title is not created; credits are split by the splitting service before matching (`"A feat. B"` links two artists in that order, `"AC/DC"` one); the same artist named twice in one call links once; blank values are ignored; names longer than 255 characters are cut to 255 before keying; **a unique-key conflict from a concurrent insert** (the fake store can be told to fail the first add with a conflict, and the integration test inserts the competing row) makes the service re-find and use the winner.
- [ ] **Step 2:** Run the class; expected: compile failure. Implement.
- [ ] **Leave uncommitted.**

---

### Task 5: Upload fills in the metadata, and responses carry the links

**Files:**
- Modify: `EntityMediaUploadJob`/`TableMediaUploadJob` (a `TitleWasSent` flag) with a migration, `HandlerMediaUpload` (set the flag), `UploadProcessor`, `ResponseMediaFile`, `EntityMediaFileExtension.ToResponse`, `MediaRepository` (load the links with every query that builds a response), `HandlerMediaDelete`, the media fakes; tests in `UploadProcessorTest.cs`, `HandlerMediaUploadTest.cs`, `EntityMediaFileExtensionTest.cs`, the end-to-end `MediaUploadFlowTest.cs`

**Interfaces:** Consumes `IMediaTagReadingService` and `IMetadataResolutionService`. The worker reads the staged file after the room checks and before the commit.

- [ ] **Step 1: Write the failing tests.** Processor: a tagged file creates its artists, album and genres and links them in the same single commit that adds the file and removes the job; a second track of the same album reuses the album and the artists (one album, one artist after both); title order: a title the user sent wins, else the embedded title, else the file name; `duration`, track, disc, year, comment and subtitle are stored from the tags; unreadable tags give a library entry titled from the file name with no items; a failing tag read never fails the job; the pushed `NotificationUploadSucceeded` carries the response with the ID lists and the derived text. Response: `artist` is the names joined with `"; "` in link order, `albumArtist` comes from the album's artists, and a file with no links has null text; the end-to-end test uploads two small tagged files of one album through HTTP and sees one album and linked artists in the library.
- [ ] **Step 2:** Run the classes; expected: compile failure. Implement. Existing text columns are no longer written for new files; reads derive from the links (files not yet backfilled still show their legacy text until Task 10 runs).
- [ ] **Step 3:** Run both projects; expected: all pass.
- [ ] **Leave uncommitted.**

---

### Task 6: Editing links through `PATCH /Media/{id}`

**Files:** modify `RequestUpdateMedia`, `CommandMediaUpdate`, `HandlerMediaUpdate`; tests in `HandlerMediaUpdateTest.cs`, `RequestUpdateMediaTest.cs` (and a `ToCommand` test), `MediaChangeFlowTest.cs`

- [ ] **Step 1: Write the failing tests.** The text fields `artist`, `album`, `albumArtist`, `genre` are still accepted: they are split and resolved exactly like an upload (`"A; B"` links two artists, the same call twice changes nothing and does not bump the modified time, blank or `null` clears the links); the new `artistIDList`, `albumID`, `albumArtistIDList`, `genreIDList` set the links directly (an ID that is unknown or another user's is a `404` and nothing changes; `albumID` of `null` clears the album); sending both the text and the IDs for the same field is a `400`; an edit that changes nothing neither commits nor publishes; the response and the `MediaUpdated` push carry the new IDs and derived text. Presence semantics stay: a field left out is untouched.
- [ ] **Step 2:** Run the classes; expected: compile failure. Implement. Item creation by text uses the resolution service.
- [ ] **Step 3:** Run both projects; expected: all pass.
- [ ] **Leave uncommitted.**

---

### Task 7: Reading the lists, and the change base

**Files:** create `Queries/Metadata/QueryArtistList.cs`, `QueryAlbumList.cs`, `QueryGenreList.cs`, handlers, `Models/Metadata/` library models for ETag and 304 (mirror `PlaylistLibrary`), the three controllers with `GET`, `ResponseArtist`/`ResponseAlbum`/`ResponseGenre`, the three `Notification…Updated`/`…Deleted` pairs and their `HandlerNotification…` classes in `API/Handlers`, and the abstract change base `HandlerMetadataChange` (shared commit, publish and "touch the files" logic); tests per handler, the notification handler tests, `ArtistFlowTest.cs`-style end-to-end tests

- [ ] **Step 1: Write the failing tests.** List: items newest first with their file counts; the ETag is strong and over each item's ID and modified time (a rename changes it, another user's item does not); a matching `If-None-Match` is a `304`; an empty list is `200` with `[]`; counts include only the user's own files. The base: `TouchFiles` sets the modified time of exactly the files whose response changed and nothing else; notifications publish after the commit with the token; a failed hub push never fails the request and is logged (existing base behaviour); the hub event names are `nameof(MediaHubEvent.ArtistUpdated)` and so on.
- [ ] **Step 2:** Run the classes; expected: compile failure. Implement; the three kinds share one generic query handler shape where they are identical, with kind-specific thin classes only where names or responses differ.
- [ ] **Step 3:** Run both projects; expected: all pass.
- [ ] **Leave uncommitted.**

---

### Task 8: Create, rename and delete, and the settings

**Files:** create `Commands/Metadata/CommandArtistCreate.cs`, `CommandArtistRename.cs`, `CommandArtistDelete.cs` and the same for genres and albums (an album's rename is `CommandAlbumUpdate(UserID, ID, Optional<string?> Title, Optional<List<Guid>> ArtistIDList)`), handlers, requests with `ToCommand`, controller actions (`POST /Artist`, `PATCH /Artist/{id}`, `DELETE /Artist/{id}`, and the same for the other kinds); `EntityUserSettings` use cases `QuerySettings`, `CommandSettingsUpdate(UserID, Optional<bool> AutoMergeOnRename)`, `SettingsController` (`GET`, `PATCH`), `HandlerNotificationSettingsUpdated`; tests per handler and end-to-end

- [ ] **Step 1: Write the failing tests.** Create: the name is trimmed and keyed; blank is a `400`; a name whose key exists for that user is a `409` naming the existing item; the same name for another user is allowed; at most 255 characters. Rename: changes the name and key and bumps the item and **every file that has it**; renaming to the same key with different case or spacing only changes the spelling and is not a collision; a collision with `AutoMergeOnRename` off is a `409` carrying the other item's ID; with it on, the renamed item is merged into the existing one (the existing keeps its name, links move, the renamed item is removed, its files are touched and `…Deleted` and `…Updated` are published). Album artists change recomputes the identity key and collides under the same rule. Delete: unlinks the item from its files (files stay, are touched and pushed with `MediaUpdated`), publishes `…Deleted`; another user's item is a `404`. Settings: `GET` returns the defaults with no row, the first `PATCH` creates the row, a left-out field is untouched, a change publishes `SettingsUpdated`, an unchanged `PATCH` neither commits nor publishes, the ETag changes with the modified time, and another user's settings are never reachable.
- [ ] **Step 2:** Run the classes; expected: compile failure. Implement, sharing artist and genre logic once.
- [ ] **Step 3:** Run both projects; expected: all pass.
- [ ] **Leave uncommitted.**

---

### Task 9: Assign, remove, merge and split

**Files:** create `CommandArtistMediaFileListAdd.cs`, `…Remove.cs`, `CommandArtistMerge.cs`, `CommandArtistSplit.cs` (genres the same; albums have `MediaFileListAdd` (sets the album, replacing the file's current one), `Remove` and `Merge`), handlers, requests, controller actions (`POST /{kind}/{id}/MediaFileList`, `.../MediaFileList/Remove`, `.../Merge`, `.../Split`); tests per handler and end-to-end

- [ ] **Step 1: Write the failing tests.** Assign: adds the item to the files at the end of their list without duplicating an existing link; an unknown or another user's file is a `404` and nothing changes; an album assignment replaces the file's album; assigning what is already there is a no-op (no commit, no push). Remove clears only that item from those files. Merge: moves every link of the source to the target without duplicates, removes the source, bumps the files, publishes `…Deleted` for the source and `…Updated` for the target; merging into itself is a `400`; for albums, merging two albums with different artist sets keeps the target's artists. **Split** (artist and genre): `{ "nameList": ["A", "B"] }` on an item named "A & B" links every file that had it to A and to B (reusing items that exist, creating the rest, by the same key rules), in the original position, removes the item, bumps the files; a name list that resolves to the item itself keeps it; fewer than two names is a `400`; a split of an artist also replaces it in every album's artist list. Another user's item is a `404`.
- [ ] **Step 2:** Run the classes; expected: compile failure. Implement on the shared change base.
- [ ] **Step 3:** Run both projects; expected: all pass.
- [ ] **Leave uncommitted.**

---

### Task 10: Backfill the existing library

**Files:** create `BackgroundServices/MetadataBackfillService.cs`; modify `Application/ServiceCollectionExtensions.cs`; tests `MetadataBackfillServiceTest.cs` (unit, fakes) and an integration test over a real database

- [ ] **Step 1: Write the failing tests.** For files whose legacy text columns are set and that have no links: artists, album and genres are created and linked by the same rules as an upload (split, keys, album identity); a file already linked is skipped; **running the service twice gives the same items and links**; empty, whitespace-only, separator-only (`";"`) and 400-character text create nothing broken (the long one is cut to 255); a file with legacy text and no title keeps its title; two users with the same artist text get two separate artists; processing is in batches of 200 files, each batch one commit, and a failure in a batch is logged and the next start-up resumes it; no file's response text changes except through the new derivation (a test compares the text before and after for plain cases).
- [ ] **Step 2:** Run the classes; expected: compile failure. Implement as a hosted service that runs once at start-up after the migration, logs how many files it linked, and is safe to run concurrently with uploads (it only touches files without links).
- [ ] **Step 3:** Run both projects; expected: all pass.
- [ ] **Leave uncommitted.**

---

### Task 11: Writing the items into downloads, documents and the acceptance run

**Files:** modify `MediaTagWritingService` (and its test), the CQRS design doc and the metadata design doc (status and the settings-on-first-`PATCH` call); create `MetadataFlowTest.cs` (integration)

- [ ] **Step 1: Write the failing tests.** The tagged download writes the derived text for artist, album artist, album and genre, and where the format supports several values it writes them as separate values (read back with TagLib); an item renamed after upload shows its new name in the next download; files without links download as before.
- [ ] **Step 2:** Implement. Then write the end-to-end `MetadataFlowTest`: upload three tagged files of one album with a "feat." credit through HTTP; assert the library shows one album, the right artists and no duplicate artist; rename an artist and see the library ETag change and the `MediaUpdated` and `ArtistUpdated` pushes; with the setting off a colliding rename is a `409`, with it on it merges; split an "A & B" artist; delete a genre and see the files keep existing without it; another user sees none of it.
- [ ] **Step 3:** Update the metadata design doc: `GET /Settings` returns defaults with no row and the row is created by the first `PATCH` (this replaces "created on first read"). Build the whole solution (0 warnings), run both test projects, and report the unit and integration counts before and after the plan and every difference.
- [ ] **Leave uncommitted.**
