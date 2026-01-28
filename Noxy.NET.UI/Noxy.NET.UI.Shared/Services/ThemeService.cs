using Microsoft.JSInterop;
using Noxy.NET.UI.Interfaces;

namespace Noxy.NET.UI.Services;

public class ThemeService<TType>(IJSRuntime runtime) : IThemeService<TType>, IAsyncDisposable where TType : struct, Enum
{
    private IJSObjectReference? Module { get; set; }

    private TType? CurrentTheme { get; set; }

    public async ValueTask DisposeAsync()
    {
        try
        {
            GC.SuppressFinalize(this);
            if (Module is not null)
            {
                await Module.DisposeAsync();
            }
        }
        catch (JSDisconnectedException)
        {
        }
    }

    public async Task<TType?> LoadTheme()
    {
        IJSObjectReference module = await GetInterop();
        string result = await module.InvokeAsync<string>($"{Constants.InteropNameUICommon}.GetTheme");
        await SetTheme(Enum.TryParse(result, true, out TType value) ? value : null);
        OnThemeChange?.Invoke(this, CurrentTheme);
        return CurrentTheme;
    }

    public TType? GetTheme() => CurrentTheme;

    public async Task SetTheme(TType? type)
    {
        IJSObjectReference module = await GetInterop();
        await module.InvokeVoidAsync($"{Constants.InteropNameUICommon}.SetTheme", type.ToString());
        OnThemeChange?.Invoke(this, CurrentTheme = type);
    }

    public event EventHandler<TType?>? OnThemeChange;

    private async Task<IJSObjectReference> GetInterop()
    {
        return Module ??= await runtime.InvokeAsync<IJSObjectReference>("import", $"./_content/{Constants.AssemblyNameUICommon}/Interop.js");
    }
}
