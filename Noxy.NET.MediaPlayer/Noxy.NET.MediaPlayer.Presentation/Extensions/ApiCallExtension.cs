using Noxy.NET.MediaPlayer.Presentation.Models;

namespace Noxy.NET.MediaPlayer.Presentation.Extensions;

public static class ApiCallExtension
{
    extension<T>(Func<Task<ApiResponse<T>>> call)
    {
        // Null when the server could not be reached. Any other failure propagates.
        public async Task<ApiResponse<T>?> RunOrNullWhenOffline()
        {
            try
            {
                return await call();
            }
            catch (Exception exception) when (exception.IsOfflineFailure)
            {
                return null;
            }
        }
    }
}
