using Mediator;

namespace Noxy.NET.MediaPlayer.Application.Commands.Metadata;

public abstract record CommandNamedItemSplit<TResponse>(Guid UserID, Guid ID, List<string> NameList) : ICommand<List<TResponse>>;
