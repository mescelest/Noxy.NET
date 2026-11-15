namespace Noxy.NET.Test.MediaPlayer.Fixtures;

public static class MediaFileFixture
{
    // A minimal valid mono PCM wave file whose audio data is a repeating byte pattern, so a test can find it again.
    public static byte[] CreateWav(int dataBytes = 1000)
    {
        byte[] pattern = Pattern(dataBytes);

        using MemoryStream stream = new();
        using BinaryWriter writer = new(stream);
        writer.Write("RIFF"u8);
        writer.Write(36 + dataBytes);
        writer.Write("WAVE"u8);
        writer.Write("fmt "u8);
        writer.Write(16);
        writer.Write((short)1);
        writer.Write((short)1);
        writer.Write(8000);
        writer.Write(16000);
        writer.Write((short)2);
        writer.Write((short)16);
        writer.Write("data"u8);
        writer.Write(dataBytes);
        writer.Write(pattern);
        writer.Flush();

        return stream.ToArray();
    }

    public static byte[] Pattern(int dataBytes = 1000)
    {
        byte[] pattern = new byte[dataBytes];
        for (int index = 0; index < dataBytes; index++)
        {
            pattern[index] = (byte)(index % 251);
        }

        return pattern;
    }
}
