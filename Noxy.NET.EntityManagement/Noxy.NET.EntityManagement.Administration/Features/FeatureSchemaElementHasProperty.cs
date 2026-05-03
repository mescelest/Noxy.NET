using System.Diagnostics.CodeAnalysis;
using Fluxor;
using Noxy.NET.EntityManagement.Administration.Abstractions;
using Noxy.NET.EntityManagement.Domain.Entities.Schemas.Junctions;
using Noxy.NET.EntityManagement.Domain.Requests.Schema.ElementHasProperty;
using Noxy.NET.EntityManagement.Domain.Responses.Schema.ElementHasProperty;
using Noxy.NET.EntityManagement.Presentation.Services;
using static Noxy.NET.EntityManagement.Administration.Features.FeatureSchemaElementHasPropertyReducers;

namespace Noxy.NET.EntityManagement.Administration.Features;

public enum FeatureSchemaElementHasPropertyActionKind
{
    Find,
    List,
    Add,
    Delete,
}

[FeatureState]
public record FeatureSchemaElementHasPropertyState : BaseFeatureStateRequest<FeatureSchemaElementHasPropertyActionKind>
{
    public Dictionary<string, EntitySchemaElementHasProperty> Find { get; init; } = [];
    public Dictionary<string, IReadOnlyList<EntitySchemaElementHasProperty>> List { get; init; } = [];
    public Dictionary<string, RequestSchemaElementHasPropertyList> Request { get; init; } = [];

    public bool TryGetLoading(string scope, FeatureSchemaElementHasPropertyActionKind kind, out bool value) => Loading.TryGetValue(new(scope, kind), out value);
    public bool TryGetError(string scope, FeatureSchemaElementHasPropertyActionKind kind, out string? error) => Error.TryGetValue(new(scope, kind), out error);

    public bool TryGetFind(string scope, [NotNullWhen(true)] out EntitySchemaElementHasProperty? value) => Find.TryGetValue(scope, out value);
    public bool TryGetList(string scope, [NotNullWhen(true)] out IReadOnlyList<EntitySchemaElementHasProperty>? list) => List.TryGetValue(scope, out list);
    public bool TryGetRequest(string scope, [NotNullWhen(true)] out RequestSchemaElementHasPropertyList? request) => Request.TryGetValue(scope, out request);
}

public class FeatureSchemaElementHasPropertyReducers : BaseFeatureReducerRequest<FeatureSchemaElementHasPropertyState, FeatureSchemaElementHasPropertyActionKind>
{
    [ReducerMethod]
    public static FeatureSchemaElementHasPropertyState ReduceFind(FeatureSchemaElementHasPropertyState state, FindAction action) => StartAction(state, action.Scope, FeatureSchemaElementHasPropertyActionKind.Find);

    [ReducerMethod]
    public static FeatureSchemaElementHasPropertyState ReduceFindSuccess(FeatureSchemaElementHasPropertyState state, SuccessAction<ResponseSchemaElementHasPropertyFind> action) =>
        HandleSuccessAction(state, action, (next, value) => next with { Find = Set(next.Find, action.Scope, value.Value) });

    [ReducerMethod]
    public static FeatureSchemaElementHasPropertyState ReduceList(FeatureSchemaElementHasPropertyState state, ListAction action) =>
        StartAction(state, action.Scope, FeatureSchemaElementHasPropertyActionKind.List, next => next with { Request = Set(next.Request, action.Scope, action.Request) });

    [ReducerMethod]
    public static FeatureSchemaElementHasPropertyState ReduceListSuccess(FeatureSchemaElementHasPropertyState state, SuccessAction<ResponseSchemaElementHasPropertyList> action) =>
        HandleSuccessAction(state, action, (next, value) => next with { List = Set(next.List, action.Scope, value.Value) });

    [ReducerMethod]
    public static FeatureSchemaElementHasPropertyState ReduceAdd(FeatureSchemaElementHasPropertyState state, AddAction action) => StartAction(state, action.Scope, FeatureSchemaElementHasPropertyActionKind.Add);

    [ReducerMethod]
    public static FeatureSchemaElementHasPropertyState ReduceDelete(FeatureSchemaElementHasPropertyState state, DeleteAction action) => StartAction(state, action.Scope, FeatureSchemaElementHasPropertyActionKind.Delete);

    [ReducerMethod]
    public static FeatureSchemaElementHasPropertyState ReduceSuccess(FeatureSchemaElementHasPropertyState state, SuccessAction action) => HandleSuccessAction(state, action);

    [ReducerMethod]
    public static FeatureSchemaElementHasPropertyState ReduceFailure(FeatureSchemaElementHasPropertyState state, FailureAction action) => HandleFailureAction(state, action);

    public record FindAction(string Scope, RequestSchemaElementHasPropertyFind Request);

    public record ListAction(string Scope, RequestSchemaElementHasPropertyList Request);

    public record AddAction(string Scope, RequestSchemaElementHasPropertyCreate Request);

    public record DeleteAction(string Scope, RequestSchemaElementHasPropertyDelete Request);
}

public class FeatureSchemaElementHasPropertyEffects(APIHttpClient client, IState<FeatureSchemaElementHasPropertyState> state) : BaseFeatureEffectRequest<FeatureSchemaElementHasPropertyState, FeatureSchemaElementHasPropertyActionKind>
{
    [EffectMethod]
    public Task Handle(FindAction action, IDispatcher dispatcher) =>
        Execute(new(action.Scope, FeatureSchemaElementHasPropertyActionKind.Find, false, false), dispatcher, async () => await client.SendRequest(action.Request));

    [EffectMethod]
    public Task Handle(ListAction action, IDispatcher dispatcher) =>
        Execute(new(action.Scope, FeatureSchemaElementHasPropertyActionKind.List, false, false), dispatcher, async () => await client.SendRequest(action.Request));

    [EffectMethod]
    public Task Handle(AddAction action, IDispatcher dispatcher) =>
        Execute(new(action.Scope, FeatureSchemaElementHasPropertyActionKind.Add), dispatcher, async () => await client.SendRequest(action.Request));

    [EffectMethod]
    public Task Handle(DeleteAction action, IDispatcher dispatcher) =>
        Execute(new(action.Scope, FeatureSchemaElementHasPropertyActionKind.Delete), dispatcher, async () => await client.SendRequest(action.Request));

    protected override void RefreshList(string scope, IDispatcher dispatcher)
    {
        if (state.Value.TryGetRequest(scope, out RequestSchemaElementHasPropertyList? request))
        {
            dispatcher.Dispatch(new ListAction(scope, request));
        }
    }
}
