using System.Buffers.Text;
using System.Collections.Specialized;
using System.Net;
using System.Net.Mime;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Noxy.NET.MediaPlayer.Presentation.Interfaces.Services;
using Noxy.NET.MediaPlayer.Presentation.Models;

// ReSharper disable once CheckNamespace
namespace Noxy.NET.MediaPlayer.App.Services;

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
    private const string CompletePageAsset = "sign-in-complete.html";
    private const string HtmlContentType = $"{MediaTypeNames.Text.Html}; charset=utf-8";

    public async Task<string?> GetIdToken(CancellationToken cancellationToken)
    {
        string clientSecret = await ReadClientSecret(cancellationToken);
        byte[] completePage = await ReadCompletePage(cancellationToken);
        PkceChallenge pkce = PkceChallenge.Create();
        string state = Base64Url.EncodeToString(RandomNumberGenerator.GetBytes(StateByteCount));

        using HttpListener listener = new();
        string redirectUri = Listen(listener);
        using CancellationTokenSource timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(TimeSpan.FromMinutes(TimeoutMinutes));

        try
        {
            if (!await Launcher.Default.OpenAsync(BuildAuthorizationUri(pkce, state, redirectUri))) return null;

            string? code = await WaitForCode(listener, state, completePage, timeout.Token);
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
        string path = Path.Combine(AppContext.BaseDirectory, AppConstants.SecretFileName);
        if (!File.Exists(path)) throw new InvalidOperationException($"The Google client secret file {AppConstants.SecretFileName} is missing next to the app.");

        await using FileStream stream = File.OpenRead(path);
        using JsonDocument document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
        if (document.RootElement.TryGetProperty(AppConstants.SecretFileProperty, out JsonElement element) && element.ValueKind == JsonValueKind.String && !string.IsNullOrWhiteSpace(element.GetString()))
        {
            return element.GetString() ?? throw new InvalidOperationException("The Google client secret is empty.");
        }

        throw new InvalidOperationException($"The Google client secret file {AppConstants.SecretFileName} has no {AppConstants.SecretFileProperty}.");
    }

    // The page the browser lands on after Google sends it back: a real file in Resources/Raw, read before the browser opens so a missing file fails the sign-in early.
    private static async Task<byte[]> ReadCompletePage(CancellationToken cancellationToken)
    {
        await using Stream stream = await FileSystem.OpenAppPackageFileAsync(CompletePageAsset);
        using MemoryStream copy = new();
        await stream.CopyToAsync(copy, cancellationToken);

        return copy.ToArray();
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
            ["client_id"] = AppConstants.WindowsClientId,
            ["redirect_uri"] = redirectUri,
            ["response_type"] = ResponseTypeCode,
            ["scope"] = AppConstants.Scope,
            ["code_challenge"] = pkce.Challenge,
            ["code_challenge_method"] = PkceChallenge.Method,
            [StateKey] = state
        };

        return new($"{AppConstants.AuthorizationEndpoint}?{string.Join("&", parameterMap.Select(x => $"{x.Key}={Uri.EscapeDataString(x.Value)}"))}");
    }

    // Requests with no code and no error (a favicon, say) are answered and skipped. A denied request returns null; a response that does not carry our state is not trusted.
    private static async Task<string?> WaitForCode(HttpListener listener, string state, byte[] completePage, CancellationToken cancellationToken)
    {
        while (true)
        {
            HttpListenerContext context = await listener.GetContextAsync().WaitAsync(cancellationToken);
            NameValueCollection query = context.Request.QueryString;
            string? error = query[ErrorKey];
            string? code = query[CodeKey];

            if (error is null && code is null)
            {
                await Respond(context, HttpStatusCode.NotFound, []);
                continue;
            }

            await Respond(context, HttpStatusCode.OK, completePage);
            if (error is not null) return null;

            string returnedState = query[StateKey] ?? string.Empty;
            if (!CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(returnedState), Encoding.UTF8.GetBytes(state)))
            {
                throw new InvalidOperationException("The Google sign-in response did not belong to this sign-in.");
            }

            return code;
        }
    }

    private static async Task Respond(HttpListenerContext context, HttpStatusCode status, byte[] body)
    {
        context.Response.StatusCode = (int)status;
        context.Response.ContentType = HtmlContentType;
        context.Response.ContentLength64 = body.Length;
        await context.Response.OutputStream.WriteAsync(body);
        context.Response.Close();
    }

    private async Task<string?> ExchangeCode(string code, string verifier, string redirectUri, string clientSecret, CancellationToken cancellationToken)
    {
        Dictionary<string, string> fieldMap = new()
        {
            ["code"] = code,
            ["client_id"] = AppConstants.WindowsClientId,
            ["client_secret"] = clientSecret,
            ["code_verifier"] = verifier,
            ["grant_type"] = GrantTypeAuthorizationCode,
            ["redirect_uri"] = redirectUri
        };

        try
        {
            using HttpClient client = factoryHttpClient.CreateClient();
            using FormUrlEncodedContent body = new(fieldMap);
            using HttpResponseMessage response = await client.PostAsync(AppConstants.TokenEndpoint, body, cancellationToken);
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
