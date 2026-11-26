using Noxy.NET.MediaPlayer.Application.Models.Metadata;
using Noxy.NET.MediaPlayer.Domain.Models;
using Noxy.NET.MediaPlayer.Domain.Responses.Media;
using Noxy.NET.MediaPlayer.Presentation.Enums;
using Noxy.NET.MediaPlayer.Presentation.Exceptions;
using Noxy.NET.MediaPlayer.Presentation.Models;
using Noxy.NET.MediaPlayer.Presentation.Services;
using Noxy.NET.Test.MediaPlayer.Fakes;
using TagLib;

namespace Noxy.NET.Test.MediaPlayer.Presentation;

public class FileTagServiceTest
{
    private static readonly DateTime First = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime Second = new(2026, 2, 1, 0, 0, 0, DateTimeKind.Utc);

    private readonly FakeFileStore _storeFile = new();
    private readonly FakeMetadataStore _storeMetadata = new();
    private readonly FakeMediaTagWritingService _writer = new();
    private readonly FakeMediaTagReadingService _reader = new();
    private readonly FakeLogger<FileTagService> _logger = new();
    private readonly FileTagService _service;
    private int _changedCount;

    public FileTagServiceTest()
    {
        _service = new(_storeFile, _storeMetadata, _storeMetadata, _writer, _reader, _logger);
        _service.Changed += () => _changedCount++;
    }

    private static ResponseMediaFile Media(string title, DateTime modified, List<Guid>? artistIDList = null)
    {
        return new()
        {
            ID = Guid.CreateVersion7(),
            Title = title,
            ContentType = "audio/mpeg",
            SizeBytes = 3,
            TimeCreated = First,
            TimeModified = modified,
            ArtistIDList = artistIDList ?? []
        };
    }

    // A media file in the library with its bytes on the device and no tag time recorded, as right after a download.
    private async Task<ResponseMediaFile> Held(string title = "Media", DateTime? modified = null, List<Guid>? artistIDList = null, DateTime? tagTime = null)
    {
        ResponseMediaFile media = Media(title, modified ?? First, artistIDList);
        await _storeMetadata.SaveMediaFile(media);
        _storeFile.Hold(media.ID, [1, 2, 3]);
        await _storeMetadata.SaveFileState(new(media.ID, FileSyncStateEnum.OnDevice, null, tagTime));

        return media;
    }

    // What a file holds when its tags are the metadata of the media file.
    private static MediaTagSet TagsOf(ResponseMediaFile media, params string[] artistNameList)
    {
        return new(media.Title, [.. artistNameList], null, [], [], null, null, null, null, null, null);
    }

    private static void AssertResult(BulkResult result, int succeededCount, List<Guid> failedIDList, string? message)
    {
        Assert.Equal(succeededCount, result.SucceededCount);
        Assert.Equal(failedIDList, result.FailedIDList);
        Assert.Equal(message, result.Message);
    }

    private async Task<FileState?> State(Guid id)
    {
        return (await _storeMetadata.GetFileStateList()).SingleOrDefault(x => x.MediaFileID == id);
    }

    private async Task<Guid> AddArtist(string name)
    {
        Guid id = Guid.CreateVersion7();
        await _storeMetadata.Artist.Save(new() { ID = id, Name = name, MediaFileCount = 1, TimeModified = First });

        return id;
    }

    #region -- Check --

    [Fact]
    public async Task CheckFiles_WhenTheTagsMatch_RecordsTheTimeModified_WritesNothing_AndTheFileIsNotOutdated()
    {
        ResponseMediaFile media = await Held();
        _reader.Result = TagsOf(media);

        await _service.CheckFiles();

        Assert.Equal(media.TimeModified, (await State(media.ID))?.TagTimeModified);
        Assert.Empty(_writer.WriteList);
        Assert.Empty(await _service.GetOutdatedIDList());
    }

    [Fact]
    public async Task CheckFiles_WhenTheTagsDiffer_LeavesTheStateAlone_AndTheFileIsOutdated()
    {
        ResponseMediaFile media = await Held();
        _reader.Result = TagsOf(Media("Another title", First));

        await _service.CheckFiles();

        Assert.Equal(new(media.ID, FileSyncStateEnum.OnDevice, null), await State(media.ID));
        Assert.Equal([media.ID], await _service.GetOutdatedIDList());
        Assert.Empty(_writer.WriteList);
    }

