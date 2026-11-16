using System.Text.Json;
using Microsoft.Data.Sqlite;
using Noxy.NET.MediaPlayer.Domain.Responses.Media;
using Noxy.NET.MediaPlayer.Domain.Responses.Playlist;
using Noxy.NET.MediaPlayer.Presentation.Interfaces.Stores;
using Noxy.NET.MediaPlayer.Presentation.Models;

namespace Noxy.NET.MediaPlayer.Storage.Stores;

public class SqliteMetadataStore(string databasePath) : IMetadataStore
{
    // SQLite's result codes for a file that is not a database and for a damaged one.
    private const int SqliteCorrupt = 11;
    private const int SqliteNotADatabase = 26;

    private const string MediaFileTable = "MediaFile";
    private const string PlaylistTable = "Playlist";
    private const string PlaylistDetailTable = "PlaylistDetail";
    private const string FileStateTable = "FileState";
    private const string LibraryETagKey = "LibraryETag";
    private const string PlaylistListETagKey = "PlaylistListETag";

    private readonly SemaphoreSlim _readyLock = new(1, 1);
    private readonly string _connectionString = new SqliteConnectionStringBuilder { DataSource = databasePath, Mode = SqliteOpenMode.ReadWriteCreate, Pooling = true }.ToString();
    private bool _ready;

    public async Task<List<ResponseMediaFile>> GetMediaFileList()
    {
        return await ReadList<ResponseMediaFile>(MediaFileTable);
    }

    public async Task<ResponseMediaFile?> GetMediaFile(Guid id)
    {
        await using SqliteConnection connection = await Open();
        await using SqliteCommand command = connection.CreateCommand();
        command.CommandText = $"SELECT Json FROM {MediaFileTable} WHERE ID = $id";
        command.Parameters.AddWithValue("$id", id.ToString());

        return await command.ExecuteScalarAsync() is string json ? Deserialize<ResponseMediaFile>(json) : null;
    }

    public async Task ReplaceMediaFileList(List<ResponseMediaFile> mediaFileList)
    {
        await using SqliteConnection connection = await Open();
        await using SqliteTransaction transaction = (SqliteTransaction)await connection.BeginTransactionAsync();

        await Execute(connection, transaction, $"DELETE FROM {MediaFileTable}");
        foreach (ResponseMediaFile mediaFile in mediaFileList) await Insert(connection, transaction, MediaFileTable, mediaFile.ID, mediaFile);

        await Execute(connection, transaction, $"DELETE FROM {FileStateTable} WHERE ID NOT IN (SELECT ID FROM {MediaFileTable})");

        await transaction.CommitAsync();
    }

    public async Task SaveMediaFile(ResponseMediaFile mediaFile)
    {
        await Upsert(MediaFileTable, mediaFile.ID, mediaFile);
    }

    public async Task DeleteMediaFile(Guid id)
    {
        await using SqliteConnection connection = await Open();
        await using SqliteTransaction transaction = (SqliteTransaction)await connection.BeginTransactionAsync();

        await Execute(connection, transaction, $"DELETE FROM {MediaFileTable} WHERE ID = $id", ("$id", id.ToString()));
        await Execute(connection, transaction, $"DELETE FROM {FileStateTable} WHERE ID = $id", ("$id", id.ToString()));

        await transaction.CommitAsync();
    }

    public async Task<List<ResponsePlaylist>> GetPlaylistList()
    {
        return await ReadList<ResponsePlaylist>(PlaylistTable);
    }

    public async Task ReplacePlaylistList(List<ResponsePlaylist> playlistList)
    {
        await using SqliteConnection connection = await Open();
        await using SqliteTransaction transaction = (SqliteTransaction)await connection.BeginTransactionAsync();

        await Execute(connection, transaction, $"DELETE FROM {PlaylistTable}");
        foreach (ResponsePlaylist playlist in playlistList) await Insert(connection, transaction, PlaylistTable, playlist.ID, playlist);

        await Execute(connection, transaction, $"DELETE FROM {PlaylistDetailTable} WHERE ID NOT IN (SELECT ID FROM {PlaylistTable})");

        await transaction.CommitAsync();
    }

