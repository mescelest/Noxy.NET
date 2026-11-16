namespace Noxy.NET.Test.MediaPlayer.Fakes;

// A response body that delivers some bytes and then either fails, stalls until it is disposed, or ends.
public sealed class FakeInterruptedStream(byte[] head, Exception? failure = null, bool stall = false) : Stream
{
    private readonly CancellationTokenSource _disposed = new();
    private int _position;

    public override bool CanRead => true;
    public override bool CanSeek => false;
    public override bool CanWrite => false;
    public override long Length => throw new NotSupportedException();

    public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }

    public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
    {
        if (_position < head.Length)
        {
            int count = Math.Min(buffer.Length, head.Length - _position);
            head.AsMemory(_position, count).CopyTo(buffer);
            _position += count;

            return count;
        }

        if (stall)
        {
            try
            {
                await Task.Delay(Timeout.Infinite, CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _disposed.Token).Token);
            }
            catch (OperationCanceledException) when (_disposed.IsCancellationRequested)
            {
                throw new ObjectDisposedException(nameof(FakeInterruptedStream));
            }
        }

        if (failure is not null) throw failure;

        return 0;
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
        if (disposing) _disposed.Cancel();
        base.Dispose(disposing);
    }
}
