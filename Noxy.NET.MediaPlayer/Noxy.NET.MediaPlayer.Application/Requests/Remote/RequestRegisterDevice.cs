using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Options;
using Noxy.NET.MediaPlayer.Application.Configurations;

namespace Noxy.NET.MediaPlayer.Application.Requests.Remote;

public class RequestRegisterDevice : IValidatableObject
{
    private readonly string _name = string.Empty;

    public required Guid DeviceID { get; init; }

    public required string? Name { get => _name; init => _name = value?.Trim() ?? string.Empty; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        // The limit is the configured one when the request is validated inside the app, the default one otherwise.
        int maxLength = (validationContext.GetService(typeof(IOptions<ConfigRemote>)) as IOptions<ConfigRemote>)?.Value.MaxDeviceNameLength ?? new ConfigRemote().MaxDeviceNameLength;
        List<ValidationResult> errorList = [];

        if (DeviceID == Guid.Empty) errorList.Add(new("The device ID is required.", [nameof(DeviceID)]));
        if (Name.Length == 0 || Name.Length > maxLength) errorList.Add(new($"The name must be 1 to {maxLength} characters.", [nameof(Name)]));

        return errorList;
    }
}
