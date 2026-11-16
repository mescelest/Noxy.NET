using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.SignalR;

namespace Noxy.NET.MediaPlayer.API.Extensions;

public static class ValidatableExtension
{
    extension<T>(T value) where T : class
    {
        // Hub methods are not validated by the framework, so the message is checked by hand. Fails with a HubException the caller sees.
        public T EnsureValid(IServiceProvider? serviceProvider = null)
        {
            List<ValidationResult> resultList = [];
            return Validator.TryValidateObject(value, new(value, serviceProvider, null), resultList, true)
                ? value
                : throw new HubException(string.Join(' ', resultList.Select(x => x.ErrorMessage)));
        }
    }
}
