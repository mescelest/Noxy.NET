using System.Net;
using System.Text.Json;
using Noxy.NET.MediaPlayer.Application.Requests.Media;
using Noxy.NET.MediaPlayer.Application.Requests.Playlist;
using Noxy.NET.MediaPlayer.Domain.Responses.Media;
using Noxy.NET.MediaPlayer.Domain.Responses.Metadata;
using Noxy.NET.MediaPlayer.Domain.Responses.Playlist;
using Noxy.NET.MediaPlayer.Domain.Responses.Setting;
using Noxy.NET.MediaPlayer.Presentation.Exceptions;
using Noxy.NET.MediaPlayer.Presentation.Interfaces.Services;
using Noxy.NET.MediaPlayer.Presentation.Models;

namespace Noxy.NET.Test.MediaPlayer.Fakes;

public sealed class FakeApiClient : IApiClient
{
    // A handler gets the ETag the call was sent with and returns the scripted response, or throws to script a failure.
    public Func<string?, ApiResponse<List<ResponseMediaFile>>> LibraryHandler { get; set; } = _ => new(HttpStatusCode.OK, [], null);

    public Func<string?, ApiResponse<List<ResponsePlaylist>>> PlaylistListHandler { get; set; } = _ => new(HttpStatusCode.OK, [], null);

    public Func<Guid, string?, ApiResponse<ResponsePlaylistDetail>> PlaylistDetailHandler { get; set; } = (_, _) => throw new ApiException(HttpStatusCode.NotFound, "Not found.");

    // Answers each GetFile call, and may wait on a gate or throw to script a failure. The default is a 404.
    public Func<Guid, long?, string?, Task<ApiFileResponse>> FileHandler { get; set; } = (_, _, _) => throw new ApiException(HttpStatusCode.NotFound, "Not found.");

    public List<(Guid ID, long? RangeStart, string? IfRange)> FileRequestList { get; } = [];

    private readonly Lock _fileRequestLock = new();

    public List<string?> LibraryETagList { get; } = [];

    public List<string?> PlaylistListETagList { get; } = [];

    public List<(Guid ID, string? ETag)> PlaylistDetailRequestList { get; } = [];

    public List<Guid> TaggedFileRequestList { get; } = [];

    // Answers each GetTaggedFile call. The default is a 404.
    public Func<Guid, Task<ApiFileResponse>> TaggedFileHandler { get; set; } = _ => throw new ApiException(HttpStatusCode.NotFound, "Not found.");

    public Task<ApiFileResponse> GetTaggedFile(Guid id)
    {
        TaggedFileRequestList.Add(id);
        return TaggedFileHandler(id);
    }

    public Task<ApiFileResponse> GetFile(Guid id, long? rangeStart, string? ifRange)
    {
        lock (_fileRequestLock) FileRequestList.Add((id, rangeStart, ifRange));
        return FileHandler(id, rangeStart, ifRange);
    }

    public Task<ApiResponse<List<ResponseMediaFile>>> GetLibrary(string? eTag)
    {
        LibraryETagList.Add(eTag);
        return Task.FromResult(LibraryHandler(eTag));
    }

    public Task<ApiResponse<List<ResponsePlaylist>>> GetPlaylistList(string? eTag)
    {
        PlaylistListETagList.Add(eTag);
        return Task.FromResult(PlaylistListHandler(eTag));
    }

    public Task<ApiResponse<ResponsePlaylistDetail>> GetPlaylistDetail(Guid id, string? eTag)
    {
        PlaylistDetailRequestList.Add((id, eTag));
        return Task.FromResult(PlaylistDetailHandler(id, eTag));
    }

    public Func<string?, ApiResponse<List<ResponseSetting>>> SettingListHandler { get; set; } = _ => new(HttpStatusCode.OK, [], null);

    public Func<string, JsonElement, ResponseSetting> UpdateSettingHandler { get; set; } = (key, _) => throw new ApiException(HttpStatusCode.NotFound, $"The setting {key} was not found.");

    public Func<string?, ApiResponse<List<ResponseArtist>>> ArtistListHandler { get; set; } = _ => new(HttpStatusCode.OK, [], null);

