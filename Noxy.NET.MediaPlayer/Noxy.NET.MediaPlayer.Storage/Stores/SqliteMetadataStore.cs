using System.Globalization;
using System.Text.Json;
using Microsoft.Data.Sqlite;
using Noxy.NET.MediaPlayer.Domain.Responses.Media;
using Noxy.NET.MediaPlayer.Domain.Responses.Metadata;
using Noxy.NET.MediaPlayer.Domain.Responses.Playlist;
using Noxy.NET.MediaPlayer.Domain.Responses.Setting;
using Noxy.NET.MediaPlayer.Presentation.Interfaces.Stores;
using Noxy.NET.MediaPlayer.Presentation.Models;

namespace Noxy.NET.MediaPlayer.Storage.Stores;

public class SqliteMetadataStore(string databasePath) : IMetadataStore, ISettingStore, IMetadataListStore, IVolumeStore
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
    private const string UserSettingTable = "UserSetting";
    private const string ArtistTable = "Artist";
    private const string AlbumTable = "Album";
    private const string GenreTable = "Genre";
    private const string SettingListETagKey = "SettingListETag";
    private const string ArtistListETagKey = "ArtistListETag";
    private const string AlbumListETagKey = "AlbumListETag";
    private const string GenreListETagKey = "GenreListETag";
    private const string VolumeKey = "Volume";
    private const int DefaultVolume = 100;

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

    #region -- Settings --

    public async Task<List<ResponseSetting>> GetSettingList()
    {
        return await ReadList<ResponseSetting>(UserSettingTable);
    }

    public async Task ReplaceSettingList(List<ResponseSetting> settingList)
    {
        await ReplaceTable(UserSettingTable, settingList, x => x.Key);
    }

    public async Task SaveSetting(ResponseSetting setting)
    {
        await Upsert(UserSettingTable, setting.Key, setting);
    }

    public async Task<string?> GetSettingListETag()
    {
        return await GetSetting(SettingListETagKey);
    }

    public async Task SetSettingListETag(string? eTag)
    {
        await SetSetting(SettingListETagKey, eTag);
    }

    #endregion -- Settings --

    #region -- Artists, albums and genres --

    public async Task<List<ResponseArtist>> GetArtistList()
    {
        return await ReadList<ResponseArtist>(ArtistTable);
    }

    public async Task ReplaceArtistList(List<ResponseArtist> artistList)
    {
        await ReplaceTable(ArtistTable, artistList, x => x.ID.ToString());
    }

    public async Task SaveArtist(ResponseArtist artist)
    {
        await Upsert(ArtistTable, artist.ID.ToString(), artist);
    }

    public async Task DeleteArtist(Guid id)
    {
        await Delete(ArtistTable, id);
    }

    public async Task<string?> GetArtistListETag()
    {
        return await GetSetting(ArtistListETagKey);
    }

    public async Task SetArtistListETag(string? eTag)
    {
        await SetSetting(ArtistListETagKey, eTag);
    }

    public async Task<List<ResponseAlbum>> GetAlbumList()
    {
        return await ReadList<ResponseAlbum>(AlbumTable);
    }

    public async Task ReplaceAlbumList(List<ResponseAlbum> albumList)
    {
        await ReplaceTable(AlbumTable, albumList, x => x.ID.ToString());
    }

    public async Task SaveAlbum(ResponseAlbum album)
    {
        await Upsert(AlbumTable, album.ID.ToString(), album);
    }

    public async Task DeleteAlbum(Guid id)
    {
        await Delete(AlbumTable, id);
    }

    public async Task<string?> GetAlbumListETag()
    {
        return await GetSetting(AlbumListETagKey);
    }

    public async Task SetAlbumListETag(string? eTag)
    {
        await SetSetting(AlbumListETagKey, eTag);
    }

    public async Task<List<ResponseGenre>> GetGenreList()
    {
        return await ReadList<ResponseGenre>(GenreTable);
    }

    public async Task ReplaceGenreList(List<ResponseGenre> genreList)
    {
        await ReplaceTable(GenreTable, genreList, x => x.ID.ToString());
    }

    public async Task SaveGenre(ResponseGenre genre)
    {
        await Upsert(GenreTable, genre.ID.ToString(), genre);
    }

    public async Task DeleteGenre(Guid id)
    {
        await Delete(GenreTable, id);
    }

    public async Task<string?> GetGenreListETag()
    {
        return await GetSetting(GenreListETagKey);
    }

    public async Task SetGenreListETag(string? eTag)
    {
        await SetSetting(GenreListETagKey, eTag);
    }

    #endregion -- Artists, albums and genres --

    #region -- Volume --

    public async Task<int> GetVolume()
    {
        return int.TryParse(await GetSetting(VolumeKey), NumberStyles.Integer, CultureInfo.InvariantCulture, out int volume) ? volume : DefaultVolume;
    }

    public async Task SetVolume(int volume)
    {
        await SetSetting(VolumeKey, volume.ToString(CultureInfo.InvariantCulture));
    }

    #endregion -- Volume --

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
        await Upsert(table, id.ToString(), value);
    }

    private async Task Upsert<T>(string table, string id, T value)
    {
        await using SqliteConnection connection = await Open();
        await using SqliteCommand command = connection.CreateCommand();
        command.CommandText = $"INSERT INTO {table} (ID, Json) VALUES ($id, $json) ON CONFLICT(ID) DO UPDATE SET Json = excluded.Json";
        command.Parameters.AddWithValue("$id", id);
        command.Parameters.AddWithValue("$json", JsonSerializer.Serialize(value, JsonSerializerOptions.Web));

        await command.ExecuteNonQueryAsync();
    }

    private async Task Delete(string table, Guid id)
    {
        await using SqliteConnection connection = await Open();
        await Execute(connection, null, $"DELETE FROM {table} WHERE ID = $id", ("$id", id.ToString()));
    }

    private async Task ReplaceTable<T>(string table, List<T> valueList, Func<T, string> idOf)
    {
        await using SqliteConnection connection = await Open();
        await using SqliteTransaction transaction = (SqliteTransaction)await connection.BeginTransactionAsync();

        await Execute(connection, transaction, $"DELETE FROM {table}");
        foreach (T value in valueList) await Insert(connection, transaction, table, idOf(value), value);

        await transaction.CommitAsync();
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
        await Insert(connection, transaction, table, id.ToString(), value);
    }

    private static async Task Insert<T>(SqliteConnection connection, SqliteTransaction transaction, string table, string id, T value)
    {
        await Execute(connection, transaction, $"INSERT INTO {table} (ID, Json) VALUES ($id, $json)", ("$id", id),("$json", JsonSerializer.Serialize(value, JsonSerializerOptions.Web)));
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

        foreach (string table in (string[])[MediaFileTable, PlaylistTable, PlaylistDetailTable, FileStateTable, UserSettingTable, ArtistTable, AlbumTable, GenreTable])
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
