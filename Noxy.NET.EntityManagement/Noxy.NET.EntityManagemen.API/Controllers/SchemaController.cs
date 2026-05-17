using Mediator;
using Microsoft.AspNetCore.Mvc;
using Noxy.NET.EntityManagement.API.Commands.Schema;
using Noxy.NET.EntityManagement.API.Commands.Schema.Context;
using Noxy.NET.EntityManagement.API.Commands.Schema.ContextHasElement;
using Noxy.NET.EntityManagement.API.Commands.Schema.Element;
using Noxy.NET.EntityManagement.API.Commands.Schema.ElementHasProperty;
using Noxy.NET.EntityManagement.API.Commands.Schema.Parameter;
using Noxy.NET.EntityManagement.API.Commands.Schema.Property;
using Noxy.NET.EntityManagement.API.Queries.Schema;
using Noxy.NET.EntityManagement.API.Queries.Schema.Context;
using Noxy.NET.EntityManagement.API.Queries.Schema.ContextHasElement;
using Noxy.NET.EntityManagement.API.Queries.Schema.Element;
using Noxy.NET.EntityManagement.API.Queries.Schema.ElementHasProperty;
using Noxy.NET.EntityManagement.API.Queries.Schema.Parameter;
using Noxy.NET.EntityManagement.API.Queries.Schema.Property;
using Noxy.NET.EntityManagement.Domain.Requests.Schema;
using Noxy.NET.EntityManagement.Domain.Requests.Schema.Context;
using Noxy.NET.EntityManagement.Domain.Requests.Schema.ContextHasElement;
using Noxy.NET.EntityManagement.Domain.Requests.Schema.Element;
using Noxy.NET.EntityManagement.Domain.Requests.Schema.ElementHasProperty;
using Noxy.NET.EntityManagement.Domain.Requests.Schema.Parameter;
using Noxy.NET.EntityManagement.Domain.Requests.Schema.Property;
using Noxy.NET.EntityManagement.Domain.Responses.Schema;
using Noxy.NET.EntityManagement.Domain.Responses.Schema.Context;
using Noxy.NET.EntityManagement.Domain.Responses.Schema.ContextHasElement;
using Noxy.NET.EntityManagement.Domain.Responses.Schema.Element;
using Noxy.NET.EntityManagement.Domain.Responses.Schema.ElementHasProperty;
using Noxy.NET.EntityManagement.Domain.Responses.Schema.Parameter;
using Noxy.NET.EntityManagement.Domain.Responses.Schema.Property;

namespace Noxy.NET.EntityManagement.API.Controllers;

[ApiController]
[Route("[controller]")]
public class SchemaController(IMediator mediator) : ControllerBase
{
    [HttpGet("")]
    public async Task<ActionResult<ResponseSchemaList>> GetSchemaByID([FromQuery] RequestSchemaList request)
    {
        return await mediator.Send(new QuerySchemaList(request));
    }

