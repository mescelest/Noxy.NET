using Noxy.NET.UI.Enums;

namespace Noxy.NET.UI.Models;

public class DialogOptions
{
    public bool Overlay { get; set; } = true;
    public bool Required { get; set; } = false;
    public DialogSizeEnum Size { get; set; } = DialogSizeEnum.Auto;
}
