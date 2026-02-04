using Microsoft.AspNetCore.Mvc;
using Noxy.NET.EntityManagement.Application.Interfaces.Services;

namespace Noxy.NET.EntityManagement.API.Controllers;

[ApiController]
[Route("[controller]")]
public class DataController(IDataService serviceData) : ControllerBase
{
    [HttpPost("Parameter/Text/Resolve")]
    public async Task<ActionResult<Dictionary<string, string>>> ResolveTextParameterList(IEnumerable<string> list)
    {
        return await serviceData.ResolveTextParameterList(list);
    }
}
