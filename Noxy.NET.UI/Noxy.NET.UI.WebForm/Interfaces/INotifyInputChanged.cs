using Microsoft.AspNetCore.Components;

namespace Noxy.NET.UI.Interfaces;

public interface INotifyInputChanged
{
    public EventCallback<ChangeEventArgs> OnChange { get; set; }
}
