using Noxy.NET.EntityManagement.Domain.Abstractions.ViewModels;
using Noxy.NET.Models;

namespace Noxy.NET.EntityManagement.Domain.ViewModels;

public class ViewModelDataProperty : BaseViewModelSchemaComponent
{
    public JsonProperty? Value { get; set; }
}
