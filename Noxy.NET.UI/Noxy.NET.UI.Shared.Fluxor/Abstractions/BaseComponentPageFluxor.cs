using Noxy.NET.UI.Interfaces;
using Noxy.NET.UI.Models;

namespace Noxy.NET.UI.Abstractions;

public abstract class BaseComponentPageFluxor : BaseComponentBlazorFluxor, IPageComponent
{
    protected const string FullscreenPageClass = Constants.ClassPageFullscreen;

    public override string CssClass => ComponentMetadata.CombineCssClass(base.CssClass, Constants.ClassPage);
}
