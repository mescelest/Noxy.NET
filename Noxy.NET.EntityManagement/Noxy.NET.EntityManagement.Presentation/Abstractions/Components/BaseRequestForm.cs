using Fluxor;
using Microsoft.AspNetCore.Components;
using Noxy.NET.EntityManagement.Domain.Abstractions.Requests;
using Noxy.NET.EntityManagement.Presentation.Features;
using Noxy.NET.EntityManagement.Presentation.Services;
using Noxy.NET.UI.Abstractions;

namespace Noxy.NET.EntityManagement.Presentation.Abstractions.Components;

public abstract class BaseRequestForm<TRequest, TResponse> : BaseForm<TRequest> where TRequest : BaseRequest<TResponse>
{
    [Inject]
    protected APIHttpClient APIHttpClient { get; set; } = null!;

    [Inject]
    protected TextService TextService { get; set; } = null!;

    [Inject]
    protected IState<FeatureTextState> TextState { get; set; } = null!;

    protected override void OnInitialized()
    {
        base.OnInitialized();
        TextState.StateChanged += OnTextStateChanged;
    }

    private void OnTextStateChanged(object? sender, EventArgs e)
    {
        InvokeAsync(StateHasChanged);
    }

    protected string GetDisplayName(string property)
    {
        return TextService.Get(Context.GetField(property)?.GetDisplayName(), GetComponentName());
    }

    protected string GetDescription(string property)
    {
        return TextService.Get(Context.GetField(property)?.GetDescription());
    }

    public void Dispose()
    {
        TextState.StateChanged -= OnTextStateChanged;
    }
}
