# Frontend Foundation: Session, Stores and Metadata Sync Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:executing-plans (the user wants this done natively, in the main session, not through subagents). Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Give the Blazor WebAssembly app everything below the screens that metadata needs: sign in and silent refresh, typed API calls with ETags, a replaceable metadata store (IndexedDB in the browser), and a sync service that keeps the media file and playlist lists current from the start-up diff and the hub, with a search function over the local library.

**Architecture:** All code lives in `Noxy.NET.MediaPlayer.Presentation` (a Razor class library) behind interfaces, so it is tested in the unit test project with fakes and end to end in the integration test project against the real API (`ApiFactory`). `Frontend` only wires it up. Services own the work; Fluxor state, screens and their look come in the screens plan, after the user has designed them.

**Tech Stack:** .NET 10, Blazor WebAssembly, `Microsoft.AspNetCore.SignalR.Client` 10.0.15, `Blazored.LocalStorage` 4.5.0 (already referenced), IndexedDB through a JS module, xunit v3.

**Spec:** `docs/superpowers/specs/2026-11-15-frontend-sync-player-design.md` (sign in, metadata sync, search, replaceable store, the backend contracts). The two follow-up plans are "downloads and the player" (file store, download queue, hash check, player, queue) and "screens" (Fluxor state and every page); neither starts before this one is done.

## Global Constraints

- Work in `Noxy.NET.MediaPlayer.Presentation` and `Noxy.NET.MediaPlayer.Frontend`, plus the two test projects. Target `net10.0`. The backend is not changed.
- One class per file, the file named after it. No summary comment above a class. Interfaces in an `Interfaces` folder, in a subfolder matching the implementation's folder (`Interfaces/Services` for `Services`, `Interfaces/Stores` for `Stores`); namespaces mirror folders. Extensions in an `Extensions` folder. Lists end in `List`. No `yield`. No magic strings (constants, `nameof`, or `nameof(MediaHubEvent.X)` for hub events). `is null`. No `!` (handle the null with a descriptive throw). Method-syntax LINQ. Media, never "song". Injected dependencies named kind first (`serviceSession`, `storeMetadata`, `clientApi`, `storageSession`); `time` keeps its name.
- Requests are in `Noxy.NET.MediaPlayer.Application.Requests`, responses in `Noxy.NET.MediaPlayer.Domain.Responses`; the app sends and reads exactly those classes. Hub event names come from `MediaHubEvent`.
- The access token lives in memory only; the refresh token is the only credential written to browser storage. Neither token, nor a password, appears in a log or an exception message.
- A stored ETag is only ever one that arrived with a full `200` response the device downloaded. A push never writes an ETag.
- Tests: xunit v3, unit tests in `Noxy.NET.Test.UnitTest/MediaPlayer/Presentation/`, end-to-end in `Noxy.NET.Test.IntegrationTest/MediaPlayer/Presentation/`. Build the test project, then run its exe, for example `Noxy.NET.Test/Noxy.NET.Test.UnitTest/bin/Debug/net10.0/Noxy.NET.Test.UnitTest.exe -class "*SyncServiceTest"`; `dotnet test` finds nothing. Pass `TestContext.Current.CancellationToken` to every call that takes a token. Zero warnings. A test folder contains only tests: fakes go in `Fakes`, helpers in `Fixtures` (extensions in `Fixtures/Extensions`). `ValueTask` or `Task` results are wrapped with `.AsTask()` where needed inside `Assert.ThrowsAsync`.
- Do not commit. The user reviews the diff in the IDE; `git add -N` files you create, by name. Do not run machine-level commands (installing workloads or tools): give the user the command instead. Write files with the editor tool when they contain quotes, apostrophes, backslashes or `$`. If a file seems to have lost an edit, re-read it before editing.

## Review Focus

