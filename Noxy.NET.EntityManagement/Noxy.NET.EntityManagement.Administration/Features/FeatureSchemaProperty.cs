using System.Diagnostics.CodeAnalysis;
using Fluxor;
using Noxy.NET.EntityManagement.Administration.Abstractions;
using Noxy.NET.EntityManagement.Domain.Entities.Schemas.Discriminators;
using Noxy.NET.EntityManagement.Domain.Requests.Schema.Property;
using Noxy.NET.EntityManagement.Domain.Responses.Schema.Property;
using Noxy.NET.EntityManagement.Presentation.Services;
using static Noxy.NET.EntityManagement.Administration.Features.FeatureSchemaPropertyReducers;

namespace Noxy.NET.EntityManagement.Administration.Features;

public enum FeatureSchemaPropertyActionKind
{
    Find,
    List,
    Add,
    Update,
    Delete,
    Clone
}

[FeatureState]
public record FeatureSchemaPropertyState : BaseFeatureStateRequest<FeatureSchemaPropertyActionKind>
{
    public Dictionary<string, EntitySchemaProperty.Discriminator> Find { get; init; } = [];
    public Dictionary<string, IReadOnlyList<EntitySchemaProperty.Discriminator>> List { get; init; } = [];
    public Dictionary<string, RequestSchemaPropertyList> Request { get; init; } = [];

    public bool TryGetLoading(string scope, FeatureSchemaPropertyActionKind kind, out bool value) => Loading.TryGetValue(new(scope, kind), out value);
    public bool TryGetError(string scope, FeatureSchemaPropertyActionKind kind, out string? error) => Error.TryGetValue(new(scope, kind), out error);

    public bool TryGetFind(string scope, [NotNullWhen(true)] out EntitySchemaProperty.Discriminator? value) => Find.TryGetValue(scope, out value);
    public bool TryGetList(string scope, [NotNullWhen(true)] out IReadOnlyList<EntitySchemaProperty.Discriminator>? list) => List.TryGetValue(scope, out list);
    public bool TryGetRequest(string scope, [NotNullWhen(true)] out RequestSchemaPropertyList? request) => Request.TryGetValue(scope, out request);
}

public class FeatureSchemaPropertyReducers : BaseFeatureReducerRequest<FeatureSchemaPropertyState, FeatureSchemaPropertyActionKind>
{
    [ReducerMethod]
    public static FeatureSchemaPropertyState ReduceFind(FeatureSchemaPropertyState state, FindAction action) =>
        StartAction(state, action.Scope, FeatureSchemaPropertyActionKind.Find);

    [ReducerMethod]
    public static FeatureSchemaPropertyState ReduceFindSuccess(FeatureSchemaPropertyState state, SuccessAction<ResponseSchemaPropertyFind> action) =>
        HandleSuccessAction(state, action, (next, value) => next with { Find = Set(next.Find, action.Scope, value.Value) });

    [ReducerMethod]
    public static FeatureSchemaPropertyState ReduceList(FeatureSchemaPropertyState state, ListAction action) =>
        StartAction(state, action.Scope, FeatureSchemaPropertyActionKind.List, next => next with { Request = Set(next.Request, action.Scope, action.Request) });

    [ReducerMethod]
    public static FeatureSchemaPropertyState ReduceListSuccess(FeatureSchemaPropertyState state, SuccessAction<ResponseSchemaPropertyList> action) =>
        HandleSuccessAction(state, action, (next, value) => next with { List = Set(next.List, action.Scope, value.Value) });

    [ReducerMethod]
    public static FeatureSchemaPropertyState ReduceAddBoolean(FeatureSchemaPropertyState state, AddBooleanAction action) => StartAction(state, action.Scope, FeatureSchemaPropertyActionKind.Add);

    [ReducerMethod]
    public static FeatureSchemaPropertyState ReduceAddDateTime(FeatureSchemaPropertyState state, AddDateTimeAction action) => StartAction(state, action.Scope, FeatureSchemaPropertyActionKind.Add);

    [ReducerMethod]
    public static FeatureSchemaPropertyState ReduceAddDecimal(FeatureSchemaPropertyState state, AddDecimalAction action) => StartAction(state, action.Scope, FeatureSchemaPropertyActionKind.Add);

    [ReducerMethod]
    public static FeatureSchemaPropertyState ReduceAddImage(FeatureSchemaPropertyState state, AddImageAction action) => StartAction(state, action.Scope, FeatureSchemaPropertyActionKind.Add);

