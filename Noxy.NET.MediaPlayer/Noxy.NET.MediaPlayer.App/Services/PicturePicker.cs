using Noxy.NET.MediaPlayer.App.Resources;
using Noxy.NET.MediaPlayer.Presentation;
using Noxy.NET.MediaPlayer.Presentation.Interfaces.Services;
using Noxy.NET.MediaPlayer.Presentation.Models;

namespace Noxy.NET.MediaPlayer.App.Services;

public class PicturePicker : IPicturePicker
{
    private const string UnknownContentType = "application/octet-stream";
    private const string AndroidImageType = "image/*";
    private const int BufferSize = 81920;

    private static readonly FilePickerFileType PictureType = new(new Dictionary<DevicePlatform, IEnumerable<string>>
    {
        [DevicePlatform.WinUI] = [".jpg", ".jpeg", ".png", ".webp", ".gif", ".bmp"],
        [DevicePlatform.Android] = [AndroidImageType]
    });

    public async Task<PickedPicture?> Pick()
    {
        FileResult? file = await FilePicker.Default.PickAsync(new() { PickerTitle = AppTexts.PicturePickerTitle, FileTypes = PictureType });
        if (file is null) return null;

        await using Stream stream = await file.OpenReadAsync();
        using MemoryStream copy = new();
        byte[] buffer = new byte[BufferSize];

        // The reading stops as soon as the picture is known to be too big, so a huge file is not read to its end.
        int read;
        while ((read = await stream.ReadAsync(buffer)) > 0)
        {
            copy.Write(buffer, 0, read);
            if (copy.Length > PresentationConstants.AlbumArtMaxBytes) throw new IOException(AppTexts.PicturePickerTooLarge);
        }

        // The server reads the bytes and ignores the type, so a file the platform could not name still goes; an empty type would make the request fail when it builds its header.
        return new(copy.ToArray(), string.IsNullOrWhiteSpace(file.ContentType) ? UnknownContentType : file.ContentType);
    }
}
