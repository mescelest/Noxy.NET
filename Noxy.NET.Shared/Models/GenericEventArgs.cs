namespace Noxy.NET.Models;

public class GenericEventArgs<T>(T value) : EventArgs
{
    public T Value { get; } = value;
}
