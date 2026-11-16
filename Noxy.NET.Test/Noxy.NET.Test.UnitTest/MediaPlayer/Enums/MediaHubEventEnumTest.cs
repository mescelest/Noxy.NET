using Noxy.NET.MediaPlayer.Domain.Enums;

namespace Noxy.NET.Test.MediaPlayer.Enums;

public class MediaHubEventEnumTest
{
    [Fact]
    public void HasExactlyTheMembersThatRemain()
    {
        List<string> expectedList =
        [
            nameof(MediaHubEventEnum.UploadSucceeded),
            nameof(MediaHubEventEnum.UploadFailed),
            nameof(MediaHubEventEnum.MediaUpdated),
            nameof(MediaHubEventEnum.MediaDeleted),
            nameof(MediaHubEventEnum.PlaylistUpdated),
            nameof(MediaHubEventEnum.PlaylistDeleted),
            nameof(MediaHubEventEnum.ArtistUpdated),
            nameof(MediaHubEventEnum.ArtistDeleted),
            nameof(MediaHubEventEnum.AlbumUpdated),
            nameof(MediaHubEventEnum.AlbumDeleted),
            nameof(MediaHubEventEnum.GenreUpdated),
            nameof(MediaHubEventEnum.GenreDeleted),
            nameof(MediaHubEventEnum.SettingUpdated)
        ];

        Assert.Equal(expectedList.Order(), Enum.GetNames<MediaHubEventEnum>().Order());
    }
}
