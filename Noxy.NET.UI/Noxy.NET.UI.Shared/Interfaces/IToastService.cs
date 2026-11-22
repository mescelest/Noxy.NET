using Noxy.NET.UI.Enums;
using Noxy.NET.UI.Models;

namespace Noxy.NET.UI.Interfaces;

public interface IToastService
{
    // Raised whenever a toast is shown, dismissed or expires.
    event Action? Changed;

    // How many toasts are shown at once: the rest wait, in the order they were raised, until there is room. At least 1.
    int MaxVisible { get; set; }

    // How long a temporary toast stays when it has no duration of its own. More than zero.
    TimeSpan DefaultDuration { get; set; }

    List<Toast> GetVisibleList();

    List<Toast> GetWaitingList();

    // The temporary toast's time only starts when it is shown, not while it waits. Raising the same message with the same status while it is still shown or waiting returns that toast instead of adding another.
    Toast Show(string message, StatusEnum status, bool isPermanent, TimeSpan? duration = null);

    // An error stays until it is dismissed, unless said otherwise.
    Toast ShowError(string message, bool isPermanent = true, TimeSpan? duration = null);

    Toast ShowWarning(string message, bool isPermanent = false, TimeSpan? duration = null);

    Toast ShowInfo(string message, bool isPermanent = false, TimeSpan? duration = null);

    Toast ShowSuccess(string message, bool isPermanent = false, TimeSpan? duration = null);

    // Removes a toast that is shown or still waiting. An unknown id is ignored.
    void Dismiss(Guid id);

    void Clear();
}
