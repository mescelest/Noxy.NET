using Mediator;

namespace Noxy.NET.MediaPlayer.Application.Commands.Metadata;

public abstract record CommandNamedItemMediaFileListAdd<TResponse>(Guid UserID, Guid ID, List<Guid> MediaFileIDList) : ICommand<TResponse>;
