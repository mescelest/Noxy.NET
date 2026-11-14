using System.ComponentModel.DataAnnotations;
using Noxy.NET.MediaPlayer.Application.Configurations;

namespace Noxy.NET.Test.MediaPlayer.Upload;

public class ConfigUploadTest
{
    private static bool IsValid(ConfigUpload config)
    {
        return Validator.TryValidateObject(config, new(config), [], true);
    }

    [Fact]
    public void Defaults_AreValidAndPendingCapIs100()
    {
        ConfigUpload config = new();

        Assert.True(IsValid(config));
        Assert.Equal(100, config.MaxPendingPerUser);
        Assert.Equal(30, config.PollSeconds);
    }

    [Fact]
    public void ZeroPendingCap_Fails()
    {
        Assert.False(IsValid(new() { MaxPendingPerUser = 0 }));
    }

    [Fact]
    public void ZeroPollSeconds_Fails()
    {
        Assert.False(IsValid(new() { PollSeconds = 0 }));
    }
}
