using Noxy.NET.MediaPlayer.Domain.Entities;

namespace Noxy.NET.Test.MediaPlayer.Metadata;

public class EntityAlbumTest
{
    [Fact]
    public void BuildIdentityKey_IsTheTitleKeyAPipeAndTheSortedArtistIDsJoinedWithCommas()
    {
        Guid low = Guid.Parse("00000000-0000-0000-0000-000000000001");
        Guid high = Guid.Parse("00000000-0000-0000-0000-000000000002");

        string key = EntityAlbum.BuildIdentityKey("greatest hits", [high, low]);

        Assert.Equal($"greatest hits|{low},{high}", key);
    }

    [Fact]
    public void BuildIdentityKey_WithNoArtists_EndsAfterThePipe()
    {
        Assert.Equal("compilation|", EntityAlbum.BuildIdentityKey("compilation", []));
    }

    [Fact]
    public void BuildIdentityKey_IsTheSameForTheSameSetInAnyOrder()
    {
        Guid first = Guid.CreateVersion7();
        Guid second = Guid.CreateVersion7();

        Assert.Equal(EntityAlbum.BuildIdentityKey("a", [first, second]), EntityAlbum.BuildIdentityKey("a", [second, first]));
    }

    [Fact]
    public void BuildIdentityKey_CollapsesARepeatedArtist()
    {
        Guid artist = Guid.CreateVersion7();

        Assert.Equal(EntityAlbum.BuildIdentityKey("a", [artist]), EntityAlbum.BuildIdentityKey("a", [artist, artist]));
    }
}
