using System.Security.Cryptography;
using System.Text;
using Noxy.NET.MediaPlayer.Presentation.Services;

namespace Noxy.NET.Test.MediaPlayer.Presentation;

public class HashServiceTest
{
    private readonly HashService _serviceHash = new();

    // Readable once and forward only, like a network stream: a seek or a length would throw.
    private sealed class ForwardOnlyStream(Stream inner) : Stream
    {
        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length => throw new NotSupportedException();

        public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }

        public override void Flush() { }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return inner.Read(buffer, offset, count);
        }

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

    [Fact]
    public async Task TheBytesOfAbc_HashToTheKnownSha256()
    {
        string hash = await _serviceHash.Sha256Hex(new MemoryStream(Encoding.ASCII.GetBytes("abc")), TestContext.Current.CancellationToken);

        Assert.Equal("ba7816bf8f01cfea414140de5dae2223b00361a396177a9cb410ff61f20015ad", hash);
    }

    [Fact]
    public async Task AnEmptyStream_HashesToTheKnownSha256()
    {
        string hash = await _serviceHash.Sha256Hex(new MemoryStream(), TestContext.Current.CancellationToken);

        Assert.Equal("e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855", hash);
    }

    [Fact]
    public async Task TheOutput_IsLowerCaseHexOf64Characters()
    {
        string hash = await _serviceHash.Sha256Hex(new MemoryStream([1, 2, 3]), TestContext.Current.CancellationToken);

        Assert.Equal(64, hash.Length);
        Assert.Equal(hash.ToLowerInvariant(), hash);
        Assert.Matches("^[0-9a-f]{64}$", hash);
    }

    [Fact]
    public async Task A50MegabyteStream_HashesToTheSameValueAsSha256HashData_FromAForwardOnlyStream()
    {
        byte[] pattern = new byte[1024 * 1024];
        for (int i = 0; i < pattern.Length; i++) pattern[i] = (byte)(i * 31 + i / 251);

        byte[] all = new byte[50 * pattern.Length];
        for (int i = 0; i < 50; i++) pattern.CopyTo(all, i * pattern.Length);

        string hash = await _serviceHash.Sha256Hex(new ForwardOnlyStream(new MemoryStream(all)), TestContext.Current.CancellationToken);

        Assert.Equal(Convert.ToHexStringLower(SHA256.HashData(all)), hash);
    }

    [Fact]
    public async Task AStreamAlreadyReadPartway_HashesFromItsCurrentPosition()
    {
        using MemoryStream stream = new(Encoding.ASCII.GetBytes("xxabc"));
        stream.Position = 2;

        string hash = await _serviceHash.Sha256Hex(stream, TestContext.Current.CancellationToken);

        Assert.Equal("ba7816bf8f01cfea414140de5dae2223b00361a396177a9cb410ff61f20015ad", hash);
    }

    [Fact]
    public async Task ACancelledToken_ThrowsOperationCanceledException()
    {
        using CancellationTokenSource source = new();
        await source.CancelAsync();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => _serviceHash.Sha256Hex(new MemoryStream([1, 2, 3]), source.Token));
    }
}
