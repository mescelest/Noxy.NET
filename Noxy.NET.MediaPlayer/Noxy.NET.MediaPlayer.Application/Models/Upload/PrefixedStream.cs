namespace Noxy.NET.MediaPlayer.Application.Models.Upload;

public sealed class PrefixedStream(ReadOnlyMemory<byte> prefix, Stream inner) : Stream
{
    private int _prefixRead;

    public override bool CanRead => true;
    public override bool CanSeek => false;
    public override bool CanWrite => false;
    public override long Length => throw new NotSupportedException();

    public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }

    public override int Read(Span<byte> buffer)
    {
        int count = ReadPrefix(buffer);

        return count > 0 ? count : inner.Read(buffer);
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        return Read(buffer.AsSpan(offset, count));
    }

    public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
    {
        int count = ReadPrefix(buffer.Span);

        return count > 0 ? ValueTask.FromResult(count) : inner.ReadAsync(buffer, cancellationToken);
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

    private int ReadPrefix(Span<byte> buffer)
    {
        int count = Math.Min(buffer.Length, prefix.Length - _prefixRead);
        if (count <= 0) return 0;

        prefix.Span.Slice(_prefixRead, count).CopyTo(buffer);
        _prefixRead += count;
        return count;
    }
}
