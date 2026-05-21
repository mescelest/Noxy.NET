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

    [HttpGet("count")]
    public async Task<ActionResult<ResponseSchemaCount>> GetSchemaCount([FromQuery] RequestSchemaCount request)
    {
        return await mediator.Send(new QuerySchemaCount(request));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ResponseSchemaFind>> GetSchemaList(Guid id)
    {
        return await mediator.Send(new QuerySchemaFind(id));
    }

    [HttpGet("context")]
    public async Task<ActionResult<ResponseSchemaContextList>> GetSchemaContextList([FromQuery] RequestSchemaContextList request)
    {
        return await mediator.Send(new QuerySchemaContextList(request));
    }

    [HttpGet("context/count")]
    public async Task<ActionResult<ResponseSchemaContextCount>> GetSchemaContextCount([FromQuery] RequestSchemaContextCount request)
    {
        return await mediator.Send(new QuerySchemaContextCount(request));
    }

    [HttpGet("context/{id:guid}")]
    public async Task<ActionResult<ResponseSchemaContextFind>> GetSchemaContextByID(Guid id)
    {
        return await mediator.Send(new QuerySchemaContextFind(id));
    }

    [HttpGet("context/element")]
    public async Task<ActionResult<ResponseSchemaContextHasElementList>> GetSchemaContextHasElementList([FromQuery] RequestSchemaContextHasElementList request)
    {
        return await mediator.Send(new QuerySchemaContextHasElementList(request));
    }

    [HttpGet("context/element/{id:guid}")]
    public async Task<ActionResult<ResponseSchemaContextHasElementFind>> GetSchemaContextHasElementByID(Guid id)
    {
        return await mediator.Send(new QuerySchemaContextHasElementFind(id));
    }

    [HttpGet("element")]
    public async Task<ActionResult<ResponseSchemaElementList>> GetSchemaElementList([FromQuery] RequestSchemaElementList request)
    {
        return await mediator.Send(new QuerySchemaElementList(request));
    }

    [HttpGet("element/count")]
    public async Task<ActionResult<ResponseSchemaElementCount>> GetSchemaElementCount([FromQuery] RequestSchemaElementCount request)
    {
        return await mediator.Send(new QuerySchemaElementCount(request));
    }

    [HttpGet("element/{id:guid}")]
    public async Task<ActionResult<ResponseSchemaElementFind>> GetSchemaElementByID(Guid id)
    {
        return await mediator.Send(new QuerySchemaElementFind(id));
    }

    [HttpGet("element/property")]
    public async Task<ActionResult<ResponseSchemaElementHasPropertyList>> GetSchemaElementHasPropertyList([FromQuery] RequestSchemaElementHasPropertyList request)
    {
        return await mediator.Send(new QuerySchemaElementHasPropertyList(request));
    }

    [HttpGet("element/property/{id:guid}")]
    public async Task<ActionResult<ResponseSchemaElementHasPropertyFind>> GetSchemaElementHasPropertyByID(Guid id)
    {
        return await mediator.Send(new QuerySchemaElementHasPropertyFind(id));
    }

    [HttpGet("parameter")]
    public async Task<ActionResult<ResponseSchemaParameterList>> GetSchemaParameterList([FromQuery] RequestSchemaParameterList request)
    {
        return await mediator.Send(new QuerySchemaParameterList(request));
    }

    [HttpGet("parameter/count")]
    public async Task<ActionResult<ResponseSchemaParameterCount>> GetSchemaParameterCount([FromQuery] RequestSchemaParameterCount request)
    {
        return await mediator.Send(new QuerySchemaParameterCount(request));
    }

    [HttpGet("parameter/{id:guid}")]
    public async Task<ActionResult<ResponseSchemaParameterFind>> GetSchemaParameterByID(Guid id)
    {
        return await mediator.Send(new QuerySchemaParameterFind(id));
    }

    [HttpGet("property")]
    public async Task<ActionResult<ResponseSchemaPropertyList>> GetSchemaPropertyList([FromQuery] RequestSchemaPropertyList request)
    {
        return await mediator.Send(new QuerySchemaPropertyList(request));
    }

    [HttpGet("property/count")]
    public async Task<ActionResult<ResponseSchemaPropertyCount>> GetSchemaPropertyCount([FromQuery] RequestSchemaPropertyCount request)
    {
        return await mediator.Send(new QuerySchemaPropertyCount(request));
    }

    [HttpGet("property/{id:guid}")]
    public async Task<ActionResult<ResponseSchemaPropertyFind>> GetSchemaPropertyByID(Guid id)
    {
        return await mediator.Send(new QuerySchemaPropertyFind(id));
    }

    [HttpPost("")]
    public async Task<ActionResult<ResponseSchemaCreate>> CreateSchema(RequestSchemaCreate request)
    {
        return await mediator.Send(new CommandSchemaCreate(request));
    }

    [HttpPost("{id:guid}")]
    public async Task<ActionResult<ResponseSchemaUpdate>> UpdateSchema(Guid id, RequestSchemaUpdate request)
    {
        return await mediator.Send(new CommandSchemaUpdate(id, request));
    }

    [HttpPost("{id:guid}/clone")]
    public async Task<ActionResult<ResponseSchemaClone>> CloneSchema(Guid id)
    {
        return await mediator.Send(new CommandSchemaClone(id));
    }

    [HttpPost("{id:guid}/activate")]
    public async Task<ActionResult<ResponseSchemaActivate>> ActivateSchema(Guid id)
    {
        return await mediator.Send(new CommandSchemaActivate(id));
    }

    [HttpPost("{id:guid}/delete")]
    public async Task<ActionResult<ResponseSchemaDelete>> DeleteSchema(Guid id)
    {
        return await mediator.Send(new CommandSchemaDelete(id));
    }

    [HttpPost("context")]
    public async Task<ActionResult<ResponseSchemaContextCreate>> CreateSchemaContext(RequestSchemaContextCreate request)
    {
        return await mediator.Send(new CommandSchemaContextCreate(request));
    }

    [HttpPost("context/{id:guid}")]
    public async Task<ActionResult<ResponseSchemaContextUpdate>> UpdateSchemaContext(Guid id, RequestSchemaContextUpdate request)
    {
        return await mediator.Send(new CommandSchemaContextUpdate(id, request));
    }

    [HttpPost("context/{id:guid}/clone")]
    public async Task<ActionResult<ResponseSchemaContextClone>> CloneSchemaContext(Guid id)
    {
        return await mediator.Send(new CommandSchemaContextClone(id));
    }

    [HttpPost("context/{id:guid}/delete")]
    public async Task<ActionResult<ResponseSchemaContextDelete>> DeleteSchemaContext(Guid id)
    {
        return await mediator.Send(new CommandSchemaContextDelete(id));
    }

    [HttpPost("context/element")]
    public async Task<ActionResult<ResponseSchemaContextHasElementCreate>> CreateSchemaContextHasElement(RequestSchemaContextHasElementCreate request)
    {
        return await mediator.Send(new CommandSchemaContextHasElementCreate(request));
    }

    [HttpPost("context/element/{id:guid}/Delete")]
    public async Task<ActionResult<ResponseSchemaContextHasElementDelete>> CreateSchemaContextHasElement(Guid id)
    {
        return await mediator.Send(new CommandSchemaContextHasElementDelete(id));
    }

    [HttpPost("element")]
    public async Task<ActionResult<ResponseSchemaElementCreate>> CreateSchemaElement(RequestSchemaElementCreate request)
    {
        return await mediator.Send(new CommandSchemaElementCreate(request));
    }

    [HttpPost("element/{id:guid}")]
    public async Task<ActionResult<ResponseSchemaElementUpdate>> UpdateSchemaElement(Guid id, RequestSchemaElementUpdate request)
    {
        return await mediator.Send(new CommandSchemaElementUpdate(id, request));
    }

    [HttpPost("element/{id:guid}/clone")]
    public async Task<ActionResult<ResponseSchemaElementClone>> CloneSchemaElement(Guid id)
    {
        return await mediator.Send(new CommandSchemaElementClone(id));
    }

    [HttpPost("element/{id:guid}/delete")]
    public async Task<ActionResult<ResponseSchemaElementDelete>> DeleteSchemaElement(Guid id)
    {
        return await mediator.Send(new CommandSchemaElementDelete(id));
    }

    [HttpPost("element/property")]
    public async Task<ActionResult<ResponseSchemaElementHasPropertyCreate>> CreateSchemaElementHasProperty(RequestSchemaElementHasPropertyCreate request)
    {
        return await mediator.Send(new CommandSchemaElementHasPropertyCreate(request));
    }

    [HttpPost("element/property/{id:guid}/Delete")]
    public async Task<ActionResult<ResponseSchemaElementHasPropertyDelete>> CreateSchemaElementHasProperty(Guid id)
    {
        return await mediator.Send(new CommandSchemaElementHasPropertyDelete(id));
    }

    [HttpPost("parameter/style")]
    public async Task<ActionResult<ResponseSchemaParameterStyleCreate>> Create(RequestSchemaParameterStyleCreate model)
    {
        return await mediator.Send(new CommandSchemaParameterStyleCreate(model));
    }

    [HttpPost("parameter/system")]
    public async Task<ActionResult<ResponseSchemaParameterSystemCreate>> Create(RequestSchemaParameterSystemCreate model)
    {
        return await mediator.Send(new CommandSchemaParameterSystemCreate(model));
    }

    [HttpPost("parameter/text")]
    public async Task<ActionResult<ResponseSchemaParameterTextCreate>> Create(RequestSchemaParameterTextCreate model)
    {
        return await mediator.Send(new CommandSchemaParameterTextCreate(model));
    }

    [HttpPost("parameter/style/{id:guid}")]
    public async Task<ActionResult<ResponseSchemaParameterStyleUpdate>> Update(Guid id, RequestSchemaParameterStyleUpdate model)
    {
        return await mediator.Send(new CommandSchemaParameterStyleUpdate(id, model));
    }

    [HttpPost("parameter/system/{id:guid}")]
    public async Task<ActionResult<ResponseSchemaParameterSystemUpdate>> Update(Guid id, RequestSchemaParameterSystemUpdate model)
    {
        return await mediator.Send(new CommandSchemaParameterSystemUpdate(id, model));
    }

    [HttpPost("parameter/text/{id:guid}")]
    public async Task<ActionResult<ResponseSchemaParameterTextUpdate>> Update(Guid id, RequestSchemaParameterTextUpdate model)
    {
        return await mediator.Send(new CommandSchemaParameterTextUpdate(id, model));
    }

    [HttpPost("parameter/{id:guid}/clone")]
    public async Task<ActionResult<ResponseSchemaParameterClone>> CloneSchemaParameter(Guid id)
    {
        return await mediator.Send(new CommandSchemaParameterClone(id));
    }

    [HttpPost("parameter/{id:guid}/delete")]
    public async Task<ActionResult<ResponseSchemaParameterDelete>> DeleteSchemaParameter(Guid id)
    {
        return await mediator.Send(new CommandSchemaParameterDelete(id));
    }

    [HttpPost("property/boolean")]
    public async Task<ActionResult<ResponseSchemaPropertyBooleanCreate>> Create(RequestSchemaPropertyBooleanCreate model)
    {
        return await mediator.Send(new CommandSchemaPropertyBooleanCreate(model));
    }

    [HttpPost("property/datetime")]
    public async Task<ActionResult<ResponseSchemaPropertyDateTimeCreate>> Create(RequestSchemaPropertyDateTimeCreate model)
    {
        return await mediator.Send(new CommandSchemaPropertyDateTimeCreate(model));
    }

    [HttpPost("property/decimal")]
    public async Task<ActionResult<ResponseSchemaPropertyDecimalCreate>> Create(RequestSchemaPropertyDecimalCreate model)
    {
        return await mediator.Send(new CommandSchemaPropertyDecimalCreate(model));
    }

    [HttpPost("property/boolean/{id:guid}")]
    public async Task<ActionResult<ResponseSchemaPropertyBooleanUpdate>> Update(Guid id, RequestSchemaPropertyBooleanUpdate model)
    {
        return await mediator.Send(new CommandSchemaPropertyBooleanUpdate(id, model));
    }

    [HttpPost("property/datetime/{id:guid}")]
    public async Task<ActionResult<ResponseSchemaPropertyDateTimeUpdate>> Update(Guid id, RequestSchemaPropertyDateTimeUpdate model)
    {
        return await mediator.Send(new CommandSchemaPropertyDateTimeUpdate(id, model));
    }

    [HttpPost("property/decimal/{id:guid}")]
    public async Task<ActionResult<ResponseSchemaPropertyDecimalUpdate>> Update(Guid id, RequestSchemaPropertyDecimalUpdate model)
    {
        return await mediator.Send(new CommandSchemaPropertyDecimalUpdate(id, model));
    }

    [HttpPost("property/image/{id:guid}")]
    public async Task<ActionResult<ResponseSchemaPropertyImageUpdate>> Update(Guid id, RequestSchemaPropertyImageUpdate model)
    {
        return await mediator.Send(new CommandSchemaPropertyImageUpdate(id, model));
    }

    [HttpPost("property/integer/{id:guid}")]
    public async Task<ActionResult<ResponseSchemaPropertyIntegerUpdate>> Update(Guid id, RequestSchemaPropertyIntegerUpdate model)
    {
        return await mediator.Send(new CommandSchemaPropertyIntegerUpdate(id, model));
    }

    [HttpPost("property/string/{id:guid}")]
    public async Task<ActionResult<ResponseSchemaPropertyStringUpdate>> Update(Guid id, RequestSchemaPropertyStringUpdate model)
    {
        return await mediator.Send(new CommandSchemaPropertyStringUpdate(id, model));
    }

    [HttpPost("property/{id:guid}/clone")]
    public async Task<ActionResult<ResponseSchemaPropertyClone>> CloneSchemaProperty(Guid id)
    {
        return await mediator.Send(new CommandSchemaPropertyClone(id));
    }

    [HttpPost("property/{id:guid}/delete")]
    public async Task<ActionResult<ResponseSchemaPropertyDelete>> DeleteSchemaProperty(Guid id)
    {
        return await mediator.Send(new CommandSchemaPropertyDelete(id));
    }
}
