using System.Text.Json;
using Microsoft.Data.Sqlite;
using Noxy.NET.MediaPlayer.Domain.Responses.Media;
using Noxy.NET.MediaPlayer.Domain.Responses.Playlist;
using Noxy.NET.MediaPlayer.Presentation.Enums;
using Noxy.NET.MediaPlayer.Presentation.Interfaces.Stores;
using Noxy.NET.MediaPlayer.Presentation.Models;
using Noxy.NET.MediaPlayer.Storage.Stores;
using Noxy.NET.Test.MediaPlayer.Fixtures;

namespace Noxy.NET.Test.MediaPlayer.Presentation;

public class SqliteMetadataStoreTest : MetadataStoreContract, IDisposable
{
    // A folder that does not exist yet, so the store has to create it.
    private readonly string _folder = Path.Combine(Path.GetTempPath(), $"metadata-store-{Guid.CreateVersion7()}");

    private string DatabasePath => Path.Combine(_folder, "metadata.db");

    public void Dispose()
    {
        SqliteConnection.ClearAllPools();
        if (Directory.Exists(_folder)) Directory.Delete(_folder, true);
    }

    protected override IMetadataStore CreateStore()
    {
        return new SqliteMetadataStore(DatabasePath);
    }

    private static ResponseMediaFile MediaFile(string title)
    {
        return new()
        {
            ID = Guid.CreateVersion7(),
            Title = title,
            ContentType = "audio/mpeg",
            SizeBytes = 1,
            TimeCreated = DateTime.UtcNow,
            TimeModified = DateTime.UtcNow
        };
    }

    private static ResponsePlaylist Playlist(string name)
    {
        return new()
        {
            ID = Guid.CreateVersion7(),
            Name = name,
            AllowDuplicates = true,
            EntryCount = 0,
            TimeCreated = DateTime.UtcNow,
            TimeModified = DateTime.UtcNow,
            TimeLastPlayed = null
        };
    }

    [Fact]
    public async Task ASecondStoreOnTheSameFile_SeesWhatTheFirstWrote_AsAfterARestart()
    {
        ResponseMediaFile file = MediaFile("Kept");
        ResponsePlaylist playlist = Playlist("Kept");
        IMetadataStore first = CreateStore();
        await first.ReplaceMediaFileList([file]);
        await first.ReplacePlaylistList([playlist]);
        await first.SetLibraryETag("\"etag\"");

        SqliteConnection.ClearAllPools();
        IMetadataStore second = CreateStore();

        Assert.Equal([file.ID], (await second.GetMediaFileList()).Select(x => x.ID));
        Assert.Equal([playlist.ID], (await second.GetPlaylistList()).Select(x => x.ID));
        Assert.Equal("\"etag\"", await second.GetLibraryETag());
    }

    [Fact]
    public async Task Seed_AnOldFileStateRowWithAVerifiedHash_StillLoads()
    {
        IMetadataStore store = CreateStore();
        await store.GetFileStateList();
        Guid id = Guid.CreateVersion7();
        string json = JsonSerializer.Serialize(new FileState(id, FileSyncStateEnum.OnDevice, null), JsonSerializerOptions.Web);
        string oldJson = json[..^1] + ",\"verifiedHash\":\"abc123\"}";
        Assert.Contains("verifiedHash", oldJson);

        await using (SqliteConnection connection = new($"Data Source={DatabasePath}"))
        {
            await connection.OpenAsync(TestContext.Current.CancellationToken);
            await using SqliteCommand command = connection.CreateCommand();
            command.CommandText = "INSERT INTO FileState (ID, Json) VALUES ($id, $json)";
            command.Parameters.AddWithValue("$id", id.ToString());
            command.Parameters.AddWithValue("$json", oldJson);
            await command.ExecuteNonQueryAsync(TestContext.Current.CancellationToken);
        }

        SqliteConnection.ClearAllPools();

        Assert.Equal(new(id, FileSyncStateEnum.OnDevice, null), Assert.Single(await store.GetFileStateList()));
    }

