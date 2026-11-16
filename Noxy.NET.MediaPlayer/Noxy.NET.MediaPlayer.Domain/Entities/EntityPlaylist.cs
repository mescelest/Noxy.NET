using Noxy.NET.MediaPlayer.Domain.Abstractions.Entities;
using Noxy.NET.MediaPlayer.Domain.Enums;

namespace Noxy.NET.MediaPlayer.Domain.Entities;

public class EntityPlaylist : BaseEntity
{
    public required Guid UserID { get; set; }

    public required string Name { get; set; }
    public bool AllowDuplicates { get; set; } = true;

    public DateTime TimeModified { get; set; } = DateTime.UtcNow;
    public DateTime? TimeLastPlayed { get; set; }

    // Null means the user's global setting applies.
    public ShuffleModeEnum? Shuffle { get; set; }
    public RepeatModeEnum? Repeat { get; set; }

    public List<EntityPlaylistEntry> EntryList { get; set; } = [];

    public bool HasDuplicates()
    {
        return EntryList.Select(x => x.MediaFileID).Distinct().Count() != EntryList.Count;
    }

    // EntryList is the intended order. Positions become 0 to n-1 in that order, and the entries whose position changed are returned.
    public List<EntityPlaylistEntry> RenumberEntryList()
    {
        List<EntityPlaylistEntry> changedList = [];
        for (int index = 0; index < EntryList.Count; index++)
        {
            if (EntryList[index].Position == index) continue;

            EntryList[index].Position = index;
            changedList.Add(EntryList[index]);
        }

        return changedList;
    }
}