    [Fact]
    public async Task CheckFiles_WhenTheFileHasNoReadableTags_TheFileIsOutdated()
    {
        ResponseMediaFile media = await Held();
        _reader.Result = MediaTagSet.Empty;

        await _service.CheckFiles();

        Assert.Equal([media.ID], await _service.GetOutdatedIDList());
    }

    [Fact]
    public async Task CheckFiles_WhenTheFileCannotBeOpened_LeavesItUnchecked()
    {
        await Held();
        _reader.Failure = new IOException("The file is locked.");

        await _service.CheckFiles();

        Assert.Empty(await _service.GetOutdatedIDList());
        Assert.Equal(0, _changedCount);
    }

    [Fact]
    public async Task CheckFiles_ADifferingFile_IsNotReadAgain_UntilItsTimeModifiedChanges()
    {
        ResponseMediaFile media = await Held();
        _reader.Result = MediaTagSet.Empty;

        await _service.CheckFiles();
        await _service.CheckFiles();

        Assert.Single(_reader.ReadList);

        ResponseMediaFile edited = new() { ID = media.ID, Title = media.Title, ContentType = media.ContentType, SizeBytes = media.SizeBytes, TimeCreated = media.TimeCreated, TimeModified = Second };
        await _storeMetadata.SaveMediaFile(edited);
        await _service.CheckFiles();

        Assert.Equal(2, _reader.ReadList.Count);
    }

    [Fact]
    public async Task CheckFiles_AfterTheMediaFileIsEdited_ChecksAgain_AndAMatchingEditBumpsTheRecord()
    {
        ResponseMediaFile media = await Held(tagTime: First);
        ResponseMediaFile edited = new() { ID = media.ID, Title = media.Title, ContentType = media.ContentType, SizeBytes = media.SizeBytes, TimeCreated = media.TimeCreated, TimeModified = Second };
        await _storeMetadata.SaveMediaFile(edited);
        _reader.Result = TagsOf(edited);

        await _service.CheckFiles();

        Assert.Equal(Second, (await State(media.ID))?.TagTimeModified);
        Assert.Empty(await _service.GetOutdatedIDList());
    }

    [Fact]
    public async Task CheckFiles_AFileThatIsNotHeld_IsNeverReadOrListed()
    {
        ResponseMediaFile media = Media("Missing", First);
        await _storeMetadata.SaveMediaFile(media);
        await _storeMetadata.SaveFileState(new(media.ID, FileSyncStateEnum.OnDevice, null));
        ResponseMediaFile downloading = Media("Downloading", First);
        await _storeMetadata.SaveMediaFile(downloading);
        _storeFile.Hold(downloading.ID, [1]);
        await _storeMetadata.SaveFileState(new(downloading.ID, FileSyncStateEnum.Downloading, null));

        await _service.CheckFiles();

        Assert.Empty(_reader.ReadList);
        Assert.Empty(await _service.GetOutdatedIDList());
    }

    [Fact]
    public async Task CheckFiles_AFileThatLeftTheDevice_IsReadAgainWhenItComesBack()
    {
        ResponseMediaFile media = await Held();
        _reader.Result = MediaTagSet.Empty;
        await _service.CheckFiles();

        await _storeMetadata.SaveFileState(new(media.ID, FileSyncStateEnum.Downloading, null));
        await _service.CheckFiles();
        await _storeMetadata.SaveFileState(new(media.ID, FileSyncStateEnum.OnDevice, null));
        _reader.Result = TagsOf(media);
        await _service.CheckFiles();

        Assert.Equal(2, _reader.ReadList.Count);
        Assert.Equal(media.TimeModified, (await State(media.ID))?.TagTimeModified);
        Assert.Empty(await _service.GetOutdatedIDList());
    }

    [Fact]
    public async Task GetOutdatedIDList_DoesNotListAFileThatWasNotChecked()
    {
        await Held();

        Assert.Empty(await _service.GetOutdatedIDList());
    }

