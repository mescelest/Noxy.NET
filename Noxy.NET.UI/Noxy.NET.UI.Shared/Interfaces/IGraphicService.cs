using Microsoft.AspNetCore.Components;

namespace Noxy.NET.UI.Interfaces;

public interface IGraphicService
{
    RenderFragment GetIcon(string key);
}
