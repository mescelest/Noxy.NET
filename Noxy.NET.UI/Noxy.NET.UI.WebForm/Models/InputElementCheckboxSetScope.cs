using Microsoft.AspNetCore.Components;

namespace Noxy.NET.UI.Models;

public record InputElementCheckboxSetScope<TOption>(string ID, string Name, TOption Option, Action<ChangeEventArgs, TOption> OnChange);
