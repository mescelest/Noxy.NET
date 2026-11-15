namespace Noxy.NET.MediaPlayer.Application.Interfaces.Models;

// The part of Optional<T> that does not depend on T, so validation attributes can read any of them.
public interface IOptional
{
    bool HasValue { get; }
    object? UntypedValue { get; }
}
