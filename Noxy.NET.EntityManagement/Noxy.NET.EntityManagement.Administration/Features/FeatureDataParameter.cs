using System.Diagnostics.CodeAnalysis;
using Fluxor;
using Noxy.NET.EntityManagement.Administration.Abstractions;
using Noxy.NET.EntityManagement.Domain.Entities.Data.Discriminators;
using Noxy.NET.EntityManagement.Domain.Requests.Data.Parameter;
using Noxy.NET.EntityManagement.Domain.Responses.Data.Parameter;
using Noxy.NET.EntityManagement.Presentation.Services;
using static Noxy.NET.EntityManagement.Administration.Features.FeatureDataParameterReducers;

namespace Noxy.NET.EntityManagement.Administration.Features;

public enum FeatureDataParameterActionKind
{
    Find,
    List,
    Count,
    Add,
    Update,
    Delete,
    Approve,
}

[FeatureState]
public record FeatureDataParameterState : BaseFeatureStateRequest<FeatureDataParameterActionKind>
{
    public Dictionary<string, int> Count { get; init; } = [];
    public Dictionary<string, EntityDataParameter> Find { get; init; } = [];
    public Dictionary<string, IReadOnlyList<EntityDataParameter>> List { get; init; } = [];
    public Dictionary<string, RequestDataParameterList> Request { get; init; } = [];

    public bool TryGetLoading(string scope, FeatureDataParameterActionKind kind, out bool value) => Loading.TryGetValue(new(scope, kind), out value);
    public bool TryGetError(string scope, FeatureDataParameterActionKind kind, out string? error) => Error.TryGetValue(new(scope, kind), out error);

    public bool TryHasListKey(string scope, out bool value) => value = List.ContainsKey(scope);

    public bool TryGetCount(string scope, out int value) => Count.TryGetValue(scope, out value);
    public bool TryGetFind(string scope, [NotNullWhen(true)] out EntityDataParameter? value) => Find.TryGetValue(scope, out value);
    public bool TryGetList(string scope, [NotNullWhen(true)] out IReadOnlyList<EntityDataParameter>? list) => List.TryGetValue(scope, out list);
    public bool TryGetRequest(string scope, [NotNullWhen(true)] out RequestDataParameterList? request) => Request.TryGetValue(scope, out request);

    public bool TryGetSortedList<T>(string scope, [NotNullWhen(true)] out IReadOnlyList<T>? sortedList) where T : EntityDataParameter
    {
        if (List.TryGetValue(scope, out IReadOnlyList<EntityDataParameter>? list))
        {
            sortedList = list.OfType<T>().OrderByDescending(t => t.TimeEffective).ThenByDescending(t => t.TimeCreated).ToList();
            return true;
        }

        sortedList = null;
        return false;
    }
}

public class FeatureDataParameterReducers : BaseFeatureReducerRequest<FeatureDataParameterState, FeatureDataParameterActionKind>
{
    [ReducerMethod]
    public static FeatureDataParameterState ReduceFind(FeatureDataParameterState state, FindAction action) => StartAction(state, action.Scope, FeatureDataParameterActionKind.Find);

    [ReducerMethod]
    public static FeatureDataParameterState ReduceFindSuccess(FeatureDataParameterState state, SuccessAction<ResponseDataParameterFind> action) =>
        HandleSuccessAction(state, action, (next, value) => next with { Find = Set(next.Find, action.Scope, value.Value.GetValue()) });

    [ReducerMethod]
    public static FeatureDataParameterState ReduceList(FeatureDataParameterState state, ListAction action) =>
        StartAction(state, action.Scope, FeatureDataParameterActionKind.List, next => next with { Request = Set(next.Request, action.Scope, action.Request) });

    [ReducerMethod]
    public static FeatureDataParameterState ReduceListSuccess(FeatureDataParameterState state, SuccessAction<ResponseDataParameterList> action) =>
        HandleSuccessAction(state, action, (next, value) => next with { List = Set(next.List, action.Scope, value.Value.Select(x => x.GetValue()).ToList()) });

    [ReducerMethod]
    public static FeatureDataParameterState ReduceCount(FeatureDataParameterState state, CountAction action) => StartAction(state, action.Scope, FeatureDataParameterActionKind.Count);

    [ReducerMethod]
    public static FeatureDataParameterState ReduceCountSuccess(FeatureDataParameterState state, SuccessAction<ResponseDataParameterCount> action) =>
        HandleSuccessAction(state, action, (next, value) => next with { Count = Set(next.Count, action.Scope, value.Value) });

    [ReducerMethod]
    public static FeatureDataParameterState ReduceAdd(FeatureDataParameterState state, AddStyleAction action) => StartAction(state, action.Scope, FeatureDataParameterActionKind.Add);

    [ReducerMethod]
    public static FeatureDataParameterState ReduceAdd(FeatureDataParameterState state, AddSystemAction action) => StartAction(state, action.Scope, FeatureDataParameterActionKind.Add);

    [ReducerMethod]
    public static FeatureDataParameterState ReduceAdd(FeatureDataParameterState state, AddTextAction action) => StartAction(state, action.Scope, FeatureDataParameterActionKind.Add);

