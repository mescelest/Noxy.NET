using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.SignalR;
using Noxy.NET.MediaPlayer.API.Extensions;
using Noxy.NET.MediaPlayer.Application.Interfaces.Services;
using Noxy.NET.MediaPlayer.Application.Requests.Remote;
using Noxy.NET.MediaPlayer.Domain.Enums;
using Noxy.NET.MediaPlayer.Domain.Exceptions;
using Noxy.NET.MediaPlayer.Domain.Models;
using Noxy.NET.MediaPlayer.Domain.Responses.Remote;

namespace Noxy.NET.MediaPlayer.API.Hubs;

public class MediaHub(IDeviceRegistryService serviceDeviceRegistry, ILiveSessionService serviceLiveSession, ILogger<MediaHub> logger) : Hub
{
    [HubMethodName(nameof(MediaHubMethodEnum.RegisterDevice))]
    public async Task<List<ResponseDevice>> RegisterDevice(RequestRegisterDevice request)
    {
        Guid userID = GetUserID();
        request.EnsureValid(Context.GetHttpContext()?.RequestServices);

        try
        {
            await serviceDeviceRegistry.Register(userID, request, Context.ConnectionId);
        }
        catch (TooManyDevicesException ex)
        {
            throw new HubException(ex.Message);
        }

        // A device that joins while somebody listens gets the queue and the state at once.
        ResponseQueue? queue = serviceLiveSession.GetQueue(userID);
        ResponsePlayerState? state = serviceLiveSession.GetState(userID);
        if (queue is not null) await Clients.Caller.SendAsync(nameof(MediaHubEventEnum.QueueSnapshot), queue);
        if (state is not null) await Clients.Caller.SendAsync(nameof(MediaHubEventEnum.PlayerStateChanged), state);

        return serviceDeviceRegistry.GetDeviceList(userID, serviceLiveSession.GetActiveDeviceID(userID));
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        try
        {
            await serviceDeviceRegistry.Disconnect(Context.ConnectionId);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Could not remove the device of a closed connection");
        }

        await base.OnDisconnectedAsync(exception);
    }

    [HubMethodName(nameof(MediaHubMethodEnum.ClaimActive))]
    public async Task<bool> ClaimActive(RequestClaimActive request)
    {
        (Guid userID, Guid deviceID) = RequireDevice();
        request.EnsureValid();
        request.State.EnsureValid();

        try
        {
            return await serviceLiveSession.Claim(userID, deviceID, request);
        }
        catch (InvalidQueueEditException ex)
        {
            throw new HubException(ex.Message);
        }
    }

    [HubMethodName(nameof(MediaHubMethodEnum.ReportState))]
    public async Task ReportState(RequestReportPlayerState state)
    {
        (Guid userID, Guid deviceID) = RequireDevice();
        state.EnsureValid();

        // A report from a device that is not the active one is ignored, not an error: it is a late message of a device that lost the role.
        await serviceLiveSession.Report(userID, deviceID, state);
    }

    [HubMethodName(nameof(MediaHubMethodEnum.EditQueue))]
    public async Task EditQueue(QueueEdit edit)
    {
        (Guid userID, Guid deviceID) = RequireDevice();
        edit.EnsureValid();

        try
        {
            await serviceLiveSession.Edit(userID, deviceID, edit, Context.ConnectionAborted);
        }
        catch (InvalidQueueEditException ex)
        {
            throw new HubException(ex.Message);
        }
        catch (MediaNotFoundException)
        {
            throw new HubException("The media file was not found.");
        }

        if (serviceLiveSession.GetQueue(userID) is null) throw new HubException("There is no live session.");
    }

    [HubMethodName(nameof(MediaHubMethodEnum.RequestSnapshot))]
    public Task<ResponseQueue?> RequestSnapshot()
    {
        (Guid userID, _) = RequireDevice();

        return Task.FromResult(serviceLiveSession.GetQueue(userID));
    }

    // A command is never queued: with no active device it is answered at once. It is not acknowledged as done either: the result is the next state report or queue delta.
    [HubMethodName(nameof(MediaHubMethodEnum.SendCommand))]
    public async Task<CommandResultEnum> SendCommand(RequestPlayerCommand command)
    {
        (Guid userID, _) = RequireDevice();
        if (!Validator.TryValidateObject(command, new(command), [], true)) return CommandResultEnum.Invalid;

        Guid? activeDeviceID = serviceLiveSession.GetActiveDeviceID(userID);
        string? connectionID = activeDeviceID is { } active ? serviceDeviceRegistry.GetConnectionID(userID, active) : null;
        if (connectionID is null) return CommandResultEnum.NoActiveDevice;

        serviceLiveSession.Touch(userID);
        await Clients.Client(connectionID).SendAsync(nameof(MediaHubEventEnum.PlayerCommandReceived), command);
        return CommandResultEnum.Sent;
    }

    // True when the transfer is under way; it finishes in the background and its failure is pushed as TransferFailed.
    [HubMethodName(nameof(MediaHubMethodEnum.Transfer))]
    public async Task<bool> Transfer(Guid targetDeviceID)
    {
        (Guid userID, _) = RequireDevice();

        // The sends run after this call is over, and a hub instance lives only for one call, so the clients are taken now.
        IHubCallerClients clients = Clients;

        return await serviceLiveSession.Transfer(userID, targetDeviceID,
            deviceID => SendToDevice(clients, userID, deviceID, MediaHubEventEnum.PlayerTransferRequested),
            (deviceID, state, queue) => SendToDevice(clients, userID, deviceID, MediaHubEventEnum.PlayerStartRequested, state, queue));
    }

    private Task SendToDevice(IHubCallerClients clients, Guid userID, Guid deviceID, MediaHubEventEnum eventKind, params object?[] argumentList)
    {
        string connectionID = serviceDeviceRegistry.GetConnectionID(userID, deviceID) ?? throw new HubException("The device is offline.");

        return clients.Client(connectionID).SendCoreAsync(eventKind.ToString(), argumentList);
    }

    private Guid GetUserID()
    {
        if (Guid.TryParse(Context.UserIdentifier, out Guid userID)) return userID;

        Context.Abort();
        throw new HubException("The connection has no user.");
    }

    // Every method after registering belongs to a registered device of the caller's own account.
    private (Guid UserID, Guid DeviceID) RequireDevice()
    {
        Guid userID = GetUserID();
        (Guid UserID, Guid DeviceID)? found = serviceDeviceRegistry.Find(Context.ConnectionId);
        if (found is not { } device || device.UserID != userID) throw new HubException("This connection is not a registered device.");

        return (device.UserID, device.DeviceID);
    }
}
