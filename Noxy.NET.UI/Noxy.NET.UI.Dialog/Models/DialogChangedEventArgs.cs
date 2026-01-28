namespace Noxy.NET.UI.Models;

public class DialogChangedEventArgs(DialogContext? context) : EventArgs
{
    public DialogContext? Context { get; set; } = context;
}
