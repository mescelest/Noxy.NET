using System.Text.RegularExpressions;
using Noxy.NET.MediaPlayer.Application.Interfaces.Services;

namespace Noxy.NET.MediaPlayer.Application.Services;

public partial class CreditSplittingService(INameKeyService serviceNameKey) : ICreditSplittingService
{
    public List<string> Split(string? credit)
    {
        return credit is null ? [] : Split([credit]);
    }

    public List<string> Split(IEnumerable<string> valueList)
    {
        List<string> result = [];
        HashSet<string> keySet = [];

        foreach (string value in valueList)
        {
            // A bracketed "(feat. B)" becomes a plain separator first, so its brackets are removed with it.
            string unbracketed = BracketedMarkerRegex().Replace(value, ";${names}");

            foreach (string part in SeparatorRegex().Split(unbracketed))
            {
                string name = part.Trim();
                if (name.Length == 0) continue;

                string key = serviceNameKey.GetKey(name);
                if (key.Length == 0 || !keySet.Add(key)) continue;

                result.Add(name);
            }
        }

        return result;
    }

    [GeneratedRegex(@"[(\[]\s*(?:feat\.|ft\.|featuring(?![\p{L}\p{N}]))\s*(?<names>[^)\]]*)[)\]]", RegexOptions.IgnoreCase)]
    private static partial Regex BracketedMarkerRegex();

    [GeneratedRegex(@";|(?<![\p{L}\p{N}])(?:feat\.|ft\.|featuring(?![\p{L}\p{N}]))", RegexOptions.IgnoreCase)]
    private static partial Regex SeparatorRegex();
}
