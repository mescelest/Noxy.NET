using Noxy.NET.MediaPlayer.Domain.Responses.Media;
using Noxy.NET.MediaPlayer.Domain.Responses.Playlist;
using Noxy.NET.MediaPlayer.Presentation.Extensions;
using Noxy.NET.MediaPlayer.Presentation.Interfaces.Services;
using Noxy.NET.MediaPlayer.Presentation.Interfaces.Stores;
using Noxy.NET.MediaPlayer.Presentation.Models;

namespace Noxy.NET.MediaPlayer.Presentation.Services;

public class HomeService(IMetadataStore storeMetadata, IPlaybackControlService serviceControl, ILocalQueueStore storeLocalQueue) : IHomeService
{
    public async Task<HomeModel> Get()
    {
        List<ResponsePlaylist> playlistList = await storeMetadata.GetPlaylistList();
        List<ResponsePlaylist> orderedList = playlistList.OrderMostRecentlyPlayedFirst();

        // The played ones, newest first; when none was ever played, the first of the sidebar order.
        List<ResponsePlaylist> recentList = orderedList.Any(x => x.TimeLastPlayed is not null)
            ? [.. orderedList.Where(x => x.TimeLastPlayed is not null).Take(PresentationConstants.HomeRecentCount)]
            : [.. orderedList.Take(PresentationConstants.HomeRecentCount)];

        return new(recentList, await GetContinue(), playlistList.Count > 0);
    }

    private async Task<ContinueInfo?> GetContinue()
    {
        if (!serviceControl.CanContinue) return null;

        LocalQueueSnapshot? snapshot = await storeLocalQueue.Get();
        if (snapshot?.Queue.Current is not { } current) return null;

        ResponseMediaFile? media = await storeMetadata.GetMediaFile(current.MediaFileID);

        return new(media?.Title, media?.Artist, snapshot.PositionSeconds);
    }
}
