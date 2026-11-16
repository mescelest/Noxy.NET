# Frontend Foundation: Solution Layout, Session, Stores and Metadata Sync Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:executing-plans (the user wants this done natively, in the main session, not through subagents). Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Replace the blank Blazor WebAssembly shell with a MAUI Blazor Hybrid app for Windows and Android, and give it everything below the screens that metadata needs: sign in (email and password, and Google) with silent refresh, typed API calls with ETags, a replaceable metadata store (SQLite on disk), and a sync service that keeps the media file and playlist lists current from the start-up diff and the hub, with a search function over the local library.

**Architecture:** Three projects. `Noxy.NET.MediaPlayer.Presentation` is a plain class library with only what any frontend could use: services, models and the contracts for everything platform-specific. `Noxy.NET.MediaPlayer.Storage` is a plain library with the implementations that need only a disk (the SQLite metadata store, later the file store). `Noxy.NET.MediaPlayer.App` replaces `Noxy.NET.MediaPlayer.Frontend`: the MAUI shell, the Blazor screens (the screens plan) and the platform implementations. Services are tested in the unit test project with fakes, the stores in the integration test project, and end to end against the real API (`ApiFactory`).

**Tech Stack:** .NET 10, .NET MAUI Blazor Hybrid (`net10.0-windows10.0.19041.0` and `net10.0-android`), `Microsoft.AspNetCore.SignalR.Client` 10.0.15, SQLite (`Microsoft.Data.Sqlite` or EF Core SQLite, as the builder prefers), `SecureStorage` and `Preferences` from MAUI, xunit v3.

**Spec:** `docs/superpowers/specs/2026-11-15-frontend-maui-design.md` (platform, projects, storage, Google sign-in, server address) and `docs/superpowers/specs/2026-11-15-frontend-sync-player-design.md` (sign in, metadata sync, search, the replaceable store, the backend contracts). The follow-up plans are "downloads and the player" and "screens"; neither starts before this one is done. Settings, the artist, album and genre lists, and the saved playback session are synced by a later plan.

## Global Constraints

- Work in `Noxy.NET.MediaPlayer.Presentation`, `Storage`, `App` and the two test projects, plus the one backend change in Task 1. Target `net10.0` (the App adds the two platform targets). **No MAUI, Blazor, browser or Android type in `Presentation` or `Storage`.**
- One class per file, the file named after it. No summary comment above a class. Interfaces in an `Interfaces` folder, in a subfolder matching the implementation's folder (`Interfaces/Services` for `Services`, `Interfaces/Stores` for `Stores`); namespaces mirror folders. Extensions in an `Extensions` folder. Lists end in `List`. No `yield`. No magic strings (constants, `nameof`, or `nameof(MediaHubEvent.X)` for hub events). `is null`. No `!` (handle the null with a descriptive throw). Method-syntax LINQ. Media, never "song". Injected dependencies named kind first (`serviceSession`, `storeMetadata`, `clientApi`, `storageSession`); `time` keeps its name.
- Requests are in `Noxy.NET.MediaPlayer.Application.Requests`, responses in `Noxy.NET.MediaPlayer.Domain.Responses`; the app sends and reads exactly those classes. Hub event names come from `MediaHubEvent`.
- The access token lives in memory only; the refresh token is the only credential written to disk, and only in `SecureStorage`. No Google token or password is stored. Neither token, nor a password, appears in a log or an exception message.
- A stored ETag is only ever one that arrived with a full `200` response the device downloaded. A push never writes an ETag.
- Tests: xunit v3, unit tests in `Noxy.NET.Test.UnitTest/MediaPlayer/Presentation/`, store and end-to-end tests in `Noxy.NET.Test.IntegrationTest/MediaPlayer/Presentation/`. Build the test project, then run its exe, for example `Noxy.NET.Test/Noxy.NET.Test.UnitTest/bin/Debug/net10.0/Noxy.NET.Test.UnitTest.exe -class "*SyncServiceTest"`; `dotnet test` finds nothing. Pass `TestContext.Current.CancellationToken` to every call that takes a token. Zero warnings. A test folder contains only tests: fakes go in `Fakes`, helpers in `Fixtures` (extensions in `Fixtures/Extensions`). `ValueTask` or `Task` results are wrapped with `.AsTask()` where needed inside `Assert.ThrowsAsync`. A MAUI project cannot be referenced from the test projects, so everything testable lives in `Presentation` or `Storage`.
- Do not commit. The user reviews the diff in the IDE; `git add -N` files you create, by name. Do not run machine-level commands (installing workloads, the Android SDK or tools): give the user the command instead. Write files with the editor tool when they contain quotes, apostrophes, backslashes or `$`. If a file seems to have lost an edit, re-read it before editing.

## Review Focus

