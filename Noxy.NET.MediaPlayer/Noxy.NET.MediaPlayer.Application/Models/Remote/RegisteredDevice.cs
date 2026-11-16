namespace Noxy.NET.MediaPlayer.Application.Models.Remote;

public class RegisteredDevice
{
    public required Guid DeviceID { get; init; }
    public required string Name { get; set; }
    public required string ConnectionID { get; set; }
}
