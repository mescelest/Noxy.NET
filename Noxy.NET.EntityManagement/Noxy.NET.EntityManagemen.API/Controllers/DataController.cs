using Microsoft.AspNetCore.Mvc;
using Noxy.NET.EntityManagement.Application.Interfaces.Services;
using Noxy.NET.EntityManagement.Domain.ViewModels;

namespace Noxy.NET.EntityManagement.API.Controllers;

[ApiController]
[Route("[controller]")]
public class DataController(IDataService serviceData) : ControllerBase
{
    [HttpGet("Context")]
    public ActionResult<ViewModelSchemaContext[]> GetContextList()
    {
        return serviceData.GetContextList();
    }

    [HttpGet("Context/{identifier}")]
    public ActionResult<ViewModelSchemaContext> GetContextListWithIdentifier(string identifier)
    {
        return serviceData.GetContextListWithIdentifier(identifier);
    }

    [HttpGet("Context/{identifier}/Element")]
    public async Task<ActionResult<ViewModelDataElement[]>> GetContextElementListWithIdentifier(string identifier)
    {
        return await serviceData.GetElementListWithContextIdentifier(identifier);
    }

    [HttpPost("Parameter/Text/Resolve")]
    public async Task<ActionResult<Dictionary<string, string>>> ResolveTextParameterList(IEnumerable<string> list)
    {
        return await serviceData.ResolveTextParameterList(list);
    }
}
