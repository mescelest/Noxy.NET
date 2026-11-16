using System.Buffers.Text;
using System.Collections.Specialized;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Noxy.NET.MediaPlayer.Presentation.Interfaces.Services;
using Noxy.NET.MediaPlayer.Presentation.Models;

// ReSharper disable once CheckNamespace
namespace Noxy.NET.MediaPlayer.App.Services;

// The system browser signs the user in at Google and sends the browser back to a listener on a free port of this machine; the code in that redirect is exchanged for the ID token. Nothing from the exchange is logged or stored.
public class GoogleSignInService(IHttpClientFactory factoryHttpClient) : IGoogleSignInService
{
    private const int StateByteCount = 32;
    private const int TimeoutMinutes = 5;
    private const string ResponseTypeCode = "code";
    private const string CodeKey = "code";
    private const string ErrorKey = "error";
    private const string StateKey = "state";
    private const string GrantTypeAuthorizationCode = "authorization_code";
    private const string IdTokenProperty = "id_token";
    private const string ClosePage = "<html><body><p>You can close this tab and go back to the app.</p></body></html>";

    public async Task<string?> GetIdToken(CancellationToken cancellationToken)
    {
        string clientSecret = await ReadClientSecret(cancellationToken);
        PkceChallenge pkce = PkceChallenge.Create();
        string state = Base64Url.EncodeToString(RandomNumberGenerator.GetBytes(StateByteCount));

        using HttpListener listener = new();
        string redirectUri = Listen(listener);
        using CancellationTokenSource timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(TimeSpan.FromMinutes(TimeoutMinutes));

        try
        {
            if (!await Launcher.Default.OpenAsync(BuildAuthorizationUri(pkce, state, redirectUri))) return null;

            string? code = await WaitForCode(listener, state, timeout.Token);
            if (code is null) return null;

            return await ExchangeCode(code, pkce.Verifier, redirectUri, clientSecret, timeout.Token);
        }
        catch (OperationCanceledException)
        {
            // The user gave up, or nobody came back from the browser in time.
            return null;
        }
    }

    private static async Task<string> ReadClientSecret(CancellationToken cancellationToken)
    {
        string path = Path.Combine(AppContext.BaseDirectory, GoogleConstants.SecretFileName);
        if (!File.Exists(path)) throw new InvalidOperationException($"The Google client secret file {GoogleConstants.SecretFileName} is missing next to the app.");

        await using FileStream stream = File.OpenRead(path);
        using JsonDocument document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
        if (document.RootElement.TryGetProperty(GoogleConstants.SecretFileProperty, out JsonElement element) && element.ValueKind == JsonValueKind.String && !string.IsNullOrWhiteSpace(element.GetString()))
        {
            return element.GetString() ?? throw new InvalidOperationException("The Google client secret is empty.");
        }

        throw new InvalidOperationException($"The Google client secret file {GoogleConstants.SecretFileName} has no {GoogleConstants.SecretFileProperty}.");
    }

    // A free port is found first, since a listener cannot take port 0.
    private static string Listen(HttpListener listener)
    {
        TcpListener probe = new(IPAddress.Loopback, 0);
        probe.Start();
        int port = ((IPEndPoint)probe.LocalEndpoint).Port;
        probe.Stop();

        string redirectUri = $"http://{IPAddress.Loopback}:{port}/";
        listener.Prefixes.Add(redirectUri);
        listener.Start();

        return redirectUri;
    }

    private static Uri BuildAuthorizationUri(PkceChallenge pkce, string state, string redirectUri)
    {
        Dictionary<string, string> parameterMap = new()
        {
            ["client_id"] = GoogleConstants.WindowsClientId,
            ["redirect_uri"] = redirectUri,
            ["response_type"] = ResponseTypeCode,
            ["scope"] = GoogleConstants.Scope,
            ["code_challenge"] = pkce.Challenge,
            ["code_challenge_method"] = PkceChallenge.Method,
            [StateKey] = state
        };

        return new($"{GoogleConstants.AuthorizationEndpoint}?{string.Join("&", parameterMap.Select(x => $"{x.Key}={Uri.EscapeDataString(x.Value)}"))}");
    }

    // Requests with no code and no error (a favicon, say) are answered and skipped. A denied request returns null; a response that does not carry our state is not trusted.
    private static async Task<string?> WaitForCode(HttpListener listener, string state, CancellationToken cancellationToken)
    {
        while (true)
        {
            HttpListenerContext context = await listener.GetContextAsync().WaitAsync(cancellationToken);
            NameValueCollection query = context.Request.QueryString;
            string? error = query[ErrorKey];
            string? code = query[CodeKey];

            if (error is null && code is null)
            {
                await Respond(context, HttpStatusCode.NotFound, string.Empty);
                continue;
            }

            await Respond(context, HttpStatusCode.OK, ClosePage);
            if (error is not null) return null;

            string returnedState = query[StateKey] ?? string.Empty;
            if (!CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(returnedState), Encoding.UTF8.GetBytes(state)))
            {
                throw new InvalidOperationException("The Google sign-in response did not belong to this sign-in.");
            }

            return code;
        }
    }

    private static async Task Respond(HttpListenerContext context, HttpStatusCode status, string body)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(body);
        context.Response.StatusCode = (int)status;
        context.Response.ContentType = "text/html; charset=utf-8";
        context.Response.ContentLength64 = bytes.Length;
        await context.Response.OutputStream.WriteAsync(bytes);
        context.Response.Close();
    }

    private async Task<string?> ExchangeCode(string code, string verifier, string redirectUri, string clientSecret, CancellationToken cancellationToken)
    {
        Dictionary<string, string> fieldMap = new()
        {
            ["code"] = code,
            ["client_id"] = GoogleConstants.WindowsClientId,
            ["client_secret"] = clientSecret,
            ["code_verifier"] = verifier,
            ["grant_type"] = GrantTypeAuthorizationCode,
            ["redirect_uri"] = redirectUri
        };

        try
        {
            using HttpClient client = factoryHttpClient.CreateClient();
            using FormUrlEncodedContent body = new(fieldMap);
            using HttpResponseMessage response = await client.PostAsync(GoogleConstants.TokenEndpoint, body, cancellationToken);
            if (!response.IsSuccessStatusCode) return null;

            await using Stream stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using JsonDocument document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);

            return document.RootElement.TryGetProperty(IdTokenProperty, out JsonElement element) && element.ValueKind == JsonValueKind.String ? element.GetString() : null;
        }
        catch (Exception exception) when (exception is HttpRequestException or JsonException)
        {
            return null;
        }
    }
}
