using Microsoft.Extensions.Logging;
using Noxy.NET.MediaPlayer.Presentation;
using Noxy.NET.MediaPlayer.Presentation.Features.Shell;
using Noxy.NET.Test.MediaPlayer.Fakes;

namespace Noxy.NET.Test.MediaPlayer.Presentation;

public class FeatureShellEffectsTest
{
    private readonly FakeDevicePreferenceStorage _storage = new();
    private readonly FakeDispatcher _dispatcher = new();
    private readonly FakeLogger<FeatureShellEffects> _logger = new();
    private readonly FeatureShellEffects _effects;

    public FeatureShellEffectsTest()
    {
        _effects = new(_storage, _logger);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task SidebarSet_IsStored_AndRestoredOnStart(bool open)
    {
        await _effects.StoreSidebar(new(open), _dispatcher);

        await _effects.RestoreSidebar(new(), _dispatcher);

        Assert.Equal(open, Assert.Single(_dispatcher.Of<ActionShellSidebarRestored>()).SidebarOpen);
    }

    [Fact]
    public async Task AMissingStoredValue_MeansOpen()
    {
        await _effects.RestoreSidebar(new(), _dispatcher);

        Assert.True(Assert.Single(_dispatcher.Of<ActionShellSidebarRestored>()).SidebarOpen);
    }

    [Theory]
    [InlineData("")]
    [InlineData("maybe")]
    [InlineData("0")]
    public async Task AnUnreadableStoredValue_MeansOpen(string stored)
    {
        await _storage.Set(Constants.SidebarOpenKey, stored);

        await _effects.RestoreSidebar(new(), _dispatcher);

        Assert.True(Assert.Single(_dispatcher.Of<ActionShellSidebarRestored>()).SidebarOpen);
    }

    [Fact]
    public async Task ARestoreThatFails_MeansOpen_AndIsLoggedByTypeOnly()
    {
        _storage.Failure = new InvalidOperationException("secret detail");

        await _effects.RestoreSidebar(new(), _dispatcher);

        Assert.True(Assert.Single(_dispatcher.Of<ActionShellSidebarRestored>()).SidebarOpen);
        Assert.Contains(_logger.EntryList, x => x.Level == LogLevel.Warning && x.Message.Contains(nameof(InvalidOperationException)));
        Assert.DoesNotContain(_logger.EntryList, x => x.Message.Contains("secret"));
    }

    [Fact]
    public async Task SidebarSet_WhenStoringFails_DoesNotThrow_AndIsLoggedByTypeOnly()
    {
        _storage.Failure = new IOException("secret detail");

        await _effects.StoreSidebar(new(false), _dispatcher);

        Assert.Contains(_logger.EntryList, x => x.Level == LogLevel.Warning && x.Message.Contains(nameof(IOException)));
        Assert.DoesNotContain(_logger.EntryList, x => x.Message.Contains("secret"));
        Assert.Empty(_dispatcher.ActionList);
    }

    [Fact]
    public void SidebarRestored_SetsTheState()
    {
        Assert.False(FeatureShellReducers.ReduceSidebarRestored(new(), new(false)).SidebarOpen);
        Assert.True(FeatureShellReducers.ReduceSidebarRestored(new() { SidebarOpen = false }, new(true)).SidebarOpen);
    }
}