    [ReducerMethod]
    public static FeatureSchemaPropertyState ReduceAddInteger(FeatureSchemaPropertyState state, AddIntegerAction action) => StartAction(state, action.Scope, FeatureSchemaPropertyActionKind.Add);

    [ReducerMethod]
    public static FeatureSchemaPropertyState ReduceAddString(FeatureSchemaPropertyState state, AddStringAction action) => StartAction(state, action.Scope, FeatureSchemaPropertyActionKind.Add);

    [ReducerMethod]
    public static FeatureSchemaPropertyState ReduceUpdateBoolean(FeatureSchemaPropertyState state, UpdateBooleanAction action) => StartAction(state, action.Scope, FeatureSchemaPropertyActionKind.Update);

    [ReducerMethod]
    public static FeatureSchemaPropertyState ReduceUpdateDateTime(FeatureSchemaPropertyState state, UpdateDateTimeAction action) => StartAction(state, action.Scope, FeatureSchemaPropertyActionKind.Update);

    [ReducerMethod]
    public static FeatureSchemaPropertyState ReduceUpdateDecimal(FeatureSchemaPropertyState state, UpdateDecimalAction action) => StartAction(state, action.Scope, FeatureSchemaPropertyActionKind.Update);

    [ReducerMethod]
    public static FeatureSchemaPropertyState ReduceUpdateImage(FeatureSchemaPropertyState state, UpdateImageAction action) => StartAction(state, action.Scope, FeatureSchemaPropertyActionKind.Update);

    [ReducerMethod]
    public static FeatureSchemaPropertyState ReduceUpdateInteger(FeatureSchemaPropertyState state, UpdateIntegerAction action) => StartAction(state, action.Scope, FeatureSchemaPropertyActionKind.Update);

    [ReducerMethod]
    public static FeatureSchemaPropertyState ReduceUpdateString(FeatureSchemaPropertyState state, UpdateStringAction action) => StartAction(state, action.Scope, FeatureSchemaPropertyActionKind.Update);

    [ReducerMethod]
    public static FeatureSchemaPropertyState ReduceClone(FeatureSchemaPropertyState state, CloneAction action) => StartAction(state, action.Scope, FeatureSchemaPropertyActionKind.Clone);

    [ReducerMethod]
    public static FeatureSchemaPropertyState ReduceDelete(FeatureSchemaPropertyState state, DeleteAction action) => StartAction(state, action.Scope, FeatureSchemaPropertyActionKind.Delete);

    [ReducerMethod]
    public static FeatureSchemaPropertyState ReduceSuccess(FeatureSchemaPropertyState state, SuccessAction action) => HandleSuccessAction(state, action);

    [ReducerMethod]
    public static FeatureSchemaPropertyState ReduceFailure(FeatureSchemaPropertyState state, FailureAction action) => HandleFailureAction(state, action);

    public record FindAction(string Scope, RequestSchemaPropertyFind Request);

    public record ListAction(string Scope, RequestSchemaPropertyList Request);

    public record AddBooleanAction(string Scope, RequestSchemaPropertyBooleanCreate Request);

    public record AddDateTimeAction(string Scope, RequestSchemaPropertyDateTimeCreate Request);

    public record AddDecimalAction(string Scope, RequestSchemaPropertyDecimalCreate Request);

    public record AddIntegerAction(string Scope, RequestSchemaPropertyIntegerCreate Request);

    public record AddImageAction(string Scope, RequestSchemaPropertyImageCreate Request);

    public record AddStringAction(string Scope, RequestSchemaPropertyStringCreate Request);

    public record UpdateBooleanAction(string Scope, RequestSchemaPropertyBooleanUpdate Request);

    public record UpdateDateTimeAction(string Scope, RequestSchemaPropertyDateTimeUpdate Request);

    public record UpdateDecimalAction(string Scope, RequestSchemaPropertyDecimalUpdate Request);

    public record UpdateImageAction(string Scope, RequestSchemaPropertyImageUpdate Request);

    public record UpdateIntegerAction(string Scope, RequestSchemaPropertyIntegerUpdate Request);

    public record UpdateStringAction(string Scope, RequestSchemaPropertyStringUpdate Request);

    public record CloneAction(string Scope, RequestSchemaPropertyClone Request);

    public record DeleteAction(string Scope, RequestSchemaPropertyDelete Request);
}

