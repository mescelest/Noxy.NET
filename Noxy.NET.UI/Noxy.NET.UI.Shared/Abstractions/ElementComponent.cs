using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Components;
using Noxy.NET.UI.Interfaces;
using Noxy.NET.UI.Models;

namespace Noxy.NET.UI.Abstractions;

public abstract class ElementComponent : BlazorComponent, IElementComponent
{
    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object>? AdditionalAttributes { get; init; }

    public override string CssClass => ComponentMetadata.CombineCssClass(base.CssClass, GetComponentClass());

    public virtual string GetComponentClass()
    {
        return ComponentMetadata.ExtractCssClass(AdditionalAttributes);
    }

    public bool TryExtractAttribute<T>(string attribute, [NotNullWhen(true)] out T? result)
    {
        return ComponentMetadata.TryExtractAttribute(AdditionalAttributes, attribute, out result);
    }
}
