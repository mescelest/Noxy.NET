using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.Http.Connections.Client;
using Microsoft.Extensions.Logging;
using Noxy.NET.MediaPlayer.Presentation.Interfaces.Services;
using Noxy.NET.MediaPlayer.Presentation.Interfaces.Stores;
using Noxy.NET.MediaPlayer.Presentation.Services;

namespace Noxy.NET.Test.MediaPlayer.Fakes;

// The real hub service, reaching the in-memory test server through long polling over the given handler.
public sealed class FakeServerMediaHubService(ISyncService serviceSync, ISessionService serviceSession, IServerAddressStorage storageServerAddress, ILogger<MediaHubService> logger, HttpMessageHandler handler)
    : MediaHubService(serviceSync, serviceSession, storageServerAddress, logger)
{
    protected override void ConfigureConnection(HttpConnectionOptions options)
    {
        options.Transports = HttpTransportType.LongPolling;
        options.HttpMessageHandlerFactory = _ => handler;
    }
}
