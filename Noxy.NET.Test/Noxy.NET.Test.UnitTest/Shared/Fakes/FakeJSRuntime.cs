using Microsoft.JSInterop;

namespace Noxy.NET.Test.Shared.Fakes;

// For components that are only rendered to text: the page's script is never reached, so any call to it is a mistake in the test.
public sealed class FakeJSRuntime : IJSRuntime
{
    public ValueTask<TValue> InvokeAsync<TValue>(string identifier, object?[]? args)
    {
        throw new NotSupportedException($"The test rendered a call to the script ({identifier}).");
    }

    public ValueTask<TValue> InvokeAsync<TValue>(string identifier, CancellationToken cancellationToken, object?[]? args)
    {
        throw new NotSupportedException($"The test rendered a call to the script ({identifier}).");
    }
}
