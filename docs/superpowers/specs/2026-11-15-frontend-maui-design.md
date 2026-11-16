# Frontend as a MAUI Blazor Hybrid app (Windows and Android)

Status: approved by the user on 2026-11-15 ("for now"), written from the conversation. It changes the platform of `2026-11-15-frontend-sync-player-design.md` and `2026-11-15-frontend-screens-design.md` from Blazor WebAssembly to a MAUI Blazor Hybrid app. Lines marked *(my call)* are decisions I made without asking.

## Intent

The app has to keep a library of up to 5000 media files on the device, play them offline, and feel like a normal desktop and phone app. A browser cannot do that well: its storage can be dropped and has a quota, and it has no real file system, media keys or background playback. A MAUI app on Windows and Android stores plain files on disk and can use the platform's own audio, secure storage and system browser. Everything below the screens stays as designed; only the platform pieces under it change.

## Goals

- One app for **Windows and Android**, with the screens written once as Blazor components.
- `Noxy.NET.MediaPlayer.Presentation` holds only what any frontend could use (a Blazor WebAssembly host, a MAUI host, a console tool): API clients, session, sync, download, player logic, models, and the **contracts** for everything platform-specific.
- The metadata and the audio files live on disk and survive restarts. No limit imposed by a browser.
- Google sign-in works on both platforms. There is no password reset, so Google is the main way in; email and password remains.

## Non-goals

- iOS and macOS (they need a Mac to build). Nothing here prevents adding them later.
- A web version. The browser host is dropped, not kept in step.
- Background playback, media keys and a system media notification **in the first version**. The audio contract is shaped so they can be added without touching the player logic.
- Password reset. It is a gap in the backend, noted below, and not part of this work.

## Anti-goals

- Platform code leaking into `Presentation`: no MAUI, Blazor, browser or Android type may appear there.
- A store that works only on one platform's file layout: the stores take a base folder from the host.
- Google sign-in that stores a Google credential on the device. Only our own tokens are kept.

## Approaches considered

1. **MAUI Blazor Hybrid, screens in the app project (chosen).** One project holds the shell, the Blazor components and the platform implementations. Fewest projects.
2. **MAUI shell with the screens in a separate Razor class library.** Lets a web host reuse the screens later. More structure now for a host that is not planned.
3. **Native MAUI XAML pages.** Best native feel, but throws away the Blazor skills and the existing `Noxy.NET.UI` component libraries.

## Decisions

**Project layout.**
- `Noxy.NET.MediaPlayer.Presentation`: becomes a plain class library (`Microsoft.NET.Sdk`, no Blazor, no browser package). Contains the services and the contracts: `IApiClient`, `ISessionService`, `ISyncService`, `IDownloadService`, `IPlayerService`, `IMetadataStore`, `IFileStore`, `ISessionStorage`, `IAudioPlayer`, `IGoogleSignInService`, `IServerAddressStorage`, and the models.
- `Noxy.NET.MediaPlayer.Storage` *(my call)*: a plain library with the implementations that need only a disk, not a platform: a SQLite metadata store and a file store that keeps one file per media file under a base folder the host passes in. It is separate so the normal test projects can test it (a MAUI project targeting Windows or Android cannot be referenced from them).
- `Noxy.NET.MediaPlayer.App`: **replaces** `Noxy.NET.MediaPlayer.Frontend`. The MAUI Blazor Hybrid app for `net10.0-windows10.0.19041.0` and `net10.0-android`. Holds the Blazor components and pages, the Fluxor state, the sign-in, server-address and library screens, and the platform implementations: secure storage for the refresh token, the audio player, and the Google sign-in flow.

**Metadata and files.** Metadata in one SQLite file, files as `{base}/files/{mediaFileID}`, both in the app's data folder (not a cache folder, which the OS may clear). Cleared app data is handled like the browser dropping storage was: a stored ETag with an empty library sends no `If-None-Match`, and start-up repair marks every media file whose file is missing as not downloaded.

