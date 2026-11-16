using Microsoft.Extensions.Options;
using Noxy.NET.MediaPlayer.Application.Configurations;
using Noxy.NET.MediaPlayer.Application.Notifications.Remote;
using Noxy.NET.MediaPlayer.Application.Requests.Remote;
using Noxy.NET.MediaPlayer.Application.Services;
using Noxy.NET.MediaPlayer.Domain.Exceptions;
using Noxy.NET.MediaPlayer.Domain.Responses.Remote;
using Noxy.NET.Test.MediaPlayer.Fakes;

namespace Noxy.NET.Test.MediaPlayer.Remote;

public class DeviceRegistryServiceTest
{
    private static readonly Guid UserID = Guid.CreateVersion7();

    private readonly FakePublisher _publisher = new();
    private readonly DeviceRegistryService _service;

    public DeviceRegistryServiceTest()
    {
        _service = new(_publisher, Options.Create(new ConfigRemote { MaxDeviceCount = 3 }));
    }

    private static RequestRegisterDevice Request(Guid? deviceID = null, string name = "Laptop")
    {
        return new() { DeviceID = deviceID ?? Guid.CreateVersion7(), Name = name };
    }

    [Fact]
    public async Task Register_AddsTheDevice_AndPublishesTheList()
    {
        RequestRegisterDevice request = Request();

        List<ResponseDevice> list = await _service.Register(UserID, request, "c1");

        ResponseDevice device = Assert.Single(list);
        Assert.Equal(request.DeviceID, device.DeviceID);
        Assert.Equal("Laptop", device.Name);
        Assert.False(device.IsActive);
        NotificationDeviceListChanged published = Assert.Single(_publisher.GetList<NotificationDeviceListChanged>());
        Assert.Equal(UserID, published.UserID);
        Assert.Equal(request.DeviceID, Assert.Single(published.DeviceList).DeviceID);
    }

    [Fact]
    public async Task Register_TheSameDeviceAgain_ReplacesTheConnection_AndKeepsOneEntry()
    {
        RequestRegisterDevice request = Request();
        await _service.Register(UserID, request, "c1");

        List<ResponseDevice> list = await _service.Register(UserID, request, "c2");

        Assert.Single(list);
        Assert.Equal("c2", _service.GetConnectionID(UserID, request.DeviceID));
        Assert.Null(_service.Find("c1"));
        Assert.Equal((UserID, request.DeviceID), _service.Find("c2"));
        Assert.Single(_publisher.GetList<NotificationDeviceListChanged>());
    }

    [Fact]
    public async Task Register_AnExistingDeviceWithANewName_RenamesIt()
    {
        Guid deviceID = Guid.CreateVersion7();
        await _service.Register(UserID, Request(deviceID), "c1");

        List<ResponseDevice> list = await _service.Register(UserID, Request(deviceID, "Phone"), "c2");

        Assert.Equal("Phone", Assert.Single(list).Name);
        Assert.Equal(2, _publisher.GetList<NotificationDeviceListChanged>().Count);
    }

    [Fact]
    public async Task Register_AboveTheDeviceLimit_Throws_AndAnExistingDeviceCanStillReconnect()
    {
        RequestRegisterDevice first = Request();
        await _service.Register(UserID, first, "c1");
        await _service.Register(UserID, Request(), "c2");
        await _service.Register(UserID, Request(), "c3");

        await Assert.ThrowsAsync<TooManyDevicesException>(() => _service.Register(UserID, Request(), "c4"));
        List<ResponseDevice> list = await _service.Register(UserID, first, "c5");

        Assert.Equal(3, list.Count);
        Assert.Null(_service.Find("c4"));
    }

    [Fact]
    public async Task Disconnect_RemovesTheDevice_PublishesTheList_AndRaisesDeviceDisconnected()
    {
        RequestRegisterDevice request = Request();
        await _service.Register(UserID, request, "c1");
        List<(Guid, Guid)> raisedList = [];
        _service.DeviceDisconnected += (user, device) =>
        {
            raisedList.Add((user, device));
            return Task.CompletedTask;
        };

        await _service.Disconnect("c1");

        Assert.False(_service.IsOnline(UserID, request.DeviceID));
        Assert.Empty(_service.GetDeviceList(UserID, null));
        Assert.Equal([(UserID, request.DeviceID)], raisedList);
        Assert.Empty(_publisher.GetList<NotificationDeviceListChanged>().Last().DeviceList);
    }