    [HttpGet("/Count")]
    public async Task<ActionResult<ResponseSchemaCount>> GetSchemaCount([FromQuery] RequestSchemaCount request)
    {
        return await mediator.Send(new QuerySchemaCount(request));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ResponseSchemaFind>> GetSchemaList(Guid id)
    {
        return await mediator.Send(new QuerySchemaFind(id));
    }

    [HttpGet("Context")]
    public async Task<ActionResult<ResponseSchemaContextList>> GetSchemaContextList([FromQuery] RequestSchemaContextList request)
    {
        return await mediator.Send(new QuerySchemaContextList(request));
    }

    [HttpGet("Context/List")]
    public async Task<ActionResult<ResponseSchemaContextCount>> GetSchemaContextCount([FromQuery] RequestSchemaContextCount request)
    {
        return await mediator.Send(new QuerySchemaContextCount(request));
    }

    [HttpGet("Context/{id:guid}")]
    public async Task<ActionResult<ResponseSchemaContextFind>> GetSchemaContextByID(Guid id)
    {
        return await mediator.Send(new QuerySchemaContextFind(id));
    }

    [HttpGet("Context/Element")]
    public async Task<ActionResult<ResponseSchemaContextHasElementList>> GetSchemaContextHasElementList([FromQuery] RequestSchemaContextHasElementList request)
    {
        return await mediator.Send(new QuerySchemaContextHasElementList(request));
    }

    [HttpGet("Context/Element/{id:guid}")]
    public async Task<ActionResult<ResponseSchemaContextHasElementFind>> GetSchemaContextHasElementByID(Guid id)
    {
        return await mediator.Send(new QuerySchemaContextHasElementFind(id));
    }

    [HttpGet("Element")]
    public async Task<ActionResult<ResponseSchemaElementList>> GetSchemaElementList([FromQuery] RequestSchemaElementList request)
    {
        return await mediator.Send(new QuerySchemaElementList(request));
    }

    [HttpGet("Element/Count")]
    public async Task<ActionResult<ResponseSchemaElementCount>> GetSchemaElementCount([FromQuery] RequestSchemaElementCount request)
    {
        return await mediator.Send(new QuerySchemaElementCount(request));
    }

    [HttpGet("Element/{id:guid}")]
    public async Task<ActionResult<ResponseSchemaElementFind>> GetSchemaElementByID(Guid id)
    {
        return await mediator.Send(new QuerySchemaElementFind(id));
    }

    [HttpGet("Element/Property")]
    public async Task<ActionResult<ResponseSchemaElementHasPropertyList>> GetSchemaElementHasPropertyList([FromQuery] RequestSchemaElementHasPropertyList request)
    {
        return await mediator.Send(new QuerySchemaElementHasPropertyList(request));
    }

    [HttpGet("Element/Property/{id:guid}")]
    public async Task<ActionResult<ResponseSchemaElementHasPropertyFind>> GetSchemaElementHasPropertyByID(Guid id)
    {
        return await mediator.Send(new QuerySchemaElementHasPropertyFind(id));
    }

    [HttpGet("Parameter")]
    public async Task<ActionResult<ResponseSchemaParameterList>> GetSchemaParameterList([FromQuery] RequestSchemaParameterList request)
    {
        return await mediator.Send(new QuerySchemaParameterList(request));
    }

    [HttpGet("Parameter/Count")]
    public async Task<ActionResult<ResponseSchemaParameterCount>> GetSchemaParameterCount([FromQuery] RequestSchemaParameterCount request)
    {
        return await mediator.Send(new QuerySchemaParameterCount(request));
    }

    [HttpGet("Parameter/{id:guid}")]
    public async Task<ActionResult<ResponseSchemaParameterFind>> GetSchemaParameterByID(Guid id)
    {
        return await mediator.Send(new QuerySchemaParameterFind(id));
    }

    [HttpGet("Property")]
    public async Task<ActionResult<ResponseSchemaPropertyList>> GetSchemaPropertyList([FromQuery] RequestSchemaPropertyList request)
    {
        return await mediator.Send(new QuerySchemaPropertyList(request));
    }

    [HttpGet("Property/Count")]
    public async Task<ActionResult<ResponseSchemaPropertyCount>> GetSchemaPropertyCount([FromQuery] RequestSchemaPropertyCount request)
    {
        return await mediator.Send(new QuerySchemaPropertyCount(request));
    }

    [HttpGet("Property/{id:guid}")]
    public async Task<ActionResult<ResponseSchemaPropertyFind>> GetSchemaPropertyByID(Guid id)
    {
        return await mediator.Send(new QuerySchemaPropertyFind(id));
    }

    [HttpPost("")]
    public async Task<IActionResult> CreateSchema(RequestSchemaCreate request)
    {
        await mediator.Send(new CommandSchemaCreate(request));
        return NoContent();
    }

    [HttpPost("{id:guid}")]
    public async Task<IActionResult> UpdateSchema(Guid id, RequestSchemaUpdate request)
    {
        await mediator.Send(new CommandSchemaUpdate(id, request));
        return NoContent();
    }

    [HttpPost("{id:guid}/Clone")]
    public async Task<IActionResult> CloneSchema(Guid id)
    {
        await mediator.Send(new CommandSchemaClone(id));
        return NoContent();
    }

    [HttpPost("{id:guid}/Activate")]
    public async Task<ActionResult> ActivateSchema(Guid id)
    {
        await mediator.Send(new CommandSchemaActivate(id));
        return NoContent();
    }

    [HttpPost("{id:guid}/Delete")]
    public async Task<IActionResult> DeleteSchema(Guid id)
    {
        await mediator.Send(new CommandSchemaDelete(id));
        return NoContent();
    }

    [HttpPost("Context")]
    public async Task<IActionResult> CreateSchemaContext(RequestSchemaContextCreate request)
    {
        await mediator.Send(new CommandSchemaContextCreate(request));
        return NoContent();
    }

    [HttpPost("Context/{id:guid}")]
    public async Task<IActionResult> UpdateSchemaContext(Guid id, RequestSchemaContextUpdate request)
    {
        await mediator.Send(new CommandSchemaContextUpdate(id, request));
        return NoContent();
    }

    [HttpPost("Context/{id:guid}/Clone")]
    public async Task<IActionResult> CloneSchemaContext(Guid id)
    {
        await mediator.Send(new CommandSchemaContextClone(id));
        return NoContent();
    }

    [HttpPost("Context/{id:guid}/Delete")]
    public async Task<IActionResult> DeleteSchemaContext(Guid id)
    {
        await mediator.Send(new CommandSchemaContextDelete(id));
        return NoContent();
    }

    [HttpPost("Context/Element")]
    public async Task<IActionResult> CreateSchemaContextHasElement(RequestSchemaContextHasElementCreate request)
    {
        await mediator.Send(new CommandSchemaContextHasElementCreate(request));
        return NoContent();
    }

    [HttpPost("Context/Element/{id:guid}/Delete")]
    public async Task<IActionResult> CreateSchemaContextHasElement(Guid id)
    {
        await mediator.Send(new CommandSchemaContextHasElementDelete(id));
        return NoContent();
    }

    [HttpPost("Element")]
    public async Task<IActionResult> CreateSchemaElement(RequestSchemaElementCreate request)
    {
        await mediator.Send(new CommandSchemaElementCreate(request));
        return NoContent();
    }

    [HttpPost("Element/{id:guid}")]
    public async Task<IActionResult> UpdateSchemaElement(Guid id, RequestSchemaElementUpdate request)
    {
        await mediator.Send(new CommandSchemaElementUpdate(id, request));
        return NoContent();
    }

    [HttpPost("Element/{id:guid}/Clone")]
    public async Task<IActionResult> CloneSchemaElement(Guid id)
    {
        await mediator.Send(new CommandSchemaElementClone(id));
        return NoContent();
    }

    [HttpPost("Element/{id:guid}/Delete")]
    public async Task<IActionResult> DeleteSchemaElement(Guid id)
    {
        await mediator.Send(new CommandSchemaElementDelete(id));
        return NoContent();
    }

    [HttpPost("Element/Property")]
    public async Task<IActionResult> CreateSchemaElementHasProperty(RequestSchemaElementHasPropertyCreate request)
    {
        await mediator.Send(new CommandSchemaElementHasPropertyCreate(request));
        return NoContent();
    }

    [HttpPost("Element/Property/{id:guid}/Delete")]
    public async Task<IActionResult> CreateSchemaElementHasProperty(Guid id)
    {
        await mediator.Send(new CommandSchemaElementHasPropertyDelete(id));
        return NoContent();
    }

    [HttpPost("Parameter/Style")]
    public async Task<IActionResult> Create(RequestSchemaParameterStyleCreate model)
    {
        await mediator.Send(new CommandSchemaParameterStyleCreate(model));
        return NoContent();
    }

    [HttpPost("Parameter/System")]
    public async Task<IActionResult> Create(RequestSchemaParameterSystemCreate model)
    {
        await mediator.Send(new CommandSchemaParameterSystemCreate(model));
        return NoContent();
    }

    [HttpPost("Parameter/Text")]
    public async Task<IActionResult> Create(RequestSchemaParameterTextCreate model)
    {
        await mediator.Send(new CommandSchemaParameterTextCreate(model));
        return NoContent();
    }

    [HttpPost("Parameter/Style/{id:guid}")]
    public async Task<IActionResult> Update(Guid id, RequestSchemaParameterStyleUpdate model)
    {
        await mediator.Send(new CommandSchemaParameterStyleUpdate(id, model));
        return NoContent();
    }

    [HttpPost("Parameter/System/{id:guid}")]
    public async Task<IActionResult> Update(Guid id, RequestSchemaParameterSystemUpdate model)
    {
        await mediator.Send(new CommandSchemaParameterSystemUpdate(id, model));
        return NoContent();
    }

    [HttpPost("Parameter/Text/{id:guid}")]
    public async Task<IActionResult> Update(Guid id, RequestSchemaParameterTextUpdate model)
    {
        await mediator.Send(new CommandSchemaParameterTextUpdate(id, model));
        return NoContent();
    }

    [HttpPost("Parameter/{id:guid}/Clone")]
    public async Task<IActionResult> CloneSchemaParameter(Guid id)
    {
        await mediator.Send(new CommandSchemaParameterClone(id));
        return NoContent();
    }

    [HttpPost("Parameter/{id:guid}/Delete")]
    public async Task<IActionResult> DeleteSchemaParameter(Guid id)
    {
        await mediator.Send(new CommandSchemaParameterDelete(id));
        return NoContent();
    }

    [HttpPost("Property/Boolean")]
    public async Task<IActionResult> Create(RequestSchemaPropertyBooleanCreate model)
    {
        await mediator.Send(new CommandSchemaPropertyBooleanCreate(model));
        return NoContent();
    }

    [HttpPost("Property/DateTime")]
    public async Task<IActionResult> Create(RequestSchemaPropertyDateTimeCreate model)
    {
        await mediator.Send(new CommandSchemaPropertyDateTimeCreate(model));
        return NoContent();
    }

    [HttpPost("Property/Decimal")]
    public async Task<IActionResult> Create(RequestSchemaPropertyDecimalCreate model)
    {
        await mediator.Send(new CommandSchemaPropertyDecimalCreate(model));
        return NoContent();
    }

    [HttpPost("Property/Boolean/{id:guid}")]
    public async Task<IActionResult> Update(Guid id, RequestSchemaPropertyBooleanUpdate model)
    {
        await mediator.Send(new CommandSchemaPropertyBooleanUpdate(id, model));
        return NoContent();
    }

    [HttpPost("Property/DateTime/{id:guid}")]
    public async Task<IActionResult> Update(Guid id, RequestSchemaPropertyDateTimeUpdate model)
    {
        await mediator.Send(new CommandSchemaPropertyDateTimeUpdate(id, model));
        return NoContent();
    }

    [HttpPost("Property/Decimal/{id:guid}")]
    public async Task<IActionResult> Update(Guid id, RequestSchemaPropertyDecimalUpdate model)
    {
        await mediator.Send(new CommandSchemaPropertyDecimalUpdate(id, model));
        return NoContent();
    }

    [HttpPost("Property/Image/{id:guid}")]
    public async Task<IActionResult> Update(Guid id, RequestSchemaPropertyImageUpdate model)
    {
        await mediator.Send(new CommandSchemaPropertyImageUpdate(id, model));
        return NoContent();
    }

    [HttpPost("Property/Integer/{id:guid}")]
    public async Task<IActionResult> Update(Guid id, RequestSchemaPropertyIntegerUpdate model)
    {
        await mediator.Send(new CommandSchemaPropertyIntegerUpdate(id, model));
        return NoContent();
    }

    [HttpPost("Property/String/{id:guid}")]
    public async Task<IActionResult> Update(Guid id, RequestSchemaPropertyStringUpdate model)
    {
        await mediator.Send(new CommandSchemaPropertyStringUpdate(id, model));
        return NoContent();
    }

    [HttpPost("Property/{id:guid}/Clone")]
    public async Task<IActionResult> CloneSchemaProperty(Guid id)
    {
        await mediator.Send(new CommandSchemaPropertyClone(id));
        return NoContent();
    }

    [HttpPost("Property/{id:guid}/Delete")]
    public async Task<IActionResult> DeleteSchemaProperty(Guid id)
    {
        await mediator.Send(new CommandSchemaPropertyDelete(id));
        return NoContent();
    }
}
