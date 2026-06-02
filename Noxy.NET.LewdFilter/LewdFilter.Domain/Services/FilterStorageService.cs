using System.Text.Json;
using System.Text.Json.Serialization;
using LewdFilter.Domain.Models;

namespace LewdFilter.Domain.Services;

public class FilterStorageService
{
    private readonly JsonSerializerOptions _options = new()
    {
        WriteIndented = true,
        // Automatically maps and preserves shared color references in memory
        ReferenceHandler = ReferenceHandler.Preserve,
        Converters = { new JsonStringEnumConverter() }
    };

    // Automatically maps and preserves shared color references in memory

    public string SaveFilterToJson(Filter filter)
    {
        return JsonSerializer.Serialize(filter, _options);
    }

    public Filter? LoadFilterFromJson(string jsonContent)
    {
        return JsonSerializer.Deserialize<Filter>(jsonContent, _options);
    }
}
