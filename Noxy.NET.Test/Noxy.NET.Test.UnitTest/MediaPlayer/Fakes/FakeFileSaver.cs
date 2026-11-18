using Noxy.NET.MediaPlayer.Presentation.Interfaces.Services;

namespace Noxy.NET.Test.MediaPlayer.Fakes;

public sealed class FakeFileSaver : IFileSaver
{
    // The suggested name and the bytes of every file saved, in order.
    public List<(string Name, byte[] Bytes)> SavedList { get; } = [];

    // Names asked for, saved or not.
    public List<string> AskedList { get; } = [];

    // The answer for each call by its index; a call past the list is saved.
    public List<bool> AnswerList { get; } = [];

    public async Task<bool> Save(string suggestedName, Stream content)
    {
        int index = AskedList.Count;
        AskedList.Add(suggestedName);
        if (index < AnswerList.Count && !AnswerList[index]) return false;

        using MemoryStream copy = new();
        await content.CopyToAsync(copy);
        SavedList.Add((suggestedName, copy.ToArray()));

        return true;
    }
}
