using Microsoft.AspNetCore.Components.Forms;

namespace Noxy.NET.Test.Shared.Fakes;

public sealed class FakeBrowserFile(string name, string contentType) : IBrowserFile
{
    public string Name { get; } = name;

    public DateTimeOffset LastModified { get; } = DateTimeOffset.UnixEpoch;

    public long Size => 0;

    public string ContentType { get; } = contentType;

    public Stream OpenReadStream(long maxAllowedSize = 512000, CancellationToken cancellationToken = default)
    {
        return new MemoryStream();
    }
}
