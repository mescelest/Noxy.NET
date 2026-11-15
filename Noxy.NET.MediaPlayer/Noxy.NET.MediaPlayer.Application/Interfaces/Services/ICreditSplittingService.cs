namespace Noxy.NET.MediaPlayer.Application.Interfaces.Services;

public interface ICreditSplittingService
{
    // Splits on ";" and on the whole words "feat.", "ft." and "featuring" (any case, also inside brackets, which are removed). Never on "&", "," or "/", which real names contain. Values are trimmed, empties dropped, and repeats removed by key keeping the first spelling and the order.
    List<string> Split(string? credit);

    // The same over a tag's own list of values, flattened and de-duplicated across all of them.
    List<string> Split(IEnumerable<string> valueList);
}
