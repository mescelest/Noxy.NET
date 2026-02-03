using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Noxy.NET.UI.Abstractions;

public abstract class BaseInputTextualValue<TValue> : BaseInputValue<TValue>, IDisposable, IAsyncDisposable
{
    [Inject]
    protected IJSRuntime JS { get; set; } = null!;

    [Parameter]
    public bool? HasChangeEventOnInput { get; set; }
    protected bool HasChangeEventOnInputCurrent => HasChangeEventOnInput ?? false;

    protected IJSObjectReference? Module { get; set; }
    protected DotNetObjectReference<BaseInputTextualValue<TValue>>? DotNetReference { get; set; }

    public async ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);
        if (Module != null)
        {
            await Module.DisposeAsync();
        }
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        DotNetReference?.Dispose();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);
        if (!firstRender) return;

        if (HasChangeEventOnInputCurrent)
        {
            await LoadInterop(JS);

            DotNetReference = DotNetObjectReference.Create(this);
            await Module!.InvokeVoidAsync("NoxyNETUIWebForm.RegisterOnInput", UUIDString, Constants.OnInputDelay, DotNetReference, nameof(OnValueInput));
        }
    }

    protected async Task LoadInterop(IJSRuntime js)
    {
        Module ??= await js.InvokeAsync<IJSObjectReference>("import", $"./_content/{Constants.AssemblyNameUIWebForm}/Interop.js");
    }

    [JSInvokable]
    public abstract void OnValueInput(string value);

    protected abstract void OnValueChange(ChangeEventArgs args);
}
