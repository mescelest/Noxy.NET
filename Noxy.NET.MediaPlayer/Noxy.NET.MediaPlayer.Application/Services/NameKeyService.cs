using System.Text;
using System.Text.RegularExpressions;
using Noxy.NET.MediaPlayer.Application.Interfaces.Services;

namespace Noxy.NET.MediaPlayer.Application.Services;

public partial class NameKeyService : INameKeyService
{
    public string GetKey(string name)
    {
        string normalized = name.Normalize(NormalizationForm.FormKC);

        return WhitespaceRegex().Replace(normalized, " ").Trim().ToLowerInvariant();
    }

    [GeneratedRegex(@"\s+")]
    private static partial Regex WhitespaceRegex();
}
