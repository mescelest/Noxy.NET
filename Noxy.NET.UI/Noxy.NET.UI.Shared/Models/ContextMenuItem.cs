using System.ComponentModel;
using Noxy.NET.UI.Enums;

namespace Noxy.NET.UI.Models;

public record ContextMenuItem(
    ContextMenuItemKindEnum Kind,
    string Text,
    Func<Task>? Run = null,
    bool IsDisabled = false,
    string? DisabledReason = null,
    bool IsDestructive = false,
    string? Icon = null,
    List<ContextMenuItem>? SubItemList = null,
    bool ClosesMenu = true,
    string? ConfirmText = null)
{
    // A separator is never enabled, and a submenu is enabled only while something inside it can be chosen.
    public bool IsEnabled
    {
        get
        {
            return Kind switch
            {
                ContextMenuItemKindEnum.Item => !IsDisabled,
                ContextMenuItemKindEnum.Separator => false,
                ContextMenuItemKindEnum.Submenu => !IsDisabled && SubItemList is not null && SubItemList.Any(x => x.IsEnabled),
                _ => throw new InvalidEnumArgumentException(nameof(Kind), (int)Kind, typeof(ContextMenuItemKindEnum))
            };
        }
    }

    public static ContextMenuItem Item(string text, Func<Task>? run = null, bool isDisabled = false, string? disabledReason = null, bool isDestructive = false, string? icon = null, bool closesMenu = true, string? confirmText = null)
    {
        return new(ContextMenuItemKindEnum.Item, text, run, isDisabled, disabledReason, isDestructive, icon, ClosesMenu: closesMenu, ConfirmText: confirmText);
    }

    public static ContextMenuItem Separator()
    {
        return new(ContextMenuItemKindEnum.Separator, string.Empty);
    }

    public static ContextMenuItem Submenu(string text, List<ContextMenuItem> subItemList, string? icon = null)
    {
        return new(ContextMenuItemKindEnum.Submenu, text, Icon: icon, SubItemList: subItemList);
    }
}
