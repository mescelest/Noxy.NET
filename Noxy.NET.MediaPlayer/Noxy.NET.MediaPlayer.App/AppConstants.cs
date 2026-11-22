namespace Noxy.NET.MediaPlayer.App;

public static class AppConstants
{
    // The Windows (Desktop app) OAuth client in Google Cloud Console. A client ID is public.
    public const string WindowsClientId = "183545471248-p7bqsimcu1b8sgmgjt2hd1rncqrgec6g.apps.googleusercontent.com";

    // Next to the app, git-ignored: { "clientSecret": "..." }. Google's token endpoint asks a desktop client for it even with PKCE; Google does not treat a desktop secret as confidential, but it stays out of the repository.
    public const string SecretFileName = "google.local.json";

    public const string SecretFileProperty = "clientSecret";

    public const string AuthorizationEndpoint = "https://accounts.google.com/o/oauth2/v2/auth";

    public const string TokenEndpoint = "https://oauth2.googleapis.com/token";

    public const string Scope = "openid email";
}