    [Fact]
    public async Task CheckFiles_RaisesChangedOnlyWhenSomethingChanged()
    {
        await Held();
        _reader.Result = MediaTagSet.Empty;

        await _service.CheckFiles();
        await _service.CheckFiles();

        Assert.Equal(1, _changedCount);
    }

    [Fact]
    public async Task CheckFiles_WhenAnArtistIsNotInTheLocalListYet_SkipsTheFile_AndChecksItOnceTheListHasIt()
    {
        Guid artistID = Guid.CreateVersion7();
        ResponseMediaFile media = await Held(artistIDList: [artistID]);
        _reader.Result = TagsOf(media, "Someone");

        await _service.CheckFiles();

        Assert.Empty(_reader.ReadList);
        Assert.Empty(await _service.GetOutdatedIDList());

        await _storeMetadata.Artist.Save(new() { ID = artistID, Name = "Someone", MediaFileCount = 1, TimeModified = First });
        await _service.CheckFiles();

        Assert.Equal(media.TimeModified, (await State(media.ID))?.TagTimeModified);
    }

    #endregion

    #region -- Write --

    [Fact]
    public async Task Update_WritesTheValuesOfTheMediaFile_AndRecordsItsTimeModified()
    {
        Guid artistID = await AddArtist("Someone");
        ResponseMediaFile media = await Held("Song", artistIDList: [artistID]);

        BulkResult result = await _service.Update([media.ID]);

        AssertResult(result, 1, [], null);
        MediaTagValues values = Assert.Single(_writer.WriteList).Values;
        Assert.Equal("Song", values.Title);
        Assert.Equal(["Someone"], values.ArtistNameList);
        Assert.Equal(media.TimeModified, (await State(media.ID))?.TagTimeModified);
    }

    [Fact]
    public async Task Update_LeavesTheStateOnDevice_AndTheFileHeld()
    {
        ResponseMediaFile media = await Held();

        await _service.Update([media.ID]);

        Assert.Equal(FileSyncStateEnum.OnDevice, (await State(media.ID))?.State);
        Assert.NotNull(await _storeFile.GetPath(media.ID));
    }

    [Fact]
    public async Task Update_ASongNotOnDevice_IsSkipped_AndReported()
    {
        ResponseMediaFile media = Media("Missing", First);
        await _storeMetadata.SaveMediaFile(media);

        BulkResult result = await _service.Update([media.ID]);

        AssertResult(result, 0, [media.ID], PresentationTexts.ActionMessageNotOnDevice);
        Assert.Empty(_writer.WriteList);
    }

    [Fact]
    public async Task Update_WhenTheWriteThrowsIOException_LeavesFileAndState_ThenALaterCallWritesIt()
    {
        ResponseMediaFile media = await Held();
        _writer.Failure = new IOException("The file is locked.");

        BulkResult failed = await _service.Update([media.ID]);

        AssertResult(failed, 0, [media.ID], PresentationTexts.ActionMessageUpdateFileFailed);
        Assert.Equal(new(media.ID, FileSyncStateEnum.OnDevice, null), await State(media.ID));
        Assert.Equal([1, 2, 3], _storeFile.ReadPath(await _storeFile.GetPath(media.ID) ?? string.Empty));

        _writer.Failure = null;
        BulkResult written = await _service.Update([media.ID]);

        Assert.Equal(1, written.SucceededCount);
        Assert.Equal(media.TimeModified, (await State(media.ID))?.TagTimeModified);
    }

    [Fact]
    public async Task Update_WhenTheTaggerCannotReadTheFile_ContinuesWithTheRest_AndReportsTheFailed()
    {
        ResponseMediaFile broken = await Held("Broken");
        ResponseMediaFile fine = await Held("Fine");
        int calls = 0;
        _writer.BeforeWrite = _ =>
        {
            if (calls++ == 0) throw new CorruptFileException("Not audio.");
        };

        BulkResult result = await _service.Update([broken.ID, fine.ID]);

        AssertResult(result, 1, [broken.ID], PresentationTexts.ActionMessageUpdateFileFailed);
        Assert.Null((await State(broken.ID))?.TagTimeModified);
        Assert.Equal(fine.TimeModified, (await State(fine.ID))?.TagTimeModified);
    }

