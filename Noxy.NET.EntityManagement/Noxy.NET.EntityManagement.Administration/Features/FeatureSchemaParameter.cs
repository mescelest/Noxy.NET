using System.Diagnostics.CodeAnalysis;
using Fluxor;
using Noxy.NET.EntityManagement.Administration.Abstractions;
using Noxy.NET.EntityManagement.Domain.Entities.Schemas;
using Noxy.NET.EntityManagement.Domain.Entities.Schemas.Discriminators;
using Noxy.NET.EntityManagement.Domain.Requests.Schema.Parameter;
using Noxy.NET.EntityManagement.Presentation.Services;
using static Noxy.NET.EntityManagement.Administration.Features.FeatureSchemaParameterReducers;

namespace Noxy.NET.EntityManagement.Administration.Features;

public enum FeatureSchemaParameterActionKind
{
    Find,
    List,
    Add,
    Update,
    Delete,
    Clone
}

[FeatureState]
public record FeatureSchemaParameterState : BaseFeatureStateRequest<FeatureSchemaParameterActionKind>
{
    public Dictionary<string, EntitySchemaParameter.Discriminator> Find { get; init; } = [];
    public Dictionary<string, IReadOnlyList<EntitySchemaParameter.Discriminator>> List { get; init; } = [];
    public Dictionary<string, RequestSchemaParameterList> Request { get; init; } = [];

    public bool TryGetLoading(string scope, FeatureSchemaParameterActionKind kind, out bool value) => Loading.TryGetValue(new(scope, kind), out value);
    public bool TryGetError(string scope, FeatureSchemaParameterActionKind kind, out string? error) => Error.TryGetValue(new(scope, kind), out error);

    public bool TryGetFind(string scope, [NotNullWhen(true)] out EntitySchemaParameter.Discriminator? value) => Find.TryGetValue(scope, out value);
    public bool TryGetList(string scope, [NotNullWhen(true)] out IReadOnlyList<EntitySchemaParameter.Discriminator>? list) => List.TryGetValue(scope, out list);
    public bool TryGetRequest(string scope, [NotNullWhen(true)] out RequestSchemaParameterList? request) => Request.TryGetValue(scope, out request);
}

public class FeatureSchemaParameterReducers : BaseFeatureReducerRequest<FeatureSchemaParameterState, FeatureSchemaParameterActionKind>
{
    [ReducerMethod]
    public static FeatureSchemaParameterState ReduceFind(FeatureSchemaParameterState state, FindAction action) =>
        StartAction(state, action.Scope, FeatureSchemaParameterActionKind.Find);

    [ReducerMethod]
    public static FeatureSchemaParameterState ReduceFindSuccess(FeatureSchemaParameterState state, SuccessAction<EntitySchemaParameter.Discriminator> action) =>
        HandleSuccessAction(state, action, (next, value) => next with { Find = Set(next.Find, action.Scope, value) });

    [ReducerMethod]
    public static FeatureSchemaParameterState ReduceFindFailure(FeatureSchemaParameterState state, FailureAction action) => HandleFailureAction(state, action);

    [ReducerMethod]
    public static FeatureSchemaParameterState ReduceList(FeatureSchemaParameterState state, ListAction action) =>
        StartAction(state, action.Scope, FeatureSchemaParameterActionKind.List, next => next with { Request = Set(next.Request, action.Scope, action.Request) });

    [ReducerMethod]
    public static FeatureSchemaParameterState ReduceListSuccess(FeatureSchemaParameterState state, SuccessAction<List<EntitySchemaParameter.Discriminator>> action) =>
        HandleSuccessAction(state, action, (next, value) => next with { List = Set(next.List, action.Scope, value) });

    [ReducerMethod]
    public static FeatureSchemaParameterState ReduceListFailure(FeatureSchemaParameterState state, FailureAction action) => HandleFailureAction(state, action);

