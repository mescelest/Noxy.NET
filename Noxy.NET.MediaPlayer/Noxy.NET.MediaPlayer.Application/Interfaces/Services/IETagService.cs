namespace Noxy.NET.MediaPlayer.Application.Interfaces.Services;

public interface IETagService
{
    // A strong ETag over the items, whatever order they come in.
    string Compute(IEnumerable<(Guid ID, DateTime TimeModified)> stampList);

    // True when the If-None-Match header names the ETag, or is the wildcard.
    bool Matches(string ifNoneMatch, string eTag);
}
