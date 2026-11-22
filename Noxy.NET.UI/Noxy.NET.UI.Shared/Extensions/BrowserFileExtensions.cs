using Microsoft.AspNetCore.Components.Forms;

namespace Noxy.NET.UI.Extensions;

public static class BrowserFileExtensions
{
    extension(IBrowserFile file)
    {
        // Whether the file fits an accept list the way a file input writes it: comma separated, each entry a type ("audio/mpeg"), a group of types ("audio/*") or an extension (".mp3"). No list accepts every file, and so does a file whose type the browser could not tell, since the name alone can't be trusted either way.
        public bool IsAccepted(string? accept)
        {
            if (string.IsNullOrWhiteSpace(accept) || string.IsNullOrWhiteSpace(file.ContentType)) return true;

            return accept.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Any(file.Matches);
        }

        private bool Matches(string entry)
        {
            if (entry.StartsWith('.')) return file.Name.EndsWith(entry, StringComparison.OrdinalIgnoreCase);
            if (entry.EndsWith("/*")) return file.ContentType.StartsWith(entry[..^1], StringComparison.OrdinalIgnoreCase);

            return string.Equals(file.ContentType, entry, StringComparison.OrdinalIgnoreCase);
        }
    }
}
