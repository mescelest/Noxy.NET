using Noxy.NET.CaseManagement.Domain.Abstractions.ViewModels;
using Noxy.NET.Models;

namespace Noxy.NET.CaseManagement.Domain.ViewModels;

public class ViewModelSchemaDynamicValue : BaseViewModelSchema
{
    public required JsonProperty? Value { get; set; }
}
