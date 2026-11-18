using System.Text.Json;
using Noxy.NET.MediaPlayer.Domain.Enums;
using Noxy.NET.MediaPlayer.Domain.Responses.Media;
using Noxy.NET.MediaPlayer.Domain.Responses.Metadata;
using Noxy.NET.MediaPlayer.Domain.Responses.Playlist;
using Noxy.NET.MediaPlayer.Presentation.Enums;
using Noxy.NET.MediaPlayer.Presentation.Services;
using Noxy.NET.Test.MediaPlayer.Fakes;
using Noxy.NET.Test.MediaPlayer.Fixtures;

namespace Noxy.NET.Test.MediaPlayer.Presentation;

public class LocalDataOwnerServiceTest
{
    private readonly FakeSessionService _session = new();
    private readonly FakeMetadataStore _storeMetadata = new();
    private readonly FakeFileStore _storeFile = new();
    private readonly FakeDownloadService _download = new();
    private readonly FakePlayerService _player = new();
    private readonly LocalDataOwnerService _service;
    private readonly Guid _oldUser = Guid.NewGuid();
    private readonly Guid _newUser = Guid.NewGuid();

    public LocalDataOwnerServiceTest()
    {
        _service = new(_session, _storeMetadata, _storeMetadata, _storeMetadata.Setting, _storeFile, _download, _player);
    }

    private void SignInAs(Guid userID)
    {
        _session.AccessToken = AccessTokenFactory.Create(DateTime.UtcNow.AddHours(1), userID.ToString());
    }

    // What the old account left on the device: a library with a file held and one half downloaded, a playlist with its detail, every ETag, a file state, a setting, an artist and the queue.
    private async Task<ResponseMediaFile> LeaveAnAccountsData()
    {
        ResponseMediaFile media = MediaFileFactory.Create("Old");
        await _storeMetadata.SaveMediaFile(media);
        _storeFile.Hold(media.ID, [1, 2, 3]);
        Guid partialID = Guid.NewGuid();
        await _storeFile.AppendPartial(partialID, new MemoryStream([1, 2]));
        ResponsePlaylistDetail detail = new() { ID = Guid.NewGuid(), Name = "Old list", AllowDuplicates = true, EntryCount = 0, TimeCreated = DateTime.UtcNow, TimeModified = DateTime.UtcNow, TimeLastPlayed = null, EntryList = [] };
        await _storeMetadata.SavePlaylist(detail);
        await _storeMetadata.SavePlaylistDetail(detail);
        await _storeMetadata.SetLibraryETag("\"lib\"");
        await _storeMetadata.SetPlaylistListETag("\"pl\"");
        await _storeMetadata.SaveFileState(new(media.ID, FileSyncStateEnum.OnDevice, new string('a', 64), null));
        await _storeMetadata.Setting.ReplaceList([new() { Key = "Shuffle", ValueType = SettingValueTypeEnum.Enum, Value = JsonSerializer.SerializeToElement("Random"), TimeModified = DateTime.UtcNow }]);
        await _storeMetadata.Setting.SetETag("\"set\"");
        await _storeMetadata.Artist.ReplaceList([new ResponseArtist { ID = Guid.NewGuid(), Name = "Old artist", MediaFileCount = 1, TimeModified = DateTime.UtcNow }]);
        await _storeMetadata.Artist.SetETag("\"art\"");
        await _storeMetadata.SetOwnerUserID(_oldUser);

        return media;
    }

    private async Task AssertEverythingOfTheOldAccountIsGone()
    {
        Assert.Empty(await _storeMetadata.GetMediaFileList());
        Assert.Empty(await _storeMetadata.GetPlaylistList());
        Assert.Empty(await _storeMetadata.GetHeldPlaylistIDList());
        Assert.Empty(await _storeMetadata.GetFileStateList());
        Assert.Null(await _storeMetadata.GetLibraryETag());
        Assert.Null(await _storeMetadata.GetPlaylistListETag());
        Assert.Empty(await _storeMetadata.Setting.GetList());
        Assert.Null(await _storeMetadata.Setting.GetETag());
        Assert.Empty(await _storeMetadata.Artist.GetList());
        Assert.Null(await _storeMetadata.Artist.GetETag());
        Assert.Empty(await _storeFile.GetHeldIDList());
        Assert.Empty(await _storeFile.GetPartialIDList());
    }

