namespace Noxy.NET.MediaPlayer.Presentation.Services;

public class APIHttpClient(HttpClient client)
{
    private const string BearerScheme = "Bearer";

    public void SetBearerToken(string token)
    {
        client.DefaultRequestHeaders.Authorization = new(BearerScheme, token);
    }

    public void ClearBearerToken()
    {
        client.DefaultRequestHeaders.Authorization = null;
    }
}
