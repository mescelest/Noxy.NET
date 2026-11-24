using Microsoft.JSInterop;
using Noxy.NET.UI.Components.Elements;
using Noxy.NET.UI.Extensions;
using Noxy.NET.UI.Interfaces;

namespace Noxy.NET.UI.Services;

public class ThemeService<TType>(IJSRuntime runtime) : IThemeService<TType>, IAsyncDisposable where TType : struct, Enum
{
    private const string FunctionGet = "GetTheme";
    private const string FunctionApply = "ApplyTheme";
    private const string FunctionSet = "SetTheme";

    private IJSObjectReference? Module { get; set; }

    private TType? CurrentTheme { get; set; }

    public event EventHandler<TType?>? OnThemeChange;

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
            // The window is closing.
        }
        catch (JSException)
        {
            // The page was reloaded, so the object is gone.
        }
    }

    public async Task<TType?> LoadTheme()
    {
        IJSObjectReference module = await GetModule();
        string? result = await module.InvokeAsync<string?>(FunctionGet);
        TType? theme = Enum.TryParse(result, true, out TType value) ? value : null;

        await module.InvokeVoidAsync(FunctionApply, theme?.ToString());
        Change(theme);

        return theme;
    }

    public TType? GetTheme() => CurrentTheme;

    public async Task SetTheme(TType? type)
    {
        IJSObjectReference module = await GetModule();
        await module.InvokeVoidAsync(FunctionSet, type?.ToString());
        Change(type);
    }

    private void Change(TType? theme)
    {
        CurrentTheme = theme;
        OnThemeChange?.Invoke(this, theme);
    }

    private async Task<IJSObjectReference> GetModule()
    {
        return Module ??= await runtime.InvokeAsync<IJSObjectReference>("import", runtime.GetComponentPath(typeof(ThemeToggle<>)));
    }
}
