using Microsoft.AspNetCore.Components;
using Noxy.NET.EntityManagement.Domain.Abstractions.Requests;
using Noxy.NET.EntityManagement.Presentation.Services;
using Noxy.NET.UI.Abstractions;

namespace Noxy.NET.EntityManagement.Presentation.Abstractions.Components;

public abstract class BaseRequestForm<TRequest, TResponse> : BaseForm<TRequest> where TRequest : BaseRequest<TResponse>
{
    [Inject]
    protected APIHttpClient APIHttpClient { get; set; } = null!;

    [Inject]
    protected TextService TextService { get; set; } = null!;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);
        if (!firstRender) return;

        LoadAfterRender(TextService.Resolve);
    }

    protected string GetDisplayName(string property)
    {
        return TextService.Get(Context.GetField(property)?.GetDisplayName());
    }

    protected string GetDescription(string property)
    {
        return TextService.Get(Context.GetField(property)?.GetDescription());
    }
}
