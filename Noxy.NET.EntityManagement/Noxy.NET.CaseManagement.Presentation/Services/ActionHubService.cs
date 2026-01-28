using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;
using Noxy.NET.CaseManagement.Application.Interfaces.Hubs;
using Noxy.NET.CaseManagement.Domain.Models;
using Noxy.NET.CaseManagement.Presentation.Abstractions.Components;
using Noxy.NET.CaseManagement.Presentation.Models;
using Noxy.NET.Models;

namespace Noxy.NET.CaseManagement.Presentation.Services;

public class ActionHubService
{
    private readonly HubConnection _hubConnection;
    private readonly Dictionary<string, Type> _collectionActionInput = [];
    private readonly Dictionary<Guid, ContextAction> _collectionActionContext = [];

    public ActionHubService(UserAuthenticationStateProvider serviceAuthentication, IConfiguration serviceConfiguration)
    {
        _hubConnection = new HubConnectionBuilder()
            .WithUrl($"{serviceConfiguration["Backend:URL"]}/ActionHub", options => { options.AccessTokenProvider = () => Task.FromResult(serviceAuthentication.Identity?.RawData); })
            .WithAutomaticReconnect()
            .Build();
    }

    public void RegisterActionInput<TComponent>(string? identifier) where TComponent : BaseComponentActionInput
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(identifier);
        _collectionActionInput[identifier] = typeof(TComponent);
    }

    public Type GetActionInput(string? identifier)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(identifier);
        return _collectionActionInput[identifier];
    }

    public async Task<ContextAction> RegisterAction(Guid id, string identifier, Dictionary<string, object?>? context = null)
    {
        if (_hubConnection.State == HubConnectionState.Disconnected) await _hubConnection.StartAsync();
        
        StateAction state = await _hubConnection.InvokeAsync<StateAction>(nameof(IActionServerHub.Register), id, identifier, context);
        return _collectionActionContext[id] = new(id, state);
    }

    public void DeregisterAction(Guid id)
    {
        _hubConnection.SendAsync(nameof(IActionServerHub.Deregister), id);
    }
    
    public async Task<ContextAction> CommitField(Guid id, string identifierInput)
    {
        ContextAction contextAction = _collectionActionContext[id];
        ContextActionField contextActionField = contextAction.CollectionField[identifierInput];
        
        StateAction result = await _hubConnection.InvokeAsync<StateAction>(nameof(IActionServerHub.CommitField), id, identifierInput, new JsonProperty(contextActionField.Value));
        return contextAction.Apply(result);
    }

    public async Task<object?> Submit(Guid id)
    {
        return await _hubConnection.InvokeAsync<StateAction>(nameof(IActionServerHub.Submit), id);
    }
}
