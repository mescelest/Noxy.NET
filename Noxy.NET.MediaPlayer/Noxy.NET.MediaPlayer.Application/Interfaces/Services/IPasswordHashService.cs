namespace Noxy.NET.MediaPlayer.Application.Interfaces.Services;

public interface IPasswordHashService
{
    string Hash(string password);
    bool Verify(string password, string storedHash);
}
