using Mediator;
using Noxy.NET.MediaPlayer.Application.Commands.Metadata;
using Noxy.NET.MediaPlayer.Application.Interfaces.Services;

namespace Noxy.NET.MediaPlayer.Application.Handlers.Metadata.Album;

public class HandlerAlbumArtSet(IAlbumArtService serviceAlbumArt) : ICommandHandler<CommandAlbumArtSet>
{
    // Throws MetadataItemNotFoundException for an unknown album or another user's, and InvalidMetadataOperationException for bytes that are not an image.
    public async ValueTask<Unit> Handle(CommandAlbumArtSet command, CancellationToken cancellationToken)
    {
        await serviceAlbumArt.Replace(command.UserID, command.ID, command.Picture, cancellationToken);

        return Unit.Value;
    }
}
