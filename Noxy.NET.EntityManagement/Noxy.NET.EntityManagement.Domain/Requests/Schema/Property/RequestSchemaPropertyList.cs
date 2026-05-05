using System.ComponentModel;
using Noxy.NET.EntityManagement.Domain.Abstractions.Requests;
using Noxy.NET.EntityManagement.Domain.Constants;
using Noxy.NET.EntityManagement.Domain.Responses.Schema.Property;

namespace Noxy.NET.EntityManagement.Domain.Requests.Schema.Property;

public class RequestSchemaPropertyList : BaseRequestGetList<ResponseSchemaPropertyList>
{
    public override string APIEndpoint => "Schema/Property";

    public Guid? SchemaID { get; set; }

    [DisplayName(TextConstants.LabelFormPropertyType)]
    [Description(TextConstants.HelpFormPropertyType)]
    public HashSet<string>? PropertyType { get; set; }

    public override Dictionary<string, object?> ToQueryParameters()
    {
        return new()
        {
            [nameof(SchemaID)] = SchemaID,
            [nameof(PropertyType)] = PropertyType,
            [nameof(Search)] = Search,
            [nameof(PageSize)] = PageSize,
            [nameof(PageNumber)] = PageNumber,
            [nameof(SortColumn)] = SortColumn,
            [nameof(SortDirection)] = SortDirection,
        };
    }
}
