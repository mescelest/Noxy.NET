using Noxy.NET.MediaPlayer.Storage.Extensions;

namespace Noxy.NET.Test.MediaPlayer.Presentation;

public class IOExceptionExtensionTest
{
    [Theory]
    [InlineData(unchecked((int)0x80070070))]
    [InlineData(unchecked((int)0x80070027))]
    [InlineData(28)]
    public void TheDiskFullCodes_AreDiskFull(int hResult)
    {
        Assert.True(new IOException("full", hResult).IsDiskFull);
    }

    [Theory]
    [InlineData(unchecked((int)0x80070020))]
    [InlineData(5)]
    [InlineData(0)]
    public void AnyOtherCode_IsNotDiskFull(int hResult)
    {
        Assert.False(new IOException("other", hResult).IsDiskFull);
    }
}
