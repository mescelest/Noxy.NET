using Microsoft.AspNetCore.Components;
using Noxy.NET.CaseManagement.Domain.ViewModels;
using Noxy.NET.CaseManagement.Presentation.Models;
using Noxy.NET.CaseManagement.Presentation.Services;
using Noxy.NET.UI.Abstractions;

namespace Noxy.NET.CaseManagement.Presentation.Abstractions.Components;

public abstract class BaseComponentActionInput : ElementComponent
{
    [Inject]
    public required ActionHubService ActionHubService { get; set; }

    [Parameter, EditorRequired]
    public required string ID { get; set; }

    [Parameter, EditorRequired]
    public required Guid Reference { get; set; }

    [Parameter, EditorRequired]
    public required ViewModelSchemaAction Action { get; set; }

    [Parameter, EditorRequired]
    public required ViewModelSchemaActionInput ActionInput { get; set; }

    [Parameter, EditorRequired]
    public required ContextActionField Context { get; set; }

    protected async Task Commit()
    {
        await ActionHubService.CommitField(Reference, ActionInput.SchemaIdentifier);
        await InvokeAsync(StateHasChanged);
    }

    protected ContextActionFieldAttribute? GetAttribute(string identifier)
    {
        return Context.CollectionAttribute.GetValueOrDefault(identifier);
    }

    protected T? GetAttributeValue<T>(string identifier) where T : class
    {
        foreach (object? item in GetAttribute(identifier)?.ValueList ?? [])
        {
            if (item is T value)
            {
                return value;
            }
        }

        return null;
    }

    protected List<T> GetAttributeValueList<T>(string identifier) where T : class
    {
        List<T> results = [];

        foreach (object? item in GetAttribute(identifier)?.ValueList ?? [])
        {
            if (item is T value)
            {
                results.Add(value);
            }
        }

        return results;
    }
}
