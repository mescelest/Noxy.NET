using Noxy.NET.MediaPlayer.Application.Streams;

namespace Noxy.NET.Test.MediaPlayer.Storage;

public class PrefixedStreamTest
{
    private static async Task<byte[]> ReadAll(Stream stream)
    {
        using MemoryStream copy = new();
        await stream.CopyToAsync(copy, TestContext.Current.CancellationToken);
        return copy.ToArray();
    }

    [Fact]
    public async Task Read_ReturnsPrefixThenInner()
    {
        PrefixedStream stream = new(new byte[] { 1, 2 }, new MemoryStream([3, 4, 5]));

        Assert.Equal([1, 2, 3, 4, 5], await ReadAll(stream));
    }

    [Fact]
    public async Task Read_EmptyPrefix_ReturnsInner()
    {
        PrefixedStream stream = new(ReadOnlyMemory<byte>.Empty, new MemoryStream([3, 4]));

        Assert.Equal([3, 4], await ReadAll(stream));
    }

    [Fact]
    public async Task Read_EmptyInner_ReturnsPrefix()
    {
        PrefixedStream stream = new(new byte[] { 1, 2 }, new MemoryStream());

        Assert.Equal([1, 2], await ReadAll(stream));
    }

    [Fact]
    public void Read_SmallBuffer_SpansThePrefixAndInner()
    {
        PrefixedStream stream = new(new byte[] { 1, 2, 3 }, new MemoryStream([4, 5]));
        byte[] buffer = new byte[2];
        List<byte> all = [];

        int read;
        while ((read = stream.Read(buffer, 0, buffer.Length)) > 0)
        {
            all.AddRange(buffer.Take(read));
        }

        Assert.Equal([1, 2, 3, 4, 5], all);
    }

    [Fact]
    public void Stream_IsReadOnlyAndForwardOnly()
    {
        PrefixedStream stream = new(ReadOnlyMemory<byte>.Empty, new MemoryStream());

        Assert.True(stream.CanRead);
        Assert.False(stream.CanSeek);
        Assert.False(stream.CanWrite);
        Assert.Throws<NotSupportedException>(() => stream.Seek(0, SeekOrigin.Begin));
    }
}
