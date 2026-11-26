using System.Net;
using System.Text.Json;
using Noxy.NET.MediaPlayer.Application.Requests.Media;
using Noxy.NET.MediaPlayer.Application.Requests.Metadata;
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

    // Decides the picture of an album, null for none. Throws to script a failure.
    public Func<Guid, byte[]?> AlbumArtHandler { get; set; } = _ => null;

    public List<Guid> AlbumArtRequestList { get; } = [];

    // When set, a picture request waits for it, so a test can hold requests in flight.
    public Task? AlbumArtGate { get; set; }

    public async Task<byte[]?> GetAlbumArt(Guid id)
    {
        AlbumArtRequestList.Add(id);
        if (AlbumArtGate is not null) await AlbumArtGate;

        return AlbumArtHandler(id);
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

    // Thrown by every write (playlist, artist, album, genre), after it is recorded, when set.
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
        (id, _) => new() { ID = id, Title = "Media", ContentType = "audio/mpeg", SizeBytes = 1, TimeCreated = DateTime.UtcNow, TimeModified = DateTime.UtcNow };

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

    // The bytes of every file sent, read from the stream UploadMedia was given, in the order of the calls.
    public List<byte[]> UploadContentList { get; } = [];

    public async Task<ResponseUploadAccepted> UploadMedia(string fileName, string? contentType, Func<Stream> openContent)
    {
        ResponseUploadAccepted accepted = UploadHandler(fileName);
        await using Stream content = openContent();
        using MemoryStream copy = new();
        await content.CopyToAsync(copy);
        UploadContentList.Add(copy.ToArray());
        UploadCallList.Add((fileName, contentType, accepted.JobID));

        return accepted;
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

    public static ResponseArtist ArtistFor(Guid id, string name) => new() { ID = id, Name = name, MediaFileCount = 0, TimeModified = DateTime.UtcNow };

    public static ResponseAlbum AlbumFor(Guid id, string title, List<Guid>? artistIDList = null) => new() { ID = id, Title = title, ArtistIDList = artistIDList ?? [], MediaFileCount = 0, TimeModified = DateTime.UtcNow };

    public static ResponseGenre GenreFor(Guid id, string name) => new() { ID = id, Name = name, MediaFileCount = 0, TimeModified = DateTime.UtcNow };

    public List<(Guid ID, byte[] Picture, string ContentType)> SetAlbumArtRequestList { get; } = [];

    public Task SetAlbumArt(Guid id, byte[] picture, string contentType)
    {
        SetAlbumArtRequestList.Add((id, picture, contentType));
        if (WriteFailure is not null) throw WriteFailure;

        return Task.CompletedTask;
    }

    public List<(Guid ID, RequestMergeItem Request)> MergeArtistRequestList { get; } = [];
    public List<(Guid ID, RequestMergeItem Request)> MergeAlbumRequestList { get; } = [];
    public List<(Guid ID, RequestMergeItem Request)> MergeGenreRequestList { get; } = [];

    public Task<ResponseArtist> MergeArtist(Guid id, RequestMergeItem request)
    {
        MergeArtistRequestList.Add((id, request));
        if (WriteFailure is not null) throw WriteFailure;

        return Task.FromResult(ArtistFor(request.TargetID, "Artist"));
    }

    public Task<ResponseAlbum> MergeAlbum(Guid id, RequestMergeItem request)
    {
        MergeAlbumRequestList.Add((id, request));
        if (WriteFailure is not null) throw WriteFailure;

        return Task.FromResult(AlbumFor(request.TargetID, "Album"));
    }

    public Task<ResponseGenre> MergeGenre(Guid id, RequestMergeItem request)
    {
        MergeGenreRequestList.Add((id, request));
        if (WriteFailure is not null) throw WriteFailure;

        return Task.FromResult(GenreFor(request.TargetID, "Genre"));
    }

    public List<RequestCreateArtist> CreateArtistRequestList { get; } = [];

    public Func<RequestCreateArtist, ResponseArtist> CreateArtistHandler { get; set; } = request => ArtistFor(Guid.CreateVersion7(), request.Name);

    public Task<ResponseArtist> CreateArtist(RequestCreateArtist request)
    {
        CreateArtistRequestList.Add(request);
        if (WriteFailure is not null) throw WriteFailure;

        return Task.FromResult(CreateArtistHandler(request));
    }

    public List<(Guid ID, RequestUpdateArtist Request)> UpdateArtistRequestList { get; } = [];

    public Func<Guid, RequestUpdateArtist, ResponseArtist> UpdateArtistHandler { get; set; } = (id, _) => ArtistFor(id, "Artist");

    public Task<ResponseArtist> UpdateArtist(Guid id, RequestUpdateArtist request)
    {
        UpdateArtistRequestList.Add((id, request));
        if (WriteFailure is not null) throw WriteFailure;

        return Task.FromResult(UpdateArtistHandler(id, request));
    }

    public List<(Guid ID, RequestSetArtistWeight Request)> SetArtistWeightRequestList { get; } = [];

    public Task<ResponseArtist> SetArtistWeight(Guid id, RequestSetArtistWeight request)
    {
        SetArtistWeightRequestList.Add((id, request));
        if (WriteFailure is not null) throw WriteFailure;

        return Task.FromResult(ArtistFor(id, "Artist"));
    }

    public List<(Guid ID, RequestSetGenreWeight Request)> SetGenreWeightRequestList { get; } = [];

    public Task<ResponseGenre> SetGenreWeight(Guid id, RequestSetGenreWeight request)
    {
        SetGenreWeightRequestList.Add((id, request));
        if (WriteFailure is not null) throw WriteFailure;

        return Task.FromResult(GenreFor(id, "Genre"));
    }

    public List<Guid> DeleteArtistList { get; } = [];

    public Task DeleteArtist(Guid id)
    {
        DeleteArtistList.Add(id);
        if (WriteFailure is not null) throw WriteFailure;

        return Task.CompletedTask;
    }

    public List<(Guid ID, RequestAddMediaFileList Request)> AddArtistMediaFileListRequestList { get; } = [];

    public Func<Guid, RequestAddMediaFileList, ResponseArtist> AddArtistMediaFileListHandler { get; set; } = (id, _) => ArtistFor(id, "Artist");

    public Task<ResponseArtist> AddArtistMediaFileList(Guid id, RequestAddMediaFileList request)
    {
        AddArtistMediaFileListRequestList.Add((id, request));
        if (WriteFailure is not null) throw WriteFailure;

        return Task.FromResult(AddArtistMediaFileListHandler(id, request));
    }

    public List<(Guid ID, RequestRemoveMediaFileList Request)> RemoveArtistMediaFileListRequestList { get; } = [];

    public Func<Guid, RequestRemoveMediaFileList, ResponseArtist> RemoveArtistMediaFileListHandler { get; set; } = (id, _) => ArtistFor(id, "Artist");

    public Task<ResponseArtist> RemoveArtistMediaFileList(Guid id, RequestRemoveMediaFileList request)
    {
        RemoveArtistMediaFileListRequestList.Add((id, request));
        if (WriteFailure is not null) throw WriteFailure;

        return Task.FromResult(RemoveArtistMediaFileListHandler(id, request));
    }

    public List<RequestCreateAlbum> CreateAlbumRequestList { get; } = [];

    public Func<RequestCreateAlbum, ResponseAlbum> CreateAlbumHandler { get; set; } = request => AlbumFor(Guid.CreateVersion7(), request.Title, request.ArtistIDList);

    public Task<ResponseAlbum> CreateAlbum(RequestCreateAlbum request)
    {
        CreateAlbumRequestList.Add(request);
        if (WriteFailure is not null) throw WriteFailure;

        return Task.FromResult(CreateAlbumHandler(request));
    }

    public List<(Guid ID, RequestUpdateAlbum Request)> UpdateAlbumRequestList { get; } = [];

    public Func<Guid, RequestUpdateAlbum, ResponseAlbum> UpdateAlbumHandler { get; set; } = (id, _) => AlbumFor(id, "Album");

    public Task<ResponseAlbum> UpdateAlbum(Guid id, RequestUpdateAlbum request)
    {
        UpdateAlbumRequestList.Add((id, request));
        if (WriteFailure is not null) throw WriteFailure;

        return Task.FromResult(UpdateAlbumHandler(id, request));
    }

    public List<Guid> DeleteAlbumList { get; } = [];

    public Task DeleteAlbum(Guid id)
    {
        DeleteAlbumList.Add(id);
        if (WriteFailure is not null) throw WriteFailure;

        return Task.CompletedTask;
    }

    public List<(Guid ID, RequestAddMediaFileList Request)> AddAlbumMediaFileListRequestList { get; } = [];

    public Func<Guid, RequestAddMediaFileList, ResponseAlbum> AddAlbumMediaFileListHandler { get; set; } = (id, _) => AlbumFor(id, "Album");

    public Task<ResponseAlbum> AddAlbumMediaFileList(Guid id, RequestAddMediaFileList request)
    {
        AddAlbumMediaFileListRequestList.Add((id, request));
        if (WriteFailure is not null) throw WriteFailure;

        return Task.FromResult(AddAlbumMediaFileListHandler(id, request));
    }

    public List<(Guid ID, RequestRemoveMediaFileList Request)> RemoveAlbumMediaFileListRequestList { get; } = [];

    public Func<Guid, RequestRemoveMediaFileList, ResponseAlbum> RemoveAlbumMediaFileListHandler { get; set; } = (id, _) => AlbumFor(id, "Album");

    public Task<ResponseAlbum> RemoveAlbumMediaFileList(Guid id, RequestRemoveMediaFileList request)
    {
        RemoveAlbumMediaFileListRequestList.Add((id, request));
        if (WriteFailure is not null) throw WriteFailure;

        return Task.FromResult(RemoveAlbumMediaFileListHandler(id, request));
    }

    public List<RequestCreateGenre> CreateGenreRequestList { get; } = [];

    public Func<RequestCreateGenre, ResponseGenre> CreateGenreHandler { get; set; } = request => GenreFor(Guid.CreateVersion7(), request.Name);

    public Task<ResponseGenre> CreateGenre(RequestCreateGenre request)
    {
        CreateGenreRequestList.Add(request);
        if (WriteFailure is not null) throw WriteFailure;

        return Task.FromResult(CreateGenreHandler(request));
    }

    public List<(Guid ID, RequestUpdateGenre Request)> UpdateGenreRequestList { get; } = [];

    public Func<Guid, RequestUpdateGenre, ResponseGenre> UpdateGenreHandler { get; set; } = (id, _) => GenreFor(id, "Genre");

    public Task<ResponseGenre> UpdateGenre(Guid id, RequestUpdateGenre request)
    {
        UpdateGenreRequestList.Add((id, request));
        if (WriteFailure is not null) throw WriteFailure;

        return Task.FromResult(UpdateGenreHandler(id, request));
    }

    public List<Guid> DeleteGenreList { get; } = [];

    public Task DeleteGenre(Guid id)
    {
        DeleteGenreList.Add(id);
        if (WriteFailure is not null) throw WriteFailure;

        return Task.CompletedTask;
    }

    public List<(Guid ID, RequestAddMediaFileList Request)> AddGenreMediaFileListRequestList { get; } = [];

    public Func<Guid, RequestAddMediaFileList, ResponseGenre> AddGenreMediaFileListHandler { get; set; } = (id, _) => GenreFor(id, "Genre");

    public Task<ResponseGenre> AddGenreMediaFileList(Guid id, RequestAddMediaFileList request)
    {
        AddGenreMediaFileListRequestList.Add((id, request));
        if (WriteFailure is not null) throw WriteFailure;

        return Task.FromResult(AddGenreMediaFileListHandler(id, request));
    }

    public List<(Guid ID, RequestRemoveMediaFileList Request)> RemoveGenreMediaFileListRequestList { get; } = [];

    public Func<Guid, RequestRemoveMediaFileList, ResponseGenre> RemoveGenreMediaFileListHandler { get; set; } = (id, _) => GenreFor(id, "Genre");

    public Task<ResponseGenre> RemoveGenreMediaFileList(Guid id, RequestRemoveMediaFileList request)
    {
        RemoveGenreMediaFileListRequestList.Add((id, request));
        if (WriteFailure is not null) throw WriteFailure;

        return Task.FromResult(RemoveGenreMediaFileListHandler(id, request));
    }
}
