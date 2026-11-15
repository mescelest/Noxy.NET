namespace Noxy.NET.Test.MediaPlayer.Fakes;

public sealed class FakeDisposeCallbackStream(byte[] bytes, Action onDispose) : MemoryStream(bytes)
{
    protected override void Dispose(bool disposing)
    {
        if (disposing) onDispose();

        base.Dispose(disposing);
    }
}
