using System.Net.Http.Headers;
using Noxy.NET.Test.MediaPlayer.Fixtures;

namespace Noxy.NET.Test.MediaPlayer.Api;

public class CorsPolicyTest : IDisposable
{
    private const string AllowedOrigin = "http://localhost:5343";
    private const string OtherOrigin = "http://evil.example";

    private readonly ApiFactory _factory = new();

    public void Dispose()
    {
        _factory.Dispose();
    }

    private async Task<HttpResponseMessage> Get(string origin)
    {
        using HttpClient client = _factory.CreateClient();
        using HttpRequestMessage request = new(HttpMethod.Get, "/Media/Library");
        request.Headers.Add("Origin", origin);

        return await client.SendAsync(request, TestContext.Current.CancellationToken);
    }

    private static List<string> ExposedHeaderList(HttpResponseHeaders headers)
    {
        return headers.TryGetValues("Access-Control-Expose-Headers", out IEnumerable<string>? values)
            ? [.. values.SelectMany(x => x.Split(',')).Select(x => x.Trim())]
            : [];
    }

    [Theory]
    [InlineData("ETag")]
    [InlineData("Content-Range")]
    [InlineData("Accept-Ranges")]
    [InlineData("Content-Disposition")]
    public async Task AnAllowedOrigin_CanReadTheHeader(string header)
    {
        using HttpResponseMessage response = await Get(AllowedOrigin);

        Assert.Contains(header, ExposedHeaderList(response.Headers), StringComparer.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task AnOriginThatIsNotAllowed_GetsNoCorsHeaders()
    {
        using HttpResponseMessage response = await Get(OtherOrigin);

        Assert.False(response.Headers.Contains("Access-Control-Allow-Origin"));
        Assert.Empty(ExposedHeaderList(response.Headers));
    }
}
