using Microsoft.AspNetCore.Components;
using Noxy.NET.CaseManagement.Presentation.Services;
using Noxy.NET.UI.Abstractions;
using Noxy.NET.UI.Models;

namespace Noxy.NET.CaseManagement.Presentation.Abstractions.Components;

public abstract class BaseComponentTextForm<TForm> : BaseComponentForm<TForm> where TForm : BaseFormModel
{
    [Inject]
    protected SchemaAPIService SchemaAPIService { get; set; } = null!;

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

        await WithLoading(TextService.Resolve);
    }
}