    [ReducerMethod]
    public static FeatureSchemaParameterState ReduceAddText(FeatureSchemaParameterState state, AddTextAction action) => StartAction(state, action.Scope, FeatureSchemaParameterActionKind.Add);

    [ReducerMethod]
    public static FeatureSchemaParameterState ReduceAddTextSuccess(FeatureSchemaParameterState state, SuccessAction<EntitySchemaParameterText> action) => HandleSuccessAction(state, action);

    [ReducerMethod]
    public static FeatureSchemaParameterState ReduceAddTextFailure(FeatureSchemaParameterState state, FailureAction action) => HandleFailureAction(state, action);

    [ReducerMethod]
    public static FeatureSchemaParameterState ReduceUpdateStyle(FeatureSchemaParameterState state, UpdateStyleAction action) => StartAction(state, action.Scope, FeatureSchemaParameterActionKind.Update);

    [ReducerMethod]
    public static FeatureSchemaParameterState ReduceUpdateStyleSuccess(FeatureSchemaParameterState state, SuccessAction<EntitySchemaParameterStyle> action) => HandleSuccessAction(state, action);

    [ReducerMethod]
    public static FeatureSchemaParameterState ReduceUpdateStyleFailure(FeatureSchemaParameterState state, FailureAction action) => HandleFailureAction(state, action);

    [ReducerMethod]
    public static FeatureSchemaParameterState ReduceUpdateSystem(FeatureSchemaParameterState state, UpdateSystemAction action) => StartAction(state, action.Scope, FeatureSchemaParameterActionKind.Update);

    [ReducerMethod]
    public static FeatureSchemaParameterState ReduceUpdateSystemSuccess(FeatureSchemaParameterState state, SuccessAction<EntitySchemaParameterSystem> action) => HandleSuccessAction(state, action);

    [ReducerMethod]
    public static FeatureSchemaParameterState ReduceUpdateSystemFailure(FeatureSchemaParameterState state, FailureAction action) => HandleFailureAction(state, action);

    [ReducerMethod]
    public static FeatureSchemaParameterState ReduceUpdateText(FeatureSchemaParameterState state, UpdateTextAction action) => StartAction(state, action.Scope, FeatureSchemaParameterActionKind.Update);

    [ReducerMethod]
    public static FeatureSchemaParameterState ReduceUpdateTextSuccess(FeatureSchemaParameterState state, SuccessAction<EntitySchemaParameterText> action) => HandleSuccessAction(state, action);

    [ReducerMethod]
    public static FeatureSchemaParameterState ReduceUpdateTextFailure(FeatureSchemaParameterState state, FailureAction action) => HandleFailureAction(state, action);

    [ReducerMethod]
    public static FeatureSchemaParameterState ReduceClone(FeatureSchemaParameterState state, CloneAction action) => StartAction(state, action.Scope, FeatureSchemaParameterActionKind.Clone);

    [ReducerMethod]
    public static FeatureSchemaParameterState ReduceCloneSuccess(FeatureSchemaParameterState state, SuccessAction<EntitySchemaParameterText> action) => HandleSuccessAction(state, action);

    [ReducerMethod]
    public static FeatureSchemaParameterState ReduceCloneFailure(FeatureSchemaParameterState state, FailureAction action) => HandleFailureAction(state, action);

    [ReducerMethod]
    public static FeatureSchemaParameterState ReduceDelete(FeatureSchemaParameterState state, DeleteAction action) => StartAction(state, action.Scope, FeatureSchemaParameterActionKind.Delete);

    [ReducerMethod]
    public static FeatureSchemaParameterState ReduceDeleteSuccess(FeatureSchemaParameterState state, SuccessAction<EntitySchemaParameterText> action) => HandleSuccessAction(state, action);

    [ReducerMethod]
    public static FeatureSchemaParameterState ReduceDeleteFailure(FeatureSchemaParameterState state, FailureAction action) => HandleFailureAction(state, action);

