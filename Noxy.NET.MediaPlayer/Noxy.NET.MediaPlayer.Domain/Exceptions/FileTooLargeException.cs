namespace Noxy.NET.MediaPlayer.Domain.Exceptions;

public class FileTooLargeException(long maxSizeBytes) : Exception($"The file exceeds the maximum size of {maxSizeBytes} bytes.");
