using Noxy.NET.MediaPlayer.Domain.Responses.Media;

namespace Noxy.NET.MediaPlayer.Presentation.Models;

public record LibraryDiff(List<ResponseMediaFile> AddedList, List<ResponseMediaFile> ChangedList, List<Guid> RemovedIDList)
{
    public bool IsEmpty => AddedList.Count == 0 && ChangedList.Count == 0 && RemovedIDList.Count == 0;
}