    [Fact]
    public async Task EnsureOwner_WithNoStoredOwner_RecordsTheUser_AndKeepsTheData()
    {
        ResponseMediaFile media = MediaFileFactory.Create("Kept");
        await _storeMetadata.SaveMediaFile(media);
        _storeFile.Hold(media.ID, [1]);
        SignInAs(_newUser);

        await _service.EnsureOwner();

        Assert.Equal(_newUser, await _storeMetadata.GetOwnerUserID());
        Assert.Single(await _storeMetadata.GetMediaFileList());
        Assert.NotNull(await _storeFile.GetPath(media.ID));
        Assert.Empty(_player.CallList);
    }

    [Fact]
    public async Task EnsureOwner_ForTheSameUser_ChangesNothing()
    {
        ResponseMediaFile media = await LeaveAnAccountsData();
        SignInAs(_oldUser);

        await _service.EnsureOwner();

        Assert.Equal(_oldUser, await _storeMetadata.GetOwnerUserID());
        Assert.Single(await _storeMetadata.GetMediaFileList());
        Assert.NotNull(await _storeFile.GetPath(media.ID));
        Assert.Equal("\"lib\"", await _storeMetadata.GetLibraryETag());
        Assert.Empty(_player.CallList);
        Assert.Empty(_download.CancelList);
    }

    [Fact]
    public async Task IsVerified_IsFalseUntilTheOwnerCheckFinished_ThenTrue_AndChangedIsRaisedOnce()
    {
        await LeaveAnAccountsData();
        SignInAs(_newUser);
        int changed = 0;
        bool verifiedWhenWiped = true;
        _service.Wiped += () => verifiedWhenWiped = _service.IsVerified;
        _service.Changed += () => changed++;
        Assert.False(_service.IsVerified);

        await _service.EnsureOwner();

        Assert.False(verifiedWhenWiped);
        Assert.True(_service.IsVerified);
        Assert.Equal(1, changed);
    }

    [Fact]
    public async Task IsVerified_IsTrueForTheSameOwner_ForNoStoredOwner_AndForATokenItCannotRead()
    {
        await LeaveAnAccountsData();
        SignInAs(_oldUser);
        await _service.EnsureOwner();
        Assert.True(_service.IsVerified);

        LocalDataOwnerService fresh = new(_session, _storeMetadata, _storeMetadata, _storeMetadata.Setting, _storeFile, _download, _player);
        _session.AccessToken = "garbage";
        await fresh.EnsureOwner();
        Assert.True(fresh.IsVerified);

        FakeMetadataStore empty = new();
        LocalDataOwnerService noOwner = new(_session, empty, empty, empty.Setting, _storeFile, _download, _player);
        SignInAs(_newUser);
        await noOwner.EnsureOwner();
        Assert.True(noOwner.IsVerified);
    }

    [Fact]
    public async Task IsVerified_ReturnsToFalse_WhenTheSessionEnds_AndChangedIsRaised()
    {
        await LeaveAnAccountsData();
        SignInAs(_oldUser);
        await _service.EnsureOwner();
        int changed = 0;
        _service.Changed += () => changed++;

        _session.AccessToken = null;
        _session.RaiseChanged();

        Assert.False(_service.IsVerified);
        Assert.Equal(1, changed);
    }

