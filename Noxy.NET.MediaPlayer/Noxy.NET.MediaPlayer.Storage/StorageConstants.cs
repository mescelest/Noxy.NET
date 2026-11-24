namespace Noxy.NET.MediaPlayer.Storage;

public static class StorageConstants
{
    // The metadata database inside the app's data folder.
    public const string MetadataDatabaseFileName = "metadata.sqlite";

    // The cache file of the queue this device had last, inside the app's data folder.
    public const string LocalQueueFileName = "queue.json";

    // The folder inside the app data folder that holds the album pictures this device keeps.
    public const string ArtCacheFolderName = "albumart";

    // This device own ID, name and volume, inside the app data folder.
    public const string DeviceFileName = "device.json";
}
