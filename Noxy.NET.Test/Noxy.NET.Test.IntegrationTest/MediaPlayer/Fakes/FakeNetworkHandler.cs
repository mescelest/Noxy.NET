using System.Net;

namespace Noxy.NET.Test.MediaPlayer.Fakes;

public sealed class FakeNetworkHandler(HttpMessageHandler inner) : DelegatingHandler(inner)
{
    private readonly Lock _lock = new();
    private CancellationTokenSource _drop = new();
    private bool _down;

    private const string FileSegment = "/File";

    private int? _cutNextFileAfterBytes;

    public List<(HttpMethod Method, string Path, string? IfNoneMatch, HttpStatusCode? Status)> RequestList { get; } = [];

    // The Range header (null when there was none) of every request for a media file's bytes, in order.
    public List<string?> FileRangeList { get; } = [];

    // The next file response delivers only this many bytes and then fails, once.
    public void CutNextFileResponseAfter(int bytes)
    {
        lock (_lock) _cutNextFileAfterBytes = bytes;
    }

    // Cuts every request in flight (a waiting long poll too) and fails the ones that follow, until Restore.
    public void Drop()
    {
        lock (_lock)
        {
            _down = true;
            _drop.Cancel();
        }
    }

    public void Restore()
    {
        lock (_lock)
        {
            _down = false;
            _drop = new();
        }
    }

    // The hub connection and the HTTP clients share one handler and each disposes what it was given: this handler stays alive until the test server goes.
    protected override void Dispose(bool disposing) { }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        CancellationTokenSource drop;
        lock (_lock)
        {
            if (_down) throw new HttpRequestException("The network is down.");
            drop = _drop;
        }

        using CancellationTokenSource linked = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, drop.Token);
        try
        {
            HttpResponseMessage response = await base.SendAsync(request, linked.Token);
            string path = request.RequestUri?.AbsolutePath ?? string.Empty;
            lock (_lock)
            {
                RequestList.Add((request.Method, path, request.Headers.IfNoneMatch.FirstOrDefault()?.Tag, response.StatusCode));

                if (path.EndsWith(FileSegment, StringComparison.Ordinal))
                {
                    FileRangeList.Add(request.Headers.Range?.ToString());

                    if (_cutNextFileAfterBytes is { } bytes && response.IsSuccessStatusCode)
                    {
                        response.Content = new FakeCuttingContent(response.Content, bytes);
                        _cutNextFileAfterBytes = null;
                    }
                }
            }

            return response;
        }
        catch (OperationCanceledException) when (drop.IsCancellationRequested && !cancellationToken.IsCancellationRequested)
        {
            throw new HttpRequestException("The network went down.");
        }
    }
}
