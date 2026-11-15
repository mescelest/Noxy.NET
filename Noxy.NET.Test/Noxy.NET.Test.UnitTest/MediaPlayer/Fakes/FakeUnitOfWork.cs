using Noxy.NET.MediaPlayer.Application.Interfaces;
using Noxy.NET.MediaPlayer.Application.Interfaces.Repositories;
using Noxy.NET.MediaPlayer.Domain.Exceptions;

namespace Noxy.NET.Test.MediaPlayer.Fakes;

public sealed class FakeUnitOfWork(FakeStore store) : IUnitOfWork
{
    private readonly FakeAuthenticationRepository _repository = new(store);
    private readonly FakeMediaRepository _media = new(store);
    private readonly FakePlaylistRepository _playlist = new(store);
    private readonly FakeMetadataRepository _metadata = new(store);
    private readonly FakeSettingRepository _setting = new(store);
    private readonly FakePlaybackRepository _playback = new(store);

    public IAuthenticationRepository Authentication => _repository;
    public IMediaRepository Media => _media;
    public IPlaylistRepository Playlist => _playlist;
    public IMetadataRepository Metadata => _metadata;
    public ISettingRepository Setting => _setting;
    public IPlaybackRepository Playback => _playback;

    public Task Commit(CancellationToken cancellationToken = default)
    {
        if (store.FailCommit) throw new InvalidOperationException("The commit failed.");

        store.OnCommit?.Invoke();

        if (store.KeyConflictCommitCount > 0)
        {
            store.KeyConflictCommitCount--;
            throw new MetadataKeyConflictException();
        }

        if (store.FailCommitCount > 0)
        {
            store.FailCommitCount--;
            throw new InvalidOperationException("The commit failed.");
        }

        _repository.Flush();
        _media.Flush();
        _playlist.Flush();
        _metadata.Flush();
        _setting.Flush();
        _playback.Flush();
        store.CommitCount++;
        return Task.CompletedTask;
    }

    public ValueTask DisposeAsync()
    {
        return ValueTask.CompletedTask;
    }
}
