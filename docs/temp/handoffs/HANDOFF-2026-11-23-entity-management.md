# Handover, 2026-11-23: EntityManagement as a set of libraries, and the shared UI work

Written so a new session on another machine can carry on without the old conversation. Read "Do this first", then "How to work with the user", then the part you need.

## Do this first

1. **Everything is committed** (the original work, then the context query, the contract check and the `em-contract` program in the later commit `ca5eb9c`). Push before leaving this PC if it is not pushed yet. "Suggested commits" below is only history now.
2. The old EntityManagement project was **moved out of the repository** to `C:\Projects\Noxy.NET.EntityManagement.Old` (a plain folder next to the repo, not in git). It is still in git history at `e126186` and before, so on the new PC get it with `git worktree add ../Noxy.NET.OldCheckout e126186` (a full checkout of the repository at that commit; the old project is then in its `Noxy.NET.EntityManagement` folder; remove it with `git worktree remove ../Noxy.NET.OldCheckout`), or read single files with `git show e126186:<path>`. Read it only to copy things out (authentication, the JWT service, the `Presentation` features). Delete it when nothing is left to copy.
3. The assistant's memory (`C:\Users\<user>\.claude\projects\...\memory`) does **not** travel with the repository. What matters from it is in this file, under "How to work with the user".

## Setup on the new PC

- .NET SDK **10.0.404** (net10.0). No `global.json`. `dotnet-ef` global tool, version 10.0.8 (it warns that it is older than the 10.0.15 runtime; that is harmless): `dotnet tool install --global dotnet-ef`.
- `dotnet test` does **not** work here (xUnit v3 on the .NET 10 SDK needs the new test experience, which is not enabled). Build the test project and run the executable instead (see "Commands").
- The `wasm-tools` workload was needed for the old Administration and Frontend; the new libraries do not need it. The MediaPlayer app builds for Windows only (`-f net10.0-windows10.0.19041.0`); its Android target needs the Android SDK and fails without it. That is unrelated to this work.
- Close Visual Studio before moving or renaming project folders: it holds directory handles and the move fails with "Permission denied".

## How to work with the user

- **A question gets an answer, not edits.** If the user asks "why", "what do you mean", or for an explanation, explain and stop. Make a change only when they ask for it. (They were annoyed more than once when a question led to edits.)
- **Do not ask about technical plumbing.** The user does not want to choose between internal options (storage classes, indexes, helper structure). Pick the recommended option, do it, and say in one line what was decided. Ask only about things that change what the system does for them. They said so directly: "I honestly don't know what I'm really answering so I'm going with your recommendation."
- **Do not rename or restyle anything without a reason they have agreed to.** They are fine with both "definitions" and "schema"; the code says "definitions".
- **Prefer framework built-ins** over custom code (for example `[Required]`, `[MaxLength]`).
- **Start slowly, one library at a time**, and show the result before the next one. They like that each step builds and is tested before the next.
- **Naming is family first, then the thing:** `DefinitionElement`, `DataElement`, `HistoryElement`. Constants go in a `Constants` folder, and only the definition file itself is in the root of the demo project.
- **Developer tools are programs, not tests.** A test only checks. Anything that changes a file (the contract lock) is a small console program. When it needs paths, with no arguments it asks, offering a default, so it can be started from PowerShell or by double-click.
- **Product names, not the developer's.** User-visible names (the command `em-contract`, prompts, settings folder `%LOCALAPPDATA%\EntityManagement`) say EntityManagement/`em-`, never Noxy. The namespaces and packages stay `Noxy.NET.EntityManagement.*`; that is not to be renamed.
- **Do not pick the "plumbing" for them, but do say the rule.** Example: they set the rule "nothing changes by accident, but it is easy on purpose" and the add/accept split came from that.
- Windows + Git Bash gotchas: shell quoting of backticks and backslashes goes wrong inside `node -e` and `sed`, so write a script file and run it. Many repository files have CRLF line endings; the `Edit` tool handles single-line edits fine, but multi-line replacements in a script should normalise `\r\n` first.

## EntityManagement: what it is and what was decided

The full design is in `docs/specs/2026-11-23-entity-management-libraries-design.md`. Read it. In short:

