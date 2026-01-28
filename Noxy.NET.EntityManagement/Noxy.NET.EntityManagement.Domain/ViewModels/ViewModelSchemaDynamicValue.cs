using Noxy.NET.EntityManagement.Domain.Abstractions.ViewModels;
using Noxy.NET.Models;

namespace Noxy.NET.EntityManagement.Domain.ViewModels;

public class ViewModelSchemaDynamicValue : BaseViewModelSchema
{
    public required JsonProperty? Value { get; set; }
}
