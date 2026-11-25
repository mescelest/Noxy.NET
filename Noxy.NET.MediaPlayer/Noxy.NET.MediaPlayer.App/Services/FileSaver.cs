using Noxy.NET.MediaPlayer.Presentation.Interfaces.Services;
// ReSharper disable once RedundantUsingDirective
using Noxy.NET.MediaPlayer.App.Resources;

namespace Noxy.NET.MediaPlayer.App.Services;

public class FileSaver : IFileSaver
{
#if WINDOWS
    public async Task<bool> Save(string suggestedName, Stream content)
    {
        string extension = Path.GetExtension(suggestedName);
        Windows.Storage.Pickers.FileSavePicker picker = new() { SuggestedFileName = Path.GetFileNameWithoutExtension(suggestedName) };
        picker.FileTypeChoices.Add(AppTexts.FileSaverMediaFile, [string.IsNullOrEmpty(extension) ? "." : extension]);

        if (Microsoft.Maui.Controls.Application.Current?.Windows.FirstOrDefault()?.Handler?.PlatformView is not Microsoft.UI.Xaml.Window window)
        {
            throw new InvalidOperationException("There is no window to show the save dialog in.");
        }

        WinRT.Interop.InitializeWithWindow.Initialize(picker, WinRT.Interop.WindowNative.GetWindowHandle(window));

        Windows.Storage.StorageFile? file = await picker.PickSaveFileAsync();
        if (file is null) return false;

        await using Stream target = await file.OpenStreamForWriteAsync();
        target.SetLength(0);
        await content.CopyToAsync(target);

        return true;
    }
#else
    // No save dialog here: the file is written to the cache and handed to the share sheet.
    public async Task<bool> Save(string suggestedName, Stream content)
    {
        string path = Path.Combine(FileSystem.CacheDirectory, Path.GetFileName(suggestedName));
        await using (FileStream target = File.Create(path))
        {
            await content.CopyToAsync(target);
        }

        await Share.Default.RequestAsync(new ShareFileRequest { Title = suggestedName, File = new(path) });

        return true;
    }
#endif
}
