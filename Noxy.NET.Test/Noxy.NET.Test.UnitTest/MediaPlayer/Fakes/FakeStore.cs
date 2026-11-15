using Noxy.NET.MediaPlayer.Domain.Entities;

namespace Noxy.NET.Test.MediaPlayer.Fakes;

public sealed class FakeStore
{
    public List<EntityUser> UserList { get; } = [];
    public List<EntityExternalLogin> ExternalLoginList { get; } = [];
    public List<EntityRefreshToken> RefreshTokenList { get; } = [];
    public List<EntityMediaFile> MediaFileList { get; } = [];
    public List<EntityMediaUploadJob> MediaUploadJobList { get; } = [];
    public List<EntityPlaylist> PlaylistList { get; } = [];

    public int CommitCount { get; set; }
    public bool FailCommit { get; set; }
    public int FailCommitCount { get; set; }
    public bool LoseJobClaim { get; set; }
    public int RevokeAllCount { get; set; }
    public bool LoseRevokeRace { get; set; }
}