**Hashing and the 100 MB buffer.** The file hash is computed from a stream with the platform's SHA-256, so the file is no longer held in memory for hashing.

**Playback.** Through `IAudioPlayer`, implemented natively in the app project. An `<audio>` element in the webview cannot read files in the app data folder, and native playback is what background playback and media keys need later.

**Refresh token.** In `SecureStorage`, never in preferences. The access token stays in memory.

**Server address.** The app is no longer served by the API, so it needs the API's address. The user enters it on first run; it is kept in preferences behind `IServerAddressStorage`, with a default for development *(my call)*. Every API call and the hub connection use it.

**Google sign-in.** The standard system-browser flow for installed apps: open Google's sign-in in the system browser with a PKCE challenge, receive the authorisation code, exchange it for an ID token, and send that token to `POST /Auth/Google` as today. The redirect is a loopback address on Windows and a custom scheme on Android *(my call; it needs a short spike to confirm on a real device)*. The app never sees the Google password and keeps no Google token.

*Windows, confirmed on a real run (2026-11-16).* The Google Cloud OAuth client is of type **Desktop app**; the consent screen is External, in Testing, with the developer's account as a test user. The app listens with an `HttpListener` on `http://127.0.0.1:<free port>/` (found by briefly binding port 0), opens the authorisation URL (`response_type=code`, `scope=openid email`, `code_challenge` with method `S256`, a random `state`) in the system browser, takes the `code` from the redirect, checks `state`, and exchanges it at `https://oauth2.googleapis.com/token`. Google's token endpoint wants the client secret from a desktop client even with PKCE (Google does not treat a desktop secret as confidential): the app reads it from `google.local.json` next to the exe, which is git-ignored. The returned `id_token` is accepted by `POST /Auth/Google`, the refresh token lands in secure storage, and a restart restores the session silently. A closed browser cannot be seen by a loopback listener, so a cancel ends when the caller cancels or after five minutes. In Testing the consent screen is shown on every sign-in. Android (custom scheme, SHA-1 of the signing key) is still to do.

**Backend change.** `GoogleTokenService` checks the token against the single `Google:ClientId`. A Windows client and an Android client are different OAuth clients with different client IDs, so the setting becomes a list, `Google:ClientIdList`, and a token is accepted when its audience is any of them. An empty list still switches Google off with a `503`. In development there is no deployed config to migrate.

## Changes to the earlier designs

- Sync-player design: "Browser storage", "Where files live in the browser" and the object-URL playback line are replaced by this document's storage, hashing and playback decisions. The replaceable `IMetadataStore` and `IFileStore` seam stays and is what makes the change small.
- Screens design: "Constraints: Blazor WebAssembly" becomes MAUI Blazor Hybrid; everything about layout and screens is unchanged. Google sign-in stays in scope and moves to the front of the sign-in screen's work.
- Foundation plan: Task 1 (wiring and the bundle-size check) becomes the solution layout and the MAUI shell; the IndexedDB store task becomes the SQLite store; the session storage becomes `SecureStorage`.
- Downloads-and-player plan: the IndexedDB file store becomes the disk file store; `SubtleCrypto` and the JS audio module become the native hash and audio implementations; "browser dropped storage" becomes "app data cleared or disk full".

## What the user has to do

- Install the MAUI workload and the Android SDK. This is a machine-level command, so I give it and the user runs it.
- Create the Google OAuth clients in Google Cloud Console for the app: one for Android (package name and the signing key's SHA-1) and one for Windows (desktop app type).

## Left to the builder

Folder and class names inside the three projects, the SQLite library (the backend already uses EF Core with SQLite, which would keep one way of working), the audio library for each platform, and the exact redirect set-up for the Google flow.

## Known gaps

- There is no password reset in the backend. An account made with email and password that loses its password is lost; a Google account is not affected. A reset flow needs a way to send email and is its own design.
- The first-run server address screen is a stop-gap until there is a fixed server to ship against.
