# Authentication: design

Lines marked *(inferred)* are reasons read out of the code, so check them.

## Intent

A person has one account, made with an email and password or with a Google account. Signing in gives the app a short-lived access token for the API and the hub, and a long-lived refresh token that the app swaps for a new pair without asking the person again. A stolen refresh token must stop being useful as soon as the real owner uses theirs, and nobody must be able to take over an account by registering an email address first.

## Goals

- **Sign up and sign in with email and password.** `POST /Auth/SignUp` and `POST /Auth/SignIn` return `ResponseAuthentication` (`AccessToken`, `RefreshToken`).
- **Sign in with Google.** `POST /Auth/Google` takes the Google ID token, checks it against Google's published keys, issuer, expiry and one of this app's client IDs (`Google:ClientIdList`, one per app that signs in; empty switches Google off with a `503`), and then signs in, creates the account, or links to an existing one.
- **Refresh.** `POST /Auth/Refresh` swaps a refresh token for a new access token and a new refresh token. Refresh tokens are single use.
- **Short access tokens, long refresh tokens.** The access token is a JWT (signed with HMAC SHA-256) valid for `Authentication:AccessTokenMinutes` (15); the refresh token lives `RefreshTokenDays` (30).
- **The same token works for the API and the hub.** The hub takes the access token as the `access_token` query parameter, for paths under `/hubs` only.
- **Everything else needs a signed-in user.** The auth endpoints are the only anonymous ones (plus the OpenAPI document, in development only). They are rate limited per client address (`RateLimiting:Auth`, 10 per 60 s) with a `Retry-After`.

## Non-goals

- Email verification, password reset, changing the email or password, two-factor authentication, sign-out of other devices.
- Roles and permissions: every signed-in user owns only their own data.
- A cookie flow for the refresh token (the client keeps it itself).
- Other identity providers than Google.

## Anti-goals (a failure even if it technically works)

- Anyone learning which emails have accounts: an unknown email and a wrong password give the same `401` and take the same time.
- A password, ID token or refresh token in a log, an exception message or the printed form of a command.
- A refresh token that can be used twice without the account noticing, or a token store that holds usable tokens (only hashes are stored).
- An account taken over because someone registered an address they do not own before its owner arrived with Google.
- A user changing who they are by changing their email at Google: Google's account ID, not the email, is the identity.

## Constraints

- Repository conventions and the command shape in `2026-11-15-backend-cqrs-design.md`; the four use cases share one base class for the token response and the email normalisation.
- Passwords are 16 to 128 characters when signing up. Sign-in takes up to 128 so the check cannot be used to feed the hasher huge input.
- Emails are trimmed and lower-cased everywhere they are stored or looked up.

## Approaches considered

**Password hashing.** PBKDF2 with SHA-512, 210,000 iterations, a 16-byte salt and a 32-byte hash, stored as one string with its own algorithm name and iteration count, so the cost can rise later and old hashes still verify. *(inferred: the framework's own primitive, no extra package.)* Verification compares in fixed time. When there is no user, a dummy hash is still computed, so the time does not reveal which emails exist.

**Refresh token storage.** A random 32-byte value, sent to the client once; only its SHA-256 is stored, with the user, the expiry and a revoked time. A database leak therefore leaks no usable token.

**Single use and reuse detection.** A refresh marks the used token revoked and issues a new one. A revoked token coming back means two parties hold it, so every refresh token of that user is revoked and they must sign in again. Losing the race to another request that is using the same token counts as reuse for the same reason. An expired token fails without revoking anything.

**Linking Google to an existing account.** Only when both sides have proven the address: Google must say the email is verified, and the existing account must itself be verified (an account made with an email and password is not, since anyone can register any address). If the existing account is unverified the request is a `409`. *(inferred from the comments in the handler.)*

## Design

### Data

- **User**: ID, `Email` (unique, normalised), `PasswordHash` (null for an account made with Google), `IsEmailVerified` (false for sign up, true when made by Google), and its external logins.
- **ExternalLogin**: user, provider (Google), provider key (Google's account ID); unique per provider and key.
- **RefreshToken**: user, `TokenHash` (indexed), expiry, revoked time.

### Flows

- **Sign up:** normalise the email, `409` if it exists, hash the password, store the user, issue a token pair, one commit.
- **Sign in:** look the user up, verify the password even when there is no user, `401` on either failure, issue a token pair, one commit.
- **Google:** validate the ID token. If a login for that Google account ID exists, sign that user in (`401` if the user is gone). Otherwise, if the email is not verified by Google: `401`. If no account has the email: create a verified user with the Google login. If one does and it is verified: add the Google login to it. If it is not verified: `409`.
- **Refresh:** look the token up by its hash (`401` if unknown); a revoked token revokes all of the user's tokens and is `401`; an expired token is `401`; the used token is revoked atomically (losing that race revokes all and is `401`); the user must still exist (`401`); issue a new pair, one commit.
- **Access token claims:** `sub` (user ID, which every ownership check reads), `email`, `jti`. Validation checks issuer, audience, lifetime and signature, with 30 seconds of clock skew.

### Errors

Problem details with the status: `401` invalid credentials or token (one message for every cause), `409` email already in use, `429` rate limit (with `Retry-After`), `503` Google not configured, `400` for request validation.

### Cleanup

A background service deletes refresh tokens that expired more than `Cleanup:RetentionDays` (7) ago, once at start-up and then every `Cleanup:IntervalHours` (24). A failed run is logged and tried again at the next tick.

### Tests

Unit tests with the fakes for each handler (normalisation, hashed storage, one token per sign in, every Google branch, every refresh branch including reuse, expiry, the lost race and a user that no longer exists), the password hasher and the token service. Integration tests against SQLite and end to end through HTTP for the full flows, the `401`/`409`/`429` cases, and the cleanup deleting only the expired.

## Left to the builder

The password hasher's parameters can be tuned; the refresh-token hash and the token layout can change as long as the rules above hold.
