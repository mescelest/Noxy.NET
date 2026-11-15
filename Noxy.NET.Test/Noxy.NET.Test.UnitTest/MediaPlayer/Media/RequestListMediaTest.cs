using System.ComponentModel.DataAnnotations;
using Noxy.NET.MediaPlayer.Application.Requests.Media;

namespace Noxy.NET.Test.MediaPlayer.Media;

public class RequestListMediaTest
{
    private static bool IsValid(RequestListMedia request)
    {
        return Validator.TryValidateObject(request, new(request), [], true);
    }

    [Fact]
    public void Defaults_AreTheFirstPageOf50()
    {
        RequestListMedia request = new();

        Assert.True(IsValid(request));
        Assert.Equal(0, request.Skip);
        Assert.Equal(50, request.Take);
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(0, 100)]
    [InlineData(5000, 50)]
    public void InRange_IsValid(int skip, int take)
    {
        Assert.True(IsValid(new() { Skip = skip, Take = take }));
    }

    [Theory]
    [InlineData(-1, 50)]
    [InlineData(0, 0)]
    [InlineData(0, -1)]
    [InlineData(0, 101)]
    public void OutOfRange_IsInvalid(int skip, int take)
    {
        Assert.False(IsValid(new() { Skip = skip, Take = take }));
    }
}
