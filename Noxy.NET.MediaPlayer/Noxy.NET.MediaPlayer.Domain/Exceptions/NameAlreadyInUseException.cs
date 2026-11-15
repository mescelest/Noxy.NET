namespace Noxy.NET.MediaPlayer.Domain.Exceptions;

public class NameAlreadyInUseException(Guid existingID) : Exception("An item with that name already exists.")
{
    public Guid ExistingID { get; } = existingID;
}
