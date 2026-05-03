using System.Diagnostics.CodeAnalysis;
using Fluxor;
using Noxy.NET.EntityManagement.Administration.Abstractions;
using Noxy.NET.EntityManagement.Domain.Entities.Schemas;
using Noxy.NET.EntityManagement.Domain.Requests.Schema.Element;
using Noxy.NET.EntityManagement.Domain.Responses.Schema.Element;
using Noxy.NET.EntityManagement.Presentation.Services;
using static Noxy.NET.EntityManagement.Administration.Features.FeatureSchemaElementReducers;

namespace Noxy.NET.EntityManagement.Administration.Features;

public enum FeatureSchemaElementActionKind
{
    Find,
    List,
    Count,
    Add,
    Update,
    Delete,
    Clone
}

[FeatureState]
public record FeatureSchemaElementState : BaseFeatureStateRequest<FeatureSchemaElementActionKind>
{
    public Dictionary<string, int> Count { get; init; } = [];
    public Dictionary<string, EntitySchemaElement> Find { get; init; } = [];
    public Dictionary<string, IReadOnlyList<EntitySchemaElement>> List { get; init; } = [];
    public Dictionary<string, RequestSchemaElementList> Request { get; init; } = [];

    public bool TryGetLoading(string scope, FeatureSchemaElementActionKind kind, out bool value) => Loading.TryGetValue(new(scope, kind), out value);
    public bool TryGetError(string scope, FeatureSchemaElementActionKind kind, out string? error) => Error.TryGetValue(new(scope, kind), out error);

    public bool TryGetCount(string scope, out int value) => Count.TryGetValue(scope, out value);
    public bool TryGetFind(string scope, [NotNullWhen(true)] out EntitySchemaElement? value) => Find.TryGetValue(scope, out value);
    public bool TryGetList(string scope, [NotNullWhen(true)] out IReadOnlyList<EntitySchemaElement>? list) => List.TryGetValue(scope, out list);
    public bool TryGetRequest(string scope, [NotNullWhen(true)] out RequestSchemaElementList? request) => Request.TryGetValue(scope, out request);
}

public class FeatureSchemaElementReducers : BaseFeatureReducerRequest<FeatureSchemaElementState, FeatureSchemaElementActionKind>
{
    [ReducerMethod]
    public static FeatureSchemaElementState ReduceFind(FeatureSchemaElementState state, FindAction action) => StartAction(state, action.Scope, FeatureSchemaElementActionKind.Find);

    [ReducerMethod]
    public static FeatureSchemaElementState ReduceFindSuccess(FeatureSchemaElementState state, SuccessAction<ResponseSchemaElementFind> action) =>
        HandleSuccessAction(state, action, (next, value) => next with { Find = Set(next.Find, action.Scope, value.Value) });

    [ReducerMethod]
    public static FeatureSchemaElementState ReduceList(FeatureSchemaElementState state, ListAction action) =>
        StartAction(state, action.Scope, FeatureSchemaElementActionKind.List, next => next with { Request = Set(next.Request, action.Scope, action.Request) });

    [ReducerMethod]
    public static FeatureSchemaElementState ReduceListSuccess(FeatureSchemaElementState state, SuccessAction<ResponseSchemaElementList> action) =>
        HandleSuccessAction(state, action, (next, value) => next with { List = Set(next.List, action.Scope, value.Value) });

    [ReducerMethod]
    public static FeatureSchemaElementState ReduceCount(FeatureSchemaElementState state, CountAction action) => StartAction(state, action.Scope, FeatureSchemaElementActionKind.Count);

    [ReducerMethod]
    public static FeatureSchemaElementState ReduceCountSuccess(FeatureSchemaElementState state, SuccessAction<ResponseSchemaElementCount> action) =>
        HandleSuccessAction(state, action, (next, value) => next with { Count = Set(next.Count, action.Scope, value.Value) });

