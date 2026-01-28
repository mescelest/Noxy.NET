namespace Noxy.NET.CaseManagement.Presentation.Services;

public class TextService(DataAPIService serviceDataAPI)
{
    private readonly Dictionary<string, (string Value, DateTime? TimeResolved)> _collection = [];

    private Task? _taskResolver; 
    private TaskCompletionSource<bool> _taskCompletionSource = new();

    public string Get(string? identifier)
    {
        if (string.IsNullOrWhiteSpace(identifier)) return string.Empty;
        if (_collection.TryGetValue(identifier, out (string Value, DateTime? TimeResolved) item) && item.TimeResolved != null)
        {
            return item.Value;
        }

        _collection[identifier] = (string.Empty, null);
        return _collection[identifier].Value;
    }

    public Task Resolve()
    {
        return _taskResolver ??= ResolveInternally();
    }

    private async Task ResolveInternally()
    {
        while (true)
        {
            if (await Task.WhenAny(Task.Delay(100), _taskCompletionSource.Task) != _taskCompletionSource.Task)
            {
                _taskResolver = null;
                await ResolveInternal();
                break;
            }

            _taskCompletionSource = new();
        }
    }

    private async Task ResolveInternal()
    {
        string[] request = _collection
            .Where(x => x.Value.TimeResolved == null)
            .Select(x => x.Key)
            .ToArray();
        
        if (request.Length == 0) return;
        Dictionary<string, string> result = await serviceDataAPI.ResolveTextParameterList(request);

        DateTime now = DateTime.UtcNow;
        foreach (KeyValuePair<string, string> item in result)
        {
            _collection[item.Key] = (item.Value, now);
        }
    }
}
