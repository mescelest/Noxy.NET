using Microsoft.AspNetCore.Components;
using Noxy.NET.EntityManagement.Presentation.Services;
using Noxy.NET.UI.Abstractions;

namespace Noxy.NET.EntityManagement.Presentation.Abstractions.Components;

public abstract class BaseComponentForm<TForm> : BaseForm<TForm> where TForm : BaseFormModel
{
    [Inject]
    protected APIHttpClientOld APIHttpClientOld { get; set; } = null!;

    [Inject]
    protected TextService TextService { get; set; } = null!;

    protected override void OnInitialized()
    {
        base.OnInitialized();
        Context = CreateContext();
    }

    protected string GetDisplayName(string property)
    {
        return TextService.Get(Context.GetFieldDisplayName(property));
    }

    protected string GetDescription(string property)
    {
        return TextService.Get(Context.GetFieldDescription(property));
    }
}
