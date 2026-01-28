using Microsoft.AspNetCore.Components;

namespace Noxy.NET.UI.Models;

public class DialogContext
{
    public Guid ID { get; } = Guid.NewGuid();
    public required RenderFragment Fragment { get; set; }
    public required DialogOptions Options { get; set; }
}
