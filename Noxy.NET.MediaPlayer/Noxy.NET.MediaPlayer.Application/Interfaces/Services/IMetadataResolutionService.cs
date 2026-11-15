using Noxy.NET.MediaPlayer.Application.Models.Metadata;

namespace Noxy.NET.MediaPlayer.Application.Interfaces.Services;

public interface IMetadataResolutionService
{
    // Turns the text of a tag set into the user's items: credits are split, each name is found by its key or added to the unit of work, and an album is found by its title and album artists. Nothing is committed, so the caller commits (and, when another request created the same item first, the commit throws MetadataKeyConflictException and the caller runs again to reuse the winner). A name is cut to 255 characters before keying, blank values are ignored, and an album with no title is not created.
    Task<ResolvedMetadata> Resolve(IUnitOfWork uow, Guid userID, MediaTagSet tags, CancellationToken cancellationToken = default);
}
