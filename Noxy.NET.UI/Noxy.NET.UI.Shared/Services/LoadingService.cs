using Noxy.NET.Models;

namespace Noxy.NET.UI.Services;

public class LoadingService
{
    private int LoadingCount { get; set; } = 0;
    public event EventHandler<string, GenericEventArgs<bool>>? OnChange;

    public int StartLoading(string key)
    {
        if (++LoadingCount > 0)
        {
            OnChange?.Invoke(key, new(true));
        }

        return LoadingCount;
    }

    public void Reset()
    {
        LoadingCount = 0;
    }

    public int FinishLoading(string key)
    {
        if (--LoadingCount == 0)
        {
            OnChange?.Invoke(key, new(false));
        }

        return LoadingCount;
    }
}
