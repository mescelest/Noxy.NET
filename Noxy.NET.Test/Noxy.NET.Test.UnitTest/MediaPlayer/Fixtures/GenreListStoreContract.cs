using Noxy.NET.MediaPlayer.Domain.Responses.Metadata;

namespace Noxy.NET.Test.MediaPlayer.Fixtures;

public abstract class GenreListStoreContract : ListStoreContract<ResponseGenre, Guid>
{
    protected override Guid NewKey()
    {
        return Guid.CreateVersion7();
    }

    protected override ResponseGenre MakeItem(string label, Guid key)
    {
        return new() { ID = key, Name = label, MediaFileCount = 3, TimeModified = Start };
    }

    protected override Guid KeyOf(ResponseGenre item)
    {
        return item.ID;
    }

    protected override string LabelOf(ResponseGenre item)
    {
        return item.Name;
    }
}
