using Noxy.NET.MediaPlayer.Presentation;
using Noxy.NET.MediaPlayer.Presentation.Features.Shell;
using Noxy.NET.MediaPlayer.Presentation.Services;
using Noxy.NET.Test.MediaPlayer.Fakes;

namespace Noxy.NET.Test.MediaPlayer.Presentation;

public class DeviceResetServiceTest
{
    private readonly FakeSessionService _serviceSession = new();
    private readonly FakeLocalDataOwnerService _serviceLocalDataOwner = new();
    private readonly FakeDevicePreferenceStorage _storageDevicePreference = new();
    private readonly FakeServerAddressStorage _storageServerAddress = new("https://one.example");
    private readonly FakeLocalQueueStore _storeLocalQueue = new();
    private readonly FakeDispatcher _dispatcher = new();
    private readonly DeviceResetService _service;

    public DeviceResetServiceTest()
    {
        _service = new(_serviceSession, _serviceLocalDataOwner, _storageDevicePreference, _storageServerAddress, _storeLocalQueue, _dispatcher);
    }

    [Fact]
    public async Task ResetPreferences_RemovesTheSidebarChoice_AndOpensTheSidebar()
    {
        await _storageDevicePreference.Set(PresentationConstants.SidebarOpenKey, "false");

        await _service.ResetPreferences();

        Assert.Null(await _storageDevicePreference.Get(PresentationConstants.SidebarOpenKey));
        Assert.True(Assert.Single(_dispatcher.Of<ActionShellSidebarRestored>()).SidebarOpen);
    }

    [Fact]
    public async Task ResetPreferences_KeepsTheSession_TheServerAddresses_AndTheData()
    {
        await _service.ResetPreferences();

        Assert.True(_serviceSession.IsSignedIn);
        Assert.Equal("https://one.example", await _storageServerAddress.GetAddress());
        Assert.Equal(0, _serviceLocalDataOwner.WipeLocalDataCount);
        Assert.Equal(0, _storeLocalQueue.ClearCount);
    }

    [Fact]
    public async Task DeleteAllData_SignsOut_AndWipesTheLocalData()
    {
        await _service.DeleteAllData();

        Assert.False(_serviceSession.IsSignedIn);
        Assert.Equal(1, _serviceLocalDataOwner.WipeLocalDataCount);
        Assert.Equal(1, _storeLocalQueue.ClearCount);
    }

    [Fact]
    public async Task DeleteAllData_RemovesEverySavedServerAddress()
    {
        await _storageServerAddress.SetAddress("https://two.example");

        await _service.DeleteAllData();

        Assert.Null(await _storageServerAddress.GetAddress());
        Assert.Empty(await _storageServerAddress.GetAddressList());
    }

    [Fact]
    public async Task DeleteAllData_ResetsThePreferences()
    {
        await _storageDevicePreference.Set(PresentationConstants.SidebarOpenKey, "false");

        await _service.DeleteAllData();

        Assert.Null(await _storageDevicePreference.Get(PresentationConstants.SidebarOpenKey));
        Assert.True(Assert.Single(_dispatcher.Of<ActionShellSidebarRestored>()).SidebarOpen);
    }

    [Fact]
    public async Task DeleteAllData_SignsOutBeforeTheDataIsWiped()
    {
        bool wasSignedInAtWipe = true;
        _serviceLocalDataOwner.OnWipeLocalData = () => wasSignedInAtWipe = _serviceSession.IsSignedIn;

        await _service.DeleteAllData();

        Assert.False(wasSignedInAtWipe);
    }
}
