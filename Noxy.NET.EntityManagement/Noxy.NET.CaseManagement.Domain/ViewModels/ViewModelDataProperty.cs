using Noxy.NET.CaseManagement.Domain.Abstractions.ViewModels;
using Noxy.NET.Models;

namespace Noxy.NET.CaseManagement.Domain.ViewModels;

public class ViewModelDataProperty : BaseViewModelSchemaComponent
{
    public JsonProperty? Value { get; set; }
}
