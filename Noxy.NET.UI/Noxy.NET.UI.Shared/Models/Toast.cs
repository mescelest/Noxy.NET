using Noxy.NET.UI.Enums;

namespace Noxy.NET.UI.Models;

public sealed class Toast
{
    public Guid ID { get; } = Guid.NewGuid();

    public required string Message { get; init; }

    public StatusEnum Status { get; init; }

    // A permanent toast stays until it is dismissed; a temporary one expires on its own.
    public bool IsPermanent { get; init; }

    // How long a temporary toast stays once it is shown. Null follows the default of the service.
    public TimeSpan? Duration { get; init; }
}
