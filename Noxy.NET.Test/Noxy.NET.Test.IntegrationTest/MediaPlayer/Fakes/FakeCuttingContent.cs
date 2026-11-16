using System.Net;

namespace Noxy.NET.Test.MediaPlayer.Fakes;

// A response body that delivers only the first bytes and then fails, like a connection that drops half way through a download. The headers (length, type) stay those of the real response.
public sealed class FakeCuttingContent : HttpContent
{
    private readonly HttpContent _inner;
    private readonly int _bytes;

    public FakeCuttingContent(HttpContent inner, int bytes)
    {
        _inner = inner;
        _bytes = bytes;

        foreach (KeyValuePair<string, IEnumerable<string>> header in inner.Headers) Headers.TryAddWithoutValidation(header.Key, header.Value);
    }

    protected override async Task SerializeToStreamAsync(Stream stream, TransportContext? context)
    {
        await using Stream cutting = await CreateContentReadStreamAsync();
        await cutting.CopyToAsync(stream);
    }

    protected override async Task<Stream> CreateContentReadStreamAsync()
    {
        return new CuttingStream(await _inner.ReadAsStreamAsync(), _bytes);
    }

    protected override bool TryComputeLength(out long length)
    {
        length = 0;
        return false;
    }

    private sealed class CuttingStream(Stream inner, int bytes) : Stream
    {
        private int _delivered;

        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length => throw new NotSupportedException();

        public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }

        public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            if (_delivered >= bytes) throw new IOException("The connection was cut.");

            int read = await inner.ReadAsync(buffer[..Math.Min(buffer.Length, bytes - _delivered)], cancellationToken);
            _delivered += read;

            return read;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return ReadAsync(buffer.AsMemory(offset, count)).AsTask().GetAwaiter().GetResult();
        }

        public override void Flush() { }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) inner.Dispose();
            base.Dispose(disposing);
        }
    }
}
