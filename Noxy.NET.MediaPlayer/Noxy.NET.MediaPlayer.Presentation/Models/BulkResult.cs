namespace Noxy.NET.MediaPlayer.Presentation.Models;

// What an action on many media files did: how many went through, which did not, and one sentence to show when something did not.
public record BulkResult(int SucceededCount, List<Guid> FailedIDList, string? Message);
