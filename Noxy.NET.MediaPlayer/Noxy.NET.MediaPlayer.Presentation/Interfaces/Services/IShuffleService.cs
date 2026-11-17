using Noxy.NET.MediaPlayer.Domain.Enums;
using Noxy.NET.MediaPlayer.Domain.Models;

namespace Noxy.NET.MediaPlayer.Presentation.Interfaces.Services;

public interface IShuffleService
{
    // Always a new list. Off keeps the order; Random is a random permutation; SpreadArtists is a random permutation in which an item is not placed straight after one of the same artist (compared ignoring case; a missing artist never conflicts) when another choice is left.
    List<QueueItem> Order(List<QueueItem> itemList, ShuffleModeEnum mode, Func<Guid, string?> artistOf);
}
