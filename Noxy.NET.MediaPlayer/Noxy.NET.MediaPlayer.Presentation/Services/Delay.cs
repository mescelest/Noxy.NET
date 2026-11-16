using Noxy.NET.MediaPlayer.Presentation.Interfaces.Services;

namespace Noxy.NET.MediaPlayer.Presentation.Services;

public class Delay : IDelay
{
    public async Task Wait(TimeSpan time, CancellationToken cancellationToken)
    {
        await Task.Delay(time, cancellationToken);
    }
}
