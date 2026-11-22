using Noxy.NET.UI.Interfaces;
using Noxy.NET.UI.Models;

namespace Noxy.NET.UI.Components.Abstractions;

public abstract class BaseComponentPage : BaseComponentBlazor, IPageComponent
{
    protected const string FullscreenPageClass = UISharedConstants.ClassPageFullscreen;

    public override string CssClass => ComponentMetadata.CombineCssClass(base.CssClass, UISharedConstants.ClassPage);
}