    [Fact]
    public async Task ReplaceMediaFileList_IsOneTransaction_AFailureHalfWayLeavesTheOldList()
    {
        IMetadataStore store = CreateStore();
        ResponseMediaFile old = MediaFile("Old");
        await store.ReplaceMediaFileList([old]);
        ResponseMediaFile twice = MediaFile("Twice");

        // The same ID twice is rejected by the table on the second insert, after the first one went in.
        await Assert.ThrowsAnyAsync<Exception>(() => store.ReplaceMediaFileList([MediaFile("Fine"), twice, twice]));

        Assert.Equal([old.ID], (await store.GetMediaFileList()).Select(x => x.ID));
    }

    [Fact]
    public async Task ReplacePlaylistList_IsOneTransaction_AFailureHalfWayLeavesTheOldListAndItsDetails()
    {
        IMetadataStore store = CreateStore();
        ResponsePlaylist old = Playlist("Old");
        await store.ReplacePlaylistList([old]);
        await store.SavePlaylistDetail(new() { ID = old.ID, Name = "Old", AllowDuplicates = true, EntryCount = 0, TimeCreated = DateTime.UtcNow, TimeModified = DateTime.UtcNow, TimeLastPlayed = null, EntryList = [] });
        ResponsePlaylist twice = Playlist("Twice");

        await Assert.ThrowsAnyAsync<Exception>(() => store.ReplacePlaylistList([twice, twice]));

        Assert.Equal([old.ID], (await store.GetPlaylistList()).Select(x => x.ID));
        Assert.Equal([old.ID], await store.GetHeldPlaylistIDList());
    }

    [Fact]
    public async Task AFileThatIsNotADatabase_IsReplacedByAnEmptyOne_AndTheStoreWorksAfterwards()
    {
        Directory.CreateDirectory(_folder);
        await File.WriteAllTextAsync(DatabasePath, "this is not a sqlite database, it is text that a crash left behind" + new string('x', 5000), TestContext.Current.CancellationToken);
        IMetadataStore store = CreateStore();

        Assert.Empty(await store.GetMediaFileList());
        Assert.Null(await store.GetLibraryETag());

        ResponseMediaFile file = MediaFile("After");
        await store.SaveMediaFile(file);

        SqliteConnection.ClearAllPools();
        Assert.Equal([file.ID], (await CreateStore().GetMediaFileList()).Select(x => x.ID));
    }

    [Fact]
    public async Task ConcurrentWrites_DoNotThrow_AndAllLand()
    {
        IMetadataStore store = CreateStore();
        List<ResponseMediaFile> fileList = [.. Enumerable.Range(0, 40).Select(index => MediaFile($"Media {index}"))];

        await Task.WhenAll(fileList.Select(file => Task.Run(() => store.SaveMediaFile(file), TestContext.Current.CancellationToken)));

        Assert.Equal(fileList.Select(x => x.ID).Order(), (await store.GetMediaFileList()).Select(x => x.ID).Order());
    }

    [Fact]
    public async Task ReadsAndWritesAtTheSameTime_DoNotThrow()
    {
        IMetadataStore store = CreateStore();
        await store.ReplaceMediaFileList([.. Enumerable.Range(0, 200).Select(index => MediaFile($"Media {index}"))]);

        Task writes = Task.Run(async () =>
        {
            for (int index = 0; index < 20; index++) await store.SaveMediaFile(MediaFile($"New {index}"));
        }, TestContext.Current.CancellationToken);

        Task reads = Task.Run(async () =>
        {
            for (int index = 0; index < 20; index++) await store.GetMediaFileList();
        }, TestContext.Current.CancellationToken);

        await Task.WhenAll(writes, reads);

        Assert.Equal(220, (await store.GetMediaFileList()).Count);
    }
}
