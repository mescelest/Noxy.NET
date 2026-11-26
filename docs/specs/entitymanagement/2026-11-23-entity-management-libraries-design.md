# Entity management as a set of libraries: schema in code, data with history, relations: design

Priority: the rebuilt Administration, as the way to test the data in real time. The detailed shape of each phase is confirmed before it starts.

## Intent

EntityManagement is **a set of libraries for building a system** that holds data described by a schema (contexts, elements, properties) with relations and history. It is not itself the system. The implementer writes three things: a **definition library** (their schema, in code), an **API host** and a **frontend**. The libraries supply everything "normal" (setup, storage, validation, the standard endpoints, the client services and components) so those three stay small, and the implementer can add their own endpoints and screens beside them.

Today it is an application where the schema lives in the database and can be changed while the system runs, which would never happen in a real deployment, and most of the code (about 240 Domain files, the schema controller, the schema builder, the whole Administration editor) exists to manage it. The redesign moves the schema into code, makes the data the focus, adds relations, and is built as a **new set of libraries, started from scratch and slowly**, not as a rework of the old projects. On 2026-11-23 the old EntityManagement was moved out of this repository (it is in git history, and as a plain folder at `C:\Projects\Noxy.NET.EntityManagement.Old`) to copy from where that helps, for example the authentication and the JWT service. The new libraries use the name `Noxy.NET.EntityManagement`.

A **demo implementation** is built on top of them to show and test the libraries: a case management system for investigations (cases with persons, vehicles, weapons and locations, and the inverse view of which cases a person appears in). It is only a demo. It lives outside the libraries and uses them the way any implementer would.

## Decisions

