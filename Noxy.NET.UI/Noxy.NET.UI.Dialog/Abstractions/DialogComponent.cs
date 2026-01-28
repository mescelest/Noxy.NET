using Microsoft.AspNetCore.Components;
using Noxy.NET.UI.Components;
using Noxy.NET.UI.Models;

namespace Noxy.NET.UI.Abstractions;

public class DialogComponent : ElementComponent
{
    [CascadingParameter(Name = nameof(DialogViewer.Context))]
    protected DialogContext? Context { get; set; }
    protected DialogContext ContextCurrent => Context ?? throw new ArgumentNullException(nameof(Context));
}
