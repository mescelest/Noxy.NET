# Album art in the local files Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** The device's tag write embeds the album picture as the file's cover (replacing the old one), and a file whose cover differs from the album picture counts as out of date.

**Architecture:** The writer takes an optional picture; the comparison takes one too; `FileTagService` fetches the album picture through `IArtService` and passes it to both; the server moves the `TimeModified` of an album's files when the picture changes, so devices recheck without any new recorded state.

**Tech Stack:** .NET 10, TagLibSharp 2.3.0, Mediator, xUnit v3 (`dotnet run` in the test project, filter `-class <full name>`; build the integration project with `dotnet build` after the Application project when the API is running: `-p:BuildProjectReferences=false`).

**Spec:** `docs/specs/mediaplayer/2026-11-26-album-art-in-files-design.md`

## Global Constraints

- Never commit (project memory): `git add -N` new files.
- A given picture replaces every picture of the file; no picture given leaves the file's pictures alone.
- Byte-for-byte comparison of the cover; an album with no picture is never compared or flagged.
- One class per file, no magic strings.

## Review Focus

- A file written with the cover compares equal at the next check (no flapping) (Task 2 and 4).
- The picture cannot be had (offline): the file stays unchecked, not flagged (Task 4).
- An album with no picture: files neither flagged nor changed (Task 4).
- A file with no embedded cover and an album picture: out of date (Task 2).
- A large album: one fetch per album, not per file (Task 4).

---

### Task 1: The writer embeds a picture

**Files:**
- Modify: `Noxy.NET.MediaPlayer.Application/Interfaces/Services/IMediaTagWritingService.cs`, `Noxy.NET.MediaPlayer.Application/Services/MediaTagWritingService.cs`, `Noxy.NET.Test.UnitTest/MediaPlayer/Fakes/FakeMediaTagWritingService.cs` (record the picture)
- Test: `Noxy.NET.Test.IntegrationTest/MediaPlayer/Media/MediaTagWritingServiceTest.cs`

**Interfaces:**
- Produces: `void Write(string filePath, MediaTagValues values, byte[]? picture = null)`.

- [ ] **Step 1: Write the failing tests:** `Write_APicture_ReplacesExistingPictures_AndReadsBackAsTheSameBytes` (seed the file with two pictures via TagLib, write with a different JPEG byte array, expect one picture, type front cover, bytes equal); `Write_NoPicture_LeavesThePicturesAlone`.
- [ ] **Step 2: Run:** `cd Noxy.NET.Test/Noxy.NET.Test.IntegrationTest && dotnet run -- -class Noxy.NET.Test.MediaPlayer.Media.MediaTagWritingServiceTest`. Expected: build errors, then failures.
- [ ] **Step 3: Implement:** when `picture` is not null set `tag.Pictures = [new Picture(new ByteVector(picture)) { Type = PictureType.FrontCover, MimeType = "image/jpeg" }]`. Update the interface summary and the fake.
- [ ] **Step 4: Run the class and the unit suite.** Expected: pass.
- [ ] **Step 5:** `git add -N` new files.

### Task 2: The comparison includes the cover

**Files:**
- Modify: `Noxy.NET.MediaPlayer.Application/Extensions/MediaTagSetExtension.cs`
- Test: `Noxy.NET.Test.UnitTest/MediaPlayer/Media/MediaTagSetMatchesTest.cs`

**Interfaces:**
- Produces: `bool MediaTagSet.Matches(MediaTagValues values, byte[]? picture)`; the one-argument `Matches(values)` stays and means no picture compared.

- [ ] **Step 1: Write the failing tests:** `Matches_WithAPicture_RequiresTheSameCoverBytes`, `Matches_WithAPictureAndAFileWithNoCover_IsFalse`, `Matches_WithNoPicture_IgnoresTheCover`.
- [ ] **Step 2: Run** the class. Expected: build error, then failures.
- [ ] **Step 3: Implement** the overload with `SequenceEqual` on the bytes; the one-argument method calls it with null.
- [ ] **Step 4: Run** the class. Expected: pass.
- [ ] **Step 5:** `git add -N`.

### Task 3: The device can give the album picture as bytes

**Files:**
- Modify: `Noxy.NET.MediaPlayer.Presentation/Interfaces/Services/IArtService.cs`, `Presentation/Services/ArtService.cs`, `Noxy.NET.Test.UnitTest/MediaPlayer/Fakes/FakeArtService.cs` (create if absent; find how tests fake it with `grep -rn "IArtService" Noxy.NET.Test`)
- Test: the existing `ArtServiceTest`

**Interfaces:**
- Produces: `Task<byte[]?> IArtService.GetAlbumArt(Guid albumID)`; the same null cases as `GetAlbumArtUri`.

