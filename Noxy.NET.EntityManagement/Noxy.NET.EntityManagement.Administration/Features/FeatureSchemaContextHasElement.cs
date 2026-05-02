using System.Diagnostics.CodeAnalysis;
using Fluxor;
using Noxy.NET.EntityManagement.Administration.Abstractions;
using Noxy.NET.EntityManagement.Domain.Entities.Schemas.Junctions;
using Noxy.NET.EntityManagement.Domain.Requests.Schema.ContextHasElement;
using Noxy.NET.EntityManagement.Presentation.Services;
using static Noxy.NET.EntityManagement.Administration.Features.FeatureSchemaContextHasElementReducers;

namespace Noxy.NET.EntityManagement.Administration.Features;

public enum FeatureSchemaContextHasElementActionKind
{
    Find,
    List,
    Add,
    Delete,
}

[FeatureState]
public record FeatureSchemaContextHasElementState : BaseFeatureStateRequest<FeatureSchemaContextHasElementActionKind>
{
    public Dictionary<string, EntitySchemaContextHasElement> Find { get; init; } = [];
    public Dictionary<string, IReadOnlyList<EntitySchemaContextHasElement>> List { get; init; } = [];
    public Dictionary<string, RequestSchemaContextHasElementList> Request { get; init; } = [];

    public bool TryGetLoading(string scope, FeatureSchemaContextHasElementActionKind kind, out bool value) => Loading.TryGetValue(new(scope, kind), out value);
    public bool TryGetError(string scope, FeatureSchemaContextHasElementActionKind kind, out string? error) => Error.TryGetValue(new(scope, kind), out error);

    public bool TryGetFind(string scope, [NotNullWhen(true)] out EntitySchemaContextHasElement? value) => Find.TryGetValue(scope, out value);
    public bool TryGetList(string scope, [NotNullWhen(true)] out IReadOnlyList<EntitySchemaContextHasElement>? list) => List.TryGetValue(scope, out list);
    public bool TryGetRequest(string scope, [NotNullWhen(true)] out RequestSchemaContextHasElementList? request) => Request.TryGetValue(scope, out request);
}

public class FeatureSchemaContextHasElementReducers : BaseFeatureReducerRequest<FeatureSchemaContextHasElementState, FeatureSchemaContextHasElementActionKind>
{
    [ReducerMethod]
    public static FeatureSchemaContextHasElementState ReduceFind(FeatureSchemaContextHasElementState state, FindAction action) => StartAction(state, action.Scope, FeatureSchemaContextHasElementActionKind.Find);

    [ReducerMethod]
    public static FeatureSchemaContextHasElementState ReduceFindSuccess(FeatureSchemaContextHasElementState state, SuccessAction<EntitySchemaContextHasElement> action) =>
        HandleSuccessAction(state, action, (next, value) => next with { Find = Set(next.Find, action.Scope, value) });

    [ReducerMethod]
    public static FeatureSchemaContextHasElementState ReduceFindFailure(FeatureSchemaContextHasElementState state, FailureAction action) => HandleFailureAction(state, action);

    [ReducerMethod]
    public static FeatureSchemaContextHasElementState ReduceList(FeatureSchemaContextHasElementState state, ListAction action) =>
        StartAction(state, action.Scope, FeatureSchemaContextHasElementActionKind.List, next => next with { Request = Set(next.Request, action.Scope, action.Request) });

    [ReducerMethod]
    public static FeatureSchemaContextHasElementState ReduceListSuccess(FeatureSchemaContextHasElementState state, SuccessAction<List<EntitySchemaContextHasElement>> action) =>
        HandleSuccessAction(state, action, (next, value) => next with { List = Set(next.List, action.Scope, value) });

    [ReducerMethod]
    public static FeatureSchemaContextHasElementState ReduceListFailure(FeatureSchemaContextHasElementState state, FailureAction action) => HandleFailureAction(state, action);

    [ReducerMethod]
    public static FeatureSchemaContextHasElementState ReduceAdd(FeatureSchemaContextHasElementState state, AddAction action) => StartAction(state, action.Scope, FeatureSchemaContextHasElementActionKind.Add);

    [ReducerMethod]
    public static FeatureSchemaContextHasElementState ReduceAddSuccess(FeatureSchemaContextHasElementState state, SuccessAction<Guid> action) => HandleSuccessAction(state, action);

    [ReducerMethod]
    public static FeatureSchemaContextHasElementState ReduceAddFailure(FeatureSchemaContextHasElementState state, FailureAction action) => HandleFailureAction(state, action);

    [ReducerMethod]
    public static FeatureSchemaContextHasElementState ReduceDelete(FeatureSchemaContextHasElementState state, DeleteAction action) => StartAction(state, action.Scope, FeatureSchemaContextHasElementActionKind.Delete);

    [ReducerMethod]
    public static FeatureSchemaContextHasElementState ReduceDeleteSuccess(FeatureSchemaContextHasElementState state, SuccessAction<Guid> action) => HandleSuccessAction(state, action);

    [ReducerMethod]
    public static FeatureSchemaContextHasElementState ReduceDeleteFailure(FeatureSchemaContextHasElementState state, FailureAction action) => HandleFailureAction(state, action);

    public record FindAction(string Scope, RequestSchemaContextHasElementFind Request);

    public record ListAction(string Scope, RequestSchemaContextHasElementList Request);

    public record AddAction(string Scope, RequestSchemaContextHasElementCreate Request);

    public record DeleteAction(string Scope, RequestSchemaContextHasElementDelete Request);
}

public class FeatureSchemaContextHasElementListEffects(APIHttpClient client, IState<FeatureSchemaContextHasElementState> state) : BaseFeatureEffectRequest<FeatureSchemaContextHasElementState, FeatureSchemaContextHasElementActionKind>
{
    [EffectMethod]
    public Task Handle(FindAction action, IDispatcher dispatcher)
    {
        return Execute(action.Scope, FeatureSchemaContextHasElementActionKind.Find, dispatcher, async () => (await client.SendRequest(action.Request)).Value);
    }

    [EffectMethod]
    public Task Handle(ListAction action, IDispatcher dispatcher)
    {
        return Execute(action.Scope, FeatureSchemaContextHasElementActionKind.List, dispatcher, async () => (await client.SendRequest(action.Request)).Value);
    }

    [EffectMethod]
    public Task Handle(AddAction action, IDispatcher dispatcher)
    {
        return ExecuteWithRefresh(action.Scope, FeatureSchemaContextHasElementActionKind.Add, dispatcher, async () => (await client.SendRequest(action.Request)).Value);
    }

    [EffectMethod]
    public Task Handle(DeleteAction action, IDispatcher dispatcher)
    {
        return ExecuteWithRefresh(action.Scope, FeatureSchemaContextHasElementActionKind.Delete, dispatcher, async () => (await client.SendRequest(action.Request)).Value);
    }

    protected override void RefreshList(string scope, IDispatcher dispatcher)
    {
        if (state.Value.TryGetRequest(scope, out RequestSchemaContextHasElementList? request))
        {
            dispatcher.Dispatch(new ListAction(scope, request));
        }
    }
}
