using System.Diagnostics.CodeAnalysis;
using Fluxor;
using Noxy.NET.EntityManagement.Administration.Abstractions;
using Noxy.NET.EntityManagement.Domain.Entities.Schemas;
using Noxy.NET.EntityManagement.Domain.Requests.Schema;
using Noxy.NET.EntityManagement.Domain.Responses.Schema;
using Noxy.NET.EntityManagement.Presentation.Services;
using static Noxy.NET.EntityManagement.Administration.Features.FeatureSchemaReducers;

namespace Noxy.NET.EntityManagement.Administration.Features;

public enum FeatureSchemaActionKind
{
    Find,
    List,
    Count,
    Add,
    Update,
    Delete,
    Activate,
    Clone
}

[FeatureState]
public record FeatureSchemaState : BaseFeatureStateRequest<FeatureSchemaActionKind>
{
    public Dictionary<string, int> Count { get; init; } = [];
    public Dictionary<string, EntitySchema> Find { get; init; } = [];
    public Dictionary<string, IReadOnlyList<EntitySchema>> List { get; init; } = [];
    public Dictionary<string, RequestSchemaList> Request { get; init; } = [];

    public bool TryGetLoading(string scope, FeatureSchemaActionKind kind, out bool value) => Loading.TryGetValue(new(scope, kind), out value);
    public bool TryGetError(string scope, FeatureSchemaActionKind kind, out string? error) => Error.TryGetValue(new(scope, kind), out error);

    public bool TryGetCount(string scope, out int value) => Count.TryGetValue(scope, out value);
    public bool TryGetFind(string scope, [NotNullWhen(true)] out EntitySchema? value) => Find.TryGetValue(scope, out value);
    public bool TryGetList(string scope, [NotNullWhen(true)] out IReadOnlyList<EntitySchema>? list) => List.TryGetValue(scope, out list);
    public bool TryGetRequest(string scope, [NotNullWhen(true)] out RequestSchemaList? request) => Request.TryGetValue(scope, out request);
}

public class FeatureSchemaReducers : BaseFeatureReducerRequest<FeatureSchemaState, FeatureSchemaActionKind>
{
    [ReducerMethod]
    public static FeatureSchemaState ReduceFind(FeatureSchemaState state, FindAction action) => StartAction(state, action.Scope, FeatureSchemaActionKind.Find);

    [ReducerMethod]
    public static FeatureSchemaState ReduceFindSuccess(FeatureSchemaState state, SuccessAction<ResponseSchemaFind> action) =>
        HandleSuccessAction(state, action, (next, value) => next with { Find = Set(next.Find, action.Scope, value.Value) });

    [ReducerMethod]
    public static FeatureSchemaState ReduceList(FeatureSchemaState state, ListAction action) =>
        StartAction(state, action.Scope, FeatureSchemaActionKind.List, next => next with { Request = Set(next.Request, action.Scope, action.Request) });

    [ReducerMethod]
    public static FeatureSchemaState ReduceListSuccess(FeatureSchemaState state, SuccessAction<ResponseSchemaList> action) =>
        HandleSuccessAction(state, action, (next, value) => next with { List = Set(next.List, action.Scope, value.Value) });

    [ReducerMethod]
    public static FeatureSchemaState ReduceCount(FeatureSchemaState state, CountAction action) => StartAction(state, action.Scope, FeatureSchemaActionKind.Count);

    [ReducerMethod]
    public static FeatureSchemaState ReduceCountSuccess(FeatureSchemaState state, SuccessAction<ResponseSchemaCount> action) =>
        HandleSuccessAction(state, action, (next, value) => next with { Count = Set(next.Count, action.Scope, value.Value) });

    [ReducerMethod]
    public static FeatureSchemaState ReduceAdd(FeatureSchemaState state, AddAction action) => StartAction(state, action.Scope, FeatureSchemaActionKind.Add);

    [ReducerMethod]
    public static FeatureSchemaState ReduceUpdate(FeatureSchemaState state, UpdateAction action) => StartAction(state, action.Scope, FeatureSchemaActionKind.Update);

