using Noxy.NET.MediaPlayer.Domain.Entities;

namespace Noxy.NET.MediaPlayer.Domain.Exceptions;

public class DuplicateMediaException(EntityMediaFile existingFile) : Exception("The library already holds this file.")
{
    // The file the library already holds, so a caller that wants the file (to put it in a playlist) does not have to look for it.
    public EntityMediaFile ExistingFile { get; } = existingFile;
}
