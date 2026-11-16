using Noxy.NET.MediaPlayer.Application.Requests.Remote;
using Noxy.NET.MediaPlayer.Domain.Models;
using Noxy.NET.MediaPlayer.Domain.Responses.Remote;

namespace Noxy.NET.MediaPlayer.Application.Interfaces.Services;

public interface ILiveSessionService
{
    // The reconnect rule: refused only while another online device is active and playing. On acceptance the claimant is the active device and its queue replaces the session's.
    Task<bool> Claim(Guid userID, Guid deviceID, RequestClaimActive request);

    // A state report: accepted only from the active device and only with a higher revision than the last one.
    Task<bool> Report(Guid userID, Guid deviceID, RequestReportPlayerState state);

    // Applies a queue edit from any device (the active-device-only kinds from the active device alone). Null when there is no session or the edit changed nothing.
    Task<ResponseQueueDelta?> Edit(Guid userID, Guid deviceID, QueueEdit edit, CancellationToken cancellationToken = default);

    ResponseQueue? GetQueue(Guid userID);

    ResponsePlayerState? GetState(Guid userID);

    Guid? GetActiveDeviceID(Guid userID);

    // Moves playback to another online device of the account. Returns false when it is refused (no session, no active device, the target is offline or already active, or another transfer is pending); true once it is under way.
    // The active device is asked for a final state, then the target is started with it and the queue, and the target must claim. A failure is pushed as a transfer failed notification and the old device stays active.
    Task<bool> Transfer(Guid userID, Guid targetDeviceID, Func<Guid, Task> requestFinalState, Func<Guid, ResponsePlayerState, ResponseQueue, Task> startTarget);

    // A command was sent: the idle countdown starts over.
    void Touch(Guid userID);
}
