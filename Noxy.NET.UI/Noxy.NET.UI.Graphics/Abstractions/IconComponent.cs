using Microsoft.AspNetCore.Components;
using Noxy.NET.UI.Enums;

namespace Noxy.NET.UI.Abstractions;

public abstract class IconComponent : ElementComponent
{
    [Parameter]
    public IconSizeEnum? Size { get; set; }
}