- **A set of libraries, not an application.** An implementer writes a definition library (the schema in code), an API host and a frontend; the libraries supply everything normal. The casework/investigation system (cases, persons, vehicles, weapons, locations) is only a **demo** built on the libraries from outside.
- **The schema is code**, written as a file of `Add...` calls run at startup (like the old seeds), with identifiers as constants. Nothing about the schema is in the database.
- **Storage is generic and rarely migrates**: elements, typed property rows (one table per type), relations, text values. A new element kind is a definition, not a migration. The aim is a base system others can build on without a base update breaking their database.
- **History is separate tables**: the current tables hold only current rows; a change creates a new row and the old one moves to the history table in the same transaction.
- **Relations are a table** (chosen over a "reference property" because they expect to do more with them), with attributes stored as ordinary property rows owned by the relation.
- **A context is only a flat group of elements** that belong together (one level, no depth, no include tree). Given a starting element, a page gets that element plus the elements directly related to it whose kind is in the context, grouped by relation. Without a starting element it lists the elements of a kind, paged. "Persons in more than one case" is a separate query, not a context.
- **Texts**: every user-facing text goes through a text parameter. The definition says only that a text exists and its kind (no default text: the user called that a trap). The defaults are registered separately per culture in texts files, checked against the definition, with an English fallback. The database holds **only what people have edited**; an edit comes before the default; nothing is copied from the files into the database (no startup sync). Multi-language is supported from the start; the demo only ships English.
- **Priority**: a rebuilt, generic Administration that shows the contract (read-only) and lets data be created, changed and deleted, to test the system in real time. It reads the contract from the API, so one Administration works for any implementation. The casework frontend comes after.
- **Audit trail and access control are later.** `CreatedByUserID` is recorded on every row from the start (a `Guid` with no user table yet).

### Libraries and what is built (all in `Noxy.NET.sln`, folder `EntityManagement`)

| Project | State |
|---|---|
| `Noxy.NET.EntityManagement.Definitions` | Done for now: the builder (`AddTextParameter`, `AddProperty`, `AddElement`, `AddRelation`, `AddContext`), `DefinitionRegistry`, `DefinitionTextBuilder`/`DefinitionTextRegistry`, culture and identifier rules, `DefinitionException` listing every problem. Range constraints are not built (only required and maximum length). **The contract:** `GetContractLineList()` (extension), `DefinitionContractFile` (`Check`, `Update(registry, acceptBreaks)`, `Verify`), `DefinitionContractResult`, `DefinitionContractException`. See "The contract" below. |
| `Noxy.NET.EntityManagement.Contract` (builds `em-contract.exe`, tool command `em-contract`) | A small console program that writes the contract file: `check`, `add`, `accept`. With no arguments it asks for the command, the definition assembly and the contract file, with defaults (the last answers, else the newest built `*.Definitions.dll` under the current folder and `Contract.txt` in its project folder); with arguments it runs at once and returns exit code 0, 1 (differs) or 2 (could not run). Loads the assembly with its own dependencies; the Definitions library is shared with the program. Run with `dotnet run --project Noxy.NET.EntityManagement/Noxy.NET.EntityManagement.Contract`. |
| `Noxy.NET.EntityManagement.Domain` | Plain classes: `DataElement`, `DataProperty*` (Boolean, DateTime, Decimal, Image, Integer, String), `DataRelation`, `DataTextParameter`, their `History*` twins, and the repository contracts `IDataRepository`, `IRelationRepository`, `ITextRepository`. |
| `Noxy.NET.EntityManagement.Persistence` | EF Core + SQLite: 18 tables (9 current, 9 history), unique indexes, no foreign keys between data tables, the `Initial` migration, `DataRepository`, `RelationRepository`, `TextRepository`, `AddEntityManagementPersistence(...)` and `MigrateEntityManagementAsync()`. |
| `Noxy.NET.EntityManagement.Application` | `ITextService`, `IElementService`, `IRelationService` with validation against the definitions; `AddEntityManagementApplication(options => ...)` runs and checks the definitions and texts at startup. **`IContextService`** (done): `GetElementPage(context, kind, skip, take)` and `GetContext(context, elementID)`, which returns the starting element and a group per relation and direction (`Forward` or `Inverse`) with the related elements of the context's kinds, in three queries (needs `IDataRepository.GetElementList(idList)`). A group is there when the definition allows it, also when empty; two relations between the same kinds are two groups; an element of a kind outside the context throws `DataValidationException` with `WrongElement`. |
| `Noxy.NET.EntityManagement.Demo.Definitions` (folder `Noxy.NET.EntityManagement.Demo`) | The demo schema: person, case, `person-involved-in-case` (required role, optional note), contexts `people` and `case-file`, English texts. |

