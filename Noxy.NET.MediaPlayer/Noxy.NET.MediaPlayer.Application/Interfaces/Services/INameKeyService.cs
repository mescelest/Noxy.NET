namespace Noxy.NET.MediaPlayer.Application.Interfaces.Services;

public interface INameKeyService
{
    // Trimmed, inner whitespace collapsed, Unicode form KC and lower-case, so spellings that differ only by case or spacing share a key. An all-blank name has no key: the result is empty and the name is never stored.
    string GetKey(string name);
}