- [ ] **Step 1: Write the failing tests:** `GetAlbumArt_ReturnsTheBytesOfThePicture`, `GetAlbumArt_ForAnAlbumWithNoPicture_IsNull`, `GetAlbumArt_WhenTheServerFails_IsNull`.
- [ ] **Step 2: Run** the class. Expected: failures.
- [ ] **Step 3: Implement** by decoding the data address from `GetAlbumArtUri` (strip the `data:image/jpeg;base64,` prefix, `Convert.FromBase64String`).
- [ ] **Step 4: Run** the class and the unit suite. Expected: pass.
- [ ] **Step 5:** `git add -N`.

### Task 4: `FileTagService` checks and writes the cover

**Files:**
- Modify: `Presentation/Services/FileTagService.cs` (constructor gains `IArtService`), the places that construct it (`Noxy.NET.Test.UnitTest/.../FileTagServiceTest.cs`, `Noxy.NET.Test.IntegrationTest/MediaPlayer/Fixtures/FrontendClient.cs`, the DI registration needs no change), `Fakes` for `IArtService` as needed
- Test: `Noxy.NET.Test.UnitTest/MediaPlayer/Presentation/FileTagServiceTest.cs`

**Interfaces:**
- Consumes: Tasks 1 to 3.

- [ ] **Step 1: Write the failing tests:** `CheckFiles_AFileWithAnotherCover_IsOutOfDate`; `CheckFiles_AFileWithTheAlbumCover_MatchesAndRecordsTheTime` (no flapping); `CheckFiles_AnAlbumWithNoPicture_IsNotFlaggedForItsCover`; `CheckFiles_WhenThePictureCannotBeHad_LeavesTheFileUnchecked` (the fake returns null for an album that has an art time but fails: model it with a fake that throws or returns null while the album list says the album has art); `Update_PassesTheAlbumPictureToTheWriter`; `Update_FetchesAnAlbumsPictureOncePerCall` (three files, one album, one fetch).
- [ ] **Step 2: Run** the class. Expected: failures.
- [ ] **Step 3: Implement:** in `Check` and `Update`, ask `IArtService.GetAlbumArt(media.AlbumID)` once per album per call (a local dictionary), compare with `tags.Matches(values, picture)`, pass `picture` to `Write`. When the album has an art time in the metadata list but the picture comes back null, skip the file this round (it stays unchecked); when the album has no art time, pass null.
- [ ] **Step 4: Run** the class, then the unit and integration suites. Expected: pass (the integration suite has one known unrelated failure, `FrontendDownloadFlowTest.AfterTheFilesAreDeletedFromTheDisk_...`).
- [ ] **Step 5:** `git add -N`.

### Task 5: The server moves the album's files when its picture changes

**Files:**
- Modify: `Noxy.NET.MediaPlayer.Application/Services/AlbumArtService.cs` (`Store` and `Inherit`, where the album is committed), possibly `Application/Interfaces/Repositories/IMetadataRepository.cs` (use the existing `GetMediaFileListByAlbum` and `uow.Media.UpdateMediaFile`)
- Test: `Noxy.NET.Test.UnitTest/MediaPlayer/Art/AlbumArtServiceTest.cs`

**Interfaces:**
- Produces: after a picture is stored, replaced or inherited, every media file of the album has `TimeModified` set to the same instant as the album's art time and a `NotificationMediaUpdated` is published for it.

- [ ] **Step 1: Write the failing tests:** `Apply_TouchesAndPushesTheFilesOfTheAlbum`, `Replace_TouchesAndPushesTheFilesOfTheAlbum`, `Inherit_TouchesAndPushesTheFilesOfTheTargetAlbum`, `Apply_ToAnAlbumThatKeepsItsPicture_TouchesNothing`.
- [ ] **Step 2: Run** the class. Expected: failures.
- [ ] **Step 3: Implement:** load the album's files (links loaded, so the response can be built) in the same unit of work, set `TimeModified = now`, `uow.Media.UpdateMediaFile`, commit once, then publish `NotificationMediaUpdated` per file after the album notification (find the type with `grep -rn "NotificationMediaUpdated" Noxy.NET.MediaPlayer.Application`).
- [ ] **Step 4: Run** the class and the suites. Expected: pass.
- [ ] **Step 5:** `git add -N`.

### Task 6: Verification

- [ ] **Step 1:** Run both test projects in full. Expected: only the known unrelated integration failure.
- [ ] **Step 2:** Hand the user the by-hand list from the spec; remind them most files will show as out of date at first and the "write metadata to files" setting stays off until they want the batch.