Tests (all in `Noxy.NET.Test/Noxy.NET.Test.UnitTest/EntityManagement`): Definitions 114, Persistence 96, Application 158, Contract 11. These namespaces pass; the whole suite was last run in full before the context query and the contract work (2,696 tests then), so run it once on the new PC.

### The contract (done)

The rule "definitions only grow" is enforced by a contract file of locked lines, one per fact stored data depends on (every identifier, the kind of each property and text, the properties of each element and relation, what a relation joins and its cardinality; titles, order, required and maximum length are not in it). The demo's file is `Noxy.NET.EntityManagement.Demo/Noxy.NET.EntityManagement.Demo.Definitions/Contract.txt`, and `CaseworkContractTest` fails when the definitions differ from it.

- **A test only checks and never writes.** Writing is the `em-contract` program: `add` locks new lines and refuses a removal or change; `accept` makes the file the definitions as they are (use only when the data that uses the old identifier can go, there is no migration).
- Do not suggest "run the test with a flag to update the lock". The user wants a small program, with prompts and defaults, not a test.
- The README of the Definitions library has the workflow.

### Not built yet, in the order I would do it

1. **The API library** (`Noxy.NET.EntityManagement.Api`, a library, not a host): standard endpoints (sign-in, contract, elements, relations, history, context query, texts) with one method to register and one to map, plus a small demo API host. Decide controllers or minimal endpoints then.
2. **Authentication**: not built in the new libraries. The old project had JWT and authentication tables and services (look in `.Old`: `Application/Services/JWTService.cs`, `Persistence/Tables/Authentication`). `CreatedByUserID` has no user table behind it yet.
3. **The Administration, rebuilt** (generic, reads the contract from the API): contract view, data browser and editor with an input per property kind, history and as-of view, a live context preview, editing text values. This is the user's stated priority.
4. **The demo frontend**, then the rest of the demo schema (vehicles, weapons, locations).
5. **A client library** (`Presentation`) with generic components, when the Administration needs it.

### Open decisions (also in the spec under "Still open")

- Whether a project may add a property to a base element (the leaning is no; offer a related extension element instead).
- **Style and System parameters** of the old project: not carried over, and nothing in the new design uses them yet. Decide if they are needed.
- Whether the Administration ships as a runnable app only, or also as pages an implementer can host (the proposal: pages as a library, the app a thin host).
- How the API library exposes endpoints (controllers or minimal endpoints).
- How an `Image` property is stored: the value is only a string reference; where images live is not designed.
- `Decimal` is stored as text in SQLite, so it round-trips exactly but cannot be compared or sorted in SQL. That matters when search and filtering are built.

## The rest of this session (outside EntityManagement)

- **Colours and the shared variables.** `Noxy.NET.UI.Shared/wwwroot/css/variables.css` is now the complete set of variables the shared components read (sizes, fonts, motion, colours). Colours are pairs (`--color-warning` with `--color-on-warning`), handpicked per theme with `light-dark()`, with derived `-hover`, `-active`, `-subtle`, `-border` shades; `color-scheme` follows the `theme` attribute. The user's original fills are kept (only `default` is nudged in dark). The MediaPlayer, EntityManagement-Administration (now gone) and LewdFilter `variables.css` files use it. `Button` and the textual input are converted to the pairs; **about 23 other shared component stylesheets are not** (MessageBox, ToastRenderer, the select, checkbox, radio, table, and so on).
- **Wizard** was rolled into `Noxy.NET.UI.Shared` (`Components/Containers/Wizard*.razor`, `Models/WizardContext.cs`), rewritten and tested; the separate `Noxy.NET.UI.Wizard` project is deleted. Dialog and Graphics (unused, not in the solution) and the empty `UI.Elements/Table/WebForm` folders are deleted.
- **Colour models** (`Models/Colors`): fixed culture-dependent CSS output, infinite recursion in some conversions, a crash for `HexColor` with alpha, parsers for `oklab`/`oklch`, lost alpha after a spaced slash, and `grad` angles; 137 tests added. **Still open:** `lab()` and `lch()` are computed against D65 while CSS defines them against D50, so those strings do not match what a browser draws.
- **LewdFilter:** `<Button type="button">` in `GroupEditor.razor` bound the string to the enum parameter and did not compile; fixed with `Type="@HTMLButtonTypeEnum.Button"`.
- **ThemeService (UI Shared), unresolved question.** The user wondered whether the service does a job the browser should do. The recommendation (not done, awaiting an answer): keep it only for the user's override, stop copying the system theme onto the `theme` attribute (which freezes the page to the OS setting at load), and give the toggle a "System" choice.
- Other known loose ends in `Noxy.NET.UI.Shared`: hardcoded English strings ("Dismiss" in `ToastRenderer`, "Loading..." in the autosuggest), `100ms` transition literals in three input stylesheets, about 28 stale `UpToDateCheckInput` entries in the csproj, the package version (0.1.20) not bumped, and the title/README still say `Noxy.NET.UI.Common` while the project is `Shared`. `MediaPlayer.App/Resources/Raw/sign-in-complete.html` keeps its own copy of colour values.
- **One flaky test**, not related to this work: `PlaybackControlServiceTest.Seek_ADragOfManySteps_SendsOneCommand_WithTheLastValue_After300ms` (a 300 ms debounce) failed once under load and passed on every rerun.