- **Libraries, not an application.** The implementer creates the definition library, the API and the frontend. The libraries supply the methods for everything normal. Whatever is specific to a system is the implementer's.
- **The demo is separate.** The casework system is a demo project that uses the libraries from the outside. Nothing casework is in the libraries.
- **The schema is code.** Contexts, elements, properties and relations are definitions in C# with stable string identifiers (for example `person`, `case`, `person-involved-in-case`). Changing the schema is a new deployment, not an edit. Nothing about the schema is stored in the database.
- **The storage stays generic and rarely migrates.** The database holds only foundation tables (elements, typed property rows, relations, parameter values, authentication). A new element is a new definition, not a migration. This is deliberate: a base system can ship and others can build on it, and a base-system update must not change anything foundational in a database that a project has built on. Draft and future schemas that could be switched between are dropped as not viable. The foundation migrations ship inside the persistence library.
- **Data has a current and a historical version, in separate tables.** The current tables (elements, property rows, relations) hold only what is current, so reading the current data never has to wade through history and stays fast. Changing a value creates a completely new row that becomes the current one, and the old row moves to the history table that mirrors that table (same columns, plus when it ended and who ended it), in the same transaction. Deleting an element or ending a relation moves its rows to history as well. History is only read when someone asks for it ("as of a date", the change history of an element). Who made the change is recorded from the start (a created-by user on every row) even though the audit and access screens come later.
- **Relations are a table.** `Relation(ID, RelationIdentifier, FromElementID, ToElementID, TimeCreated)`, with attributes (for example a person's role in a case) stored as ordinary property rows owned by the relation. The inverse is the same table read from the other side. An ended relation moves to the relation history table like any other row. Chosen over a Reference property because relations are expected to be used for more later.
- **Every text a user sees goes through a text parameter, so it can be translated, and the system supports several languages from the start.** A text parameter is defined in code with only an identifier and a kind (line, text or rich text); it has no text in it, because a default written in the definition is a trap: it hides which language it is in and makes one language look like the text itself. What the texts say is registered in separate files (a texts file for each language, or one for several), one call for each text and culture, run at startup like the definition and checked against it. The database only holds what a person has edited (one value for each text and culture, with history like any other data), through the Administration; an edit comes before the default, and a text says its default until someone edits it. Nothing is copied from the files into the database, so a newer default in a file reaches everybody who has not edited that text. Every text parameter must have a text in the fallback culture (English unless set otherwise). What is not translated: the names and identifiers of the definitions themselves (they are for developers). What is: the titles and descriptions shown for contexts, elements and properties, and the labels, buttons and messages of an implementer's application.
- **The Administration is generic and comes first.** It reads the contract (the definitions) from the API, so one Administration works for any implementation without referencing the implementer's definition library. It shows the contract read-only, and lets the data be created, changed and deleted, so a system can be tried in real time as an app would use it.
- **Audit trail and access control are later.** The history and the created-by column are the groundwork. Per-case access and roles are not designed yet.

## Shape of the libraries

| Library | Holds | Used by |
|---|---|---|
| `Definitions` (new, small) | the builder (`AddTextParameter`, `AddProperty`, `AddElement`, `AddRelation`, `AddContext`), the definition types, the registry, the validation. No EF, no ASP.NET. | the implementer's definition library, the API, the Administration, a frontend (for identifiers) |
| `Domain` | the data entities and their history twins, the repository contracts, the request and response models | Application, Persistence, Api, Presentation |
| `Application` | the services: create, change and delete elements and relations validated against the definitions, the context query, history and as-of reads, the text default sync | Api |
| `Persistence` | EF Core, the foundation tables and their migrations | Api |
| `Api` (a library, today a host) | the standard endpoints (sign-in, definitions, data, relations, history, context queries) as a method that maps them, and the setup method that registers everything | the implementer's API host |
| `Presentation` | the client services and state, and generic components (render and edit an element from its definition, show a context) | the Administration, an implementer's frontend |
| `Administration` | the generic admin pages and a runnable app | run as it is, or its pages hosted by the implementer |

What the implementer writes, and roughly how much:

```csharp
// the definition library: one file of Add... calls and the constants (see below)

// the API host
builder.Services.AddEntityManagement(options =>
{
    options.UseSqlite(connectionString);
    options.AddDefinition<CaseworkDefinition>();
});
app.MapEntityManagement();   // the standard endpoints; add your own beside them

// the frontend
builder.Services.AddEntityManagementClient(apiAddress);
```

The names are provisional. The point is that the normal parts are one call each, and the implementer's own endpoints and pages sit beside them. How the API library exposes its endpoints (controllers, or minimal endpoints mapped as a group the implementer can place under a prefix and put a policy on) is decided in phase 4.

## Goals

- **Definitions** (the `Definitions` library), keeping the structure the system already has: a **context** groups elements (which data belongs together, and which data a page queries), an **element** is a kind of thing (a person, a location) made of properties, and a **property** is a typed field (name, picture, age; address, picture, country). Elements and properties are reusable: one property definition can belong to several elements, and one element to several contexts (a person and a location can share a context). Relation definitions (identifier, from element, to element, forward and inverse labels, cardinality, attribute properties) link element instances. A registry holds them all.
- **The contract is locked.** A snapshot test holds every identifier and property kind of a definition. Removing or retyping an identifier fails the test. Definitions may only grow. Readers ignore properties they do not know and treat a missing one as empty, so a newer or older definition does not break stored data. The library supplies the check, so an implementer can put the same test on their own definition library.
- **Persistence:** a regenerated `Initial` migration (the one existing migration is not deployed): the current tables (elements, the typed property tables with created-by, the text parameter values with a unique index on the text and culture, the relation table), and a history twin for each of them with the same columns plus the time it ended and who ended it. A plain unique index on the owner and property identifier of each current table (one current row per property, no filter needed), and an index for reading relations from both ends. The history tables are indexed by owner and time. The history twins are mapped from one shared configuration so a new property kind gets its twin without extra code. The persistence library applies its own migrations (an option on startup).
- **Application:** create, change and end an element or a relation, validated against the definitions; read an element with its current properties, its history, and the relations it has in both directions; search; as-of reads; the context query.
- **API library:** a read-only definitions endpoint, element and relation endpoints with create, change and delete, history, and a context query (with or without a starting element), plus sign-in. Mapped and registered by one call each.
- **Administration (first priority):** (1) a read-only view of the contract: contexts with the elements they group, elements, properties with their kind, relations, text parameters; (2) a data browser and editor generated from the definitions, with an input per property kind, to create, change and delete elements and relations; (3) the history of an element and an as-of view; (4) a live context preview: pick a context and a starting element and see the data a page would receive; (5) editing the text parameter values.
- **Demo implementation** (after the Administration): the definitions for the investigation domain (Case, Person, Vehicle, Weapon, Location and their relations), a small API host, and a frontend: case list, case page with linked persons, vehicles, weapons and locations, an element page with its history and "appears in", search. Besides being a demo it is the proof that the libraries are enough to build a system.
- **Not carried over from the old project:** the schema entities, requests, repositories and tables, `SchemaController`, `SchemaBuilderService`, the schema-editing screens of Administration, and the old `Frontend` project. The Parameter definitions move to code (the text parameters as described above); the Parameter values stay data, as they are today. The style and system parameters are left as they are for now.

## Why this protects a project built on the base system

With the schema in code, a new base version cannot ask a project to migrate a schema that no longer fits its structure: there is no schema in the project's database. This holds only under these rules:

- **Definitions only grow.** An identifier is never removed, retyped or reused. A property that is no longer wanted is marked obsolete. The snapshot test fails the build otherwise. Structural safety is not meaning safety: changing what an identifier stands for breaks stored data just as a migration would.
- **The foundation tables migrate rarely and only additively:** new tables, new nullable columns, new indexes. Nothing is renamed, dropped or retyped.
- **A project extends with its own definitions** under its own identifier prefix, so its identifiers cannot collide with a later base version. Whether a project may add a property to a base element is open; the leaning is no, and to offer a related extension element instead, which keeps the base contract closed.

## How the definitions are written: a definition file that runs at startup

The way the schema is built stays as it is today: a file of calls, like the `Register(...)` calls of the text seed, with the identifiers in a constants class (like `ParameterTextConstants`). What changes is where the result goes: today the seeds write schema rows into the database, in the new model the same kind of calls fill an in-memory registry and nothing is written to the database. The definition file belongs to the implementer, in their definition library. The data stays generic: context, element, property, and nothing turns a stored element into a typed C# class.

```
// the implementer's definition library, run once at startup (names are provisional)
public void Define(IDefinitionBuilder builder)
{
    builder.AddTextParameter(CaseworkTexts.PersonTitle, TextKind.Line);
    builder.AddTextParameter(CaseworkTexts.PropertyNameTitle, TextKind.Line);

    builder.AddProperty(CaseworkProperties.Name, PropertyKind.String, CaseworkTexts.PropertyNameTitle, required: true, maxLength: 200);
    builder.AddProperty(CaseworkProperties.Picture, PropertyKind.Image, CaseworkTexts.PropertyPictureTitle);
    builder.AddProperty(CaseworkProperties.Age, PropertyKind.Integer, CaseworkTexts.PropertyAgeTitle);

    builder.AddElement(CaseworkElements.Person, CaseworkTexts.PersonTitle, [CaseworkProperties.Name, CaseworkProperties.Picture, CaseworkProperties.Age]);
    builder.AddElement(CaseworkElements.Location, CaseworkTexts.LocationTitle, [CaseworkProperties.Address, CaseworkProperties.Picture, CaseworkProperties.Country]);

    builder.AddRelation(CaseworkRelations.PersonInvolvedInCase, from: CaseworkElements.Person, to: CaseworkElements.Case, ...);

    builder.AddContext(CaseworkContexts.CaseFile, CaseworkTexts.CaseFileTitle, [CaseworkElements.Case, CaseworkElements.Person, CaseworkElements.Vehicle, CaseworkElements.Weapon, CaseworkElements.Location]);
}
```

- **Identifiers are constants** in constants classes, one per kind of definition (texts, properties, elements, relations, contexts), and that string is what is stored on the data rows (`SchemaIdentifier`). A rename in C# can never change what is stored. The contract snapshot test locks the identifiers.
- **One build step, then it is frozen.** The calls only collect. When the file has run, the builder validates everything (duplicate identifiers, a property or element or relation that is not defined, a context that lists an element that is not defined, a text parameter without a default) and either produces the immutable registry or fails the start of the application with the list of errors. The snapshot test builds the registry from the same file, so the test and the running system cannot disagree.
- **A project can register several definitions** (their own and a base one, each with its own identifier prefix). The builder merges them and validates the whole.
- **Constraints are parameters of `AddProperty`** (required, maximum length, range) and are enforced by the application on every write. There is no separate constraint language.
- **The registry is read-only at runtime** and cached. A page asks it which elements and properties a context covers, so one query can fetch exactly that data. The API serves it as the contract endpoint, which is how the Administration and any generic client learn the definitions.
- **Nothing of the texts is copied into the database:** the text service (in the Application library) gives the text a person has edited, or else the default from the files, looking in the culture asked for, then its language (`da-DK` then `da`), then the fallback culture, and in each an edit before the default. There is no startup step that writes defaults.

## What a context is and does

A context is a group of elements that belong together: these elements are shown together on a page, and so they are the data to query for it. It is one level, as it is today: the context lists its elements and nothing more. It does not describe relations or a hierarchy, and it has no depth.

```
context "case-file"   =   case, person, vehicle, weapon, location
```

What a page gets from a context:

- **Without a starting element:** the elements of the context's kinds, paged, for list and search pages. The page chooses which kind to list.
- **With a starting element** (any element of the context, for example a case): the starting element with its current properties, and the elements directly related to it whose kind is in the context, grouped by relation. Direct relations only, one step. Starting from a case gives its persons, vehicles, weapons and locations. Starting from a person gives the cases they are involved in (the same relation read from the other side, which is the inverse view) and the vehicles they own. The other people in those cases are not included: the page asks for a case to get them, which is a second request.
- **Nothing is guessed.** If two relations join the same two kinds, both are returned, each group labelled with its relation identifier and its direction.
- **A fixed, small number of queries:** one for the relation rows of the starting element towards the kinds in the context, then one for the related elements and their properties.
- **Result:** the starting element, then a group per relation (relation identifier, direction, the relation's own attributes such as a role, and the related elements with their current properties). Generic and the same for every context. The frontend renders from it.
- **"Persons in more than one case"** is not a context. It is a query over the relation table (group by the person, count the distinct cases) and belongs with the application queries.
- **As-of and history** use the same shape, reading the history tables for the dates asked for.

## Settled details

- **Nested Collection and Table property kinds** are left out of the first version.
- **Language:** several languages are supported from the start: every user-facing text goes through a text parameter, and the defaults are registered for each culture in files of their own (see Decisions). The demo only ships English: the mechanism is tested on its own, so the demo does not have to maintain more than one language.
- **Naming: the family first, then the thing.** The schema in code is `DefinitionElement`, `DefinitionProperty`, `DefinitionRelation`, `DefinitionContext` and `DefinitionTextParameter` (next to `DefinitionBuilder` and `DefinitionRegistry`); the current data is `DataElement` and `DataProperty` (`DataPropertyString` and so on); the ended data is `HistoryElement` and `HistoryProperty` (`HistoryPropertyString` and so on). The tables have the class names, so a database tool groups the current tables and the history tables. This is the same pattern as the old project (`EntitySchemaElement`, `EntityDataElement`).
- **Relations (built):** a relation is a table, with a history twin, and its attributes are ordinary property rows owned by the relation. There is one current link of a kind between two elements (a second role in the same case is another relation, or another attribute). The cardinality of the definition is kept by limits that are checked in the same transaction as the insert. Ending an element ends all of its relations in both directions. Reading a relation is the same from either end: the inverse view needs no extra data.
- **The six data property kinds** (Boolean, DateTime, Decimal, Image, Integer, String) are each linked to a property definition and exist so the data is held in the correct, strongly typed column. All six get Domain classes, matching the tables.

## Proposed solution layout, to be confirmed

- The libraries are new, in `Noxy.NET.EntityManagement/` (created library by library, in the order of the phases): `…Definitions`, `…Domain`, `…Application`, `…Persistence`, `…Api`, `…Presentation` and `…Administration`, plus a project for generating migrations. Only what is needed is copied from the old project, and checked while it is copied.
- The demo lives apart, in `Noxy.NET.EntityManagement.Demo/` (names provisional): a definitions library (the constants and the definition file), a small API host, and a frontend. The old `EntityManagement.Frontend` is not carried over.
- Inside `…Definitions`: the builder, the definition types, the registry and the validation. Inside the demo's definitions library: constants per kind (texts, properties, elements, relations, contexts, not per element, because properties and elements are shared) and the one definition file.
- `Administration/Pages/Contract/` (read-only views of the definitions) and `Administration/Pages/Data/` (browser, editor, history, context preview, text values).

## Still open

- Whether a project may add a property to a base element (the leaning is no).
- The Style and System parameters: unchanged for now.
- Whether the Administration ships as a runnable app only, or also as pages the implementer can host in their own frontend (the proposal is both: the pages are a library, and the app is a thin host of them).
- How the API library exposes its endpoints (controllers or minimal endpoints), decided in phase 4.

## Phases

The Administration is the test bed for everything, so the first pass is a thin slice: one context, one element (person) and a few properties, through all of phases 1 to 5, so the data can be created and viewed end to end. The demo's definitions and API host are needed from phase 4, because the Administration needs a running API to talk to. Then the definitions are widened and the demo frontend is built.

1. **Definitions library:** the builder, the definition types, the registry, startup validation, the snapshot check, the text parameter definitions with their default sync. Pure code with unit tests, no database. A first demo definition file (person only).
2. **Persistence:** the new foundation tables and their history twins, the regenerated migration, and the repository that moves the old row to history in the same transaction as the change, with tests on an in-memory SQLite (including that a failed change leaves current and history untouched).
3. **Application:** the commands and queries above, validated against the definitions, including the context query.
4. **API library:** the standard endpoints and the setup and mapping methods; the demo API host that calls them.
5. **Administration, rebuilt:** the contract view, the data browser and editor, history, the live context preview, and the text values.
6. **Demo implementation:** the full investigation definitions and the demo frontend, built only on the libraries. Anything that turns out to be missing from the libraries is added to them.
7. **Cleanup:** delete `C:\Projects\Noxy.NET.EntityManagement.Old` when nothing is left to copy from it.

Each phase builds and tests on its own, and the repository builds throughout.