    public async Task SavePlaylist(ResponsePlaylist playlist)
    {
        await Upsert(PlaylistTable, playlist.ID, playlist);
    }

    public async Task DeletePlaylist(Guid id)
    {
        await using SqliteConnection connection = await Open();
        await using SqliteTransaction transaction = (SqliteTransaction)await connection.BeginTransactionAsync();

        await Execute(connection, transaction, $"DELETE FROM {PlaylistTable} WHERE ID = $id", ("$id", id.ToString()));
        await Execute(connection, transaction, $"DELETE FROM {PlaylistDetailTable} WHERE ID = $id", ("$id", id.ToString()));

        await transaction.CommitAsync();
    }

    public async Task<ResponsePlaylistDetail?> GetPlaylistDetail(Guid id)
    {
        await using SqliteConnection connection = await Open();
        await using SqliteCommand command = connection.CreateCommand();
        command.CommandText = $"SELECT Json FROM {PlaylistDetailTable} WHERE ID = $id";
        command.Parameters.AddWithValue("$id", id.ToString());

        return await command.ExecuteScalarAsync() is string json ? Deserialize<ResponsePlaylistDetail>(json) : null;
    }

    public async Task SavePlaylistDetail(ResponsePlaylistDetail detail)
    {
        await Upsert(PlaylistDetailTable, detail.ID, detail);
    }

    public async Task<List<Guid>> GetHeldPlaylistIDList()
    {
        await using SqliteConnection connection = await Open();
        await using SqliteCommand command = connection.CreateCommand();
        command.CommandText = $"SELECT ID FROM {PlaylistDetailTable} ORDER BY RowID";

        List<Guid> idList = [];
        await using SqliteDataReader reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync()) idList.Add(Guid.Parse(reader.GetString(0)));

