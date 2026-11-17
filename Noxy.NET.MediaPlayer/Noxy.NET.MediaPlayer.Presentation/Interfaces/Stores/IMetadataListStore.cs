using Noxy.NET.MediaPlayer.Domain.Responses.Metadata;

namespace Noxy.NET.MediaPlayer.Presentation.Interfaces.Stores;

public interface IMetadataListStore
{
    // The three kinds never touch each other.
    IListStore<ResponseArtist, Guid> Artist { get; }

    IListStore<ResponseAlbum, Guid> Album { get; }

    IListStore<ResponseGenre, Guid> Genre { get; }
}
