using Microsoft.AspNetCore.Components;

namespace Noxy.NET.UI.Models;

public record InputElementCheckboxSetScope<TOption>(string ID, string Name, bool Checked, TOption Option, Action<ChangeEventArgs, TOption> OnChange);
