namespace Noxy.NET.MediaPlayer.Application;

public static class ApplicationConstants
{
    // The path of the media hub, added to the server address. The API maps the hub here and the app connects here.
    public const string MediaHubPath = "/hubs/media";

    // The fields of the upload form. The optional title field comes before the file field; the API reads them and the app writes them.
    public const string TitleField = "title";
    public const string FileField = "file";
}
