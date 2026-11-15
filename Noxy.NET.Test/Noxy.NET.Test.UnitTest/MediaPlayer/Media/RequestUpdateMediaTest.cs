using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Noxy.NET.MediaPlayer.Application.Models;
using Noxy.NET.MediaPlayer.Application.Requests.Media;

namespace Noxy.NET.Test.MediaPlayer.Media;

public class RequestUpdateMediaTest
{
    private static List<ValidationResult> Validate(RequestUpdateMedia request)
    {
        List<ValidationResult> results = [];
        Validator.TryValidateObject(request, new(request), results, true);
        return results;
    }

    private static Optional<string?> Text(int length)
    {
        return new(new string('a', length));
    }

    [Fact]
    public void Empty_IsValid()
    {
        Assert.Empty(Validate(new()));
    }

    [Fact]
    public void NullTags_AreValid()
    {
        RequestUpdateMedia request = new()
        {
            Artist = new(null),
            Album = new(null),
            AlbumArtist = new(null),
            Subtitle = new(null),
            Comment = new(null),
            Genre = new(null),
            TrackNumber = new(null),
            DiscNumber = new(null),
            Year = new(null)
        };

        Assert.Empty(Validate(request));
    }

    [Fact]
    public void TitleNull_IsInvalid()
    {
        Assert.Contains("Title", Assert.Single(Validate(new() { Title = new(null) })).MemberNames);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void TitleBlank_IsInvalid(string title)
    {
        Assert.Single(Validate(new() { Title = new(title) }));
    }

    [Fact]
    public void Title255_IsValidAnd256IsInvalid()
    {
        Assert.Empty(Validate(new() { Title = Text(255) }));
        Assert.Single(Validate(new() { Title = Text(256) }));
    }

    [Theory]
    [InlineData("Artist", 256)]
    [InlineData("Album", 256)]
    [InlineData("AlbumArtist", 256)]
    [InlineData("Subtitle", 256)]
    [InlineData("Genre", 128)]
    [InlineData("Comment", 2048)]
    public void LengthLimits_PerField(string member, int limit)
    {
        RequestUpdateMedia Create(int length)
        {
            RequestUpdateMedia request = new();
            (typeof(RequestUpdateMedia).GetProperty(member) ?? throw new InvalidOperationException($"RequestUpdateMedia has no {member} property.")).SetValue(request, Text(length));
            return request;
        }

        Assert.Empty(Validate(Create(limit)));
        Assert.Contains(member, Assert.Single(Validate(Create(limit + 1))).MemberNames);
    }

    [Theory]
    [InlineData("TrackNumber")]
    [InlineData("DiscNumber")]
    [InlineData("Year")]
    public void NumbersOutOfRange_AreInvalid(string member)
    {
        RequestUpdateMedia Create(int value)
        {
            RequestUpdateMedia request = new();
            (typeof(RequestUpdateMedia).GetProperty(member) ?? throw new InvalidOperationException($"RequestUpdateMedia has no {member} property.")).SetValue(request, new Optional<int?>(value));
            return request;
        }

        Assert.Empty(Validate(Create(0)));
        Assert.Empty(Validate(Create(9999)));
        Assert.Contains(member, Assert.Single(Validate(Create(-1))).MemberNames);
        Assert.Single(Validate(Create(10000)));
    }


    [Fact]
    public void IDFields_AreValidOnTheirOwn()
    {
        RequestUpdateMedia request = new()
        {
            ArtistIDList = new([Guid.CreateVersion7()]),
            AlbumID = new(Guid.CreateVersion7()),
            GenreIDList = new(null)
        };

        Assert.Empty(Validate(request));
    }

    [Fact]
    public void AlbumArtistIDListAlone_IsValid()
    {
        Assert.Empty(Validate(new() { AlbumArtistIDList = new([Guid.CreateVersion7()]) }));
    }

    [Theory]
    [InlineData("Artist", "ArtistIDList")]
    [InlineData("Genre", "GenreIDList")]
    [InlineData("AlbumArtist", "AlbumArtistIDList")]
    [InlineData("Album", "AlbumID")]
    public void TextAndIDsForTheSameField_AreInvalid(string text, string ids)
    {
        RequestUpdateMedia request = new();
        SetText(request, text);
        SetIDs(request, ids);

        ValidationResult result = Assert.Single(Validate(request));

        Assert.Contains(text, result.MemberNames);
        Assert.Contains(ids, result.MemberNames);
    }

    [Theory]
    [InlineData("AlbumArtist")]
    [InlineData("AlbumArtistIDList")]
    public void AnAlbumIDWithAnAlbumArtistField_IsInvalid(string other)
    {
        RequestUpdateMedia request = new() { AlbumID = new(Guid.CreateVersion7()) };
        if (other == "AlbumArtist") SetText(request, other);
        else SetIDs(request, other);

        Assert.Single(Validate(request));
    }

    [Fact]
    public void AnIDListOverTheLimit_IsInvalid()
    {
        List<Guid> idList = [.. Enumerable.Range(0, RequestUpdateMedia.MaxIDListLength + 1).Select(_ => Guid.CreateVersion7())];

        Assert.Contains("ArtistIDList", Assert.Single(Validate(new() { ArtistIDList = new(idList) })).MemberNames);
        Assert.Empty(Validate(new() { ArtistIDList = new(idList[..RequestUpdateMedia.MaxIDListLength]) }));
    }

    private static void SetText(RequestUpdateMedia request, string member)
    {
        (typeof(RequestUpdateMedia).GetProperty(member) ?? throw new InvalidOperationException($"RequestUpdateMedia has no {member} property.")).SetValue(request, new Optional<string?>("x"));
    }

    private static void SetIDs(RequestUpdateMedia request, string member)
    {
        PropertyInfo property = typeof(RequestUpdateMedia).GetProperty(member) ?? throw new InvalidOperationException($"RequestUpdateMedia has no {member} property.");
        property.SetValue(request, member == "AlbumID" ? new Optional<Guid?>(Guid.CreateVersion7()) : new Optional<List<Guid>?>([Guid.CreateVersion7()]));
    }

    [Fact]
    public void SeveralProblems_AreAllReported()
    {
        RequestUpdateMedia request = new() { Title = new(""), Year = new(-5), Genre = Text(500) };

        Assert.Equal(3, Validate(request).Count);
    }
}