    public Func<string?, ApiResponse<List<ResponseAlbum>>> AlbumListHandler { get; set; } = _ => new(HttpStatusCode.OK, [], null);

    public Func<string?, ApiResponse<List<ResponseGenre>>> GenreListHandler { get; set; } = _ => new(HttpStatusCode.OK, [], null);

    public List<string?> SettingListETagList { get; } = [];

    public List<(string Key, JsonElement Value)> UpdateSettingRequestList { get; } = [];

    public List<string?> ArtistListETagList { get; } = [];

    public List<string?> AlbumListETagList { get; } = [];

    public List<string?> GenreListETagList { get; } = [];

    public List<Guid> PlayedRequestList { get; } = [];

    public Task<ApiResponse<List<ResponseSetting>>> GetSettingList(string? eTag)
    {
        SettingListETagList.Add(eTag);
        return Task.FromResult(SettingListHandler(eTag));
    }

    public Task<ResponseSetting> UpdateSetting(string key, JsonElement value)
    {
        UpdateSettingRequestList.Add((key, value));
        return Task.FromResult(UpdateSettingHandler(key, value));
    }

    public Task<ApiResponse<List<ResponseArtist>>> GetArtistList(string? eTag)
    {
        ArtistListETagList.Add(eTag);
        return Task.FromResult(ArtistListHandler(eTag));
    }

    public Task<ApiResponse<List<ResponseAlbum>>> GetAlbumList(string? eTag)
    {
        AlbumListETagList.Add(eTag);
        return Task.FromResult(AlbumListHandler(eTag));
    }

    public Task<ApiResponse<List<ResponseGenre>>> GetGenreList(string? eTag)
    {
        GenreListETagList.Add(eTag);
        return Task.FromResult(GenreListHandler(eTag));
    }

    // Thrown by MarkPlaylistPlayed, after it is recorded, when set.
    public Exception? MarkPlaylistPlayedException { get; set; }

    public Task MarkPlaylistPlayed(Guid id)
    {
        PlayedRequestList.Add(id);
        if (MarkPlaylistPlayedException is not null) throw MarkPlaylistPlayedException;

        return Task.CompletedTask;
    }

    public List<RequestCreatePlaylist> CreatePlaylistRequestList { get; } = [];

    public Task<ResponsePlaylist> CreatePlaylist(RequestCreatePlaylist request)
    {
        CreatePlaylistRequestList.Add(request);
        return WriteFailure is null
            ? Task.FromResult(PlaylistFor(Guid.CreateVersion7(), request.Name))
            : throw WriteFailure;
    }

    // Thrown by every playlist write, after it is recorded, when set.
    public Exception? WriteFailure { get; set; }

    public static ResponsePlaylist PlaylistFor(Guid id, string name) => new() { ID = id, Name = name, AllowDuplicates = true, EntryCount = 0, TimeCreated = DateTime.UtcNow, TimeModified = DateTime.UtcNow, TimeLastPlayed = null };

    public static ResponsePlaylistDetail DetailFor(Guid id) => new() { ID = id, Name = "Playlist", AllowDuplicates = true, EntryCount = 0, TimeCreated = DateTime.UtcNow, TimeModified = DateTime.UtcNow, TimeLastPlayed = null, EntryList = [] };

    public Func<Guid, RequestUpdatePlaylist, ResponsePlaylist> UpdatePlaylistHandler { get; set; } = (id, _) => PlaylistFor(id, "Playlist");

    public List<(Guid ID, RequestUpdatePlaylist Request)> UpdatePlaylistRequestList { get; } = [];

    public Task<ResponsePlaylist> UpdatePlaylist(Guid id, RequestUpdatePlaylist request)
    {
        UpdatePlaylistRequestList.Add((id, request));
        if (WriteFailure is not null) throw WriteFailure;
        return Task.FromResult(UpdatePlaylistHandler(id, request));
    }

    public Func<Guid, RequestUpdateMedia, ResponseMediaFile> UpdateMediaHandler { get; set; } =
        (id, _) => new() { ID = id, Title = "Media", ContentType = "audio/mpeg", SizeBytes = 1, ContentHash = "h", TimeCreated = DateTime.UtcNow, TimeModified = DateTime.UtcNow };

