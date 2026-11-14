using System.Security.Cryptography;
using System.Text;
using Noxy.NET.MediaPlayer.Application.Interfaces.Services;

namespace Noxy.NET.MediaPlayer.Application.Services;

public class PasswordHashService : IPasswordHashService
{
    private const string Prefix = "PBKDF2-SHA512";
    private const int SaltSize = 16;
    private const int HashSize = 32;
    private const int Iterations = 210_000;

    private static readonly byte[] DummySalt = RandomNumberGenerator.GetBytes(SaltSize);

    public string Hash(string password)
    {
        byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);
        byte[] hash = Derive(password, salt, Iterations, HashSize);

        return $"{Prefix}${Iterations}${Convert.ToBase64String(salt)}${Convert.ToBase64String(hash)}";
    }

    public bool Verify(string password, string? storedHash)
    {
        if (storedHash is null)
        {
            Derive(password, DummySalt, Iterations, HashSize);
            return false;
        }

        string[] parts = storedHash.Split('$');
        if (parts is not [Prefix, _, _, _] || !int.TryParse(parts[1], out int iterations) || iterations <= 0)
        {
            return false;
        }

        try
        {
            byte[] salt = Convert.FromBase64String(parts[2]);
            byte[] expected = Convert.FromBase64String(parts[3]);

            return CryptographicOperations.FixedTimeEquals(Derive(password, salt, iterations, expected.Length), expected);
        }
        catch (FormatException)
        {
            return false;
        }
    }

    private static byte[] Derive(string password, byte[] salt, int iterations, int size)
    {
        return Rfc2898DeriveBytes.Pbkdf2(Encoding.UTF8.GetBytes(password), salt, iterations, HashAlgorithmName.SHA512, size);
    }
}
