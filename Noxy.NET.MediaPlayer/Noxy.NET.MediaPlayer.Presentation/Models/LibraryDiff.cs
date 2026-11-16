using Noxy.NET.MediaPlayer.Domain.Responses.Media;

namespace Noxy.NET.MediaPlayer.Presentation.Models;

// What a change to the library did to the media files. ChangedList holds the media files whose ContentHash differs from the previous copy: a metadata-only change is not in it, since the bytes on the device are still right.
public record LibraryDiff(List<ResponseMediaFile> AddedList, List<ResponseMediaFile> ChangedList, List<Guid> RemovedIDList)
{
    public bool IsEmpty => AddedList.Count == 0 && ChangedList.Count == 0 && RemovedIDList.Count == 0;
}
