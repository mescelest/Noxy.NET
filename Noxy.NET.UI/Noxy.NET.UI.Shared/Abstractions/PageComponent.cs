using Noxy.NET.Models;

namespace Noxy.NET.UI.Abstractions;

public abstract class PageComponent : BlazorComponent, IDisposable
{
    protected override string CssClass => CombineCssClass(base.CssClass, "Page");

    public virtual void Dispose()
    {
        LoadingService.OnChange -= OnChangeHandler;
        GC.SuppressFinalize(this);
    }

    protected override void OnInitialized()
    {
        base.OnInitialized();

        LoadingService.Reset();
        LoadingService.OnChange += OnChangeHandler;
    }

    private void OnChangeHandler(string sender, GenericEventArgs<bool> args)
    {
        if (sender != UUIDString || args.Value == IsLoading) return;

        IsLoading = args.Value;
        InvokeAsync(StateHasChanged);
    }
}