- **Two requests refresh at once.** Refresh tokens are single use and reuse revokes the whole account, so two 401s at the same moment must produce one refresh call, not two (Tasks 3 and 4 each pin it).
- **Cleared app data.** A stored ETag with an empty local list (the app data was cleared, or the database file is gone) must not turn the next `304` into an empty library: sync sends no `If-None-Match` when the local list is empty (Task 7).
- **Out-of-order or repeated pushes.** An older `timeModified` never overwrites a newer local copy (an equal `timeModified` with a newer `timeLastPlayed` does apply), a delete for an unknown ID is harmless, and a push for a playlist whose entries are not held does not fetch them (Task 8).
- **Offline at start-up.** A failed network call leaves the local lists and ETags untouched and reports offline; it never clears the library or throws out of start-up (Tasks 4, 7, 10).
- **Expired or revoked session.** A refresh that fails with 401 signs the user out cleanly (tokens cleared, local metadata kept) instead of looping on 401 (Tasks 3 and 4).
- **Wrong or missing server address.** Before an address is saved nothing calls the network; a bad address surfaces as offline or a clear error, never a crash at start-up (Tasks 4 and 10).
- **Google sign-in cancelled or failing.** The user closing the browser or denying access ends signed out with no error text from Google leaking into a log, and never leaves a half-signed-in session (Tasks 3 and 11).

## File Structure

