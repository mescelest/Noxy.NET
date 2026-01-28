using Microsoft.AspNetCore.Components;
using Noxy.NET.Models;
using Noxy.NET.UI.Services;

namespace Noxy.NET.UI.Abstractions;

public abstract class BlazorComponent : ComponentBase, IDisposable
{
    [Inject]
    public LoadingService LoadingService { get; set; } = null!;

    protected bool IsRendered { get; private set; }
    protected bool IsLoading { get; private set; }

    protected Guid UUID { get; } = Guid.NewGuid();
    protected string UUIDString => UUID.ToString();
    protected string UUIDCode => UUID.ToString().Replace("-", "");

    protected virtual string CssClass => GetComponentName();

    public virtual void Dispose()
    {
        LoadingService.OnChange -= OnChangeHandler;
        GC.SuppressFinalize(this);
    }

    protected override void OnInitialized()
    {
        base.OnInitialized();

        LoadingService.OnChange += OnChangeHandler;
    }

    protected override void OnAfterRender(bool firstRender)
    {
        base.OnAfterRender(firstRender);
        if (!firstRender) return;
        IsRendered = true;
    }

    protected string GetComponentName()
    {
        return GetType().Name.Split('`').First();
    }

    protected void WithLoading(Action callback)
    {
        try
        {
            LoadingService.StartLoading(UUIDString);
            callback();
        }
        catch (Exception)
        {
            // Do stuff
        }
        finally
        {
            LoadingService.FinishLoading(UUIDString);
        }
    }

    protected async Task WithLoading(Func<Task> callback)
    {
        try
        {
            LoadingService.StartLoading(UUIDString);
            await callback();
        }
        catch (Exception)
        {
            // Do stuff
        }
        finally
        {
            LoadingService.FinishLoading(UUIDString);
        }
    }

    protected static string CombineCssClass(params string?[] @params)
    {
        return string.Join(' ', @params.Where(x => !string.IsNullOrWhiteSpace(x)).OfType<string>().Select(@class => @class.Trim()));
    }

    private void OnChangeHandler(string sender, GenericEventArgs<bool> args)
    {
        if (sender != UUIDString || args.Value == IsLoading) return;

        IsLoading = args.Value;
        InvokeAsync(StateHasChanged);
    }
}
