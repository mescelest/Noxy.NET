using System.Security.Cryptography;
using System.Text;
using Noxy.NET.MediaPlayer.Application.Interfaces.Services;

namespace Noxy.NET.MediaPlayer.Application.Services;

public class ETagService : IETagService
{
    private const string WeakETagPrefix = "W/";
    private const string Wildcard = "*";
    private const char ETagQuote = '"';

    public string Compute(IEnumerable<(Guid ID, DateTime TimeModified)> stampList)
    {
        return Hash(stampList.OrderBy(x => x.ID).Select(x => $"{x.ID:D}:{x.TimeModified.Ticks}\n"));
    }

    public string ComputeCounted(IEnumerable<(Guid ID, DateTime TimeModified, int Count)> stampList)
    {
        return Hash(stampList.OrderBy(x => x.ID).Select(x => $"{x.ID:D}:{x.TimeModified.Ticks}:{x.Count}\n"));
    }

    // The header holds a comma separated list of tags, any of which may be weak ("W/") or the wildcard.
    public bool Matches(string ifNoneMatch, string eTag)
    {
        foreach (string token in ifNoneMatch.Split(','))
        {
            string tag = token.Trim();
            if (tag.StartsWith(WeakETagPrefix, StringComparison.Ordinal)) tag = tag[WeakETagPrefix.Length..];

            if (tag == Wildcard || tag == eTag) return true;
        }

        return false;
    }

    private static string Hash(IEnumerable<string> lineList)
    {
        using IncrementalHash hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        foreach (string line in lineList)
        {
            hash.AppendData(Encoding.UTF8.GetBytes(line));
        }

        return $"{ETagQuote}{Convert.ToHexStringLower(hash.GetHashAndReset())}{ETagQuote}";
    }
}
