using System.Net;

namespace Noxy.NET.Test.MediaPlayer.Fakes;

// A response body that notes whether anything buffered it or read it, and whether its stream was disposed.
public sealed class FakeTrackedContent : HttpContent
{
    private readonly byte[] _bytes;
    private readonly TrackedStream _stream;

    public FakeTrackedContent(byte[] bytes)
    {
        _bytes = bytes;
        _stream = new(bytes);
    }

    // True when something asked the content to write itself out, which is what buffering the whole body does.
    public bool Buffered { get; private set; }

    public bool WasRead => _stream.WasRead;

    public bool StreamDisposed => _stream.IsDisposed;

    protected override Task SerializeToStreamAsync(Stream stream, TransportContext? context)
    {
        Buffered = true;
        return stream.WriteAsync(_bytes).AsTask();
    }

    protected override Task<Stream> CreateContentReadStreamAsync()
    {
        return Task.FromResult<Stream>(_stream);
    }

    protected override bool TryComputeLength(out long length)
    {
        length = _bytes.Length;
        return true;
    }

    private sealed class TrackedStream(byte[] bytes) : MemoryStream(bytes, false)
    {
        public bool WasRead { get; private set; }

        public bool IsDisposed { get; private set; }

        public override int Read(byte[] buffer, int offset, int count)
        {
            WasRead = true;
            return base.Read(buffer, offset, count);
        }

        public override int Read(Span<byte> buffer)
        {
            WasRead = true;
            return base.Read(buffer);
        }

        public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            WasRead = true;
            return base.ReadAsync(buffer, cancellationToken);
        }

        protected override void Dispose(bool disposing)
        {
            IsDisposed = true;
            base.Dispose(disposing);
        }
    }
}
