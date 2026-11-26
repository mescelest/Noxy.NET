using Noxy.NET.MediaPlayer.Presentation.Declarations;
using Noxy.NET.MediaPlayer.Presentation.Services;
using Noxy.NET.Test.MediaPlayer.Fakes;

namespace Noxy.NET.Test.MediaPlayer.Presentation;

public class HotkeyServiceTest
{
    private readonly FakePlaybackControlService _control = new();
    private readonly FakeMediaKeyService _media = new();
    private readonly HotkeyService _hotkey;

    public HotkeyServiceTest()
    {
        _hotkey = new(_control, [_media], new FakeLogger<HotkeyService>());
    }

    // The media keys are raised without anyone to wait for them.
    private async Task WaitForCalls(int count)
    {
        DateTime deadline = DateTime.UtcNow.AddSeconds(10);
        while (_control.CallList.Count < count)
        {
            Assert.True(DateTime.UtcNow < deadline, "Timed out waiting for the call.");
            await Task.Delay(5, TestContext.Current.CancellationToken);
        }
    }

    [Fact]
    public async Task Space_TogglesPlay()
    {
        bool handled = await _hotkey.Press(HotkeyCatalog.Space.Key);

        Assert.True(handled);
        Assert.Equal(["TogglePlay"], _control.CallList);
    }

    [Fact]
    public async Task APressedKeyWithNoBinding_IsNotHandled_AndCallsNothing()
    {
        bool handled = await _hotkey.Press("KeyA");

        Assert.False(handled);
        Assert.Empty(_control.CallList);
    }

    [Fact]
    public async Task EachMediaKey_CallsItsAction()
    {
        _media.RaisePlayPause();
        await WaitForCalls(1);
        _media.RaiseNext();
        await WaitForCalls(2);
        _media.RaisePrevious();
        await WaitForCalls(3);

        Assert.Equal(["TogglePlay", "Next", "Previous"], _control.CallList);
    }

    [Fact]
    public async Task TheStopMediaKey_Pauses_AndReturnsToTheStart()
    {
        _media.RaiseStop();
        await WaitForCalls(2);

        Assert.Equal(["Pause", $"Seek {TimeSpan.Zero}"], _control.CallList);
    }

    [Fact]
    public async Task APlatformWithNoMediaKeys_StillHandlesSpace()
    {
        HotkeyService hotkey = new(_control, [], new FakeLogger<HotkeyService>());

        Assert.True(await hotkey.Press(HotkeyCatalog.Space.Key));
    }

    [Fact]
    public void TheCatalog_HasNoDuplicateKey()
    {
        Assert.Equal(HotkeyCatalog.HotkeyList.Count, HotkeyCatalog.HotkeyList.Select(x => x.Key).Distinct().Count());
    }
}
