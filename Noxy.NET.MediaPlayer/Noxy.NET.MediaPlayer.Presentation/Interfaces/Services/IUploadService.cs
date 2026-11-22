using Noxy.NET.MediaPlayer.Presentation.Models;

namespace Noxy.NET.MediaPlayer.Presentation.Interfaces.Services;

public interface IUploadService
{
    // Raised when a call to Upload begins sending, with the number of files.
    event Action<int>? Started;

    // Raised once every file of a call to Upload has a final answer. Can come long after Upload returned: the server finishes each file in the background and tells the app on the hub.
    event Action<UploadBatchResult>? BatchCompleted;

    // Sends the files one after the other, in the order given, to the library. With a playlist ID each file that the server accepts into the library is also added to that playlist, once the server says it is stored. A file the server refuses is part of the result and never throws. The result of a file that is accepted but never reported (the hub is down) is not part of any batch, so such a batch does not complete.
    Task Upload(List<UploadFile> fileList, Guid? playlistID = null);
}
