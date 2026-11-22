using Microsoft.AspNetCore.Components.Forms;
using Noxy.NET.MediaPlayer.Presentation.Models;

namespace Noxy.NET.MediaPlayer.App.Extensions;

public static class BrowserFileListExtensions
{
    extension(List<IBrowserFile> fileList)
    {
        // The server enforces the size limit, so each stream is opened without one of its own.
        public List<UploadFile> ToUploadFileList()
        {
            return [.. fileList.Select(file => new UploadFile(file.Name, file.ContentType, () => file.OpenReadStream(long.MaxValue)))];
        }
    }
}