        return idList;
    }

    public async Task<string?> GetLibraryETag()
    {
        return await GetSetting(LibraryETagKey);
    }

    public async Task SetLibraryETag(string? eTag)
    {
        await SetSetting(LibraryETagKey, eTag);
    }

    public async Task<string?> GetPlaylistListETag()
    {
        return await GetSetting(PlaylistListETagKey);
    }

    public async Task SetPlaylistListETag(string? eTag)
    {
        await SetSetting(PlaylistListETagKey, eTag);
    }

    public async Task<List<FileState>> GetFileStateList()
    {
        return await ReadList<FileState>(FileStateTable);
    }

    public async Task SaveFileState(FileState fileState)
    {
        await Upsert(FileStateTable, fileState.MediaFileID, fileState);
    }

    public async Task DeleteFileState(Guid mediaFileID)
    {
        await Delete(FileStateTable, mediaFileID);
    }

    private async Task<List<T>> ReadList<T>(string table)
    {
        await using SqliteConnection connection = await Open();
        await using SqliteCommand command = connection.CreateCommand();
        command.CommandText = $"SELECT Json FROM {table} ORDER BY RowID";

        List<T> list = [];
        await using SqliteDataReader reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync()) list.Add(Deserialize<T>(reader.GetString(0)));

        return list;
    }

    // An existing row keeps its place in the order: only a new ID goes last.
    private async Task Upsert<T>(string table, Guid id, T value)
    {
        await using SqliteConnection connection = await Open();
        await using SqliteCommand command = connection.CreateCommand();
        command.CommandText = $"INSERT INTO {table} (ID, Json) VALUES ($id, $json) ON CONFLICT(ID) DO UPDATE SET Json = excluded.Json";
        command.Parameters.AddWithValue("$id", id.ToString());
        command.Parameters.AddWithValue("$json", JsonSerializer.Serialize(value, JsonSerializerOptions.Web));

        await command.ExecuteNonQueryAsync();
    }

    private async Task Delete(string table, Guid id)
    {
        await using SqliteConnection connection = await Open();
        await Execute(connection, null, $"DELETE FROM {table} WHERE ID = $id", ("$id", id.ToString()));
    }

    private async Task<string?> GetSetting(string key)
    {
        await using SqliteConnection connection = await Open();
        await using SqliteCommand command = connection.CreateCommand();
        command.CommandText = "SELECT Value FROM Setting WHERE Key = $key";
        command.Parameters.AddWithValue("$key", key);

        return await command.ExecuteScalarAsync() as string;
    }

    private async Task SetSetting(string key, string? value)
    {
        await using SqliteConnection connection = await Open();
        if (value is null)
        {
            await Execute(connection, null, "DELETE FROM Setting WHERE Key = $key", ("$key", key));
            return;
        }

        await Execute(connection, null, "INSERT INTO Setting (Key, Value) VALUES ($key, $value) ON CONFLICT(Key) DO UPDATE SET Value = excluded.Value", ("$key", key), ("$value", value));
    }

    private static async Task Insert<T>(SqliteConnection connection, SqliteTransaction transaction, string table, Guid id, T value)
    {
        await Execute(connection, transaction, $"INSERT INTO {table} (ID, Json) VALUES ($id, $json)", ("$id", id.ToString()), ("$json", JsonSerializer.Serialize(value, JsonSerializerOptions.Web)));
    }

    private static async Task Execute(SqliteConnection connection, SqliteTransaction? transaction, string sql, params (string Name, string Value)[] parameterList)
    {
        await using SqliteCommand command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = sql;
        foreach ((string name, string value) in parameterList) command.Parameters.AddWithValue(name, value);

        await command.ExecuteNonQueryAsync();
    }

    private static T Deserialize<T>(string json)
    {
        return JsonSerializer.Deserialize<T>(json, JsonSerializerOptions.Web) ?? throw new InvalidOperationException($"A stored {typeof(T).Name} is empty.");
    }

    private async Task<SqliteConnection> Open()
    {
        await EnsureReady();

        SqliteConnection connection = new(_connectionString);
        await connection.OpenAsync();
        return connection;
    }

    // Creates the folder, the file and the tables on first use. A file that is not a database, or a damaged one, is deleted and made again: the library on the device is only a copy, and the next sync fills it.
    private async Task EnsureReady()
    {
        if (_ready) return;

        await _readyLock.WaitAsync();
        try
        {
            if (_ready) return;

            string? folder = Path.GetDirectoryName(Path.GetFullPath(databasePath));
            if (folder is not null) Directory.CreateDirectory(folder);

            try
            {
                await CreateSchema();
            }
            catch (SqliteException exception) when (exception.SqliteErrorCode is SqliteCorrupt or SqliteNotADatabase)
            {
                DeleteDatabaseFiles();
                await CreateSchema();
            }

            _ready = true;
        }
        finally
        {
            _readyLock.Release();
        }
    }

    private async Task CreateSchema()
    {
        await using SqliteConnection connection = new(_connectionString);
        await connection.OpenAsync();

        // Write-ahead logging lets a read go on while a write happens.
        await Execute(connection, null, "PRAGMA journal_mode = WAL");

        foreach (string table in (string[])[MediaFileTable, PlaylistTable, PlaylistDetailTable, FileStateTable])
        {
            await Execute(connection, null, $"CREATE TABLE IF NOT EXISTS {table} (RowID INTEGER PRIMARY KEY AUTOINCREMENT, ID TEXT NOT NULL UNIQUE, Json TEXT NOT NULL)");
        }

        await Execute(connection, null, "CREATE TABLE IF NOT EXISTS Setting (Key TEXT PRIMARY KEY, Value TEXT NOT NULL)");
    }

    private void DeleteDatabaseFiles()
    {
        SqliteConnection.ClearAllPools();

        foreach (string path in (string[])[databasePath, $"{databasePath}-wal", $"{databasePath}-shm"])
        {
            if (File.Exists(path)) File.Delete(path);
        }
    }
}
