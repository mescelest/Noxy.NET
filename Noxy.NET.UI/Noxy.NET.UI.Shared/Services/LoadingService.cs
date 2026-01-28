using Noxy.NET.Models;

namespace Noxy.NET.UI.Services;

public class LoadingService
{
    private Dictionary<string, int> LoadingCollection { get; set; } = [];
    public event EventHandler<string, GenericEventArgs<bool>>? OnChange;

    public int StartLoading(string key)
    {
        if (LoadingCollection.TryGetValue(key, out int value)) return ++value;

        value = LoadingCollection[key] = 0;
        OnChange?.Invoke(key, new(true));

        return ++value;
    }

    public int FinishLoading(string key)
    {
        if (!LoadingCollection.TryGetValue(key, out int value)) throw new InvalidOperationException();
        if (--value == 0) OnChange?.Invoke(key, new(false));
        return value;
    }
}