    [ReducerMethod]
    public static FeatureDataParameterState ReduceUpdate(FeatureDataParameterState state, UpdateStyleAction action) => StartAction(state, action.Scope, FeatureDataParameterActionKind.Update);

    [ReducerMethod]
    public static FeatureDataParameterState ReduceUpdate(FeatureDataParameterState state, UpdateSystemAction action) => StartAction(state, action.Scope, FeatureDataParameterActionKind.Update);

    [ReducerMethod]
    public static FeatureDataParameterState ReduceUpdate(FeatureDataParameterState state, UpdateTextAction action) => StartAction(state, action.Scope, FeatureDataParameterActionKind.Update);

    [ReducerMethod]
    public static FeatureDataParameterState ReduceDelete(FeatureDataParameterState state, DeleteAction action) => StartAction(state, action.Scope, FeatureDataParameterActionKind.Delete);

    [ReducerMethod]
    public static FeatureDataParameterState ReduceSuccess(FeatureDataParameterState state, SuccessAction action) => HandleSuccessAction(state, action);

    [ReducerMethod]
    public static FeatureDataParameterState ReduceFailure(FeatureDataParameterState state, FailureAction action) => HandleFailureAction(state, action);

    public record FindAction(string Scope, RequestDataParameterFind Request);

    public record ListAction(string Scope, RequestDataParameterList Request);

    public record CountAction(string Scope, RequestDataParameterCount Request);

    public record AddStyleAction(string Scope, RequestDataParameterStyleCreate Request);

    public record AddSystemAction(string Scope, RequestDataParameterSystemCreate Request);

    public record AddTextAction(string Scope, RequestDataParameterTextCreate Request);

    public record UpdateStyleAction(string Scope, RequestDataParameterStyleUpdate Request);

    public record UpdateSystemAction(string Scope, RequestDataParameterSystemUpdate Request);

    public record UpdateTextAction(string Scope, RequestDataParameterTextUpdate Request);

    public record ApproveAction(string Scope, RequestDataParameterApprove Request);

    public record DeleteAction(string Scope, RequestDataParameterDelete Request);
}

public class FeatureDataParameterEffects(APIHttpClient client, IState<FeatureDataParameterState> state) : BaseFeatureEffectRequest<FeatureDataParameterState, FeatureDataParameterActionKind>
{
    [EffectMethod]
    public Task Handle(FindAction action, IDispatcher dispatcher) =>
        Execute(new(action.Scope, FeatureDataParameterActionKind.Find, false, false), dispatcher, async () => await client.SendRequest(action.Request));

    [EffectMethod]
    public Task Handle(ListAction action, IDispatcher dispatcher) =>
        Execute(new(action.Scope, FeatureDataParameterActionKind.List, false, false), dispatcher, async () => await client.SendRequest(action.Request));

    [EffectMethod]
    public Task Handle(CountAction action, IDispatcher dispatcher) =>
        Execute(new(action.Scope, FeatureDataParameterActionKind.Count, false, false), dispatcher, async () => await client.SendRequest(action.Request));

    [EffectMethod]
    public Task Handle(AddStyleAction action, IDispatcher dispatcher) =>
        Execute(new(action.Scope, FeatureDataParameterActionKind.Add), dispatcher, async () => await client.SendRequest(action.Request));

    [EffectMethod]
    public Task Handle(AddSystemAction action, IDispatcher dispatcher) =>
        Execute(new(action.Scope, FeatureDataParameterActionKind.Add), dispatcher, async () => await client.SendRequest(action.Request));

    [EffectMethod]
    public Task Handle(AddTextAction action, IDispatcher dispatcher) =>
        Execute(new(action.Scope, FeatureDataParameterActionKind.Add), dispatcher, async () => await client.SendRequest(action.Request));

    [EffectMethod]
    public Task Handle(UpdateStyleAction action, IDispatcher dispatcher) =>
        Execute(new(action.Scope, FeatureDataParameterActionKind.Update), dispatcher, async () => await client.SendRequest(action.Request));

    [EffectMethod]
    public Task Handle(UpdateSystemAction action, IDispatcher dispatcher) =>
        Execute(new(action.Scope, FeatureDataParameterActionKind.Update), dispatcher, async () => await client.SendRequest(action.Request));

    [EffectMethod]
    public Task Handle(UpdateTextAction action, IDispatcher dispatcher) =>
        Execute(new(action.Scope, FeatureDataParameterActionKind.Update), dispatcher, async () => await client.SendRequest(action.Request));

    [EffectMethod]
    public Task Handle(ApproveAction action, IDispatcher dispatcher) =>
        Execute(new(action.Scope, FeatureDataParameterActionKind.Approve), dispatcher, async () => await client.SendRequest(action.Request));

    [EffectMethod]
    public Task Handle(DeleteAction action, IDispatcher dispatcher) =>
        Execute(new(action.Scope, FeatureDataParameterActionKind.Delete), dispatcher, async () => await client.SendRequest(action.Request));

    protected override void RefreshList(string scope, IDispatcher dispatcher)
    {
        if (!state.Value.TryGetRequest(scope, out RequestDataParameterList? request)) return;
        dispatcher.Dispatch(new ListAction(scope, request));
        dispatcher.Dispatch(new CountAction(scope, new() { SchemaIdentifier = request.SchemaIdentifier, Search = request.Search }));
    }
}
