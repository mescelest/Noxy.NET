# Frontend as a MAUI Blazor Hybrid app (Windows and Android)

## Intent

The app keeps a library of up to 5000 media files on the device, plays them offline, and feels like a normal desktop and phone app. A MAUI app on Windows and Android stores plain files on disk and can use the platform's own audio, secure storage, media keys and system browser.

## Goals

- One app for **Windows and Android**, with the screens written once as Blazor components.
- `Noxy.NET.MediaPlayer.Presentation` holds only what any frontend could use (a MAUI host, a console tool): API clients, session, sync, download, player logic, models, and the **contracts** for everything platform-specific.
- The metadata and the audio files live on disk and survive restarts.
- Google sign-in works on both platforms. There is no password reset, so Google is the main way in; email and password remains.

## Non-goals

- iOS and macOS (they need a Mac to build). Nothing here prevents adding them later.
- A web version.
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
- `Noxy.NET.MediaPlayer.Presentation`: a plain class library (`Microsoft.NET.Sdk`, no Blazor, no browser package). Contains the services and the contracts: `IApiClient`, `ISessionService`, `ISyncService`, `IDownloadService`, `IPlayerService`, `IMetadataStore`, `IFileStore`, `ISessionStorage`, `IAudioPlayer`, `IGoogleSignInService`, `IServerAddressStorage`, and the models.
- `Noxy.NET.MediaPlayer.Storage` *(my call)*: a plain library with the implementations that need only a disk, not a platform: a SQLite metadata store and a file store that keeps one file per media file under a base folder the host passes in. It is separate so the normal test projects can test it (a MAUI project targeting Windows or Android cannot be referenced from them).
- `Noxy.NET.MediaPlayer.App`: the MAUI Blazor Hybrid app for `net10.0-windows10.0.19041.0` and `net10.0-android`. Holds the Blazor components and pages, the Fluxor state, the sign-in, server-address and library screens, and the platform implementations: secure storage for the refresh token, the audio player, and the Google sign-in flow.

**Metadata and files.** Metadata in one SQLite file, files as `{base}/files/{mediaFileID}`, both in the app's data folder (not a cache folder, which the OS may clear). Cleared app data is handled by sync and repair: a stored ETag with an empty library sends no `If-None-Match`, and start-up repair marks every media file whose file is missing as not downloaded.

**Playback.** Through `IAudioPlayer`, implemented natively in the app project. An `<audio>` element in the webview cannot read files in the app data folder, and native playback is what background playback and media keys need later.

**Refresh token.** In `SecureStorage`, never in preferences. The access token stays in memory.

**Server address.** The app is not served by the API, so it needs the API's address. The user enters it on first run; it is kept in preferences behind `IServerAddressStorage`, with a default for development *(my call)*. Every API call and the hub connection use it.

**Google sign-in.** The standard system-browser flow for installed apps: open Google's sign-in in the system browser with a PKCE challenge, receive the authorisation code, exchange it for an ID token, and send that token to `POST /Auth/Google`. The redirect is a loopback address on Windows and a custom scheme on Android *(my call; it needs a short spike to confirm on a real device)*. The app never sees the Google password and keeps no Google token.

*Windows.* The Google Cloud OAuth client is of type **Desktop app**; the consent screen is External, in Testing, with the developer's account as a test user. The app listens with an `HttpListener` on `http://127.0.0.1:<free port>/` (found by briefly binding port 0), opens the authorisation URL (`response_type=code`, `scope=openid email`, `code_challenge` with method `S256`, a random `state`) in the system browser, takes the `code` from the redirect, checks `state`, and exchanges it at `https://oauth2.googleapis.com/token`. Google's token endpoint wants the client secret from a desktop client even with PKCE (Google does not treat a desktop secret as confidential): the app reads it from `google.local.json` next to the exe, which is git-ignored. The returned `id_token` is accepted by `POST /Auth/Google`, the refresh token lands in secure storage, and a restart restores the session silently. A closed browser cannot be seen by a loopback listener, so a cancel ends when the caller cancels or after five minutes. In Testing the consent screen is shown on every sign-in. On Android the redirect is a custom scheme and the OAuth client uses the SHA-1 of the signing key.

**Backend change.** `GoogleTokenService` checks the token against the single `Google:ClientId`. A Windows client and an Android client are different OAuth clients with different client IDs, so the setting is a list, `Google:ClientIdList`, and a token is accepted when its audience is any of them. An empty list still switches Google off with a `503`.

## What the user has to do

- Install the MAUI workload and the Android SDK. This is a machine-level command, so I give it and the user runs it.
- Create the Google OAuth clients in Google Cloud Console for the app: one for Android (package name and the signing key's SHA-1) and one for Windows (desktop app type).

## Left to the builder

Folder and class names inside the three projects, the SQLite library (the backend already uses EF Core with SQLite, which would keep one way of working), the audio library for each platform, and the exact redirect set-up for the Google flow.

## Known gaps

- There is no password reset in the backend. An account made with email and password that loses its password is lost; a Google account is not affected. A reset flow needs a way to send email and is its own design.
- The first-run server address screen is a stop-gap until there is a fixed server to ship against.