    [Fact]
    public async Task Disconnect_OfAReplacedOldConnection_ChangesNothing()
    {
        RequestRegisterDevice request = Request();
        await _service.Register(UserID, request, "c1");
        await _service.Register(UserID, request, "c2");
        int published = _publisher.PublishedList.Count;
        bool raised = false;
        _service.DeviceDisconnected += (_, _) =>
        {
            raised = true;
            return Task.CompletedTask;
        };

        await _service.Disconnect("c1");

        Assert.True(_service.IsOnline(UserID, request.DeviceID));
        Assert.Equal(published, _publisher.PublishedList.Count);
        Assert.False(raised);
    }

    [Fact]
    public async Task Disconnect_OfAnUnknownConnection_ChangesNothing()
    {
        await _service.Disconnect("nobody");

        Assert.Empty(_publisher.PublishedList);
    }

    [Fact]
    public async Task Devices_OfTwoAccounts_NeverMix()
    {
        Guid other = Guid.CreateVersion7();
        RequestRegisterDevice mine = Request();
        RequestRegisterDevice theirs = Request();
        await _service.Register(UserID, mine, "c1");
        await _service.Register(other, theirs, "c2");

        Assert.Equal(mine.DeviceID, Assert.Single(_service.GetDeviceList(UserID, null)).DeviceID);
        Assert.Equal(theirs.DeviceID, Assert.Single(_service.GetDeviceList(other, null)).DeviceID);
        Assert.False(_service.IsOnline(UserID, theirs.DeviceID));
        Assert.Null(_service.GetConnectionID(UserID, theirs.DeviceID));
        Assert.All(_publisher.GetList<NotificationDeviceListChanged>().Where(x => x.UserID == other), x => Assert.DoesNotContain(x.DeviceList, y => y.DeviceID == mine.DeviceID));
    }

    [Fact]
    public async Task Find_ByConnectionID_ReturnsTheUserAndDevice_AndNullForAnUnknownOne()
    {
        RequestRegisterDevice request = Request();
        await _service.Register(UserID, request, "c1");

        Assert.Equal((UserID, request.DeviceID), _service.Find("c1"));
        Assert.Null(_service.Find("c9"));
    }

    [Fact]
    public async Task IsOnline_IsFalseAfterDisconnect()
    {
        RequestRegisterDevice request = Request();
        await _service.Register(UserID, request, "c1");
        Assert.True(_service.IsOnline(UserID, request.DeviceID));

        await _service.Disconnect("c1");

        Assert.False(_service.IsOnline(UserID, request.DeviceID));
    }

    [Fact]
    public async Task TheListMarksTheActiveDevice_AndThePublishedListUsesTheResolver()
    {
        RequestRegisterDevice one = Request();
        RequestRegisterDevice two = Request();
        _service.ActiveDeviceResolver = _ => two.DeviceID;
        await _service.Register(UserID, one, "c1");
        await _service.Register(UserID, two, "c2");

        List<ResponseDevice> list = _service.GetDeviceList(UserID, two.DeviceID);

        Assert.Equal(two.DeviceID, Assert.Single(list, x => x.IsActive).DeviceID);
        Assert.Equal(two.DeviceID, Assert.Single(_publisher.GetList<NotificationDeviceListChanged>().Last().DeviceList, x => x.IsActive).DeviceID);
    }

    [Fact]
    public async Task ConcurrentRegistrations_AreAllKept()
    {
        await Task.WhenAll(Enumerable.Range(0, 3).Select(index => Task.Run(() => _service.Register(UserID, Request(), $"c{index}"), TestContext.Current.CancellationToken)));

        Assert.Equal(3, _service.GetDeviceList(UserID, null).Count);
    }

    [Fact]
    public async Task Register_ASecondDeviceOnTheSameConnection_ReplacesTheFirst_AndRaisesItsDisconnect()
    {
        RequestRegisterDevice first = Request();
        RequestRegisterDevice second = Request();
        await _service.Register(UserID, first, "c1");
        List<Guid> raisedList = [];
        _service.DeviceDisconnected += (_, device) =>
        {
            raisedList.Add(device);
            return Task.CompletedTask;
        };

        List<ResponseDevice> list = await _service.Register(UserID, second, "c1");

        Assert.Equal(second.DeviceID, Assert.Single(list).DeviceID);
        Assert.False(_service.IsOnline(UserID, first.DeviceID));
        Assert.Equal([first.DeviceID], raisedList);
        await _service.Disconnect("c1");
        Assert.Empty(_service.GetDeviceList(UserID, null));
    }

    [Fact]
    public async Task Disconnect_WhenThePublishThrows_StillRaisesDeviceDisconnected()
    {
        RequestRegisterDevice request = Request();
        await _service.Register(UserID, request, "c1");
        bool raised = false;
        _service.DeviceDisconnected += (_, _) =>
        {
            raised = true;
            return Task.CompletedTask;
        };
        _publisher.OnPublish = _ => throw new InvalidOperationException("The push failed.");

        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.Disconnect("c1"));

        Assert.True(raised);
    }
}
