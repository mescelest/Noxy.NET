using Microsoft.AspNetCore.Components;
using Noxy.NET.UI.Services;

namespace Noxy.NET.UI.Abstractions;

public abstract class BlazorComponent : ComponentBase
{
    [Inject]
    protected PageLoadingService PageLoadingService { get; set; } = null!;

    protected bool IsRendered { get; set; }
    protected bool IsLoading { get; set; }

    protected Guid UUID { get; } = Guid.NewGuid();
    protected string UUIDString => UUID.ToString();
    protected string UUIDCode => UUID.ToString().Replace("-", "");

    protected virtual string CssClass => GetComponentName();

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

    protected static string CombineCssClass(params string?[] @params)
    {
        return string.Join(' ', @params.Where(x => !string.IsNullOrWhiteSpace(x)).OfType<string>().Select(@class => @class.Trim()));
    }
}
