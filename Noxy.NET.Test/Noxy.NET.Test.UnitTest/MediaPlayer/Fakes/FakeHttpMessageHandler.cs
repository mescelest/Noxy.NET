using System.Net;
using System.Text;

namespace Noxy.NET.Test.MediaPlayer.Fakes;

public sealed class FakeHttpMessageHandler : HttpMessageHandler
{
    public sealed record RecordedRequest(HttpMethod Method, Uri? Uri, string? Authorization, string? IfNoneMatch, string? Body, string? Range = null, string? IfRange = null);

    public List<RecordedRequest> RequestList { get; } = [];

    // Decides the answer to each request. The default answers 200 with an empty JSON object.
    public Func<RecordedRequest, HttpResponseMessage> Respond { get; set; } = _ => Json(HttpStatusCode.OK, "{}");

    // Makes every request fail as if the network were down.
    public Exception? Failure { get; set; }

    public static HttpResponseMessage Json(HttpStatusCode status, string json)
    {
        return new(status) { Content = new StringContent(json, Encoding.UTF8, "application/json") };
    }

    public static HttpResponseMessage Problem(HttpStatusCode status, string title)
    {
        return new(status) { Content = new StringContent($"{{\"title\":\"{title}\",\"status\":{(int)status}}}", Encoding.UTF8, "application/problem+json") };
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        string? body = request.Content is null ? null : await request.Content.ReadAsStringAsync(cancellationToken);
        RecordedRequest recorded = new(request.Method, request.RequestUri, request.Headers.Authorization?.ToString(), request.Headers.IfNoneMatch.ToString() is { Length: > 0 } ifNoneMatch ? ifNoneMatch : null, body, request.Headers.Range?.ToString(),
            request.Headers.IfRange?.ToString());
        RequestList.Add(recorded);

        if (Failure is not null) throw Failure;

        return Respond(recorded);
    }
}
