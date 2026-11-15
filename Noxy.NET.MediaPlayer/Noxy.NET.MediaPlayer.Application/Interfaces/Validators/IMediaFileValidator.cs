using Noxy.NET.MediaPlayer.Application.Models.Upload;

namespace Noxy.NET.MediaPlayer.Application.Interfaces.Validators;

public interface IMediaFileValidator
{
    // Number of leading bytes of the file that Validate needs.
    const int HeaderSize = 12;

    // Throws UnsupportedMediaTypeException when the extension is not allowed or the header does not match it.
    ValidatedMediaFile Validate(string fileName, ReadOnlySpan<byte> header);
}
