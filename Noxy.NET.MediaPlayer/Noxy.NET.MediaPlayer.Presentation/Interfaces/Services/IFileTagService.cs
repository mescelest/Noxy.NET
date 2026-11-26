using Noxy.NET.MediaPlayer.Presentation.Models;

namespace Noxy.NET.MediaPlayer.Presentation.Interfaces.Services;

// Keeps the tags in the files on this device level with the metadata of their media files, without changing what the server stores or making a file download again. A file problem (a locked or unreadable file, a full disk) never throws: the result or the list says what happened, and the file and its state stay as they were.
public interface IFileTagService
{
    // Raised after a check or a write changed what is recorded or what is out of date.
    event Action? Changed;

    // Reads the tags of the held files whose recorded tag time is not the media file's TimeModified and compares them with the metadata. Equal: the current TimeModified is recorded and nothing is written. Different, or no readable tags: the file is remembered as out of date until its state or its media file's TimeModified changes. A file whose artists or genres are not in the local lists yet is left unchecked. Never writes a file.
    Task CheckFiles();

    // Writes the current metadata into the files that are on the device and records the TimeModified it wrote for. The ones that are not on the device, or whose metadata has not been loaded yet, or whose write failed, come back in the failed list with a message.
    Task<BulkResult> Update(List<Guid> mediaFileIDList);

    // The held files whose tags a check found different from the metadata. A file that was not checked is not listed.
    Task<List<Guid>> GetOutdatedIDList();

    // Update for every file in GetOutdatedIDList.
    Task<BulkResult> UpdateOutdated();
}
