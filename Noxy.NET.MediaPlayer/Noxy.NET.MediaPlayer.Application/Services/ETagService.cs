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
        using IncrementalHash hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        foreach ((Guid id, DateTime timeModified) in stampList.OrderBy(x => x.ID))
        {
            hash.AppendData(Encoding.UTF8.GetBytes($"{id:D}:{timeModified.Ticks}\n"));
        }

        return $"{ETagQuote}{Convert.ToHexStringLower(hash.GetHashAndReset())}{ETagQuote}";
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
}
