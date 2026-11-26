# Weights for artists and genres Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Artists and genres get a weight; a song's artists and genres are ordered by it everywhere, including the tags written to files.

**Architecture:** The order is worked out once on the server in `EntityMediaFileExtension` (weight descending, then the song's own `Position`). Responses and tag values carry the result, so devices follow with no logic of their own. A weight change is a metadata change handled like a rename: it touches and pushes the files that carry the item.

**Tech Stack:** .NET 10, EF Core + migrations (API must be stopped), Mediator commands/handlers, xUnit v3 (`dotnet run` in the test project, filter `-class <full name>`), Blazor/Fluxor client.

**Spec:** `docs/specs/mediaplayer/2026-11-26-metadata-weights-design.md`

## Global Constraints

- Never commit (project memory): `git add -N` new files instead. Migrations and DB deploy are fine inside a plan (project memory).
- Pre-production: no backfill or compatibility code. Default weight 100, a named constant (`MetadataWeight.Default`, plus `Minimum` 0 and `Maximum` 1000) in the Domain project.
- Weight is an `int`, valid range 0 to 1000, higher first, ties by `Position`. Album artists are not weighted.
- One class per file, no magic strings, texts in resource files, enums end in `Enum`.

## Review Focus

- A song whose items all have the default weight: order unchanged from today (Task 1).
- An item below the default (Soundtrack at 50) sorts after untouched ones (Task 1); a weight of 0 sorts last of all.
- Setting a weight on an item no song carries: succeeds, pushes only the item (Task 3).
- The same item on thousands of songs: all touched in one commit (Task 3).
- The text `Artist`/`Genre` and the ID lists of a song agree in order (Task 1).

---

### Task 1: The weighted order (Domain)

**Files:**
- Modify: `Noxy.NET.MediaPlayer.Domain/Entities/EntityArtist.cs`, `EntityGenre.cs` (add `public int Weight { get; set; } = MetadataWeight.Default;`, with the new `Domain/Constants/MetadataWeight.cs` holding `Default = 100`, `Minimum = 0`, `Maximum = 1000`), `Domain/Responses/Metadata/ResponseArtist.cs`, `ResponseGenre.cs` (add `public required int Weight { get; init; }`), `Domain/Extensions/EntityMediaFileExtension.cs` (lines using `OrderBy(x => x.Position)` for artists and genres), the extensions that build `ResponseArtist`/`ResponseGenre` (find with `grep -rn "new ResponseArtist\|new ResponseGenre"`)
- Test: `Noxy.NET.Test/Noxy.NET.Test.UnitTest/MediaPlayer/Media/EntityMediaFileExtensionTest.cs` (create if absent, in the folder of the nearest existing extension tests)

**Interfaces:**
- Produces: `EntityArtist.Weight`, `EntityGenre.Weight`, `ResponseArtist.Weight`, `ResponseGenre.Weight`; `GetArtistNameList()`, `GetGenreNameList()`, `ToResponse()` and `ToTagValues()` of `EntityMediaFile` return artists and genres ordered by `Weight` descending then `Position`.

- [ ] **Step 1: Write the failing tests** with a song carrying links: `ArtistNameList_OrdersByWeightDescending`, `ArtistNameList_WithEqualWeights_KeepsThePositionOrder`, `GenreNameList_LowerWeightComesLast` (Soundtrack 50, Pop at the default 100, Rock 200 → Rock, Pop, Soundtrack), `Response_ArtistText_IDList_AndTagValues_AgreeInOrder`, `AlbumArtistNameList_IsNotWeighted`.
- [ ] **Step 2: Run:** `cd Noxy.NET.Test/Noxy.NET.Test.UnitTest && dotnet run -- -class <the test class>`. Expected: build errors (no `Weight`), then failures.
- [ ] **Step 3: Implement** the two properties, the response fields, and one private ordered view per kind in `EntityMediaFileExtension` that every artist/genre read there uses (`OrderByDescending(x => x.Artist.Weight).ThenBy(x => x.Position)`; the loaded-item guard stays).
- [ ] **Step 4: Run the class, then the whole unit suite.** Expected: pass (fix the `new ResponseArtist`/`ResponseGenre` call sites the compiler lists).
- [ ] **Step 5:** `git add -N` new files.

### Task 2: Persistence and migration

**Files:**
- Modify: `Noxy.NET.MediaPlayer.Persistence/Tables/Metadata/TableArtist.cs`, `TableGenre.cs` (find with `grep -rn "class TableArtist"`) adding `public int Weight { get; set; }`; `Persistence/Mappers/EntityToTableMapper.cs`, `TableToEntityMapper.cs`; the repository queries that build `ResponseArtist`/`ResponseGenre` or order link lists by `Position` for artists/genres (`grep -rn "Position" Persistence/Repositories`).
- Create: a migration `AddMetadataWeight` (EF tool, in `Persistence/Migrations`).
- Test: the existing repository/integration tests that cover artists and genres (`Noxy.NET.Test.IntegrationTest`).

**Interfaces:**
- Consumes: Task 1 properties. Produces: persisted `Weight` (default 100) that round-trips through the mappers.

- [ ] **Step 1: Write the failing test** (integration, next to the existing artist/genre repository tests): `Weight_RoundTrips_ForArtistAndGenre` (add with weight 7, load, expect 7) and `LinkedItems_AreLoadedWithTheirWeight` (a song's artist links come back with `Artist.Weight`).
- [ ] **Step 2: Run:** `cd Noxy.NET.Test/Noxy.NET.Test.IntegrationTest && dotnet build -p:BuildProjectReferences=false` after building the Application and Persistence projects, then `dotnet run --no-build -- -class <class>`. Expected: failure.
- [ ] **Step 3: Implement** columns and mappers; ensure the includes that load a song's links also load the item (they must, the accessors already require it).
- [ ] **Step 4: Stop the API, then generate the migration:** `dotnet ef migrations add AddMetadataWeight --project Noxy.NET.MediaPlayer/Noxy.NET.MediaPlayer.Persistence --startup-project Noxy.NET.MediaPlayer/Noxy.NET.MediaPlayer.API`. Expected: a migration adding two `int NOT NULL DEFAULT 100` columns (existing rows get 100).
- [ ] **Step 5: Run** the integration class and suite. Expected: pass.
- [ ] **Step 6:** `git add -N` new files.

### Task 3: Setting a weight (Application and API)

**Files:**
- Create: `Application/Commands/Metadata/CommandNamedItemSetWeight.cs` (abstract record `(Guid UserID, Guid ID, int Weight)` like `CommandNamedItemRename`), `CommandArtistSetWeight.cs`, `CommandGenreSetWeight.cs`, `Application/Handlers/Metadata/Artist/HandlerArtistSetWeight.cs`, `Genre/HandlerGenreSetWeight.cs`, the API endpoints next to the rename ones (follow the route and request naming of `ArtistRename`/`GenreRename` in `Noxy.NET.MediaPlayer.API`)
- Modify: `Application/Interfaces/Repositories/IMetadataRepository.cs` only if an update method for the weight is missing
- Test: `Noxy.NET.Test.UnitTest/MediaPlayer/Metadata/HandlerArtistSetWeightTest.cs`, `HandlerGenreSetWeightTest.cs` (model them on the existing rename handler tests)

**Interfaces:**
- Consumes: Task 1 and 2. Produces: commands `CommandArtistSetWeight(Guid UserID, Guid ID, int Weight)` and `CommandGenreSetWeight` returning the updated `ResponseArtist`/`ResponseGenre`; an HTTP endpoint for each.

- [ ] **Step 1: Write the failing tests** per handler: `SetWeight_ChangesTheWeight_AndPushesTheItem`, `SetWeight_TouchesAndPushesTheFilesThatCarryTheItem` (their `TimeModified` moves, a `NotificationMediaFileUpdated`-style push per file, with the new order), `SetWeight_OutOfRange_Throws` (-1 and 1001; the type the rename handler uses for invalid input), `SetWeight_ForAnotherUsersItem_ThrowsNotFound`, `SetWeight_ToTheSameValue_ChangesNothing`, `SetWeight_OnAnItemNoSongCarries_PushesOnlyTheItem`.
- [ ] **Step 2: Run** the two classes. Expected: build errors, then failures.
- [ ] **Step 3: Implement** the handlers following `HandlerArtistRename`/`HandlerGenreRename` (same unit of work, change set and touch helpers); validate the range with `MetadataWeight.Minimum`/`Maximum`; endpoints and request models next to the rename ones.
- [ ] **Step 4: Run** the classes and the unit suite. Expected: pass.
- [ ] **Step 5:** `git add -N` new files.

### Task 4: The tags follow the weights

**Files:**
- Test: `Noxy.NET.Test.UnitTest/MediaPlayer/Presentation/FileTagServiceTest.cs`, `Noxy.NET.Test.IntegrationTest/MediaPlayer/Media/MediaTagWritingServiceTest.cs`, and the upload processor test that covers re-tagging (`grep -rn "ToTagValues" Noxy.NET.Test`)

**Interfaces:**
- Consumes: Task 1 (the server sends `ArtistIDList`/`GenreIDList` in weighted order; `ToTagValues` uses the ordered view).

- [ ] **Step 1: Write the tests:** `ToTagValues_GenresFollowTheWeights` (server side, Task 1's view), `Update_WritesTheGenresInTheOrderOfTheSongsGenreIDList` (client `FileTagService`: a media file whose `GenreIDList` is [Rock, Soundtrack] gives tag values [Rock, Soundtrack]), `Write_KeepsTheGivenGenreOrder` (writer: three genres, raw TCON frame in the same order).
- [ ] **Step 2: Run** the three classes. Expected: pass, since Tasks 1 to 3 already provide the order; any failure is a real gap to fix in the place the failing test names.
- [ ] **Step 3:** `git add -N` new files.

### Task 5: Client: models and the Organise dialog

**Files:**
- Modify: `Presentation/Models/MetadataItem.cs` (add `int Weight`), `Presentation/Extensions/FeatureMetadataListStateExtension.cs`, `Presentation/Interfaces/Services/IMetadataActionService.cs` and `Services/MetadataActionService.cs` (`Task SetWeight(MetadataKindEnum kind, Guid id, int weight)` for artist and genre), the API client interface and implementation (`IApiClient`, `ApiClient`) for the new endpoints, the cached metadata store (`Storage/Stores/SqliteListStore.cs` and `SqliteMetadataStore.cs`) if it stores columns, `App/Components/Dialogs/MetadataItemDialog.razor` (a weight number field for the Artist and Genre kinds, saved with the name), `App/Resources/AppTexts.resx` + designer
- Test: `Noxy.NET.Test.UnitTest/MediaPlayer/Presentation/MetadataActionServiceTest.cs` (existing, extend), the list store test (extend if the store has columns)

**Interfaces:**
- Consumes: the endpoints of Task 3 and `Weight` on `ResponseArtist`/`ResponseGenre`. Produces: `IMetadataActionService.SetWeight`.

- [ ] **Step 1: Write the failing tests:** `SetWeight_ForAnArtist_CallsTheApiWithTheWeight`, `SetWeight_ForAnAlbum_Throws` (not supported), `TheListStore_RoundTripsTheWeight` (only if columns are stored).
- [ ] **Step 2: Run** the classes. Expected: failures.
- [ ] **Step 3: Implement** the model, the service method, the API client calls, the dialog field (an integer `InputControl` bound to the item's weight, shown only for Artist and Genre, saving through `SetWeight` when it changed), and the texts.
- [ ] **Step 4: Build the app:** `dotnet build Noxy.NET.MediaPlayer/Noxy.NET.MediaPlayer.App/Noxy.NET.MediaPlayer.App.csproj -f net10.0-windows10.0.19041.0` (close a running debug session first). Expected: Build succeeded.
- [ ] **Step 5:** `git add -N` new files.

### Task 6: Verification

- [ ] **Step 1:** Run both test projects in full. Expected: 0 failed.
- [ ] **Step 2:** Hand the user the by-hand list from the spec's Tests section (set Soundtrack low, check the player bar, table and edit dialog, write the tags, read them in another player). Remind them to restart the API after the migration.