    public record FindAction(string Scope, RequestSchemaParameterFind Request);

    public record ListAction(string Scope, RequestSchemaParameterList Request);

    public record AddStyleAction(string Scope, RequestSchemaParameterStyleCreate Request);

    public record AddSystemAction(string Scope, RequestSchemaParameterSystemCreate Request);

    public record AddTextAction(string Scope, RequestSchemaParameterTextCreate Request);

    public record UpdateStyleAction(string Scope, RequestSchemaParameterStyleUpdate Request);

    public record UpdateSystemAction(string Scope, RequestSchemaParameterSystemUpdate Request);

    public record UpdateTextAction(string Scope, RequestSchemaParameterTextUpdate Request);

    public record CloneAction(string Scope, RequestSchemaParameterClone Request);

    public record DeleteAction(string Scope, RequestSchemaParameterDelete Request);
}

public class FeatureSchemaParameterEffects(APIHttpClient client, IState<FeatureSchemaParameterState> state) : BaseFeatureEffectRequest<FeatureSchemaParameterState, FeatureSchemaParameterActionKind>
{
    [EffectMethod]
    public Task Handle(FindAction action, IDispatcher dispatcher) => Execute(action.Scope, FeatureSchemaParameterActionKind.Find, dispatcher, async () => (await client.SendRequest(action.Request)).Value);

    [EffectMethod]
    public Task Handle(ListAction action, IDispatcher dispatcher) => Execute(action.Scope, FeatureSchemaParameterActionKind.List, dispatcher, async () => (await client.SendRequest(action.Request)).Value);

    [EffectMethod]
    public Task Handle(AddStyleAction action, IDispatcher dispatcher) => ExecuteWithRefresh(action.Scope, FeatureSchemaParameterActionKind.Add, dispatcher, async () => (await client.SendRequest(action.Request)).Value);

    [EffectMethod]
    public Task Handle(AddSystemAction action, IDispatcher dispatcher) => ExecuteWithRefresh(action.Scope, FeatureSchemaParameterActionKind.Add, dispatcher, async () => (await client.SendRequest(action.Request)).Value);

    [EffectMethod]
    public Task Handle(AddTextAction action, IDispatcher dispatcher) => ExecuteWithRefresh(action.Scope, FeatureSchemaParameterActionKind.Add, dispatcher, async () => (await client.SendRequest(action.Request)).Value);

    [EffectMethod]
    public Task Handle(UpdateStyleAction action, IDispatcher dispatcher) => ExecuteWithRefresh(action.Scope, FeatureSchemaParameterActionKind.Update, dispatcher, async () => (await client.SendRequest(action.Request)).Value);

    [EffectMethod]
    public Task Handle(UpdateSystemAction action, IDispatcher dispatcher) => ExecuteWithRefresh(action.Scope, FeatureSchemaParameterActionKind.Update, dispatcher, async () => (await client.SendRequest(action.Request)).Value);

    [EffectMethod]
    public Task Handle(UpdateTextAction action, IDispatcher dispatcher) => ExecuteWithRefresh(action.Scope, FeatureSchemaParameterActionKind.Update, dispatcher, async () => (await client.SendRequest(action.Request)).Value);

    [EffectMethod]
    public Task Handle(CloneAction action, IDispatcher dispatcher) => Execute(action.Scope, FeatureSchemaParameterActionKind.Clone, dispatcher, async () => (await client.SendRequest(action.Request)).Value);

    [EffectMethod]
    public Task Handle(DeleteAction action, IDispatcher dispatcher) => Execute(action.Scope, FeatureSchemaParameterActionKind.Delete, dispatcher, async () => (await client.SendRequest(action.Request)).Value);

    protected override void RefreshList(string scope, IDispatcher dispatcher)
    {
        if (state.Value.TryGetRequest(scope, out RequestSchemaParameterList? request))
        {
            dispatcher.Dispatch(new ListAction(scope, request));
        }
    }
}