- **Two requests refresh at once.** Refresh tokens are single use and reuse revokes the whole account, so two 401s at the same moment must produce one refresh call, not two (Task 2 and Task 3 each pin it).
- **Cleared browser storage.** A stored ETag with an empty local list (the browser dropped IndexedDB) must not turn the next `304` into an empty library: sync sends no `If-None-Match` when the local list is empty (Task 6).
- **Out-of-order or repeated pushes.** An older `timeModified` never overwrites a newer local copy, a delete for an unknown ID is harmless, and a push for a playlist whose entries are not held does not fetch them (Task 7).
- **Offline at start-up.** A failed network call leaves the local lists and ETags untouched and reports offline; it never clears the library or throws out of start-up (Tasks 3, 6, 9).
- **Expired or revoked session.** A refresh that fails with 401 signs the user out cleanly (tokens cleared, local metadata kept) instead of looping on 401 (Tasks 2 and 3).

## File Structure

New files (relative to `Noxy.NET.MediaPlayer.Presentation/`; tests relative to the test projects' `MediaPlayer/Presentation/`):
- `Interfaces/Services/`: `IAuthApiClient.cs`, `IApiClient.cs`, `ISessionService.cs`, `ISyncService.cs`, `IMediaHubService.cs`
- `Interfaces/Stores/`: `IMetadataStore.cs`, `ISessionStorage.cs`
- `Services/`: `AuthApiClient.cs`, `ApiClient.cs`, `SessionService.cs`, `SyncService.cs`, `MediaHubService.cs`; the existing `APIHttpClient.cs` stays the HTTP wrapper
- `Stores/`: `LocalSessionStorage.cs` (Blazored.LocalStorage), `IndexedDbMetadataStore.cs`; `wwwroot/metadataStore.js`
- `Models/`: `ApiResponse.cs`; `Exceptions/ApiException.cs`; `Helpers/AccessTokenReader.cs`, `Helpers/LibrarySearch.cs`; `Constants.cs`
- Tests: unit `Fakes/FakeHttpMessageHandler.cs`, `Fakes/FakeSessionStorage.cs`, `Fakes/FakeMetadataStore.cs`, `Fakes/FakeApiClient.cs`, `Fakes/FakeAuthApiClient.cs`, `Fakes/FakeSessionService.cs`; test classes named after the class they test; integration `FrontendSyncFlowTest.cs`

Modified: `Presentation.csproj` (SignalR client package), `ServiceCollectionExtensions.cs` (`AddPresentation` registers everything), `Frontend/Program.cs` and a start-up orchestrator, `Test.UnitTest.csproj` and `Test.IntegrationTest.csproj` (reference `Presentation`).

Contracts every task shares:
- `record ApiResponse<T>(HttpStatusCode Status, T? Value, string? ETag)` with `bool IsNotModified => Status == HttpStatusCode.NotModified`. `class ApiException(HttpStatusCode status, string title) : Exception(title)` with `Status` and `Title`.
- `IMetadataStore` (all members `Task`-based): `GetMediaFileList() → List<ResponseMediaFile>`, `ReplaceMediaFileList(List<ResponseMediaFile>)`, `SaveMediaFile(ResponseMediaFile)`, `DeleteMediaFile(Guid id)`, `GetPlaylistList() → List<ResponsePlaylist>`, `ReplacePlaylistList(List<ResponsePlaylist>)`, `SavePlaylist(ResponsePlaylist)`, `DeletePlaylist(Guid id)` (also removes that playlist's detail), `GetPlaylistDetail(Guid id) → ResponsePlaylistDetail?`, `SavePlaylistDetail(ResponsePlaylistDetail)`, `GetLibraryETag() → string?`, `SetLibraryETag(string?)`, `GetPlaylistListETag() → string?`, `SetPlaylistListETag(string?)`, `GetHeldPlaylistIDList() → List<Guid>` (playlists whose detail is stored). A `Replace…List` is one atomic write.
- `ISessionStorage`: `Task<string?> GetRefreshToken()`, `Task SetRefreshToken(string token)`, `Task ClearRefreshToken()`.
- `IAuthApiClient`: `Task<ResponseAuthentication> SignUp(string email, string password)`, `SignIn(...)`, `Task<ResponseAuthentication> Refresh(string refreshToken)`; no bearer header; non-success throws `ApiException`.
- `ISessionService`: `bool IsSignedIn`, `event Action? Changed`, `Task SignUp(email, password)`, `Task SignIn(email, password)`, `Task<bool> TryRestore()` (silent sign-in from the stored refresh token), `Task SignOut()`, `Task<string?> GetAccessToken()` (null when signed out; refreshes first when the token expires within `Constants.RefreshMarginSeconds` = 60), `Task<bool> Refresh()` (one refresh, shared by concurrent callers; false when it failed and the user was signed out).
- `IApiClient`: `Task<ApiResponse<List<ResponseMediaFile>>> GetLibrary(string? eTag)`, `Task<ApiResponse<List<ResponsePlaylist>>> GetPlaylistList(string? eTag)`, `Task<ApiResponse<ResponsePlaylistDetail>> GetPlaylistDetail(Guid id, string? eTag)`, and the playlist writes `Task<ResponsePlaylist> CreatePlaylist(RequestCreatePlaylist)`, `Task<ResponsePlaylist> UpdatePlaylist(Guid, RequestUpdatePlaylist)`, `Task DeletePlaylist(Guid)`, and `Task<ResponsePlaylistDetail> AddPlaylistEntryList(Guid, RequestAddPlaylistEntry)`, `RemovePlaylistEntryList(Guid, RequestRemovePlaylistEntry)`, `MovePlaylistEntry(Guid id, Guid entryID, RequestMovePlaylistEntry)`.
- `ISyncService`: `event Action? Changed`, `bool IsOffline`, `Task SyncLibrary(bool force = false)`, `Task SyncPlaylistList(bool force = false)`, `Task SyncPlaylistDetail(Guid id)` (always a full download), and the push handlers `Task ApplyMediaUpdated(ResponseMediaFile)`, `ApplyMediaDeleted(Guid)`, `ApplyPlaylistUpdated(ResponsePlaylist)`, `ApplyPlaylistDeleted(Guid)`, `ApplyUploadSucceeded(ResponseUploadResult)`.

---

### Task 1: Wiring, and the bundle-size check

**Files:**
- Modify: `Presentation.csproj` (add `Microsoft.AspNetCore.SignalR.Client` 10.0.15), `Test.UnitTest.csproj` and `Test.IntegrationTest.csproj` (project reference to `Presentation`), design doc (record the result)
- Create: `Constants.cs` (`RefreshTokenKey`, `RefreshMarginSeconds = 60`), `Models/ApiResponse.cs`, `Exceptions/ApiException.cs`

**Interfaces:**
- Produces: the contracts under "Contracts every task shares" for `ApiResponse<T>` and `ApiException`.

- [ ] **Step 1:** Run `dotnet publish Noxy.NET.MediaPlayer/Noxy.NET.MediaPlayer.Frontend -c Release -o <a new empty temp directory>` and list the files of `_framework` larger than 500 KB with their sizes. Note whether `TagLibSharp`, `Google.Apis.Auth` or `Microsoft.EntityFrameworkCore` assemblies are in it.
- [ ] **Step 2:** Add the references and the package, create the three small types, and build the whole solution (0 warnings).
- [ ] **Step 3:** Publish again and compare. Record both lists, and the decision, in a short "Bundle size, checked 2026-11-15" note under "Calls made while writing this" in the design doc. If server-only assemblies are in the bundle, stop and tell the user before Task 2: the request classes would then need a lighter shared project, and that changes where everything below is built.
- [ ] **Leave uncommitted.**

---

### Task 2: Session service

**Files:**
- Create: `Interfaces/Services/IAuthApiClient.cs`, `Interfaces/Services/ISessionService.cs`, `Interfaces/Stores/ISessionStorage.cs`, `Services/AuthApiClient.cs`, `Services/SessionService.cs`, `Stores/LocalSessionStorage.cs`, `Helpers/AccessTokenReader.cs`; tests `Fakes/FakeAuthApiClient.cs`, `Fakes/FakeSessionStorage.cs`, `Fakes/FakeHttpMessageHandler.cs`, `AuthApiClientTest.cs`, `SessionServiceTest.cs`, `AccessTokenReaderTest.cs`

**Interfaces:**
- Consumes: the contracts above.
- Produces: `static class AccessTokenReader` with `DateTime? GetExpires(string accessToken)` (the `exp` claim of the unvalidated payload, UTC; null when it cannot be read); `SessionService(IAuthApiClient clientAuthApi, ISessionStorage storageSession, TimeProvider time)`; `AuthApiClient(HttpClient)` posting `RequestSignUp`, `RequestSignIn`, `RequestRefresh` to `/Auth/SignUp`, `/Auth/SignIn`, `/Auth/Refresh`; `LocalSessionStorage(ILocalStorageService)`.

- [ ] **Step 1: Write the failing tests.** `AccessTokenReaderTest`: a token built by hand with a known `exp` returns it; garbage, a token with two segments, and a payload without `exp` return null. `AuthApiClientTest` (with `FakeHttpMessageHandler`): each call sends the right method, path and JSON body, no `Authorization` header, and returns the parsed tokens; a 401 problem details response throws `ApiException` with status 401 and its title, and a 409 the same. `SessionServiceTest`: `SignIn_StoresTheRefreshTokenAndKeepsTheAccessTokenInMemoryOnly` (the fake storage holds the refresh token, nothing else); `TryRestore_WithAStoredRefreshToken_SignsIn_AndRotatesTheStoredToken`; `TryRestore_WithNoStoredToken_ReturnsFalse`; `TryRestore_WhenTheRefreshIsRejected_ClearsTheStoredToken_AndReturnsFalse`; `TryRestore_WhenTheNetworkIsDown_KeepsTheStoredToken_AndStaysSignedOut` (an `HttpRequestException` is not a rejection); `GetAccessToken_WhenItExpiresWithinTheMargin_RefreshesFirst`; `GetAccessToken_WhenSignedOut_ReturnsNull`; `ConcurrentRefresh_CallsTheServerOnce` (ten parallel `Refresh()` calls, the fake counts one request); `Refresh_WhenRejected_SignsOut_AndRaisesChanged`; `SignOut_ClearsEverything`; `Changed_IsRaisedOnSignInAndSignOut`.
- [ ] **Step 2:** Run the classes; expected: compile failure.
- [ ] **Step 3:** Implement. `SessionService` serialises refreshes with a `SemaphoreSlim`; a caller that gets the lock after another refresh succeeded returns its result without calling the server. A rejection means an `ApiException` with status 401 (or 400); any other exception propagates and leaves the stored token alone. Expiry comes from `AccessTokenReader` and `time`.
- [ ] **Step 4:** Run the unit project; expected: all pass.
- [ ] **Leave uncommitted.**

---

### Task 3: Authorised API client

**Files:**
- Create: `Interfaces/Services/IApiClient.cs`, `Services/ApiClient.cs`; tests `Fakes/FakeSessionService.cs`, `ApiClientTest.cs`
- Modify: `Services/APIHttpClient.cs` only if it needs to expose the `HttpClient` (it may be replaced by `ApiClient` taking `HttpClient` directly; if so delete it and its registration and say so in the report)

**Interfaces:**
- Consumes: `ISessionService.GetAccessToken()` and `Refresh()`.
- Produces: `ApiClient(HttpClient, ISessionService serviceSession)` implementing `IApiClient` as listed above.

- [ ] **Step 1: Write the failing tests** (fake handler plus `FakeSessionService`): `GetLibrary_SendsTheBearerTokenAndTheETagAsIfNoneMatch`; `GetLibrary_On200_ReturnsTheListAndTheETagHeader`; `GetLibrary_On304_ReturnsNotModifiedWithNoValue`; `GetLibrary_WithNoETag_SendsNoIfNoneMatch`; `AUnauthorizedResponse_RefreshesOnceAndRetriesOnce` (second request carries the new token); `AUnauthorizedResponse_AfterTheRefreshFailed_ThrowsApiException401_AndDoesNotRetryAgain`; `ConcurrentUnauthorizedResponses_RefreshOnce` (five parallel calls, the fake session counts one `Refresh()` that actually ran, as `SessionService` serialises it); `ANetworkFailure_PropagatesAsHttpRequestException`; one test per write method for method, path (`/Playlist`, `/Playlist/{id}`, `/Playlist/{id}/EntryList`, `/Playlist/{id}/EntryList/Remove`, `/Playlist/{id}/Entry/{entryID}/Position`) and JSON body, and that a 404 or 409 problem response throws `ApiException` with its status and title.
- [ ] **Step 2:** Run `*ApiClientTest`; expected: compile failure.
- [ ] **Step 3:** Implement with one private send method that builds the request, adds the bearer token, and on a 401 calls `serviceSession.Refresh()` once and sends a fresh copy of the request once more. Responses with a non-success status other than 304 throw `ApiException` from the problem details title. Bodies are read with `System.Net.Http.Json`; the ETag is `response.Headers.ETag?.Tag` (the quoted tag as sent).
- [ ] **Step 4:** Run the unit project; expected: all pass.
- [ ] **Leave uncommitted.**

---

### Task 4: Metadata store contract, with the fake

**Files:**
- Create: `Interfaces/Stores/IMetadataStore.cs`; tests `Fakes/FakeMetadataStore.cs`, `Fixtures/MetadataStoreContract.cs` (an abstract class of tests, run by each store implementation), `FakeMetadataStoreTest.cs`

**Interfaces:**
- Produces: `IMetadataStore` as listed above. `MetadataStoreContract` has `protected abstract IMetadataStore CreateStore()` and the tests below as `[Fact]` methods, so the IndexedDB store in Task 5 can inherit them in a browser run.

- [ ] **Step 1: Write the contract tests:** an empty store returns empty lists, no ETags and no detail; `ReplaceMediaFileList` replaces rather than merges; `SaveMediaFile` inserts or replaces by ID; `DeleteMediaFile` of an unknown ID is harmless; the same for playlists; `DeletePlaylist` also removes the held detail and its ID from `GetHeldPlaylistIDList`; `SavePlaylistDetail` makes the ID held; ETags round-trip and `SetLibraryETag(null)` clears; `ReplaceMediaFileList` with 5000 items returns all 5000 in the given order.
- [ ] **Step 2:** Run `*FakeMetadataStoreTest`; expected: compile failure. Implement `FakeMetadataStore` (in-memory) and make it pass.
- [ ] **Leave uncommitted.**

---

### Task 5: IndexedDB metadata store

**Files:**
- Create: `Stores/IndexedDbMetadataStore.cs`, `wwwroot/metadataStore.js`; a manual check note in the design doc under "Tests"
- Modify: `ServiceCollectionExtensions.cs` (register the store)

**Interfaces:**
- Consumes: `IMetadataStore`. Produces: `IndexedDbMetadataStore(IJSRuntime)` implementing it through a JS module imported from `_content/Noxy.NET.MediaPlayer.Presentation/metadataStore.js`.

- [ ] **Step 1:** Implement the JS module: one database with object stores for media files, playlists, playlist details and a key-value store for the two ETags; every `Replace…List` is a single transaction (clear and put all); values are stored as the JSON the API sends, keyed by `id`. Calls return plain JSON that the C# class deserialises with `JsonSerializerOptions.Web`.
- [ ] **Step 2:** Implement the C# class as a thin wrapper, one JS call per member; the module is imported lazily once.
- [ ] **Step 3:** Verification is in the browser, not by an exe: run the Frontend against a running API (`dotnet run` for both), add a temporary diagnostic page or console call that runs the `MetadataStoreContract` scenarios against the store, and record the result (pass or the failing scenario) in the report. Remove the temporary code afterwards.
- [ ] **Leave uncommitted.**

---

### Task 6: Sync service, the diffs

**Files:**
- Create: `Interfaces/Services/ISyncService.cs`, `Services/SyncService.cs`; tests `Fakes/FakeApiClient.cs`, `SyncServiceTest.cs`

**Interfaces:**
- Consumes: `IApiClient`, `IMetadataStore`. Produces: `SyncService(IApiClient clientApi, IMetadataStore storeMetadata)` with `SyncLibrary`, `SyncPlaylistList`, `SyncPlaylistDetail`, `IsOffline` and `Changed` (the push handlers come in Task 7).

- [ ] **Step 1: Write the failing tests** with `FakeApiClient` (scripted responses, records the ETags it was sent) and `FakeMetadataStore`: `SyncLibrary_FirstRun_SendsNoETag_StoresTheListAndTheETag`; `SyncLibrary_On304_ChangesNothing`; `SyncLibrary_On200_ReplacesTheLocalListAndStoresTheNewETag` (a file gone from the server is gone locally); `SyncLibrary_WhenTheLocalListIsEmptyButAnETagIsStored_SendsNoETag` (cleared storage) and then stores the full list; `SyncLibrary_Force_SendsNoETag_EvenWhenOneIsStored`; `SyncLibrary_WhenTheNetworkIsDown_LeavesListAndETagAlone_AndSetsOffline`; `SyncLibrary_AfterAFailure_ThenSuccess_ClearsOffline`; `SyncPlaylistList` the same five behaviours; `SyncPlaylistList_ReFetchesAHeldDetailWhoseSummaryIsNewer` and leaves an up-to-date one alone; `SyncPlaylistList_DropsTheDetailOfAPlaylistThatNoLongerExists`; `SyncPlaylistDetail_AlwaysDownloadsInFull_AndStoresIt`; `SyncPlaylistDetail_WhenTheServerSays404_DeletesTheLocalPlaylist`; `Changed_IsRaisedAfterEachStoreWrite`.
- [ ] **Step 2:** Run `*SyncServiceTest`; expected: compile failure.
- [ ] **Step 3:** Implement. Only an `HttpRequestException` or a `TaskCanceledException` that is not a user cancellation means offline; an `ApiException` other than the 404 above propagates. The ETag is stored only after the list it came with is stored.
- [ ] **Step 4:** Run the unit project; expected: all pass.
- [ ] **Leave uncommitted.**

---

### Task 7: Push handling and the hub

**Files:**
- Create: `Interfaces/Services/IMediaHubService.cs`, `Services/MediaHubService.cs`; tests added to `SyncServiceTest.cs` and a new `MediaHubServiceTest.cs`
- Modify: `Services/SyncService.cs`

**Interfaces:**
- Consumes: `ISyncService`, `ISessionService.GetAccessToken()`. Produces: the five `Apply…` members on `SyncService`; `MediaHubService(ISyncService serviceSync, ISessionService serviceSession, string hubUrl)` with `Task Start()` and `Task Stop()`.

- [ ] **Step 1: Write the failing tests** (sync service): `ApplyMediaUpdated_SavesIt_AndKeepsTheStoredETag`; `ApplyMediaDeleted_RemovesIt_AndAnUnknownIDIsHarmless`; `ApplyUploadSucceeded_WithAMedia_SavesIt`; `ApplyUploadSucceeded_WithNoMedia_ChangesNothing`; `ApplyPlaylistUpdated_SavesTheSummary`; `ApplyPlaylistUpdated_WithANewerTimeThanAHeldDetail_RefetchesThatDetail`; `ApplyPlaylistUpdated_WithAnOlderTimeThanTheLocalSummary_IsIgnored`; `ApplyPlaylistUpdated_ForAPlaylistWhoseDetailIsNotHeld_FetchesNothing`; `ApplyPlaylistDeleted_RemovesPlaylistAndDetail`; a push never calls `SetLibraryETag` or `SetPlaylistListETag`.
- [ ] **Step 2:** Hub tests are in the integration project (Task 9) because they need the real hub; `MediaHubServiceTest` covers only what is unit-testable: the handler names registered are exactly `nameof(MediaHubEvent.X)` for the six events (expose the list as a `static IReadOnlyList<string> HandledEventNameList` on the class and assert it equals the six enum names).
- [ ] **Step 3:** Implement. `MediaHubService` builds the connection with `WithUrl(url, options => options.AccessTokenProvider = …)`, `WithAutomaticReconnect()`, forwards each event to the matching `Apply…`, and on `Reconnected` runs `SyncLibrary()` and `SyncPlaylistList()` (pushes sent while offline are lost). It never lets an exception from a handler escape into the connection.
- [ ] **Step 4:** Run the unit project; expected: all pass.
- [ ] **Leave uncommitted.**

---

### Task 8: Search and sort

**Files:**
- Create: `Helpers/LibrarySearch.cs`; test `LibrarySearchTest.cs`

**Interfaces:**
- Produces: `static class LibrarySearch` with `List<ResponseMediaFile> Filter(List<ResponseMediaFile> list, string? query)` and `List<ResponseMediaFile> Sort(List<ResponseMediaFile> list, LibrarySort sort, bool descending)`, and `enum LibrarySort { Title, Artist, Album, Year, TimeCreated }` in `Models/`.

- [ ] **Step 1: Write the failing tests:** a blank or null query returns everything in the given order; a query matches title, artist, album, album artist and genre, case-insensitively and by substring; several words must all match (in any of the fields); a null field never throws; accents are matched as typed (no folding); sort is stable (equal keys keep the input order) and null keys sort last in both directions.
- [ ] **Step 2:** Run `*LibrarySearchTest`; expected: compile failure. Implement; run; expected: pass.
- [ ] **Leave uncommitted.**

---

### Task 9: Wiring, start-up and the end-to-end test

**Files:**
- Create: `Services/AppStartupService.cs` and its interface `IAppStartupService` (`Task Start()`), integration test `FrontendSyncFlowTest.cs` and fixtures it needs under `Fixtures/`
- Modify: `ServiceCollectionExtensions.cs` (`AddPresentation` registers `AuthApiClient`, `ApiClient`, `SessionService`, `SyncService`, `MediaHubService`, `LocalSessionStorage`, `IndexedDbMetadataStore`, `AppStartupService`, `TimeProvider.System`), `Frontend/Program.cs` (start the startup service after the host is built; the hub URL is `Backend:URL` plus `/hubs/media`), `Frontend/wwwroot/appsettings.json` if a key is needed

**Interfaces:**
- Produces: `AppStartupService.Start()`: `TryRestore()`; if signed in, `SyncLibrary()` and `SyncPlaylistList()`, then `MediaHubService.Start()`; when offline or signed out it does only what it can and never throws.

- [ ] **Step 1: Write the failing tests.** Unit `AppStartupServiceTest`: `Start_WhenSignedIn_SyncsBothListsThenConnectsTheHub` (order recorded by fakes); `Start_WhenTheStoredTokenIsRejected_SyncsNothing_AndDoesNotConnect`; `Start_WhenOffline_DoesNotThrow`. Integration `FrontendSyncFlowTest` (real API through `ApiFactory`, real `AuthApiClient`, `SessionService`, `ApiClient`, `SyncService`, `MediaHubService`, with `FakeMetadataStore` and `FakeSessionStorage`): sign up and sign in; upload a media file through the existing `MediaApiClient` test fixture and wait for the hub push; `SyncLibrary` then fills the store with it and a second `SyncLibrary` is a 304 (the fake counts the `If-None-Match` it sent); an edit through the API arrives as a `MediaUpdated` push and updates the store without any ETag change; a delete arrives as `MediaDeleted` and `PlaylistUpdated` for the playlist that held it; after dropping the connection and reconnecting the lists are in sync again; a sign-in restored from the stored refresh token works after the access token expired (advance `FakeTimeProvider` past 15 minutes); an expired refresh token signs the user out without wiping the local lists.
- [ ] **Step 2:** Run the new classes; expected: compile failure.
- [ ] **Step 3:** Implement the startup service and the registrations; make the app start: run `dotnet build` for the solution (0 warnings) and then both test projects.
- [ ] **Step 4:** Report the unit and integration counts before and after the whole plan, and list what the next plan (downloads and the player) can now build on.
- [ ] **Leave uncommitted.**
