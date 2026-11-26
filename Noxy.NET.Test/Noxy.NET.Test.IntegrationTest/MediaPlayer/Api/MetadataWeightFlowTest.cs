using System.Net;
using Noxy.NET.MediaPlayer.Domain.Declarations;
using Noxy.NET.MediaPlayer.Domain.Responses.Metadata;
using Noxy.NET.Test.MediaPlayer.Fixtures;
using Noxy.NET.Test.MediaPlayer.Fixtures.Extensions;

namespace Noxy.NET.Test.MediaPlayer.Api;

public class MetadataWeightFlowTest
{
    private static async Task<T> Send<T>(MediaApiClient api, ApiAccount user, HttpMethod method, string path, string? json = null, HttpStatusCode expected = HttpStatusCode.OK)
    {
        HttpResponseMessage response = await api.SendJson(user.Token, method, path, json);
        Assert.Equal(expected, response.StatusCode);
        return await response.ReadJson<T>();
    }

    [Theory]
    [InlineData("Artist")]
    [InlineData("Genre")]
    public async Task SetWeight_ChangesTheWeight_AndTheListShowsIt(string kind)
    {
        await using ApiFactory factory = new();
        MediaApiClient api = new(factory);
        ApiAccount user = await api.SignUp("a@example.com");
        ResponseArtist created = await Send<ResponseArtist>(api, user, HttpMethod.Post, $"/{kind}", """{"name":"A"}""", HttpStatusCode.Created);
        Assert.Equal(MetadataWeight.Default, created.Weight);

        ResponseArtist changed = await Send<ResponseArtist>(api, user, HttpMethod.Put, $"/{kind}/{created.ID}/Weight", """{"weight":700}""");

        Assert.Equal(700, changed.Weight);
        Assert.Equal(700, Assert.Single(await Send<List<ResponseArtist>>(api, user, HttpMethod.Get, $"/{kind}")).Weight);
    }

    [Theory]
    [InlineData("Artist", -1)]
    [InlineData("Genre", 1001)]
    public async Task SetWeight_OutOfRange_IsABadRequest(string kind, int weight)
    {
        await using ApiFactory factory = new();
        MediaApiClient api = new(factory);
        ApiAccount user = await api.SignUp("a@example.com");
        ResponseArtist created = await Send<ResponseArtist>(api, user, HttpMethod.Post, $"/{kind}", """{"name":"A"}""", HttpStatusCode.Created);

        HttpResponseMessage response = await api.SendJson(user.Token, HttpMethod.Put, $"/{kind}/{created.ID}/Weight", $$"""{"weight":{{weight}}}""");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task SetWeight_OnAnUnknownItem_IsNotFound()
    {
        await using ApiFactory factory = new();
        MediaApiClient api = new(factory);
        ApiAccount user = await api.SignUp("a@example.com");

        HttpResponseMessage response = await api.SendJson(user.Token, HttpMethod.Put, $"/Genre/{Guid.CreateVersion7()}/Weight", """{"weight":5}""");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
