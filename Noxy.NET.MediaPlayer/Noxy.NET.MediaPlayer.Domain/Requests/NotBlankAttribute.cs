using System.ComponentModel.DataAnnotations;

namespace Noxy.NET.MediaPlayer.Domain.Requests;

[AttributeUsage(AttributeTargets.Property)]
public class NotBlankAttribute() : ValidationAttribute("The field {0} can't be empty.")
{
    public override bool IsValid(object? value)
    {
        return value is string text && !string.IsNullOrWhiteSpace(text);
    }
}
