using System.ComponentModel;
using Noxy.NET.MediaPlayer.Domain.Enums;
using Noxy.NET.MediaPlayer.Domain.Models;
using Noxy.NET.MediaPlayer.Presentation.Interfaces.Services;

namespace Noxy.NET.MediaPlayer.Presentation.Services;

public class ShuffleService(Random random) : IShuffleService
{
    public List<QueueItem> Order(List<QueueItem> itemList, ShuffleModeEnum mode, Func<Guid, string?> artistOf)
    {
        return mode switch
        {
            ShuffleModeEnum.Off => [.. itemList],
            ShuffleModeEnum.Random => Permute(itemList),
            ShuffleModeEnum.SpreadArtists => Spread(Permute(itemList), artistOf),
            _ => throw new InvalidEnumArgumentException(nameof(mode), (int)mode, typeof(ShuffleModeEnum))
        };
    }

    // Fisher-Yates.
    private List<QueueItem> Permute(List<QueueItem> itemList)
    {
        List<QueueItem> result = [.. itemList];
        for (int index = result.Count - 1; index > 0; index--)
        {
            int other = random.Next(index + 1);
            (result[index], result[other]) = (result[other], result[index]);
        }

        return result;
    }

    // One pass: take the next item unless its artist is the one just taken; then take the first later item of another artist, and the next one anyway when none is left.
    private static List<QueueItem> Spread(List<QueueItem> permuted, Func<Guid, string?> artistOf)
    {
        List<(QueueItem Item, string? Artist)> remaining = permuted.Select(x => (x, artistOf(x.MediaFileID))).ToList();
        List<QueueItem> result = new(permuted.Count);
        string? previous = null;

        while (remaining.Count > 0)
        {
            int index = previous is null ? 0 : remaining.FindIndex(x => !string.Equals(x.Artist, previous, StringComparison.OrdinalIgnoreCase));
            if (index < 0) index = 0;

            result.Add(remaining[index].Item);
            previous = remaining[index].Artist;
            remaining.RemoveAt(index);
        }

        return result;
    }
}
