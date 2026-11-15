using System.Net;
using Noxy.NET.MediaPlayer.Domain.Responses.Media;

namespace Noxy.NET.Test.MediaPlayer.Fixtures.Extensions;

public static class MediaApiClientExtension
{
    extension(MediaApiClient api)
    {
        // Uploads that many media files for the user, waits until they are all in the library, and returns their IDs ordered by title.
        public async Task<List<Guid>> UploadMediaFileList(ApiAccount user, int count)
        {
            for (int index = 1; index <= count; index++)
            {
                HttpResponseMessage response = await api.Upload(user.Token, MediaApiClient.Mp3(300 + index), $"Media {index}.mp3", $"Media {index}");
                Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);
                await api.WaitForMediaFileList(user.Token, index);
            }

            List<ResponseMediaFile> list = await api.GetMediaFileList(user.Token);
            return [.. list.OrderBy(x => x.Title).Select(x => x.ID)];
        }
    }
}
