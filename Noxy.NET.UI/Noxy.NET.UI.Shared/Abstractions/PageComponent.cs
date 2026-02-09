namespace Noxy.NET.UI.Abstractions;

public abstract class PageComponent : BlazorComponent, IDisposable
{
    private IDisposable? _pageLoadScope;

    protected bool IsLoading { get; set; }

    protected override string CssClass => CombineCssClass(base.CssClass, "Page");

    public virtual void Dispose()
    {
        PageLoadingService.StateChanged -= OnLoadingChanged;
        GC.SuppressFinalize(this);
    }

    protected override void OnInitialized()
    {
        PageLoadingService.StateChanged += OnLoadingChanged;
        _pageLoadScope = PageLoadingService.BeginOperation();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);
        if (!firstRender) return;

        await PageLoadingService.WaitForIdleExcept(_pageLoadScope!);

        _pageLoadScope!.Dispose();
        StateHasChanged();
    }

    protected async Task WithPageLoading(Func<Task> action)
    {
        using IDisposable scope = PageLoadingService.BeginOperation();
        await action();
        await PageLoadingService.WaitForIdleExcept(scope);
        StateHasChanged();
    }

    private void OnLoadingChanged()
    {
        IsLoading = PageLoadingService.IsLoading;
        InvokeAsync(StateHasChanged);
    }
}