    [Fact]
    public async Task EnsureOwner_ForADifferentUser_RaisesWiped_AfterTheStoresAreEmpty()
    {
        await LeaveAnAccountsData();
        SignInAs(_newUser);
        int count = 0;
        int mediaCountWhenRaised = -1;
        _service.Wiped += () =>
        {
            count++;
            mediaCountWhenRaised = _storeMetadata.GetMediaFileList().Result.Count;
        };

        await _service.EnsureOwner();

        Assert.Equal(1, count);
        Assert.Equal(0, mediaCountWhenRaised);
    }

    [Fact]
    public async Task EnsureOwner_ForTheSameUser_DoesNotRaiseWiped()
    {
        await LeaveAnAccountsData();
        SignInAs(_oldUser);
        int count = 0;
        _service.Wiped += () => count++;

        await _service.EnsureOwner();

        Assert.Equal(0, count);
    }

    [Fact]
    public async Task EnsureOwner_ForADifferentUser_WipesLibraryPlaylistsETagsFileStatesAndSettings_AndRecordsTheNewOwner()
    {
        await LeaveAnAccountsData();
        SignInAs(_newUser);

        await _service.EnsureOwner();

        await AssertEverythingOfTheOldAccountIsGone();
        Assert.Equal(_newUser, await _storeMetadata.GetOwnerUserID());
    }

    [Fact]
    public async Task EnsureOwner_ForADifferentUser_DeletesHeldAndPartialFiles()
    {
        ResponseMediaFile media = await LeaveAnAccountsData();
        Assert.NotEmpty(await _storeFile.GetPartialIDList());
        SignInAs(_newUser);

        await _service.EnsureOwner();

        Assert.Null(await _storeFile.GetPath(media.ID));
        Assert.Empty(await _storeFile.GetPartialIDList());
    }

    [Fact]
    public async Task EnsureOwner_ForADifferentUser_StopsThePlayerAndCancelsDownloads()
    {
        ResponseMediaFile media = await LeaveAnAccountsData();
        SignInAs(_newUser);

        await _service.EnsureOwner();

        Assert.Equal(["Clear"], _player.CallList);
        Assert.Equal([media.ID], _download.CancelList);
    }

    [Fact]
    public async Task EnsureOwner_WhenSignedOut_DoesNothing()
    {
        ResponseMediaFile media = await LeaveAnAccountsData();
        _session.AccessToken = null;

        await _service.EnsureOwner();

        Assert.Equal(_oldUser, await _storeMetadata.GetOwnerUserID());
        Assert.Single(await _storeMetadata.GetMediaFileList());
        Assert.NotNull(await _storeFile.GetPath(media.ID));
        Assert.Empty(_player.CallList);
    }

    [Theory]
    [InlineData("garbage")]
    [InlineData("a.b.c")]
    public async Task EnsureOwner_WithATokenItCannotRead_NeverWipes(string token)
    {
        ResponseMediaFile media = await LeaveAnAccountsData();
        _session.AccessToken = token;

        await _service.EnsureOwner();

        Assert.Equal(_oldUser, await _storeMetadata.GetOwnerUserID());
        Assert.Single(await _storeMetadata.GetMediaFileList());
        Assert.NotNull(await _storeFile.GetPath(media.ID));
    }

    [Fact]
    public async Task EnsureOwner_WithASubjectThatIsNotAUserID_NeverWipes()
    {
        await LeaveAnAccountsData();
        _session.AccessToken = AccessTokenFactory.Create(DateTime.UtcNow.AddHours(1), "not-a-guid");

        await _service.EnsureOwner();

        Assert.Equal(_oldUser, await _storeMetadata.GetOwnerUserID());
        Assert.Single(await _storeMetadata.GetMediaFileList());
    }

    [Fact]
    public async Task AfterTheWipe_TheSameNewUserAgain_ChangesNothing()
    {
        await LeaveAnAccountsData();
        SignInAs(_newUser);
        await _service.EnsureOwner();
        _player.CallList.Clear();

        await _service.EnsureOwner();

        Assert.Empty(_player.CallList);
        Assert.Equal(_newUser, await _storeMetadata.GetOwnerUserID());
    }
}
