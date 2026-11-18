using Microsoft.AspNetCore.Components;

namespace Noxy.NET.UI.Models.Forms;

public record InputElementRadioSetProperties<TOption>(string ID, string Name, TOption Option, Action<ChangeEventArgs, TOption> OnChange);
