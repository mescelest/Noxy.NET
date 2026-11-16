using System.Net;

namespace Noxy.NET.Test.MediaPlayer.Fakes;

// Stands between the app's HTTP clients and the in-memory test server: it records every request, and can cut the network.
public sealed class FakeNetworkHandler(HttpMessageHandler inner) : DelegatingHandler(inner)
{
    private readonly Lock _lock = new();
    private CancellationTokenSource _drop = new();
    private bool _down;

    public List<(HttpMethod Method, string Path, string? IfNoneMatch, HttpStatusCode? Status)> RequestList { get; } = [];

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
            lock (_lock)
            {
                RequestList.Add((request.Method, request.RequestUri?.AbsolutePath ?? string.Empty, request.Headers.IfNoneMatch.FirstOrDefault()?.Tag, response.StatusCode));
            }

            return response;
        }
        catch (OperationCanceledException) when (drop.IsCancellationRequested && !cancellationToken.IsCancellationRequested)
        {
            throw new HttpRequestException("The network went down.");
        }
    }
}
