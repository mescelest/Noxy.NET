namespace Noxy.NET.MediaPlayer.Domain.Exceptions;

public class InvalidSettingValueException(string key) : Exception($"The value is not valid for the setting {key}.")
{
    public string Key { get; } = key;
}
