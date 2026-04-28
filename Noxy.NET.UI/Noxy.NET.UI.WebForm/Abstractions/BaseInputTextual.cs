using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Noxy.NET.UI.Abstractions;

public abstract class BaseInputTextual<TValue> : BaseInput<TValue>, IDisposable, IAsyncDisposable
{
    [Inject]
    protected IJSRuntime JS { get; set; } = null!;

    [Parameter]
    public bool? HasChangeEventOnInput { get; set; }
    protected bool HasChangeEventOnInputCurrent => HasChangeEventOnInput ?? false;

    [Parameter]
    public int? Size { get; set; }
    protected int SizeCurrent => Size ?? 40;

    [Parameter]
    public int? InputDelay { get; set; }
    protected int InputDelayCurrent => InputDelay ?? 200;

    protected ElementReference InputRef { get; set; }
    protected IJSObjectReference? InteropModule { get; set; }
    protected DotNetObjectReference<BaseInputTextual<TValue>>? DotNetReference { get; set; }

    protected override async Task OnAfterFirstRenderAsync()
    {
        await base.OnAfterFirstRenderAsync();

        if (HasChangeEventOnInputCurrent)
        {
            await LoadInterop(JS);

            DotNetReference ??= DotNetObjectReference.Create(this);
            await InteropModule!.InvokeVoidAsync("RegisterOnInput", InputRef, InputDelayCurrent, DotNetReference, nameof(OnValueInput));
        }
    }

    protected async Task LoadInterop(IJSRuntime js)
    {
        InteropModule ??= await js.InvokeAsync<IJSObjectReference>("import", $"./_content/{Constants.AssemblyNameUIWebForm}/Interop.js");
    }

    [JSInvokable]
    public abstract void OnValueInput(string value);

    protected abstract void OnValueChange(ChangeEventArgs args);

    public virtual void Dispose()
    {
        GC.SuppressFinalize(this);
        DotNetReference?.Dispose();
    }

    public async virtual ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);
        if (InteropModule != null)
        {
            await InteropModule.DisposeAsync();
        }
    }
}