    [ReducerMethod]
    public static FeatureSchemaElementState ReduceAdd(FeatureSchemaElementState state, AddAction action) => StartAction(state, action.Scope, FeatureSchemaElementActionKind.Add);

    [ReducerMethod]
    public static FeatureSchemaElementState ReduceUpdate(FeatureSchemaElementState state, UpdateAction action) => StartAction(state, action.Scope, FeatureSchemaElementActionKind.Update);

    [ReducerMethod]
    public static FeatureSchemaElementState ReduceClone(FeatureSchemaElementState state, CloneAction action) => StartAction(state, action.Scope, FeatureSchemaElementActionKind.Clone);

    [ReducerMethod]
    public static FeatureSchemaElementState ReduceDelete(FeatureSchemaElementState state, DeleteAction action) => StartAction(state, action.Scope, FeatureSchemaElementActionKind.Delete);

    [ReducerMethod]
    public static FeatureSchemaElementState ReduceSuccess(FeatureSchemaElementState state, SuccessAction action) => HandleSuccessAction(state, action);

    [ReducerMethod]
    public static FeatureSchemaElementState ReduceFailure(FeatureSchemaElementState state, FailureAction action) => HandleFailureAction(state, action);

    public record FindAction(string Scope, RequestSchemaElementFind Request);

    public record ListAction(string Scope, RequestSchemaElementList Request);

    public record CountAction(string Scope, RequestSchemaElementCount Request);

    public record AddAction(string Scope, RequestSchemaElementCreate Request);

    public record UpdateAction(string Scope, RequestSchemaElementUpdate Request);

    public record CloneAction(string Scope, RequestSchemaElementClone Request);

    public record DeleteAction(string Scope, RequestSchemaElementDelete Request);
}

public class FeatureSchemaElementEffects(APIHttpClient client, IState<FeatureSchemaElementState> state) : BaseFeatureEffectRequest<FeatureSchemaElementState, FeatureSchemaElementActionKind>
{
    [EffectMethod]
    public Task Handle(FindAction action, IDispatcher dispatcher) =>
        Execute(new(action.Scope, FeatureSchemaElementActionKind.Find, false, false), dispatcher, async () => await client.SendRequest(action.Request));

    [EffectMethod]
    public Task Handle(ListAction action, IDispatcher dispatcher) =>
        Execute(new(action.Scope, FeatureSchemaElementActionKind.List, false, false), dispatcher, async () => await client.SendRequest(action.Request));

    [EffectMethod]
    public Task Handle(CountAction action, IDispatcher dispatcher) =>
        Execute(new(action.Scope, FeatureSchemaElementActionKind.Count, false, false), dispatcher, async () => await client.SendRequest(action.Request));

    [EffectMethod]
    public Task Handle(AddAction action, IDispatcher dispatcher) =>
        Execute(new(action.Scope, FeatureSchemaElementActionKind.Add), dispatcher, async () => await client.SendRequest(action.Request));

    [EffectMethod]
    public Task Handle(UpdateAction action, IDispatcher dispatcher) =>
        Execute(new(action.Scope, FeatureSchemaElementActionKind.Update), dispatcher, async () => await client.SendRequest(action.Request));

    [EffectMethod]
    public Task Handle(CloneAction action, IDispatcher dispatcher) =>
        Execute(new(action.Scope, FeatureSchemaElementActionKind.Clone), dispatcher, async () => await client.SendRequest(action.Request));

    [EffectMethod]
    public Task Handle(DeleteAction action, IDispatcher dispatcher) =>
        Execute(new(action.Scope, FeatureSchemaElementActionKind.Delete), dispatcher, async () => await client.SendRequest(action.Request));

    protected override void RefreshList(string scope, IDispatcher dispatcher)
    {
        if (!state.Value.TryGetRequest(scope, out RequestSchemaElementList? request)) return;
        dispatcher.Dispatch(new ListAction(scope, request));
        dispatcher.Dispatch(new CountAction(scope, new() { Search = request.Search }));
    }
}
