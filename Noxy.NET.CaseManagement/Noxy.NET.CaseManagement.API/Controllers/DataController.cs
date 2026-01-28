using Microsoft.AspNetCore.Mvc;
using Noxy.NET.CaseManagement.Application.Interfaces.Services;
using Noxy.NET.CaseManagement.Domain.ViewModels;

namespace Noxy.NET.CaseManagement.API.Controllers;

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

    [HttpGet("Context/{identifier}/Action")]
    public ActionResult<ViewModelSchemaAction[]> GetContextActionListWithIdentifier(string identifier)
    {
        return serviceData.GetActionListWithContextIdentifier(identifier);
    }

    [HttpGet("Context/{identifier}/Element")]
    public async Task<ActionResult<ViewModelDataElement[]>> GetContextElementListWithIdentifier(string identifier)
    {
        return await serviceData.GetElementListWithContextIdentifier(identifier);
    }
    
    [HttpPost("TextParameter/Resolve")]
    public async Task<ActionResult<Dictionary<string, string>>> ResolveTextParameterList(IEnumerable<string> list)
    {
        return await serviceData.ResolveTextParameterList(list);
    }
}
