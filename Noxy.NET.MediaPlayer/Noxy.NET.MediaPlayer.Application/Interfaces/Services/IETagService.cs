namespace Noxy.NET.MediaPlayer.Application.Interfaces.Services;

public interface IETagService
{
    // A strong ETag over the items, whatever order they come in.
    string Compute(IEnumerable<(Guid ID, DateTime TimeModified)> stampList);

    // The same over items that also carry a count, which an ETag over IDs and times alone would miss.
    string ComputeCounted(IEnumerable<(Guid ID, DateTime TimeModified, int Count)> stampList);

    // True when the If-None-Match header names the ETag, or is the wildcard.
    bool Matches(string ifNoneMatch, string eTag);
}