    public List<(Guid ID, RequestUpdateMedia Request)> UpdateMediaRequestList { get; } = [];

    public Task<ResponseMediaFile> UpdateMedia(Guid id, RequestUpdateMedia request)
    {
        UpdateMediaRequestList.Add((id, request));
        if (WriteFailure is not null) throw WriteFailure;
        return Task.FromResult(UpdateMediaHandler(id, request));
    }

    public List<Guid> DeleteMediaList { get; } = [];

    // Returns the exception DeleteMedia throws for an ID, or null for success.
    public Func<Guid, Exception?>? DeleteMediaFailure { get; set; }

    public Task DeleteMedia(Guid id)
    {
        DeleteMediaList.Add(id);
        if (DeleteMediaFailure?.Invoke(id) is { } failure) throw failure;

        return Task.CompletedTask;
    }

    public List<(Guid PlaylistID, RequestAddPlaylistEntry Request)> AddEntryCallList { get; } = [];

    // Thrown by AddPlaylistEntryList, after it is recorded, when set.
    public Exception? AddEntryFailure { get; set; }

    public List<Guid> DeletePlaylistList { get; } = [];

    public Task DeletePlaylist(Guid id)
    {
        DeletePlaylistList.Add(id);
        if (WriteFailure is not null) throw WriteFailure;

        return Task.CompletedTask;
    }

    // Answers each UploadMedia call, and may throw to script a refusal. The default accepts every file as a new job.
    public Func<string, ResponseUploadAccepted> UploadHandler { get; set; } = _ => new() { JobID = Guid.NewGuid() };

    public List<(string FileName, string? ContentType, Guid JobID)> UploadCallList { get; } = [];

    public Task<ResponseUploadAccepted> UploadMedia(string fileName, string? contentType, Func<Stream> openContent)
    {
        ResponseUploadAccepted accepted = UploadHandler(fileName);
        UploadCallList.Add((fileName, contentType, accepted.JobID));

        return Task.FromResult(accepted);
    }

    public Task<ResponsePlaylistDetail> AddPlaylistEntryList(Guid id, RequestAddPlaylistEntry request)
    {
        AddEntryCallList.Add((id, request));
        if (AddEntryFailure is not null) throw AddEntryFailure;

        return Task.FromResult(new ResponsePlaylistDetail { ID = id, Name = "Playlist", AllowDuplicates = true, EntryCount = request.MediaFileIDList.Count, TimeCreated = DateTime.UtcNow, TimeModified = DateTime.UtcNow, TimeLastPlayed = null, EntryList = [] });
    }

    public List<(Guid ID, RequestRemovePlaylistEntry Request)> RemoveEntryCallList { get; } = [];

    public Task<ResponsePlaylistDetail> RemovePlaylistEntryList(Guid id, RequestRemovePlaylistEntry request)
    {
        RemoveEntryCallList.Add((id, request));
        if (WriteFailure is not null) throw WriteFailure;

        return Task.FromResult(DetailFor(id));
    }

    public List<(Guid ID, Guid EntryID, RequestMovePlaylistEntry Request)> MoveEntryCallList { get; } = [];

    public Func<Guid, RequestMovePlaylist, List<ResponsePlaylist>> MovePlaylistHandler { get; set; } = (_, _) => [];

    public List<(Guid ID, RequestMovePlaylist Request)> MovePlaylistCallList { get; } = [];

    public Task<List<ResponsePlaylist>> MovePlaylist(Guid id, RequestMovePlaylist request)
    {
        MovePlaylistCallList.Add((id, request));
        if (WriteFailure is not null) throw WriteFailure;

        return Task.FromResult(MovePlaylistHandler(id, request));
    }

    public Task<ResponsePlaylistDetail> MovePlaylistEntry(Guid id, Guid entryID, RequestMovePlaylistEntry request)
    {
        MoveEntryCallList.Add((id, entryID, request));
        if (WriteFailure is not null) throw WriteFailure;

        return Task.FromResult(DetailFor(id));
    }
}
