namespace Noxy.NET.Test.MediaPlayer.Fakes;

public sealed class FakeBlockingStream(byte[] content) : Stream
{
    private readonly TaskCompletionSource _release = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private int _position;

    public bool IsWaiting { get; private set; }

    public void Release()
    {
        _release.TrySetResult();
    }

    public override bool CanRead => true;
    public override bool CanSeek => false;
    public override bool CanWrite => false;
    public override long Length => throw new NotSupportedException();

    public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }

    // Hands over its content as it is asked for, then waits for Release, like an upload that is still arriving.
    public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
    {
        if (_position < content.Length)
        {
            int count = Math.Min(buffer.Length, content.Length - _position);
            content.AsMemory(_position, count).CopyTo(buffer);
            _position += count;
            return count;
        }

        IsWaiting = true;
        await _release.Task.WaitAsync(cancellationToken);
        return 0;
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        throw new NotSupportedException();
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
}
