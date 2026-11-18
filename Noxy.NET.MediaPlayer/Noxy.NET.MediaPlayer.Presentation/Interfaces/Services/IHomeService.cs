using Noxy.NET.MediaPlayer.Presentation.Models;

namespace Noxy.NET.MediaPlayer.Presentation.Interfaces.Services;

// What the Home page shows. It reads only the local copy: no call to the server.
public interface IHomeService
{
    Task<HomeModel> Get();
}
