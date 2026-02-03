using Microsoft.AspNetCore.Components;
using Noxy.NET.EntityManagement.Presentation.Services;
using Noxy.NET.UI.Abstractions;

namespace Noxy.NET.EntityManagement.Presentation.Abstractions.Components;

public abstract class BaseComponentForm<TForm> : BaseForm<TForm> where TForm : BaseFormModel
{
    [Inject]
    protected APIHttpClient APIHttpClient { get; set; } = null!;

    [Inject]
    protected TextService TextService { get; set; } = null!;

    protected string GetDisplayName(string property)
    {
        return TextService.Get(Context.GetField(property)?.GetDisplayName());
    }

    protected string GetDescription(string property)
    {
        return TextService.Get(Context.GetField(property)?.GetDescription());
    }

    protected override void OnInitialized()
    {
        base.OnInitialized();
        Context = CreateContext();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);
        if (!firstRender) return;

        LoadAfterRender(TextService.Resolve);
    }
}