## Suggested commits

The tree is large, so commit in pieces that each build:

1. `Noxy.NET.UI.Shared` + the three `variables.css` files + `LewdFilter` fix + the colour model fixes and tests (the shared UI work).
2. Delete `UI.Wizard`, `UI.Dialog`, `UI.Graphics` (+ `Noxy.NET.sln` entries) and the Wizard added to Shared.
3. Remove the old `Noxy.NET.EntityManagement` (the 780 deleted files, and its solution entries).
4. Add the new `Noxy.NET.EntityManagement/*` libraries, `Noxy.NET.EntityManagement.Demo`, the tests under `Noxy.NET.Test.UnitTest/EntityManagement`, and the spec and this handover.

Git will show some new files as *modified*, because they sit at the same paths as old tracked files (the new `Domain` and `Persistence` project files, `DataRepository.cs`, `ServiceCollectionExtensions.cs`). That is expected.

## Commands (from the repository root)

```
# build the tests, then run everything
cd Noxy.NET.Test/Noxy.NET.Test.UnitTest && dotnet build -v q
./bin/Debug/net10.0/Noxy.NET.Test.UnitTest.exe

# only one area (names are namespaces or classes)
./bin/Debug/net10.0/Noxy.NET.Test.UnitTest.exe -namespace "Noxy.NET.Test.EntityManagement.Application"
./bin/Debug/net10.0/Noxy.NET.Test.UnitTest.exe -class "Noxy.NET.Test.EntityManagement.Persistence.RelationRepositoryTest"

# change the tables: edit the Domain classes and Persistence/Configurations, then regenerate.
# While nothing is deployed, delete the Migrations folder and recreate Initial; afterwards delete design-time.sqlite*
rm -rf Noxy.NET.EntityManagement/Noxy.NET.EntityManagement.Persistence/Migrations
dotnet ef migrations add Initial --project Noxy.NET.EntityManagement/Noxy.NET.EntityManagement.Persistence
```

A test (`TheMigrations_MatchTheModel`) fails if the model changed and the migration was not regenerated. Once something is deployed, migrations are additive only (new tables, nullable columns, indexes).

## Where things are

- Spec (read first): `docs/specs/2026-11-23-entity-management-libraries-design.md`.
- Each library has a README that explains its rules: `Noxy.NET.EntityManagement/*/README.md`.
- Shared UI notes: `Noxy.NET.UI/Noxy.NET.UI.Shared/README.md` (colours, the Wizard, the variables file).
- Older handover and plans (the MediaPlayer app, not this work): `docs/temp/handoffs/HANDOFF-2026-11-17-plan-5.md` and `docs/temp/plans`.

## Outside EntityManagement, since the first handover

- A review of the `::deep` CSS selectors found two dead rules (`.show .visual` in `BlockEditor`, `.new-playlist` in `SidebarElement`); both were removed. The ones left are fine on purpose: `::deep + input` in the two `InputVisual` components, and the descendant selectors after `::deep > child` in `InputControlComposite`.
- The user was about to work on the **MediaPlayer** next (the Dialog, `UploadMediaDropZone` and `Button.razor.css` changes were in progress, uncommitted at the time this was written; check `git status`).