    [ReducerMethod]
    public static FeatureSchemaState ReduceClone(FeatureSchemaState state, CloneAction action) => StartAction(state, action.Scope, FeatureSchemaActionKind.Clone);

    [ReducerMethod]
    public static FeatureSchemaState ReduceDelete(FeatureSchemaState state, DeleteAction action) => StartAction(state, action.Scope, FeatureSchemaActionKind.Delete);

    [ReducerMethod]
    public static FeatureSchemaState ReduceActivate(FeatureSchemaState state, ActivateAction action) => StartAction(state, action.Scope, FeatureSchemaActionKind.Activate);

    [ReducerMethod]
    public static FeatureSchemaState ReduceSuccess(FeatureSchemaState state, SuccessAction action) => HandleSuccessAction(state, action);

    [ReducerMethod]
    public static FeatureSchemaState ReduceFailure(FeatureSchemaState state, FailureAction action) => HandleFailureAction(state, action);

    public record FindAction(string Scope, RequestSchemaFind Request);

    public record ListAction(string Scope, RequestSchemaList Request);

    public record CountAction(string Scope, RequestSchemaCount Request);

    public record AddAction(string Scope, RequestSchemaCreate Request);

    public record UpdateAction(string Scope, RequestSchemaUpdate Request);

    public record CloneAction(string Scope, RequestSchemaClone Request);

    public record DeleteAction(string Scope, RequestSchemaDelete Request);

    public record ActivateAction(string Scope, RequestSchemaActivate Request);
}

public class FeatureSchemaListEffects(APIHttpClient client, IState<FeatureSchemaState> state) : BaseFeatureEffectRequest<FeatureSchemaState, FeatureSchemaActionKind>
{
    [EffectMethod]
    public Task Handle(FindAction action, IDispatcher dispatcher) =>
        Execute(new(action.Scope, FeatureSchemaActionKind.Find, false, false), dispatcher, async () => await client.SendRequest(action.Request));

    [EffectMethod]
    public Task Handle(ListAction action, IDispatcher dispatcher) =>
        Execute(new(action.Scope, FeatureSchemaActionKind.List, false, false), dispatcher, async () => await client.SendRequest(action.Request));

    [EffectMethod]
    public Task Handle(CountAction action, IDispatcher dispatcher) =>
        Execute(new(action.Scope, FeatureSchemaActionKind.Count, false, false), dispatcher, async () => await client.SendRequest(action.Request));

    [EffectMethod]
    public Task Handle(AddAction action, IDispatcher dispatcher) =>
        Execute(new(action.Scope, FeatureSchemaActionKind.Add), dispatcher, async () => await client.SendRequest(action.Request));

    [EffectMethod]
    public Task Handle(UpdateAction action, IDispatcher dispatcher) =>
        Execute(new(action.Scope, FeatureSchemaActionKind.Update), dispatcher, async () => await client.SendRequest(action.Request));

    [EffectMethod]
    public Task Handle(CloneAction action, IDispatcher dispatcher) =>
        Execute(new(action.Scope, FeatureSchemaActionKind.Clone), dispatcher, async () => await client.SendRequest(action.Request));

    [EffectMethod]
    public Task Handle(DeleteAction action, IDispatcher dispatcher) =>
        Execute(new(action.Scope, FeatureSchemaActionKind.Delete), dispatcher, async () => await client.SendRequest(action.Request));

    [EffectMethod]
    public Task Handle(ActivateAction action, IDispatcher dispatcher) =>
        Execute(new(action.Scope, FeatureSchemaActionKind.Activate), dispatcher, async () => await client.SendRequest(action.Request));

    protected override void RefreshList(string scope, IDispatcher dispatcher)
    {
        if (!state.Value.TryGetRequest(scope, out RequestSchemaList? request)) return;
        dispatcher.Dispatch(new ListAction(scope, request));
        dispatcher.Dispatch(new CountAction(scope, new() { Search = request.Search }));
    }
}
