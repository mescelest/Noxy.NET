using Noxy.NET.MediaPlayer.Presentation.Services;
using Noxy.NET.Test.MediaPlayer.Fakes;

namespace Noxy.NET.Test.MediaPlayer.Presentation;

public class DeviceServiceTest
{
    private readonly FakeDeviceStore _storeDevice = new();

    [Fact]
    public async Task GetDeviceID_IsCreatedOnce_AndStable_AcrossServiceInstances()
    {
        Guid first = await new DeviceService(_storeDevice).GetDeviceID();
        Guid second = await new DeviceService(_storeDevice).GetDeviceID();
        Guid third = await new DeviceService(_storeDevice).GetDeviceID();

        Assert.NotEqual(Guid.Empty, first);
        Assert.Equal(first, second);
        Assert.Equal(first, third);
    }

    [Fact]
    public async Task GetName_DefaultsToTheMachineName()
    {
        Assert.Equal(Environment.MachineName, await new DeviceService(_storeDevice).GetName());
    }

    [Fact]
    public async Task SetName_TrimsAndPersists()
    {
        await new DeviceService(_storeDevice).SetName("  Kitchen laptop  ");

        Assert.Equal("Kitchen laptop", await new DeviceService(_storeDevice).GetName());
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task SetName_Empty_Throws(string name)
    {
        await Assert.ThrowsAsync<ArgumentException>(() => new DeviceService(_storeDevice).SetName(name));
    }

    [Fact]
    public async Task SetName_Over50Characters_Throws_AndFiftyIsAccepted()
    {
        DeviceService service = new(_storeDevice);

        await Assert.ThrowsAsync<ArgumentException>(() => service.SetName(new string('a', 51)));
        await service.SetName(new string('a', 50));

        Assert.Equal(new string('a', 50), await service.GetName());
    }

    [Fact]
    public async Task SetName_Over50Characters_OnlyBecauseOfSurroundingSpaces_IsAccepted()
    {
        DeviceService service = new(_storeDevice);

        await service.SetName($" {new string('a', 50)} ");

        Assert.Equal(new string('a', 50), await service.GetName());
    }
}
