# MediaPlayer backend: commands, queries and notifications: design

Status: implemented (all five phases), checked against the code on 2026-11-15. Where the build differs from the first draft the text says so: commands and queries ended up public, not internal, and the use cases that share logic inherit a base class (see "How it ended up").

## Intent

Replace the use-case services (`IMediaService`, `IPlaylistService`, `IAuthenticationService`) with commands, queries and one handler per use case, sent through a mediator, and replace the hub notifier interfaces with notifications that handlers publish after a change is saved. The goals are focused single-purpose classes, controllers that only translate HTTP into a command or query, and handlers that never know about SignalR. Behaviour seen from outside (routes, request and response shapes, status codes, ETags, hub events) does not change. A frontend move to Fluxor follows later and is not part of this design.

## Goals

- **One handler per use case**, in the `Application` project: every public method of the three services becomes a command or a query with its own handler.
- **Commands and queries are separate from the request classes** (public types, so the mediator's generator in the API project can see them; they are still "internal" in the sense that no client sees them). The `Request…` classes stay the public contract that OpenAPI exposes. A command or query is its own class holding exactly what the handler needs, including values a client must never send, such as the `UserID` from the token. The translation from request to command is a `ToCommand` or `ToQuery` method on the request class, so it lives in one place per request. (The first draft put it at the API boundary; the requests then sat in `Domain`, which cannot see commands, so the requests moved to `Application`, which is where the use-case inputs belong.)
- **Thin controllers**: read the user from the token, build the command or query, send it, shape the HTTP result (ETag, 304, status codes, file results).
- **Notifications instead of notifiers (option B).** After a successful commit a handler publishes a notification (`NotificationMediaUpdated`, `NotificationMediaDeleted`, `NotificationPlaylistUpdated`, `NotificationPlaylistDeleted`, `NotificationUploadSucceeded`, `NotificationUploadFailed`). Handlers for them in the API project push the matching hub event. A notification with no handler does nothing, which replaces the `NoOp…Notifier` classes. The "a failed push never fails the request and is only logged" rule lives in one place (the base of the hub handlers) instead of being repeated in every service.
- **Behaviour preserved.** The integration tests (they go through HTTP and the real hub) and the unit tests, moved to handlers, are the safety net; no endpoint, status code, header or hub payload changes.
- **Infrastructure services stay services**: ETag, token, password hash, Google token, file storage, tag writer, validator, upload gate and signal are not use cases and keep their interfaces.

## Non-goals

- Any change to the public API, the database or the hub events.
- Fluxor and the frontend (a later design).
- Pipeline behaviours for things the code does not do today (no new logging, caching or transaction behaviour). They can be added later; the mediator makes that possible.
- Turning the background services (`UploadWorker`, `CleanupService`, `UploadProcessor`) into handlers. `UploadProcessor` stays a processor but publishes notifications instead of calling a notifier.
- Event sourcing, separate read and write databases, or splitting the project.

## Anti-goals

- A behaviour change hiding in the move: a different status code, a lost ETag, a notification sent before the commit, a missing notification, a changed exception mapping.
- A handler that talks to SignalR, or a notification handler that can fail a request.
- Commands that expose or copy the public request classes wholesale, so that a change to the public contract silently changes a handler.
- A half-migrated state at the end of a phase: every phase ends with the solution building, all tests green, and no use case served by both the old service and a handler.

## Constraints

- Repository conventions: one class per file, no summary comment above a class, `List` suffix for lists, no `yield`, no magic strings, interfaces in an `Interfaces` folder, `is null`, no `!` (handle the null, or no comment for plain EF accessor paths), method-syntax LINQ, media and never "song", DI parameters named kind first (`servicePlaylist`, `notifierMedia`, `configStorage`).
- Names follow EntityManagement: the kind first, then the area and the action, for example `CommandMediaUpdate`, `QueryMediaLibrary`, `HandlerMediaUpdate`, `NotificationMediaUpdated`. Folders per kind and area (`Commands/Media`, `Queries/Media`, `Handlers/Media`, `Notifications/Media`), as there.
- The `Mediator` package (martinothamar, source generated, MIT) as in EntityManagement, not MediatR (commercial licence). Its source generator runs in the project that holds the handlers.
- Ownership rules stay in the data access: handlers pass the `UserID` and the repository returns nothing for another user's data, so another user's resource is still a 404.

## Approaches considered

**How notifications are sent.** (A) handlers call notifier interfaces after the commit, as now; (B) handlers publish mediator notifications and separate handlers push over SignalR. Chosen: **B**, the user's call. Handlers stay about the data change, the "log and carry on" rule exists once, and anything else that wants to react to a change (later sync features, logging) can listen without touching the handler. The cost is a second small set of classes.

**What a command carries.** (A) the public request class wrapped with the user ID; (B) a separate class, mapped from the request by a `ToCommand` or `ToQuery` method on the request class (the requests live in `Application` for that reason; see "How it ended up"). Chosen: **B**, the user's call (the request is the OpenAPI contract, the command is internal communication and carries values like `UserID` that a client must not send). The mapping is kept in one place so it is not repeated in each controller.

**Where handlers live.** (A) the API project, as in EntityManagement; (B) the `Application` project. Chosen: **B**, my call: `Application` is where the use-case rules already live, it keeps the API project to HTTP and SignalR, and handlers can be tested without the web stack.

**How to migrate.** (A) all at once; (B) in phases, one area at a time, each ending green. Chosen: **B**, my call, in the order below.

## Design

### Inventory (what becomes what)

- **Media** (`IMediaService`): upload (stage and enqueue), update (tags), delete, library (with ETag), list (paged), file (stream for sync), file with tags (temporary tagged copy). Commands: upload, update, delete. Queries: library, list, file, file with tags.
- **Playlist** (`IPlaylistService`): library (list with ETag), one playlist (with entries, ETag), create, update, delete, add entries, remove entries, move an entry. Commands: create, update, delete, add entries, remove entries, move entry. Queries: library, one playlist.
- **Authentication** (`IAuthenticationService`): sign up, sign in, Google sign in, refresh. All commands (they change state or issue tokens).
- **Notifications** replacing `IMediaNotifier`, `IPlaylistNotifier`, `IUploadNotifier`: media updated, media deleted, playlist updated, playlist deleted, upload succeeded, upload failed. The hub event names (`MediaHubEventEnum`) and payloads stay exactly as they are.

### Shape of a use case

- A command or query is a small immutable class or record holding plain values (`UserID`, the IDs, the fields to change, a stream for an upload). It returns what the service method returned today (the response classes and the existing models such as `MediaLibrary`, `PlaylistView`, `MediaDownload`).
- A handler takes its dependencies by constructor (`factoryUnitOfWork`, infrastructure services, a publisher for notifications) and does what the matching service method did: the same checks, the same single commit, the same order, then publishes its notifications after the commit.
- The controller keeps everything HTTP: `If-None-Match`, `ETag`, 304, `Cache-Control`, file results, `Created` for create. The mapping from a request class to its command or query is a method on the request class, and the controllers send through the mediator.
- Exceptions are unchanged: handlers throw the existing domain exceptions and `ApiExceptionHandler` maps them.

### Phases (each ends with the solution building, all tests green, and no use case served twice)

1. **Infrastructure and notifications.** Add the mediator to `Application` and register it; add the notification classes and the SignalR notification handlers (with the log-and-carry-on base) in the API project; switch the services and `UploadProcessor` from notifier interfaces to publishing notifications, and delete the notifier interfaces and the `NoOp` classes. The services still exist at this point.
2. **Media.** One command or query plus handler per media use case, controller moved over, `IMediaService`/`MediaService` and their tests removed, handler tests added in their place.
3. **Playlist.** The same for playlists (including the media-delete cascade, which touches playlists: it stays inside the delete-media handler, with the same single commit).
4. **Authentication.** The same for sign up, sign in, Google and refresh.
5. **Clean-up.** Remove the empty service interfaces and registrations, update the docs and conventions notes.

Phase 2 (Media) is done: `IMediaService` and `MediaService` no longer exist.

Phase 3 (Playlist) is done: `IPlaylistService` and `PlaylistService` no longer exist.

Phase 4 (Authentication) is done: `IAuthenticationService` and `AuthenticationService` no longer exist.

Phase 5 (Clean-up) is done: no service, notifier or `NoOp` type for a use case remains. The layout is commands, queries and handlers per area in `Application` (`Commands`, `Queries`, `Handlers`, `Notifications`), and the request classes in `Application/Requests`, each with the method that builds its command or query. `API/Handlers` holds the SignalR notification handlers in one folder per area (`Media`, `Metadata`, `Playlist`, `Remote`, `Setting`; the exception handler sits in the API project root). The services that remain (`ETagService`, `GoogleTokenService`, `PasswordHashService`, `TokenService`, `MediaTagWritingService`, the file storage service) are infrastructure, not use cases.

### How it ended up

- **Shared base classes.** Playlist changes that load a playlist with its entries and save it (update, add entries, remove entries, move an entry) inherit `BaseHandlerPlaylistChange`, which holds the save once (bump the modified time, commit, publish `NotificationPlaylistUpdated`; each handler builds its own response). The load (entries in position order, or not found) is the `LoadPlaylistWithEntries` extension on `IUnitOfWork` in `Application/Extensions`, shared with the playlist query. The four authentication handlers inherit `BaseHandlerAuthentication`, which holds the token response and the email normalisation once. Handlers that need neither (list, get, create, delete, the media handlers) stand alone.
- **Secrets stay out of logs.** Authentication commands carry passwords, an ID token or a refresh token, so each hides its members from `ToString()`.
- **Folders.** `Application/Commands`, `Queries`, `Handlers` and `Notifications` have one folder per area. In the API project, `Handlers` holds the SignalR notification handlers (the exception handler sits in the API root); the request classes (and their `ToCommand`/`ToQuery` methods) are in `Application/Requests`, one folder per area, and nothing else is: the `Optional` type is in `Application/Models`, the validation attributes in `Application/Attributes` and the JSON converters in `Application/Converters`.
- **Models by area.** `Application/Models` holds `Optional` at its root and one subfolder per area: `Authentication` (the token and Google identity models), `Media`, `Playlist` and `Upload` (the upload processor, signal, gate, gate lease and prefixed stream, plus the validated and stored file results). Their interfaces mirror it in `Interfaces/Models/Upload`.
- **Entity names.** The domain entities carry an `Entity` prefix, like the `Table` classes in Persistence (`EntityPlaylist`, `EntityMediaFile`, `EntityUser`, and so on), so they never clash with a namespace or model of the same name such as `Handlers.Playlist` or `Models.Authentication.RefreshToken`. Their extension classes follow (`EntityPlaylistExtension`).
- **Tests.** One unit test class per handler and one mapper test class per request class, plus the notification handler tests and an end-to-end hub test.

### Tests

- The integration tests are the behaviour contract and must pass unchanged in every phase (route constants and helper names in them do not change).
- The existing unit tests of each service move to handler tests in the same phase: one test class per handler, using the same fakes (`FakeUnitOfWorkFactory`, `FakeStore` and the repository fakes); the fake notifiers become a fake publisher that records published notifications. Each moved test keeps its assertions, so a lost behaviour shows up as a failing test.
- New tests: each SignalR notification handler pushes the right hub event with the right payload and survives a failing hub; a command-mapping test per request class (the `ToCommand`/`ToQuery` method on the request).

## Calls made after the builder check (mine, to be checked)

- **Where the generator runs:** in the API project (the composition root), so one generated mediator sees the handlers in `Application` (public classes, public commands and queries) and the notification handlers in the API project. `Application` references only the mediator abstractions, and `AddMediator` is called from the API startup. Phase 1 ends with an end-to-end test that publishes a notification and receives the hub event, which proves the notification handlers are registered (otherwise they would silently do nothing).
- **Lifetime:** the mediator and its handlers are singletons. Every handler's dependencies (unit of work factory, file storage, validator, tag writer, ETag service, upload gate and signal, configuration, time) are already singletons, and the per-request values (user ID and so on) travel in the command, so no handler holds request state. This lets the singleton `UploadProcessor` and the background services publish notifications directly. If a handler ever needs a scoped dependency, that handler's lifetime is revisited then.
- **Notification payloads and order:** a notification carries the same payload the notifier received (`ResponseMediaFile`, `ResponsePlaylist` without entries, `ResponseUploadResult`, the IDs for deletes). Handlers publish in the same order as the services call the notifiers today (for a media delete: the media-deleted notification, then one playlist-updated notification per affected playlist), always after the commit and after the stored file is removed. "Log and carry on" copies today's behaviour: the hub handler base catches every exception from the push and logs a warning.
- **Streams:** a command holding a stream (upload) does not own it: the caller, the controller reading the request body, owns and disposes it, as now. The file queries return a stream the caller owns, which the file result disposes, as now. `ResponseUploadAccepted` and `MediaDownload` stay the return types.
- **Tests of the notifier classes:** `NoOpMediaNotifierTest` and `NoOpPlaylistNotifierTest` are deleted with their classes in phase 1 (a notification with no handler doing nothing is the mediator's behaviour, and the hub event is covered end to end). The three fake notifiers become one fake publisher in phase 1, at the same time `MediaService`, `PlaylistService` and `UploadProcessor` switch to publishing, so `UploadProcessorTest` and `UploadRecoveryTest` move in phase 1 too.
- **Mapping tests** cover every command or query that is built from a request class (including queries with paging or search values). A query made only of route IDs and the user ID, built straight in the controller, needs no mapping test.

## Left to the builder

The exact command and query members, the names of the mapping methods, the notification handler base class, how the fake publisher is shaped, the order of the use cases inside a phase, and the Mediator publisher strategy for notifications (sequential is fine).

## Metadata use cases (added with `2026-11-15-metadata-entities-design.md`)

The artist, genre and album use cases follow the same shape. Artist and genre share generic command records and generic abstract handlers (`BaseHandlerNamedItem…`, over `INamedItemService<TItem, TResponse>`) with thin concrete classes per kind, as `BaseHandlerPlaylistChange` does for playlists. All change handlers derive from `BaseHandlerMetadataChange`, which owns "touch the files whose response changed in the same commit" and "publish after the commit". The album side of an artist change lives in `MetadataChangeService`.

## Settings use cases (added with `2026-11-15-user-settings-design.md`)

The settings area has one query (`QuerySettingList`), one command (`CommandSettingUpdate`), their two handlers and one notification. Declarations in `Domain` own the validation, parsing and entity creation of each setting, so no service is injected; code reads a setting through the typed `IUnitOfWork` extensions `GetBoolean`, `GetInteger` and `GetEnum`.
