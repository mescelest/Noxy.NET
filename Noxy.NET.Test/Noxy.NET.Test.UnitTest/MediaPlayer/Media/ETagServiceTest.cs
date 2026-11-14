using Noxy.NET.MediaPlayer.Application.Services;

namespace Noxy.NET.Test.MediaPlayer.Media;

public class ETagServiceTest
{
    private readonly ETagService _service = new();

    private static readonly Guid FirstID = Guid.Parse("00000000-0000-7000-8000-000000000001");
    private static readonly Guid SecondID = Guid.Parse("00000000-0000-7000-8000-000000000002");
    private static readonly DateTime Time = new(2026, 1, 2, 3, 4, 5, DateTimeKind.Utc);

    [Fact]
    public void Compute_EmptyList_IsStable()
    {
        Assert.Equal(_service.Compute([]), _service.Compute([]));
    }

    [Fact]
    public void Compute_IsAQuotedLowercaseSha256()
    {
        Assert.Matches("^\"[0-9a-f]{64}\"$", _service.Compute([(FirstID, Time)]));
    }

    [Fact]
    public void Compute_IgnoresTheOrderOfTheInput()
    {
        string forward = _service.Compute([(FirstID, Time), (SecondID, Time)]);
        string backward = _service.Compute([(SecondID, Time), (FirstID, Time)]);

        Assert.Equal(forward, backward);
    }

    [Fact]
    public void Compute_ChangesWhenAnIdOrATimeChanges()
    {
        string original = _service.Compute([(FirstID, Time)]);

        Assert.NotEqual(original, _service.Compute([(SecondID, Time)]));
        Assert.NotEqual(original, _service.Compute([(FirstID, Time.AddTicks(1))]));
    }

    [Fact]
    public void Compute_ChangesWhenAnItemIsAddedOrRemoved()
    {
        string one = _service.Compute([(FirstID, Time)]);
        string two = _service.Compute([(FirstID, Time), (SecondID, Time)]);

        Assert.NotEqual(one, two);
        Assert.NotEqual(one, _service.Compute([]));
    }

    [Theory]
    [InlineData("{0}")]
    [InlineData("W/{0}")]
    [InlineData("\"other\", {0}")]
    [InlineData("{0} , \"other\"")]
    [InlineData("*")]
    public void Matches_AcceptsTheTagInAnyOfTheHeaderForms(string template)
    {
        string eTag = _service.Compute([(FirstID, Time)]);

        Assert.True(_service.Matches(string.Format(template, eTag), eTag));
    }

    [Theory]
    [InlineData("\"other\"")]
    [InlineData("garbage")]
    [InlineData("")]
    [InlineData("   ")]
    public void Matches_RejectsAMismatchedOrMalformedHeader(string header)
    {
        Assert.False(_service.Matches(header, _service.Compute([(FirstID, Time)])));
    }
}
