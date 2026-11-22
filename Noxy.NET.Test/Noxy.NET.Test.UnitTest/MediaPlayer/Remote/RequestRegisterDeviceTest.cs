using System.ComponentModel.DataAnnotations;
using Noxy.NET.MediaPlayer.Application.Configurations;
using Noxy.NET.MediaPlayer.Application.Requests.Remote;
using Noxy.NET.MediaPlayer.Domain.Enums;
using Noxy.NET.MediaPlayer.Domain.Exceptions;

namespace Noxy.NET.Test.MediaPlayer.Remote;

public class RequestRegisterDeviceTest
{
    private static List<ValidationResult> Validate(object request)
    {
        List<ValidationResult> results = [];
        Validator.TryValidateObject(request, new(request), results, true);
        return results;
    }

    [Fact]
    public void ANormalRequest_IsValid()
    {
        Assert.Empty(Validate(new RequestRegisterDevice { DeviceID = Guid.CreateVersion7(), Name = "Laptop" }));
    }

    [Fact]
    public void AnEmptyID_AnEmptyName_OrATooLongName_IsInvalid()
    {
        Assert.NotEmpty(Validate(new RequestRegisterDevice { DeviceID = Guid.Empty, Name = "Laptop" }));
        Assert.NotEmpty(Validate(new RequestRegisterDevice { DeviceID = Guid.CreateVersion7(), Name = string.Empty }));
        Assert.NotEmpty(Validate(new RequestRegisterDevice { DeviceID = Guid.CreateVersion7(), Name = "   " }));
        Assert.NotEmpty(Validate(new RequestRegisterDevice { DeviceID = Guid.CreateVersion7(), Name = new('a', new ConfigRemote().MaxDeviceNameLength + 1) }));
        Assert.Empty(Validate(new RequestRegisterDevice { DeviceID = Guid.CreateVersion7(), Name = new('a', new ConfigRemote().MaxDeviceNameLength) }));
    }

    [Fact]
    public void ANullName_IsInvalid_InsteadOfFailingWithANullReference()
    {
        Assert.NotEmpty(Validate(new RequestRegisterDevice { DeviceID = Guid.CreateVersion7(), Name = null }));
    }

    [Fact]
    public void TheNameIsTrimmed_BeforeItIsMeasured()
    {
        Assert.Empty(Validate(new RequestRegisterDevice { DeviceID = Guid.CreateVersion7(), Name = $" {new string('a', new ConfigRemote().MaxDeviceNameLength)} " }));
    }

    [Fact]
    public void TheStateReport_RejectsAVolumeOutsideZeroTo100_AndANegativePosition()
    {
        RequestReportPlayerState Report(int volume, double position)
        {
            return new() { StateRevision = 1, IsPlaying = true, PositionSeconds = position, Volume = volume, Shuffle = ShuffleModeEnum.Off, Repeat = RepeatModeEnum.No };
        }

        Assert.Empty(Validate(Report(50, 0)));
        Assert.NotEmpty(Validate(Report(101, 0)));
        Assert.NotEmpty(Validate(Report(-1, 0)));
        Assert.NotEmpty(Validate(Report(50, -0.5)));
    }

    [Fact]
    public void ConfigRemote_HasTheDocumentedDefaults()
    {
        ConfigRemote config = new();

        Assert.Equal([10, 5, 10, 10, 50, 50, 20000, 4194304],
            [config.ActiveGraceSeconds, config.TransferTimeoutSeconds, (int)config.IdleTeardownMinutes, config.MaxDeviceCount, config.MaxDeviceNameLength, config.HistoryLimit, config.MaxQueueLength, config.MaxHubMessageBytes]);
        Assert.Empty(Validate(config));
        Assert.Equal("Remote", ConfigRemote.SectionName);
    }

    [Fact]
    public void TheNewExceptions_AreDistinctTypes()
    {
        Assert.IsAssignableFrom<Exception>(new InvalidQueueEditException("x"));
        Assert.IsAssignableFrom<Exception>(new TooManyDevicesException());
    }
}
