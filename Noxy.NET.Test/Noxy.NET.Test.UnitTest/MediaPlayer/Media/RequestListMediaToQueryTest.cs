using Noxy.NET.MediaPlayer.Application.Queries.Media;
using Noxy.NET.MediaPlayer.Application.Requests.Media;

namespace Noxy.NET.Test.MediaPlayer.Media;

public class RequestListMediaToQueryTest
{
    [Fact]
    public void ToQuery_CarriesTheUserSkipAndTake()
    {
        Guid userID = Guid.CreateVersion7();

        QueryMediaList query = new RequestListMedia { Skip = 3, Take = 7 }.ToQuery(userID);

        Assert.Equal(new(userID, 3, 7), query);
    }
}
