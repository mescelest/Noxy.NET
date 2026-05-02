using System.Diagnostics.CodeAnalysis;
using Fluxor;
using Noxy.NET.EntityManagement.Administration.Abstractions;
using Noxy.NET.EntityManagement.Domain.Entities.Schemas;
using Noxy.NET.EntityManagement.Domain.Requests.Schema.Context;
using Noxy.NET.EntityManagement.Presentation.Services;
using static Noxy.NET.EntityManagement.Administration.Features.FeatureSchemaContextReducers;

namespace Noxy.NET.EntityManagement.Administration.Features;

public enum FeatureSchemaContextActionKind
{
    Find,
    List,
    Add,
    Update,
    Delete,
    Clone
}

[FeatureState]
public record FeatureSchemaContextState : BaseFeatureStateRequest<FeatureSchemaContextActionKind>
{
    public Dictionary<string, EntitySchemaContext> Find { get; init; } = [];
    public Dictionary<string, IReadOnlyList<EntitySchemaContext>> List { get; init; } = [];
    public Dictionary<string, RequestSchemaContextList> Request { get; init; } = [];

    public bool TryGetLoading(string scope, FeatureSchemaContextActionKind kind, out bool value) => Loading.TryGetValue(new(scope, kind), out value);
    public bool TryGetError(string scope, FeatureSchemaContextActionKind kind, out string? error) => Error.TryGetValue(new(scope, kind), out error);

    public bool TryGetFind(string scope, [NotNullWhen(true)] out EntitySchemaContext? value) => Find.TryGetValue(scope, out value);
    public bool TryGetList(string scope, [NotNullWhen(true)] out IReadOnlyList<EntitySchemaContext>? list) => List.TryGetValue(scope, out list);
    public bool TryGetRequest(string scope, [NotNullWhen(true)] out RequestSchemaContextList? request) => Request.TryGetValue(scope, out request);
}

public class FeatureSchemaContextReducers : BaseFeatureReducerRequest<FeatureSchemaContextState, FeatureSchemaContextActionKind>
{
    [ReducerMethod]
    public static FeatureSchemaContextState ReduceList(FeatureSchemaContextState state, ListAction action) =>
        StartAction(state, action.Scope, FeatureSchemaContextActionKind.List, next => next with { Request = Set(next.Request, action.Scope, action.Request) });

    [ReducerMethod]
    public static FeatureSchemaContextState ReduceListSuccess(FeatureSchemaContextState state, SuccessAction<List<EntitySchemaContext>> action) =>
        HandleSuccessAction(state, action, (next, value) => next with { List = Set(next.List, action.Scope, value) });

    [ReducerMethod]
    public static FeatureSchemaContextState ReduceListFailure(FeatureSchemaContextState state, FailureAction action) => HandleFailureAction(state, action);

    [ReducerMethod]
    public static FeatureSchemaContextState ReduceAdd(FeatureSchemaContextState state, AddAction action) => StartAction(state, action.Scope, FeatureSchemaContextActionKind.Add);

    [ReducerMethod]
    public static FeatureSchemaContextState ReduceAddSuccess(FeatureSchemaContextState state, SuccessAction<Guid> action) => HandleSuccessAction(state, action);

    [ReducerMethod]
    public static FeatureSchemaContextState ReduceAddFailure(FeatureSchemaContextState state, FailureAction action) => HandleFailureAction(state, action);

    [ReducerMethod]
    public static FeatureSchemaContextState ReduceUpdate(FeatureSchemaContextState state, UpdateAction action) => StartAction(state, action.Scope, FeatureSchemaContextActionKind.Update);

    [ReducerMethod]
    public static FeatureSchemaContextState ReduceUpdateSuccess(FeatureSchemaContextState state, SuccessAction<Guid> action) => HandleSuccessAction(state, action);