    [Fact]
    public async Task Update_WhenTheMediaFileChangesDuringTheWrite_StaysOutdatedAfterTheNextCheck()
    {
        ResponseMediaFile media = await Held();
        ResponseMediaFile edited = new() { ID = media.ID, Title = "Edited", ContentType = media.ContentType, SizeBytes = media.SizeBytes, TimeCreated = media.TimeCreated, TimeModified = Second };
        _writer.BeforeWrite = _ => _storeMetadata.SaveMediaFile(edited).Wait(TestContext.Current.CancellationToken);

        await _service.Update([media.ID]);
        _writer.BeforeWrite = null;
        _reader.Result = TagsOf(media);
        await _service.CheckFiles();

        Assert.Equal(First, (await State(media.ID))?.TagTimeModified);
        Assert.Equal([media.ID], await _service.GetOutdatedIDList());
    }


    [Fact]
    public async Task Update_WhenTheDiskIsFull_ReportsTheDiskFullText()
    {
        ResponseMediaFile media = await Held();
        _writer.Failure = new StorageFullException();

        BulkResult result = await _service.Update([media.ID]);

        AssertResult(result, 0, [media.ID], PresentationTexts.DownloadDiskFull);
    }

    [Fact]
    public async Task Update_RaisesChanged_OnlyAfterASuccessfulWrite()
    {
        ResponseMediaFile media = await Held();
        _writer.Failure = new IOException("The file is locked.");

        await _service.Update([media.ID]);

        Assert.Equal(0, _changedCount);

        _writer.Failure = null;
        await _service.Update([media.ID]);

        Assert.Equal(1, _changedCount);
    }

    [Fact]
    public async Task Update_WhenAnArtistIsNotInTheLocalListYet_ReportsItAndWritesNothing()
    {
        ResponseMediaFile media = await Held(artistIDList: [Guid.CreateVersion7()]);

        BulkResult result = await _service.Update([media.ID]);

        AssertResult(result, 0, [media.ID], PresentationTexts.ActionMessageMetadataNotLoaded);
        Assert.Empty(_writer.WriteList);
    }

    [Fact]
    public async Task Update_WritesAnArtistNameWithASemicolonAsOneArtist()
    {
        Guid artistID = await AddArtist("Foo; Bar");
        ResponseMediaFile media = await Held(artistIDList: [artistID]);

        await _service.Update([media.ID]);

        Assert.Equal(["Foo; Bar"], Assert.Single(_writer.WriteList).Values.ArtistNameList);
    }

    [Fact]
    public async Task UpdateOutdated_WritesOnlyTheOutdatedFiles()
    {
        ResponseMediaFile outdated = await Held("Outdated");
        ResponseMediaFile matching = await Held("Matching");
        await Held("Unchecked", artistIDList: [Guid.CreateVersion7()]);
        _reader.ResultByPath[await _storeFile.GetPath(outdated.ID) ?? string.Empty] = MediaTagSet.Empty;
        _reader.ResultByPath[await _storeFile.GetPath(matching.ID) ?? string.Empty] = TagsOf(matching);
        await _service.CheckFiles();

        BulkResult result = await _service.UpdateOutdated();

        Assert.Equal(1, result.SucceededCount);
        Assert.Equal("Outdated", Assert.Single(_writer.WriteList).Values.Title);
        Assert.Empty(await _service.GetOutdatedIDList());
    }

    [Fact]
    public async Task Update_AndCheckFiles_AtOnce_DoNotTouchTheSameFileConcurrently()
    {
        ResponseMediaFile media = await Held();
        TaskCompletionSource entered = new(TaskCreationOptions.RunContinuationsAsynchronously);
        TaskCompletionSource release = new(TaskCreationOptions.RunContinuationsAsynchronously);
        _writer.BeforeWrite = _ =>
        {
            entered.TrySetResult();
            release.Task.Wait(TestContext.Current.CancellationToken);
        };

        Task update = _service.Update([media.ID]);
        await entered.Task;
        Task check = _service.CheckFiles();
        await Task.Delay(100, TestContext.Current.CancellationToken);

        Assert.False(check.IsCompleted);
        Assert.Empty(_reader.ReadList);

        release.SetResult();
        await Task.WhenAll(update, check);
    }

    #endregion
}