public class FeatureSchemaPropertyEffects(APIHttpClient client, IState<FeatureSchemaPropertyState> state) : BaseFeatureEffectRequest<FeatureSchemaPropertyState, FeatureSchemaPropertyActionKind>
{
    [EffectMethod]
    public Task Handle(FindAction action, IDispatcher dispatcher) =>
        Execute(new(action.Scope, FeatureSchemaPropertyActionKind.Find, false, false), dispatcher, async () => await client.SendRequest(action.Request));

    [EffectMethod]
    public Task Handle(ListAction action, IDispatcher dispatcher) =>
        Execute(new(action.Scope, FeatureSchemaPropertyActionKind.List, false, false), dispatcher, async () => await client.SendRequest(action.Request));

    [EffectMethod]
    public Task Handle(AddBooleanAction action, IDispatcher dispatcher) =>
        Execute(new(action.Scope, FeatureSchemaPropertyActionKind.Add), dispatcher, async () => await client.SendRequest(action.Request));

    [EffectMethod]
    public Task Handle(AddDateTimeAction action, IDispatcher dispatcher) =>
        Execute(new(action.Scope, FeatureSchemaPropertyActionKind.Add), dispatcher, async () => await client.SendRequest(action.Request));

    [EffectMethod]
    public Task Handle(AddDecimalAction action, IDispatcher dispatcher) =>
        Execute(new(action.Scope, FeatureSchemaPropertyActionKind.Add), dispatcher, async () => await client.SendRequest(action.Request));

    [EffectMethod]
    public Task Handle(AddIntegerAction action, IDispatcher dispatcher) =>
        Execute(new(action.Scope, FeatureSchemaPropertyActionKind.Add), dispatcher, async () => await client.SendRequest(action.Request));

    [EffectMethod]
    public Task Handle(AddImageAction action, IDispatcher dispatcher) =>
        Execute(new(action.Scope, FeatureSchemaPropertyActionKind.Add), dispatcher, async () => await client.SendRequest(action.Request));

    [EffectMethod]
    public Task Handle(AddStringAction action, IDispatcher dispatcher) =>
        Execute(new(action.Scope, FeatureSchemaPropertyActionKind.Add), dispatcher, async () => await client.SendRequest(action.Request));

    [EffectMethod]
    public Task Handle(UpdateBooleanAction action, IDispatcher dispatcher) =>
        Execute(new(action.Scope, FeatureSchemaPropertyActionKind.Update), dispatcher, async () => await client.SendRequest(action.Request));

    [EffectMethod]
    public Task Handle(UpdateDateTimeAction action, IDispatcher dispatcher) =>
        Execute(new(action.Scope, FeatureSchemaPropertyActionKind.Update), dispatcher, async () => await client.SendRequest(action.Request));

    [EffectMethod]
    public Task Handle(UpdateDecimalAction action, IDispatcher dispatcher) =>
        Execute(new(action.Scope, FeatureSchemaPropertyActionKind.Update), dispatcher, async () => await client.SendRequest(action.Request));

    [EffectMethod]
    public Task Handle(UpdateImageAction action, IDispatcher dispatcher) =>
        Execute(new(action.Scope, FeatureSchemaPropertyActionKind.Update), dispatcher, async () => await client.SendRequest(action.Request));

    [EffectMethod]
    public Task Handle(UpdateIntegerAction action, IDispatcher dispatcher) =>
        Execute(new(action.Scope, FeatureSchemaPropertyActionKind.Update), dispatcher, async () => await client.SendRequest(action.Request));

    [EffectMethod]
    public Task Handle(UpdateStringAction action, IDispatcher dispatcher) =>
        Execute(new(action.Scope, FeatureSchemaPropertyActionKind.Update), dispatcher, async () => await client.SendRequest(action.Request));

    [EffectMethod]
    public Task Handle(CloneAction action, IDispatcher dispatcher) =>
        Execute(new(action.Scope, FeatureSchemaPropertyActionKind.Clone), dispatcher, async () => await client.SendRequest(action.Request));

    [EffectMethod]
    public Task Handle(DeleteAction action, IDispatcher dispatcher) =>
        Execute(new(action.Scope, FeatureSchemaPropertyActionKind.Delete), dispatcher, async () => await client.SendRequest(action.Request));

    protected override void RefreshList(string scope, IDispatcher dispatcher)
    {
        if (state.Value.TryGetRequest(scope, out RequestSchemaPropertyList? request))
        {
            dispatcher.Dispatch(new ListAction(scope, request));
        }
    }
}