    [ReducerMethod]
    public static FeatureSchemaContextState ReduceUpdateFailure(FeatureSchemaContextState state, FailureAction action) => HandleFailureAction(state, action);

    [ReducerMethod]
    public static FeatureSchemaContextState ReduceClone(FeatureSchemaContextState state, CloneAction action) => StartAction(state, action.Scope, FeatureSchemaContextActionKind.Clone);

    [ReducerMethod]
    public static FeatureSchemaContextState ReduceCloneSuccess(FeatureSchemaContextState state, SuccessAction<Guid> action) => HandleSuccessAction(state, action);

    [ReducerMethod]
    public static FeatureSchemaContextState ReduceCloneFailure(FeatureSchemaContextState state, FailureAction action) => HandleFailureAction(state, action);

    [ReducerMethod]
    public static FeatureSchemaContextState ReduceDelete(FeatureSchemaContextState state, DeleteAction action) => StartAction(state, action.Scope, FeatureSchemaContextActionKind.Delete);

    [ReducerMethod]
    public static FeatureSchemaContextState ReduceDeleteSuccess(FeatureSchemaContextState state, SuccessAction<Guid> action) => HandleSuccessAction(state, action);

    [ReducerMethod]
    public static FeatureSchemaContextState ReduceDeleteFailure(FeatureSchemaContextState state, FailureAction action) => HandleFailureAction(state, action);

    public record FindAction(string Scope, RequestSchemaContextFind Request);

    public record ListAction(string Scope, RequestSchemaContextList Request);

    public record AddAction(string Scope, RequestSchemaContextCreate Request);

    public record UpdateAction(string Scope, RequestSchemaContextUpdate Request);

    public record CloneAction(string Scope, RequestSchemaContextClone Request);

    public record DeleteAction(string Scope, RequestSchemaContextDelete Request);
}

public class FeatureSchemaContextListEffects(APIHttpClient client, IState<FeatureSchemaContextState> state) : BaseFeatureEffectRequest<FeatureSchemaContextState, FeatureSchemaContextActionKind>
{
    [EffectMethod]
    public Task Handle(FindAction action, IDispatcher dispatcher)
    {
        return Execute(action.Scope, FeatureSchemaContextActionKind.Find, dispatcher, async () => (await client.SendRequest(action.Request)).Value);
    }

    [EffectMethod]
    public Task Handle(ListAction action, IDispatcher dispatcher)
    {
        return Execute(action.Scope, FeatureSchemaContextActionKind.List, dispatcher, async () => (await client.SendRequest(action.Request)).Value);
    }

    [EffectMethod]
    public Task Handle(AddAction action, IDispatcher dispatcher)
    {
        return ExecuteWithRefresh(action.Scope, FeatureSchemaContextActionKind.Add, dispatcher, async () => (await client.SendRequest(action.Request)).Value);
    }

    [EffectMethod]
    public Task Handle(UpdateAction action, IDispatcher dispatcher)
    {
        return ExecuteWithRefresh(action.Scope, FeatureSchemaContextActionKind.Update, dispatcher, async () => (await client.SendRequest(action.Request)).Value);
    }

    [EffectMethod]
    public Task Handle(CloneAction action, IDispatcher dispatcher)
    {
        return ExecuteWithRefresh(action.Scope, FeatureSchemaContextActionKind.Clone, dispatcher, async () => (await client.SendRequest(action.Request)).Value);
    }

    [EffectMethod]
    public Task Handle(DeleteAction action, IDispatcher dispatcher)
    {
        return ExecuteWithRefresh(action.Scope, FeatureSchemaContextActionKind.Delete, dispatcher, async () => (await client.SendRequest(action.Request)).Value);
    }

    protected override void RefreshList(string scope, IDispatcher dispatcher)
    {
        if (state.Value.TryGetRequest(scope, out RequestSchemaContextList? request))
        {
            dispatcher.Dispatch(new ListAction(scope, request));
        }
    }
}
