using Mediator;
using Noxy.NET.MediaPlayer.Application.Models.Metadata;

namespace Noxy.NET.MediaPlayer.Application.Queries.Metadata;

public abstract record QueryMetadataList<TResponse>(Guid UserID, string? IfNoneMatch) : IQuery<MetadataLibrary<TResponse>>;
