using Noxy.NET.MediaPlayer.Domain.Entities;

namespace Noxy.NET.Test.MediaPlayer.Fakes;

public sealed class FakeStore
{
    public List<User> UserList { get; } = [];
    public List<ExternalLogin> ExternalLoginList { get; } = [];
    public List<RefreshToken> RefreshTokenList { get; } = [];
    public List<MediaFile> MediaFileList { get; } = [];
    public List<MediaUploadJob> MediaUploadJobList { get; } = [];

    public int CommitCount { get; set; }
    public bool FailCommit { get; set; }
    public int FailCommitCount { get; set; }
    public bool LoseJobClaim { get; set; }
    public int RevokeAllCount { get; set; }
    public bool LoseRevokeRace { get; set; }
}
