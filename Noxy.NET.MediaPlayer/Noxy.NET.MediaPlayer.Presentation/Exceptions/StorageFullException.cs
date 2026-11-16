namespace Noxy.NET.MediaPlayer.Presentation.Exceptions;

public class StorageFullException(Exception? innerException = null) : Exception("The device has no room for this file.", innerException);