New files (relative to the project named; tests relative to the test projects' `MediaPlayer/Presentation/`):
- `Presentation/Interfaces/Services/`: `IAuthApiClient.cs`, `IApiClient.cs`, `ISessionService.cs`, `ISyncService.cs`, `IMediaHubService.cs`, `IGoogleSignInService.cs`, `IAppStartupService.cs`
- `Presentation/Interfaces/Stores/`: `IMetadataStore.cs`, `ISessionStorage.cs`, `IServerAddressStorage.cs`
- `Presentation/Services/`: `AuthApiClient.cs`, `ApiClient.cs`, `SessionService.cs`, `SyncService.cs`, `MediaHubService.cs`, `AppStartupService.cs`; the existing `APIHttpClient.cs` is replaced by `ApiClient`
- `Presentation/Models/`: `ApiResponse.cs`, `LibrarySort.cs`; `Exceptions/ApiException.cs`, `Exceptions/ServerAddressMissingException.cs`; `Extensions/StringExtensions.cs` (access token expiry, server address), `Extensions/HttpResponseMessageExtension.cs`, `Extensions/ResponseMediaFileListExtension.cs` (search and sort); `Constants.cs`
- `Storage/Stores/`: `SqliteMetadataStore.cs`
- `App/`: the MAUI project (shell page with one `BlazorWebView`, `MauiProgram.cs`, Blazor `_Imports.razor`, `wwwroot`), `Stores/SecureSessionStorage.cs`, `Stores/PreferencesServerAddressStorage.cs`, `Services/GoogleSignInService.cs` (and per-platform parts)
- Tests: unit `Fakes/FakeHttpMessageHandler.cs`, `Fakes/FakeSessionStorage.cs`, `Fakes/FakeServerAddressStorage.cs`, `Fakes/FakeMetadataStore.cs`, `Fakes/FakeApiClient.cs`, `Fakes/FakeAuthApiClient.cs`, `Fakes/FakeSessionService.cs`, `Fakes/FakeGoogleSignInService.cs`, `Fixtures/MetadataStoreContract.cs`; test classes named after the class they test; integration `SqliteMetadataStoreTest.cs`, `FrontendSyncFlowTest.cs`

Modified: `Presentation.csproj` (plain SDK, SignalR client package), `Test.UnitTest.csproj` and `Test.IntegrationTest.csproj` (reference `Presentation` and `Storage`; the integration project links `MetadataStoreContract.cs` from the unit project), `Noxy.NET.sln`, backend `ConfigGoogle`/`GoogleTokenService` and the API `appsettings.json`.

Removed: `Noxy.NET.MediaPlayer.Frontend` (project, folder and solution entry), `Presentation/Services/APIHttpClient.cs`.

Contracts every task shares:
- `record ApiResponse<T>(HttpStatusCode Status, T? Value, string? ETag)` with `bool IsNotModified => Status == HttpStatusCode.NotModified`. `class ApiException(HttpStatusCode status, string title) : Exception(title)` with `Status` and `Title`.
- `IMetadataStore` (all members `Task`-based): `GetMediaFileList() → List<ResponseMediaFile>`, `ReplaceMediaFileList(List<ResponseMediaFile>)`, `SaveMediaFile(ResponseMediaFile)`, `DeleteMediaFile(Guid id)`, `GetPlaylistList() → List<ResponsePlaylist>`, `ReplacePlaylistList(List<ResponsePlaylist>)`, `SavePlaylist(ResponsePlaylist)`, `DeletePlaylist(Guid id)` (also removes that playlist's detail), `GetPlaylistDetail(Guid id) → ResponsePlaylistDetail?`, `SavePlaylistDetail(ResponsePlaylistDetail)`, `GetLibraryETag() → string?`, `SetLibraryETag(string?)`, `GetPlaylistListETag() → string?`, `SetPlaylistListETag(string?)`, `GetHeldPlaylistIDList() → List<Guid>` (playlists whose detail is stored). A `Replace…List` is one atomic write.
- `ISessionStorage`: `Task<string?> GetRefreshToken()`, `Task SetRefreshToken(string token)`, `Task ClearRefreshToken()`.
- `IServerAddressStorage`: `Task<string?> GetAddress()`, `Task SetAddress(string address)`, `Task ClearAddress()`. An address is an absolute `http` or `https` URL without a trailing slash; `SetAddress` normalises it and throws `ArgumentException` for anything else.
- `IGoogleSignInService`: `Task<string?> GetIdToken(CancellationToken cancellationToken)`, the Google ID token, or null when the user cancelled or denied. Platform implementation in the App.
- `IAuthApiClient`: `Task<ResponseAuthentication> SignUp(string email, string password)`, `SignIn(...)`, `Task<ResponseAuthentication> SignInGoogle(string idToken)`, `Task<ResponseAuthentication> Refresh(string refreshToken)`; no bearer header; non-success throws `ApiException`.
- `ISessionService`: `bool IsSignedIn`, `event Action? Changed`, `Task SignUp(email, password)`, `Task SignIn(email, password)`, `Task<bool> SignInWithGoogle()` (false when the user cancelled), `Task<bool> TryRestore()` (silent sign-in from the stored refresh token), `Task SignOut()`, `Task<string?> GetAccessToken()` (null when signed out; refreshes first when the token expires within `Constants.RefreshMarginSeconds` = 60), `Task<bool> Refresh()` (one refresh, shared by concurrent callers; false when it failed and the user was signed out).
- `IApiClient`: `Task<ApiResponse<List<ResponseMediaFile>>> GetLibrary(string? eTag)`, `Task<ApiResponse<List<ResponsePlaylist>>> GetPlaylistList(string? eTag)`, `Task<ApiResponse<ResponsePlaylistDetail>> GetPlaylistDetail(Guid id, string? eTag)`, and the playlist writes `Task<ResponsePlaylist> CreatePlaylist(RequestCreatePlaylist)`, `Task<ResponsePlaylist> UpdatePlaylist(Guid, RequestUpdatePlaylist)`, `Task DeletePlaylist(Guid)`, and `Task<ResponsePlaylistDetail> AddPlaylistEntryList(Guid, RequestAddPlaylistEntry)`, `RemovePlaylistEntryList(Guid, RequestRemovePlaylistEntry)`, `MovePlaylistEntry(Guid id, Guid entryID, RequestMovePlaylistEntry)`. Every call goes to the saved server address; with none saved it throws `ServerAddressMissingException` (new, in `Exceptions/`).
- `ISyncService`: `event Action? Changed`, `bool IsOffline`, `Task SyncLibrary(bool force = false)`, `Task SyncPlaylistList(bool force = false)`, `Task SyncPlaylistDetail(Guid id)` (always a full download), and the push handlers `Task ApplyMediaUpdated(ResponseMediaFile)`, `ApplyMediaDeleted(Guid)`, `ApplyPlaylistUpdated(ResponsePlaylist)`, `ApplyPlaylistDeleted(Guid)`, `ApplyUploadSucceeded(ResponseUploadResult)`.

---

### Task 1: Backend, a list of Google client IDs

**Files:**
- Modify: `Application/Configurations/ConfigGoogle.cs` (`ClientId` becomes `List<string> ClientIdList`), `Application/Services/GoogleTokenService.cs`, `API/appsettings.json` (`"Google": { "ClientIdList": [] }`), the integration test configuration if it sets `Google:ClientId`, the authentication design doc (the sentence about `Google:ClientId`), and `GoogleTokenService` tests if any exist

**Interfaces:**
- Produces: `ConfigGoogle.ClientIdList`; `GoogleTokenService.Validate` accepts a token whose audience is any entry of the list. A list that is empty or holds only blanks still means Google is off (`GoogleLoginNotConfiguredException`, `503`).

- [ ] **Step 1: Write the failing tests.** The token check itself calls Google's keys, so test what is testable without the network: a unit test that an empty list, a list of blanks and a list with a blank entry beside a real one are decided by the config alone (`ConfigGoogle` exposes `ConfiguredClientIdList`, the list without blanks, and `GoogleTokenService` throws `GoogleLoginNotConfiguredException` before any call when it is empty); a blank token still throws `AuthenticationFailedException` when Google is configured. Keep the existing `HandlerAuthenticationSignInGoogleTest` cases passing unchanged.
- [ ] **Step 2:** Run the new classes; expected: compile failure. Implement: pass `ConfiguredClientIdList` as the `Audience` of `GoogleJsonWebSignature.ValidateAsync`.
- [ ] **Step 3:** Build the solution (0 warnings) and run both test projects; expected: all pass.
- [ ] **Leave uncommitted.**

---

### Task 2: Solution layout and the MAUI shell

**Files:**
- Modify: `Presentation.csproj` (plain `Microsoft.NET.Sdk`; remove the Blazor, WebAssembly and `Blazored.LocalStorage` packages and the `browser` supported platform; add `Microsoft.AspNetCore.SignalR.Client` 10.0.15 and `Microsoft.Extensions.Http`; delete `_Imports.razor`), `Noxy.NET.sln`, `Test.UnitTest.csproj` and `Test.IntegrationTest.csproj`
- Create: `Storage/Noxy.NET.MediaPlayer.Storage.csproj` (plain library referencing `Presentation`), `App/` (from the .NET MAUI Blazor Hybrid template), `Presentation/Constants.cs` (`RefreshTokenKey`, `RefreshMarginSeconds = 60`), `Presentation/Models/ApiResponse.cs`, `Presentation/Exceptions/ApiException.cs`, `Presentation/Exceptions/ServerAddressMissingException.cs`
- Delete: `Noxy.NET.MediaPlayer.Frontend` (folder and solution entry), `Presentation/Services/APIHttpClient.cs`

**Interfaces:**
- Produces: the three types and the constant above; an `App` that builds for Windows and starts to a placeholder Blazor page; the project references `Presentation` and `Storage`.

- [ ] **Step 1:** Check the MAUI workload is installed with `dotnet workload list`. If `maui` (or `maui-windows`) is missing, **stop and give the user the command** (`dotnet workload install maui`, plus the Android SDK from Visual Studio's installer) and wait. Do not install it yourself.
- [ ] **Step 2:** Create the App with `dotnet new maui-blazor -n Noxy.NET.MediaPlayer.App` in `Noxy.NET.MediaPlayer/`, keep only the Windows and Android targets, set `WindowsPackageType` to `None` (unpackaged, simplest to run), remove the template's sample pages and data, and keep one root component with a placeholder page that shows the app name. Reference `Presentation` and `Storage`; reference `Noxy.NET.Shared` as the old Frontend did. Delete the Frontend project and replace its solution entry with the three new projects; make the Presentation changes above.
- [ ] **Step 3:** Add the three small types and the constant. Build the whole solution for both platform targets (0 warnings) and run both test projects; expected: everything that passed before still passes (the unit and integration test projects reference only the plain libraries).
- [ ] **Step 4:** Run the App on Windows (`dotnet run -f net10.0-windows10.0.19041.0` from its folder, or the IDE) and check the placeholder page appears. Record what was done and the Android build result in the report; if Android cannot build because its SDK is missing, say so and carry on, since the SDK is the user's install.
- [ ] **Leave uncommitted.**

---

### Task 3: Session service

**Files:**
- Create: `Interfaces/Services/IAuthApiClient.cs`, `Interfaces/Services/ISessionService.cs`, `Interfaces/Services/IGoogleSignInService.cs`, `Interfaces/Stores/ISessionStorage.cs`, `Services/AuthApiClient.cs`, `Services/SessionService.cs`, `Extensions/StringExtensions.cs`, `Extensions/HttpResponseMessageExtension.cs`; tests `Fakes/FakeAuthApiClient.cs`, `Fakes/FakeSessionStorage.cs`, `Fakes/FakeGoogleSignInService.cs`, `Fakes/FakeHttpMessageHandler.cs`, `AuthApiClientTest.cs`, `SessionServiceTest.cs`, `StringExtentionsTest.cs`, `HttpResponseMessageExtensionTest.cs`

**Interfaces:**
- Consumes: the contracts above.
- Produces: an extension `string.GetAccessTokenExpires() : DateTime?` (the `exp` claim of the unvalidated payload, UTC; null when it cannot be read); `SessionService(IAuthApiClient clientAuthApi, IGoogleSignInService serviceGoogleSignIn, ISessionStorage storageSession, TimeProvider time)`; `AuthApiClient(HttpClient)` posting `RequestSignUp`, `RequestSignIn`, `RequestSignInGoogle`, `RequestRefresh` to `/Auth/SignUp`, `/Auth/SignIn`, `/Auth/Google`, `/Auth/Refresh`. The `HttpClient` has its base address set per request from the saved server address (see Task 4's note; here the client takes the address through an `IServerAddressStorage` so it can be faked).

- [ ] **Step 1: Write the failing tests.** `StringExtentionsTest`: a token built by hand with a known `exp` returns it; garbage, a token with two segments, and a payload without `exp` return null. `AuthApiClientTest` (with `FakeHttpMessageHandler` and `FakeServerAddressStorage`): each call sends the right method, path under the saved address and JSON body, no `Authorization` header, and returns the parsed tokens; a 401 problem details response throws `ApiException` with status 401 and its title, and a 409 the same; with no saved address it throws `ServerAddressMissingException` and sends nothing. `SessionServiceTest`: `SignIn_StoresTheRefreshTokenAndKeepsTheAccessTokenInMemoryOnly` (the fake storage holds the refresh token, nothing else); `SignInWithGoogle_SendsTheIdTokenAndSignsIn_AndStoresOnlyOurRefreshToken`; `SignInWithGoogle_WhenTheUserCancels_ReturnsFalse_AndStaysSignedOut_WithNoServerCall`; `SignInWithGoogle_WhenTheServerRejectsTheToken_ThrowsApiException_AndStaysSignedOut`; `TryRestore_WithAStoredRefreshToken_SignsIn_AndRotatesTheStoredToken`; `TryRestore_WithNoStoredToken_ReturnsFalse`; `TryRestore_WhenTheRefreshIsRejected_ClearsTheStoredToken_AndReturnsFalse`; `TryRestore_WhenTheNetworkIsDown_KeepsTheStoredToken_AndStaysSignedOut` (an `HttpRequestException` is not a rejection); `GetAccessToken_WhenItExpiresWithinTheMargin_RefreshesFirst`; `GetAccessToken_WhenSignedOut_ReturnsNull`; `ConcurrentRefresh_CallsTheServerOnce` (ten parallel `Refresh()` calls, the fake counts one request); `Refresh_WhenRejected_SignsOut_AndRaisesChanged`; `SignOut_ClearsEverything`; `Changed_IsRaisedOnSignInAndSignOut`.
- [ ] **Step 2:** Run the classes; expected: compile failure.
- [ ] **Step 3:** Implement. `SessionService` serialises refreshes with a `SemaphoreSlim`; a caller that gets the lock after another refresh succeeded returns its result without calling the server. A rejection means an `ApiException` with status 401 (or 400); any other exception propagates and leaves the stored token alone. Expiry comes from `GetAccessTokenExpires` and `time`.
- [ ] **Step 4:** Run the unit project; expected: all pass.
- [ ] **Leave uncommitted.**

---

### Task 4: Authorised API client and the server address

**Files:**
- Create: `Interfaces/Services/IApiClient.cs`, `Interfaces/Stores/IServerAddressStorage.cs`, `Services/ApiClient.cs`; tests `Fakes/FakeServerAddressStorage.cs`, `Fakes/FakeSessionService.cs`, `ApiClientTest.cs`

**Interfaces:**
- Consumes: `ISessionService.GetAccessToken()` and `Refresh()`, `IServerAddressStorage`.
- Produces: `ApiClient(HttpClient, ISessionService serviceSession, IServerAddressStorage storageServerAddress)` implementing `IApiClient` as listed above. The address is read for every request (so a changed address takes effect without a restart) and the request URL is the address plus the path.

- [ ] **Step 1: Write the failing tests** (fake handler, `FakeSessionService`, `FakeServerAddressStorage`): `GetLibrary_SendsTheBearerTokenAndTheETagAsIfNoneMatch`; `GetLibrary_On200_ReturnsTheListAndTheETagHeader`; `GetLibrary_On304_ReturnsNotModifiedWithNoValue`; `GetLibrary_WithNoETag_SendsNoIfNoneMatch`; `GetLibrary_UsesTheSavedServerAddress_AndANewAddressAppliesToTheNextCall`; `AnyCall_WithNoSavedAddress_ThrowsServerAddressMissingException_AndSendsNothing`; `AUnauthorizedResponse_RefreshesOnceAndRetriesOnce` (second request carries the new token); `AUnauthorizedResponse_AfterTheRefreshFailed_ThrowsApiException401_AndDoesNotRetryAgain`; `ConcurrentUnauthorizedResponses_RefreshOnce` (five parallel calls, the fake session counts one `Refresh()` that actually ran, as `SessionService` serialises it); `ANetworkFailure_PropagatesAsHttpRequestException`; one test per write method for method, path (`/Playlist`, `/Playlist/{id}`, `/Playlist/{id}/EntryList`, `/Playlist/{id}/EntryList/Remove`, `/Playlist/{id}/Entry/{entryID}/Position`) and JSON body, and that a 404 or 409 problem response throws `ApiException` with its status and title.
- [ ] **Step 2:** Run `*ApiClientTest`; expected: compile failure.
- [ ] **Step 3:** Implement with one private send method that builds the request, adds the bearer token, and on a 401 calls `serviceSession.Refresh()` once and sends a fresh copy of the request once more. Responses with a non-success status other than 304 throw `ApiException` from the problem details title. Bodies are read with `System.Net.Http.Json`; the ETag is `response.Headers.ETag?.Tag` (the quoted tag as sent). Make `AuthApiClient` (Task 3) use the same address rule.
- [ ] **Step 4:** Run the unit project; expected: all pass.
- [ ] **Leave uncommitted.**

---

### Task 5: Metadata store contract, with the fake

**Files:**
- Create: `Interfaces/Stores/IMetadataStore.cs`; tests `Fakes/FakeMetadataStore.cs`, `Fixtures/MetadataStoreContract.cs` (an abstract class of tests, run by each store implementation), `FakeMetadataStoreTest.cs`

**Interfaces:**
- Produces: `IMetadataStore` as listed above. `MetadataStoreContract` has `protected abstract IMetadataStore CreateStore()` and the tests below as `[Fact]` methods, so the SQLite store in Task 6 runs the same tests (the integration project links the file from the unit project).

- [ ] **Step 1: Write the contract tests:** an empty store returns empty lists, no ETags and no detail; `ReplaceMediaFileList` replaces rather than merges; `SaveMediaFile` inserts or replaces by ID; `DeleteMediaFile` of an unknown ID is harmless; the same for playlists; `DeletePlaylist` also removes the held detail and its ID from `GetHeldPlaylistIDList`; `SavePlaylistDetail` makes the ID held; ETags round-trip and `SetLibraryETag(null)` clears; `ReplaceMediaFileList` with 5000 items returns all 5000 in the given order; every field of a saved `ResponseMediaFile`, `ResponsePlaylist` and `ResponsePlaylistDetail` (including a null `timeLastPlayed`, a repeated media file in the entries and a media file with no album) comes back equal.
- [ ] **Step 2:** Run `*FakeMetadataStoreTest`; expected: compile failure. Implement `FakeMetadataStore` (in-memory) and make it pass.
- [ ] **Leave uncommitted.**

---

### Task 6: SQLite metadata store

**Files:**
- Create: `Storage/Stores/SqliteMetadataStore.cs`; integration test `SqliteMetadataStoreTest.cs` (inherits `MetadataStoreContract`)
- Modify: `Test.IntegrationTest.csproj` (link `MetadataStoreContract.cs`)

**Interfaces:**
- Consumes: `IMetadataStore`. Produces: `SqliteMetadataStore(string databasePath)` implementing it; it creates the file and its tables on first use.

- [ ] **Step 1: Write the failing test class:** `SqliteMetadataStoreTest` inherits the contract and creates each store in a fresh temporary file (deleted in `Dispose`); extra tests: `ASecondStoreOnTheSameFile_SeesWhatTheFirstWrote` (data survives a restart); `ReplaceMediaFileList_IsOneTransaction` (a failure half-way, forced by a value the table rejects, leaves the old list untouched); `AnUnreadableDatabaseFile_IsReplacedByAnEmptyOne_AndTheLibraryIsEmpty` (a corrupt file does not crash start-up; sync then rebuilds it); `ConcurrentWrites_DoNotThrow`.
- [ ] **Step 2:** Run `*SqliteMetadataStoreTest`; expected: compile failure. Implement: one table per kind with the response as JSON text keyed by `ID` (the API's shape is the schema, so a new field needs no migration), a key-value table for the two ETags, every `Replace…List` in one transaction, JSON through `JsonSerializerOptions.Web`; connection per call with `Pooling` and WAL so reads do not block writes.
- [ ] **Step 3:** Run the integration project; expected: all pass.
- [ ] **Leave uncommitted.**

---

### Task 7: Sync service, the diffs

**Files:**
- Create: `Interfaces/Services/ISyncService.cs`, `Services/SyncService.cs`; tests `Fakes/FakeApiClient.cs`, `SyncServiceTest.cs`

**Interfaces:**
- Consumes: `IApiClient`, `IMetadataStore`. Produces: `SyncService(IApiClient clientApi, IMetadataStore storeMetadata)` with `SyncLibrary`, `SyncPlaylistList`, `SyncPlaylistDetail`, `IsOffline` and `Changed` (the push handlers come in Task 8).

- [ ] **Step 1: Write the failing tests** with `FakeApiClient` (scripted responses, records the ETags it was sent) and `FakeMetadataStore`: `SyncLibrary_FirstRun_SendsNoETag_StoresTheListAndTheETag`; `SyncLibrary_On304_ChangesNothing`; `SyncLibrary_On200_ReplacesTheLocalListAndStoresTheNewETag` (a file gone from the server is gone locally); `SyncLibrary_WhenTheLocalListIsEmptyButAnETagIsStored_SendsNoETag` (cleared data) and then stores the full list; `SyncLibrary_Force_SendsNoETag_EvenWhenOneIsStored`; `SyncLibrary_WhenTheNetworkIsDown_LeavesListAndETagAlone_AndSetsOffline`; `SyncLibrary_WhenNoServerAddressIsSaved_DoesNothing_AndSetsOffline`; `SyncLibrary_AfterAFailure_ThenSuccess_ClearsOffline`; `SyncPlaylistList` the same behaviours; `SyncPlaylistList_ReFetchesAHeldDetailWhoseSummaryIsNewer` and leaves an up-to-date one alone; `SyncPlaylistList_DropsTheDetailOfAPlaylistThatNoLongerExists`; `SyncPlaylistDetail_AlwaysDownloadsInFull_AndStoresIt`; `SyncPlaylistDetail_WhenTheServerSays404_DeletesTheLocalPlaylist`; `Changed_IsRaisedAfterEachStoreWrite`.
- [ ] **Step 2:** Run `*SyncServiceTest`; expected: compile failure.
- [ ] **Step 3:** Implement. Only an `HttpRequestException`, a `ServerAddressMissingException` or a `TaskCanceledException` that is not a user cancellation means offline; an `ApiException` other than the 404 above propagates. The ETag is stored only after the list it came with is stored. The playlist summary is newer when its `TimeModified` is later, or equal with a later `TimeLastPlayed` only for the summary itself (a held detail is re-fetched only for a later `TimeModified`, since a play does not change the entries).
- [ ] **Step 4:** Run the unit project; expected: all pass.
- [ ] **Leave uncommitted.**

---

### Task 8: Push handling and the hub

**Files:**
- Create: `Interfaces/Services/IMediaHubService.cs`, `Services/MediaHubService.cs`; tests added to `SyncServiceTest.cs` and a new `MediaHubServiceTest.cs`
- Modify: `Services/SyncService.cs`

**Interfaces:**
- Consumes: `ISyncService`, `ISessionService.GetAccessToken()`, `IServerAddressStorage`. Produces: the five `Apply…` members on `SyncService`; `MediaHubService(ISyncService serviceSync, ISessionService serviceSession, IServerAddressStorage storageServerAddress)` with `Task Start()` and `Task Stop()`; the hub URL is the saved address plus `/hubs/media`.

- [ ] **Step 1: Write the failing tests** (sync service): `ApplyMediaUpdated_SavesIt_AndKeepsTheStoredETag`; `ApplyMediaDeleted_RemovesIt_AndAnUnknownIDIsHarmless`; `ApplyUploadSucceeded_WithAMedia_SavesIt`; `ApplyUploadSucceeded_WithNoMedia_ChangesNothing`; `ApplyPlaylistUpdated_SavesTheSummary`; `ApplyPlaylistUpdated_WithANewerTimeThanAHeldDetail_RefetchesThatDetail`; `ApplyPlaylistUpdated_WithAnOlderTimeThanTheLocalSummary_IsIgnored`; `ApplyPlaylistUpdated_WithTheSameModifiedTimeButANewerLastPlayedTime_IsApplied_AndDoesNotRefetchTheDetail`; `ApplyPlaylistUpdated_ForAPlaylistWhoseDetailIsNotHeld_FetchesNothing`; `ApplyPlaylistDeleted_RemovesPlaylistAndDetail`; a push never calls `SetLibraryETag` or `SetPlaylistListETag`.
- [ ] **Step 2:** Hub tests are in the integration project (Task 10) because they need the real hub; `MediaHubServiceTest` covers only what is unit-testable: the handler names registered are exactly `nameof(MediaHubEvent.X)` for the events this plan handles (expose the list as a `static IReadOnlyList<string> HandledEventNameList` on the class and assert it equals those enum names), and `Start` with no saved address does nothing and does not throw.
- [ ] **Step 3:** Implement. `MediaHubService` builds the connection with `WithUrl(url, options => options.AccessTokenProvider = …)`, `WithAutomaticReconnect()`, forwards each event to the matching `Apply…`, and on `Reconnected` runs `SyncLibrary()` and `SyncPlaylistList()` (pushes sent while offline are lost). It never lets an exception from a handler escape into the connection. Events for data a later plan syncs (settings, artists, albums, genres, the playback session) are not subscribed to yet.
- [ ] **Step 4:** Run the unit project; expected: all pass.
- [ ] **Leave uncommitted.**

---

### Task 9: Search and sort

**Files:**
- Create: `Extensions/ResponseMediaFileListExtension.cs`, `Models/LibrarySort.cs`; test `ResponseMediaFileListExtensionTest.cs`

**Interfaces:**
- Produces: extensions on `List<ResponseMediaFile>`: `Filter(string? query) : List<ResponseMediaFile>` and `Sort(LibrarySort sort, bool descending) : List<ResponseMediaFile>`, and `enum LibrarySort { Title, Artist, Album, Year, TimeCreated }`.

- [ ] **Step 1: Write the failing tests:** a blank or null query returns everything in the given order; a query matches title, artist, album, album artist and genre, case-insensitively and by substring (the media file's artist, album and genre text as the API returns it); several words must all match (in any of the fields); a null field never throws; accents are matched as typed (no folding); sort is stable (equal keys keep the input order) and null keys sort last in both directions.
- [ ] **Step 2:** Run `*ResponseMediaFileListExtensionTest`; expected: compile failure. Implement; run; expected: pass.
- [ ] **Leave uncommitted.**

---

### Task 10: Wiring, start-up and the end-to-end test

**Files:**
- Create: `Services/AppStartupService.cs` and its interface `IAppStartupService` (`Task Start()`), `App/Stores/SecureSessionStorage.cs`, `App/Stores/PreferencesServerAddressStorage.cs`, integration test `FrontendSyncFlowTest.cs` and fixtures it needs under `Fixtures/`; a `ServiceCollectionExtensions.cs` in `Presentation` (`AddPresentation`) and one in `Storage` (`AddPresentationLocal(string dataFolder)`)
- Modify: `App/MauiProgram.cs` (register `AddPresentation`, `AddPresentationLocal` with `FileSystem.AppDataDirectory`, the two App stores and the Google service, and start `IAppStartupService` after the app is built)

**Interfaces:**
- Produces: `AppStartupService.Start()`: `TryRestore()`; if signed in, `SyncLibrary()` and `SyncPlaylistList()`, then `MediaHubService.Start()`; with no saved address, when offline or when signed out it does only what it can and never throws. `SecureSessionStorage` wraps `SecureStorage` under `Constants.RefreshTokenKey`; `PreferencesServerAddressStorage` wraps `Preferences`.

- [ ] **Step 1: Write the failing tests.** Unit `AppStartupServiceTest`: `Start_WhenSignedIn_SyncsBothListsThenConnectsTheHub` (order recorded by fakes); `Start_WhenTheStoredTokenIsRejected_SyncsNothing_AndDoesNotConnect`; `Start_WithNoServerAddress_DoesNothing_AndDoesNotThrow`; `Start_WhenOffline_DoesNotThrow`. Integration `FrontendSyncFlowTest` (real API through `ApiFactory`, real `AuthApiClient`, `SessionService`, `ApiClient`, `SyncService`, `MediaHubService`, `SqliteMetadataStore` on a temporary file, `FakeSessionStorage` and `FakeServerAddressStorage` pointing at the test server): sign up and sign in; upload a media file through the existing `MediaApiClient` test fixture and wait for the hub push; `SyncLibrary` then fills the store with it and a second `SyncLibrary` is a 304 (the fake counts the `If-None-Match` it sent); an edit through the API arrives as a `MediaUpdated` push and updates the store without any ETag change; a delete arrives as `MediaDeleted` and `PlaylistUpdated` for the playlist that held it; a `Played` call arrives as `PlaylistUpdated` with the new `timeLastPlayed` and the same `timeModified`; after dropping the connection and reconnecting the lists are in sync again; a sign-in restored from the stored refresh token works after the access token expired (advance `FakeTimeProvider` past 15 minutes); an expired refresh token signs the user out without wiping the local lists; a fresh store file after the old one was deleted is refilled by the next `SyncLibrary` even though the ETag is gone with it.
- [ ] **Step 2:** Run the new classes; expected: compile failure.
- [ ] **Step 3:** Implement the startup service, the registrations and the two App stores; build the solution for both platform targets (0 warnings) and run both test projects.
- [ ] **Step 4:** Manual run on Windows: start the API, start the App, enter the server address, check the app starts without a crash both with and without an address and with the API stopped. (No screens exist yet; the check is the log and that nothing throws.) Record what was done.
- [ ] **Step 5:** Report the unit and integration counts before and after the whole plan, and list what the next plans (Google sign-in on the device, downloads and the player) can now build on.
- [ ] **Leave uncommitted.**

---

### Task 11: Google sign-in on the device

**Files:**
- Create: `App/Services/GoogleSignInService.cs` (implementing `IGoogleSignInService`) with the per-platform parts under `App/Platforms/Windows` and `App/Platforms/Android`; a small PKCE helper in `Presentation` if it is platform-neutral (`Models/PkceChallenge.cs`, a record with a static `Create()` with tests `PkceChallengeTest.cs`: the verifier is 43 to 128 characters of the allowed set, the challenge is the base64url SHA-256 of it, and two verifiers differ)
- Modify: `App/MauiProgram.cs`, the Android manifest (the redirect scheme), the design doc (record the redirect set-up that worked)

**Interfaces:**
- Consumes: `IGoogleSignInService`. Produces: the real implementation: open Google's authorisation URL in the system browser with `response_type=code`, `scope=openid email`, `code_challenge`, `code_challenge_method=S256` and a random `state`; receive the redirect (a loopback `HttpListener` on a free port on Windows, a custom scheme on Android); check `state`; exchange the code with the verifier at Google's token endpoint; return the `id_token`. A closed browser, a denied request, a `state` mismatch or a network failure returns null (or throws a descriptive exception for a state mismatch), and nothing from the exchange is logged or stored.

- [ ] **Step 1:** Stop and ask the user for the two Google client IDs (Android and Windows desktop) if they are not in the App's configuration yet; give them the steps from the design ("What the user has to do") rather than doing it. The client IDs are public and go in the App's configuration, and the backend `Google:ClientIdList` must contain both.
- [ ] **Step 2: Write the failing tests** for `PkceChallenge` and run them (compile failure, then pass once implemented).
- [ ] **Step 3:** Implement the flow with the user's client IDs. This is a spike in the design's words: confirm on a real run that the code exchange returns an `id_token` and that the backend accepts it (`POST /Auth/Google` returns the tokens).
- [ ] **Step 4:** Manual acceptance on Windows (and Android when its SDK is installed): sign in with Google, see the app signed in (the log, or a temporary line), restart the app and be restored silently, cancel the browser and stay signed out, and sign in again after `SignOut`. Record the results, and any platform that could not be tried, in the report.
- [ ] **Leave uncommitted.**
