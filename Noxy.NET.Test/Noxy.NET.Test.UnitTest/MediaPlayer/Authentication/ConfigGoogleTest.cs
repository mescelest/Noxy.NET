using Noxy.NET.MediaPlayer.Application.Configurations;

namespace Noxy.NET.Test.MediaPlayer.Authentication;

public class ConfigGoogleTest
{
    [Fact]
    public void Default_HasNoClientIDs_SoGoogleIsOff()
    {
        ConfigGoogle config = new();

        Assert.Empty(config.ClientIdList);
        Assert.Empty(config.ConfiguredClientIdList);
    }

    [Fact]
    public void ConfiguredClientIdList_DropsBlankEntriesAndTrimsTheRest()
    {
        ConfigGoogle config = new() { ClientIdList = ["", "  ", " android.apps.googleusercontent.com ", "desktop.apps.googleusercontent.com"] };

        Assert.Equal(["android.apps.googleusercontent.com", "desktop.apps.googleusercontent.com"], config.ConfiguredClientIdList);
    }

    [Fact]
    public void ConfiguredClientIdList_OfOnlyBlanks_IsEmpty()
    {
        Assert.Empty(new ConfigGoogle { ClientIdList = ["", " "] }.ConfiguredClientIdList);
    }
}
