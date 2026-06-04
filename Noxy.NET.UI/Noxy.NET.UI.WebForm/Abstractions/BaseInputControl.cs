using Microsoft.AspNetCore.Components;
using Noxy.NET.UI.Interfaces;

namespace Noxy.NET.UI.Abstractions;

public abstract class BaseInputControl<TValue> : BaseInput<TValue>, IBaseInputControl
{
    [Parameter]
    public IReadOnlyDictionary<string, object>? InputAttributes { get; set; }
}
