namespace Noxy.NET.MediaPlayer.Storage.Extensions;

public static class IOExceptionExtension
{
    // ERROR_HANDLE_DISK_FULL and ERROR_DISK_FULL as Windows reports them in an HResult, and ENOSPC as Android (Linux) reports it.
    private const int WindowsHandleDiskFull = unchecked((int)0x80070027);
    private const int WindowsDiskFull = unchecked((int)0x80070070);
    private const int UnixNoSpace = 28;

    extension(IOException exception)
    {
        public bool IsDiskFull => exception.HResult is WindowsHandleDiskFull or WindowsDiskFull or UnixNoSpace;
    }
}
