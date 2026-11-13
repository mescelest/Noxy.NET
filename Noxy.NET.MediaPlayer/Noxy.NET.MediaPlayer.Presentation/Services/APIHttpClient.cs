namespace Noxy.NET.MediaPlayer.Presentation.Services;

public class APIHttpClient(HttpClient client)
{
    public void SetBearerToken(string token)
    {
        client.DefaultRequestHeaders.Authorization = new("Bearer", token);
    }

    public void ClearBearerToken()
    {
        client.DefaultRequestHeaders.Authorization = null;
    }
}
