namespace Noxy.NET.MediaPlayer.Domain.Exceptions;

public class MetadataKeyConflictException(Exception? innerException = null) : Exception("An item with that name already exists.", innerException);
